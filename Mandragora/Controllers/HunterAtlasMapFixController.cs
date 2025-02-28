using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Scp106;
using Interactables.Interobjects;
using PlayerRoles.PlayableScps.Scp106;
using UnityEngine;

namespace Mandragora.Controllers
{
    class HunterAtlasMapFixController : IDisposable
    {
        private ConcurrentDictionary<ElevatorChamber, float> _lastElevatorRide = new ConcurrentDictionary<ElevatorChamber, float>();

        public HunterAtlasMapFixController()
        {
            if (PluginFeature.HunterAtlasSurfaceFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Scp106.Teleporting += On106Teleporting;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
            ElevatorChamber.OnElevatorMoved += OnElevatorMoved;
        }

        public void Dispose()
        {
            if (PluginFeature.HunterAtlasSurfaceFix.IsKillswitched())
                return;

            Exiled.Events.Handlers.Scp106.Teleporting -= On106Teleporting;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
            ElevatorChamber.OnElevatorMoved -= OnElevatorMoved;
        }

        void OnWaitingForPlayers()
        {
            _lastElevatorRide.Clear();
        }

        void On106Teleporting(TeleportingEventArgs ev)
        {
            if (!ev.IsAllowed || ev.Player.IsDisconnected())
                return;

            if (ev.Position.y > Scp106Minimap.SurfaceHeightThreshold || ev.Player.Position.y > Scp106Minimap.SurfaceHeightThreshold)
            {
                ev.IsAllowed = false;
                return;
            }

            if (ev.Player.Lift is Lift lift && _lastElevatorRide.TryGetValue(lift.Base, out var lastRideTime))
            {
                bool elevatorWasMovingRecently = lastRideTime < Scp106Minimap.ElevatorCooldown;
                ev.IsAllowed &= !elevatorWasMovingRecently;
            }
        }

        void OnElevatorMoved(Bounds elevatorBounds, ElevatorChamber chamber, Vector3 deltaPos, Quaternion deltaRot)
        {
            _lastElevatorRide.AddOrUpdate(chamber, CurrentGameTime, (elev, prevValue) => CurrentGameTime);
        }

        static float CurrentGameTime => Time.timeSinceLevelLoad;
    }
}
