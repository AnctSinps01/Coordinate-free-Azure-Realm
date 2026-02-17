# 类幸存者游戏 - 升级系统架构文档

## 系统概览

### 核心机制（增量游戏设计）

```
游戏循环：
┌─────────────┐    杀怪+XP    ┌─────────────┐
│   游戏中    │ ────────────→ │  获得经验值  │
│  (实时显示)  │               │ (GameUI显示) │
└─────────────┘               └─────────────┘
       │                            │
       │ 死亡触发                    │
       ↓                            ↓
┌─────────────┐    购买天赋    ┌─────────────┐
│  升级界面    │ ←──────────── │  消耗经验值  │
│ (技能树UI)   │               │ (移速/攻击等)│
└─────────────┘               └─────────────┘
       │
       │ 点击继续
       ↓
┌─────────────┐    保留进度    ┌─────────────┐
│   新一局    │ ←──────────── │  JSON存档   │
│ (天赋生效)   │               │ (跨游戏保留) │
└─────────────┘               └─────────────┘
```

---

## 模块架构

### 1. 数据层

#### UpgradeData (ScriptableObject)
- **职责**: 定义单个天赋的所有属性
- **字段**: ID、名称、图标、等级、数值、价格、前置条件、网格位置
- **文件**: `Assets/Scripts/UpgradeSystem/UpgradeData.cs`

#### SaveData (JSON)
- **职责**: 持久化存储玩家进度
- **字段**: upgradeLevels、totalExperience、playerStats
- **路径**: `Application.persistentDataPath/game_save.json`

### 2. 逻辑层

#### ExperienceSystem
- **职责**: 经验值收集和管理（累积货币模式）
- **关键方法**:
  - `AddExperience(int)` - 杀怪获得经验
  - `SpendExperience(int)` - 购买天赋消耗经验
  - `HasEnoughExperience(int)` - 检查余额
- **事件**: `OnExperienceChanged`

#### UpgradeManager
- **职责**: 天赋购买逻辑和属性应用
- **关键方法**:
  - `CanBuy(string)` - 检查是否可购买（前置+余额）
  - `BuyUpgrade(string)` - 执行购买（扣XP+升级+应用属性）
  - `GetBuyCost(string)` - 获取价格
- **依赖**: PlayerStats、ExperienceSystem、SaveManager

#### PlayerStats
- **职责**: 玩家属性管理和加成计算
- **关键方法**:
  - `ApplyModifier(StatType, float, ModifierType)` - 应用属性修饰器
  - `GetStat(StatType)` - 获取当前属性值
- **属性**: MoveSpeed、AttackSpeed、MaxHealth、Damage等

#### SaveManager
- **职责**: 数据持久化
- **关键方法**:
  - `SaveGame()` - 保存到JSON
  - `LoadGame()` - 从JSON加载
  - `GetUpgradeLevel(string)` - 获取天赋等级
  - `SetUpgradeLevel(string, int)` - 设置天赋等级
- **自动保存**: 应用暂停/退出时

### 3. 触发层

#### EnemyController
- **职责**: 敌人死亡时发放经验值
- **事件**: `OnEnemyDied`（静态事件）
- **字段**: `xpValue`（默认1）

#### GameManager
- **职责**: 游戏流程控制
- **关键方法**:
  - `EnterUpgradePhase()` - 死亡后打开升级面板
  - `ContinueGame()` - 继续游戏（新一局）
- **流程**: 死亡→暂停→显示UI→购买→继续→重置场景

### 4. UI层

#### GameUI
- **职责**: 游戏内UI显示
- **元素**: 血量、经验值（实时更新）
- **事件订阅**: `ExperienceSystem.OnExperienceChanged`

#### UpgradeTreeUI
- **职责**: 技能树面板控制器
- **布局**: 4×3网格
- **关键方法**:
  - `Refresh()` - 刷新整个技能树
  - `CreateAllNodes()` - 生成天赋节点
  - `CreateAllConnections()` - 生成连线
  - `UpdateAllNodeStates()` - 更新状态

#### UpgradeNodeUI
- **职责**: 单个天赋节点UI
- **状态**: Locked → Available → Purchased → MaxLevel
- **颜色**: 灰色 → 绿色 → 黄色 → 橙色
- **交互**: 点击购买

#### TreeConnectionLine
- **职责**: 天赋连线渲染
- **组件**: 自定义UILineRenderer
- **状态**: 未解锁（灰色）→ 已解锁（亮色）

### 5. 表现层

#### move_wasd
- **职责**: 玩家移动控制
- **修改**: 订阅PlayerStats事件，动态更新移速
- **关键方法**: `GetCurrentMoveSpeed()` - 从PlayerStats读取有效移速

---

## 数据流

### 经验值流向

```
Enemy死亡
    ↓
EnemyController.OnEnemyDied
    ↓
ExperienceSystem.AddExperience()
    ↓
SaveManager.AddExperience() [自动保存]
    ↓
GameUI.UpdateExperienceDisplay() [UI更新]
```

### 购买天赋流向

```
点击UpgradeNodeUI
    ↓
UpgradeTreeUI.OnNodeBuyClicked()
    ↓
UpgradeManager.BuyUpgrade()
    ├── ExperienceSystem.SpendExperience() [扣XP]
    ├── SaveManager.SetUpgradeLevel() [保存等级]
    └── PlayerStats.ApplyModifier() [应用属性]
        ↓
    move_wasd.OnStatChanged() [更新移速]
        ↓
    GameUI.UpdateExperienceDisplay() [刷新UI]
```

### 游戏重启流向

```
游戏启动
    ↓
SaveManager.LoadGame() [加载存档]
    ↓
UpgradeManager.LoadUpgradeProgress()
    ↓
UpgradeManager.ApplyAllPurchasedUpgrades()
    ↓
PlayerStats.ApplyModifier() [应用所有加成]
    ↓
ExperienceSystem.LoadExperience()
    ↓
move_wasd.Start() [获取当前移速]
```

---

## 数值设计

### 移速升级

| 等级 | 数值效果 | 价格 | 累计价格 | 有效移速 |
|------|---------|------|---------|---------|
| 0级 | ×1.0 (基础) | - | 0 | 5.0 |
| 1级 | ×1.5 | 4 XP | 4 XP | 7.5 |
| 2级 | ×2.25 | 10 XP | 14 XP | 11.25 |
| 3级 | ×3.375 | 25 XP | 39 XP | 16.875 |

**价格公式**: 4 × 2.5^(level-1)  
**效果公式**: 基础值 5 × levelValue

---

## 事件系统

### 全局事件

| 事件 | 发布者 | 订阅者 | 用途 |
|------|--------|--------|------|
| `OnEnemyDied` | EnemyController | ExperienceSystem | 发放经验值 |
| `OnExperienceChanged` | ExperienceSystem | GameUI | 更新经验值显示 |
| `OnStatChanged` | PlayerStats | move_wasd | 更新移速 |
| `OnUpgradePurchased` | UpgradeManager | UpgradeTreeUI | 刷新技能树 |
| `OnGameRestart` | GameManager | 多个组件 | 重置游戏状态 |

---

## 文件结构

```
Assets/
├── Scripts/
│   ├── ExperienceSystem.cs           # 经验值系统
│   ├── PlayerStats.cs                # 属性管理
│   ├── SaveManager.cs                # 存档管理
│   ├── GameManager.cs                # 游戏管理（修改）
│   ├── GameUI.cs                     # 游戏UI（修改）
│   ├── EnemyController.cs            # 敌人控制（修改）
│   ├── move_wasd.cs                  # 玩家移动（修改）
│   └── UpgradeSystem/
│       ├── UpgradeData.cs            # 天赋数据定义
│       ├── UpgradeManager.cs         # 天赋购买管理
│       └── UI/
│           ├── UpgradeNodeUI.cs      # 天赋节点UI
│           ├── TreeConnectionLine.cs # 连线渲染
│           └── UpgradeTreeUI.cs      # 技能树面板
├── Upgrades/
│   ├── README.md                     # 配置指南
│   └── MoveSpeedUpgrade.asset        # 移速天赋（手动创建）
├── Prefabs/
│   └── UI/
│       ├── UpgradeNode.prefab        # 节点预制体
│       ├── ConnectionLine.prefab     # 连线预制体
│       └── UpgradePanel.prefab       # 面板预制体
├── Resources/
│   └── Upgrades/
│       └── MoveSpeedUpgrade.asset    # 运行时加载
└── Tests/
    └── IntegrationTestGuide.md       # 测试指南
```

---

## 配置步骤

### 1. 创建天赋资源
1. 右键 Project → `Create > 游戏/天赋升级`
2. 配置字段（见 Upgrades/README.md）
3. 保存到 `Assets/Resources/Upgrades/`

### 2. 创建UI预制体
1. 创建 UpgradeNode 按钮预制体
2. 创建 ConnectionLine 空物体预制体
3. 创建 UpgradePanel 面板预制体（包含 UpgradeTreeUI）

### 3. 场景配置
1. Player: 添加 PlayerStats 组件
2. GameManager: 配置 UpgradePanel 引用
3. GameUI: 添加 ExperienceText 引用
4. 创建空物体: 添加 ExperienceSystem、UpgradeManager、SaveManager

### 4. 测试运行
1. 运行游戏
2. 杀怪获得经验值
3. 死亡打开升级面板
4. 购买移速升级
5. 继续游戏验证效果
6. 重启游戏验证存档

---

## 扩展建议

### 添加新天赋类型

1. **创建ScriptableObject**
   - 复制 MoveSpeedUpgrade.asset
   - 修改 ID、名称、statType
   - 配置价格和数值

2. **配置网格位置**
   - 设置 gridPosition（不与其他天赋重叠）
   - 可选：添加前置条件（prerequisites）

3. **测试**
   - 运行游戏验证显示
   - 测试购买逻辑
   - 验证属性生效

### 添加新属性类型

1. **修改 StatType 枚举**
   - 在 UpgradeData.cs 中添加新类型

2. **修改 PlayerStats**
   - 添加 base 字段
   - 在 GetBaseStat() 中添加 case

3. **修改受影响的组件**
   - 订阅 OnStatChanged 事件
   - 应用新属性值

---

## 注意事项

1. **数据完整性**
   - UpgradeID 必须唯一
   - levelValues 长度 >= maxLevel
   - gridPosition 不重叠

2. **性能优化**
   - PlayerStats 使用缓存机制
   - 升级面板只在打开时刷新
   - 自动保存不阻塞主线程

3. **兼容性**
   - 射击降速逻辑保持不变
   - 冲刺系统不受影响
   - 存档版本兼容（添加version字段）

---

## 完成状态

✅ Wave 1: 基础架构（4/4任务）  
✅ Wave 2: 核心系统（3/3任务）  
✅ Wave 3: UI系统（4/4任务）  
✅ Wave 4: 集成测试（2/2任务）  

**总计**: 13/13 任务完成
