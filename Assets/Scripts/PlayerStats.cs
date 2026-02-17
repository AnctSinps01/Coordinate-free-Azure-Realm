using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 玩家属性管理器
/// 仅支持乘法增益
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("基础属性")]
    [Tooltip("基础移动速度")]
    public float baseMoveSpeed = 5f;
    
    [Tooltip("基础攻击速度（每秒攻击次数）")]
    public float baseAttackSpeed = 1f;
    
    [Tooltip("基础最大生命值")]
    public float baseMaxHealth = 100f;
    
    [Tooltip("基础伤害")]
    public float baseDamage = 10f;
    
    [Tooltip("基础冲刺冷却时间（秒）")]
    public float baseDashCooldown = 2f;
    
    [Tooltip("基础子弹速度")]
    public float baseBulletSpeed = 10f;
    
    [Tooltip("基础拾取范围")]
    public float basePickupRange = 2f;
    
    // 乘法修饰器存储（StatType -> 倍率列表）
    private Dictionary<StatType, List<float>> multipliers = new Dictionary<StatType, List<float>>();
    
    // 缓存的计算后属性值
    private Dictionary<StatType, float> cachedStats = new Dictionary<StatType, float>();
    private bool cacheDirty = true;
    
    // 事件：属性变化时触发
    public event Action<StatType, float> OnStatChanged;
    public event Action OnAnyStatChanged;
    
    // 单例实例
    public static PlayerStats Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeMultipliers();
        }
        else
        {
            Debug.LogWarning("PlayerStats: 存在多个实例，销毁重复实例");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameRestart += OnGameRestart;
        }
        RecalculateAllStats();
    }
    
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameRestart -= OnGameRestart;
        }
    }
    
    void InitializeMultipliers()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            multipliers[statType] = new List<float>();
        }
    }
    
    void OnGameRestart()
    {
        ClearAllMultipliers();
        RecalculateAllStats();
        Debug.Log("PlayerStats: 属性已重置");
    }
    
    #region 属性获取
    
    public float MoveSpeed => GetStat(StatType.MoveSpeed);
    public float AttackSpeed => GetStat(StatType.AttackSpeed);
    public float MaxHealth => GetStat(StatType.MaxHealth);
    public float Damage => GetStat(StatType.Damage);
    public float DashCooldown => GetStat(StatType.DashCooldown);
    public float BulletSpeed => GetStat(StatType.BulletSpeed);
    public float PickupRange => GetStat(StatType.PickupRange);
    
    public float GetStat(StatType statType)
    {
        if (cacheDirty)
        {
            RecalculateAllStats();
        }
        
        if (cachedStats.TryGetValue(statType, out float value))
        {
            return value;
        }
        
        return GetBaseStat(statType);
    }
    
    float GetBaseStat(StatType statType)
    {
        switch (statType)
        {
            case StatType.MoveSpeed: return baseMoveSpeed;
            case StatType.AttackSpeed: return baseAttackSpeed;
            case StatType.MaxHealth: return baseMaxHealth;
            case StatType.Damage: return baseDamage;
            case StatType.DashCooldown: return baseDashCooldown;
            case StatType.BulletSpeed: return baseBulletSpeed;
            case StatType.PickupRange: return basePickupRange;
            default: return 0f;
        }
    }
    
    #endregion
    
    #region 属性修改（仅乘法）
    
    /// <summary>
    /// 应用乘法修饰器
    /// </summary>
    public void ApplyMultiplier(StatType statType, float multiplier, string sourceId = "")
    {
        if (!multipliers.ContainsKey(statType))
        {
            multipliers[statType] = new List<float>();
        }
        
        multipliers[statType].Add(multiplier);
        cacheDirty = true;
        
        float newValue = GetStat(statType);
        OnStatChanged?.Invoke(statType, newValue);
        OnAnyStatChanged?.Invoke();
        
        Debug.Log($"PlayerStats: 应用倍率 {statType} ×{multiplier}，当前值: {newValue:F2}");
    }
    
    /// <summary>
    /// 清除所有倍率
    /// </summary>
    public void ClearAllMultipliers()
    {
        foreach (var kvp in multipliers)
        {
            kvp.Value.Clear();
        }
        cacheDirty = true;
    }
    
    /// <summary>
    /// 重新计算所有属性值（仅乘法）
    /// </summary>
    void RecalculateAllStats()
    {
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float baseValue = GetBaseStat(statType);
            float finalValue = baseValue;
            
            if (multipliers.TryGetValue(statType, out var multList))
            {
                foreach (var mult in multList)
                {
                    finalValue *= mult;
                }
            }
            
            // 确保数值不为负
            if (statType != StatType.DashCooldown)
            {
                finalValue = Mathf.Max(0.01f, finalValue);
            }
            
            cachedStats[statType] = finalValue;
        }
        
        cacheDirty = false;
    }
    
    #endregion
    
    #region 调试
    
    [ContextMenu("打印所有属性")]
    void DebugPrintAllStats()
    {
        Debug.Log("=== PlayerStats 当前状态 ===");
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float baseValue = GetBaseStat(statType);
            float finalValue = GetStat(statType);
            Debug.Log($"{statType}: 基础={baseValue:F2} | 最终={finalValue:F2}");
        }
    }
    
    #endregion
}
