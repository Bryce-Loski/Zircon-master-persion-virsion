using Library;
using Server.Models;
using System;
using S = Library.Network.ServerPackets;

// 说明（给新手）：
// - 这个类定义了一个管理员命令 "增加属性"，允许管理员给玩家的装备增加指定的属性值。
// - 命令格式：AddStat <EquipmentSlot> <Stat> <Amount>
//   例如：AddStat Weapon MaxDC 50
// - 该命令会检查参数的有效性，找到玩家指定的装备槽位，如果装备存在，则增加指定的属性值，并通知玩家属性已更新。
namespace Server.Envir.Commands.Command.Admin
{
    class AddStat : AbstractParameterizedCommand<IAdminCommand>
    {
        public override string VALUE => "AddStat";
        public override int PARAMS_LENGTH => 4;

        public override void Action(PlayerObject player, string[] vals)
        {
            if (vals.Length < PARAMS_LENGTH)
                ThrowNewInvalidParametersException(); //AddStat Weapon MaxDC 50

            if (!Enum.TryParse(vals[1], true, out EquipmentSlot tslot))
                ThrowNewInvalidParametersException();
            if (!Enum.TryParse(vals[2], true, out Stat tstat))
                ThrowNewInvalidParametersException();
            if (!int.TryParse(vals[3], out int tamount))
                ThrowNewInvalidParametersException();

            if (player.Equipment[(int)tslot] != null)
            {
                player.Equipment[(int)tslot].AddStat(tstat, tamount, StatSource.Added);
                player.Equipment[(int)tslot].StatsChanged();

                player.SendShapeUpdate();
                player.RefreshStats();

                player.Enqueue(new S.ItemStatsRefreshed { GridType = GridType.Equipment, Slot = (int)tslot, NewStats = new Stats(player.Equipment[(int)tslot].Stats) });
                player.Connection.ReceiveChat(string.Format("{0} added {1} + {2}", player.Equipment[(int)tslot].Info.ItemName, tstat, tamount), MessageType.System);
            }
        }
    }
}
