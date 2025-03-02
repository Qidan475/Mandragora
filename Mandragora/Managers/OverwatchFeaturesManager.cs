using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace Mandragora.Managers
{
    public class OverwatchFeaturesManager
    {
        public List<string> UseridsWallhackRequests { get; set; } = new List<string>();
        public List<string> UseridsAntiflashRequests { get; set; } = new List<string>();
        public List<string> UseridsClientPosRequests { get; set; } = new List<string>();
        public IEnumerable<Player> ActiveWallhack => Player.List.Where(HasActiveWallhack);
        public IEnumerable<Player> ActiveAntiflash => Player.List.Where(HasActiveAntiflash);
        public IEnumerable<Player> ActiveRealClientPos => Player.List.Where(HasActiveAntiflash);

        public bool HasActiveWallhack(Player player) => UseridsWallhackRequests.Contains(player.UserId) && player.IsOverwatchEnabled;
        public bool HasActiveAntiflash(Player player) => UseridsAntiflashRequests.Contains(player.UserId) && player.IsOverwatchEnabled;
        public bool HasActiveRealClientPos(Player player) => UseridsClientPosRequests.Contains(player.UserId) && player.IsOverwatchEnabled;

        public static void EnableVisuals(Player player)
        {
            foreach (var ply in GetAllPlayersAndDummies())
            {
                if (ply == player)
                    continue;

                ply.SendFakeEffectTo(player, Exiled.API.Enums.EffectType.Scp1344, 1);
                ply.SendDetectionMessageTo(player);
            }
        }

        public static void DisableVisuals(Player player)
        {
            foreach (var ply in GetAllPlayersAndDummies())
            {
                if (ply == player)
                    continue;

                ply.ResyncEffectTo(player, Exiled.API.Enums.EffectType.Scp1344);
            }
        }

        public static IEnumerable<Player> GetAllPlayersAndDummies()
        {
            return ReferenceHub.AllHubs.Where(x => !x.IsHost).Select(Player.Get);
        }
    }
}
