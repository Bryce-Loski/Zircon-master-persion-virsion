using Library;
using System.Collections.Generic;

namespace Server.Helpers
{
    /// <summary>
    /// 技能类型中文名称映射 - 仅包含官方文档中有记录的技能
    /// </summary>
    public static class MagicTypeNames
    {
        /// <summary>
        /// 技能中文名称字典 - 基于"传奇3技能介绍"官方文档
        /// </summary>
        public static readonly Dictionary<MagicType, string> ChineseNames = new Dictionary<MagicType, string>
        {
            // 战士技能
            { MagicType.Swordsmanship, "基本剑术" },
            { MagicType.PotionMastery, "运气术" },
            { MagicType.Slaying, "攻杀剑术" },
            { MagicType.Thrusting, "刺杀剑术" },
            { MagicType.HalfMoon, "半月弯刀" },
            { MagicType.ShoulderDash, "野蛮冲撞" },
            { MagicType.FlamingSword, "烈火剑法" },
            { MagicType.DragonRise, "翔空剑法" },
            { MagicType.BladeStorm, "莲月剑法" },
            { MagicType.DestructiveSurge, "十方斩" },
            { MagicType.Interchange, "乾坤大挪移" },
            { MagicType.Defiance, "铁布衫" },
            { MagicType.Beckon, "斗转星移" },
            { MagicType.Might, "破血狂杀" },
            { MagicType.SwiftBlade, "快刀斩马" },
            { MagicType.Endurance, "金刚之躯" },
            { MagicType.ReflectDamage, "移花接木" },
            { MagicType.Fetter, "泰山压顶" },
            { MagicType.AdvancedPotionMastery, "高级运气术" },
            { MagicType.MassBeckon, "挑衅" },
            { MagicType.SeismicSlam, "天雷锤" },


            // 法师技能
            { MagicType.FireBall, "火球术" },
            { MagicType.LightningBall, "霹雳掌" },
            { MagicType.IceBolt, "冰月神掌" },
            { MagicType.GustBlast, "风掌" },
            { MagicType.Repulsion, "抗拒火环" },
            { MagicType.ElectricShock, "诱惑之光" },
            { MagicType.Teleportation, "瞬息移动" },
            { MagicType.AdamantineFireBall, "大火球" },
            { MagicType.ThunderBolt, "雷电术" },
            { MagicType.IceBlades, "冰月震天" },
            { MagicType.Cyclone, "击风" },
            { MagicType.ScortchedEarth, "地狱火" },
            { MagicType.LightningBeam, "疾光电影" },
            { MagicType.FrozenEarth, "冰沙掌" },
            { MagicType.BlowEarth, "风震天" },
            { MagicType.FireWall, "火墙" },
            { MagicType.ExpelUndead, "圣言术" },
            { MagicType.GeoManipulation, "移形换位" },
            { MagicType.MagicShield, "魔法盾" },
            { MagicType.FireStorm, "爆裂火焰" },
            { MagicType.LightningWave, "地狱雷光" },
            { MagicType.IceStorm, "冰咆哮" },
            { MagicType.DragonTornado, "龙卷风" },
            { MagicType.GreaterFrozenEarth, "魄冰刺" },
            { MagicType.ChainLightning, "怒神霹雳" },
            { MagicType.MeteorShower, "焰天火雨" },
            { MagicType.Renounce, "凝血离魂" },
            { MagicType.Tempest, "旋风墙" },
            { MagicType.JudgementOfHeaven, "天打雷劈" },
            { MagicType.ThunderStrike, "电闪雷鸣" },
            { MagicType.MirrorImage, "分身术" },
            { MagicType.FrostBite, "冰雨" },
            

            // 道士技能
            { MagicType.Heal, "治愈术" },
            { MagicType.SpiritSword, "精神力战法" },
            { MagicType.PoisonDust, "施毒术" },
            { MagicType.ExplosiveTalisman, "灵魂火符" },
            { MagicType.EvilSlayer, "月魂断玉" },
            { MagicType.Invisibility, "隐身术" },
            { MagicType.MagicResistance, "幽灵盾" },
            { MagicType.MassInvisibility, "集体隐身术" },
            { MagicType.GreaterEvilSlayer, "月魂灵波" },
            { MagicType.Resilience, "神圣战甲术" },
            { MagicType.TrapOctagon, "困魔咒" },
            { MagicType.ElementalSuperiority, "强魔震法" },
            { MagicType.MassHeal, "群体治愈术" },
            { MagicType.BloodLust, "猛虎强势" },
            { MagicType.Resurrection, "回生术" },
            { MagicType.Purification, "云寂术" },
            { MagicType.Transparency, "妙影无踪" },
            { MagicType.CelestialLight, "阴阳法环" },
            { MagicType.EmpoweredHealing, "养生术" },
            { MagicType.LifeSteal, "吸星大法" },
            { MagicType.ImprovedExplosiveTalisman, "灭魂火符" },
            { MagicType.ThunderKick, "横扫千军" },
            { MagicType.SummonSkeleton, "召唤骷髅" },
            { MagicType.SummonShinsu, "召唤神兽" },
            { MagicType.SummonJinSkeleton, "超强召唤骷髅" },
            { MagicType.StrengthOfFaith, "移花接玉" },
            { MagicType.SummonDemonicCreature, "焰魔召唤术" },
            { MagicType.DemonExplosion, "魔焰强解术" }
        };

        /// <summary>
        /// 获取技能的中文名称，如果没有映射则返回英文名
        /// </summary>
        public static string GetChineseName(MagicType magicType)
        {
            return ChineseNames.TryGetValue(magicType, out string chineseName) 
                ? chineseName 
                : magicType.ToString();
        }
    }
}
