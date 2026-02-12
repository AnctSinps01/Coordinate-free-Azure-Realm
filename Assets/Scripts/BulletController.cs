using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 1f;
    
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 使用刚体速度让子弹向前移动（美术朝下，所以用 -transform.up）
        rb.linearVelocity = -transform.up * speed;
        
        // 20秒后自动销毁，防止内存泄漏
        Destroy(gameObject, 3f);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果撞到墙壁，销毁子弹
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}