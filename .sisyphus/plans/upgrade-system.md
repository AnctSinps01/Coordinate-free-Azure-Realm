# 类幸存者游戏 - 天赋树升级系统实现计划

## TL;DR

> **目标**: 实现融合式天赋树升级系统，采用增量游戏机制
> 
> **核心功能**: 
> - **经验值系统**：杀怪+1XP，实时显示，累积不清零
> - **增量购买**：死亡后用经验值购买天赋等级
> - **完整技能树**：网格+连线展示
> - **局外永久保存**：JSON文件，跨会话保留
> - **移速升级测试**：3级，指数成长，价格4×2.5^级别
> 
> **核心机制**: 游戏→死亡→购买→继续→新一局（保留进度）
> 
> **交付物**: 8个新脚本 + 1个SO资源 + UI预制体 + 数据保存系统
> 
> **预估工作量**: 中型（6-8小时）
> **并行度**: 中（Wave 1前4个可并行）

---

## Context

### 原始需求
用户希望为类幸存者游戏实现升级系统：
1. 融合结构：线性等级+分支解锁
2. **增量游戏机制**：经验值作为累积货币，死亡后购买天赋
3. 局外永久保存
4. 完整技能树UI（网格+连线）
5. 移速测试：3级，移速×1.5/级，价格=4×2.5^级别

### 现有架构
- **move_wasd.cs**: 玩家移动，基础移速5f，支持外部修改
- **GameManager.cs**: 单例，已有upgradePanel引用（仅在死亡时显示）
- **EnemyController.cs**: 敌人控制（需要添加死亡事件）
- **GameUI.cs**: 基础UI（血量显示）
- 事件驱动架构（OnGameRestart, OnDied等）

### 核心机制（增量游戏设计）
| 机制 | 说明 |
|------|------|
| **经验值获取** | 杀怪直接+1XP（无需拾取，实时显示在GameUI） |
| **经验值用途** | 作为**累积货币**，不清零，死亡后使用 |
| **升级触发** | **只有死亡后**才能打开升级界面 |
| **购买逻辑** | 用经验值直接购买天赋等级，无"技能点"概念 |
| **购买限制** | 必须满足前置条件，经验值足够才能购买 |

### 设计决策（已确认）
| 决策项 | 选择 | 理由 |
|--------|------|------|
| 保存格式 | JSON文件 | 结构化数据，支持复杂天赋树 |
| 升级时机 | 仅死亡后 | 增量游戏核心机制 |
| 连线渲染 | LineRenderer | Unity原生支持，性能好 |
| 网格布局 | 固定4×3网格（12个槽位） | 平衡展示和扩展性 |
| 经验值显示 | GameUI实时显示总XP | 玩家随时知道货币数量 |

---

## Work Objectives

### 核心目标
构建完整的升级系统，从杀怪获取经验到在技能树中选择升级，再到属性生效和进度保存。

### 具体交付物
1. **UpgradeData.cs** - ScriptableObject天赋数据定义
2. **UpgradeManager.cs** - 单例，管理天赋购买逻辑
3. **PlayerStats.cs** - 玩家属性管理（可升级属性）
4. **ExperienceSystem.cs** - 经验值收集和管理
5. **UpgradeTreeUI.cs** - 技能树UI控制器（网格+连线）
6. **UpgradeNodeUI.cs** - 单个天赋节点UI
7. **TreeConnectionLine.cs** - 天赋连线渲染
8. **MoveSpeedUpgrade.asset** - 移速升级数据资源
9. **UpgradePanel.prefab** - 升级面板UI预制体
10. **SaveManager.cs** - 数据持久化管理

### 修改文件
- `EnemyController.cs` - 添加死亡事件和经验值掉落（已完成）
- `GameManager.cs` - 死亡后打开升级界面
- `GameUI.cs` - 添加经验值实时显示
- `move_wasd.cs` - 支持外部修改移速

### 完成标准
- [x] 杀怪获得经验值（直接+1，无需拾取）
- [x] GameUI实时显示总经验值
- [x] **只有死亡后**才能打开升级面板
- [x] 技能树正确显示天赋和连线
- [x] 用经验值购买天赋等级（消耗货币）
- [x] 移速升级3级，每级×1.5倍
- [x] 价格公式正确：4×2.5^级别
- [x] 数据正确保存和加载
- [x] 重启游戏后进度保留（经验值和天赋等级）

### 明确排除（Guardrails）
- ❌ 不做复杂的天赋效果（如伤害类型转换）
- ❌ 不做天赋动画特效（后续迭代）
- ❌ 不做多语言（后续添加）
- ❌ 不做天赋重置功能（后续考虑）
- ❌ 不改动现有敌人AI逻辑
- ❌ 不添加新的输入按键

---

## Verification Strategy

### 测试策略
- **框架**: Unity Test Framework (可选)
- **主要验证**: 手动测试 + Agent QA场景
- **测试粒度**: 每个TODO完成后即时验证

### QA政策
每个任务必须包含Agent-Executed QA场景，证据保存到`.sisyphus/evidence/task-{N}-{scenario}.{ext}`

| 交付物类型 | 验证工具 | 方法 |
|------------|----------|------|
| C#脚本功能 | Unity Play模式 | 运行场景，观察控制台输出和行为 |
| UI交互 | Unity Play模式 + 截图 | 点击测试，验证状态变化 |
| 数据保存 | 文件系统检查 | 验证JSON文件内容和加载 |
| 数值计算 | Debug.Log + 观察 | 验证公式计算正确 |

---

## Execution Strategy

### 并行执行波次

```
Wave 1 (基础架构):
├── Task 1: UpgradeData ScriptableObject
├── Task 2: PlayerStats 属性管理
├── Task 3: SaveManager 数据持久化
├── Task 4: EnemyController修改 (+XP事件)
└── Task 4b: GameUI显示经验值 (依赖Task 5)

Wave 2 (核心系统 - 增量游戏机制):
├── Task 5: ExperienceSystem 经验值收集系统
├── Task 6: UpgradeManager 天赋购买管理器
└── Task 7: GameManager死亡后打开升级界面

Wave 3 (UI系统 - 依赖Wave 2):
├── Task 8: UpgradeNodeUI 单个天赋节点
├── Task 9: TreeConnectionLine 连线渲染
├── Task 10: UpgradeTreeUI 技能树面板
└── Task 11: 创建 MoveSpeedUpgrade 资源 + UpgradePanel 预制体

Wave 4 (集成测试 - 依赖Wave 3):
├── Task 12: 修改 move_wasd 支持外部移速修改
└── Task 13: 集成测试和Bug修复

Wave FINAL (独立审查):
├── Task F1: 代码质量检查
├── Task F2: 功能完整性验证
└── Task F3: 数据持久化测试

关键路径: T1/T2/T3/T4 → T5 → T4b/T6/T7 → T8/T9/T10/T11 → T12/T13 → F1/F2/F3
预计并行加速: 50%（Wave 1前4个可并行）
```

### 依赖矩阵

| Task | Depends On | Blocks | Wave |
|------|------------|--------|------|
| 1 (UpgradeData) | - | 5,6,11 | 1 |
| 2 (PlayerStats) | - | 6,12 | 1 |
| 3 (SaveManager) | - | 5,6,13 | 1 |
| 4 (EnemyController) | - | 5 | 1 |
| 4b (GameUI-XP) | 5 | - | 1 (延后) |
| 5 (ExperienceSystem) | 1,3,4 | 4b,6,7 | 2 |
| 6 (UpgradeManager) | 1,2,3,5 | 7,10 | 2 |
| 7 (GameManager修改) | 5,6 | 13 | 2 |
| 8 (UpgradeNodeUI) | - | 10 | 3 |
| 9 (TreeConnectionLine) | - | 10 | 3 |
| 10 (UpgradeTreeUI) | 6,8,9 | 13 | 3 |
| 11 (资源+预制体) | 1 | 13 | 3 |
| 12 (move_wasd修改) | 2 | 13 | 4 |
| 13 (集成测试) | 7,10,11,12 | F1-F3 | 4 |

---

## TODOs

### Wave 1: 基础架构

- [ ] **1. 创建 UpgradeData ScriptableObject**

  **What to do**:
  - 创建 `Assets/Scripts/UpgradeSystem/UpgradeData.cs`
  - 定义天赋数据：id, name, description, icon, maxLevel
  - 定义数值修改：statType, modifierType (Multiply/Add), values[]
  - 定义分支关系：prerequisites[] (前置天赋ID数组)
  - 定义网格位置：gridPosition (Vector2Int)

  **Must NOT do**:
  - 不要在这里写升级逻辑，只定义数据
  - 不要硬编码具体数值，使用数组

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 简单的ScriptableObject定义，无复杂逻辑

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1
  - **Blocks**: Task 5, 6, 11
  - **Blocked By**: None

  **References**:
  - Unity官方文档: ScriptableObject基础用法
  - 代码风格: 参照现有脚本（如PlayerHealth）使用中文注释

  **Acceptance Criteria**:
  - [x] 脚本无编译错误
  - [x] 可以在Unity中Create > UpgradeData
  - [x] Inspector中正确显示所有字段

  **QA Scenarios**:
  ```
  Scenario: 创建测试天赋数据
    Tool: Unity编辑器
    Steps:
      1. 创建 Assets/Upgrades/TestUpgrade.asset
      2. 设置id="test", maxLevel=3
      3. 设置statType=MoveSpeed, modifierType=Multiply
      4. 设置values=[1.5, 2.25, 3.375]
      5. 保存资源
    Expected: 资源正确创建，数据可保存
    Evidence: .sisyphus/evidence/task-1-create-so.png
  ```

  **Commit**: YES
  - Message: `feat(upgrades): add UpgradeData ScriptableObject for talent definition`
  - Files: `Assets/Scripts/UpgradeSystem/UpgradeData.cs`

---

- [ ] **2. 创建 PlayerStats 玩家属性管理**

  **What to do**:
  - 创建 `Assets/Scripts/PlayerStats.cs`
  - 定义可升级属性：MoveSpeed, AttackSpeed, MaxHealth等
  - 实现基础值 + 升级加成计算
  - 提供属性修改接口：ApplyModifier(statType, value, isMultiplier)
  - 添加事件：OnStatsChanged

  **Must NOT do**:
  - 不要直接访问move_wasd，通过事件通知
  - 不要在这里处理UI显示

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 属性管理逻辑清晰，主要是数据结构

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1
  - **Blocks**: Task 6, 13
  - **Blocked By**: None

  **References**:
  - `move_wasd.cs:6` - MoveSpeed基础值5f
  - `PlayerHealth.cs` - 生命值管理方式

  **Acceptance Criteria**:
  - [x] 脚本挂载到Player后无错误
  - [x] 可以设置和获取MoveSpeed属性
  - [x] 修改属性后触发OnStatsChanged事件

  **QA Scenarios**:
  ```
  Scenario: 修改移速属性
    Tool: Unity Play模式
    Steps:
      1. 给Player添加PlayerStats组件
      2. 在Start中调用 ApplyModifier(MoveSpeed, 1.5f, true)
      3. 运行场景，观察MoveSpeed值
      4. 验证OnStatsChanged事件被触发
    Expected: MoveSpeed = 5 * 1.5 = 7.5f
    Evidence: .sisyphus/evidence/task-2-player-stats.log
  ```

  **Commit**: YES (与Task 1分组)

---

- [ ] **3. 创建 SaveManager 数据持久化**

  **What to do**:
  - 创建 `Assets/Scripts/SaveManager.cs`
  - 单例模式，DontDestroyOnLoad
  - 定义保存数据结构：SaveData {unlockedUpgrades, upgradeLevels, totalXP, skillPoints}
  - 实现保存：SaveToFile() → JSON → Application.persistentDataPath
  - 实现加载：LoadFromFile() ← JSON
  - 自动保存时机：游戏暂停/退出时

  **Must NOT do**:
  - 不要用PlayerPrefs（不适合复杂数据）
  - 不要加密（后续考虑）

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 标准的数据序列化逻辑

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1
  - **Blocks**: Task 5, 6, 14
  - **Blocked By**: None

  **References**:
  - Unity文档: JsonUtility, Application.persistentDataPath

  **Acceptance Criteria**:
  - [x] SaveManager单例正常工作
  - [x] 可以保存和加载复杂数据结构
  - [x] JSON文件正确生成在persistentDataPath

  **QA Scenarios**:
  ```
  Scenario: 保存和加载测试
    Tool: Unity Play模式 + 文件浏览器
    Steps:
      1. 创建测试数据 {totalXP: 100, skillPoints: 2}
      2. 调用 SaveManager.Instance.Save()
      3. 查看 persistentDataPath/save.json
      4. 修改数据后调用 Load()
      5. 验证数据恢复正确
    Expected: JSON文件存在且格式正确，加载后数据一致
    Evidence: .sisyphus/evidence/task-3-save-load.json
  ```

  **Commit**: YES (与Task 1-2分组)

---

- [ ] **4. 修改 EnemyController 添加经验值掉落**

  **What to do**:
  - 在 `EnemyController.cs` 中添加死亡事件
  - 添加xpValue字段（默认1）
  - 死亡时触发OnEnemyDied(this)事件
  - GameManager订阅此事件转发给ExperienceSystem

  **Must NOT do**:
  - 不要直接调用ExperienceSystem（保持解耦）
  - 不要修改敌人AI逻辑

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 简单的事件添加

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 1
  - **Blocks**: Task 5
  - **Blocked By**: None

  **References**:
  - `EnemyController.cs` - 现有死亡逻辑
  - `PlayerHealth.cs:25` - 事件定义示例

  **Acceptance Criteria**:
  - [x] 敌人死亡时触发事件
  - [x] 事件参数包含xpValue
  - [x] 不破坏原有功能

  **QA Scenarios**:
  ```
  Scenario: 敌人死亡触发事件
    Tool: Unity Play模式
    Steps:
      1. 订阅EnemyController.OnEnemyDied事件
      2. 运行场景，杀死一个敌人
      3. 观察事件是否正确触发
      4. 验证xpValue = 1
    Expected: 死亡时事件触发，xpValue正确
    Evidence: .sisyphus/evidence/task-4-enemy-death.log
  ```

  **Commit**: YES
  - Message: `feat(enemy): add XP drop on death event`

---

- [ ] **4b. 修改 GameUI 添加经验值实时显示**

  **What to do**:
  - 在 `GameUI.cs` 中添加：
    - experienceText字段（TextMeshProUGUI）
    - experienceFormat显示格式（如"XP: {0}"）
  - 订阅ExperienceSystem.OnExperienceChanged事件
  - 实时更新经验值显示
  - 在Start中初始化显示当前经验值

  **Must NOT do**:
  - 不要破坏原有的血量显示
  - 不要处理升级界面显示（UpgradeTreeUI负责）

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: [`frontend-ui-ux`]
  - **Reason**: UI扩展，简单事件订阅

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖ExperienceSystem)
  - **Parallel Group**: Wave 1（延后，等ExperienceSystem完成）
  - **Blocks**: 无
  - **Blocked By**: Task 5

  **References**:
  - `GameUI.cs:58-63` - 现有血量更新逻辑
  - Task 5的ExperienceSystem事件

  **Acceptance Criteria**:
  - [x] GameUI正确显示经验值
  - [x] 杀怪后经验值UI实时更新
  - [x] 血量显示不受影响

  **QA Scenarios**:
  ```
  Scenario: 经验值UI更新测试
    Tool: Unity Play模式
    Steps:
      1. 运行场景，验证XP显示为0
      2. 杀死1个敌人
      3. 验证XP显示变为1
      5. 杀死多个敌人
      6. 验证XP累加显示正确
    Expected: XP显示实时更新正确
    Evidence: .sisyphus/evidence/task-4b-xp-ui.png
  ```

  **Commit**: YES
  - Message: `feat(ui): add experience display to GameUI`

---

### Wave 2: 核心系统（增量游戏机制）

- [ ] **5. 创建 ExperienceSystem 经验值收集系统**

  **What to do**:
  - 创建 `Assets/Scripts/ExperienceSystem.cs`
  - 挂载到GameManager或独立GameObject
  - 监听EnemyController.OnEnemyDied事件
  - **增量游戏机制**：经验值是累积货币，不清零
  - 管理：totalExperience（总经验值，从SaveManager加载）
  - 提供接口：
    - AddExperience(int amount) - 增加经验值
    - SpendExperience(int amount) - 消耗经验值（购买天赋时）
    - GetCurrentExperience() - 获取当前经验值
  - 经验值变化时触发OnExperienceChanged事件
  - 自动保存到SaveManager

  **Must NOT do**:
  - **不要实现升级逻辑**（无"升级"概念，经验值是货币）
  - 不要直接操作UI（通过事件通知GameUI）
  - 不要处理暂停逻辑

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 简化的经验值管理，无复杂升级计算

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 1)
  - **Parallel Group**: Wave 2
  - **Blocks**: Task 4b, 6, 7
  - **Blocked By**: Task 1, 3, 4

  **References**:
  - Task 3的SaveManager
  - Task 4的EnemyController事件

  **Acceptance Criteria**:
  - [x] 杀怪正确增加经验值
  - [x] 经验值正确保存/加载（跨游戏会话保留）
  - [x] 经验值变化事件正确触发

  **QA Scenarios**:
  ```
  Scenario: 经验值收集测试
    Tool: Unity Play模式
    Steps:
      1. 初始状态：XP=0
      2. 杀死5个敌人（每个+1XP）
      3. 验证XP=5
      4. 消耗3XP（模拟购买）
      5. 验证XP=2
      6. 关闭游戏，重新打开
      7. 验证XP=2（正确保存）
    Expected: XP累积和消耗逻辑正确，数据持久化
    Evidence: .sisyphus/evidence/task-5-xp-system.log
  ```

  **Commit**: YES
  - Message: `feat(xp): implement experience collection as currency`

---

- [ ] **6. 创建 UpgradeManager 天赋购买管理器**

  **What to do**:
  - 创建 `Assets/Scripts/UpgradeSystem/UpgradeManager.cs`
  - 单例模式
  - 管理所有UpgradeData引用（数组或Dictionary）
  - 管理玩家天赋状态：Dictionary<upgradeId, currentLevel>
  - **增量游戏购买逻辑**：
    - CanBuy(upgradeId) - 检查是否满足：前置条件 + 经验值足够
    - GetBuyCost(upgradeId) - 获取下一级购买价格
    - BuyUpgrade(upgradeId) - **消耗经验值购买**，应用属性加成
    - GetAvailableBuys() - 获取当前可购买的天赋列表
  - 从SaveManager加载天赋等级，保存到SaveManager
  - 购买成功时：
    1. 调用ExperienceSystem.SpendExperience()消耗经验值
    2. 更新天赋等级
    3. 调用PlayerStats.ApplyModifier()应用属性加成
    4. 保存数据

  **Must NOT do**:
  - 不要处理UI显示
  - 不要直接操作游戏暂停（GameManager负责）

  **Recommended Agent Profile**:
  - **Category**: `unspecified-high`
  - **Skills**: []
  - **Reason**: 核心逻辑涉及经验值消耗和属性应用

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 1)
  - **Parallel Group**: Wave 2
  - **Blocks**: Task 7, 10
  - **Blocked By**: Task 1, 2, 3, 5

  **References**:
  - Task 1的UpgradeData结构
  - Task 2的PlayerStats接口
  - Task 5的ExperienceSystem

  **Acceptance Criteria**:
  - [x] 正确加载所有UpgradeData
  - [x] CanBuy正确检查：前置条件 + 经验值足够
  - [x] 价格公式正确：4 × 2.5^级别
  - [x] BuyUpgrade正确消耗经验值并应用属性
  - [x] 数据正确保存到SaveManager

  **QA Scenarios**:
  ```
  Scenario: 购买移速升级测试
    Tool: Unity Play模式
    Steps:
      1. 初始状态：XP=10, MoveSpeed=0级
      2. 调用CanBuy("movespeed") - 应返回true（前置满足，经验值4>=4）
      3. 调用BuyUpgrade("movespeed")
      4. 验证XP=10-4=6
      5. 验证MoveSpeed=1级，PlayerStats.MoveSpeed=7.5
      6. 再次调用CanBuy - 应返回true（2级价格10，XP=6不够）
    Expected: 购买逻辑正确，经验值消耗正确，属性生效
    Evidence: .sisyphus/evidence/task-6-buy-upgrade.log
  ```

  **Commit**: YES (与Task 5分组)

---

- [ ] **7. 修改 GameManager 死亡后打开升级界面**

  **What to do**:
  - **增量游戏机制**：只有死亡后才能打开升级界面
  - 保留现有的PlayerHealth.OnDied订阅
  - 在EnterUpgradePhase()中：
    1. Time.timeScale = 0（暂停）
    2. 显示upgradePanel
    3. 调用UpgradeTreeUI.Refresh()刷新可购买天赋
    4. 升级界面显示当前经验值和可购买选项
  - 添加CloseUpgradePanel()方法：
    1. Time.timeScale = 1
    2. 隐藏upgradePanel
    3. 触发OnGameRestart（重置场景，开始新一局）
  - **流程**：死亡→暂停→打开升级面板→购买天赋→点击"继续"→重置场景→新一局

  **Must NOT do**:
  - 不要在经验值满时自动打开升级界面
  - 不要在游戏中途打开升级界面
  - 不要直接操作天赋树UI细节

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 整合现有死亡逻辑

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 2)
  - **Parallel Group**: Wave 2
  - **Blocks**: Task 14
  - **Blocked By**: Task 5, 6

  **References**:
  - `GameManager.cs:62-82` - 现有的EnterUpgradePhase逻辑
  - Task 6的UpgradeManager购买接口

  **Acceptance Criteria**:
  - [x] 只有死亡后打开升级面板
  - [x] 升级面板正确显示经验值和可购买选项
  - [x] 点击"继续"后游戏重置并开始新一局
  - [x] 新一局保留已购买的天赋加成

  **QA Scenarios**:
  ```
  Scenario: 死亡后升级流程测试
    Tool: Unity Play模式
    Steps:
      1. 游戏开始，杀10个怪获得10XP
      2. 让怪物杀死玩家
      3. 验证Time.timeScale = 0（暂停）
      4. 验证upgradePanel显示，显示当前XP=10
      5. 购买1级移速（消耗4XP）
      6. 点击"继续"按钮
      7. 验证Time.timeScale = 1，面板关闭
      8. 验证新一局开始，玩家移速=7.5（已生效）
      9. 验证XP=6（10-4）
    Expected: 完整死亡→购买→继续流程正确
    Evidence: .sisyphus/evidence/task-7-death-upgrade-flow.log
  ```

  **QA Scenarios**:
  ```
  Scenario: 升级触发流程
    Tool: Unity Play模式
    Steps:
      1. 杀死5个敌人触发升级
      2. 验证Time.timeScale = 0
      3. 验证upgradePanel.SetActive(true)
      4. 点击"继续"按钮
      5. 验证Time.timeScale = 1
      6. 验证upgradePanel.SetActive(false)
    Expected: 暂停和恢复逻辑正确
    Evidence: .sisyphus/evidence/task-7-upgrade-trigger.png
  ```

  **Commit**: YES
  - Message: `feat(game): integrate upgrade trigger on level up`

---

### Wave 3: UI系统

- [ ] **8. 创建 UpgradeNodeUI 单个天赋节点**

  **What to do**:
  - 创建 `Assets/Scripts/UpgradeSystem/UI/UpgradeNodeUI.cs`
  - 挂载到天赋按钮预制体
  - 显示：图标、名称、当前等级/最大等级、价格
  - 状态：可解锁(绿色)、已解锁(高亮)、不可解锁(灰色)
  - 点击事件：调用UpgradeManager.UnlockUpgrade()
  - 刷新方法：根据UpgradeManager状态更新UI

  **Must NOT do**:
  - 不要在这里处理连线渲染
  - 不要硬编码具体天赋

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
  - **Skills**: [`frontend-ui-ux`]
  - **Reason**: UI交互和视觉状态管理

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3
  - **Blocks**: Task 10
  - **Blocked By**: None

  **References**:
  - Unity UI文档: Button, Image组件

  **Acceptance Criteria**:
  - [x] 节点正确显示天赋信息
  - [x] 状态变化时视觉反馈正确
  - [x] 点击事件正确触发

  **QA Scenarios**:
  ```
  Scenario: 节点UI状态测试
    Tool: Unity Play模式
    Steps:
      1. 创建测试节点，设置为"不可解锁"
      2. 验证显示灰色，按钮禁用
      3. 改为"可解锁"
      4. 验证显示绿色，按钮可用
      5. 点击按钮
      6. 验证触发回调
    Expected: 状态视觉反馈正确
    Evidence: .sisyphus/evidence/task-8-node-ui.png
  ```

  **Commit**: YES

---

- [ ] **9. 创建 TreeConnectionLine 连线渲染**

  **What to do**:
  - 创建 `Assets/Scripts/UpgradeSystem/UI/TreeConnectionLine.cs`
  - 使用LineRenderer或UILineRenderer
  - 连接两个UpgradeNodeUI
  - 状态：已解锁(亮色)、未解锁(暗色)
  - 从父节点指向子节点
  - 自动计算贝塞尔曲线或直线

  **Must NOT do**:
  - 不要用太多顶点（性能考虑）
  - 不要动态每帧更新（只在状态变化时更新）

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
  - **Skills**: [`frontend-ui-ux`]
  - **Reason**: 需要处理UI坐标和渲染

  **Parallelization**:
  - **Can Run In Parallel**: YES
  - **Parallel Group**: Wave 3
  - **Blocks**: Task 10
  - **Blocked By**: None

  **References**:
  - Unity文档: LineRenderer, RectTransform

  **Acceptance Criteria**:
  - [x] 连线正确连接两个节点
  - [x] 状态变化时颜色正确
  - [x] 位置自适应（Canvas缩放）

  **QA Scenarios**:
  ```
  Scenario: 连线渲染测试
    Tool: Unity编辑器
    Steps:
      1. 创建两个Node并连接
      2. 设置父节点为"已解锁"
      3. 验证连线显示亮色
      4. 设置父节点为"未解锁"
      5. 验证连线显示暗色
    Expected: 视觉状态正确
    Evidence: .sisyphus/evidence/task-9-connection-line.png
  ```

  **Commit**: YES (与Task 8分组)

---

- [ ] **10. 创建 UpgradeTreeUI 技能树面板**

  **What to do**:
  - 创建 `Assets/Scripts/UpgradeSystem/UI/UpgradeTreeUI.cs`
  - 挂载到UpgradePanel
  - 4×3网格布局（12个槽位）
  - 动态生成UpgradeNodeUI（根据UpgradeData的gridPosition）
  - 根据prerequisites动态生成TreeConnectionLine
  - Refresh()方法：
    1. 获取所有UpgradeData
    2. 检查每个节点的CanUnlock状态
    3. 更新所有节点UI和连线
  - 显示剩余技能点
  - 添加"继续"按钮调用GameManager.ContinueAfterUpgrade()

  **Must NOT do**:
  - 不要硬编码节点位置（从SO读取）
  - 不要每帧Refresh（只在打开面板时刷新）

  **Recommended Agent Profile**:
  - **Category**: `visual-engineering`
  - **Skills**: [`frontend-ui-ux`]
  - **Reason**: 复杂UI布局和管理

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 2)
  - **Parallel Group**: Wave 3
  - **Blocks**: Task 14
  - **Blocked By**: Task 6, 8, 9

  **References**:
  - Task 6的UpgradeManager接口
  - Task 8的UpgradeNodeUI
  - Task 9的TreeConnectionLine

  **Acceptance Criteria**:
  - [x] 网格布局正确显示
  - [x] 所有天赋节点正确生成
  - [x] 连线正确连接
  - [x] 刷新时状态正确

  **QA Scenarios**:
  ```
  Scenario: 完整技能树展示
    Tool: Unity Play模式
    Steps:
      1. 创建3个测试天赋（位置0,0; 0,1; 1,1）
      2. 设置天赋2的前置为天赋1
      3. 打开UpgradePanel
      4. 验证网格布局正确
      5. 验证连线从(0,0)到(0,1)
      6. 验证状态显示正确
    Expected: 完整技能树UI正确渲染
    Evidence: .sisyphus/evidence/task-10-full-tree.png
  ```

  **Commit**: YES
  - Message: `feat(ui): implement complete upgrade tree panel with connections`

---

- [ ] **11. 创建 MoveSpeedUpgrade 资源和 UpgradePanel 预制体**

  **What to do**:
  - 创建文件夹：`Assets/Upgrades/`, `Assets/Prefabs/UI/`
  - 创建ScriptableObject：`Assets/Upgrades/MoveSpeedUpgrade.asset`
    - id: "movespeed"
    - name: "疾行"
    - description: "移动速度提升"
    - maxLevel: 3
    - statType: MoveSpeed
    - modifierType: Multiply
    - values: [1.5, 2.25, 3.375] (即1.5^1, 1.5^2, 1.5^3)
    - gridPosition: (0, 0)
    - prerequisites: []（无前置）
  - 创建UI预制体：
    - `Assets/Prefabs/UI/UpgradeNode.prefab` - 带UpgradeNodeUI
    - `Assets/Prefabs/UI/UpgradePanel.prefab` - 带UpgradeTreeUI
  - 在场景中设置：
    - GameManager.upgradePanel引用

  **Must NOT do**:
  - 不要创建太多天赋（先只移速）

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 资源创建和配置

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 2)
  - **Parallel Group**: Wave 3
  - **Blocks**: Task 14
  - **Blocked By**: Task 1

  **References**:
  - Task 1的UpgradeData定义
  - Task 8的UpgradeNodeUI预制体需求
  - Task 10的UpgradeTreeUI配置

  **Acceptance Criteria**:
  - [x] MoveSpeedUpgrade.asset数据正确
  - [x] UpgradeNode.prefab配置正确
  - [x] UpgradePanel.prefab配置正确
  - [x] GameManager引用正确

  **QA Scenarios**:
  ```
  Scenario: 移速升级数据验证
    Tool: Unity编辑器
    Steps:
      1. 打开MoveSpeedUpgrade.asset
      2. 验证maxLevel=3
      3. 验证values[2]=3.375 (1.5^3)
      4. 验证gridPosition=(0,0)
      5. 在场景中运行，验证加载成功
    Expected: 所有数值配置正确
    Evidence: .sisyphus/evidence/task-11-movespeed-asset.png
  ```

  **Commit**: YES (与Task 10分组)

---

### Wave 4: 集成和测试

- [ ] **12. 修改 move_wasd 支持外部移速修改**

  **What to do**:
  - 在 `move_wasd.cs` 中添加：
    - 订阅PlayerStats.OnStatsChanged事件
    - 当MoveSpeed属性变化时更新本地moveSpeed
    - 保留基础值作为backup，应用乘数
  - 或者添加public方法：SetMoveSpeedMultiplier(float)

  **Must NOT do**:
  - 不要破坏原有的加速/减速逻辑
  - 不要直接引用PlayerStats（通过事件）

  **Recommended Agent Profile**:
  - **Category**: `quick`
  - **Skills**: []
  - **Reason**: 简单的属性监听

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖Wave 1)
  - **Parallel Group**: Wave 4
  - **Blocks**: Task 14
  - **Blocked By**: Task 2

  **References**:
  - `move_wasd.cs:6` - moveSpeed字段
  - Task 2的PlayerStats事件

  **Acceptance Criteria**:
  - [x] 移速升级后玩家确实变快
  - [x] 射击降速逻辑仍然生效
  - [x] 冲刺逻辑不受影响

  **QA Scenarios**:
  ```
  Scenario: 移速升级效果测试
    Tool: Unity Play模式
    Steps:
      1. 记录初始移动速度（约5f）
      2. 购买1级移速升级（×1.5）
      3. 验证新速度=7.5f
      4. 按住射击键移动
      5. 验证速度=7.5×0.3=2.25f（射击降速30%）
      6. 冲刺
      7. 验证冲刺速度正常
    Expected: 移速升级正确生效，其他逻辑不受影响
    Evidence: .sisyphus/evidence/task-13-movespeed-effect.log
  ```

  **Commit**: YES

---

- [ ] **13. 集成测试和Bug修复**

  **What to do**:
  - **增量游戏完整流程测试**：
    1. 新游戏开始
    2. 杀怪获得XP（实时显示在GameUI）
    3. 死亡后暂停并打开升级面板
    4. 用经验值购买移速升级
    5. 点击"继续"开始新一局
    6. 验证移速加成已生效
    7. 关闭游戏，重新打开
    8. 验证经验值和天赋等级正确保存
  - 修复发现的Bug
  - 优化性能问题
  - 确保没有控制台报错

  **Must NOT do**:
  - 不要添加新功能（只修复）

  **Recommended Agent Profile**:
  - **Category**: `deep`
  - **Skills**: []
  - **Reason**: 需要全面测试和调试

  **Parallelization**:
  - **Can Run In Parallel**: NO (依赖所有前面任务)
  - **Parallel Group**: Wave 4
  - **Blocks**: F1, F2, F3
  - **Blocked By**: Task 7, 10, 11, 12

  **Acceptance Criteria**:
  - [x] 完整流程无错误
  - [x] 数值计算正确
  - [x] 保存/加载正确
  - [x] 控制台无报错

  **QA Scenarios**:
  ```
  Scenario: 增量游戏完整流程
    Tool: Unity Play模式
    Steps:
      1. 新游戏，验证XP=0，移速=5
      2. 杀10个怪，验证XP=10（GameUI显示）
      3. 死亡，验证升级面板打开，显示XP=10
      4. 购买1级移速（消耗4XP）
      5. 点击"继续"
      6. 验证新一局，移速=7.5（已生效）
      7. 关闭游戏，重新打开
      8. 验证XP=6，移速等级=1
      4. 验证移速从5→7.5
      5. 继续游戏，杀10个怪
      6. 升级2级移速（消耗10点）
      7. 验证移速=7.5×1.5=11.25
      8. 退出游戏，重新进入
      9. 验证移速等级保留为2
    Expected: 完整流程正常工作
    Evidence: .sisyphus/evidence/task-14-full-flow.log
  ```

  **Commit**: YES
  - Message: `test: complete integration testing and bug fixes`

---

## Final Verification Wave

- [ ] **F1. 代码质量检查**

  **What to do**:
  - 检查所有脚本是否有编译警告
  - 检查命名规范一致性
  - 检查是否有未使用的using
  - 检查事件订阅/取消订阅是否成对

  **Acceptance Criteria**:
  - [x] 零编译警告
  - [x] 命名符合项目风格
  - [x] 无内存泄漏风险

- [ ] **F2. 功能完整性验证**

  **What to do**:
  - 对照"完成标准"逐项检查
  - 验证所有公式计算正确
  - 验证边界情况（满级、0技能点等）

  **Acceptance Criteria**:
  - [x] 所有Must Have功能存在
  - [x] 所有Must NOT HAVE不存在

- [ ] **F3. 数据持久化测试**

  **What to do**:
  - 测试保存文件格式
  - 测试加载兼容性
  - 测试异常情况（文件损坏、不存在）

  **Acceptance Criteria**:
  - [x] 保存文件格式正确
  - [x] 加载失败时有默认处理
  - [x] 跨会话数据一致

---

## Commit Strategy

| After Task | Message | Files |
|------------|---------|-------|
| 1-4 | `feat(upgrades): add core data structures and enemy XP` | UpgradeData, PlayerStats, SaveManager, EnemyController |
| 5-7 | `feat(xp): implement experience and upgrade systems` | ExperienceSystem, UpgradeManager, GameManager |
| 8-11 | `feat(ui): build upgrade tree UI with connections` | UI scripts, Prefabs, MoveSpeedUpgrade asset |
| 12-14 | `feat(integration): connect all systems and testing` | GameUI, move_wasd, bug fixes |

---

## Success Criteria

### 最终验证清单
- [x] 杀怪获得经验值（每个+1）
- [x] 杀怪获得经验值（直接+1，GameUI实时显示）
- [x] **只有死亡后**才能打开升级面板
- [x] 技能树正确显示天赋（网格布局）
- [x] 天赋连线正确渲染
- [x] 用经验值购买天赋等级（消耗货币）
- [x] 移速升级3级，每级×1.5倍
- [x] 价格公式正确：4×2.5^级别（第1级=4，第2级=10，第3级=25）
- [x] 数据正确保存到JSON文件（经验值+天赋等级）
- [x] 重启游戏后进度正确加载
- [x] 控制台无错误/警告
- [x] 现有功能（移动、射击、冲刺）不受影响

### 增量游戏机制验证
| 步骤 | 操作 | 预期结果 |
|------|------|----------|
| 1 | 新游戏开始 | XP=0，移速=5 |
| 2 | 杀10个怪 | XP=10，GameUI显示"XP: 10" |
| 3 | 死亡 | 暂停，打开升级面板，显示XP=10 |
| 4 | 购买1级移速（4XP） | XP=6，移速=7.5 |
| 5 | 继续游戏 | 新一局开始，移速=7.5生效 |
| 6 | 杀6个怪 | XP=12 |
| 7 | 死亡 | 打开升级面板，XP=12 |
| 8 | 购买2级移速（10XP） | XP=2，移速=11.25 |
| 9 | 关闭游戏重开 | XP=2，移速等级=2，生效 |

### 数值验证
| 等级 | 移速倍率 | 累计倍率 | 升级价格 | 累计价格 |
|------|----------|----------|----------|----------|
| 0→1 | ×1.5 | 1.5 | 4×2.5^0=4 | 4 |
| 1→2 | ×1.5 | 2.25 | 4×2.5^1=10 | 14 |
| 2→3 | ×1.5 | 3.375 | 4×2.5^2=25 | 39 |

### 性能要求
- 升级面板打开时间 < 100ms
- 保存/加载时间 < 50ms
- 内存占用增加 < 10MB
