using UnityEngine;

// 自动根据 Y 坐标设置渲染顺序：Y 越大（越靠下），显示越靠前
[RequireComponent(typeof(SpriteRenderer))]
public class AutoSortByY : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // 关键逻辑：Y 坐标越大（角色越靠下），sortingOrder 越大，显示越靠前
        // 乘以 10 是为了提高精度（避免两个角色 Y 相差很小却排序相同）
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 200);
    }
}