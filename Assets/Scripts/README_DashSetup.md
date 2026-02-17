# Unity 操作步骤指南

## 概述
本文档说明如何在Unity中配置**空格键冲刺**功能。

---

## 步骤 1: 为玩家角色添加冲刺组件

### 操作
1. 在 **Hierarchy** 窗口中选择玩家角色 GameObject
2. 在 Inspector 窗口点击 **Add Component**
3. 搜索并添加 **Player Dash** 组件
4. （可选）再次添加组件搜索 **Dash UI**（也可以后续单独设置）

### 参数配置
在 Inspector 中调整以下参数：

| 参数名 | 默认值 | 说明 |
|--------|--------|------|
| **Dash Distance** | 3 | 冲刺距离，单位是Unity世界单位 |
| **Dash Speed** | 15 | 冲刺速度，建议比普通移动快2-3倍 |
| **Cooldown Time** | 1.5 | 冷却时间（秒），建议1-2.5秒 |
| **Dash Key** | Space | 冲刺按键（默认空格键）|
| **Show Debug Info** | ☐ | 勾选后在Console中显示调试信息 |

### 推荐参数组合

| 风格 | Dash Distance | Dash Speed | Cooldown Time |
|------|---------------|------------|---------------|
| 轻度/频繁使用 | 2 | 10 | 1.0 |
| 标准/平衡 | 3 | 15 | 1.5 |
| 重度/策略性 | 5 | 20 | 2.5 |

---

## 步骤 2: 创建UI冷却进度条（可选但推荐）

### 方法 A: 简单进度条（推荐）

1. **创建 Canvas**（如不存在）
   - 右键 Hierarchy → UI → Canvas
   - 确保 Canvas 的 Render Mode 为 **Screen Space - Overlay**

2. **创建 Slider**
   - 选中 Canvas
   - 右键 → UI → Slider
   - 重命名为 "DashCooldownSlider"

3. **配置 Slider**
   - 在 Inspector 中找到 Slider 组件
   - 设置 **Direction**: Left To Right（水平）或 Bottom To Top（垂直）
   - 展开 Transition，设置为 **None**（可选）

4. **设置样式**（可选）
   - 展开 Slider → Handle Slide Area，**删除或禁用 Handle**（使其不显示）
   - 选择 Background 对象，调整颜色为深色（如深灰色）
   - 选择 Fill Area → Fill 对象，调整颜色为亮色（如青色/绿色）

5. **添加 DashUI 脚本**
   - 在 Slider 对象上点击 **Add Component**
   - 搜索并添加 **Dash UI**
   - 在 Inspector 中配置：
     - **Player Dash**: 拖拽玩家角色（会自动查找同场景中的PlayerDash）
     - **Cooldown Slider**: 保持为当前Slider（自动）

6. **UI自动定位**（可选）
   DashUI 默认会自动定位到屏幕左下角。如需手动调整：
   - **Auto Position**: ☐ 取消勾选可手动定位
   - **Offset**: X/Y偏移量（默认 20, 20）
   - **Bar Size**: 进度条尺寸（默认 150x15）

### 方法 B: 圆形/自定义进度条

如需圆形冷却效果：
1. 使用 **Radial 360** 类型的 Slider
2. 或使用 Image 组件配合 Sprite 的 Filled 模式
3. 编写自定义脚本或使用 DashUI 作为参考

---

## 步骤 3: 测试冲刺功能

### 快速测试
1. 点击 **Play** 按钮进入游戏模式
2. 使用 **WASD** 移动角色
3. 按住移动方向的同时，按 **空格键 (Space)**
4. 观察角色是否快速冲刺一段距离
5. 在冷却期间再次按空格，应无法触发

### 测试检查清单
- [ ] 空格键可触发冲刺
- [ ] 冲刺时角色快速移动
- [ ] 撞墙时停止（不穿透）
- [ ] 冷却期间无法再次冲刺
- [ ] 进度条正确显示冷却状态（位于屏幕左下角）
- [ ] WASD移动在冲刺期间被禁用

---

## 步骤 4: 调整与优化

### 手感调优
根据游戏体验调整参数：

**冲刺太短？**
- 增加 Dash Distance（如从3改为4）

**冲刺太慢？**
- 增加 Dash Speed（如从15改为18）
- 同时可能需要减少 Dash Distance 保持总时间

**冷却太频繁/太少？**
- 调整 Cooldown Time
- 太快 → 增加冷却时间
- 太慢 → 减少冷却时间

**无法穿透障碍？**
- 这是预期行为！冲刺撞墙会停止
- 如需穿墙，需要修改 PlayerDash.cs 的 OnCollisionEnter2D 方法

### 常见问题

**Q: 按空格没反应？**
A: 检查：
1. GameObject 是否有 Rigidbody2D 组件
2. PlayerDash 组件是否正确添加
3. 是否处于冷却中（看进度条或Console调试信息）
4. Input Manager 中 Jump 是否占用空格键（可能冲突）

**Q: 冲刺方向不对？**
A: 冲刺方向优先级：
1. WASD输入方向（优先）
2. 角色朝向（transform.right）
如果静止时冲刺，确保角色有朝向

**Q: 冲刺时还能WASD移动？**
A: 检查：
1. move_wasd.cs 是否已保存和编译
2. 控制台是否有错误信息
3. PlayerDash 和 move_wasd 是否在同一个 GameObject 上

**Q: 进度条不显示？**
A: 检查：
1. Slider 的 Fill Area 是否可见（颜色不是透明）
2. DashUI 脚本中的 Player Dash 引用是否正确
3. 是否在 Canvas 下创建

**Q: 进度条不在左下角？**
A: 检查：
1. DashUI 的 Auto Position 是否勾选
2. Canvas 的 Render Mode 是否为 Screen Space - Overlay
3. 尝试调整 Offset 参数

**Q: 游戏重启后CD还在？**
A: PlayerDash 已自动订阅 GameManager.OnGameRestart 事件，重启时会自动重置CD。如果无效，检查：
1. GameManager 是否正确存在于场景中
2. GameManager.OnGameRestart 是否正常触发
3. 控制台是否有错误信息

---

## 步骤 5: 进阶配置（可选）

### 添加冲刺音效
1. 在 PlayerDash 中添加 AudioSource 引用
2. 在 StartDash() 方法中播放音效

### 添加视觉特效
1. 在冲刺时启用 Trail Renderer（拖尾）
2. 添加 Particle System 粒子效果
3. 修改 PlayerDash.cs，在 StartDash() 和 EndDash() 中控制特效

### 多段冲刺
修改 PlayerDash.cs：
- 添加 `maxDashCharges` 属性
- 修改冷却逻辑为充能系统

### 修改冲刺按键
在 Inspector 中修改 **Dash Key**：
- 默认：Space（空格键）
- 可选：LeftShift、LeftControl、Q、E 等任意 KeyCode

---

## 架构说明

### 事件驱动设计
本实现采用**事件订阅模式**，实现组件解耦：

```
PlayerDash (发布者)
├── OnDashStart 事件 → move_wasd 订阅
├── OnDashEnd 事件 → move_wasd 订阅
└── OnDashStart/End 事件 → 其他组件可订阅

move_wasd (订阅者)
├── 订阅 OnDashStart → 禁用移动
└── 订阅 OnDashEnd → 恢复移动
```

**优势**：
- 组件间无需直接引用
- 易于扩展（其他组件也可订阅冲刺事件）
- 避免循环依赖

### 文件结构

```
Assets/
├── Scripts/
│   ├── PlayerDash.cs          ← 新创建：冲刺核心逻辑
│   ├── DashUI.cs              ← 新创建：UI组件
│   ├── move_wasd.cs           ← 已更新：支持冲刺状态检测
│   └── ...其他脚本
└── Scenes/
    └── SampleScene.unity
```

---

## 组件依赖关系

```
玩家角色 GameObject
├── Rigidbody2D (必须)
├── move_wasd.cs (可选，但推荐)
├── PlayerShooting.cs (可选)
└── PlayerDash.cs (新添加)
    └── 需要 Rigidbody2D

Canvas
└── DashCooldownSlider (Slider)
    └── DashUI.cs (新添加)
        └── 需要引用 PlayerDash
```

---

## 完成！

现在你已经成功配置了**空格键冲刺**功能！

按 **Play** 测试：
- **WASD** 移动
- **空格键** 冲刺
- 查看左下角进度条

---

## 附录：事件订阅示例

### 在其他组件中监听冲刺事件

```csharp
public class MyComponent : MonoBehaviour
{
    private PlayerDash playerDash;
    
    void Start()
    {
        playerDash = GetComponent<PlayerDash>();
        if (playerDash != null)
        {
            playerDash.OnDashStart += OnPlayerDashStart;
            playerDash.OnDashEnd += OnPlayerDashEnd;
        }
    }
    
    void OnDestroy()
    {
        if (playerDash != null)
        {
            playerDash.OnDashStart -= OnPlayerDashStart;
            playerDash.OnDashEnd -= OnPlayerDashEnd;
        }
    }
    
    void OnPlayerDashStart(bool isDashing)
    {
        Debug.Log("玩家开始冲刺！");
        // 在这里添加冲刺开始时的逻辑
    }
    
    void OnPlayerDashEnd(bool isDashing)
    {
        Debug.Log("玩家冲刺结束！");
        // 在这里添加冲刺结束时的逻辑
    }
}
```

### PlayerDash 公共API
```csharp
// 查询状态
bool isDashing = playerDash.IsDashing;
bool canDash = playerDash.CanDash;
bool isOnCooldown = playerDash.IsOnCooldown;

// 获取冷却进度（0-1）
float cdPercent = playerDash.GetCooldownPercent();

// 获取剩余冷却时间
float cdRemaining = playerDash.GetCooldownRemaining();

// 强制重置（如游戏重启）
playerDash.ResetDash();
```

