using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("射击设置")]
    public GameObject bulletPrefab;  // 子弹预制体
    public Transform firePoint;      // 发射点
    public float bulletSpeed = 10f;  // 子弹速度
    
    void Update()
    {
        // 检测鼠标左键按下
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }
    
    void Shoot()
    {
        // 获取鼠标在世界空间的坐标
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // 确保z轴为0（2D游戏）
        
        // 计算射击方向（从发射点到鼠标位置）
        Vector2 shootDirection = (mousePosition - firePoint.position).normalized;
        
        // 计算旋转角度（+90度补偿，因为子弹美术朝下而非朝右）
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg + 90f;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        
        // 在FirePoint位置实例化子弹，并应用旋转角度
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);
        
        // 设置子弹速度（通过BulletController组件）
        BulletController bulletController = bullet.GetComponent<BulletController>();
        if (bulletController != null)
        {
            bulletController.speed = bulletSpeed;
        }
    }
}