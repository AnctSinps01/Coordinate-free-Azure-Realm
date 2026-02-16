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
    public float shootingSpeedMultiplier = 0.3f;  // 默认30%
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private float currentSpeedMultiplier = 1f;    // 当前速度倍率
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
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
    }
    
    /// <summary>
    /// 游戏重启时重置速度向量为零
    /// </summary>
    void OnGameRestart()
    {
        currentVelocity = Vector2.zero;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
    
    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        PlayerShooting shooting = GetComponent<PlayerShooting>();
        if (shooting != null)
        {
            shooting.OnShootStart -= OnShootStart;
            shooting.OnShootEnd -= OnShootEnd;
        }
        
        // 取消订阅游戏重启事件
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart -= OnGameRestart;
    }
    
    void OnShootStart()
    {
        currentSpeedMultiplier = shootingSpeedMultiplier;
    }
    
    void OnShootEnd()
    {
        currentSpeedMultiplier = 1f;
    }
    
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized;
    }
    
    void FixedUpdate()
    {
        // 根据射击状态应用速度倍率
        float effectiveSpeed = moveSpeed * currentSpeedMultiplier;
        Vector2 targetVelocity = moveInput * effectiveSpeed;
        
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
