using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    // 公开变量，可在Unity编辑器中调整
    [SerializeField] private float baseSpeed = 1f;      // 速度系数

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 获取鼠标在世界坐标中的位置（2D 平面，Z=0）
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;

        // 计算到鼠标的距离
        float distance = Vector2.Distance(transform.position, mousePos);

        // 动态速度：距离越大，速度越快（但有上限）
        float dynamicSpeed = baseSpeed * distance / (1f + distance);

        // 平滑移向鼠标
        transform.position = Vector2.MoveTowards(
            transform.position,
            mousePos,
            dynamicSpeed * Time.deltaTime
        );
    }
}