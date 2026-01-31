using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class force_wasd : MonoBehaviour
{
    public float forceMagnitude = 10f;
    private Rigidbody2D rb;
    private Vector2 moveInput; // 临时存储输入方向

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 每帧检测输入（因为输入需要即时响应）
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D 或 左右箭头
        float y = Input.GetAxisRaw("Vertical");   // W/S 或 上下箭头
        moveInput = new Vector2(x, y);
    }

    // 固定频率施加物理力
    void FixedUpdate()
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 force = transform.TransformDirection(moveInput.normalized) * forceMagnitude;
            rb.AddForce(force);
        }
    }
}