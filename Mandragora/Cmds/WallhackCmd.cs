using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.Usables.Scp1344;
using Mandragora.Managers;
using Mirror;

namespace Mandragora.Cmds
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class WallhackCmd: ICommand
    {
        public string Command { get; } = "wallhack";
        public string[] Aliases { get; } = { "awh", "esp" };
        public string Description { get; } = "Grants a locally visible wallhack. Only you can see that. Overwatch is required";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (PluginFeature.AdminWallhackCmd.IsKillswitched())
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

            if (EntryPoint.Instance.OWManager.UseridsWallhackRequests.Contains(player.UserId))
            {
                OverwatchFeaturesManager.DisableVisuals(player);
                EntryPoint.Instance.OWManager.UseridsWallhackRequests.Remove(player.UserId);
                response = "Wallhack is <color=orange>disabled</color>";
                return true;
            }

            if (player.Role.Type != PlayerRoles.RoleTypeId.Overwatch)
            {
                response = "You must be in overwatch";
                return false;
            }

            OverwatchFeaturesManager.EnableVisuals(player);
            EntryPoint.Instance.OWManager.UseridsWallhackRequests.Add(player.UserId);
            response = $"Wallhack is <color=green>enabled</color>";
            return true;
        }
    }
}
