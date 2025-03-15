using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.Features;
using HarmonyLib;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.FirstPersonControl;
using Exiled.API.Features;

namespace Mandragora.Patches
{
    [HarmonyPatch()]
    public class FpcDataApplyPatch
    {
        public static Event<EventArgs> FpcDataFromClient { get; set; } = new Event<EventArgs>();

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(FpcSyncData), nameof(FpcSyncData.TryApply))]
        private static void TryApply(FpcSyncData __instance, ReferenceHub hub, ref FirstPersonMovementModule module, ref bool bit, ref bool __result)
        {
            if (!__result)
                return;

            var ev = new EventArgs(Player.Get(hub), __instance);
            FpcDataFromClient.InvokeSafely(ev);
        }

        public class EventArgs : IPlayerEvent
        {
            public EventArgs(Player player, FpcSyncData instance)
            {
                Player = player;
                Instance = instance;
            }

            public Player Player { get; }
            public FpcSyncData Instance { get; }
        }
    }
}
