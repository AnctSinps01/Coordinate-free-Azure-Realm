using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 技能树UI控制器
/// 管理整个天赋树面板的显示和交互
/// </summary>
public class UpgradeTreeUI : MonoBehaviour
{
    [Header("布局设置")]
    public int gridColumns = 4;
    public int gridRows = 3;
    public float cellWidth = 150f;
    public float cellHeight = 150f;
    public float spacing = 20f;
    
    [Header("预制体")]
    public GameObject nodePrefab;
    public GameObject connectionLinePrefab;
    
    [Header("UI引用")]
    public Transform nodesContainer;
    public Transform linesContainer;
    public TextMeshProUGUI experienceText;
    public Button continueButton;
    
    [Header("事件")]
    public GameEvent onUpgradePurchased;
    
    // 节点网格 [列, 行] -> UpgradeNodeUI
    private UpgradeNodeUI[,] nodeGrid;
    
    // 所有连线
    private List<TreeConnectionLine> connectionLines = new List<TreeConnectionLine>();
    
    // 节点映射：upgradeId -> UpgradeNodeUI
    private Dictionary<string, UpgradeNodeUI> nodeLookup = new Dictionary<string, UpgradeNodeUI>();
    
    void Awake()
    {
        // 初始化网格
        nodeGrid = new UpgradeNodeUI[gridColumns, gridRows];
        
        // 绑定继续按钮
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
        }
    }
    
    void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }
    }
    
    /// <summary>
    /// 刷新整个技能树
    /// </summary>
    public void Refresh()
    {
        // 清空现有节点和连线
        ClearAll();
        
        // 更新经验值显示
        UpdateExperienceDisplay();
        
        // 创建所有节点
        CreateAllNodes();
        
        // 创建连线
        CreateAllConnections();
        
        // 更新所有节点状态
        UpdateAllNodeStates();
    }
    
    /// <summary>
    /// 清空所有节点和连线
    /// </summary>
    void ClearAll()
    {
        // 清空节点
        if (nodesContainer != null)
        {
            foreach (Transform child in nodesContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        // 清空连线
        if (linesContainer != null)
        {
            foreach (Transform child in linesContainer)
            {
                Destroy(child.gameObject);
            }
        }
        
        nodeGrid = new UpgradeNodeUI[gridColumns, gridRows];
        connectionLines.Clear();
        nodeLookup.Clear();
    }
    
    /// <summary>
    /// 创建所有天赋节点
    /// </summary>
    void CreateAllNodes()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeTreeUI: UpgradeManager未找到！");
            return;
        }
        
        var allUpgrades = UpgradeManager.Instance.GetAllUpgrades();
        
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;
            
            CreateNode(upgrade);
        }
    }
    
    /// <summary>
    /// 创建单个节点
    /// </summary>
    UpgradeNodeUI CreateNode(UpgradeData upgradeData)
    {
        if (nodePrefab == null || nodesContainer == null)
        {
            Debug.LogError("UpgradeTreeUI: 缺少预制体或容器引用！");
            return null;
        }
        
        // 实例化节点
        GameObject nodeObj = Instantiate(nodePrefab, nodesContainer);
        UpgradeNodeUI nodeUI = nodeObj.GetComponent<UpgradeNodeUI>();
        
        if (nodeUI == null)
        {
            Debug.LogError("UpgradeTreeUI: 预制体缺少UpgradeNodeUI组件！");
            Destroy(nodeObj);
            return null;
        }
        
        // 设置位置
        Vector2Int gridPos = upgradeData.gridPosition;
        if (gridPos.x >= 0 && gridPos.x < gridColumns && gridPos.y >= 0 && gridPos.y < gridRows)
        {
            RectTransform rectTransform = nodeObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float x = gridPos.x * (cellWidth + spacing);
                float y = -(gridPos.y * (cellHeight + spacing));
                rectTransform.anchoredPosition = new Vector2(x, y);
                rectTransform.sizeDelta = new Vector2(cellWidth, cellHeight);
            }
            
            nodeGrid[gridPos.x, gridPos.y] = nodeUI;
        }
        else
        {
            Debug.LogWarning($"UpgradeTreeUI: 天赋 {upgradeData.upgradeId} 的网格位置 {gridPos} 超出范围");
        }
        
        // 获取当前等级
        int currentLevel = UpgradeManager.Instance.GetUpgradeLevel(upgradeData.upgradeId);
        
        // 设置数据
        nodeUI.SetUpgradeData(upgradeData, currentLevel);
        
        // 订阅购买事件
        nodeUI.OnBuyClicked += OnNodeBuyClicked;
        
        // 添加到查找表
        nodeLookup[upgradeData.upgradeId] = nodeUI;
        
        return nodeUI;
    }
    
    /// <summary>
    /// 创建所有连线
    /// </summary>
    void CreateAllConnections()
    {
        if (connectionLinePrefab == null || linesContainer == null)
            return;
        
        var allUpgrades = UpgradeManager.Instance.GetAllUpgrades();
        
        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null || upgrade.prerequisites == null)
                continue;
            
            // 查找子节点
            if (!nodeLookup.TryGetValue(upgrade.upgradeId, out UpgradeNodeUI childNode))
                continue;
            
            // 为每个前置创建连线
            foreach (string prereqId in upgrade.prerequisites)
            {
                if (nodeLookup.TryGetValue(prereqId, out UpgradeNodeUI parentNode))
                {
                    CreateConnection(parentNode, childNode);
                }
            }
        }
    }
    
    /// <summary>
    /// 创建单个连线
    /// </summary>
    void CreateConnection(UpgradeNodeUI parent, UpgradeNodeUI child)
    {
        if (connectionLinePrefab == null || linesContainer == null)
            return;
        
        GameObject lineObj = Instantiate(connectionLinePrefab, linesContainer);
        TreeConnectionLine line = lineObj.GetComponent<TreeConnectionLine>();
        
        if (line != null)
        {
            line.SetNodes(parent, child);
            connectionLines.Add(line);
        }
    }
    
    /// <summary>
    /// 更新所有节点状态
    /// </summary>
    void UpdateAllNodeStates()
    {
        if (UpgradeManager.Instance == null || ExperienceSystem.Instance == null)
            return;
        
        int currentXP = ExperienceSystem.Instance.TotalExperience;
        
        foreach (var kvp in nodeLookup)
        {
            string upgradeId = kvp.Key;
            UpgradeNodeUI nodeUI = kvp.Value;
            UpgradeData upgradeData = nodeUI.GetUpgradeData();
            
            if (upgradeData == null) continue;
            
            int currentLevel = UpgradeManager.Instance.GetUpgradeLevel(upgradeId);
            int maxLevel = upgradeData.maxLevel;
            
            // 确定状态
            UpgradeNodeUI.NodeState state;
            bool canAfford = false;
            
            if (currentLevel >= maxLevel)
            {
                state = UpgradeNodeUI.NodeState.MaxLevel;
            }
            else if (UpgradeManager.Instance.CanBuy(upgradeId))
            {
                state = UpgradeNodeUI.NodeState.Available;
                int cost = UpgradeManager.Instance.GetBuyCost(upgradeId);
                canAfford = currentXP >= cost;
            }
            else if (currentLevel > 0)
            {
                state = UpgradeNodeUI.NodeState.Purchased;
            }
            else
            {
                state = UpgradeNodeUI.NodeState.Locked;
            }
            
            nodeUI.UpdateState(state, canAfford);
        }
        
        // 更新连线状态
        UpdateConnectionLines();
    }
    
    /// <summary>
    /// 更新连线状态
    /// </summary>
    void UpdateConnectionLines()
    {
        foreach (var line in connectionLines)
        {
            if (line == null) continue;
            
            UpgradeNodeUI parent = line.GetParentNode();
            if (parent != null)
            {
                // 如果父节点已解锁（有等级），连线显示亮色
                bool parentUnlocked = parent.GetCurrentLevel() > 0;
                line.SetUnlocked(parentUnlocked);
            }
        }
    }
    
    /// <summary>
    /// 节点购买点击回调
    /// </summary>
    void OnNodeBuyClicked(UpgradeData upgradeData)
    {
        if (upgradeData == null) return;
        
        // 调用UpgradeManager购买
        bool success = UpgradeManager.Instance.BuyUpgrade(upgradeData.upgradeId);
        
        if (success)
        {
            // 刷新显示
            Refresh();
            
            // 触发事件
            onUpgradePurchased?.Raise();
            
            Debug.Log($"UpgradeTreeUI: 购买天赋 {upgradeData.displayName} 成功");
        }
        else
        {
            Debug.LogWarning($"UpgradeTreeUI: 购买天赋 {upgradeData.displayName} 失败");
        }
    }
    
    /// <summary>
    /// 更新经验值显示
    /// </summary>
    void UpdateExperienceDisplay()
    {
        if (experienceText != null && ExperienceSystem.Instance != null)
        {
            experienceText.text = $"经验值: {ExperienceSystem.Instance.TotalExperience}";
        }
    }
    
    /// <summary>
    /// 继续按钮点击
    /// </summary>
    void OnContinueClicked()
    {
        // 调用GameManager继续游戏
        if (GameManager.Instance != null)
        {
            // 使用反射或直接调用，取决于GameManager的可见性
            // 这里假设有一个公共方法
            // GameManager.Instance.ContinueGame();
        }
        
        Debug.Log("UpgradeTreeUI: 点击继续按钮");
    }
    
    /// <summary>
    /// 获取指定位置的节点
    /// </summary>
    public UpgradeNodeUI GetNodeAt(int column, int row)
    {
        if (column >= 0 && column < gridColumns && row >= 0 && row < gridRows)
        {
            return nodeGrid[column, row];
        }
        return null;
    }
    
    /// <summary>
    /// 通过ID获取节点
    /// </summary>
    public UpgradeNodeUI GetNodeById(string upgradeId)
    {
        if (nodeLookup.TryGetValue(upgradeId, out UpgradeNodeUI node))
        {
            return node;
        }
        return null;
    }
}

/// <summary>
/// 简单的游戏事件类（ScriptableObject）
/// 如果没有使用ScriptableObject事件系统，可以删除
/// </summary>
[CreateAssetMenu(fileName = "GameEvent", menuName = "游戏/事件")]
public class GameEvent : ScriptableObject
{
    private System.Action listeners;
    
    public void Raise()
    {
        listeners?.Invoke();
    }
    
    public void RegisterListener(System.Action listener)
    {
        listeners += listener;
    }
    
    public void UnregisterListener(System.Action listener)
    {
        listeners -= listener;
    }
}
