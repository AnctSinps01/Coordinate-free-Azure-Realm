using UnityEngine;

/// <summary>
/// 可升级属性类型枚举
/// </summary>
public enum StatType
{
    MoveSpeed,      // 移动速度
    AttackSpeed,    // 攻击速度
    MaxHealth,      // 最大生命值
    Damage,         // 伤害
    DashCooldown,   // 冲刺冷却
    BulletSpeed,    // 子弹速度
    PickupRange     // 拾取范围
}

/// <summary>
/// 升级数据ScriptableObject
/// 仅支持乘法增益
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "游戏/天赋升级")]
public class UpgradeData : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("天赋唯一ID，用于代码识别")]
    public string upgradeId;
    
    [Tooltip("天赋显示名称")]
    public string displayName;
    
    [Tooltip("天赋描述")]
    [TextArea(2, 4)]
    public string description;
    
    [Tooltip("天赋图标")]
    public Sprite icon;
    
    [Header("等级设置")]
    [Tooltip("最大等级")]
    public int maxLevel = 3;
    
    [Header("数值修改（仅乘法）")]
    [Tooltip("影响的属性类型")]
    public StatType statType;
    
    [Tooltip("每级的乘法倍率（数组长度应与maxLevel一致）\n示例：[1.5, 2.25, 3.375] 表示每级×1.5倍")]
    public float[] levelValues = new float[] { 1.5f, 2.25f, 3.375f };
    
    [Header("价格配置")]
    [Tooltip("每级的升级价格（数组长度应与maxLevel一致）\n示例：[4, 10, 25] 表示1级4点、2级10点、3级25点\n留空则使用默认公式：4 × 2.5^(level-1)")]
    public int[] levelCosts = new int[0];
    
    [Header("分支关系")]
    [Tooltip("前置天赋ID列表（为空表示无前置）")]
    public string[] prerequisites = new string[0];
    
    [Header("网格位置")]
    [Tooltip("在技能树网格中的位置（列, 行），从0开始")]
    public Vector2Int gridPosition = Vector2Int.zero;
    
    /// <summary>
    /// 获取指定等级的倍率
    /// </summary>
    public float GetValueForLevel(int level)
    {
        if (level <= 0 || level > maxLevel || levelValues == null || level > levelValues.Length)
        {
            Debug.LogWarning($"[{upgradeId}] 请求的等级 {level} 无效，返回默认值1");
            return 1f;
        }
        return levelValues[level - 1];
    }
    
    /// <summary>
    /// 计算升级到指定等级所需的价格
    /// </summary>
    public int GetCostForLevel(int targetLevel)
    {
        if (targetLevel <= 0 || targetLevel > maxLevel)
        {
            Debug.LogWarning($"[{upgradeId}] 请求的等级 {targetLevel} 无效，返回0");
            return 0;
        }
        
        if (levelCosts != null && levelCosts.Length >= targetLevel)
        {
            return levelCosts[targetLevel - 1];
        }
        
        return Mathf.RoundToInt(4f * Mathf.Pow(2.5f, targetLevel - 1));
    }
    
    /// <summary>
    /// 获取从当前等级升级到下一级所需的价格
    /// </summary>
    public int GetNextLevelCost(int currentLevel)
    {
        return GetCostForLevel(currentLevel + 1);
    }
    
    /// <summary>
    /// 验证数据完整性
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrEmpty(upgradeId))
        {
            Debug.LogError($"天赋数据 {name} 缺少upgradeId！");
            return false;
        }
        
        if (levelValues == null || levelValues.Length < maxLevel)
        {
            Debug.LogError($"[{upgradeId}] levelValues数组长度 ({levelValues?.Length ?? 0}) 小于maxLevel ({maxLevel})！");
            return false;
        }
        
        if (levelCosts != null && levelCosts.Length > 0 && levelCosts.Length < maxLevel)
        {
            Debug.LogWarning($"[{upgradeId}] levelCosts数组长度 ({levelCosts.Length}) 小于maxLevel ({maxLevel})");
        }
        
        return true;
    }
    
    void OnValidate()
    {
        if (!string.IsNullOrEmpty(upgradeId))
        {
            Validate();
        }
    }
    
    #region 编辑器辅助方法
    
    /// <summary>
    /// 【编辑器用】生成指数增长的倍率
    /// 例如：base=1.5, 生成 [1.5, 2.25, 3.375]
    /// </summary>
    public void GenerateLevelValues(float baseValue)
    {
        levelValues = new float[maxLevel];
        for (int i = 0; i < maxLevel; i++)
        {
            levelValues[i] = Mathf.Pow(baseValue, i + 1);
        }
        Debug.Log($"[{upgradeId}] 已生成倍率：{string.Join(", ", levelValues)}");
    }
    
    /// <summary>
    /// 【编辑器用】生成等级价格
    /// </summary>
    public void GenerateLevelCosts(float baseCost = 4f, float growthRate = 2.5f)
    {
        levelCosts = new int[maxLevel];
        for (int i = 0; i < maxLevel; i++)
        {
            levelCosts[i] = Mathf.RoundToInt(baseCost * Mathf.Pow(growthRate, i));
        }
        Debug.Log($"[{upgradeId}] 已生成价格：{string.Join(", ", levelCosts)}");
    }
    
    #endregion
}
