using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;
using Exiled.API.Features;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp1344;
using Mandragora.Managers;
using MEC;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Extensions;

namespace Mandragora.Controllers
{
    class AdminWallhackController : IDisposable
    {
        public AdminWallhackController()
        {
            if (PluginFeature.AdminWallhackCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Player.Spawned += OnRoleChanged;
            Exiled.Events.Handlers.Player.ReceivingEffect += OnEffectReceived;
        }

        public void Dispose()
        {
            if (PluginFeature.AdminWallhackCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Player.Spawned -= OnRoleChanged;
            Exiled.Events.Handlers.Player.ReceivingEffect -= OnEffectReceived;
        }

        void OnRoleChanged(SpawnedEventArgs ev)
        {
            if (ev.Player.IsDisconnected())
                return;

            if (ev.Player.Role.Type == ev.OldRole.Type)
                return;

            UpdateFakeEffect(ev);
            HandleActiveWallhacker(ev);
        }

        private void UpdateFakeEffect(SpawnedEventArgs ev)
        {
            if (ev.Player.Role.IsAlive)
            {
                foreach (var wh in EntryPoint.Instance.OWManager.ActiveWallhack)
                {
                    ev.Player.SendFakeEffectTo(wh, EffectType.Scp1344, 1);
                }
            }
        }

        private void HandleActiveWallhacker(SpawnedEventArgs ev)
        {
            if (!EntryPoint.Instance.OWManager.UseridsWallhackRequests.Contains(ev.Player.UserId))
                return;

            if (ev.Player.Role.Type == PlayerRoles.RoleTypeId.Overwatch)
                OverwatchFeaturesManager.EnableVisuals(ev.Player);
            else
                OverwatchFeaturesManager.DisableVisuals(ev.Player);
        }

        void OnEffectReceived(ReceivingEffectEventArgs ev)
        {
            if (ev.Player.IsDisconnected() || ev.Effect == null)
                return;

            if (ev.Effect.GetEffectType() != EffectType.Scp1344 || ev.Intensity > 0)
                return;

            Timing.CallDelayed(Timing.WaitForOneFrame, () =>
            {
                foreach (var wh in EntryPoint.Instance.OWManager.ActiveWallhack)
                {
                    ev.Player.SendFakeEffectTo(wh, EffectType.Scp1344, 1);
                }
            });
        }
    }
}
