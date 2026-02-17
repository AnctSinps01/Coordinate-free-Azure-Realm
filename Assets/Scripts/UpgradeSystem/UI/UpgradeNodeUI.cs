using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 天赋节点UI
/// 单个天赋按钮的显示和交互
/// </summary>
public class UpgradeNodeUI : MonoBehaviour
{
    [Header("UI组件")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button buyButton;
    public Image backgroundImage;
    public Image lockIcon;
    
    [Header("状态颜色")]
    public Color availableColor = Color.green;
    public Color purchasedColor = Color.yellow;
    public Color lockedColor = Color.gray;
    public Color maxLevelColor = new Color(1f, 0.5f, 0f); // 橙色
    
    // 关联的天赋数据
    private UpgradeData upgradeData;
    
    // 当前等级
    private int currentLevel;
    
    // 当前状态
    private NodeState currentState;
    
    // 事件：点击购买按钮
    public event Action<UpgradeData> OnBuyClicked;
    
    // 节点状态枚举
    public enum NodeState
    {
        Locked,     // 未解锁（前置未满足）
        Available,  // 可购买（前置满足，经验值足够）
        Purchased,  // 已购买（可以继续升级）
        MaxLevel    // 已满级
    }
    
    void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnButtonClick);
        }
    }
    
    void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnButtonClick);
        }
    }
    
    /// <summary>
    /// 设置天赋数据并刷新显示
    /// </summary>
    public void SetUpgradeData(UpgradeData data, int level)
    {
        upgradeData = data;
        currentLevel = level;
        
        if (upgradeData != null)
        {
            UpdateVisuals();
        }
    }
    
    /// <summary>
    /// 更新节点状态
    /// </summary>
    public void UpdateState(NodeState state, bool canAfford = false)
    {
        currentState = state;
        
        // 更新视觉
        switch (state)
        {
            case NodeState.Locked:
                SetVisuals(lockedColor, false, true);
                break;
                
            case NodeState.Available:
                SetVisuals(canAfford ? availableColor : lockedColor, canAfford, false);
                break;
                
            case NodeState.Purchased:
                SetVisuals(purchasedColor, true, false);
                break;
                
            case NodeState.MaxLevel:
                SetVisuals(maxLevelColor, false, false);
                break;
        }
    }
    
    /// <summary>
    /// 更新所有视觉元素
    /// </summary>
    void UpdateVisuals()
    {
        // 图标
        if (iconImage != null && upgradeData.icon != null)
        {
            iconImage.sprite = upgradeData.icon;
        }
        
        // 名称
        if (nameText != null)
        {
            nameText.text = upgradeData.displayName;
        }
        
        // 等级显示
        if (levelText != null)
        {
            levelText.text = $"{currentLevel}/{upgradeData.maxLevel}";
        }
        
        // 价格显示
        UpdateCostText();
    }
    
    /// <summary>
    /// 更新价格文本
    /// </summary>
    public void UpdateCostText()
    {
        if (costText != null)
        {
            if (currentState == NodeState.MaxLevel)
            {
                costText.text = "已满级";
            }
            else if (currentState == NodeState.Locked)
            {
                costText.text = "未解锁";
            }
            else
            {
                int cost = upgradeData.GetCostForLevel(currentLevel + 1);
                costText.text = $"{cost} XP";
            }
        }
    }
    
    /// <summary>
    /// 设置视觉状态
    /// </summary>
    void SetVisuals(Color bgColor, bool buttonInteractable, bool showLock)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = bgColor;
        }
        
        if (buyButton != null)
        {
            buyButton.interactable = buttonInteractable;
        }
        
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(showLock);
        }
    }
    
    /// <summary>
    /// 按钮点击回调
    /// </summary>
    void OnButtonClick()
    {
        if (currentState == NodeState.Available || currentState == NodeState.Purchased)
        {
            OnBuyClicked?.Invoke(upgradeData);
        }
    }
    
    /// <summary>
    /// 获取当前关联的天赋数据
    /// </summary>
    public UpgradeData GetUpgradeData()
    {
        return upgradeData;
    }
    
    /// <summary>
    /// 获取当前等级
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// 获取当前状态
    /// </summary>
    public NodeState GetCurrentState()
    {
        return currentState;
    }
}
