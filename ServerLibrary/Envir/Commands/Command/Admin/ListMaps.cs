using Library;
using Library.SystemModels;
using Server.Envir.Commands.Exceptions;
using Server.Models;
using System.Linq;
using System.Text;

namespace Server.Envir.Commands.Command.Admin
{
    class ListMaps : AbstractParameterizedCommand<IAdminCommand>
    {
        public override string VALUE => "LISTMAPS";
        public override int PARAMS_LENGTH => 1;

        public override void Action(PlayerObject player, string[] vals)
        {
            string filter = vals.Length > 1 ? vals[1] : string.Empty;
            
            var maps = SEnvir.MapInfoList.Binding
                .Where(m => string.IsNullOrEmpty(filter) || 
                           m.FileName.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                           m.Description.IndexOf(filter, System.StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(m => m.FileName)
                .Take(50); // 限制最多显示50个，避免刷屏

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("========== 地图列表 ==========");
            sb.AppendLine("格式: [文件名] - [描述]");
            sb.AppendLine("------------------------------");

            int count = 0;
            foreach (var map in maps)
            {
                sb.AppendLine($"{map.FileName} - {map.Description}");
                count++;
            }

            sb.AppendLine("------------------------------");
            sb.AppendLine($"共找到 {count} 个地图");
            if (count >= 50)
                sb.AppendLine("提示: 结果过多，请使用 /LISTMAPS 关键词 过滤");
            sb.AppendLine("用法: /MOVE 文件名或描述 [X] [Y]");
            
            player.Connection.ReceiveChat(sb.ToString(), MessageType.System);
        }
    }
}
