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
    public class AntiflashCmd : ICommand
    {
        public string Command { get; } = "antiflash";
        public string[] Aliases { get; } = { "aflash" };
        public string Description { get; } = "Disables flashed effect when spectating someone. Only you can see that. Overwatch is required";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (PluginFeature.AdminAntiflashCmd.IsKillswitched())
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

            if (EntryPoint.Instance.OWManager.UseridsAntiflashRequests.Contains(player.UserId))
            {
                EntryPoint.Instance.OWManager.UseridsAntiflashRequests.Remove(player.UserId);
                response = "Antiflash is <color=orange>disabled</color>";
                return true;
            }

            if (player.Role.Type != PlayerRoles.RoleTypeId.Overwatch)
            {
                response = "You must be in overwatch";
                return false;
            }

            EntryPoint.Instance.OWManager.UseridsAntiflashRequests.Add(player.UserId);
            response = $"Antiflash is <color=green>enabled</color>";
            return true;
        }
    }
}
