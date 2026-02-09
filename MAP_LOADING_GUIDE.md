# 【Mir3 地图加载完整流程指南】

## 概述

本文档详细说明服务器如何从 `.map` 二进制文件加载游戏地图的完整过程。

---

## 一、MapInfo 类结构

**文件位置：** `LibraryCore/SystemModels/MapInfo.cs`

### 1.1 核心字段

| 字段名 | 类型 | 说明 | 示例 |
|--------|------|------|------|
| **FileName** | string | 地图文件名（主键） | "Lodeight" |
| **Description** | string | 地图显示名称 | "比奇城 1" |
| **MiniMap** | int | 小地图资源索引 | 5 |
| **Width** | int (计算得出) | 地图宽度(Cell数) | 200 |
| **Height** | int (计算得出) | 地图高度(Cell数) | 200 |

### 1.2 MapInfo 生命周期

```
┌─────────────────┐
│ 数据库加载      │  SEnvir.LoadDatabase()
└────────┬────────┘
         │
┌────────▼────────┐
│ OnLoaded()      │  初始化 Stats 属性
└────────┬────────┘
         │
┌────────▼────────┐
│ MapInfo 对象    │  包含所有地图配置
└────────┬────────┘
         │
┌────────▼────────┐
│ 创建 Map 实例   │  Map(MapInfo info)
└─────────────────┘
```

---

## 二、.MAP 文件格式

### 2.1 二进制文件结构

```
┌─────────────────────────────────────────────────────────┐
│                   .MAP 文件结构                         │
├─────────────────┬──────────┬──────────────────────────┤
│ 字节范围        │ 字节数   │ 说明                     │
├─────────────────┼──────────┼──────────────────────────┤
│ 0-21            │ 22       │ 文件头（预留）           │
│ 22-23           │ 2        │ 地图宽度(Width)          │
│ 24-25           │ 2        │ 地图高度(Height)         │
│ 26-27           │ 2        │ 预留字节                 │
│ 28-offset-1     │ 变长     │ 地形缓冲数据(遮挡数据)   │
│ offset+         │ 14*N     │ Cell 数据区(每Cell 14字节) │
└─────────────────┴──────────┴──────────────────────────┘

说明：
- offset = 28 + Width * Height / 4 * 3
- N = Width * Height（Cell总数）
```

### 2.2 尺寸解析（小端序 Little Endian）

```csharp
// Width 在字节 22-23 中，小端序格式
Width = fileBytes[23] << 8 | fileBytes[22];
//     = (高字节 << 8) | 低字节
//     = fileBytes[23] * 256 + fileBytes[22]

// 示例：如果 fileBytes[22]=0xC8, fileBytes[23]=0x00
//      Width = (0x00 << 8) | 0xC8 = 200

Height = fileBytes[25] << 8 | fileBytes[24];  // 同理
```

### 2.3 Cell 标志位说明

```
Cell 标志位（第0字节）：
┌─────────────────────────────────────┐
│ 位  │ 掩码 │ 说明                   │
├─────┼─────┼────────────────────────┤
│ 0   │ 0x01│ 可行走标志             │
│ 1   │ 0x02│ 有地形标志             │
│ 2-7 │ -   │ 保留（用途不详）       │
└─────┴─────┴────────────────────────┘

Cell 有效条件：
(flag & 0x02) == 2  AND  (flag & 0x01) == 1
即：bit1 为 1 且 bit0 为 1
```

---

## 三、服务器加载流程

### 3.1 完整流程图

```
┌──────────────────────────────────────────────────────────────┐
│  1. SEnvir.Initialize()                                      │
│     从数据库加载所有 MapInfo 对象到 MapInfoList 集合        │
└────────────┬─────────────────────────────────────────────────┘
             │
┌────────────▼─────────────────────────────────────────────────┐
│  2. SEnvir.StartEnvir()                                      │
│     创建 Map 实例并加载地形数据                             │
└────────────┬─────────────────────────────────────────────────┘
             │
┌────────────▼─────────────────────────────────────────────────┐
│  3. for (int i = 0; i < MapInfoList.Count; i++)            │
│     Maps[MapInfoList[i]] = new Map(MapInfoList[i])          │
│     Map.Load()   ← 读取 .map 文件                           │
│     Map.Setup()  ← 生成固定对象                             │
└──────────────────────────────────────────────────────────────┘
```

### 3.2 代码执行顺序

#### 步骤 1: 创建 Map 实例

```csharp
// ServerLibrary/Models/Map.cs
public Map(MapInfo info, InstanceInfo instance = null, byte instanceSequence = 0, int respawnIndex = 0)
{
    Info = info;          // 保存地图配置引用
    RespawnIndex = respawnIndex;
    
    // 如果是副本地图，设置过期时间
    if (instance != null)
    {
        Instance = instance;
        InstanceSequence = instanceSequence;
        InstanceExpiry = instance.TimeLimitInMinutes > 0 
            ? SEnvir.Now.AddMinutes(instance.TimeLimitInMinutes) 
            : DateTime.MinValue;
    }
}
```

#### 步骤 2: 加载地形数据

```csharp
// ServerLibrary/Models/Map.cs
public void Load()
{
    // 2.1 构建文件路径
    var path = Path.Combine(Config.MapPath, Info.FileName + ".map");
    //        = "Data/Maps/Lodeight.map"
    
    if (!File.Exists(path)) 
    {
        SEnvir.Log($"地图文件未找到: {path}");
        return;  // ← 关键错误：如果文件不存在，地图加载失败
    }
    
    // 2.2 读取文件
    byte[] fileBytes = File.ReadAllBytes(path);
    
    // 2.3 解析尺寸（小端序）
    Width = fileBytes[23] << 8 | fileBytes[22];  // 字节 22-23
    Height = fileBytes[25] << 8 | fileBytes[24]; // 字节 24-25
    
    // 2.4 创建 Cell 数组
    Cells = new Cell[Width, Height];
    
    // 2.5 计算 Cell 数据起始位置
    int offSet = 28 + Width * Height / 4 * 3;
    
    // 2.6 遍历所有 Cell 并创建有效 Cell 对象
    for (int x = 0; x < Width; x++)
        for (int y = 0; y < Height; y++)
        {
            // 2.6.1 计算此 Cell 在文件中的位置
            int cellOffset = offSet + (x * Height + y) * 14;
            byte flag = fileBytes[cellOffset];
            
            // 2.6.2 检查是否可行走
            if ((flag & 0x02) != 2 || (flag & 0x01) != 1) 
                continue;  // 跳过不可行走的 Cell
            
            // 2.6.3 创建 Cell 对象并添加到集合
            Cell cell = new Cell(new Point(x, y)) { Map = this };
            Cells[x, y] = cell;
            ValidCells.Add(cell);
        }
    
    // 2.7 初始化有序对象集合
    OrderedObjects = new HashSet<MapObject>[Width];
    for (int i = 0; i < OrderedObjects.Length; i++)
        OrderedObjects[i] = new HashSet<MapObject>();
}
```

#### 步骤 3: 设置固定对象

```csharp
// ServerLibrary/Models/Map.cs
public void Setup()
{
    // 生成 MapInfo.Guards 中的守卫
    CreateGuards();
    
    // 生成城堡相关对象
    CreateCastleFlags();
    CreateCastleGates();
    CreateCastleGuards();
    
    // 创建地图区域
    CreateCellRegions();
    
    LastPlayer = SEnvir.Now;
}
```

---

## 四、MapInfo 与 .MAP 文件的关联

### 4.1 关键字段映射

```
MapInfo.FileName  ────────→  .map 文件名
                             例如："Lodeight" → "Lodeight.map"

MapInfo.Info      ────────→  Map 对象
                             Map.Load() 读取 FileName 对应的 .map 文件

MapInfo.Width/Height (计算) ← .map 文件内容
                             从文件的字节 22-25 解析得出
```

### 4.2 加载链

```
MapInfo（数据库）
    │
    └─→ Map(info)
            │
            └─→ Map.Load()
                    │
                    ├─→ 读取 info.FileName + ".map"
                    ├─→ 解析宽度 / 高度
                    ├─→ 创建 Cell 网格
                    └─→ ValidCells 列表（约60%）
                    
            └─→ Map.Setup()
                    │
                    └─→ 生成 NPC / 守卫 / 城堡 / 区域
```

---

## 五、客户端显示流程

### 5.1 小地图显示

```
客户端启动
    │
    └─→ CEnvir.LoadDatabase()
            │
            └─→ Globals.MapInfoList = Session.GetCollection<MapInfo>()
            
玩家进入游戏
    │
    └─→ S.StartGame 包（包含 MapIndex）
            │
            └─→ MapControl.MapInfo = MapInfoList.Find(x => x.Index == MapIndex)
            
            └─→ MapControl.OnMapInfoChanged() 事件触发
                    │
                    └─→ MiniMapDialog.MapControl_MapInfoChanged()
                            │
                            ├─→ Image.Index = MapInfo.MiniMap
                            │   （加载小地图图像资源）
                            │
                            ├─→ ScaleX/Y = 计算缩放比例
                            │
                            └─→ 显示 NPC / 怪物 / 传送点标记
```

---

## 六、关键数据结构

### 6.1 Cell 类

```csharp
public class Cell
{
    public Point Location { get; set; }  // 坐标 (x, y)
    public Map Map { get; set; }         // 所属地图
    public List<MapObject> Objects;      // 此Cell上的对象列表
    // 其他地形/地形属性字段...
}
```

### 6.2 ValidCells 用途

```
ValidCells 是一个 List<Cell>，仅包含可行走的 Cell
用处：
1. 快速获取地图上所有有效位置
2. 怪物生成时随机选择生成点
3. 路径寻找算法的候选点集合
4. 节省内存（Width * Height 的约 60%）

大小：通常 Width * Height * 0.6 个元素
访问速度：O(1) 随机访问（列表）
```

---

## 七、常见问题排查

### Q1: 地图加载失败，提示"地图文件未找到"

**原因：**
- .map 文件不存在或路径错误
- Config.MapPath 配置不正确

**解决方案：**
```csharp
// 检查实际文件位置
// 应该在 Config.MapPath + Info.FileName + ".map"
// 例如 "Data/Maps/Lodeight.map"

// 查看日志输出，会显示完整路径
SEnvir.Log($"地图文件未找到: {path}");
```

### Q2: 地图显示范围不对或玩家卡住

**原因：**
- .map 文件损坏或格式不正确
- Width/Height 解析错误（字节序问题）

**排查步骤：**
```csharp
// 1. 检查宽高值是否合理（通常 50-500）
Console.WriteLine($"Width: {map.Width}, Height: {map.Height}");

// 2. 检查 ValidCells 数量
Console.WriteLine($"Valid Cells: {map.ValidCells.Count}");

// 3. 确保二进制文件确实包含数据
if (fileBytes.Length < 28 + Width * Height * 14)
    throw new Exception("文件大小不匹配");
```

### Q3: 小地图显示不正确

**原因：**
- MapInfo.MiniMap 索引设置错误
- 小地图资源文件 (MiniMap.Zl) 缺失

**解决方案：**
```csharp
// 检查 MapInfo 配置
if (mapInfo.MiniMap <= 0)
    Console.WriteLine($"地图 {mapInfo.Description} 没有配置小地图");

// 确保资源文件存在
// Data/MiniMap.Zl 必须包含指定索引的图像
```

---

## 八、性能优化建议

### 8.1 预加载 vs 延迟加载

**当前实现：** 服务器启动时加载所有地图（预加载）

**优点：**
- 玩家进入地图时无延迟
- 怪物生成速度快

**缺点：**
- 启动时间长
- 内存占用大（若地图数量多）

### 8.2 内存优化

```csharp
// 当前：ValidCells 存储所有可行走位置
List<Cell> ValidCells;  // 约 Width * Height * 0.6 个元素

// 替代方案（如果内存紧张）：
// 运行时计算而不是预存储
// 但会增加 CPU 负担
```

---

## 九、相关文件一览

| 文件 | 作用 | 关键方法 |
|------|------|---------|
| `LibraryCore/SystemModels/MapInfo.cs` | 地图配置 | OnLoaded(), StatsChanged() |
| `ServerLibrary/Models/Map.cs` | 地图实例 | Load(), Setup() |
| `Client/Envir/CEnvir.cs` | 客户端数据加载 | LoadDatabase() |
| `Client/Scenes/Views/MiniMapDialog.cs` | 小地图显示 | MapControl_MapInfoChanged() |
| `Server/Views/MapInfoView.cs` | 地图编辑工具 | 配置地图属性 |

---

## 十、总结

```
MapInfo（数据库配置）
    ↓
Map 实例化
    ↓
Map.Load()（读取 .map 二进制文件）
    • 解析 Width/Height
    • 创建 Cell[Width, Height] 网格
    • 筛选有效 Cell 进入 ValidCells
    ↓
Map.Setup()（生成游戏内容）
    • 创建 NPC、守卫、城堡
    • 建立区域映射
    ↓
游戏运行（地图处理、玩家交互、小地图显示）
```

---

**文档更新时间：** 2026年2月9日  
**涉及版本：** Mir3 Zircon  
