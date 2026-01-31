using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class force_gravity : MonoBehaviour
{
    private Rigidbody2D rb;
    public float gravityStrength = 1f; // 引力强度

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {

        // 获取鼠标在世界坐标中的位置（2D 平面）
        // 获取鼠标在世界坐标中的位置（2D 平面，Z=0）
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // 计算从当前物体指向鼠标的向量
        Vector2 direction = (Vector2)mousePos - rb.position;
        float distance = direction.magnitude;

        if (distance > 0.01f) // 防止除零和抖动
        {
            float forceMagnitude = gravityStrength / (distance * distance + 0.1f); // +1 防止无穷大

            Vector2 force = direction.normalized * forceMagnitude;
            rb.AddForce(force, ForceMode2D.Force);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
