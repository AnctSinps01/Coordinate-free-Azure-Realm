using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 天赋等级条目（可序列化）
/// </summary>
[Serializable]
public class UpgradeLevelEntry
{
    public string upgradeId;
    public int level;
}

/// <summary>
/// 保存数据结构（可序列化版本）
/// </summary>
[Serializable]
public class SaveData
{
    // 升级系统数据 - 使用List代替Dictionary以便序列化
    public List<UpgradeLevelEntry> upgradeLevels;
    public int totalExperience;
    public int currentSkillPoints;
    public int playerLevel;
    
    // 游戏统计
    public int totalKills;
    public float totalPlayTime;
    
    // 版本号
    public string version;
    
    public SaveData()
    {
        upgradeLevels = new List<UpgradeLevelEntry>();
        totalExperience = 0;
        currentSkillPoints = 0;
        playerLevel = 1;
        totalKills = 0;
        totalPlayTime = 0f;
        version = Application.version;
    }
}

public class SaveManager : MonoBehaviour
{
    [Header("存档设置")]
    public string saveFileName = "game_save.json";
    public float autoSaveInterval = 30f;
    public bool autoSaveOnPause = true;
    
    // 运行时使用的字典（便于查询）
    private Dictionary<string, int> upgradeLevelDict = new Dictionary<string, int>();
    
    // 当前存档数据
    private SaveData currentSave;
    
    private float lastAutoSaveTime;
    private bool hasUnsavedChanges = false;
    
    public static SaveManager Instance { get; private set; }
    
    public event Action OnSaveLoaded;
    public event Action OnSaveCompleted;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else
        {
            Debug.LogWarning("SaveManager: 存在多个实例，销毁重复实例");
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (autoSaveInterval > 0 && hasUnsavedChanges)
        {
            if (Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveGame();
            }
        }
    }
    
    void OnApplicationPause(bool pause)
    {
        if (pause && autoSaveOnPause && hasUnsavedChanges)
        {
            SaveGame();
        }
    }
    
    void OnApplicationQuit()
    {
        if (hasUnsavedChanges)
        {
            SaveGame();
        }
    }
    
    string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
    
    public bool SaveExists()
    {
        return File.Exists(GetSaveFilePath());
    }
    
    /// <summary>
    /// 保存前：将字典转换为列表
    /// </summary>
    void SyncDictToList()
    {
        currentSave.upgradeLevels.Clear();
        foreach (var kvp in upgradeLevelDict)
        {
            currentSave.upgradeLevels.Add(new UpgradeLevelEntry
            {
                upgradeId = kvp.Key,
                level = kvp.Value
            });
        }
    }
    
    /// <summary>
    /// 加载后：将列表转换为字典
    /// </summary>
    void SyncListToDict()
    {
        upgradeLevelDict.Clear();
        if (currentSave.upgradeLevels != null)
        {
            foreach (var entry in currentSave.upgradeLevels)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.upgradeId))
                {
                    upgradeLevelDict[entry.upgradeId] = entry.level;
                }
            }
        }
    }
    
    public void SaveGame()
    {
        try
        {
            // 保存前同步字典到列表
            SyncDictToList();
            
            string json = JsonUtility.ToJson(currentSave, true);
            string path = GetSaveFilePath();
            
            File.WriteAllText(path, json);
            
            hasUnsavedChanges = false;
            lastAutoSaveTime = Time.time;
            
            OnSaveCompleted?.Invoke();
            
            Debug.Log($"SaveManager: 游戏已保存，天赋数 {currentSave.upgradeLevels.Count}，经验值 {currentSave.totalExperience}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: 保存失败 - {e.Message}");
        }
    }
    
    public void MarkAsDirty()
    {
        hasUnsavedChanges = true;
    }
    
    public void LoadGame()
    {
        string path = GetSaveFilePath();
        
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                currentSave = JsonUtility.FromJson<SaveData>(json);
                
                if (currentSave == null)
                {
                    Debug.LogWarning("SaveManager: 存档解析失败，创建新存档");
                    currentSave = new SaveData();
                }
                else
                {
                    // 加载后同步列表到字典
                    SyncListToDict();
                    
                    Debug.Log($"SaveManager: 游戏已加载，天赋数 {upgradeLevelDict.Count}，经验值 {currentSave.totalExperience}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: 加载失败 - {e.Message}，创建新存档");
                currentSave = new SaveData();
                upgradeLevelDict.Clear();
            }
        }
        else
        {
            Debug.Log("SaveManager: 未找到存档，创建新存档");
            currentSave = new SaveData();
            upgradeLevelDict.Clear();
        }
        
        OnSaveLoaded?.Invoke();
    }
    
    public SaveData GetCurrentSave()
    {
        return currentSave;
    }
    
    /// <summary>
    /// 获取指定天赋的当前等级（从字典查询）
    /// </summary>
    public int GetUpgradeLevel(string upgradeId)
    {
        if (upgradeLevelDict.TryGetValue(upgradeId, out int level))
        {
            return level;
        }
        return 0;
    }
    
    /// <summary>
    /// 设置天赋等级（更新字典，标记脏数据）
    /// </summary>
    public void SetUpgradeLevel(string upgradeId, int level)
    {
        upgradeLevelDict[upgradeId] = level;
        MarkAsDirty();
    }
    
    /// <summary>
    /// 获取所有天赋等级（返回字典副本）
    /// </summary>
    public Dictionary<string, int> GetAllUpgradeLevels()
    {
        return new Dictionary<string, int>(upgradeLevelDict);
    }
    
    public int GetSkillPoints()
    {
        return currentSave.currentSkillPoints;
    }
    
    public void SetSkillPoints(int points)
    {
        currentSave.currentSkillPoints = Mathf.Max(0, points);
        MarkAsDirty();
    }
    
    public void AddSkillPoints(int amount)
    {
        SetSkillPoints(currentSave.currentSkillPoints + amount);
    }
    
    public bool SpendSkillPoints(int amount)
    {
        if (currentSave.currentSkillPoints >= amount)
        {
            SetSkillPoints(currentSave.currentSkillPoints - amount);
            return true;
        }
        return false;
    }
    
    public int GetPlayerLevel()
    {
        return currentSave.playerLevel;
    }
    
    public void SetPlayerLevel(int level)
    {
        currentSave.playerLevel = Mathf.Max(1, level);
        MarkAsDirty();
    }
    
    public int GetTotalExperience()
    {
        return currentSave.totalExperience;
    }
    
    public void AddExperience(int amount)
    {
        currentSave.totalExperience += Mathf.Max(0, amount);
        MarkAsDirty();
    }
    
    public void DeleteSave()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("SaveManager: 存档已删除");
        }
        
        currentSave = new SaveData();
        upgradeLevelDict.Clear();
        OnSaveLoaded?.Invoke();
    }
    
    public void ResetProgress()
    {
        currentSave = new SaveData();
        upgradeLevelDict.Clear();
        SaveGame();
        Debug.Log("SaveManager: 进度已重置");
    }
    
    [ContextMenu("打印存档信息")]
    void DebugPrintSaveInfo()
    {
        Debug.Log("=== 存档信息 ===");
        Debug.Log($"存档路径: {GetSaveFilePath()}");
        Debug.Log($"玩家等级: {currentSave.playerLevel}");
        Debug.Log($"总经验值: {currentSave.totalExperience}");
        Debug.Log($"技能点: {currentSave.currentSkillPoints}");
        Debug.Log($"已解锁天赋数: {upgradeLevelDict.Count}");
        foreach (var kvp in upgradeLevelDict)
        {
            Debug.Log($"  - {kvp.Key}: 等级 {kvp.Value}");
        }
    }
}
