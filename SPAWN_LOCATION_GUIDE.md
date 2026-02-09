# 角色出生地配置指南

## 📍 概述

角色创建后的出生地（复活点）是通过 **SafeZoneInfo** 数据库配置系统来实现的。出生地不是在创建角色时确定的，而是在第一次登录游戏时根据职业动态选择的。

---

## 🏗️ 数据库结构

### SafeZoneInfo 类（出生地信息）

**位置：** `LibraryCore/SystemModels/SafeZoneInfo.cs`

```csharp
public sealed class SafeZoneInfo : DBObject
{
    // 【出生区域】安全区所在的地图区域
    [IsIdentity]
    public MapRegion Region { get; set; }
    
    // 【复活点】角色死亡后复活的地点
    // 通常与 Region 相同，或指向不同的地图
    public MapRegion BindRegion { get; set; }
    
    // 【职业限制】该出生地允许的职业
    // 使用位掩码：Warrior | Wizard | Taoist | Assassin
    public RequiredClass StartClass { get; set; }
    
    // 【PVP 区域】是否为红名 PVP 区
    public bool RedZone { get; set; }
    
    // 【边界】是否为地图边界区域
    public bool Border { get; set; }
    
    // 【有效出生点列表】具体的出生坐标集合
    public List<Point> ValidBindPoints = new List<Point>();
}
```

---

## 🔄 角色出生流程

### 1. 角色创建阶段（NewCharacter）

**文件：** `ServerLibrary/Envir/SEnvir.cs` → `NewCharacter()` 方法

```csharp
// 创建新角色
CharacterInfo cInfo = CharacterInfoList.CreateNewObject();

cInfo.CharacterName = p.CharacterName;
cInfo.Class = p.Class;           // 保存职业信息
cInfo.Gender = p.Gender;
cInfo.HairType = p.HairType;
// ... 其他属性 ...

// 此时角色还没有绑定出生地
// BindPoint 字段为 null
```

**关键点：** 
- 此阶段只是创建角色数据
- 出生地（BindPoint）仍为空

---

### 2. 游戏启动阶段（StartGame）

**文件：** `ServerLibrary/Models/PlayerObject.cs` → `StartGame()` 方法

```csharp
public void StartGame()
{
    // 【第一步】设置出生点
    if (!SetBindPoint())  // ← 核心方法
    {
        Enqueue(new S.StartGame { Result = StartGameResult.UnableToSpawn });
        return;
    }
    
    // 【第二步】生成角色到游戏世界
    if (Character.CurrentInstance != null)
    {
        // 副本中的重新连接处理
    }
    else if (!Spawn(Character.CurrentMap, null, 0, CurrentLocation) && 
             !Spawn(Character.BindPoint.BindRegion, null, 0))
    {
        // 生成失败处理
    }
}
```

---

### 3. 出生地选择逻辑（SetBindPoint）

**文件：** `ServerLibrary/Models/PlayerObject.cs` → `SetBindPoint()` 方法

```csharp
private bool SetBindPoint()
{
    // ✅ 如果已经有有效的出生点，就使用现有的
    if (Character.BindPoint != null && Character.BindPoint.ValidBindPoints.Count > 0)
        return true;
    
    // 📋 收集所有有效的出生地
    List<SafeZoneInfo> spawnPoints = new List<SafeZoneInfo>();
    
    // 📌 遍历所有 SafeZoneInfo 记录
    foreach (SafeZoneInfo info in SEnvir.SafeZoneInfoList.Binding)
    {
        // 🚫 跳过没有有效出生点的区域
        if (info.ValidBindPoints.Count == 0) continue;
        
        // 👤 【职业过滤】只选择当前职业允许的出生地
        switch (Class)
        {
            case MirClass.Warrior:
                // 检查该出生地是否支持战士职业
                if ((info.StartClass & RequiredClass.Warrior) != RequiredClass.Warrior) 
                    continue;  // 不支持，跳过
                break;
            
            case MirClass.Wizard:
                // 检查该出生地是否支持魔法师职业
                if ((info.StartClass & RequiredClass.Wizard) != RequiredClass.Wizard) 
                    continue;
                break;
            
            case MirClass.Taoist:
                // 检查该出生地是否支持道士职业
                if ((info.StartClass & RequiredClass.Taoist) != RequiredClass.Taoist) 
                    continue;
                break;
            
            case MirClass.Assassin:
                // 检查该出生地是否支持刺客职业
                if ((info.StartClass & RequiredClass.Assassin) != RequiredClass.Assassin) 
                    continue;
                break;
        }
        
        // ✅ 该出生地符合条件，添加到列表
        spawnPoints.Add(info);
    }
    
    // 🎯 【随机选择】从符合条件的出生地中随机选择一个
    if (spawnPoints.Count > 0)
        Character.BindPoint = spawnPoints[SEnvir.Random.Next(spawnPoints.Count)];
    
    // ⚠️ 如果没有找到任何出生地，返回失败
    return Character.BindPoint != null;
}
```

---

## 📊 职业限制（RequiredClass）

出生地可以限制哪些职业可以使用：

```csharp
[Flags]
public enum RequiredClass : byte
{
    None = 0,
    Warrior = 1,    // 战士
    Wizard = 2,     // 魔法师
    Taoist = 4,     // 道士
    Assassin = 8,   // 刺客
    All = 15        // 所有职业
}
```

**位掩码示例：**
- `RequiredClass.Warrior` = 仅战士
- `RequiredClass.Warrior | RequiredClass.Wizard` = 战士和魔法师
- `RequiredClass.All` = 所有职业

---

## 🎲 出生点选择过程

```
【数据库中的 SafeZoneInfo 列表】
    ↓
【遍历每个 SafeZoneInfo】
    ↓
【检查条件】
    ├─ ValidBindPoints.Count > 0？
    └─ StartClass 包含当前职业？
    ↓
【符合条件的出生地加入候选列表】
    ↓
【从候选列表中随机选择一个】
    ↓
【设置 Character.BindPoint】
    ↓
【该出生地的 BindRegion 就是角色第一次登录的出生地】
```

---

## 💾 具体配置步骤

### 在编辑器/数据库中添加出生地：

1. **创建 SafeZoneInfo 记录**
   - `Region`：选择一个安全区（如：比奇城）
   - `BindRegion`：选择复活点所在的地图（如：比奇城）
   - `StartClass`：设置允许的职业（如：`RequiredClass.All`）
   - `RedZone`：是否为 PVP 区（通常为 false）
   - `Border`：是否为地图边界（通常为 false）

2. **添加出生坐标**
   - 在 `ValidBindPoints` 集合中添加具体的 Point(X, Y)
   - 例如：`new Point(100, 100)` 表示坐标 (100, 100)
   - 可以添加多个点，角色复活时会随机选择

### 示例配置：

```
SafeZoneInfo 记录 1：
  Region = "Lodeight"（比奇城区域）
  BindRegion = "Lodeight"（比奇城作为复活点）
  StartClass = RequiredClass.All
  ValidBindPoints = 
    [
      Point(100, 100),
      Point(110, 100),
      Point(120, 100)
    ]

SafeZoneInfo 记录 2：
  Region = "PVP_Area"（PVP 区域）
  BindRegion = "Lodeight"（PVP 战死后回到比奇城）
  StartClass = RequiredClass.All
  RedZone = true
  ValidBindPoints = 
    [
      Point(200, 200)
    ]
```

---

## 🔗 相关数据模型

### CharacterInfo（角色信息）

**字段：** `BindPoint`（SafeZoneInfo 类型）
- 存储当前角色的出生点信息
- 初始为 null，在 `StartGame()` 时由 `SetBindPoint()` 设置
- 后续可通过回城符、传送等方式更改

### MapRegion（地图区域）

定义一个地图上的矩形区域，通常用于：
- 定义安全区范围
- 定义副本入口
- 定义 NPC 交互区

---

## ⚙️ 运行流程总结

```
【新角色创建】
  ↓
【角色保存到数据库】
  ↓
【玩家首次登录】
  ↓
【StartGame() 被调用】
  ↓
【SetBindPoint() 检查 BindPoint 是否存在】
  ├─ 存在 → 使用现有出生点
  └─ 不存在 → 搜索符合条件的 SafeZoneInfo
      ↓
      【遍历所有 SafeZoneInfo】
      ↓
      【职业过滤 - 只保留当前职业允许的出生地】
      ↓
      【随机选择一个出生地】
      ↓
      【设置 Character.BindPoint】
      ↓
【角色生成到出生地】
  ↓
【玩家可以开始游戏】
```

---

## 🔍 关键代码位置

| 功能 | 文件 | 方法 |
|------|------|------|
| 角色创建 | ServerLibrary/Envir/SEnvir.cs | `NewCharacter()` |
| 游戏启动 | ServerLibrary/Models/PlayerObject.cs | `StartGame()` |
| **出生地选择** | **ServerLibrary/Models/PlayerObject.cs** | **`SetBindPoint()`** |
| 复活处理 | ServerLibrary/Models/PlayerObject.cs | `TownRevive()` |
| 出生地数据模型 | LibraryCore/SystemModels/SafeZoneInfo.cs | `SafeZoneInfo` |

---

## 📝 注意事项

1. **首次登录才设置：** 出生地是在首次 `StartGame()` 时设置的，创建角色时不设置

2. **职业固定的出生地：** 可以为不同职业设置不同的出生地（如戏人沙漠仅限刺客）

3. **随机出生点：** 如果一个 SafeZoneInfo 有多个 ValidBindPoints，角色和复活时都会随机选择

4. **出生地变更：** 通过回城符、复活等操作可以改变 `Character.BindPoint` 到新的 SafeZoneInfo

5. **复活机制：** 角色死亡后调用 `TownRevive()` 方法，会从当前 BindPoint 的 ValidBindPoints 中随机选择一个点复活

---

## 💡 自定义出生地的方法

1. **编辑器方式（推荐）**
   - 在 `Server/Views/SafeZoneInfoView` 中编辑
   - 可视化管理所有出生地

2. **数据库方式**
   - 直接修改数据库 SafeZoneInfo 表
   - 添加新的记录或修改现有记录
   - 需要重启服务器生效

3. **代码方式**
   - 在 `SEnvir.LoadDatabase()` 后手动添加 SafeZoneInfo
   - 在开发/测试中使用

---

## 🎯 总结

- ✅ **出生地配置：** 通过 `SafeZoneInfo` 数据库表
- ✅ **职业区分：** 使用 `RequiredClass` 位掩码
- ✅ **随机选择：** 符合条件的出生地中随机挑选
- ✅ **动态设置：** 在首次 `StartGame()` 时动态确定
- ✅ **灵活调整：** 修改数据库即可，无需重新编译地图
