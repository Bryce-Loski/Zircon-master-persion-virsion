using MirDB;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Library.SystemModels
{
    /// <summary>
    /// 【地图配置类】MapInfo 是Mir3游戏中的地图配置元数据类
    /// 
    /// 功能说明：
    /// 1. 存储游戏地图的所有配置信息（名称、尺寸、游戏规则等）
    /// 2. 关联实际的.map二进制地图文件（通过FileName字段）
    /// 3. 关联NPC、怪物生成点（Guards、Regions）、采矿点（Mining）、城堡等游戏内容
    /// 4. 配置地图的视觉效果（小地图、背景、光照、音乐）和游戏规则（PK设置、召唤规则等）
    /// 
    /// 数据流：
    /// 【服务器启动】→ 从数据库加载MapInfo → 创建Map实例 → Map.Load()读取.map文件 → 初始化游戏世界
    /// 
    /// 客户端流程：
    /// 【客户端启动】→ 加载MapInfoList → 玩家进入游戏 → 根据MapIndex找到对应MapInfo → 显示地图和小地图
    /// </summary>
    public partial class MapInfo : DBObject
    {
        /// <summary>
        /// 【地图文件名】- 必须与服务器 Data/Maps/ 目录下的 .map 文件名对应
        /// 例如：FileName = "Lodeight" 则对应文件为 "Lodeight.map"
        /// 
        /// .map 文件格式（二进制）：
        /// - 字节 0-21：地图头信息（28字节）
        /// - 字节 22-23：地图宽度(Width)，小端序 (低字节在前)
        /// - 字节 24-25：地图高度(Height)，小端序
        /// - 字节 26-27：预留
        /// - 字节 28+：地形数据，每个Cell占14字节
        ///   其中第0字节是标志位：
        ///   - bit0 (0x01)：是否可通行
        ///   - bit1 (0x02)：是否有地形
        /// 
        /// [IsIdentity] 标记：表示此字段是数据库中的主键，唯一标识一个MapInfo
        /// </summary>
        [IsIdentity]
        public string FileName
        {
            get { return _FileName; }
            set
            {
                if (_FileName == value) return;

                var oldValue = _FileName;
                _FileName = value;

                OnChanged(oldValue, value, "FileName");
            }
        }
        private string _FileName;

        /// <summary>
        /// 【地图显示名称】在数据库中存储的描述，例如"比奇城 1"、"沙漠"等
        /// 
        /// 说明：
        /// - ServerDescription：服务器端显示，格式 "FileName - Description"（如 "Lodeight - 比奇城 1"）
        /// - PlayerDescription：客户端显示给玩家，去掉末尾的数字标记（如 "比奇城" 去掉了 " 1"）
        /// - 使用正则表达式 @"\s\d+$" 移除末尾空格和数字
        /// </summary>
        public string Description
        {
            get { return _Description; }
            set
            {
                if (_Description == value) return;

                var oldValue = _Description;
                _Description = value;

                OnChanged(oldValue, value, "Description");
            }
        }
        private string _Description;

        /// <summary>
        /// 【客户端显示用地图名】计算属性，去掉Description末尾的数字
        /// 示例：Description="比奇城 1" → PlayerDescription="比奇城"
        /// </summary>
        [JsonIgnore]
        [IgnoreProperty]
        public string PlayerDescription => TrailingSpaceAndNumberRegex().Replace(Description, string.Empty);

        /// <summary>
        /// 【服务器显示用地图名】计算属性，格式为 "文件名 - 描述"
        /// 示例：FileName="Lodeight" Description="比奇城 1" → ServerDescription="Lodeight - 比奇城 1"
        /// </summary>
        [JsonIgnore]
        [IgnoreProperty]
        public string ServerDescription => $"{FileName} - {Description}";

        /// <summary>
        /// 【小地图资源索引】指向 MiniMap.Zl 资源文件中的图像索引
        /// 
        /// 图像资源加载流程：
        /// 1. 客户端加载资源库：LibraryFile.MiniMap → Data/MiniMap.Zl
        /// 2. MapInfo.MiniMap 存储该图像库中的图像索引号
        /// 3. 当玩家进入地图时：
        ///    - 服务器向客户端发送 S.MapChanged，包含地图的 MapIndex
        ///    - 客户端从 MapInfoList 中找到对应的 MapInfo
        ///    - MiniMapDialog 订阅 MapControl.MapInfoChanged 事件
        ///    - 事件触发时：Image.Index = MapInfo.MiniMap（加载对应的小地图图像）
        /// 
        /// 示例：
        /// - MiniMap = 0 表示小地图显示灰化（无效地图）
        /// - MiniMap = 5 表示加载 MiniMap.Zl 中的第5张图片作为该地图的小地图
        /// </summary>
        public int MiniMap
        {
            get { return _MiniMap; }
            set
            {
                if (_MiniMap == value) return;

                var oldValue = _MiniMap;
                _MiniMap = value;

                OnChanged(oldValue, value, "MiniMap");
            }
        }
        private int _MiniMap;

        /// <summary>
        /// 【地图光照设置】控制地图上的光照效果
        /// 
        /// LightSetting 枚举值：
        /// - Normal：正常光照
        /// - Dark：黑暗（可能需要火焰照明）
        /// - Bright：明亮
        /// 
        /// 客户端光照处理：
        /// - Client/Scenes/Views/MapControl.cs 中的 LLayer (Light Layer) 根据此设置调整光照
        /// </summary>
        public LightSetting Light
        {
            get { return _Light; }
            set
            {
                if (_Light == value) return;

                var oldValue = _Light;
                _Light = value;

                OnChanged(oldValue, value, "Light");
            }
        }
        private LightSetting _Light;

        /// <summary>
        /// 【地图天气设置】控制地图上是否下雪、下雨等天气效果
        /// </summary>
        public Weather Weather
        {
            get { return _Weather; }
            set
            {
                if (_Weather == value) return;

                var oldValue = _Weather;
                _Weather = value;

                OnChanged(oldValue, value, "Weather");
            }
        }
        private Weather _Weather;

        /// <summary>
        /// 【地图PK设置】控制玩家在该地图上的PvP行为
        /// 
        /// FightSetting 枚举值（可能的设置）：
        /// - Safe：安全区，不能PK
        /// - Fight：战斗区，可以PK
        /// - GuildWar：公会战争专用区
        /// </summary>
        public FightSetting Fight
        {
            get { return _Fight; }
            set
            {
                if (_Fight == value) return;

                var oldValue = _Fight;
                _Fight = value;

                OnChanged(oldValue, value, "Fight");
            }
        }
        private FightSetting _Fight;

        /// <summary>
        /// 【是否允许传送戒指(日月戒指)】控制玩家是否可以在此地图使用传送戒指
        /// </summary>
        public bool AllowRT
        {
            get { return _AllowRT; }
            set
            {
                if (_AllowRT == value) return;

                var oldValue = _AllowRT;
                _AllowRT = value;

                OnChanged(oldValue, value, "AllowRT");
            }
        }
        private bool _AllowRT;

        /// <summary>
        /// 【技能冷却延迟】设置此地图上所有技能的额外冷却延迟（单位：毫秒）
        /// 用于控制地图的难度或游戏平衡
        /// </summary>
        public int SkillDelay
        {
            get { return _SkillDelay; }
            set
            {
                if (_SkillDelay == value) return;

                var oldValue = _SkillDelay;
                _SkillDelay = value;

                OnChanged(oldValue, value, "SkillDelay");
            }
        }
        private int _SkillDelay;

        /// <summary>
        /// 【是否允许骑马】控制玩家是否可以在此地图上骑马
        /// </summary>
        public bool CanHorse
        {
            get { return _CanHorse; }
            set
            {
                if (_CanHorse == value) return;

                var oldValue = _CanHorse;
                _CanHorse = value;

                OnChanged(oldValue, value, "CanHorse");
            }
        }
        private bool _CanHorse;

        /// <summary>
        /// 【是否允许传送传送】控制玩家是否可以在此地图上使用传送技能（如瞬移）
        /// </summary>
        public bool AllowTT
        {
            get { return _AllowTT; }
            set
            {
                if (_AllowTT == value) return;

                var oldValue = _AllowTT;
                _AllowTT = value;

                OnChanged(oldValue, value, "AllowTT");
            }
        }
        private bool _AllowTT;

        /// <summary>
        /// 【是否允许挖矿】控制玩家是否可以在此地图上采矿
        /// 相关配置存储在 Mining 集合中，包含具体的采矿点信息
        /// </summary>
        public bool CanMine
        {
            get { return _CanMine; }
            set
            {
                if (_CanMine == value) return;

                var oldValue = _CanMine;
                _CanMine = value;

                OnChanged(oldValue, value, "CanMine");
            }
        }
        private bool _CanMine;

        /// <summary>
        /// 【是否允许夫妻回城符】控制玩家是否可以在此地图上使用夫妻回城符（传送到配偶所在地图）
        /// </summary>
        public bool CanMarriageRecall
        {
            get { return _CanMarriageRecall; }
            set
            {
                if (_CanMarriageRecall == value) return;

                var oldValue = _CanMarriageRecall;
                _CanMarriageRecall = value;

                OnChanged(oldValue, value, "CanMarriageRecall");
            }
        }
        private bool _CanMarriageRecall;

        /// <summary>
        /// 【是否允许回城符】控制玩家是否可以在此地图上使用普通回城符传送回去
        /// </summary>
        public bool AllowRecall
        {
            get => _AllowRecall;
            set
            {
                if (_AllowRecall == value) return;

                bool oldValue = _AllowRecall;
                _AllowRecall = value;

                OnChanged(oldValue, value, "AllowRecall");
            }
        }
        private bool _AllowRecall;

        /// <summary>
        /// 【最低等级限制】玩家等级低于此值无法进入此地图
        /// 示例：MinimumLevel = 30，则等级 < 30 的玩家无法进入
        /// </summary>
        public int MinimumLevel
        {
            get { return _MinimumLevel; }
            set
            {
                if (_MinimumLevel == value) return;

                var oldValue = _MinimumLevel;
                _MinimumLevel = value;

                OnChanged(oldValue, value, "MinimumLevel");
            }
        }
        private int _MinimumLevel;

        /// <summary>
        /// 【最高等级限制】玩家等级高于此值无法进入此地图
        /// 示例：MaximumLevel = 50，则等级 > 50 的玩家无法进入
        /// 通常用于新手保护区或特定等级的地图
        /// </summary>
        public int MaximumLevel
        {
            get { return _MaximumLevel; }
            set
            {
                if (_MaximumLevel == value) return;

                var oldValue = _MaximumLevel;
                _MaximumLevel = value;

                OnChanged(oldValue, value, "MaximumLevel");
            }
        }
        private int _MaximumLevel;

        /// <summary>
        /// 【断线重连地图】当玩家从此地图断线重新连接时，将被传送到哪个地图
        /// 
        /// 如果为 null，则使用默认的回城点
        /// 如果指定了值，则玩家断线后重新登录时会被传送到 ReconnectMap
        /// 
        /// 示例应用场景：
        /// - 野外 PvP 地图：设置为 ReconnectMap = 安全区地图
        /// - 副本地图：设置为对应的副本入口地图
        /// </summary>
        public MapInfo ReconnectMap
        {
            get { return _ReconnectMap; }
            set
            {
                if (_ReconnectMap == value) return;

                var oldValue = _ReconnectMap;
                _ReconnectMap = value;

                OnChanged(oldValue, value, "ReconnectMap");
            }
        }
        private MapInfo _ReconnectMap;

        /// <summary>
        /// 【地图背景音乐】指向 SoundIndex 枚举中的音乐文件编号
        /// 
        /// 客户端处理：
        /// - MapControl.OnMapInfoChanged() 中
        /// - 旧地图：DXSoundManager.Stop(oldMap.Music)
        /// - 新地图：DXSoundManager.Play(newMap.Music)
        /// 
        /// 示例：Music = SoundIndex.Village 表示播放城镇背景音乐
        /// </summary>
        public SoundIndex Music
        {
            get { return _Music; }
            set
            {
                if (_Music == value) return;

                var oldValue = _Music;
                _Music = value;

                OnChanged(oldValue, value, "Music");
            }
        }
        private SoundIndex _Music;

        /// <summary>
        /// 【地图背景图像索引】指向背景图像资源库中的图像索引
        /// 
        /// 客户端渲染时：
        /// - MapControl.DrawBackground() 方法会检查此值
        /// - 如果 Background > 0，则加载该索引的背景图像
        /// - 背景图像会在所有地形之下显示
        /// </summary>
        public int Background
        {
            get { return _Background; }
            set
            {
                if (_Background == value) return;

                var oldValue = _Background;
                _Background = value;

                OnChanged(oldValue, value, "Background");
            }
        }
        private int _Background;

        //================================ DO NOT USE - 已弃用的字段 ================================
        // 以下字段已在新版本中弃用，不再使用。保留是为了向后兼容性。

        /// <summary>
        /// 【已弃用】怪物基础生命值
        /// 说明：此字段已弃用。个别怪物的生命值应通过 MonsterInfo.Health 进行配置
        /// </summary>
        public int MonsterHealth
        {
            get { return _MonsterHealth; }
            set
            {
                if (_MonsterHealth == value) return;

                var oldValue = _MonsterHealth;
                _MonsterHealth = value;

                OnChanged(oldValue, value, "MonsterHealth");
            }
        }
        private int _MonsterHealth;

        /// <summary>
        /// 【已弃用】怪物基础伤害
        /// 说明：此字段已弃用。个别怪物的伤害应通过 MonsterInfo.Damage 进行配置
        /// </summary>
        public int MonsterDamage
        {
            get { return _MonsterDamage; }
            set
            {
                if (_MonsterDamage == value) return;

                var oldValue = _MonsterDamage;
                _MonsterDamage = value;

                OnChanged(oldValue, value, "MonsterDamage");
            }
        }
        private int _MonsterDamage;

        /// <summary>
        /// 【已弃用】物品掉落倍数
        /// 说明：此字段已弃用。掉落倍数应通过全局或动态配置进行调整
        /// </summary>
        public int DropRate
        {
            get { return _DropRate; }
            set
            {
                if (_DropRate == value) return;

                var oldValue = _DropRate;
                _DropRate = value;

                OnChanged(oldValue, value, "DropRate");
            }
        }
        private int _DropRate;

        /// <summary>
        /// 【已弃用】经验值倍数
        /// 说明：此字段已弃用。经验倍数应通过全局或动态配置进行调整
        /// </summary>
        public int ExperienceRate
        {
            get { return _ExperienceRate; }
            set
            {
                if (_ExperienceRate == value) return;

                var oldValue = _ExperienceRate;
                _ExperienceRate = value;

                OnChanged(oldValue, value, "ExperienceRate");
            }
        }
        private int _ExperienceRate;

        /// <summary>
        /// 【已弃用】金币掉落倍数
        /// 说明：此字段已弃用。金币倍数应通过全局或动态配置进行调整
        /// </summary>
        public int GoldRate
        {
            get { return _GoldRate; }
            set
            {
                if (_GoldRate == value) return;

                var oldValue = _GoldRate;
                _GoldRate = value;

                OnChanged(oldValue, value, "GoldRate");
            }
        }
        private int _GoldRate;

        /// <summary>
        /// 【已弃用】怪物最大生命值
        /// </summary>
        public int MaxMonsterHealth
        {
            get { return _MaxMonsterHealth; }
            set
            {
                if (_MaxMonsterHealth == value) return;

                var oldValue = _MaxMonsterHealth;
                _MaxMonsterHealth = value;

                OnChanged(oldValue, value, "MaxMonsterHealth");
            }
        }
        private int _MaxMonsterHealth;

        /// <summary>
        /// 【已弃用】怪物最大伤害
        /// </summary>
        public int MaxMonsterDamage
        {
            get { return _MaxMonsterDamage; }
            set
            {
                if (_MaxMonsterDamage == value) return;

                var oldValue = _MaxMonsterDamage;
                _MaxMonsterDamage = value;

                OnChanged(oldValue, value, "MaxMonsterDamage");
            }
        }
        private int _MaxMonsterDamage;

        /// <summary>
        /// 【已弃用】物品最大掉落倍数
        /// </summary>
        public int MaxDropRate
        {
            get { return _MaxDropRate; }
            set
            {
                if (_MaxDropRate == value) return;

                var oldValue = _MaxDropRate;
                _MaxDropRate = value;

                OnChanged(oldValue, value, "MaxDropRate");
            }
        }
        private int _MaxDropRate;

        /// <summary>
        /// 【已弃用】经验最大倍数
        /// </summary>
        public int MaxExperienceRate
        {
            get { return _MaxExperienceRate; }
            set
            {
                if (_MaxExperienceRate == value) return;

                var oldValue = _MaxExperienceRate;
                _MaxExperienceRate = value;

                OnChanged(oldValue, value, "MaxExperienceRate");
            }
        }
        private int _MaxExperienceRate;

        /// <summary>
        /// 【已弃用】金币最大掉落倍数
        /// </summary>
        public int MaxGoldRate
        {
            get { return _MaxGoldRate; }
            set
            {
                if (_MaxGoldRate == value) return;

                var oldValue = _MaxGoldRate;
                _MaxGoldRate = value;

                OnChanged(oldValue, value, "MaxGoldRate");
            }
        }
        private int _MaxGoldRate;

        //================================ 关联数据 - 与MapInfo相关的其他游戏内容 ================================

        /// <summary>
        /// 【副本关联】此地图所属的副本实例
        /// 
        /// 说明：
        /// - 如果此字段为 null，则这是一个常规地图（如城镇、野外）
        /// - 如果指定了 InstanceInfo，则此地图只能在特定的副本实例中创建
        /// - 副本地图会根据 InstanceInfo 的配置（如时间限制）进行特殊处理
        /// </summary>
        [JsonIgnore]
        [Association("Maps")]
        public InstanceInfo Instance
        {
            get { return _Instance; }
            set
            {
                if (_Instance == value) return;

                var oldValue = _Instance;
                _Instance = value;

                OnChanged(oldValue, value, "Instance");
            }
        }
        private InstanceInfo _Instance;

        /// <summary>
        /// 【职业限制】该地图是否限制特定职业进入
        /// 
        /// RequiredClass 枚举值：
        /// - None：无限制，所有职业可进入
        /// - Warrior：仅战士可进入
        /// - Wizard：仅法师可进入
        /// - Taoist：仅道士可进入
        /// 等等
        /// </summary>
        public RequiredClass RequiredClass
        {
            get => _requiredClass;
            set
            {
                if (_requiredClass == value) return;
                var oldValue = _requiredClass;
                _requiredClass = value;
                OnChanged(oldValue, value, nameof(RequiredClass));
            }
        }
        private RequiredClass _requiredClass;

        /// <summary>
        /// 【守卫列表】该地图上的NPC守卫信息
        /// 
        /// GuardInfo 包含：
        /// - Monster：守卫的怪物类型（指向 MonsterInfo）
        /// - X, Y：守卫在地图上的坐标
        /// - Direction：守卫朝向
        /// 
        /// 服务器启动时：Map.Setup() → CreateGuards() → 遍历此集合生成守卫
        /// </summary>
        [Association("Guards", true)]
        public DBBindingList<GuardInfo> Guards { get; set; }

        /// <summary>
        /// 【区域列表】该地图上的游戏区域（MapRegion）
        /// 
        /// MapRegion 包含：
        /// - PointArray 或 BitArray：该区域在地图上的坐标范围
        /// - RegionType：区域类型（如安全区、任务区等）
        /// - NPC、怪物生成点等信息
        /// 
        /// 用途：
        /// - NPC/怪物的区域限制
        /// - 地图上的视觉标记（在编辑器中显示区域范围）
        /// - 游戏逻辑（如判断是否在安全区）
        /// </summary>
        [JsonIgnore]
        [Association("Regions", true)]
        public DBBindingList<MapRegion> Regions { get; set; }

        /// <summary>
        /// 【采矿点列表】该地图上的采矿点位置
        /// 
        /// MineInfo 包含：
        /// - X, Y：采矿点在地图上的坐标
        /// - MineType：采矿类型（铁矿、金矿等）
        /// 
        /// 服务器处理：
        /// - 玩家接近采矿点时可以采矿
        /// - 采矿点会定期刷新
        /// - CanMine 字段必须为 true，此列表才有效
        /// </summary>
        [Association("Mining", true)]
        public DBBindingList<MineInfo> Mining { get; set; }

        /// <summary>
        /// 【城堡列表】该地图上的城堡信息（用于攻城战）
        /// 
        /// CastleInfo 包含：
        /// - CastleFlags：城堡的旗帜（用于控制城堡所有权）
        /// - CastleGates：城堡的大门
        /// - CastleGuards：城堡的守卫
        /// - GuildInfo：占据此城堡的公会
        /// 
        /// 服务器处理：
        /// - Map.Setup() → CreateCastleFlags/Gates/Guards()
        /// - 遍历 Castles 集合生成城堡相关对象
        /// </summary>
        [Association("Castles", true)]
        public DBBindingList<CastleInfo> Castles { get; set; }

        /// <summary>
        /// 【地图增强属性】该地图上应用的Buff属性
        /// 
        /// MapInfoStat 包含：
        /// - Stat：属性类型（如增加 MaxHealth、MaxMana 等）
        /// - Amount：属性增加值
        /// 
        /// 示例：
        /// - 地图可以配置为"该地图上所有玩家的生命值 +100"
        /// - 或者"该地图上的怪物伤害 +50%"
        /// 
        /// 处理流程：
        /// - OnLoaded() → StatsChanged() → 将所有 BuffStats 聚合到 Stats 对象中
        /// </summary>
        [Association("MapInfoStats", true)]
        public DBBindingList<MapInfoStat> BuffStats { get; set; }

        /// <summary>
        /// 【最终的属性统计对象】所有 BuffStats 的聚合结果
        /// 用于游戏逻辑中快速查询该地图的全局属性修改
        /// </summary>
        public Stats Stats = new();

        /// <summary>
        /// 【MapInfo 数据库对象生命周期 - OnCreated】
        /// 当新创建的 MapInfo 对象首次保存到数据库时调用
        /// 
        /// 功能：设置新地图的默认值
        /// - AllowRT = true：允许传送戒指
        /// - AllowTT = true：允许传送技能
        /// - CanMarriageRecall = true：允许夫妻回城符
        /// - AllowRecall = true：允许回城符
        /// </summary>
        protected internal override void OnCreated()
        {
            base.OnCreated();

            AllowRT = true;
            AllowTT = true;
            CanMarriageRecall = true;
            AllowRecall = true;
        }

        /// <summary>
        /// 【MapInfo 数据库对象生命周期 - OnLoaded】
        /// 当 MapInfo 从数据库加载时调用
        /// 
        /// 功能：初始化计算属性和关联数据
        /// - 调用 StatsChanged() 将所有 BuffStats 聚合到 Stats 对象
        /// 
        /// 调用时机：
        /// 1. 服务器启动 SEnvir.LoadDatabase() 加载所有地图配置时
        /// 2. 客户端加载 MapInfoList 时
        /// </summary>
        protected internal override void OnLoaded()
        {
            base.OnLoaded();

            StatsChanged();
        }

        /// <summary>
        /// 【属性统计更新方法】
        /// 
        /// 功能：将 BuffStats 集合中的所有属性修改聚合到 Stats 对象中
        /// 
        /// 流程：
        /// 1. 清空 Stats 对象（Stats.Clear()）
        /// 2. 遍历 BuffStats 集合中的每个 MapInfoStat
        /// 3. 将每个属性的增量累加到 Stats 对象中
        ///    Stats[stat.Stat] += stat.Amount
        /// 
        /// 示例：
        /// 如果 BuffStats 中有：
        /// - MaxHealth += 100
        /// - MaxMana += 50
        /// 那么处理后 Stats 对象中会有这些属性及其值
        /// </summary>
        public void StatsChanged()
        {
            Stats.Clear();
            foreach (MapInfoStat stat in BuffStats)
                Stats[stat.Stat] += stat.Amount;
        }

        //================================ 客户端变量 ================================

        /// <summary>
        /// 【客户端UI展开状态】在编辑器或客户端UI中，地图树形结构是否展开
        /// 仅在客户端显示层使用，服务器不关心此值
        /// </summary>
        public bool Expanded = true;

        /// <summary>
        /// 【正则表达式工具方法】移除字符串末尾的空格和数字
        /// 
        /// 用途：从 Description 中提取 PlayerDescription
        /// 示例："比奇城 1" → "比奇城"（移除了 " 1"）
        /// 
        /// 正则模式：@"\s\d+$"
        /// - \s：匹配任何空白字符（空格、制表符等）
        /// - \d+：匹配一个或多个数字
        /// - $：匹配字符串末尾
        /// </summary>
        [GeneratedRegex(@"\s\d+$", RegexOptions.Compiled)]
        public static partial Regex TrailingSpaceAndNumberRegex();
    }


    /// <summary>
    /// 【地图属性增强类】MapInfoStat - 存储单个地图的属性修改
    /// 
    /// 关系图：
    /// MapInfo.BuffStats (集合) → MapInfoStat (单个属性修改)
    /// 
    /// 数据库存储关系（一对多）：
    /// - MapInfo（主表）: FileName 唯一性标识
    /// - MapInfoStat（从表）: Map + Stat 组成复合主键（IsIdentity）
    /// 
    /// 示例数据库表结构：
    /// MapInfoStats 表：
    /// | Map     | Stat           | Amount |
    /// |---------|----------------|--------|
    /// | Lodeight| MaxHealth      | 100    |
    /// | Lodeight| MaxMana        | 50     |
    /// | Lodeight| DefenseRating  | 10     |
    /// </summary>
    public sealed class MapInfoStat : DBObject
    {
        /// <summary>
        /// 【地图引用】指向此属性修改所属的地图
        /// 
        /// [IsIdentity]：表示此字段是复合主键的一部分
        /// [Association("MapInfoStats")]：表示与 MapInfo.BuffStats 的一对多关系
        /// 
        /// 数据库处理：
        /// - 当删除 MapInfo 时，与它关联的所有 MapInfoStat 也会被删除（级联删除）
        /// </summary>
        [IsIdentity]
        [Association("MapInfoStats")]
        public MapInfo Map
        {
            get { return _Map; }
            set
            {
                if (_Map == value) return;

                var oldValue = _Map;
                _Map = value;

                OnChanged(oldValue, value, "Map");
            }
        }
        private MapInfo _Map;

        /// <summary>
        /// 【属性类型】指定这个修改是针对哪个属性的
        /// 
        /// Stat 枚举值包括（示例）：
        /// - Health：生命值
        /// - Mana：魔法值
        /// - DefenseRating：防御
        /// - MagicResistance：魔法抵抗
        /// - ExperienceGain：经验获取倍数
        /// 等等
        /// 
        /// [IsIdentity]：此字段与 Map 字段共同组成复合主键
        /// </summary>
        [IsIdentity]
        public Stat Stat
        {
            get { return _Stat; }
            set
            {
                if (_Stat == value) return;

                var oldValue = _Stat;
                _Stat = value;

                OnChanged(oldValue, value, "Stat");
            }
        }
        private Stat _Stat;

        /// <summary>
        /// 【属性增加值】该属性在此地图上的增加量
        /// 
        /// 示例：
        /// - Amount = 100：该属性增加 100（绝对值）
        /// - Amount = -50：该属性减少 50
        /// 
        /// 在游戏逻辑中的应用：
        /// 当玩家进入此地图时，游戏引擎会将此值应用到玩家的属性计算中
        /// 例如：玩家在地图上的最大生命值 = 基础最大生命值 + 100
        /// </summary>
        public int Amount
        {
            get { return _Amount; }
            set
            {
                if (_Amount == value) return;

                var oldValue = _Amount;
                _Amount = value;

                OnChanged(oldValue, value, "Amount");
            }
        }
        private int _Amount;
    }
}
