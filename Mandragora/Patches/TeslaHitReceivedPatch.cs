using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.Features;
using HarmonyLib;
using Mirror;
using PlayerStatsSystem;
using Subtitles;
using UnityEngine;

namespace Mandragora.Patches
{
    [HarmonyPatch]
    class TeslaHitReceivedPatch
    {
        public static Event<TeslaHitEventArgs> TeslaHitReceived { get; set; } = new Event<TeslaHitEventArgs>();

        [HarmonyPrefix()]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(TeslaGateController), nameof(TeslaGateController.ServerReceiveMessage))]
        private static bool ServerReceivedTeslaHit(NetworkConnection conn, TeslaHitMsg msg)
        {
            if (PluginFeature.AntiTeslaFix.IsKillswitched())
                return true;

            if (!ReferenceHub.TryGetHubNetID(conn.identity.netId, out ReferenceHub referenceHub))
                return false;

            if (msg.Gate == null)
            {
                referenceHub.gameConsoleTransmission.SendToClient("Received non-existing tesla gate!", "red");
                return false;
            }
            if (Vector3.Distance(msg.Gate.transform.position, referenceHub.transform.position) > msg.Gate.sizeOfTrigger * 2.2f)
            {
                referenceHub.gameConsoleTransmission.SendToClient("You are too far from a tesla gate!", "red");
                return false;
            }

            TeslaHitReceived.InvokeSafely(new TeslaHitEventArgs(referenceHub, msg.Gate));

            DamageHandlerBase.CassieAnnouncement cassieAnnouncement = new DamageHandlerBase.CassieAnnouncement
            {
                Announcement = "SUCCESSFULLY TERMINATED BY AUTOMATIC SECURITY SYSTEM",
                SubtitleParts = new SubtitlePart[]
                {
                    new SubtitlePart(SubtitleType.TerminatedBySecuritySystem, null)
                }
            };
            referenceHub.playerStats.DealDamage(new UniversalDamageHandler(UnityEngine.Random.Range(200, 300), DeathTranslations.Tesla, cassieAnnouncement));
            return false;
        }

        public class TeslaHitEventArgs
        {
            public TeslaHitEventArgs(ReferenceHub referenceHub, TeslaGate instance)
            {
                ReferenceHub = referenceHub;
                Instance = instance;
            }

            public ReferenceHub ReferenceHub { get; }
            public TeslaGate Instance { get; }
        }
    }
}
