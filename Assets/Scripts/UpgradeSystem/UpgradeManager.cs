using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    [Header("天赋配置")]
    public UpgradeData[] allUpgrades;
    
    [Header("调试")]
    public bool autoLoadFromResources = true;
    
    private Dictionary<string, int> upgradeLevels = new Dictionary<string, int>();
    private Dictionary<string, UpgradeData> upgradeDataLookup;
    
    public static UpgradeManager Instance { get; private set; }
    public event Action<string, int> OnUpgradePurchased;
    public event Action OnUpgradesLoaded;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("UpgradeManager: 存在多个实例，销毁重复实例");
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadUpgradeData();
        LoadUpgradeProgress();
        ApplyAllPurchasedUpgrades();
        OnUpgradesLoaded?.Invoke();
        Debug.Log($"UpgradeManager: 已加载 {allUpgrades.Length} 个天赋");
    }
    
    void LoadUpgradeData()
    {
        if (allUpgrades != null && allUpgrades.Length > 0)
        {
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null) upgrade.Validate();
            }
        }
        else if (autoLoadFromResources)
        {
            allUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
            if (allUpgrades == null || allUpgrades.Length == 0)
            {
                Debug.LogWarning("UpgradeManager: 未在Resources/Upgrades找到天赋数据");
                allUpgrades = new UpgradeData[0];
            }
        }
        else
        {
            allUpgrades = new UpgradeData[0];
        }
        
        upgradeDataLookup = new Dictionary<string, UpgradeData>();
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null && !string.IsNullOrEmpty(upgrade.upgradeId))
            {
                if (!upgradeDataLookup.ContainsKey(upgrade.upgradeId))
                {
                    upgradeDataLookup[upgrade.upgradeId] = upgrade;
                }
                else
                {
                    Debug.LogWarning($"UpgradeManager: 重复的天赋ID {upgrade.upgradeId}");
                }
            }
        }
    }
    
    void LoadUpgradeProgress()
    {
        upgradeLevels.Clear();
        if (SaveManager.Instance != null)
        {
            var savedLevels = SaveManager.Instance.GetAllUpgradeLevels();
            foreach (var kvp in savedLevels)
            {
                upgradeLevels[kvp.Key] = kvp.Value;
            }
            Debug.Log($"UpgradeManager: 已加载 {upgradeLevels.Count} 个天赋等级");
        }
        else
        {
            Debug.LogWarning("UpgradeManager: SaveManager未找到");
        }
    }
    
    void ApplyAllPurchasedUpgrades()
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("UpgradeManager: PlayerStats未找到！");
            return;
        }
        
        foreach (var kvp in upgradeLevels)
        {
            string upgradeId = kvp.Key;
            int level = kvp.Value;
            if (upgradeDataLookup.TryGetValue(upgradeId, out var upgradeData))
            {
                ApplyUpgradeEffect(upgradeData, level);
            }
        }
        Debug.Log($"UpgradeManager: 已应用 {upgradeLevels.Count} 个天赋效果");
    }
    
    public int GetUpgradeLevel(string upgradeId)
    {
        if (upgradeLevels.TryGetValue(upgradeId, out int level))
        {
            return level;
        }
        return 0;
    }
    
    public UpgradeData GetUpgradeData(string upgradeId)
    {
        if (upgradeDataLookup.TryGetValue(upgradeId, out var data))
        {
            return data;
        }
        return null;
    }
    
    public bool CanBuy(string upgradeId)
    {
        if (!upgradeDataLookup.TryGetValue(upgradeId, out var upgradeData))
        {
            Debug.LogWarning($"UpgradeManager: 未找到天赋 {upgradeId}");
            return false;
        }
        
        int currentLevel = GetUpgradeLevel(upgradeId);
        if (currentLevel >= upgradeData.maxLevel)
        {
            return false; // 已满级
        }
        
        // 检查前置条件
        foreach (string prereqId in upgradeData.prerequisites)
        {
            int prereqLevel = GetUpgradeLevel(prereqId);
            if (prereqLevel <= 0)
            {
                return false; // 前置未解锁
            }
        }
        
        // 检查经验值是否足够
        int cost = GetBuyCost(upgradeId);
        if (ExperienceSystem.Instance == null)
        {
            return false;
        }
        
        return ExperienceSystem.Instance.HasEnoughExperience(cost);
    }
    
    public int GetBuyCost(string upgradeId)
    {
        if (!upgradeDataLookup.TryGetValue(upgradeId, out var upgradeData))
        {
            return 0;
        }
        
        int currentLevel = GetUpgradeLevel(upgradeId);
        return upgradeData.GetCostForLevel(currentLevel + 1);
    }
    
    public bool BuyUpgrade(string upgradeId)
    {
        if (!CanBuy(upgradeId))
        {
            return false;
        }
        
        if (!upgradeDataLookup.TryGetValue(upgradeId, out var upgradeData))
        {
            return false;
        }
        
        int cost = GetBuyCost(upgradeId);
        
        // 消耗经验值
        if (ExperienceSystem.Instance == null || !ExperienceSystem.Instance.SpendExperience(cost))
        {
            return false;
        }
        
        // 提升等级
        int newLevel = GetUpgradeLevel(upgradeId) + 1;
        upgradeLevels[upgradeId] = newLevel;
        
        // 保存到SaveManager
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetUpgradeLevel(upgradeId, newLevel);
        }
        
        // 应用属性加成
        ApplyUpgradeEffect(upgradeData, newLevel);
        
        // 触发事件
        OnUpgradePurchased?.Invoke(upgradeId, newLevel);
        
        Debug.Log($"UpgradeManager: 购买天赋 {upgradeId} 等级 {newLevel}，消耗 {cost} XP");
        return true;
    }
    
    void ApplyUpgradeEffect(UpgradeData upgradeData, int level)
    {
        if (PlayerStats.Instance == null || level <= 0)
        {
            return;
        }
        
        float multiplier = upgradeData.GetValueForLevel(level);
        StatType statType = upgradeData.statType;
        
        // 应用乘法倍率到PlayerStats
        PlayerStats.Instance.ApplyMultiplier(statType, multiplier, upgradeData.upgradeId);
        
        Debug.Log($"UpgradeManager: 应用天赋效果 {upgradeData.upgradeId} - {statType} ×{multiplier}");
    }
    
    public List<UpgradeData> GetAvailableUpgrades()
    {
        List<UpgradeData> available = new List<UpgradeData>();
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null && CanBuy(upgrade.upgradeId))
            {
                available.Add(upgrade);
            }
        }
        return available;
    }
    
    public List<UpgradeData> GetAllUpgrades()
    {
        return allUpgrades.ToList();
    }
    
    [ContextMenu("打印所有天赋状态")]
    void DebugPrintAllUpgrades()
    {
        Debug.Log("=== 天赋状态 ===");
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade != null)
            {
                int level = GetUpgradeLevel(upgrade.upgradeId);
                bool canBuy = CanBuy(upgrade.upgradeId);
                int cost = GetBuyCost(upgrade.upgradeId);
                Debug.Log($"{upgrade.displayName}: 等级 {level}/{upgrade.maxLevel}, 可购买: {canBuy}, 价格: {cost}");
            }
        }
    }
}
