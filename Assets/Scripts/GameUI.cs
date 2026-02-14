using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("UI引用")]
    public TextMeshProUGUI healthText;
    
    [Header("玩家引用")]
    public PlayerHealth playerHealth;
    
    [Header("显示格式")]
    public string healthFormat = "{0}/{1}";
    
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
