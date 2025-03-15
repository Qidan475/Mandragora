using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.Features;
using HarmonyLib;
using Hazards;
using MEC;
using RelativePositioning;
using UnityEngine;
using static PlayerList;

namespace Mandragora.Patches
{
    [HarmonyPatch()]
    class TeslaChargePatch
    {
        public static Event<TeslaChangingStateEventArgs> TeslaChangingState { get; set; } = new Event<TeslaChangingStateEventArgs>();

        [HarmonyPrefix()]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.ServerSideCode))]
        private static bool ServerSideCode(TeslaGate __instance)
        {
            if (PluginFeature.AntiTeslaFix.IsKillswitched())
                return true;

            if (__instance.InProgress)
                return false;

            ServerSideWaitForAnimation(__instance).RunCoroutine();
            __instance.RpcPlayAnimation();
            return false;
        }

        private static IEnumerator<float> ServerSideWaitForAnimation(TeslaGate __instance)
        {
            TeslaChangingState.InvokeSafely(new TeslaChangingStateEventArgs(TeslaState.WindupStarted, __instance));
            __instance.InProgress = true;
            yield return Timing.WaitForSeconds(__instance.windupTime);
            TeslaChangingState.InvokeSafely(new TeslaChangingStateEventArgs(TeslaState.FiringStarted, __instance));
            DestroyTantrums(__instance);

            var firingMultiplier = EntryPoint.Instance.Config.TeslaFiringDurationMultiplier;
            var firingEnds = Timing.WaitForSeconds(__instance.cooldownTime * firingMultiplier);
            while (Timing.LocalTime < firingEnds)
            {
                yield return Timing.WaitForOneFrame;

                TeslaChangingState.InvokeSafely(new TeslaChangingStateEventArgs(TeslaState.Firing, __instance));
            }
            var cooldownEnds = Timing.WaitForSeconds(__instance.cooldownTime * (1f - firingMultiplier));
            while (Timing.LocalTime < cooldownEnds)
            {
                yield return Timing.WaitForOneFrame;

                TeslaChangingState.InvokeSafely(new TeslaChangingStateEventArgs(TeslaState.Cooldown, __instance));
            }

            __instance.InProgress = false;
            TeslaChangingState.InvokeSafely(new TeslaChangingStateEventArgs(TeslaState.Ended, __instance));
        }

        private static void DestroyTantrums(TeslaGate __instance)
        {
            if (__instance.TantrumsToBeDestroyed.Count > 0)
            {
                __instance.TantrumsToBeDestroyed.ForEach(delegate (TantrumEnvironmentalHazard tantrum)
                {
                    if (tantrum != null)
                    {
                        tantrum.PlaySizzle = true;
                        tantrum.ServerDestroy();
                    }
                });
                __instance.TantrumsToBeDestroyed.Clear();
            }
        }

        public class TeslaChangingStateEventArgs
        {
            public TeslaChangingStateEventArgs(TeslaState state, TeslaGate instance)
            {
                State = state;
                Instance = instance;
            }

            public TeslaState State { get; }
            public TeslaGate Instance { get; }
        }

        public enum TeslaState
        {
            WindupStarted,
            FiringStarted,
            Firing,
            Cooldown,
            Ended
        }
    }
}
