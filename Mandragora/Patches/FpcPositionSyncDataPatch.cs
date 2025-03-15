using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;
using Exiled.Events.EventArgs.Interfaces;
using Exiled.Events.Features;
using Exiled.API.Features;
using UnityEngine;

namespace Mandragora.Patches
{
    [HarmonyPatch()]
    class FpcPositionSyncDataPatch
    {
        public static Event<EventArgs> FpcDataToClient { get; set; } = new Event<EventArgs>();

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(FpcServerPositionDistributor), nameof(FpcServerPositionDistributor.GetNewSyncData))]
        private static bool GetNewSyncData(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm, bool isInvisible, ref FpcSyncData __result)
        {
            var curPos = target.transform.position;
            var ev = new EventArgs(receiver, target, fpmm, isInvisible, __result, curPos);
            FpcDataToClient.InvokeSafely(ev);

            isInvisible = ev.IsInvisible;
            curPos = ev.TargetPosition;

            FpcSyncData prevSyncData = FpcServerPositionDistributor.GetPrevSyncData(receiver, target);
            FpcSyncData fpcSyncData = (isInvisible ? default : new FpcSyncData(prevSyncData, fpmm.SyncMovementState, fpmm.IsGrounded, new RelativePosition(curPos), fpmm.MouseLook));
            FpcServerPositionDistributor.PreviouslySent[receiver.netId][target.netId] = fpcSyncData;
            __result = fpcSyncData;
            return false;
        }

        public class EventArgs
        {
            public EventArgs(ReferenceHub receiver, ReferenceHub target, FirstPersonMovementModule fpmm, bool isInvisible, FpcSyncData instance, Vector3 targetPos)
            {
                Receiver = Player.Get(receiver);
                Target = Player.Get(target);
                Fpmm = fpmm;
                IsInvisible = isInvisible;
                Instance = instance;
                TargetPosition = targetPos;
            }

            public Player Receiver { get; }
            public Player Target { get; }
            public FirstPersonMovementModule Fpmm { get; }
            public bool IsInvisible { get; set; }
            public Vector3 TargetPosition { get; set; }
            public FpcSyncData Instance { get; }
        }
    }
}
