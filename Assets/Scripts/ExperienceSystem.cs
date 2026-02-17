using UnityEngine;
using System;

/// <summary>
/// 经验值收集系统
/// 增量游戏机制：经验值是累积货币，不清零
/// 杀怪直接获得经验值，死亡后在升级界面使用
/// </summary>
public class ExperienceSystem : MonoBehaviour
{
    [Header("经验值设置")]
    [Tooltip("每杀一个敌人获得的经验值")]
    public int xpPerKill = 1;
    
    // 当前总经验值（累积货币）
    private int totalExperience;
    
    // 事件：经验值变化时触发（新值，变化量）
    public event Action<int, int> OnExperienceChanged;
    
    // 事件：经验值增加时触发
    public event Action<int> OnExperienceAdded;
    
    // 事件：经验值消耗时触发
    public event Action<int> OnExperienceSpent;
    
    // 单例实例
    public static ExperienceSystem Instance { get; private set; }
    
    // 公开只读属性
    public int TotalExperience => totalExperience;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("ExperienceSystem: 存在多个实例，销毁重复实例");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 从SaveManager加载经验值
        LoadExperience();
        
        // 订阅敌人死亡事件
        EnemyController.OnEnemyDied += OnEnemyKilled;
    }
    
    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        EnemyController.OnEnemyDied -= OnEnemyKilled;
    }
    
    /// <summary>
    /// 敌人被杀死时的回调
    /// </summary>
    void OnEnemyKilled(EnemyController enemy, int xpValue)
    {
        // 增加经验值（使用敌人配置的经验值，或使用默认xpPerKill）
        int gainedXP = xpValue > 0 ? xpValue : xpPerKill;
        AddExperience(gainedXP);
    }
    
    /// <summary>
    /// 增加经验值（杀怪时调用）
    /// </summary>
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        
        totalExperience += amount;
        
        // 触发事件
        OnExperienceAdded?.Invoke(amount);
        OnExperienceChanged?.Invoke(totalExperience, amount);
        
        // 标记SaveManager需要保存
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.AddExperience(amount);
        }
        
        Debug.Log($"ExperienceSystem: 获得 {amount} XP，当前总经验值: {totalExperience}");
    }
    
    /// <summary>
    /// 消耗经验值（购买天赋时调用）
    /// </summary>
    /// <param name="amount">消耗数量</param>
    /// <returns>是否成功消耗</returns>
    public bool SpendExperience(int amount)
    {
        if (amount <= 0) return false;
        
        if (totalExperience >= amount)
        {
            totalExperience -= amount;
            
            // 触发事件（变化量为负数）
            OnExperienceSpent?.Invoke(amount);
            OnExperienceChanged?.Invoke(totalExperience, -amount);
            
            // 标记SaveManager需要保存
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.MarkAsDirty();
            }
            
            Debug.Log($"ExperienceSystem: 消耗 {amount} XP，剩余: {totalExperience}");
            return true;
        }
        
        Debug.LogWarning($"ExperienceSystem: 经验值不足！需要 {amount}，当前 {totalExperience}");
        return false;
    }
    
    /// <summary>
    /// 检查是否有足够的经验值
    /// </summary>
    public bool HasEnoughExperience(int amount)
    {
        return totalExperience >= amount;
    }
    
    /// <summary>
    /// 从SaveManager加载经验值
    /// </summary>
    void LoadExperience()
    {
        if (SaveManager.Instance != null)
        {
            totalExperience = SaveManager.Instance.GetTotalExperience();
            Debug.Log($"ExperienceSystem: 加载经验值 {totalExperience}");
        }
        else
        {
            totalExperience = 0;
            Debug.LogWarning("ExperienceSystem: SaveManager未找到，经验值从0开始");
        }
        
        // 触发初始化事件
        OnExperienceChanged?.Invoke(totalExperience, 0);
    }
    
    /// <summary>
    /// 设置经验值（调试用，或从存档恢复）
    /// </summary>
    public void SetExperience(int amount)
    {
        int oldValue = totalExperience;
        totalExperience = Mathf.Max(0, amount);
        int delta = totalExperience - oldValue;
        
        OnExperienceChanged?.Invoke(totalExperience, delta);
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.MarkAsDirty();
        }
    }
    
    #region 调试
    
    /// <summary>
    /// 打印当前经验值（调试用）
    /// </summary>
    [ContextMenu("打印经验值")]
    void DebugPrintExperience()
    {
        Debug.Log($"当前总经验值: {totalExperience}");
    }
    
    /// <summary>
    /// 添加测试经验值（调试用）
    /// </summary>
    [ContextMenu("添加10点经验值")]
    void DebugAddExperience()
    {
        AddExperience(10);
    }
    
    #endregion
}
