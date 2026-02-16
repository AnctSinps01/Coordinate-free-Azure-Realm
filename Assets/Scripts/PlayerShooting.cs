using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class PlayerShooting : MonoBehaviour
{
    [Header("射击设置")]
    public GameObject bulletPrefab;  // 子弹预制体
    public Transform firePoint;      // 发射点
    public float bulletSpeed = 10f;  // 子弹速度
    
    [Header("攻速设置")]
    [Tooltip("发射间隔时间（秒），越小越快")]
    public float fireRate = 0.2f;    // 发射间隔（0.2秒 = 每秒5发）
    
    // 事件系统
    public event Action OnShootStart;  // 开始射击（按住左键）
    public event Action OnShoot;       // 每次发射子弹
    public event Action OnShootEnd;    // 停止射击（松开左键）
    
    private float nextFireTime = 0f;   // 下次可发射时间
    private bool wasShooting = false;  // 上一帧是否在射击
    
    void Update()
    {
        // 检测鼠标左键状态
        int pointerId = -1; // 鼠标左键对应 -1
        bool isShooting = Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject(pointerId);
        
        // 检测射击状态变化，触发事件
        if (isShooting && !wasShooting)
        {
            // 刚开始射击
            OnShootStart?.Invoke();
        }
        else if (!isShooting && wasShooting)
        {
            // 停止射击
            OnShootEnd?.Invoke();
        }
        
        wasShooting = isShooting;
        
        // 射击逻辑
        if (isShooting)
        {
            // 检查是否到达下次可发射时间
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }
    
    void Shoot()
    {
        // 触发发射事件
        OnShoot?.Invoke();
        
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
