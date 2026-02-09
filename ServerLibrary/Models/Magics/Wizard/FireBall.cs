using Library;
using Server.DBModels;
using System.Collections.Generic;
using System.Drawing;

namespace Server.Models.Magics
{
    /// <summary>
    /// 火球术 - 法师基础火系单体攻击技能
    /// 发射一个火球攻击目标，造成火属性魔法伤害
    /// 可通过 Burning（燃烧）技能增强，附加燃烧效果
    /// </summary>
    [MagicType(MagicType.FireBall)]
    public class FireBall : MagicObject
    {
        /// <summary>
        /// 技能元素类型 - 火属性
        /// 决定了伤害类型和元素抗性计算
        /// </summary>
        protected override Element Element => Element.Fire;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="player">施法的玩家对象</param>
        /// <param name="magic">玩家的技能数据</param>
        public FireBall(PlayerObject player, UserMagic magic) : base(player, magic)
        {

        }

        /// <summary>
        /// 计算燃烧伤害值
        /// 如果玩家学习了 Burning（燃烧）增益技能，则使用增益技能的威力值
        /// 否则返回默认的燃烧伤害
        /// </summary>
        /// <param name="burn">基础燃烧伤害</param>
        /// <param name="stats">目标属性（可选）</param>
        /// <returns>最终燃烧伤害值</returns>
        public override int GetBurn(int burn, Stats stats = null)
        {
            var burning = GetAugmentedSkill(MagicType.Burning);

            if (burning != null)
            {
                return burning.GetPower();
            }

            return base.GetBurn(burn, stats);
        }

        /// <summary>
        /// 计算燃烧等级
        /// 如果玩家学习了 Burning（燃烧）增益技能，燃烧等级 = 增益技能等级 + 1
        /// 燃烧等级影响燃烧效果的持续时间和强度
        /// </summary>
        /// <param name="burnLevel">基础燃烧等级</param>
        /// <param name="stats">目标属性（可选）</param>
        /// <returns>最终燃烧等级</returns>
        public override int GetBurnLevel(int burnLevel, Stats stats = null)
        {
            var burning = GetAugmentedSkill(MagicType.Burning);

            if (burning != null)
            {
                return burning.Level + 1;
            }

            return base.GetBurnLevel(burnLevel, stats);
        }

        /// <summary>
        /// 施放火球术
        /// 玩家点击技能后调用此方法，执行技能施放逻辑
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="location">目标位置</param>
        /// <param name="direction">施法方向</param>
        /// <returns>技能施放响应数据，包含目标和位置信息</returns>
        public override MagicCast MagicCast(MapObject target, Point location, MirDirection direction)
        {
            var response = new MagicCast
            {
                Ob = target
            };

            if (!Player.CanAttackTarget(target))
            {
                response.Ob = null;
                response.Locations.Add(location);
                return response;
            }

            response.Targets.Add(target.ObjectID);

            var delay = GetDelayFromDistance(500, target);

            ActionList.Add(new DelayedAction(delay, ActionType.DelayMagic, Type, target));

            return response;
        }

        /// <summary>
        /// 技能命中后的处理逻辑
        /// 火球飞行到目标后，延迟动作触发此方法，执行伤害计算
        /// </summary>
        /// <param name="data">延迟动作传递的参数，data[1] 为目标对象</param>
        public override void MagicComplete(params object[] data)
        {
            MapObject target = (MapObject)data[1];

            var damage = Player.MagicAttack(new List<MagicType> { Type }, target);

            if (damage > 0)
            {
                var burning = GetAugmentedSkill(MagicType.Burning);

                if (burning != null)
                {
                    Player.LevelMagic(burning);
                }
            }
        }

        /// <summary>
        /// 修改技能威力（加法部分）
        /// 计算公式：基础威力 + 技能等级威力 + 玩家魔法值（MC）
        /// </summary>
        /// <param name="primary">是否为主要攻击</param>
        /// <param name="power">当前威力值</param>
        /// <param name="ob">目标对象</param>
        /// <param name="stats">额外属性</param>
        /// <param name="extra">额外参数</param>
        /// <returns>修改后的威力值</returns>
        public override int ModifyPowerAdditionner(bool primary, int power, MapObject ob, Stats stats = null, int extra = 0)
        {
            power += Magic.GetPower() + Player.GetMC();

            return power;
        }
    }
}
