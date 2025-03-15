using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Mandragora.Managers;
using UnityEngine;

namespace Mandragora.Cmds
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    class LocalPosCmd : ICommand
    {
        public string Command { get; } = "localpos";
        public string[] Aliases { get; } = [];
        public string Description { get; } = "Shows the current local position in a room";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (PluginFeature.LocalPositionCmd.IsKillswitched())
            {
                response = "Command was disabled by server administrators";
                return false;
            }

            if (!Player.TryGet(sender, out var player) || player.IsDisconnected())
            {
                response = "You must be a player and be connected to the server";
                return false;
            }

            if (!player.RemoteAdminAccess)
            {
                response = "You must have ra access";
                return false;
            }

            var curRoom = player.CurrentRoom;
            if (curRoom == null)
            {
                response = "room is null";
                return false;
            }

            Vector3 position = player.Position;
            if (curRoom.Type != RoomType.Surface)
                position = curRoom.LocalPosition(position);

            response = $"{curRoom.Type}: {position}";
            return true;
        }
    }
}
