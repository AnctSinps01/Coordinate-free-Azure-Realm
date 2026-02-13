using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("生命值")]
    public float maxHealth = 30f;
    [HideInInspector]
    public float currentHealth;
    
    [Header("移动设置")]
    public float moveSpeed = 3f;
    
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
}
