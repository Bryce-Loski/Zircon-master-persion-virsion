using MirDB;

namespace Library.SystemModels
{
    /// <summary>
    /// 【守卫信息类】GuardInfo - 存储地图上的守卫（NPC）配置
    /// 
    /// 功能说明：
    /// 1. 定义地图上一个固定的守卫/NPC的所有属性
    /// 2. 与 MapInfo 形成一对多关系（一个地图有多个守卫）
    /// 3. 与 MonsterInfo 关联（指定守卫是哪种怪物类型）
    /// 4. 存储守卫的位置和朝向信息
    /// 
    /// 数据存储：
    /// - 数据库表：GuardInfo 表
    /// - 关联表：每个 GuardInfo 记录属于某个 MapInfo
    /// - 主键：(Map, Monster, X, Y) 复合主键（[IsIdentity] 标记）
    /// 
    /// 生成流程：
    /// 【数据库加载】→ MapInfo.Guards 集合包含所有 GuardInfo
    ///            ↓
    /// 【服务器启动】→ Map.Setup() → CreateGuards()
    ///            ↓
    /// 【遍历 GuardInfo】→ MonsterObject.GetMonster(info.Monster)
    ///            ↓
    /// 【设置属性】→ mob.Direction = info.Direction
    ///            ↓
    /// 【生成到地图】→ mob.Spawn(this, new Point(info.X, info.Y))
    /// 
    /// 示例：
    /// 比奇城地图可能有：
    /// - GuardInfo 1: Monster=卫兵, X=100, Y=100, Direction=East
    /// - GuardInfo 2: Monster=卫兵, X=150, Y=100, Direction=West
    /// - GuardInfo 3: Monster=城主, X=200, Y=150, Direction=North
    /// 
    /// 服务器启动时会自动创建这3个守卫对象并放置在地图上
    /// </summary>
    public sealed class GuardInfo : DBObject
    {
        /// <summary>
        /// 【地图引用】此守卫所属的地图
        /// 
        /// [IsIdentity]：表示此字段是复合主键的一部分
        /// [Association("Guards")]：表示与 MapInfo.Guards 的多对一关系
        /// 
        /// 数据库关系：
        /// - 一个 MapInfo 有多个 GuardInfo（一对多）
        /// - 当删除 MapInfo 时，与其关联的所有 GuardInfo 也会被删除（级联删除）
        /// - 当修改此字段时，GuardInfo 会移动到新的地图
        /// 
        /// 示例：
        /// GuardInfo.Map = MapInfoList["Lodeight"]  // 这个守卫属于比奇城
        /// </summary>
        [IsIdentity]
        [Association("Guards")]
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
        /// 【怪物类型】此守卫是什么怪物/NPC
        /// 
        /// [IsIdentity]：表示此字段是复合主键的一部分
        /// 
        /// 关键说明：
        /// - 指向 MonsterInfo 对象，定义守卫的外观、AI、属性、掉落等
        /// - 不是一个新建怪物，而是引用现有的怪物配置模板
        /// - 不同的 GuardInfo 可以引用相同的 MonsterInfo（多个守卫是同一种怪物）
        /// 
        /// 生成流程：
        /// 1. 服务器加载 GuardInfo 时获取 Monster 字段
        /// 2. 调用 MonsterObject.GetMonster(info.Monster)
        ///    创建该怪物类型的新实例
        /// 3. 设置实例的方向和位置
        /// 4. 添加到地图
        /// 
        /// 示例数据库记录：
        /// GuardInfo {
        ///   Map: "Lodeight"
        ///   Monster: 104  // MonsterInfo.Index = 104（卫兵怪物）
        ///   X: 100, Y: 100
        ///   Direction: East
        /// }
        /// 
        /// 错误处理：
        /// - 如果 Monster 为 null → 生成失败，记录错误日志
        /// - MonsterInfo 可能被删除或编辑，需要检查有效性
        /// </summary>
        [IsIdentity]
        public MonsterInfo Monster
        {
            get { return _Monster; }
            set
            {
                if (_Monster == value) return;

                var oldValue = _Monster;
                _Monster = value;

                OnChanged(oldValue, value, "Monster");
            }
        }
        private MonsterInfo _Monster;

        /// <summary>
        /// 【守卫的X坐标】在地图上的水平位置
        /// 
        /// [IsIdentity]：表示此字段是复合主键的一部分
        /// 
        /// 说明：
        /// - 范围：0 到 Map.Width - 1
        /// - 单位：Cell（地图网格单位，每个Cell约48像素宽）
        /// - 与 Y 坐标组成二维位置
        /// 
        /// 坐标有效性检查：
        /// - 必须是 ValidCell（可行走的位置）
        /// - 如果 (X, Y) 指向不可行走的 Cell → 生成失败
        /// 
        /// 示例：
        /// - X=100 表示地图的第100列
        /// - 在 MapControl 中对应 Point(100, Y)
        /// </summary>
        [IsIdentity]
        public int X
        {
            get { return _X; }
            set
            {
                if (_X == value) return;

                var oldValue = _X;
                _X = value;

                OnChanged(oldValue, value, "X");
            }
        }
        private int _X;

        /// <summary>
        /// 【守卫的Y坐标】在地图上的竖直位置
        /// 
        /// [IsIdentity]：表示此字段是复合主键的一部分
        /// 
        /// 说明：
        /// - 范围：0 到 Map.Height - 1
        /// - 单位：Cell（地图网格单位，每个Cell约32像素高）
        /// - 与 X 坐标组成二维位置
        /// 
        /// 生成检验：
        /// - Cells[X, Y] 必须存在且有效
        /// - 如果为 null → mob.Spawn() 返回 false → 生成失败
        /// 
        /// 示例：
        /// - Y=100 表示地图的第100行
        /// - 在 MapControl 中对应 Point(X, 100)
        /// 
        /// 特殊情况：
        /// - 守卫可能被放在 ValidCell 上（使用 Cell.GetMovement() 获取最近的可通行位置）
        /// </summary>
        [IsIdentity]
        public int Y
        {
            get { return _Y; }
            set
            {
                if (_Y == value) return;

                var oldValue = _Y;
                _Y = value;

                OnChanged(oldValue, value, "Y");
            }
        }
        private int _Y;

        /// <summary>
        /// 【守卫的朝向】守卫面向的方向
        /// 
        /// MirDirection 枚举值：
        /// - North：向上
        /// - NorthEast：向右上
        /// - East：向右
        /// - SouthEast：向右下
        /// - South：向下
        /// - SouthWest：向左下
        /// - West：向左
        /// - NorthWest：向左上
        /// 
        /// 设置流程：
        /// 1. 从数据库读取 Direction 字段
        /// 2. 创建怪物对象：mob = MonsterObject.GetMonster(Monster)
        /// 3. 设置方向：mob.Direction = info.Direction
        /// 4. 生成到地图：mob.Spawn(this, new Point(X, Y))
        /// 
        /// 用途：
        /// - 影响怪物的视觉显示（朝向的动画帧）
        /// - 可能影响怪物的AI行为（某些怪物可能根据朝向进行动作）
        /// - 玩家看到守卫时的初始朝向
        /// 
        /// 示例：
        /// - Direction = MirDirection.East 表示守卫面向右边
        /// - 通常会选择让守卫面向路口或重要位置
        /// </summary>
        public MirDirection Direction
        {
            get { return _Direction; }
            set
            {
                if (_Direction == value) return;

                var oldValue = _Direction;
                _Direction = value;

                OnChanged(oldValue, value, "Direction");
            }
        }
        private MirDirection _Direction;
    }
}
