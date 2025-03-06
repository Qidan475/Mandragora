using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
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
        private static float time;

        [HarmonyPrefix()]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.ServerSideCode))]
        private static bool ServerSideCode(TeslaGate __instance)
        {
            var timePassed = Time.time - time;
            Log.Info(timePassed);
            if (__instance.InProgress)
                return false;

            ServerSideWaitForAnimation(__instance).RunCoroutine();
            __instance.RpcPlayAnimation();

            return false;
        }

        private static IEnumerator<float> ServerSideWaitForAnimation(TeslaGate __instance)
        {
            Dictionary<Player, RelativePosition> playerPositionsPrev = new Dictionary<Player, RelativePosition>();
            GetPlayersPositions(playerPositionsPrev);

            __instance.InProgress = true;
            yield return Timing.WaitForSeconds(__instance.windupTime);
            DestroyTantrums(__instance);
            Dictionary<Player, RelativePosition> playerPositionsCurrent = new Dictionary<Player, RelativePosition>();
            GetPlayersPositions(playerPositionsCurrent);
            foreach (var item in playerPositionsCurrent)
            {
                if (!playerPositionsPrev.TryGetValue(item.Key, out var startPos) || item.Key.IsDisconnected())
                    continue;

                var endPos = item.Value;

                Vector3 min = Vector3.Min(startPos.Position, endPos.Position);
                Vector3 max = Vector3.Max(startPos.Position, endPos.Position);
                var startEndBounds = new Bounds((min + max) / 2, max - min);

                foreach (var killer in __instance.killers)
                {
                    var bounds = new Bounds(killer.transform.position, __instance.sizeOfKiller);
                    bool hasPassedThrough = bounds.Intersects(startEndBounds);
                    bool isInKillTrigger = bounds.Contains(endPos.Position);
                    Log.Info($"hasPassedThrough: {hasPassedThrough}; isInKillTrigger: {isInKillTrigger}; speed: {item.Key.Velocity.MagnitudeIgnoreY()}; (windup:{__instance.windupTime};cooldownTime:{__instance.cooldownTime})");

                    PrimitivesAPI.DrawPrimitive(PrimitivesAPI.Pool.GetAndForget(10f), PrimitiveType.Sphere, startPos.Position, Vector3.one * 0.05f, default, Color.green.SetBrightness(40));
                    PrimitivesAPI.DrawPrimitive(PrimitivesAPI.Pool.GetAndForget(10f), PrimitiveType.Sphere, endPos.Position, Vector3.one * 0.05f, default, Color.red.SetBrightness(40));
                }
            }

            Log.Debug("before waitforseconds");
            var firstAwaitEnds = Timing.WaitForSeconds(__instance.cooldownTime * 0.45f);
            Log.Debug("after waitforseconds");
            while (Timing.LocalTime < firstAwaitEnds)
            {
                PrimitivesAPI.DrawPrimitive(PrimitivesAPI.Pool.GetAndForget(3f), PrimitiveType.Sphere, Player.List.First().Position, Vector3.one * 0.05f, default, Color.cyan.SetBrightness(40));

                yield return Timing.WaitForOneFrame;
            }
            var secondAwaitEnds = Timing.WaitForSeconds(__instance.cooldownTime * 0.55f);
            while (Timing.LocalTime < secondAwaitEnds)
            {
                PrimitivesAPI.DrawPrimitive(PrimitivesAPI.Pool.GetAndForget(3f), PrimitiveType.Sphere, Player.List.First().Position, Vector3.one * 0.05f, default, Color.gray.SetBrightness(40));

                yield return Timing.WaitForOneFrame;
            }

            __instance.InProgress = false;
        }

        private static void GetPlayersPositions(in Dictionary<Player, RelativePosition> buffer)
        {
            foreach (var item in Player.List)
            {
                if (item.IsDisconnected() || item.IsDead)
                    continue;

                buffer.Add(item, item.RelativePosition);
            }
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
    }
}
