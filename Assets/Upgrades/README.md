# 移速升级资源配置指南

## 手动创建 MoveSpeedUpgrade.asset

### 步骤 1: 创建资源文件
1. 在 Unity 中，右键 Project 窗口
2. 选择 `Create > 游戏/天赋升级`
3. 命名为 `MoveSpeedUpgrade`
4. 保存到 `Assets/Resources/Upgrades/` 文件夹

### 步骤 2: 配置字段

| 字段 | 值 | 说明 |
|------|-----|------|
| **upgradeId** | `movespeed` | 唯一标识符 |
| **displayName** | `疾行` | 显示名称 |
| **description** | `提升移动速度` | 描述 |
| **icon** | (拖入Sprite) | 图标 |
| **maxLevel** | `3` | 最大3级 |
| **statType** | `MoveSpeed` | 影响移速 |
| **modifierType** | `Multiply` | 乘法模式 |
| **levelValues** | `[1.5, 2.25, 3.375]` | 每级倍数 |
| **levelCosts** | `[4, 10, 25]` | 每级价格 |
| **prerequisites** | (空数组) | 无前置 |
| **gridPosition** | `X:0, Y:0` | 网格位置(0,0) |

### 数值说明

**价格计算**: 4 × 2.5^(level-1)
- 1级: 4 × 2.5^0 = **4 XP**
- 2级: 4 × 2.5^1 = **10 XP**
- 3级: 4 × 2.5^2 = **25 XP**

**效果计算**: 基础移速 5 × levelValue
- 0级: 5 × 1 = **5** (基础)
- 1级: 5 × 1.5 = **7.5**
- 2级: 5 × 2.25 = **11.25**
- 3级: 5 × 3.375 = **16.875**

### 可选：使用编辑器辅助方法

在编辑器中选中 MoveSpeedUpgrade.asset 后，可以在 Inspector 中调用以下方法（需要添加编辑器扩展）：

```csharp
// 生成等级数值（指数增长）
GenerateLevelValues(1.5f);  // 结果: [1.5, 2.25, 3.375]

// 生成等级价格
GenerateLevelCosts(4f, 2.5f);  // 结果: [4, 10, 25]
```

### 文件位置确认

确保资源文件在以下路径：
```
Assets/
└── Resources/
    └── Upgrades/
        └── MoveSpeedUpgrade.asset
```

UpgradeManager 会从 `Resources/Upgrades` 自动加载所有天赋数据。

---

## UpgradePanel 预制体配置

### 创建步骤

1. 在场景中创建一个空物体，命名为 `UpgradePanel`
2. 添加 `UpgradeTreeUI` 组件
3. 配置以下字段：

### UpgradeTreeUI 组件配置

| 字段 | 配置 |
|------|------|
| **Grid Settings** | Columns: 4, Rows: 3, Cell: 150×150, Spacing: 20 |
| **Node Prefab** | (拖入升级节点预制体) |
| **Connection Prefab** | (拖入连线预制体) |
| **Nodes Container** | (创建空子物体作为容器) |
| **Lines Container** | (创建空子物体作为容器) |
| **Experience Text** | (拖入TextMeshPro显示经验值) |
| **Continue Button** | (拖入继续按钮) |

### 子物体结构

```
UpgradePanel (GameObject)
├── Background (Image - 半透明黑色背景)
├── Title (TextMeshPro - "选择天赋")
├── ExperienceDisplay (TextMeshPro - 显示当前经验值)
├── TreeContainer (空物体)
│   ├── LinesContainer (空物体 - 放连线)
│   └── NodesContainer (空物体 - 放节点)
└── ContinueButton (Button)
    └── Text (TextMeshPro - "继续")
```

### 节点预制体 (UpgradeNodePrefab)

创建一个按钮预制体，包含：
- **Button** 组件
- **Image** (背景)
- **Icon** (Image - 显示天赋图标)
- **NameText** (TextMeshPro - 天赋名称)
- **LevelText** (TextMeshPro - 当前等级/最大等级)
- **CostText** (TextMeshPro - 价格)
- **LockIcon** (Image - 锁图标，未解锁时显示)

挂载 `UpgradeNodeUI` 脚本，并配置所有引用字段。

### 连线预制体 (ConnectionPrefab)

创建一个空物体预制体，包含：
- `TreeConnectionLine` 脚本（自动添加 UILineRenderer）

---

## GameManager 配置

在场景中选中 GameManager，配置：

| 字段 | 值 |
|------|-----|
| **Upgrade Panel** | (拖入UpgradePanel预制体) |
| **Continue Text** | (可选，拖入提示文本) |
| **Upgrade Tree UI** | (拖入UpgradePanel的UpgradeTreeUI组件) |

---

## 完整测试流程

1. 配置 MoveSpeedUpgrade.asset
2. 创建 UpgradeNode 预制体
3. 创建 ConnectionLine 预制体
4. 创建 UpgradePanel 预制体
5. 配置 GameManager 引用
6. 在 GameUI 中添加 ExperienceText
7. 运行测试：
   - 杀怪获得经验值
   - 死亡后打开升级面板
   - 购买移速升级
   - 继续新一局
   - 验证移速增加
