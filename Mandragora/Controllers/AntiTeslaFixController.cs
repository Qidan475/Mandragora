using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Pools;
using Mandragora.Patches;
using MEC;
using PlayerStatsSystem;
using UnityEngine;

namespace Mandragora.Controllers
{
    class AntiTeslaFixController : IDisposable
    {
        private Dictionary<TeslaGate, Bounds> _teslaGateKillBounds = new Dictionary<TeslaGate, Bounds>();
        private Dictionary<TeslaGate, HashSet<Player>> _detectedPlayers = new Dictionary<TeslaGate, HashSet<Player>>();
        private Dictionary<TeslaGate, HashSet<Player>> _recordedTeslaHits = new Dictionary<TeslaGate, HashSet<Player>>();
        

        public AntiTeslaFixController()
        {
            if (PluginFeature.AntiTeslaFix.IsKillswitched())
                return;

            TeslaChargePatch.TeslaChangingState += OnTeslaStateChange;
            TeslaHitReceivedPatch.TeslaHitReceived += OnTeslaHitReceived;
            Exiled.Events.Handlers.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public void Dispose()
        {
            if (PluginFeature.AntiTeslaFix.IsKillswitched())
                return;

            TeslaChargePatch.TeslaChangingState -= OnTeslaStateChange;
            TeslaHitReceivedPatch.TeslaHitReceived -= OnTeslaHitReceived;
            Exiled.Events.Handlers.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        IEnumerator<float> OnWaitingForPlayers()
        {
            Reset();

            yield return Timing.WaitForSeconds(1f);
            SetupColliders();
        }

        private void Reset()
        {
            _teslaGateKillBounds.Clear();
            _detectedPlayers.Clear();
        }

        private void SetupColliders()
        {
            foreach (var item in Exiled.API.Features.TeslaGate.List)
            {
                Bounds bounds = default;
                foreach (var killer in item.Base.killers)
                {
                    var tempBounds = new Bounds(killer.transform.position, item.Base.sizeOfKiller * EntryPoint.Instance.Config.TeslaKillboxMultiplier);
                    if (bounds == default)
                        bounds = tempBounds;
                    else
                        bounds.Encapsulate(tempBounds);
                }

                if (bounds == default)
                {
                    Log.Error("why tf tesla gate has no killers");
                    continue;
                }

                _teslaGateKillBounds.Add(item.Base, bounds);
                _detectedPlayers.Add(item.Base, new HashSet<Player>());
            }
        }

        void OnTeslaHitReceived(TeslaHitReceivedPatch.TeslaHitEventArgs ev)
        {
            if (!Player.TryGet(ev.ReferenceHub, out var player) || player.IsDisconnected() || ev.Instance == null)
                return;

            if (!_recordedTeslaHits.TryGetValue(ev.Instance, out var recordedPlayers))
                return;

            recordedPlayers.Add(player);
        }

        void OnTeslaStateChange(TeslaChargePatch.TeslaChangingStateEventArgs ev)
        {
            if (ev.Instance?.gameObject == null)
                return;

            if (ev.State == TeslaChargePatch.TeslaState.WindupStarted && _detectedPlayers.TryGetValue(ev.Instance, out var detectedPlayers))
            {
                _recordedTeslaHits.Add(ev.Instance, HashSetPool<Player>.Pool.Get());
                detectedPlayers.Clear();
                return;
            }

            if (ev.State == TeslaChargePatch.TeslaState.FiringStarted || ev.State == TeslaChargePatch.TeslaState.Firing)
            {
                foreach (var ply in Player.List)
                {
                    if (ply.IsDead)
                        continue;

                    var bounds = _teslaGateKillBounds[ev.Instance];
                    bool isInKillRange = bounds.Contains(ply.Position);
                    if (isInKillRange)
                        _detectedPlayers[ev.Instance].Add(ply);
                }
                return;
            }
            if (ev.State == TeslaChargePatch.TeslaState.Ended)
            {
                var recordedPlayers = _recordedTeslaHits[ev.Instance];
                foreach (var item in _detectedPlayers[ev.Instance])
                {
                    if (item.IsDisconnected() || item.IsDead)
                        continue;

                    if (recordedPlayers.Contains(item))
                        continue;

                    Log.Warn($"{item.Nickname} ({item.Role.Type}) got caught on tesla");
                    item.Hurt(new UniversalDamageHandler(500, DeathTranslations.Tesla, null));
                }
                HashSetPool<Player>.Pool.Return(recordedPlayers);
                _recordedTeslaHits.Remove(ev.Instance);
                return;
            }
        }
    }
}
