using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wasd_move : MonoBehaviour
{
    // 公开变量，可在Unity编辑器中进行调整
    public float moveSpeed = 5f; // 定义物体的移动速度

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 获取水平方向的输入 (A/D 或 左/右箭头)
        float horizontalInput = Input.GetAxis("Horizontal");
        // 获取垂直方向的输入 (W/S 或 上/下箭头)
        float verticalInput = Input.GetAxis("Vertical");

        // 创建一个移动向量
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0f);

        // 根据输入移动物体
        // Time.deltaTime 确保移动速度与帧率无关，表现更平滑
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}