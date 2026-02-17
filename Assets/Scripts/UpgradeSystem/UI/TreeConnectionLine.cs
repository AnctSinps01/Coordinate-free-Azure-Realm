using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 技能树连线渲染
/// 使用UILineRenderer连接两个天赋节点
/// </summary>
[RequireComponent(typeof(UILineRenderer))]
public class TreeConnectionLine : MonoBehaviour
{
    [Header("连线设置")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public float lineWidth = 4f;
    
    [Header("动画")]
    public bool animateOnUnlock = true;
    public float animationDuration = 0.3f;
    
    private UILineRenderer lineRenderer;
    private UpgradeNodeUI parentNode;
    private UpgradeNodeUI childNode;
    private bool isUnlocked = false;
    
    void Awake()
    {
        lineRenderer = GetComponent<UILineRenderer>() ?? gameObject.AddComponent<UILineRenderer>();
        SetupLineRenderer();
    }
    
    void SetupLineRenderer()
    {
        if (lineRenderer != null)
        {
            lineRenderer.lineWidth = lineWidth;
            lineRenderer.color = lockedColor;
        }
    }
    
    /// <summary>
    /// 设置连接的节点
    /// </summary>
    public void SetNodes(UpgradeNodeUI parent, UpgradeNodeUI child)
    {
        parentNode = parent;
        childNode = child;
        
        // 立即更新连线位置
        UpdateLinePosition();
    }
    
    /// <summary>
    /// 更新连线位置（当节点位置变化时调用）
    /// </summary>
    public void UpdateLinePosition()
    {
        if (parentNode == null || childNode == null || lineRenderer == null)
            return;
        
        // 获取节点在RectTransform中的位置
        Vector2 parentPos = GetLocalPosition(parentNode.GetComponent<RectTransform>());
        Vector2 childPos = GetLocalPosition(childNode.GetComponent<RectTransform>());
        
        // 设置连线路径（简单的直线，可以改为贝塞尔曲线）
        Vector2[] points = new Vector2[] { parentPos, childPos };
        lineRenderer.points = points;
    }
    
    /// <summary>
    /// 获取RectTransform的本地位置
    /// </summary>
    Vector2 GetLocalPosition(RectTransform rectTransform)
    {
        if (rectTransform == null) return Vector2.zero;
        return rectTransform.anchoredPosition;
    }
    
    /// <summary>
    /// 设置连线状态
    /// </summary>
    public void SetUnlocked(bool unlocked)
    {
        if (isUnlocked == unlocked) return;
        
        isUnlocked = unlocked;
        
        if (animateOnUnlock && unlocked)
        {
            // 播放解锁动画
            StartCoroutine(AnimateUnlock());
        }
        else
        {
            // 直接设置颜色
            if (lineRenderer != null)
            {
                lineRenderer.color = unlocked ? unlockedColor : lockedColor;
            }
        }
    }
    
    /// <summary>
    /// 解锁动画协程
    /// </summary>
    System.Collections.IEnumerator AnimateUnlock()
    {
        if (lineRenderer == null) yield break;
        
        float elapsed = 0f;
        Color startColor = lockedColor;
        Color targetColor = unlockedColor;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            lineRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }
        
        lineRenderer.color = targetColor;
    }
    
    /// <summary>
    /// 获取当前解锁状态
    /// </summary>
    public bool IsUnlocked()
    {
        return isUnlocked;
    }
    
    /// <summary>
    /// 获取父节点
    /// </summary>
    public UpgradeNodeUI GetParentNode()
    {
        return parentNode;
    }
    
    /// <summary>
    /// 获取子节点
    /// </summary>
    public UpgradeNodeUI GetChildNode()
    {
        return childNode;
    }
}

/// <summary>
/// 简单的UI线渲染器
/// 基于Unity UI的直线绘制
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : Graphic
{
    public Vector2[] points;
    public float lineWidth = 2f;
    public bool usePolygonCollider = false;
    
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        
        if (points == null || points.Length < 2)
            return;
        
        for (int i = 0; i < points.Length - 1; i++)
        {
            DrawLineSegment(vh, points[i], points[i + 1]);
        }
    }
    
    void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * lineWidth * 0.5f;
        
        int index = vh.currentVertCount;
        
        // 四个顶点
        vh.AddVert(start - normal, color, Vector2.zero);
        vh.AddVert(start + normal, color, Vector2.zero);
        vh.AddVert(end + normal, color, Vector2.zero);
        vh.AddVert(end - normal, color, Vector2.zero);
        
        // 两个三角形
        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index + 2, index + 3, index);
    }
    
    /// <summary>
    /// 强制刷新（当points变化时调用）
    /// </summary>
    public void ForceRefresh()
    {
        SetVerticesDirty();
    }
    
    void Update()
    {
        // 如果有动画或其他变化，每帧刷新
        if (points != null && points.Length > 1)
        {
            SetVerticesDirty();
        }
    }
}
