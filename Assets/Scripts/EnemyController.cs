using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("生命值")]
    public float maxHealth = 30f;
    [HideInInspector]
    public float currentHealth;
    
    [Header("移动设置")]
    public float moveSpeed = 3f;
    
    [Header("攻击设置")]
    public float damage = 10f;
    
    private Transform player;
    private Rigidbody2D rb;
    
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("EnemyController: 未找到 Player 标签的物体！");
        }
    }
    
    void FixedUpdate()
    {
        if (player != null)
        {
            MoveTowardsPlayer();
        }
    }
    
    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        Destroy(gameObject);
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        // 检测是否持续碰撞玩家（贴着时会持续造成伤害）
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
