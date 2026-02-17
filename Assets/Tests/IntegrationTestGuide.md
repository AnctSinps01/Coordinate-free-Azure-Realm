# 集成测试指南

## 测试环境准备

### 1. 场景配置检查清单

- [ ] **Player 物体**
  - 挂载 `PlayerHealth` 脚本
  - 挂载 `PlayerStats` 脚本
  - 挂载 `move_wasd` 脚本
  - 挂载 `PlayerDash` 脚本（如有）
  - 挂载 `PlayerShooting` 脚本（如有）
  - Tag 设置为 "Player"

- [ ] **GameManager 物体**
  - 挂载 `GameManager` 脚本
  - Upgrade Panel 字段：拖入 UpgradePanel 预制体
  - Upgrade Tree UI 字段：拖入 UpgradeTreeUI 组件
  - Player Health 字段：拖入 Player
  - Map Generator 字段：（配置地图生成器）
  - Enemy Spawner 字段：（配置敌人生成器）

- [ ] **GameUI 物体**
  - 挂载 `GameUI` 脚本
  - Health Text 字段：拖入血量文本
  - Experience Text 字段：拖入经验值文本（新建）
  - Player Health 字段：拖入 Player

- [ ] **独立 GameObject（空物体）**
  - 挂载 `ExperienceSystem` 脚本
  - 挂载 `UpgradeManager` 脚本
    - All Upgrades 数组：拖入 MoveSpeedUpgrade.asset
  - 挂载 `SaveManager` 脚本

- [ ] **敌人预制体**
  - 挂载 `EnemyController` 脚本
  - XP Value 字段：设置为 1

### 2. UI 元素检查

- [ ] **GameUI Canvas**
  - Health Text（左上角）
  - Experience Text（右上角，格式："XP: {0}")

- [ ] **UpgradePanel Canvas**（初始隐藏）
  - 背景面板（半透明黑色）
  - 标题文本（"选择天赋"）
  - 经验值显示文本
  - 节点容器（4×3 网格）
  - 连线容器
  - 继续按钮（"按空格继续" 或 "继续游戏"）

### 3. 资源配置检查

- [ ] **MoveSpeedUpgrade.asset**
  - 路径：`Assets/Resources/Upgrades/MoveSpeedUpgrade.asset`
  - Upgrade Id: "movespeed"
  - Max Level: 3
  - Level Values: [1.5, 2.25, 3.375]
  - Level Costs: [4, 10, 25]
  - Grid Position: (0, 0)

---

## 测试用例

### 测试 1: 经验值收集

**步骤**:
1. 运行游戏
2. 观察 GameUI，确认显示 "XP: 0"
3. 杀死 1 个敌人
4. 观察 GameUI，确认显示 "XP: 1"
5. 杀死 5 个敌人
6. 观察 GameUI，确认显示 "XP: 6"

**预期结果**: 经验值实时更新

---

### 测试 2: 死亡触发升级界面

**步骤**:
1. 运行游戏，获得一些经验值（如 10 XP）
2. 让敌人杀死玩家
3. 观察：
   - 游戏暂停（Time.timeScale = 0）
   - UpgradePanel 显示
   - 面板显示当前经验值 "经验值: 10"
   - 移速天赋节点显示为"可购买"状态（绿色）

**预期结果**: 死亡后自动打开升级面板

---

### 测试 3: 购买天赋

**步骤**:
1. 在升级界面，点击移速天赋节点
2. 观察：
   - 经验值从 10 减少到 6（消耗 4 XP）
   - 节点显示等级 1/3
   - 显示"已购买"状态（黄色）
3. 再次点击（尝试购买 2 级）
4. 观察：无法购买（经验值 6 < 价格 10）

**预期结果**: 经验值正确消耗，等级正确更新

---

### 测试 4: 继续游戏

**步骤**:
1. 购买 1 级移速后，点击"继续"按钮
2. 观察：
   - UpgradePanel 隐藏
   - 游戏恢复（Time.timeScale = 1）
   - 新一局开始（地图重置、敌人重置、玩家满血）
3. 移动玩家

**预期结果**: 玩家移速明显提升（5 → 7.5）

---

### 测试 5: 移速升级效果

**步骤**:
1. 新一局开始
2. 按住 WASD 移动，感受速度
3. 记录移速感觉
4. 死亡，购买 1 级移速
5. 继续游戏
6. 再次移动，感受速度
7. 重复直到 3 级

**预期结果**:
- 0 级：基础速度 5
- 1 级：速度 7.5（明显变快）
- 2 级：速度 11.25（很快）
- 3 级：速度 16.875（非常快）

---

### 测试 6: 数据持久化

**步骤**:
1. 运行游戏
2. 获得 20 XP
3. 死亡，购买 2 级移速（消耗 4+10=14 XP，剩余 6 XP）
4. 继续游戏，确认移速已提升
5. **关闭游戏**
6. 重新打开游戏
7. 观察：
   - GameUI 显示 "XP: 6"
   - 移速为 11.25（2 级效果）

**预期结果**: 经验值和天赋等级正确保存和加载

---

### 测试 7: 射击降速兼容性

**步骤**:
1. 购买移速升级（如 2 级，速度 11.25）
2. 按住射击键移动
3. 观察速度变化

**预期结果**: 射击时速度为 11.25 × 0.3 = 3.375，射击结束后恢复正常

---

### 测试 8: 冲刺兼容性

**步骤**:
1. 购买移速升级
2. 使用冲刺技能
3. 观察冲刺速度和效果

**预期结果**: 冲刺功能正常工作，不受移速升级影响（或按设计叠加）

---

## 常见问题排查

### Q1: 经验值不增加
**检查**:
- EnemyController 是否有 XP Value 字段？
- ExperienceSystem 是否订阅了 OnEnemyDied 事件？
- 敌人死亡时是否触发了 Die() 方法？

### Q2: 升级界面不打开
**检查**:
- GameManager 是否订阅了 PlayerHealth.OnDied？
- Upgrade Panel 字段是否已赋值？
- Player 是否有 PlayerHealth 组件？

### Q3: 移速升级不生效
**检查**:
- move_wasd 是否订阅了 PlayerStats.OnStatChanged？
- PlayerStats 是否正确应用了修饰器？
- FixedUpdate 是否使用了 GetCurrentMoveSpeed()？

### Q4: 数据不保存
**检查**:
- SaveManager 是否为单例且 DontDestroyOnLoad？
- 购买天赋时是否调用了 SaveManager.SetUpgradeLevel()？
- 增加经验值时是否调用了 SaveManager.AddExperience()？
- 存档文件路径是否正确？

### Q5: UI 不更新
**检查**:
- GameUI 是否订阅了 ExperienceSystem.OnExperienceChanged？
- TextMeshProUGUI 字段是否已赋值？
- 事件是否正确触发？

---

## 性能检查

- [ ] 升级面板打开时间 < 100ms
- [ ] 保存/加载时间 < 50ms
- [ ] 游戏帧率稳定（60 FPS）
- [ ] 内存占用增加 < 10MB

---

## 提交检查清单

- [ ] 所有脚本无编译错误
- [ ] 控制台无红色错误
- [ ] 所有功能按预期工作
- [ ] 数据持久化正常
- [ ] 代码注释完整
- [ ] 场景配置已保存

---

## 完成标准

✅ 杀怪获得经验值（直接+1，GameUI实时显示）  
✅ 只有死亡后才能打开升级面板  
✅ 技能树正确显示天赋（网格布局）  
✅ 用经验值购买天赋等级（消耗货币）  
✅ 移速升级3级，每级×1.5倍  
✅ 价格公式正确：4×2.5^级别  
✅ 数据正确保存到JSON文件  
✅ 重启游戏后进度正确加载  
✅ 控制台无错误/警告  
✅ 现有功能（移动、射击、冲刺）不受影响  
