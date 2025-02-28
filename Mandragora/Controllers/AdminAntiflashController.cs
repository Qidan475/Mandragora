using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using MEC;
using PluginAPI.Roles;

namespace Mandragora.Controllers
{
    class AdminAntiflashController : IDisposable
    {
        public AdminAntiflashController()
        {
            if (PluginFeature.AdminAntiflashCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Player.ReceivingEffect += OnReceivingEffect;
        }

        public void Dispose()
        {
            if (PluginFeature.AdminAntiflashCmd.IsKillswitched())
                return;

            Exiled.Events.Handlers.Player.ReceivingEffect -= OnReceivingEffect;
        }

        void OnReceivingEffect(ReceivingEffectEventArgs ev)
        {
            if (ev.Player.IsDisconnected() || ev.Effect == null)
                return;

            if (!EntryPoint.Instance.Config.AntiflashEffects.Contains(ev.Effect.GetEffectType()) || ev.Intensity < 1 || (ev.Duration > 0 && ev.Duration <= 0.5f))
                return;

            if (EntryPoint.Instance.OWManager.HasActiveAntiflash(ev.Player))
            {
                ev.IsAllowed = false;
                return;
            }

            Timing.CallDelayed(0.250f, () =>
            {
                if (ev.Player.IsDisconnected())
                    return;

                foreach (var spectator in ev.Player.CurrentSpectatingPlayers)
                {
                    if (!EntryPoint.Instance.OWManager.HasActiveAntiflash(spectator))
                        continue;

                    EntryPoint.Instance.Config.AntiflashEffects.Do(effect =>
                        ev.Player.SendFakeEffectTo(spectator, effect, 0));
                }
            });
        }
    }
}
