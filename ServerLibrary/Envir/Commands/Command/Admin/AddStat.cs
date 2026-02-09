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

        // 说明（给新手）：
        // - PARAMS_LENGTH 定义了这个命令需要的参数数量（不包括命令本身）。在这个例子中，AddStat 需要4个参数：命令、装备槽位、属性类型和增加的数值。
        // - 在 Action 方法中会检查参数数量是否正确，如果不够则抛出异常提示管理员输入正确的命令格式。
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

            // 说明（给新手）：
            // - 这里首先检查玩家在指定的装备槽位上是否有装备，如果没有则无法增加属性，命令执行失败。
            // - 如果装备存在，则调用 AddStat 方法增加指定的属性值，并调用 StatsChanged 来更新装备的状态。
            // - 最后，发送更新通知给玩家，并在聊天窗口显示属性增加的消息。
            if (player.Equipment[(int)tslot] != null)
            {
                player.Equipment[(int)tslot].AddStat(tstat, tamount, StatSource.Added);
                player.Equipment[(int)tslot].StatsChanged();

                player.SendShapeUpdate();
                player.RefreshStats();

                // 说明（给新手）：
                // - 这里发送了一个 ItemStatsRefreshed 包给玩家，通知客户端指定装备槽位的属性已经更新，客户端会根据这个包来刷新界面上的属性显示。
                // - 同时在聊天窗口显示一条系统消息，告诉玩家哪个装备增加了哪个属性以及增加了多少数值。
                player.Enqueue(new S.ItemStatsRefreshed { GridType = GridType.Equipment, Slot = (int)tslot, NewStats = new Stats(player.Equipment[(int)tslot].Stats) });
                player.Connection.ReceiveChat(string.Format("{0} added {1} + {2}", player.Equipment[(int)tslot].Info.ItemName, tstat, tamount), MessageType.System);
            }
        }
    }
}
