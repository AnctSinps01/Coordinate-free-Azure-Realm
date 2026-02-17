using UnityEngine;
using UnityEngine.UI;

public class DashUI : MonoBehaviour
{
    [Header("组件引用")]
    [Tooltip("PlayerDash组件（通常在同物体的玩家角色上）")]
    public PlayerDash playerDash;
    
    [Tooltip("冷却进度条（Slider组件）")]
    public Slider cooldownSlider;
    
    [Header("可选：文本显示")]
    [Tooltip("显示冷却时间的Text组件（可选）")]
    public Text cooldownText;
    
    [Tooltip("显示状态文字的Text组件（可选）")]
    public Text statusText;
    
    [Header("视觉设置")]
    [Tooltip("冲刺可用时的颜色")]
    public Color readyColor = Color.green;
    
    [Tooltip("冷却中的颜色")]
    public Color cooldownColor = Color.red;
    
    [Tooltip("是否显示小数位时间")]
    public bool showDecimalTime = true;
    
    [Header("进度条样式")]
    [Tooltip("true = 满条表示可用，冷却时减少\nfalse = 空条表示可用，冷却时填满")]
    public bool fillWhenReady = false;
    
    [Header("位置设置")]
    [Tooltip("是否自动定位到屏幕左下角")]
    public bool autoPosition = true;
    
    [Tooltip("左下角的偏移量（x=水平偏移，y=垂直偏移）")]
    public Vector2 offset = new Vector2(20f, 20f);
    
    [Tooltip("进度条尺寸（宽度，高度）")]
    public Vector2 barSize = new Vector2(150f, 15f);
    
    // 缓存Image组件用于颜色变化
    private Image sliderFillImage;
    private RectTransform rectTransform;
    
    void Start()
    {
        // 获取RectTransform
        rectTransform = GetComponent<RectTransform>();
        
        // 自动定位到屏幕左下角
        if (autoPosition && rectTransform != null)
        {
            PositionToBottomLeft();
        }
        
        // 自动查找PlayerDash（如果没有手动指定）
        if (playerDash == null)
        {
            playerDash = GetComponent<PlayerDash>();
            if (playerDash == null)
            {
                playerDash = FindObjectOfType<PlayerDash>();
            }
        }
        
        // 自动查找Slider（如果没有手动指定且此物体有Slider）
        if (cooldownSlider == null)
        {
            cooldownSlider = GetComponent<Slider>();
        }
        
        // 获取Slider的填充图像
        if (cooldownSlider != null)
        {
            sliderFillImage = cooldownSlider.fillRect?.GetComponent<Image>();
        }
        
        // 初始化Slider
        if (cooldownSlider != null)
        {
            cooldownSlider.minValue = 0f;
            cooldownSlider.maxValue = 1f;
            cooldownSlider.value = fillWhenReady ? 1f : 0f;
        }
        
        // 验证引用
        if (playerDash == null)
        {
            Debug.LogWarning("[DashUI] PlayerDash component not found! Please assign it in the inspector.");
        }
        if (cooldownSlider == null)
        {
            Debug.LogWarning("[DashUI] Cooldown Slider not found! Please assign it in the inspector.");
        }
    }
    
    /// <summary>
    /// 将UI定位到屏幕左下角
    /// </summary>
    private void PositionToBottomLeft()
    {
        if (rectTransform == null) return;
        
        // 设置锚点为左下角
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);
        
        // 设置偏移位置
        rectTransform.anchoredPosition = offset;
        
        // 设置尺寸
        rectTransform.sizeDelta = barSize;
        
        // 确保不跟随父物体缩放（如果父物体是Canvas或玩家）
        rectTransform.localScale = Vector3.one;
    }
    
    void Update()
    {
        if (playerDash == null) return;
        
        UpdateSlider();
        UpdateText();
        UpdateColor();
    }
    
    /// <summary>
    /// 更新进度条数值
    /// </summary>
    private void UpdateSlider()
    {
        if (cooldownSlider == null) return;
        
        float cooldownPercent = playerDash.GetCooldownPercent();
        
        if (fillWhenReady)
        {
            // 满条表示可用，冷却时从1减少到0
            cooldownSlider.value = 1f - cooldownPercent;
        }
        else
        {
            // 空条表示可用，冷却时从0增加到1
            cooldownSlider.value = cooldownPercent;
        }
    }
    
    /// <summary>
    /// 更新文本显示
    /// </summary>
    private void UpdateText()
    {
        // 更新冷却时间文本
        if (cooldownText != null)
        {
            if (playerDash.IsOnCooldown)
            {
                float remaining = playerDash.GetCooldownRemaining();
                if (showDecimalTime)
                {
                    cooldownText.text = remaining.ToString("F1") + "s";
                }
                else
                {
                    cooldownText.text = Mathf.CeilToInt(remaining) + "s";
                }
            }
            else
            {
                cooldownText.text = "";
            }
        }
        
        // 更新状态文本
        if (statusText != null)
        {
            if (playerDash.IsDashing)
            {
                statusText.text = "冲刺中!";
                statusText.color = Color.cyan;
            }
            else if (playerDash.CanDash)
            {
                statusText.text = "冲刺就绪";
                statusText.color = readyColor;
            }
            else
            {
                statusText.text = "冷却中...";
                statusText.color = cooldownColor;
            }
        }
    }
    
    /// <summary>
    /// 更新进度条颜色
    /// </summary>
    private void UpdateColor()
    {
        if (sliderFillImage == null) return;
        
        if (playerDash.CanDash)
        {
            sliderFillImage.color = readyColor;
        }
        else
        {
            sliderFillImage.color = cooldownColor;
        }
    }
    
    /// <summary>
    /// 设置是否填充（运行时切换样式）
    /// </summary>
    public void SetFillStyle(bool fillWhenReady)
    {
        this.fillWhenReady = fillWhenReady;
    }
}
