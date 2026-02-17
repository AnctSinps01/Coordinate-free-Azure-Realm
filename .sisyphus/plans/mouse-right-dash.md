# 鼠标右键冲刺功能设计文档

## 功能概述
为玩家角色添加鼠标右键触发的短距离冲刺能力。冲刺为纯快速移动，无无敌帧，无传送效果。

## 核心参数

| 参数名 | 类型 | 说明 | 默认值建议 |
|--------|------|------|------------|
| `dashDistance` | float | 冲刺距离（单位：Unity单位） | 3f |
| `dashSpeed` | float | 冲刺速度（单位/秒） | 15f |
| `cooldownTime` | float | 冷却时间（秒） | 1.5f |

## 行为规则

### 触发条件
- 鼠标右键按下（Input.GetMouseButtonDown(1)）
- 冲刺不在冷却中
- 冲刺未在进行中

### 冲刺方向
1. 优先：当前移动输入方向（WASD）
2. 备选：角色朝向（transform.right）
3. 如果移动输入为0且角色朝向无效，则不执行冲刺

### 冲刺实现
- 使用 `Rigidbody2D.velocity` 在 `FixedUpdate` 中控制移动
- 冲刺期间禁用普通移动输入（可选，根据需求调整）
- 遇到碰撞时正常停止（不穿透墙体）

### 冷却机制
- 冲刺结束后开始计时
- 使用 `Time.time` 或协程管理冷却
- 提供 `GetCooldownPercent()` 供UI读取进度

### 中断处理
- 冲刺期间无法再次触发冲刺
- 冲刺期间普通移动输入被忽略（冲刺优先）

## 架构设计

### 组件分离
```
PlayerDash (核心逻辑)
├── 监听输入
├── 管理冲刺状态
├── 控制Rigidbody2D速度
└── 提供冷却状态查询

DashUI (UI显示)
├── 监听PlayerDash状态
├── 更新Slider进度条
└── 显示可用/冷却状态
```

### 与现有系统集成
- 可与 `move_wasd.cs` 共存
- 可与 `move_follow_mouse.cs` 共存
- 通过冲刺状态标志控制是否禁用普通移动

## 视觉效果
- **基础版**：无额外特效，仅有高速移动效果
- 可选扩展：拖尾、粒子、屏幕震动

## UI设计
- **简单进度条**：位于屏幕合适位置（建议：角色上方或屏幕角落）
- 冲刺可用时：进度条满，显示高亮
- CD中：进度条显示剩余冷却百分比

## 文件清单

| 文件名 | 路径 | 说明 |
|--------|------|------|
| `PlayerDash.cs` | `Assets/Scripts/PlayerDash.cs` | 冲刺核心逻辑 |
| `DashUI.cs` | `Assets/Scripts/DashUI.cs` | 冲刺UI组件 |

## 测试场景
1. 正常冲刺：右键触发，角色快速移动指定距离
2. 冷却测试：连续右键，第二次应无效
3. 碰撞测试：冲刺撞墙应正常停止
4. 方向测试：WASD各方向均可正确冲刺
5. 静止冲刺：无输入时朝角色朝向冲刺
6. UI同步：进度条与CD状态一致

## 验收标准
- [ ] 鼠标右键可触发冲刺
- [ ] 冲刺距离、速度、CD参数可调
- [ ] 冲刺期间无无敌帧，碰撞正常
- [ ] CD期间无法再次冲刺
- [ ] 进度条正确显示CD状态
- [ ] 与现有移动系统无冲突

---

## TODOs

### 任务 1: 创建 PlayerDash.cs 冲刺核心组件
**状态**: ⬜ 待完成  
**优先级**: 🔴 高  
**预计耗时**: 20分钟

**目标**: 创建独立的冲刺逻辑组件，可挂载到玩家角色上

**实现要点**:
1. 创建 `Assets/Scripts/PlayerDash.cs`
2. 定义可配置参数：`dashDistance`, `dashSpeed`, `cooldownTime`
3. 实现 `Update()` 监听鼠标右键输入
4. 实现 `FixedUpdate()` 处理冲刺物理移动
5. 使用 `Rigidbody2D.velocity` 控制移动
6. 实现冷却计时逻辑
7. 提供公共方法 `GetCooldownPercent()` 供UI读取
8. 添加冲刺状态标志 `IsDashing`，供其他组件查询

**关键代码结构**:
```csharp
[Header("冲刺设置")]
public float dashDistance = 3f;
public float dashSpeed = 15f;
public float cooldownTime = 1.5f;

private bool isDashing = false;
private bool isCooldown = false;
private float cooldownTimer = 0f;
private Vector2 dashDirection;
private float dashDistanceRemaining;

// 状态查询属性
public bool IsDashing => isDashing;
public float GetCooldownPercent() => isCooldown ? cooldownTimer / cooldownTime : 0f;
```

**QA验证**:
- [ ] 挂载到玩家角色后，点击Play
- [ ] 右键触发冲刺，角色是否快速移动
- [ ] 连续右键，第二次是否被CD阻止
- [ ] 撞墙时是否正常停止（不穿透）
- [ ] 各方向移动时冲刺方向是否正确

---

### 任务 2: 创建 DashUI.cs 冷却进度条组件
**状态**: ⬜ 待完成  
**优先级**: 🔴 高  
**预计耗时**: 15分钟

**目标**: 创建可复用的冲刺冷却UI组件

**实现要点**:
1. 创建 `Assets/Scripts/DashUI.cs`
2. 引用 `PlayerDash` 组件获取状态
3. 使用 `UnityEngine.UI.Slider` 显示进度
4. 在 `Update()` 中同步冷却百分比
5. 可选：冲刺可用时改变颜色/高亮

**UI设置步骤**:
1. 在场景中创建 Canvas（如不存在）
2. 创建 Slider 作为子对象
3. 将 `DashUI` 挂载到 Slider 或玩家角色
4. 在 Inspector 中配置 Slider 引用

**关键代码**:
```csharp
public class DashUI : MonoBehaviour
{
    [SerializeField] private PlayerDash playerDash;
    [SerializeField] private Slider cooldownSlider;
    
    void Update()
    {
        if (playerDash != null && cooldownSlider != null)
        {
            float percent = playerDash.GetCooldownPercent();
            cooldownSlider.value = 1f - percent; // 满为可用
        }
    }
}
```

**QA验证**:
- [ ] 进入Play模式，进度条初始为满
- [ ] 右键冲刺后，进度条开始减少
- [ ] CD结束后，进度条恢复满值
- [ ] 多次冲刺，进度条同步正确

---

### 任务 3: 集成测试与调参
**状态**: ⬜ 待完成  
**优先级**: 🟡 中  
**预计耗时**: 15分钟

**目标**: 验证与现有系统兼容性，调整参数手感

**测试清单**:
- [ ] 与 `move_wasd.cs` 共存测试：冲刺期间WASD输入是否正确处理
- [ ] 与 `PlayerShooting.cs` 共存测试：冲刺时能否射击
- [ ] 不同距离/速度参数的手感测试
- [ ] 不同CD时长的平衡性测试

**推荐参数范围**:
| 参数 | 轻度 | 标准 | 重度 |
|------|------|------|------|
| dashDistance | 2f | 3f | 5f |
| dashSpeed | 10f | 15f | 20f |
| cooldownTime | 1f | 1.5f | 2.5f |

**QA验证**:
- [ ] 所有测试项通过
- [ ] 参数调整后手感良好
- [ ] 无报错或警告

---

### 任务 4: 文档与使用指南
**状态**: ⬜ 待完成  
**优先级**: 🟢 低  
**预计耗时**: 10分钟

**目标**: 编写组件使用说明

**内容**:
1. 如何挂载组件
2. 参数说明与调参建议
3. 与其他移动系统的协作方式
4. 常见问题排查

---

## 执行顺序

```
Task 1 (PlayerDash.cs) ───────────────────────┐
                                              ├──→ Task 3 (集成测试)
Task 2 (DashUI.cs) ───────────────────────────┘
                         ↓
                   Task 4 (文档)
```

**说明**: 任务1和任务2可并行开发，完成后进行集成测试。

---

## 快速开始指南

### 1. 挂载冲刺组件
1. 选择玩家角色 GameObject
2. 添加 Component → PlayerDash
3. 在 Inspector 中调整参数

### 2. 设置UI进度条
1. 创建 UI → Slider
2. 配置 Slider 为 "Bottom To Top" 或 "Left To Right"
3. 挂载 DashUI 组件
4. 拖拽引用 PlayerDash 和 Slider

### 3. 调整手感
在 Inspector 中实时调整参数，直到找到理想手感。

---

## 扩展建议

### 可选增强
- **无敌帧**: 添加 `isInvincible` 标志，配合 `PlayerHealth.cs`
- **冲刺音效**: 添加 AudioSource 播放冲刺音效
- **冲刺特效**: 添加粒子系统或拖尾效果
- **多段冲刺**: 支持2-3段连续冲刺（类似黑魂）
- **方向锁定**: 冲刺期间锁定角色朝向

---

*计划创建时间: 2026-02-17*  
*预计总耗时: 60分钟*  
*复杂度: 中等*

