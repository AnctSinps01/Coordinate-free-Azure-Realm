using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerDash : MonoBehaviour
{
    [Header("冲刺设置")]
    [Tooltip("冲刺距离（Unity单位）")]
    public float dashDistance = 3f;
    
    [Tooltip("冲刺速度（单位/秒）")]
    public float dashSpeed = 15f;
    
    [Tooltip("冷却时间（秒）")]
    public float cooldownTime = 1.5f;
    
    [Header("输入设置")]
    [Tooltip("冲刺按键")]
    public KeyCode dashKey = KeyCode.Space;
    
    [Header("调试")]
    public bool showDebugInfo = false;
    
    // 组件引用
    private Rigidbody2D rb;
    
    // 状态变量
    private bool isDashing = false;
    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;
    private Vector2 dashDirection;
    private float dashDistanceRemaining;
    private Vector2 dashStartPosition;
    
    // 公共属性（供其他组件查询）
    public bool IsDashing => isDashing;
    public bool IsOnCooldown => isOnCooldown;
    public bool CanDash => !isDashing && !isOnCooldown;
    
    // 事件定义 - 供其他组件订阅
    public delegate void DashStateHandler(bool isDashing);
    public event DashStateHandler OnDashStart;
    public event DashStateHandler OnDashEnd;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[PlayerDash] Rigidbody2D component not found!");
            enabled = false;
            return;
        }
        
        // 订阅游戏重启事件
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart += OnGameRestart;
    }
    
    void Update()
    {
        // 处理输入
        HandleInput();
        
        // 更新冷却计时
        UpdateCooldown();
        
        // 调试输出
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerDash] IsDashing: {isDashing}, CanDash: {CanDash}, CD: {GetCooldownPercent():P0}");
        }
    }
    
    void FixedUpdate()
    {
        // 处理冲刺移动
        if (isDashing)
        {
            PerformDash();
        }
    }
    
    /// <summary>
    /// 处理冲刺输入
    /// </summary>
    private void HandleInput()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(dashKey))
        {
            if (CanDash)
            {
                StartDash();
            }
            else if (showDebugInfo)
            {
                Debug.Log($"[PlayerDash] Dash blocked: IsDashing={isDashing}, IsOnCooldown={isOnCooldown}");
            }
        }
    }
    
    /// <summary>
    /// 开始冲刺
    /// </summary>
    private void StartDash()
    {
        // 获取冲刺方向
        dashDirection = GetDashDirection();
        
        // 如果方向为零，不执行冲刺
        if (dashDirection == Vector2.zero)
        {
            if (showDebugInfo)
            {
                Debug.Log("[PlayerDash] Dash cancelled: no direction input");
            }
            return;
        }
        
        // 设置冲刺状态
        isDashing = true;
        dashDistanceRemaining = dashDistance;
        dashStartPosition = transform.position;
        
        // 触发冲刺开始事件
        OnDashStart?.Invoke(true);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerDash] Dash started! Direction: {dashDirection}, Distance: {dashDistance}");
        }
    }
    
    /// <summary>
    /// 获取冲刺方向
    /// 优先级：1. WASD输入方向 2. 角色朝向
    /// </summary>
    private Vector2 GetDashDirection()
    {
        // 1. 优先使用WASD输入方向
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 inputDirection = new Vector2(horizontal, vertical);
        
        if (inputDirection != Vector2.zero)
        {
            return inputDirection.normalized;
        }
        
        // 2. 如果没有输入，使用角色朝向（基于transform.right）
        // 假设角色朝右时 transform.right = (1, 0)
        Vector2 facingDirection = transform.right;
        if (facingDirection != Vector2.zero)
        {
            return facingDirection.normalized;
        }
        
        // 3. 方向无效
        return Vector2.zero;
    }
    
    /// <summary>
    /// 执行冲刺移动（在FixedUpdate中调用）
    /// </summary>
    private void PerformDash()
    {
        // 计算本帧要移动的距离
        float moveDistance = dashSpeed * Time.fixedDeltaTime;
        
        // 如果剩余距离小于本帧移动距离，只移动到目标位置
        if (moveDistance >= dashDistanceRemaining)
        {
            moveDistance = dashDistanceRemaining;
            EndDash();
        }
        else
        {
            dashDistanceRemaining -= moveDistance;
        }
        
        // 应用速度
        rb.linearVelocity = dashDirection * dashSpeed;
        
        // 检查是否已移动足够距离（基于位置）
        float distanceMoved = Vector2.Distance(dashStartPosition, transform.position);
        if (distanceMoved >= dashDistance)
        {
            EndDash();
        }
    }
    
    /// <summary>
    /// 结束冲刺
    /// </summary>
    private void EndDash()
    {
        isDashing = false;
        isOnCooldown = true;
        cooldownTimer = cooldownTime;
        
        // 停止移动（将速度设为0，让普通移动系统接管）
        rb.linearVelocity = Vector2.zero;
        
        // 触发冲刺结束事件
        OnDashEnd?.Invoke(false);
        
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerDash] Dash ended! Distance moved: {Vector2.Distance(dashStartPosition, transform.position):F2}");
        }
    }
    
    /// <summary>
    /// 更新冷却计时
    /// </summary>
    private void UpdateCooldown()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
                
                if (showDebugInfo)
                {
                    Debug.Log("[PlayerDash] Cooldown finished!");
                }
            }
        }
    }
    
    /// <summary>
    /// 获取冷却进度百分比（0-1）
    /// 0 = 冷却完成，可以冲刺
    /// 1 = 刚开始冷却
    /// </summary>
    public float GetCooldownPercent()
    {
        if (!isOnCooldown) return 0f;
        return cooldownTimer / cooldownTime;
    }
    
    /// <summary>
    /// 获取冷却剩余时间（秒）
    /// </summary>
    public float GetCooldownRemaining()
    {
        return cooldownTimer;
    }
    
    /// <summary>
    /// 强制重置冲刺（用于游戏重启等场景）
    /// </summary>
    public void ResetDash()
    {
        isDashing = false;
        isOnCooldown = false;
        cooldownTimer = 0f;
        dashDistanceRemaining = 0f;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[PlayerDash] Dash reset!");
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 冲刺期间撞墙会提前结束冲刺
        if (isDashing)
        {
            // 检查碰撞是否是墙体/障碍物（通过tag判断，可选）
            // 如果没有特定tag，所有碰撞都会中断冲刺
            if (showDebugInfo)
            {
                Debug.Log($"[PlayerDash] Dash interrupted by collision with: {collision.gameObject.name}");
            }
            
            EndDash();
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅游戏重启事件
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameRestart -= OnGameRestart;
    }
    
    /// <summary>
    /// 游戏重启时重置冲刺状态
    /// </summary>
    private void OnGameRestart()
    {
        // 如果正在冲刺，先触发结束事件
        if (isDashing)
        {
            isDashing = false;
            OnDashEnd?.Invoke(false);
        }
        
        // 重置所有状态
        isOnCooldown = false;
        cooldownTimer = 0f;
        dashDistanceRemaining = 0f;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[PlayerDash] Dash reset on game restart!");
        }
    }
    
    // 可视化调试
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        // 绘制冲刺方向
        if (isDashing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)dashDirection * 2f);
            
            // 绘制目标位置
            Vector2 targetPos = dashStartPosition + dashDirection * dashDistance;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPos, 0.2f);
        }
    }
}
