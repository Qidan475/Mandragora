using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using GameCore;
using MEC;
using UnityEngine;
using Log = Exiled.API.Features.Log;

namespace Mandragora.Controllers
{
    class PickingThroughWallFixController : IDisposable
    {
        private List<CheckPosition> _checkPositions = new List<CheckPosition>();

        public PickingThroughWallFixController()
        {
            if (PluginFeature.PickingItemsThroughWallsFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.PickingUpItem += OnPickingUpItem;
        }

        public void Dispose()
        {
            if (PluginFeature.PickingItemsThroughWallsFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Exiled.Events.Handlers.Player.PickingUpItem -= OnPickingUpItem;
        }

        IEnumerator<float> OnWaitingForPlayers()
        {
            Reset();

            yield return Timing.WaitForSeconds(2f);
            SetupCheckPositions();
        }

        private void Reset()
        {
            _checkPositions.Clear();
        }

        private void SetupCheckPositions()
        {
            foreach (var item in EntryPoint.Instance.Config.CheckableRoomPositions)
            {
                var boundsStart = item.BoundsStart.GetWorldPositionFrom(item.Room);
                var boundsEnd = item.BoundsEnd.GetWorldPositionFrom(item.Room);
                var linecastPoint = item.LinecastPoint.GetWorldPositionFrom(item.Room);
                var resolvedPos = new CheckPosition(boundsStart, boundsEnd, linecastPoint);

                _checkPositions.Add(resolvedPos);
            }
        }

        void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Pickup?.GameObject == null || ev.Player.IsDisconnected())
                return;

            var pickupPos = ev.Pickup.Position + (Vector3.up * 0.2f);
            var playerPos = ev.Player.CameraTransform.position;
            foreach (var checkablePos in _checkPositions)
            {
                if (!checkablePos.Bounds.Contains(pickupPos))
                    continue;

                bool hasObstructionToItem = Physics.Linecast(checkablePos.LinecastPoint, pickupPos, EntryPoint.Instance.WallLayerMask);
                if (hasObstructionToItem)
                    break;

                bool hasObstructionToPlayer = Physics.Linecast(checkablePos.LinecastPoint, playerPos, EntryPoint.Instance.WallLayerMask);
                if (hasObstructionToPlayer)
                {
                    ev.IsAllowed = false;
                    ForceResyncPosition(ev.Player);
                }

                break;
            }
        }

        private static void ForceResyncPosition(Player player) => player.Position += Vector3.up * 0.2f;
    }

    public class CheckableRoom
    {
        public RoomType Room { get; set; }
        public Vector3 LinecastPoint { get; set; }
        public Vector3 BoundsStart { get; set; }
        public Vector3 BoundsEnd { get; set; }
    }

    public class CheckPosition
    {
        public Bounds Bounds { get; }
        public Vector3 LinecastPoint { get; }

        public CheckPosition(Vector3 boundsStart, Vector3 boundsEnd, Vector3 linecastPoint)
        {
            var bounds = new Bounds(boundsStart, Vector3.zero);
            bounds.Encapsulate(boundsEnd);
            Bounds = bounds;
            LinecastPoint = linecastPoint;
        }
    }
}
