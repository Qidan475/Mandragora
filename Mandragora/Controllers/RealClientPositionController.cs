using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using Mandragora.Patches;
using MEC;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using RelativePositioning;
using UnityEngine;

namespace Mandragora.Controllers
{
    class RealClientPositionController : IDisposable
    {
        private CoroutineHandle _checker;

        public RealClientPositionController()
        {
            if (PluginFeature.RealClientPosCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStart;
            FpcPositionSyncDataPatch.FpcDataToClient += OnSendingFpcData;
        }

        public void Dispose()
        {
            if (PluginFeature.RealClientPosCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStart;
            FpcPositionSyncDataPatch.FpcDataToClient -= OnSendingFpcData;
        }

        void OnWaitingForPlayers()
        {
            _checker.IsRunning = false;
        }

        void OnRoundStart()
        {
            _checker = CheckServerClientDistanceDiff().RunCoroutine();
        }

        void OnSendingFpcData(FpcPositionSyncDataPatch.EventArgs ev)
        {
            if (ev.Receiver.IsDisconnected() || ev.Target.IsDisconnected())
                return;

            if (!EntryPoint.Instance.OWManager.HasActiveRealClientPos(ev.Receiver))
                return;

            if (ev.Target.Role is not FpcRole targetFpc)
                return;

            ev.TargetPosition = targetFpc.ClientRelativePosition.Position;
            ev.IsInvisible = false;
        }

        private IEnumerator<float> CheckServerClientDistanceDiff()
        {
            while (true)
            {
                foreach (var ply in EntryPoint.Instance.OWManager.ActiveRealClientPos)
                {
                    if (ply.IsDisconnected() || ply.Role is not OverwatchRole owRole)
                        continue;

                    var target = owRole.SpectatedPlayer;
                    if (target.IsDisconnected() || target.Role is not FpcRole targetFpc)
                        continue;

                    var targetClientPos = targetFpc.ClientRelativePosition.Position;
                    var targetServerPos = target.Position;
                    if (Vector3.Distance(targetClientPos, targetServerPos) >= 10f)
                        ply.Broadcast(5, $"[WARNING] {target.Nickname} is trying to visually desync his position. Try to disable the \"clientpos\" command");
                }

                yield return Timing.WaitForSeconds(8f);
            }
        }
    }
}
