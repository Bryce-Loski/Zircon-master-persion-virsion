using Library;
using Library.Network;
using Library.SystemModels;
using Server.DBModels;
using Server.Envir;
using Server.Models.Monsters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using S = Library.Network.ServerPackets;

/**
 * 【地图类 - Map.cs】
 * 
 * 功能说明：
 * 1. 代表游戏中的一个地图实例（包括常规地图和副本实例）
 * 2. 从 .map 二进制文件加载地图的地形数据（Cell网格）
 * 3. 管理地图上的所有游戏对象（玩家、怪物、NPC、物品等）
 * 4. 执行地图的游戏逻辑（怪物AI、对象更新等）
 * 
 * 数据来源：
 * ┌─────────────────────┐
 * │  Database MapInfo   │  从数据库加载地图配置
 * └──────────┬──────────┘
 *            │
 * ┌──────────▼──────────┐
 * │   Map .ctor()       │  创建Map实例，关联MapInfo
 * └──────────┬──────────┘
 *            │
 * ┌──────────▼──────────┐
 * │   Map.Load()        │  读取 .map 文件，解析二进制地形数据
 * └──────────┬──────────┘
 *            │
 * ┌──────────▼──────────┐
 * │   Map.Setup()       │  生成NPC、守卫、城堡等游戏内容
 * └─────────────────────┘
**/
namespace Server.Models
{
    public sealed class Map
    {
        /// <summary>
        /// 【地图配置信息引用】
        /// 指向数据库中的 MapInfo 对象，包含此地图的所有配置信息
        /// 例如：FileName="Lodeight"、Description="比奇城 1"、Music、Light等
        /// </summary>
        public MapInfo Info { get; }

        /// <summary>
        /// 【所属副本信息】
        /// - 如果为 null：此地图是常规地图（永久存在）
        /// - 如果不为 null：此地图是副本地图（会根据InstanceInfo配置进行销毁）
        /// </summary>
        public InstanceInfo Instance { get; }

        /// <summary>
        /// 【副本序列号】
        /// 当同一副本有多个实例时的序列编号（0-255）
        /// 用于区分多个并行运行的副本实例
        /// </summary>
        public byte InstanceSequence { get; }

        /// <summary>
        /// 【副本过期时间】
        /// 根据 InstanceInfo.TimeLimitInMinutes 计算
        /// 当 DateTime.Now > InstanceExpiry 时，此副本实例会被自动销毁
        /// </summary>
        public int RespawnIndex { get; }

        /// <summary>
        /// 【地图宽度】单位：Cell数量
        /// 从 .map 文件的字节 22-23 读取
        /// 例如：Width = 200 表示地图有 200 列
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// 【地图高度】单位：Cell数量
        /// 从 .map 文件的字节 24-25 读取
        /// 例如：Height = 200 表示地图有 200 行
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// 【是否有安全区】标记此地图是否包含安全区域
        /// 安全区内玩家不能被 PK 杀害
        /// </summary>
        public bool HasSafeZone { get; set; }

        /// <summary>
        /// 【地图Cell网格】二维数组，存储地图上的所有单元格
        /// 
        /// Cell 结构：
        /// - Cells[x, y]：在坐标 (x, y) 的Cell对象
        /// - 每个Cell代表地图上的一个方块（约48x32像素）
        /// 
        /// 加载过程：
        /// - .map 文件中每 14 字节代表一个Cell
        /// - 第0字节是标志位（bit0=可行走，bit1=有地形）
        /// - 如果此Cell不可行走，则 Cells[x, y] = null
        /// - 如果可行走，则创建新的Cell对象并加入ValidCells列表
        /// </summary>
        public Cell[,] Cells { get; private set; }

        /// <summary>
        /// 【有效单元格列表】仅包含可行走的Cell
        /// 
        /// 用途：
        /// 1. 快速查询地图上的有效位置
        /// 2. 生成怪物时从此列表中随机选择生成点
        /// 3. 计算跑步路径时仅需考虑有效单元格
        /// 
        /// 大小通常为：Width × Height × 60%（约60%的Cell可行走）
        /// </summary>
        public List<Cell> ValidCells { get; } = new List<Cell>();

        /// <summary>
        /// 【地图上的所有对象】包括玩家、怪物、NPC、物品、陷阱等
        /// 在地图实时处理时（Tick）遍历此集合进行更新
        /// </summary>
        public List<MapObject> Objects { get; } = new List<MapObject>();

        /// <summary>
        /// 【地图上的所有玩家】是Objects的子集，便于快速访问
        /// 用于广播消息、伤害计算等需要只影响玩家的逻辑
        /// </summary>
        public List<PlayerObject> Players { get; } = new List<PlayerObject>();

        /// <summary>
        /// 【BOSS怪物列表】只包含BOSS级怪物
        /// 用于特殊逻辑（如BOSS掉落、BOSS通告等）
        /// </summary>
        public List<MonsterObject> Bosses { get; } = new List<MonsterObject>();

        /// <summary>
        /// 【城堡旗帜列表】此地图上的城堡旗帜对象
        /// 旗帜用于表示城堡的所有权：
        /// - 旗帜被击倒 → 城堡易主
        /// </summary>
        public List<CastleFlag> CastleFlags { get; } = new List<CastleFlag>();

        /// <summary>
        /// 【城堡大门列表】此地图上的城堡大门
        /// 大门可以被破坏，防止敌方进入城堡
        /// </summary>
        public List<CastleGate> CastleGates { get; } = new List<CastleGate>();

        /// <summary>
        /// 【城堡守卫列表】此地图上的城堡守卫（NPC）
        /// 守卫会保护城堡免受攻击
        /// </summary>
        public List<CastleGuard> CastleGuards { get; } = new List<CastleGuard>();

        /// <summary>
        /// 【NPC列表】此地图上的NPC对象（商人、任务提供者等）
        /// NPC在地图上有固定的位置和方向
        /// </summary>
        public List<NPCObject> NPCs { get; } = new List<NPCObject>();

        /// <summary>
        /// 【有序对象集合】按X坐标分类存储对象
        /// OrderedObjects[x] 包含所有 X坐标为x 的对象
        /// 
        /// 用途：优化范围查询性能
        /// 当玩家走动时，服务器需要快速找到"视野范围内的所有对象"
        /// 使用此数据结构可以快速定位而不需遍历全部对象
        /// </summary>
        public HashSet<MapObject>[] OrderedObjects;

        /// <summary>
        /// 【最后处理时间】上一次Tick()执行的时间
        /// 用于计算此次Tick的时间间隔
        /// </summary>
        public DateTime LastProcess, LastPlayer;

        /// <summary>
        /// 【副本过期时间】此副本实例何时应该被销毁
        /// 当 DateTime.Now > InstanceExpiry 时，调用 UnloadInstance()
        /// </summary>
        public DateTime InstanceExpiry;

        /// <summary>
        /// 【特殊事件时间】用于记录周年纪念日、圣诞节等特殊事件
        /// 可用于条件性生成特殊怪物或装饰
        /// </summary>
        public DateTime HalloweenEventTime, ChristmasEventTime;

        /// <summary>
        /// 【Map构造函数】初始化地图对象并关联配置信息
        /// 
        /// 参数说明：
        /// - info (MapInfo)：地图配置对象，包含此地图的所有元数据
        /// - instance (InstanceInfo)：如果此地图是副本，指向副本配置；否则为null
        /// - instanceSequence (byte)：副本的序列编号（0-255），默认为0
        /// - respawnIndex (int)：怪物重生点索引，默认为0
        /// 
        /// 注意：构造函数只进行初始化，不加载地图文件
        /// 实际加载需要在构造后调用 Load() 方法
        /// </summary>
        public Map(MapInfo info, InstanceInfo instance = null, byte instanceSequence = 0, int respawnIndex = 0)
        {
            Info = info;
            RespawnIndex = respawnIndex;

            if (instance != null)
            {
                Instance = instance;
                InstanceSequence = instanceSequence;
                InstanceExpiry = instance.TimeLimitInMinutes > 0 ? SEnvir.Now.AddMinutes(instance.TimeLimitInMinutes) : DateTime.MinValue;
            }
        }

        //================================================================================================
        //                            【MAP FILE LOADING - 地图文件加载】
        //================================================================================================

        /// <summary>
        /// 【地图加载方法】从磁盘读取.map文件并解析二进制数据
        /// 
        /// .MAP 文件格式说明：
        /// ┌─────────────────────────────────────────────────────────────────┐
        /// │ .MAP 二进制文件结构                                               │
        /// ├─────────────────────────────────────────────────────────────────┤
        /// │ 字节范围  │ 字节数 │ 说明                                         │
        /// ├─────────────────────────────────────────────────────────────────┤
        /// │ 0-21      │ 22     │ 地图头信息（用途不详）                       │
        /// │ 22-23     │ 2      │ 地图宽度(Width) - 小端序(Low Byte First)    │
        /// │ 24-25     │ 2      │ 地图高度(Height) - 小端序                  │
        /// │ 26-27     │ 2      │ 预留字节                                    │
        /// │ 28+(中间)  │ 变长   │ 地形缓冲数据（用于遮挡计算）              │
        /// │ offset+*  │ 14     │ 单个Cell数据（从offset开始，每Cell 14字节）│
        /// └─────────────────────────────────────────────────────────────────┘
        /// 
        /// Cell 数据结构（14字节）：
        /// ┌──────────────────────────────────────────────────────────┐
        /// │ 偏移 │ 字节数 │ 说明                                      │
        /// ├──────────────────────────────────────────────────────────┤
        /// │ 0    │ 1      │ 标志位(Flag)                             │
        /// │      │        │   bit0 (0x01) = 1：可行走                │
        /// │      │        │   bit1 (0x02) = 1：有地形数据            │
        /// │      │        │   其他位：保留                            │
        /// │ 1-13 │ 13     │ 其他Cell属性（纹理、动画等）            │
        /// └──────────────────────────────────────────────────────────┘
        /// 
        /// Cell 遍历顺序：
        /// for x in 0..Width-1
        ///   for y in 0..Height-1
        ///     offset = 28 + (Width * Height / 4 * 3) + (x * Height + y) * 14
        ///     flag = fileBytes[offset]
        /// 
        /// 执行流程：
        /// 1. 构建.map文件路径：Config.MapPath + Info.FileName + ".map"
        /// 2. 读取整个文件到内存 (fileBytes)
        /// 3. 从 fileBytes[22-23] 解析 Width
        /// 4. 从 fileBytes[24-25] 解析 Height
        /// 5. 创建 Cell[Width, Height] 数组
        /// 6. 遍历每个Cell，根据标志位决定是否创建Cell对象
        /// 7. 可行走的Cell加入到 ValidCells 列表
        /// 8. 创建 OrderedObjects 数组用于对象快速查询
        /// </summary>
        public void Load()
        {
            // 构建.map文件的完整路径
            // 例如：Config.MapPath = "Data/Maps", Info.FileName = "Lodeight"
            //       最终路径 = "Data/Maps/Lodeight.map"
            var path = Path.Combine(Config.MapPath, Info.FileName + ".map");

            // 检查文件是否存在
            if (!File.Exists(path))
            {
                SEnvir.Log($"地图文件未找到: {path}。 ");
                return;
            }

            // ✅ 第1步：读取整个.map文件到内存
            // 这是一个二进制文件，包含地图的完整地形数据
            byte[] fileBytes = File.ReadAllBytes(path);

            // ✅ 第2步：解析地图尺寸
            // 小端序(Little Endian)格式：低字节在前
            // Width 存储在 fileBytes[22] (低位) 和 fileBytes[23] (高位)
            // 计算公式：Width = (高位 << 8) | 低位 = fileBytes[23] * 256 + fileBytes[22]
            Width = fileBytes[23] << 8 | fileBytes[22];
            
            // Height 存储在 fileBytes[24] 和 fileBytes[25]
            Height = fileBytes[25] << 8 | fileBytes[24];

            // ✅ 第3步：创建Cell数组
            // 初始化二维数组，所有元素默认为null
            // null表示该位置不可行走或不存在
            Cells = new Cell[Width, Height];

            // ✅ 第4步：计算Cell数据的起始偏移
            // .map文件结构：
            // - 字节 0-27：文件头 (28字节)
            // - 字节 28到offset-1：地形缓冲数据 (Width * Height / 4 * 3 字节)
            //   这部分数据用于遮挡(Occlusion)计算，我们在这里跳过
            // - 字节 offset开始：Cell数据区，每个Cell占14字节
            int offSet = 28 + Width * Height / 4 * 3;

            // ✅ 第5步：遍历所有Cell，解析标志位
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    // 计算当前Cell在文件中的偏移
                    // Cell索引 = x * Height + y
                    // 文件偏移 = offSet + 索引 * 14
                    byte flag = fileBytes[offSet + (x * Height + y) * 14];

                    // 检查标志位
                    // (flag & 0x02) == 2：检查bit1是否为1（是否有地形）
                    // (flag & 0x01) == 1：检查bit0是否为1（是否可行走）
                    // 只有两个条件都满足，此Cell才是有效的、可行走的
                    if ((flag & 0x02) != 2 || (flag & 0x01) != 1) continue;

                    // ✅ 第6步：创建有效的Cell对象
                    // 为可行走的Cell创建对象，并设置其坐标和所属地图
                    Cell cell = new Cell(new Point(x, y)) { Map = this };
                    
                    // 添加到二维数组
                    Cells[x, y] = cell;
                    
                    // 添加到ValidCells列表，便于快速访问有效位置
                    ValidCells.Add(cell);
                }

            // ✅ 第7步：初始化有序对象集合
            // 创建与Width相同长度的数组
            // OrderedObjects[x] 存储所有X坐标为x的对象
            OrderedObjects = new HashSet<MapObject>[Width];
            for (int i = 0; i < OrderedObjects.Length; i++)
                OrderedObjects[i] = new HashSet<MapObject>();
        }
        
        /// <summary>
        /// 【地图初始化方法】在Load()之后调用
        /// 
        /// 功能：生成地图上的固定对象（NPC、守卫、城堡等）
        /// 
        /// 执行顺序：
        /// 1. CreateGuards()：根据 Info.Guards 生成守卫NPC
        /// 2. CreateCastleFlags/Gates/Guards()：生成城堡相关对象
        /// 3. CreateCellRegions()：从 MapRegion 数据创建区域
        /// </summary>
        public void Setup()
        {
            CreateGuards();

            CreateCastleFlags();
            CreateCastleGates();
            CreateCastleGuards();

            CreateCellRegions();

            LastPlayer = SEnvir.Now;
        }

        /// <summary>
        /// 【守卫生成方法】
        /// 
        /// 功能：遍历 MapInfo.Guards 集合，在地图上生成守卫NPC
        /// 
        /// 数据来源：MapInfo.Guards (DBBindingList<GuardInfo>)
        /// 每个 GuardInfo 包含：
        /// - Monster：守卫的怪物类型（指向 MonsterInfo）
        /// - X, Y：守卫在地图上的坐标
        /// - Direction：守卫朝向（North, South, East, West等）
        /// 
        /// 错误处理：如果守卫类型为null或生成失败，记录错误日志
        /// </summary>
        /// <summary>
        /// 【创建地图守卫】CreateGuards() - 从数据库加载并生成所有守卫
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 【流程说明】
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// 1. 【数据源】Info.Guards 集合
        ///    - Info：MapInfo 对象（当前地图的数据库配置）
        ///    - Info.Guards：DBBindingList&lt;GuardInfo&gt; 集合
        ///    - 包含该地图所有的守卫定义
        ///    
        ///    关键提示：
        ///    - 守卫信息【不存储】在 .map 二进制文件中，只存储在数据库
        ///    - .map 文件只包含地形数据（Cell 信息、高度、是否可行走等）
        ///    - 所有 NPC 和守卫配置都来自数据库的 GuardInfo 表
        /// 
        /// 2. 【数量确定】守卫生成的数量
        ///    - 守卫数量 = Info.Guards.Count
        ///    - 例如：如果 Info.Guards 包含 5 条记录，就会生成 5 个守卫
        ///    - 管理员可以在编辑器中添加/删除 GuardInfo 记录来调整守卫数量
        /// 
        /// 3. 【类型选择】每个守卫是什么怪物
        ///    - 通过 GuardInfo.Monster 字段确定
        ///    - Monster 字段指向一个 MonsterInfo 对象
        ///    - MonsterInfo 定义了怪物的：
        ///      • 外观和动画（图片ID）
        ///      • AI行为和战斗属性
        ///      • 生命值、魔法值、防御等
        ///      • 掉落物品配置
        ///      • 经验值和金币奖励
        /// 
        /// 4. 【位置设定】守卫放在哪里
        ///    - GuardInfo.X：横坐标（0 到 Map.Width-1）
        ///    - GuardInfo.Y：纵坐标（0 到 Map.Height-1）
        ///    - 必须是有效的可行走 Cell（ValidCell）
        /// 
        /// 5. 【朝向设定】守卫面向哪个方向
        ///    - GuardInfo.Direction：MirDirection 枚举值
        ///    - 8 个方向：North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest
        ///    - 仅影响初始显示，不影响 AI 行为
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 【具体步骤】
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// 步骤 1：遍历所有守卫配置
        ///        for each GuardInfo in Info.Guards
        /// 
        /// 步骤 2：验证怪物模板
        ///        if (info.Monster == null)
        ///          → 模板已被删除或未正确关联
        ///          → 记录错误日志并跳过此守卫
        ///          → 不会有对应的怪物对象出现在地图上
        /// 
        /// 步骤 3：创建怪物实例
        ///        mob = MonsterObject.GetMonster(info.Monster)
        ///          → 工厂方法，根据 MonsterInfo 创建一个新的 MonsterObject
        ///          → 该实例还未添加到地图中（独立对象）
        ///          → 继承 MonsterInfo 的所有配置（属性、掉落等）
        /// 
        /// 步骤 4：设置朝向
        ///        mob.Direction = info.Direction
        ///          → 从数据库读取的朝向值赋给怪物
        ///          → 影响图形显示（会使用不同的动画帧）
        /// 
        /// 步骤 5：生成到地图
        ///        if (!mob.Spawn(this, new Point(info.X, info.Y)))
        ///          → Spawn() 方法将怪物添加到地图
        ///          → 参数：this（目标地图）、Point(X坐标, Y坐标)
        ///          → 返回值：
        ///            ✓ true：成功，怪物已添加到地图的 Objects 集合
        ///            ✗ false：失败，可能原因：
        ///              • (X,Y) 不是有效的 ValidCell（不可行走）
        ///              • Cell.GetMovement() 无法找到最近的可行走位置
        ///              • 该位置已经有其他对象
        ///              • 地图初始化尚未完成
        /// 
        /// 步骤 6：错误处理
        ///        if spawn fails → 记录错误日志，继续处理下一个守卫
        ///        不会停止整个地图加载，其他守卫仍会正常生成
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 【数据库结构示例】
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// 假设比奇城（Lodeight）的 GuardInfo 记录：
        /// 
        /// [记录1] GuardInfo
        ///   Map = Lodeight
        ///   Monster = 104 (MonsterInfo - 卫兵)
        ///   X = 100, Y = 100
        ///   Direction = East
        ///   → 生成：一个朝向右边的卫兵，位置在 (100, 100)
        /// 
        /// [记录2] GuardInfo
        ///   Map = Lodeight
        ///   Monster = 104 (MonsterInfo - 卫兵)
        ///   X = 150, Y = 100
        ///   Direction = West
        ///   → 生成：一个朝向左边的卫兵，位置在 (150, 100)
        /// 
        /// [记录3] GuardInfo
        ///   Map = Lodeight
        ///   Monster = 105 (MonsterInfo - 城主)
        ///   X = 200, Y = 150
        ///   Direction = North
        ///   → 生成：一个朝向上的城主，位置在 (200, 150)
        /// 
        /// 服务器启动时会自动遍历这 3 条记录，创建 3 个怪物对象，并将它们放置在地图上。
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 【重要提示】
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// ❌ 守卫【不是】硬编码在地图文件中
        /// ✅ 守卫【是】数据库配置，灵活调整无需重新编译地图
        /// 
        /// 如何增加/减少守卫？
        /// → 直接编辑数据库 GuardInfo 表，添加/删除记录
        /// → 重启服务器后新的守卫配置生效
        /// → 不需要修改地图文件
        /// 
        /// 如何修改守卫位置？
        /// → 编辑数据库中对应 GuardInfo 记录的 X 和 Y 字段
        /// → 重启服务器生效
        /// 
        /// 如何修改守卫类型？
        /// → 编辑数据库中对应 GuardInfo 记录的 Monster 字段
        /// → 关联不同的 MonsterInfo 对象
        /// → 重启服务器生效
        /// </summary>
        private void CreateGuards()
        {
            foreach (GuardInfo info in Info.Guards)
            {
                if (info.Monster == null)
                {
                    SEnvir.Log($"[生成守卫失败] 地图:{Info.Description}, 位置: {info.X}, {info.Y}");
                    continue;
                }

                MonsterObject mob = MonsterObject.GetMonster(info.Monster);
                mob.Direction = info.Direction;

                if (!mob.Spawn(this, new Point(info.X, info.Y)))
                {
                    SEnvir.Log($"[生成守卫失败] 地图:{Info.Description}, 位置: {info.X}, {info.Y}");
                    continue;
                }
            }
        }

        /// <summary>
        /// 【城堡旗帜生成方法】
        /// 
        /// 功能：生成城堡控制旗帜（用于表示城堡所有权）
        /// </summary>
        private void CreateCastleFlags()
        {
            foreach (var castle in Info.Castles)
            {
                foreach (var info in castle.Flags)
                {
                    CastleFlag mob = MonsterObject.GetMonster(info.Monster) as CastleFlag;

                    mob.Castle = castle;

                    if (!mob.Spawn(castle, info))
                    {
                        SEnvir.Log($"[生成旗帜失败] 地图:{Info.Description}, 位置: {info.X}, {info.Y}");
                        continue;
                    }
                }
            }
        }
        private void CreateCastleGates()
        {
            foreach (var castle in Info.Castles)
            {
                foreach (var gate in castle.Gates)
                {
                    var mob = CastleGates.FirstOrDefault(x => x.GateInfo == gate);

                    if (mob == null)
                    {
                        mob = MonsterObject.GetMonster(gate.Monster) as CastleGate;

                        mob.Spawn(castle, gate);
                    }
                    else
                    {
                        mob.RepairGate();
                    }
                }
            }
        }
        private void CreateCastleGuards()
        {
            foreach (var castle in Info.Castles)
            {
                foreach (var guard in castle.Guards)
                {
                    var mob = CastleGuards.FirstOrDefault(x => x.GuardInfo == guard);

                    if (mob == null)
                    {
                        mob = MonsterObject.GetMonster(guard.Monster) as CastleGuard;

                        mob.Spawn(castle, guard);
                    }
                    else
                    {
                        mob.RepairGuard();
                    }
                }
            }
        }

        public void CreateCellRegions()
        {
            foreach (MapRegion region in Info.Regions)
            {
                if (region.RegionType != RegionType.Area) continue;

                var points = region.GetPoints(Width);

                foreach (Point sPoint in points)
                {
                    Cell source = GetCell(sPoint);

                    if (source == null)
                    {
                        SEnvir.Log($"[单元格错误] 源: {Info.FileName} {region.Description}, X:{sPoint.X}, Y:{sPoint.Y}");
                        continue;
                    }

                    if (source.Regions == null)
                        source.Regions = new List<MapRegion>();

                    source.Regions.Add(region);
                }
            }
        }

        public void RefreshFlags()
        {
            foreach (var ob in CastleFlags)
            {
                ob.Refresh();
            }
        }

        public void Process()
        {
            if (LastPlayer.AddMinutes(1) < SEnvir.Now && Players.Any())
            {
                LastPlayer = SEnvir.Now;
            }
        }

        public void AddObject(MapObject ob)
        {
            Objects.Add(ob);

            switch (ob.Race)
            {
                case ObjectType.Player:
                    Players.Add((PlayerObject)ob);
                    break;
                case ObjectType.Item:
                    break;
                case ObjectType.NPC:
                    NPCs.Add((NPCObject)ob);
                    break;
                case ObjectType.Spell:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject)ob;
                    if (mob.MonsterInfo.IsBoss)
                        Bosses.Add(mob);
                    break;
            }
        }
        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            switch (ob.Race)
            {
                case ObjectType.Player:
                    Players.Remove((PlayerObject)ob);
                    break;
                case ObjectType.Item:
                    break;
                case ObjectType.NPC:
                    NPCs.Remove((NPCObject)ob);
                    break;
                case ObjectType.Spell:
                    break;
                case ObjectType.Monster:
                    MonsterObject mob = (MonsterObject)ob;
                    if (mob.MonsterInfo.IsBoss)
                        Bosses.Remove(mob);
                    break;
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return null;

            return Cells[x, y];
        }
        public Cell GetCell(Point location)
        {
            return GetCell(location.X, location.Y);
        }

        //获取地图上某个位置周围一定范围内的单元格列表，
        // 范围可以是一个圆形或方形（由 circle 参数控制），并且可以选择是否以随机顺序返回结果。
        //参数说明：
        // - location：中心位置的坐标。
        // - minRadius：最小半径，只有距离中心位置大于或等于这个值的单元格才会被包含在结果中。
        // - maxRadius：最大半径，只有距离中心位置小于或等于这个值的单元格才会被包含在结果中。
        // - randomOrder：如果为 true，返回的单元格列表将以随机顺序排列；如果为 false，返回的单元格列表将按照从左到右、从上到下的顺序排列。
        // - circle：如果为 true，使用欧几里得距离来计算单元格与中心位置的距离，返回一个圆形范围内的单元格；如果为 false，使用切比雪夫距离（Chebyshev distance）来计算单元格与中心位置的距离，返回一个方形或菱形范围内的单元格。
        public List<Cell> GetCells(Point location, int minRadius, int maxRadius, bool randomOrder = false, bool circle = false)
        {
            List<Cell> cells = new List<Cell>();

            // Iterate over a square bounding box that covers the circle
            for (int y = location.Y - maxRadius; y <= location.Y + maxRadius; y++)
            {
                if (y < 0 || y >= Height) continue;

                for (int x = location.X - maxRadius; x <= location.X + maxRadius; x++)
                {
                    if (x < 0 || x >= Width) continue;

                    // Compute Manhattan/Euclidean distance depending on circle flag
                    int dx = x - location.X;
                    int dy = y - location.Y;

                    double distance = circle
                        ? Math.Sqrt(dx * dx + dy * dy)   // Euclidean distance for circle
                        : Math.Max(Math.Abs(dx), Math.Abs(dy)); // Chebyshev distance for square/diamond

                    // Only keep cells inside the desired radius range
                    if (distance < minRadius || distance > maxRadius)
                        continue;

                    Cell cell = Cells[x, y];
                    if (cell == null) continue;

                    cells.Add(cell);
                }
            }

            if (randomOrder)
            {
                return cells.OrderBy(item => SEnvir.Random.Next()).ToList();
            }

            return cells;
        }

        public Point GetRandomLocation()
        {
            return ValidCells.Count > 0 ? ValidCells[SEnvir.Random.Next(ValidCells.Count)].Location : Point.Empty;
        }

        public Point GetRandomLocation(Point location, int range, int attempts = 25)
        {
            int minX = Math.Max(0, location.X - range);
            int maxX = Math.Min(Width, location.X + range);
            int minY = Math.Max(0, location.Y - range);
            int maxY = Math.Min(Height, location.Y + range);

            for (int i = 0; i < attempts; i++)
            {
                Point test = new Point(SEnvir.Random.Next(minX, maxX), SEnvir.Random.Next(minY, maxY));

                if (GetCell(test) != null)
                    return test;
            }

            return Point.Empty;
        }

        public Point GetRandomLocation(int minX, int maxX, int minY, int maxY, int attempts = 25)
        {
            for (int i = 0; i < attempts; i++)
            {
                Point test = new Point(SEnvir.Random.Next(minX, maxX), SEnvir.Random.Next(minY, maxY));

                if (GetCell(test) != null)
                    return test;
            }

            return Point.Empty;
        }

        public void Broadcast(Point location, Packet p)
        {
            foreach (PlayerObject player in Players)
            {
                if (!Functions.InRange(location, player.CurrentLocation, Config.MaxViewRange)) continue;
                player.Enqueue(p);
            }
        }
        public void Broadcast(Packet p)
        {
            foreach (PlayerObject player in Players)
                player.Enqueue(p);
        }
    }

    public class SpawnInfo
    {
        public RespawnInfo Info;
        public Map CurrentMap;

        public DateTime NextSpawn;
        public int AliveCount;

        public DateTime LastCheck;

        public SpawnInfo(RespawnInfo info, InstanceInfo instance, byte index)
        {
            Info = info;
            CurrentMap = SEnvir.GetMap(info.Region.Map, instance, index);
            LastCheck = SEnvir.Now;
        }

        public void DoSpawn(bool eventSpawn)
        {
            if (CurrentMap.RespawnIndex != Info.RespawnIndex) return;

            if (!eventSpawn)
            {
                if (Info.EventSpawn || SEnvir.Now < NextSpawn) return;

                if (Info.Delay >= 1000000)
                {
                    TimeSpan timeofDay = TimeSpan.FromMinutes(Info.Delay - 1000000);

                    if (LastCheck.TimeOfDay >= timeofDay || SEnvir.Now.TimeOfDay < timeofDay)
                    {
                        LastCheck = SEnvir.Now;
                        return;
                    }

                    LastCheck = SEnvir.Now;
                }
                else
                {
                    if (Info.Announce)
                        NextSpawn = SEnvir.Now.AddSeconds(Info.Delay * 60);
                    else
                        NextSpawn = SEnvir.Now.AddSeconds(SEnvir.Random.Next(Info.Delay * 60) + Info.Delay * 30);

                }
            }

            for (int i = AliveCount; i < Info.Count; i++)
            {
                MonsterObject mob = MonsterObject.GetMonster(Info.Monster);

                if (!Info.Monster.IsBoss)
                {
                    if (SEnvir.Now > CurrentMap.HalloweenEventTime && SEnvir.Now <= Config.HalloweenEventEnd)
                    {
                        mob = new HalloweenMonster { MonsterInfo = Info.Monster, HalloweenEventMob = true };
                        CurrentMap.HalloweenEventTime = SEnvir.Now.AddHours(1);
                    }
                    else if (SEnvir.Now > CurrentMap.ChristmasEventTime && SEnvir.Now <= Config.ChristmasEventEnd)
                    {
                        mob = new ChristmasMonster { MonsterInfo = Info.Monster, ChristmasEventMob = true };
                        CurrentMap.ChristmasEventTime = SEnvir.Now.AddMinutes(20);
                    }
                }

                mob.SpawnInfo = this;

                if (!mob.Spawn(Info.Region, CurrentMap.Instance, CurrentMap.InstanceSequence))
                {
                    mob.SpawnInfo = null;
                    continue;
                }

                if (Info.Announce)
                {
                    if (Info.Delay >= 1000000)
                    {
                        foreach (SConnection con in SEnvir.Connections)
                            con.ReceiveChat($"{mob.MonsterInfo.MonsterName} 已出现。", MessageType.System);
                    }
                    else
                    {
                        foreach (SConnection con in SEnvir.Connections)
                            con.ReceiveChat(string.Format(con.Language.BossSpawn, CurrentMap.Info.Description), MessageType.System);
                    }
                }

                mob.DropSet = Info.DropSet;
                AliveCount++;
            }
        }
    }

    //地图上的单元格（Cell）类，包含了该单元格的位置、地形信息、以及当前在该单元格上的对象列表（例如玩家、怪物、NPC等）。
    public class Cell
    {
        public Point Location;//单元格在地图上的坐标位置。

        public Map Map; //单元格所属的地图引用，方便在需要时访问地图的其他信息或方法。

        public List<MapObject> Objects;//当前在该单元格上的对象列表，例如玩家、怪物、NPC等。
        public SafeZoneInfo SafeZone;//如果该单元格是安全区的一部分，则包含安全区的信息（SafeZoneInfo），否则为 null。

        public List<MovementInfo> Movements;//如果该单元格是一个传送点，则包含传送信息（MovementInfo）的列表，指示玩家在该单元格上时可能触发的传送行为。

        public List<MapRegion> Regions = [];//如果该单元格属于一个或多个地图区域（MapRegion），则包含这些区域的列表，方便在玩家进入该单元格时触发区域相关的事件或效果。

        public List<QuestTask> QuestTasks;//如果该单元格与某些任务相关，则包含这些任务的列表，方便在玩家进入该单元格时检查是否满足任务条件并更新任务进度。

        public Cell(Point location)
        {
            Location = location;
        }


        public void AddObject(MapObject ob)
        {
            if (Objects == null)
                Objects = new List<MapObject>();

            Objects.Add(ob);

            ob.CurrentMap = Map;
            ob.CurrentLocation = Location;

            Map.OrderedObjects[Location.X].Add(ob);
        }
        public void RemoveObject(MapObject ob)
        {
            Objects.Remove(ob);

            if (Objects.Count == 0)
                Objects = null;

            Map.OrderedObjects[Location.X].Remove(ob);
        }
        public bool IsBlocking(MapObject checker, bool cellTime)
        {
            if (Objects == null) return false;

            foreach (MapObject ob in Objects)
            {
                if (!ob.Blocking) continue;
                if (cellTime && SEnvir.Now < ob.CellTime) continue;

                if (ob.Stats == null) return true;

                if (ob.Buffs.Any(x => x.Type == BuffType.Cloak || x.Type == BuffType.Transparency) && ob.Level > checker.Level && !ob.InGroup(checker)) continue;


                return true;
            }

            return false;
        }

        //处理当一个地图对象（通常是玩家）进入该单元格时的逻辑，
        // 包括检查任务触发条件和处理传送点等。
        public Cell GetMovement(MapObject ob)
        {
            //TODO - 对于大区域来说，这种方法可能效率不高。需要找到更好的方法来检查对象何时加入或离开区域。
            // - 首先检查该单元格上是否有与任务相关的任务（QuestTasks）。如果有，并且当前对象是玩家（PlayerObject），则进一步检查玩家的任务列表，看看是否有未完成的任务与这些任务相关。
            if (QuestTasks != null && QuestTasks.Count > 0)
            {
                if (ob.Race == ObjectType.Player)
                {
                    PlayerObject player = (PlayerObject)ob;

                    foreach (var task in QuestTasks)
                    {
                        var userQuest = player.Quests.FirstOrDefault(x => x.QuestInfo == task.Quest && !x.Completed);

                        if (userQuest == null) continue;

                        UserQuestTask userTask = userQuest.Tasks.FirstOrDefault(x => x.Task == task);

                        if (userTask == null)
                        {
                            userTask = SEnvir.UserQuestTaskList.CreateNewObject();
                            userTask.Task = task;
                            userTask.Quest = userQuest;
                        }

                        if (userTask.Completed) continue;

                        userTask.Amount = 1;

                        player.Enqueue(new S.QuestChanged { Quest = userQuest.ToClientInfo() });
                    }
                }
            }

            // - 接下来检查该单元格上是否有传送点（Movements）。
            // 如果有，则随机选择一个传送点，并尝试将对象传送到目标位置。
            // 传送过程中会检查各种条件，例如玩家等级、职业、所需物品等，如果条件不满足，则不会进行传送。
            if (Movements == null || Movements.Count == 0)
                return this; //没有传送点，原地不动

            // - 由于一个单元格上可能有多个传送点，因此这里使用一个循环来尝试随机选择一个传送点进行传送，最多尝试 5 次。
            for (int i = 0; i < 5; i++) //20 Attempts to get movement;
            {
                MovementInfo movement = Movements[SEnvir.Random.Next(Movements.Count)];

                //获取目标地图
                Map map = SEnvir.GetMap(movement.DestinationRegion.Map, Map.Instance, Map.InstanceSequence);

                //处理副本实例逻辑
                if (movement.NeedInstance != null)
                {
                    if (ob.Race != ObjectType.Player) break;

                    if (Map.Instance != null) //离开当前副本
                    {
                        map = SEnvir.GetMap(movement.DestinationRegion.Map, null, 0);
                    }
                    else //进入副本
                    {
                        var (index, result) = ((PlayerObject)ob).GetInstance(movement.NeedInstance, walkOn: true);

                        if (result != InstanceResult.Success)
                        {
                            ((PlayerObject)ob).SendInstanceMessage(movement.NeedInstance, result);
                            break;
                        }

                        map = SEnvir.GetMap(movement.DestinationRegion.Map, movement.NeedInstance, index.Value);
                    }
                }

                if (map == null) break;

                //随机选择目标区域内的一个点
                Cell cell = map.GetCell(movement.DestinationRegion.PointList[SEnvir.Random.Next(movement.DestinationRegion.PointList.Count)]);

                if (cell == null) continue;

                //检查玩家条件（等级、职业、物品）
                if (ob.Race == ObjectType.Player)
                {
                    bool allowed = true;
                    PlayerObject player = (PlayerObject)ob;

                    //检查最低等级
                    if (movement.DestinationRegion.Map.MinimumLevel > ob.Level && !player.Character.Account.TempAdmin)
                    {
                        player.Connection.ReceiveChatWithObservers(con => string.Format(con.Language.NeedLevel, movement.DestinationRegion.Map.MinimumLevel), MessageType.System);
                        break;
                    }

                    //检查最高等级
                    if (movement.DestinationRegion.Map.MaximumLevel > 0 && movement.DestinationRegion.Map.MaximumLevel < ob.Level && !player.Character.Account.TempAdmin)
                    {
                        player.Connection.ReceiveChatWithObservers(con => string.Format(con.Language.NeedMaxLevel, movement.DestinationRegion.Map.MaximumLevel), MessageType.System);
                        break;
                    }

                    //检查职业要求
                    if (movement.DestinationRegion.Map.RequiredClass != RequiredClass.None && movement.DestinationRegion.Map.RequiredClass != RequiredClass.All)
                    {
                        switch (player.Class)
                        {
                            case MirClass.Warrior:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Warrior) != RequiredClass.Warrior)
                                    allowed = false;
                                break;
                            case MirClass.Wizard:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Wizard) != RequiredClass.Wizard)
                                    allowed = false;
                                break;
                            case MirClass.Taoist:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Taoist) != RequiredClass.Taoist)
                                    allowed = false;
                                break;
                            case MirClass.Assassin:
                                if ((movement.DestinationRegion.Map.RequiredClass & RequiredClass.Assassin) != RequiredClass.Assassin)
                                    allowed = false;
                                break;
                        }

                        if (!allowed)
                        {
                            player.Connection.ReceiveChatWithObservers(con => string.Format(con.Language.NeedClass, movement.DestinationRegion.Map.RequiredClass.ToString()), MessageType.System);

                            break;
                        }
                    }

                    //检查物品、刷新点、洞穴等要求
                    if (movement.NeedSpawn != null)
                    {
                        SpawnInfo spawn = SEnvir.Spawns.FirstOrDefault(x => x.Info == movement.NeedSpawn);

                        if (spawn == null)
                            break;

                        if (spawn.AliveCount == 0)
                        {
                            player.Connection.ReceiveChatWithObservers(con => con.Language.NeedMonster, MessageType.System);
                            break;
                        }
                    }

                    //检查副本要求
                    if (movement.NeedHole)
                    {
                        var holes = Objects?.OfType<SpellObject>().Any(m => m.Effect == SpellEffect.ZombieHole && m.CurrentLocation == Location) ?? false;

                        if (!holes)
                            break;
                    }

                    //检查物品要求,消耗物品
                    if (movement.NeedItem != null)
                    {
                        if (player.GetItemCount(movement.NeedItem) == 0)
                        {
                            player.Connection.ReceiveChatWithObservers(con => string.Format(con.Language.NeedItem, movement.NeedItem.ItemName), MessageType.System);
                            break;
                        }

                        player.TakeItem(movement.NeedItem, 1);
                    }

                    //执行传送效果
                    switch (movement.Effect)
                    {
                        // 修复装备
                        case MovementEffect.SpecialRepair:
                            player.SpecialRepair(EquipmentSlot.Weapon);
                            player.SpecialRepair(EquipmentSlot.Shield);
                            player.SpecialRepair(EquipmentSlot.Helmet);
                            player.SpecialRepair(EquipmentSlot.Armour);
                            player.SpecialRepair(EquipmentSlot.Necklace);
                            player.SpecialRepair(EquipmentSlot.BraceletL);
                            player.SpecialRepair(EquipmentSlot.BraceletR);
                            player.SpecialRepair(EquipmentSlot.RingL);
                            player.SpecialRepair(EquipmentSlot.RingR);
                            player.SpecialRepair(EquipmentSlot.Shoes);

                            player.RefreshStats();
                            break;
                    }
                }

                // 8. 递归检查目标点是否还有传送点
                return cell.GetMovement(ob);
            }

            return this;// 传送失败，原地不动
        }
    }
}
