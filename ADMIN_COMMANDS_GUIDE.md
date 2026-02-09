# 🔐 管理员命令完全指南

## 📋 概述

管理员命令是游戏中最高权限的操作集合，用于游戏管理、调试和维护。所有管理员命令都需要账号具有以下权限之一：

- **Admin**：永久管理员权限（存储在数据库）
- **TempAdmin**：临时管理员权限（会话级别）

---

## 🔑 权限体系

### 权限设置

**文件：** `ServerLibrary/DBModels/AccountInfo.cs`

```csharp
// 永久管理员标志（数据库字段）
public bool Admin { get; set; }

// 临时管理员标志（会话级别）
public bool TempAdmin;
```

### 权限判定

**文件：** `ServerLibrary/Envir/Commands/AdminCommandHandler.cs`

```csharp
public class AdminCommandHandler : AbstractCommandHandler<IAdminCommand>
{
    public override bool IsAllowedByPlayer(PlayerObject player)
    {
        // 检查账号是否具有 TempAdmin 权限
        return player.Character.Account.TempAdmin;
    }
}
```

### 权限判定方法

```csharp
// 在 AccountInfo 中定义的权限判定方法
public bool IsAdmin(bool includeTemp = false)
{
    return Admin || (includeTemp && TempAdmin);
}
```

---

## 🎮 完整命令列表

### ✨ 通用管理命令

#### 1. **GAMEMASTER** - 游戏大师模式
```
命令：/gamemaster
参数：无
描述：切换游戏大师模式
效果：
  - 激活：玩家无法被其他玩家或怪物锁定/攻击
  - 停用：恢复正常的可攻击状态
备注：一般用于 GM 测试游戏内容
```

#### 2. **OBSERVER** - 观察者模式
```
命令：/observer
参数：无
描述：切换观察者模式
效果：
  - 激活：玩家隐身，其他玩家和怪物看不见你
  - 停用：恢复可见状态
  - 自动刷新周围对象视图
备注：用于隐身巡察游戏
```

#### 3. **SUPERMAN** - 超人模式
```
命令：/superman
参数：无
描述：切换超人模式
效果：
  - 激活：玩家不能受伤，不消耗法力值
  - 停用：恢复正常的伤害和消耗
备注：用于无敌测试
```

---

### 👤 角色属性命令

#### 4. **LEVEL** - 设置等级
```
命令：/level [等级]
或：  /level [角色名] [等级]
参数：
  - 等级：整数，必须 >= 0
  - 角色名：可选，指定其他玩家
描述：设置角色等级
示例：
  /level 50              # 设置自己的等级为 50
  /level PlayerName 99   # 设置 PlayerName 的等级为 99
备注：激活 LevelUp() 方法同步属性
```

#### 5. **ADDSTAT** - 增加装备属性
```
命令：/addstat [装备位置] [属性类型] [数值]
参数：
  - 装备位置：Weapon, Head, Body, Hands, Feet, Armour...
  - 属性类型：MaxDC, MaxMC, MaxSC, AC, MAC, HP, MP...
  - 数值：整数，增加的属性值
描述：为装备添加额外属性
示例：
  /addstat Weapon MaxDC 50      # 武器增加 50 最大物攻
  /addstat Armour AC 10         # 盔甲增加 10 物防
备注：修改装备的当前属性，需要装备已穿戴
```

#### 6. **LEVELSKILL** - 设置技能等级
```
命令：/levelskill [角色名] [技能ID] [等级]
参数：
  - 角色名：目标玩家名称
  - 技能ID：技能的 ID 号
  - 等级：技能等级
描述：设置目标玩家的技能等级
示例：
  /levelskill PlayerName 1 30    # 设置 PlayerName 的技能 1 等级为 30
备注：直接修改技能等级，无需升级过程
```

---

### 💰 物品与货币命令

#### 7. **MAKE** - 创建物品
```
命令：/make [物品名称] [数量]
参数：
  - 物品名称：物品的 ItemInfo 名称
  - 数量：创建数量，默认为 1
描述：创建物品并放入背包
示例：
  /make Gold 1000000            # 创建 100 万金币
  /make HealthPotion 100        # 创建 100 瓶血药
效果：
  - 普通物品：堆叠到背包（受背包限制）
  - 货币物品：直接加到货币余额
备注：创建的物品标记为 GameMaster 标志
```

#### 8. **GIVEGAMEGOLD** - 给予游戏币
```
命令：/givegamegold [角色名] [数量]
参数：
  - 角色名：目标玩家名称
  - 数量：给予数量
描述：给指定玩家增加游戏币
示例：
  /givegamegold PlayerName 1000000  # 给 PlayerName 1 百万游戏币
备注：直接修改账户游戏币余额
```

#### 9. **TAKEGAMEGOLD** - 扣除游戏币
```
命令：/takegamegold [角色名] [数量]
参数：
  - 角色名：目标玩家名称
  - 数量：扣除数量
描述：从指定玩家扣除游戏币
示例：
  /takegamegold PlayerName 500000  # 扣除 PlayerName 50 万游戏币
备注：数量不能超过玩家当前余额
```

---

### 🐴 宠物与伙伴命令

#### 10. **GIVEHORSE** - 给予坐骑
```
命令：/givehorse [角色名] [坐骑类型]
参数：
  - 角色名：目标玩家名称
  - 坐骑类型：Normal, Undead, FireUnicorn, IceUnicorn 等
描述：给玩家增加坐骑
示例：
  /givehorse PlayerName Normal      # 给 PlayerName 普通马
  /givehorse PlayerName FireUnicorn # 给 PlayerName 火独角兽
备注：添加坐骑到玩家的坐骑列表
```

#### 11. **SETCOMPANIONLEVEL** - 设置伙伴等级
```
命令：/setcompanionlevel [伙伴等级]
参数：
  - 伙伴等级：伙伴的等级值
描述：设置当前玩家伙伴的等级
示例：
  /setcompanionlevel 50            # 设置伙伴等级为 50
备注：设置伙伴的基础等级
```

#### 12. **SETCOMPANIONSTAT** - 设置伙伴属性
```
命令：/setcompanionstat [等级] [属性] [数值]
参数：
  - 等级：伙伴等级阶段（1, 5, 7, 10, 11, 13, 15）
  - 属性：属性类型（MaxDC, AC, HP 等）
  - 数值：属性数值
描述：为伙伴的特定等级阶段设置属性
示例：
  /setcompanionstat 5 MaxDC 100    # 设置等级 5 时的最大物攻为 100
备注：伙伴在不同等级有不同的属性
```

#### 13. **SETHERMITSTAT** - 设置隐士属性
```
命令：/sethermitstat [属性] [数值]
参数：
  - 属性：隐士属性类型
  - 数值：属性数值
描述：设置玩家隐士（宠物）的属性
示例：
  /sethermitstat MaxDC 80          # 设置隐士最大物攻为 80
备注：仅适用于拥有隐士的玩家
```

---

### 🎓 技能与学习命令

#### 14. **GIVESKILLS** - 给予所有技能
```
命令：/giveskills [角色名]
参数：
  - 角色名：目标玩家名称
描述：给玩家学习所有职业技能
示例：
  /giveskills PlayerName           # 给 PlayerName 学习所有技能
备注：直接学习所有可用技能，无需等级要求
```

#### 15. **RESETDISCIPLINE** - 重置修复
```
命令：/resetdiscipline [角色名]
参数：
  - 角色名：目标玩家名称
描述：重置玩家的修复（Discipline）状态
示例：
  /resetdiscipline PlayerName      # 重置 PlayerName 的修复
备注：清除修复相关的限制或数据
```

---

### 🗺️ 地图与移动命令

#### 16. **MOVE** - 移动到地图
```
命令：/move [地图名称]
或：  /move [X坐标] [Y坐标]
参数：
  - 地图名称：地图的名称
  - X坐标：X 坐标
  - Y坐标：Y 坐标
描述：传送玩家到指定地图或坐标
示例：
  /move Lodeight                   # 传送到比奇城
  /move Lodeight 100 100           # 传送到比奇城 (100, 100)
备注：完全瞬间传送，无距离限制
```

#### 17. **GOTO** - 移动到玩家
```
命令：/goto [角色名]
参数：
  - 角色名：目标玩家名称
描述：传送到指定玩家的位置
示例：
  /goto PlayerName                 # 传送到 PlayerName 所在位置
备注：会同时跟随目标玩家的地图和坐标
```

#### 18. **RECALL** - 召回玩家
```
命令：/recall [角色名]
参数：
  - 角色名：目标玩家名称
描述：将指定玩家召回到自己的位置
示例：
  /recall PlayerName               # 召回 PlayerName 到你的位置
备注：强制传送目标玩家
```

---

### 🔒  封禁与管制命令

#### 19. **BAN** - 封禁账户
```
命令：/ban [角色名] [分钟数]
参数：
  - 角色名：目标玩家名称
  - 分钟数：封禁时长（默认 365 天 = 525600 分钟）
描述：封禁玩家账户
示例：
  /ban PlayerName                  # 永久封禁 PlayerName
  /ban PlayerName 1440             # 封禁 PlayerName 1 天（1440 分钟）
效果：
  - 账户被标记为已封禁
  - 设置封禁原因和到期时间
  - 若玩家在线，立即断开连接
备注：可以通过修改 BanExpiry 解除
```

#### 20. **CHATBAN** - 禁言玩家
```
命令：/chatban [角色名] [分钟数]
参数：
  - 角色名：目标玩家名称
  - 分钟数：禁言时长（默认 365 天）
描述：禁止玩家使用聊天功能
示例：
  /chatban PlayerName              # 永久禁言 PlayerName
  /chatban PlayerName 60           # 禁言 PlayerName 1 小时
备注：玩家仍可以正常游戏，只是无法聊天
```

#### 21. **GLOBALBAN** - 全局禁言
```
命令：/globalban [角色名] [分钟数]
参数：
  - 角色名：目标玩家名称
  - 分钟数：禁言时长（默认 365 天）
描述：禁止玩家使用全局喊话（GlobalShout）
示例：
  /globalban PlayerName            # 禁止 PlayerName 使用全局喊话
备注：仅限制全局频道，不影响普通聊天
```

#### 22. **KICK** - 踢出玩家
```
命令：/kick [角色名]
参数：
  - 角色名：目标玩家名称
描述：断开玩家连接
示例：
  /kick PlayerName                 # 踢出 PlayerName
效果：立即断开目标玩家的连接
备注：不能踢出自己
```

#### 23. **REMOVEPKPOINTS** - 移除 PK 值
```
命令：/removepkpoints [角色名]
参数：
  - 角色名：目标玩家名称
描述：清除玩家的 PK 点数
示例：
  /removepkpoints PlayerName       # 清除 PlayerName 的 PK 值
备注：PK 值清零，不再显示红名
```

#### 24. **REMOVECAPTION** - 移除称号
```
命令：/removecaption [角色名]
参数：
  - 角色名：目标玩家名称
描述：移除玩家的游戏称号
示例：
  /removecaption PlayerName        # 移除 PlayerName 的称号
备注：清除玩家头上的自定义标题
```

---

### 🏰 城堡命令

#### 25. **TAKECASTLE** - 占领城堡
```
命令：/takecastle [城堡名称]
参数：
  - 城堡名称：城堡的名称
描述：强制占领指定城堡
示例：
  /takecastle WhiteIsland          # 占领白岛城堡
效果：
  - 城堡所有权变更
  - 触发城堡占领事件
备注：绕过正常的公会战系统
```

#### 26. **STARTCONQUEST** - 开始征服
```
命令：/startconquest
参数：无
描述：开始全图城堡征服事件
示例：
  /startconquest                   # 启动征服模式
备注：可能需要特定的游戏模式
```

#### 27. **ENDCONQUEST** - 结束征服
```
命令：/endconquest
参数：无
描述：结束城堡征服事件
示例：
  /endconquest                     # 停止征服模式
备注：恢复正常游戏模式
```

---

### 🐉 怪物与生成命令

#### 28. **MONSTER** - 生成怪物
```
命令：/monster [怪物名称] [数量]
参数：
  - 怪物名称：怪物的 MonsterInfo 名称
  - 数量：生成数量（默认为 1）
描述：在玩家前方生成怪物
示例：
  /monster Ant                     # 生成 1 只蚂蚁
  /monster Scorpion 10             # 生成 10 只蝎子
效果：
  - 怪物生成在玩家前方（根据玩家方向）
  - 可以生成多个相同的怪物
备注：用于控制怪物刷新或测试战斗
```

---

### 💎 其他管理命令

#### 29. **CREATEGUILD** - 创建公会
```
命令：/createguild [公会名称] [公会长名称]
参数：
  - 公会名称：新公会的名称
  - 公会长名称：公会长的角色名
描述：创建新的公会
示例：
  /createguild MyGuild PlayerName  # 创建名为 MyGuild 的公会，公会长为 PlayerName
效果：
  - 创建公会信息对象
  - 创建公会长角色
  - 初始化公会参数
备注：直接创建，绕过正常的公会申请流程
```

#### 30. **PROMOTEFAME** - 提升声誉
```
命令：/promotefame [角色名]
参数：
  - 角色名：目标玩家名称
描述：提升玩家的声誉等级
示例：
  /promotefame PlayerName          # 提升 PlayerName 的声誉
备注：与公会声誉系统相关
```

#### 31. **REBOOT** - 重启检查
```
命令：/reboot
参数：无
描述：执行系统重启检查和市场清理
示例：
  /reboot                          # 执行重启流程
效果：
  - 取消所有市场交易
  - 计算执行时间
备注：用于性能监测和数据清理
```

#### 32. **GCOLLECT** - 垃圾回收
```
命令：/gcollect
参数：无
描述：执行 .NET 垃圾回收
示例：
  /gcollect                        # 强制垃圾回收
效果：
  - 触发 GC.Collect()
  - 释放不用的内存
备注：用于内存优化和性能调试
```

#### 33. **CLEARIPBLOCKS** - 清除 IP 封禁
```
命令：/clearipblocks
参数：无
描述：清除所有 IP 地址封禁
示例：
  /clearipblocks                   # 清除所有 IP 封禁
效果：
  - 移除 IP 黑名单
  - 允许之前被 IP 封禁的玩家重新登录
备注：谨慎使用，可能导致安全问题
```

---

## 📊 命令分类汇总

| 类别 | 命令数 | 命令列表 |
|------|--------|---------|
| 🎮 **模式** | 3 | GAMEMASTER, OBSERVER, SUPERMAN |
| 👤 **角色** | 3 | LEVEL, ADDSTAT, LEVELSKILL |
| 💰 **物品** | 3 | MAKE, GIVEGAMEGOLD, TAKEGAMEGOLD |
| 🐴 **宠物** | 4 | GIVEHORSE, SETCOMPANIONLEVEL, SETCOMPANIONSTAT, SETHERMITSTAT |
| 🎓 **技能** | 2 | GIVESKILLS, RESETDISCIPLINE |
| 🗺️ **移动** | 3 | MOVE, GOTO, RECALL |
| 🔒 **管制** | 6 | BAN, CHATBAN, GLOBALBAN, KICK, REMOVEPKPOINTS, REMOVECAPTION |
| 🏰 **城堡** | 3 | TAKECASTLE, STARTCONQUEST, ENDCONQUEST |
| 🐉 **生成** | 1 | MONSTER |
| 💎 **其他** | 5 | CREATEGUILD, PROMOTEFAME, REBOOT, GCOLLECT, CLEARIPBLOCKS |

---

## 🔧 命令系统架构

### 命令执行流程

```
【玩家发送命令】
    ↓
【CommandParser 解析】
    ↓
【判断命令类型】(Admin/Player/GM)
    ↓
【检查权限】
    ├─ AdminCommandHandler.IsAllowedByPlayer()
    ├─ 检查 TempAdmin 或 Admin 标志
    └─ 权限检查失败 → 拒绝执行
    ↓
【调用命令的 Action() 方法】
    ↓
【参数验证】
    ├─ AbstractParameterizedCommand：检查参数数量
    └─ 参数检查失败 → 抛出异常
    ↓
【执行业务逻辑】
    ├─ 修改对象属性
    ├─ 调用相关方法
    └─ 返回执行结果
    ↓
【反馈消息】
    └─ player.Connection.ReceiveChat() 返回结果
```

### 命令类型

#### AbstractCommand<IAdminCommand>
无参数命令，直接执行：
- GAMEMASTER, OBSERVER, SUPERMAN
- REBOOT, GCOLLECT, CLEARIPBLOCKS

#### AbstractParameterizedCommand<IAdminCommand>
有参数命令，需要参数：
- 大多数命令都属于此类

### 异常处理

```csharp
// 参数数量不足
ThrowNewInvalidParametersException()

// 自定义异常信息
throw new UserCommandException("error message")
```

---

## ✅ 使用指南

### 权限设置方式

#### 1. 数据库设置（永久权限）
```sql
UPDATE AccountInfo SET Admin = 1 WHERE EMailAddress = 'admin@example.com'
```

#### 2. 运行时设置（临时权限）
```csharp
// 在代码中设置
accountInfo.TempAdmin = true;  // 激活临时管理员权限
```

#### 3. 移除权限
```csharp
accountInfo.TempAdmin = false;  // 关闭临时管理员权限
accountInfo.Admin = false;      // 删除永久权限（需要数据库操作）
```

### 命令执行示例

```
/level 50                       # 设置自己等级为 50
/make Gold 1000000              # 创建 100 万金币
/goto PlayerName                # 传送到 PlayerName
/ban HackerName 10080           # 封禁黑客 7 天
/monster Boss 5                 # 生成 5 个 Boss 进行测试
/Superman                       # 激活无敌模式进行 GM 测试
```

---

## ⚠️ 注意事项

1. **权限管理**
   - TempAdmin 权限是会话级别，重新登录后失效
   - Admin 权限是永久的，存储在数据库
   - 使用 `IsAdmin(includeTemp: true)` 判定时两者都有效

2. **命令安全**
   - 某些命令（如 BAN、CLEARIPBLOCKS）可能产生严重后果
   - 建议只给予信任的操作员管理员权限
   - 所有管理员操作应该被记录

3. **游戏平衡**
   - MAKE 命令可能导致经济崩溃
   - GIVESKILLS 可能破坏玩家进度
   - SUPERMAN 仅用于测试，不应对普通玩家开放

4. **性能考虑**
   - MONSTER 批量生成可能影响服务器性能
   - GCOLLECT 应在离峰时段使用
   - OBSERVER 和 GAMEMASTER 不消耗性能，可安全使用

---

## 📁 相关文件

| 文件 | 说明 |
|------|------|
| `ServerLibrary/Envir/Commands/Command/Admin/*.cs` | 所有管理员命令实现 |
| `ServerLibrary/Envir/Commands/AdminCommandHandler.cs` | 管理员命令权限检查 |
| `ServerLibrary/DBModels/AccountInfo.cs` | 账户权限字段定义 |
| `ServerLibrary/Models/PlayerObject.cs` | 玩家对象和命令执行 |

---

## 🎯 总结

- ✅ **33 个管理员命令**，覆盖角色、物品、宠物、地图、城堡等全面功能
- ✅ **权限体系**：Admin（永久）和 TempAdmin（临时）两级权限
- ✅ **安全机制**：通过 AdminCommandHandler 严格检查权限
- ✅ **易于扩展**：新命令可继承 AbstractCommand 或 AbstractParameterizedCommand
- ✅ **日志记录**：所有关键操作都有记录（BAN、KICK 等）
