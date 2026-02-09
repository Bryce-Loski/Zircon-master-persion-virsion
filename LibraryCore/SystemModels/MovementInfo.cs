using MirDB;

namespace Library.SystemModels
{
    // - 本类表示地图上的一个移动连接（MovementInfo），定义了从一个地图区域（SourceRegion）到另一个地图区域（DestinationRegion）的移动路径。
    // - 每个 MovementInfo 可以有一个关联的图标（Icon），以及一些条件（例如需要的物品、需要的刷新点、需要的洞穴、需要的副本等）和效果（Effect）。
    // - 这些条件和效果可以在地图编辑器中设置，以控制玩家在游戏中是否能够使用这个移动连接，以及使用后会发生什么。
    public sealed class MovementInfo : DBObject
    {
        // 说明（给新手）：
        // - 该类的属性都使用了 `OnChanged` 方法来触发属性更改事件，这样在编辑器中修改这些属性时，界面可以及时更新。
        // - `SourceRegion` 和 `DestinationRegion` 是两个地图区域对象，表示移动连接的起点和终点。它们都被标记为 `[IsIdentity]
        [IsIdentity]
        public MapRegion SourceRegion
        {
            get { return _SourceRegion; }
            set
            {
                if (_SourceRegion == value) return;

                var oldValue = _SourceRegion;
                _SourceRegion = value;

                OnChanged(oldValue, value, "SourceRegion");
            }
        }
        private MapRegion _SourceRegion;

        [IsIdentity]
        public MapRegion DestinationRegion
        {
            get { return _DestinationRegion; }
            set
            {
                if (_DestinationRegion == value) return;

                var oldValue = _DestinationRegion;
                _DestinationRegion = value;

                OnChanged(oldValue, value, "DestinationRegion");
            }
        }
        private MapRegion _DestinationRegion;

        // 图标属性，表示在地图上显示的图标类型（例如传送门、楼梯等）
        public MapIcon Icon
        {
            get { return _Icon; }
            set
            {
                if (_Icon == value) return;

                var oldValue = _Icon;
                _Icon = value;

                OnChanged(oldValue, value, "Icon");
            }
        }
        private MapIcon _Icon;

        // 条件和效果属性，表示使用这个移动连接所需的条件（例如需要的物品、需要的刷新点、需要的洞穴、需要的副本等）以及使用后会产生的效果
        public ItemInfo NeedItem
        {
            get { return _NeedItem; }
            set
            {
                if (_NeedItem == value) return;

                var oldValue = _NeedItem;
                _NeedItem = value;

                OnChanged(oldValue, value, "NeedItem");
            }
        }
        private ItemInfo _NeedItem;

        // - `NeedSpawn` 属性表示使用这个移动连接所需的刷新点（RespawnInfo）。如果玩家想要使用这个连接，必须在地图上有一个满足条件的刷新点。
        public RespawnInfo NeedSpawn
        {
            get { return _NeedSpawn; }
            set
            {
                if (_NeedSpawn == value) return;

                var oldValue = _NeedSpawn;
                _NeedSpawn = value;

                OnChanged(oldValue, value, "NeedSpawn");
            }
        }
        private RespawnInfo _NeedSpawn;

        public bool NeedHole
        {
            get { return _NeedHole; }
            set
            {
                if (_NeedHole == value) return;

                var oldValue = _NeedHole;
                _NeedHole = value;

                OnChanged(oldValue, value, "NeedHole");
            }
        }
        private bool _NeedHole;

        public InstanceInfo NeedInstance
        {
            get { return _NeedInstance; }
            set
            {
                if (_NeedInstance == value) return;

                var oldValue = _NeedInstance;
                _NeedInstance = value;

                OnChanged(oldValue, value, "NeedInstance");
            }
        }
        private InstanceInfo _NeedInstance;

        public MovementEffect Effect
        {
            get { return _Effect; }
            set
            {
                if (_Effect == value) return;

                var oldValue = _Effect;
                _Effect = value;

                OnChanged(oldValue, value, "Effect");
            }
        }
        private MovementEffect _Effect;

        public RequiredClass RequiredClass
        {
            get { return _RequiredClass; }
            set
            {
                if (_RequiredClass == value) return;

                var oldValue = _RequiredClass;
                _RequiredClass = value;

                OnChanged(oldValue, value, "RequiredClass");
            }
        }
        private RequiredClass _RequiredClass;

        /// <summary>
        /// Skips valid origin cell validation, allowing for invalid movements to added. Situations such as wanting to add minimap connections for npc movements.
        /// </summary>
        public bool SkipValidation
        {
            get { return _SkipValidation; }
            set
            {
                if (_SkipValidation == value) return;

                var oldValue = _SkipValidation;
                _SkipValidation = value;

                OnChanged(oldValue, value, "SkipValidation");
            }
        }
        private bool _SkipValidation;


        protected internal override void OnCreated()
        {
            base.OnCreated();

            RequiredClass = RequiredClass.All;
        }
    }
}
