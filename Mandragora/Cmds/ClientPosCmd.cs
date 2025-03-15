using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Features;
using Mandragora.Managers;

namespace Mandragora.Cmds
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    class ClientPosCmd : ICommand
    {
        public string Command { get; } = "clientpos";
        public string[] Aliases { get; } = [];
        public string Description { get; } = "Shows the actual position of a player. Can be useful in detecting noclipping";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (PluginFeature.RealClientPosCmd.IsKillswitched())
            {
                response = "Command was disabled by server administrators";
                return false;
            }

            if (!sender.CheckPermission(EntryPoint.Instance.Config.OverwatchCmdsRequiredPermissions, out response))
                return false;

            if (!Player.TryGet(sender, out var player) || player.IsDisconnected())
            {
                response = "You must be a player and be connected to the server";
                return false;
            }

            if (EntryPoint.Instance.OWManager.UseridsClientPosRequests.Contains(player.UserId))
            {
                EntryPoint.Instance.OWManager.UseridsClientPosRequests.Remove(player.UserId);
                response = "Clientpos is <color=orange>disabled</color>";
                return true;
            }

            if (player.Role.Type != PlayerRoles.RoleTypeId.Overwatch)
            {
                response = "You must be in overwatch";
                return false;
            }

            EntryPoint.Instance.OWManager.UseridsClientPosRequests.Add(player.UserId);
            response = $"Clientpos is <color=green>enabled</color>";
            return true;
        }
    }
}
