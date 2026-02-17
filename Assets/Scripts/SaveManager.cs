using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 保存数据结构
/// 存储玩家升级进度和游戏数据
/// </summary>
[Serializable]
public class SaveData
{
    // 升级系统数据
    public Dictionary<string, int> upgradeLevels;  // 天赋ID -> 当前等级
    public int totalExperience;                    // 总经验值（累计）
    public int currentSkillPoints;                 // 当前技能点
    public int playerLevel;                        // 玩家等级
    
    // 游戏统计（可选）
    public int totalKills;                         // 累计击杀数
    public float totalPlayTime;                    // 累计游戏时间
    
    // 版本号（用于兼容性检查）
    public string version;
    
    public SaveData()
    {
        upgradeLevels = new Dictionary<string, int>();
        totalExperience = 0;
        currentSkillPoints = 0;
        playerLevel = 1;
        totalKills = 0;
        totalPlayTime = 0f;
        version = Application.version;
    }
}

/// <summary>
/// 存档管理器
/// 处理游戏数据的保存和加载
/// 使用JSON格式存储在Application.persistentDataPath
/// </summary>
public class SaveManager : MonoBehaviour
{
    [Header("存档设置")]
    [Tooltip("存档文件名")]
    public string saveFileName = "game_save.json";
    
    [Tooltip("自动保存间隔（秒），0表示不自动保存")]
    public float autoSaveInterval = 30f;
    
    [Tooltip("是否在应用暂停时自动保存")]
    public bool autoSaveOnPause = true;
    
    // 当前存档数据
    private SaveData currentSave;
    
    // 上次自动保存时间
    private float lastAutoSaveTime;
    
    // 是否有未保存的更改
    private bool hasUnsavedChanges = false;
    
    // 单例实例
    public static SaveManager Instance { get; private set; }
    
    // 事件：存档加载完成
    public event Action OnSaveLoaded;
    
    // 事件：存档保存完成
    public event Action OnSaveCompleted;
    
    #region 生命周期
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保留
            LoadGame(); // 启动时自动加载
        }
        else
        {
            Debug.LogWarning("SaveManager: 存在多个实例，销毁重复实例");
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // 自动保存检查
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
    
    #endregion
    
    #region 存档路径
    
    /// <summary>
    /// 获取完整存档路径
    /// </summary>
    string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, saveFileName);
    }
    
    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists()
    {
        return File.Exists(GetSaveFilePath());
    }
    
    #endregion
    
    #region 保存
    
    /// <summary>
    /// 保存游戏数据到文件
    /// </summary>
    public void SaveGame()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSave, true);
            string path = GetSaveFilePath();
            
            File.WriteAllText(path, json);
            
            hasUnsavedChanges = false;
            lastAutoSaveTime = Time.time;
            
            OnSaveCompleted?.Invoke();
            
            Debug.Log($"SaveManager: 游戏已保存到 {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveManager: 保存失败 - {e.Message}");
        }
    }
    
    /// <summary>
    /// 标记有未保存的更改
    /// </summary>
    public void MarkAsDirty()
    {
        hasUnsavedChanges = true;
    }
    
    #endregion
    
    #region 加载
    
    /// <summary>
    /// 从文件加载游戏数据
    /// </summary>
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
                    // 确保字典不为null
                    if (currentSave.upgradeLevels == null)
                    {
                        currentSave.upgradeLevels = new Dictionary<string, int>();
                    }
                    
                    Debug.Log($"SaveManager: 游戏已加载，玩家等级 {currentSave.playerLevel}，技能点 {currentSave.currentSkillPoints}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: 加载失败 - {e.Message}，创建新存档");
                currentSave = new SaveData();
            }
        }
        else
        {
            Debug.Log("SaveManager: 未找到存档，创建新存档");
            currentSave = new SaveData();
        }
        
        OnSaveLoaded?.Invoke();
    }
    
    #endregion
    
    #region 数据访问
    
    /// <summary>
    /// 获取当前存档数据
    /// </summary>
    public SaveData GetCurrentSave()
    {
        return currentSave;
    }
    
    /// <summary>
    /// 获取指定天赋的当前等级
    /// </summary>
    public int GetUpgradeLevel(string upgradeId)
    {
        if (currentSave.upgradeLevels.TryGetValue(upgradeId, out int level))
        {
            return level;
        }
        return 0; // 未解锁
    }
    
    /// <summary>
    /// 设置天赋等级
    /// </summary>
    public void SetUpgradeLevel(string upgradeId, int level)
    {
        currentSave.upgradeLevels[upgradeId] = level;
        MarkAsDirty();
    }
    
    /// <summary>
    /// 获取所有已解锁的天赋
    /// </summary>
    public Dictionary<string, int> GetAllUpgradeLevels()
    {
        return new Dictionary<string, int>(currentSave.upgradeLevels);
    }
    
    /// <summary>
    /// 获取当前技能点
    /// </summary>
    public int GetSkillPoints()
    {
        return currentSave.currentSkillPoints;
    }
    
    /// <summary>
    /// 设置技能点
    /// </summary>
    public void SetSkillPoints(int points)
    {
        currentSave.currentSkillPoints = Mathf.Max(0, points);
        MarkAsDirty();
    }
    
    /// <summary>
    /// 增加技能点
    /// </summary>
    public void AddSkillPoints(int amount)
    {
        SetSkillPoints(currentSave.currentSkillPoints + amount);
    }
    
    /// <summary>
    /// 消耗技能点
    /// </summary>
    public bool SpendSkillPoints(int amount)
    {
        if (currentSave.currentSkillPoints >= amount)
        {
            SetSkillPoints(currentSave.currentSkillPoints - amount);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 获取玩家等级
    /// </summary>
    public int GetPlayerLevel()
    {
        return currentSave.playerLevel;
    }
    
    /// <summary>
    /// 设置玩家等级
    /// </summary>
    public void SetPlayerLevel(int level)
    {
        currentSave.playerLevel = Mathf.Max(1, level);
        MarkAsDirty();
    }
    
    /// <summary>
    /// 获取总经验值
    /// </summary>
    public int GetTotalExperience()
    {
        return currentSave.totalExperience;
    }
    
    /// <summary>
    /// 增加经验值
    /// </summary>
    public void AddExperience(int amount)
    {
        currentSave.totalExperience += Mathf.Max(0, amount);
        MarkAsDirty();
    }
    
    #endregion
    
    #region 删除和重置
    
    /// <summary>
    /// 删除存档文件
    /// </summary>
    public void DeleteSave()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("SaveManager: 存档已删除");
        }
        
        currentSave = new SaveData();
        OnSaveLoaded?.Invoke();
    }
    
    /// <summary>
    /// 重置所有进度（但保留存档文件）
    /// </summary>
    public void ResetProgress()
    {
        currentSave = new SaveData();
        SaveGame();
        Debug.Log("SaveManager: 进度已重置");
    }
    
    #endregion
    
    #region 调试
    
    /// <summary>
    /// 打印当前存档信息
    /// </summary>
    [ContextMenu("打印存档信息")]
    void DebugPrintSaveInfo()
    {
        Debug.Log("=== 存档信息 ===");
        Debug.Log($"存档路径: {GetSaveFilePath()}");
        Debug.Log($"玩家等级: {currentSave.playerLevel}");
        Debug.Log($"总经验值: {currentSave.totalExperience}");
        Debug.Log($"技能点: {currentSave.currentSkillPoints}");
        Debug.Log($"已解锁天赋数: {currentSave.upgradeLevels.Count}");
        foreach (var kvp in currentSave.upgradeLevels)
        {
            Debug.Log($"  - {kvp.Key}: 等级 {kvp.Value}");
        }
    }
    
    #endregion
}
