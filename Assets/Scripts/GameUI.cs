using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI引用")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI experienceText;  // 经验值显示
    
    [Header("玩家引用")]
    public PlayerHealth playerHealth;
    
    [Header("显示格式")]
    public string healthFormat = "{0}/{1}";
    public string experienceFormat = "XP: {0}";  // 经验值显示格式
    
    void Start()
    {
        // 自动查找玩家（如果没有手动指定）
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }
        
        // 订阅血量变化事件（先取消再订阅，防止重复订阅）
        if (playerHealth != null)
        {
            UnsubscribeEvents();
            playerHealth.OnHealthChanged += UpdateHealthDisplay;
            // 初始化显示
            UpdateHealthDisplay(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
        else
        {
            Debug.LogWarning("GameUI: 未找到 PlayerHealth！请确保场景中有带Player标签且挂载PlayerHealth脚本的物体。");
        }
        
        // 订阅经验值变化事件
        SubscribeExperienceEvents();
        // 初始化经验值显示
        InitializeExperienceDisplay();
    }
    
    void OnDisable()
    {
        // 取消订阅，防止内存泄漏
        UnsubscribeEvents();
    }
    
    void UnsubscribeEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthDisplay;
        }
        
        // 取消订阅经验值事件
        if (ExperienceSystem.Instance != null)
        {
            ExperienceSystem.Instance.OnExperienceChanged -= UpdateExperienceDisplay;
        }
    }
    
    /// <summary>
    /// 订阅经验值系统事件
    /// </summary>
    void SubscribeExperienceEvents()
    {
        if (ExperienceSystem.Instance != null)
        {
            ExperienceSystem.Instance.OnExperienceChanged += UpdateExperienceDisplay;
        }
        else
        {
            Debug.LogWarning("GameUI: 未找到 ExperienceSystem，经验值显示将不会更新");
        }
    }
    
    /// <summary>
    /// 初始化经验值显示
    /// </summary>
    void InitializeExperienceDisplay()
    {
        if (experienceText != null && ExperienceSystem.Instance != null)
        {
            UpdateExperienceDisplay(ExperienceSystem.Instance.TotalExperience, 0);
        }
    }
    
    /// <summary>
    /// 更新经验值显示
    /// </summary>
    public void UpdateExperienceDisplay(int currentXP, int delta)
    {
        if (experienceText != null)
        {
            experienceText.text = string.Format(experienceFormat, currentXP);
        }
    }
    
    /// <summary>
    /// 更新血量显示
    /// </summary>
    public void UpdateHealthDisplay(float current, float max)
    {
        if (healthText != null)
        {
            healthText.text = string.Format(healthFormat, Mathf.CeilToInt(current), Mathf.CeilToInt(max));
        }
    }
}
