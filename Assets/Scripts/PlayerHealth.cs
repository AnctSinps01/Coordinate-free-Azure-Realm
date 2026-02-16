using UnityEngine;
using System;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("血量设置")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("无敌帧设置")]
    public float invincibleTime = 1f;
    [SerializeField] private bool isInvincible = false;
    private float invincibleTimer = 0f;
    
    [Header("闪烁效果")]
    public float flashInterval = 0.1f;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;
    
    // 事件：血量变化时触发（当前血量，最大血量）
    public event Action<float, float> OnHealthChanged;
    
    // 事件：受伤时触发
    public event Action OnDamaged;
    
    // 事件：死亡时触发
    public event Action OnDied;
    
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsInvincible => isInvincible;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("PlayerHealth: 未找到 SpriteRenderer，闪烁效果将不会生效！");
        }
        
        currentHealth = maxHealth;
        // 初始化时触发一次事件，更新UI显示
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 订阅游戏重启事件
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart += OnGameRestart;
    }
    
    /// <summary>
    /// 游戏重启时恢复满血
    /// </summary>
    void OnGameRestart()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Update()
    {
        // 处理无敌帧计时
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0)
            {
                EndInvincibility();
            }
        }
    }
    
    void EndInvincibility()
    {
        isInvincible = false;
        
        // 停止闪烁并确保可见
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
    
    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        while (isInvincible)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
        }
        
        // 确保无敌结束后可见
        spriteRenderer.enabled = true;
    }
    
    /// <summary>
    /// 受到伤害（模块二将实现怪物调用此方法）
    /// </summary>
    public void TakeDamage(float damage)
    {
        // 无敌时不受伤害
        if (isInvincible) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // 触发受伤事件
        OnDamaged?.Invoke();
        // 触发血量变化事件
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // 进入无敌状态
        if (currentHealth > 0)
        {
            isInvincible = true;
            invincibleTimer = invincibleTime;
            
            // 开始闪烁效果
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashEffect());
        }
        else
        {
            Die();
        }
    }
    
    /// <summary>
    /// 恢复血量（预留，供后续治疗道具使用）
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    void Die()
    {
        Debug.Log("玩家死亡！");
        OnDied?.Invoke();  // 触发死亡事件
    }
    
    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart -= OnGameRestart;
    }
}
