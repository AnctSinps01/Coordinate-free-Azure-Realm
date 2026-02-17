using UnityEngine;

public class move_wasd : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float acceleration = 50f;
    public float deceleration = 30f;
    
    [Header("射击时移速")]
    [Range(0f, 1f)]
    [Tooltip("射击时的移速比例（0-1），1表示不降速")]
    public float shootingSpeedMultiplier = 0.3f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private float currentSpeedMultiplier = 1f;
    private bool isDashing = false;
    
    // 基础移速备份
    private float baseMoveSpeed;
    // 当前移速倍率（来自升级系统）
    private float upgradeSpeedMultiplier = 1f;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        baseMoveSpeed = moveSpeed;
        
        // 订阅冲刺事件
        PlayerDash playerDash = GetComponent<PlayerDash>();
        if (playerDash != null)
        {
            playerDash.OnDashStart += OnDashStart;
            playerDash.OnDashEnd += OnDashEnd;
        }
        
        // 订阅射击事件
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.OnShootStart += OnShootStart;
            shooting.OnShootEnd += OnShootEnd;
        }
        
        // 订阅游戏重启事件
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart += OnGameRestart;
        
        // 订阅移速升级事件
        SubscribeToPlayerStats();
    }
    
    void OnDestroy()
    {
        PlayerDash playerDash = GetComponent<PlayerDash>();
        if (playerDash != null)
        {
            playerDash.OnDashStart -= OnDashStart;
            playerDash.OnDashEnd -= OnDashEnd;
        }
        
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.OnShootStart -= OnShootStart;
            shooting.OnShootEnd -= OnShootEnd;
        }
        
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart -= OnGameRestart;
        
        UnsubscribeFromPlayerStats();
    }
    
    void SubscribeToPlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatChanged += OnStatChanged;
            UpdateMoveSpeedMultiplier();
        }
    }
    
    void UnsubscribeFromPlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatChanged -= OnStatChanged;
        }
    }
    
    void OnStatChanged(StatType statType, float newValue)
    {
        if (statType == StatType.MoveSpeed)
        {
            UpdateMoveSpeedMultiplier();
        }
    }
    
    void UpdateMoveSpeedMultiplier()
    {
        if (PlayerStats.Instance != null)
        {
            // 从PlayerStats获取移速倍率
            float statsMoveSpeed = PlayerStats.Instance.MoveSpeed;
            if (statsMoveSpeed > 0 && baseMoveSpeed > 0)
            {
                // 计算倍率：statsValue / baseValue
                upgradeSpeedMultiplier = statsMoveSpeed / baseMoveSpeed;
            }
        }
    }
    
    void OnGameRestart()
    {
        currentVelocity = Vector2.zero;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
    
    void OnShootStart()
    {
        currentSpeedMultiplier = shootingSpeedMultiplier;
    }
    
    void OnShootEnd()
    {
        currentSpeedMultiplier = 1f;
    }
    
    void OnDashStart(bool dashing)
    {
        isDashing = dashing;
    }
    
    void OnDashEnd(bool dashing)
    {
        isDashing = dashing;
    }
    
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }
    
    void FixedUpdate()
    {
        if (isDashing)
        {
            return;
        }
        
        // 计算最终移速：基础值 × 升级倍率 × 射击倍率
        float finalSpeed = baseMoveSpeed * upgradeSpeedMultiplier * currentSpeedMultiplier;
        Vector2 targetVelocity = moveInput * finalSpeed;
        
        if (moveInput != Vector2.zero)
        {
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                Vector2.zero,
                deceleration * Time.fixedDeltaTime
            );
        }
        
        rb.linearVelocity = currentVelocity;
    }
}
