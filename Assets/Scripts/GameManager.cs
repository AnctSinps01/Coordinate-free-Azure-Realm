using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("升级面板引用")]
    public GameObject upgradePanel;
    public TextMeshProUGUI continueText;
    public UpgradeTreeUI upgradeTreeUI;  // 技能树UI控制器
    
    [Header("玩家引用")]
    public PlayerHealth playerHealth;
    private GameObject playerGameObject;
    
    [Header("地图系统引用")]
    public MapGenerator mapGenerator;
    public EnemySpawner enemySpawner;
    
    bool isInUpgrade = false;
    
    // 事件：游戏重启时触发
    public event Action OnGameRestart;
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        if (playerHealth == null)
        {
            playerGameObject = GameObject.FindGameObjectWithTag("Player");
            if (playerGameObject != null) playerHealth = playerGameObject.GetComponent<PlayerHealth>();
        }
        else
        {
            playerGameObject = playerHealth.gameObject;
        }
        
        // 订阅玩家死亡事件
        if (playerHealth != null)
            playerHealth.OnDied += EnterUpgradePhase;
        
        // 隐藏升级面板
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    void Update()
    {
        if (isInUpgrade && Input.GetKeyDown(KeyCode.Space))
        {
            ContinueGame();
        }
    }
    
    void EnterUpgradePhase()
    {
        isInUpgrade = true;
        Time.timeScale = 0;  // 暂停游戏
        
        // 清空地图和敌人
        ClearGameWorld();
        
        // 隐藏玩家
        if (playerGameObject != null)
            playerGameObject.SetActive(false);
        
        // 显示升级面板
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);
            
            // 刷新技能树UI（显示可购买选项和当前经验值）
            if (upgradeTreeUI != null)
            {
                upgradeTreeUI.Refresh();
            }
            
            if (continueText != null)
                continueText.text = "Press \"Space\" to Continue";
        }
    }
    
    void ClearGameWorld()
    {
        // 清空地图
        if (mapGenerator != null)
            mapGenerator.ClearMap();
        
        // 清空敌人
        if (enemySpawner != null)
            enemySpawner.ClearAllEnemies();
        
        // 清空子弹
        ClearAllBullets();
    }
    
    void ClearAllBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        foreach (var bullet in bullets)
        {
            Destroy(bullet);
        }
    }
    
    void ContinueGame()
    {
        isInUpgrade = false;
        
        // 显示玩家
        if (playerGameObject != null)
            playerGameObject.SetActive(true);
        
        // 隐藏升级面板
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // 重新生成地图
        if (mapGenerator != null)
            mapGenerator.ClearMap();
            mapGenerator.GenerateMap();
        
        // 重置敌人生成器
        if (enemySpawner != null)
            enemySpawner.Reset();
        
        // 触发游戏重启事件（各组件自行重置状态）
        OnGameRestart?.Invoke();
        
        // 恢复游戏
        Time.timeScale = 1;
    }
    
    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= EnterUpgradePhase;
    }
}
