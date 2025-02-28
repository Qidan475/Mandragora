using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;
using UnityEngine;

namespace Mandragora.Patches
{
    [HarmonyPatch(typeof(FpcBacktracker))]
    internal class FpcBacktrackPatch
    {
        [HarmonyPrefix()]
        [HarmonyPatch(MethodType.Constructor, typeof(ReferenceHub), typeof(Vector3), typeof(Quaternion), typeof(float), typeof(float), typeof(bool), typeof(bool))]
        static bool BacktrackerConstructorPrefix(FpcBacktracker __instance, ReferenceHub hub, Vector3 claimedPos, Quaternion claimedRot, float backtrack, float forecast, bool ignoreTp, bool restoreUponDeath)
        {
            if (PluginFeature.BacktrackForecastPatch.IsKillswitched())
                return true;

            if (!PluginFeature.BacktrackForecastOverride.IsKillswitched())
                forecast = EntryPoint.Instance.Config.BacktrackForecastOverride;

            if (hub.roleManager.CurrentRole is IFpcRole fpcRole)
            {
                FirstPersonMovementModule fpcModule = fpcRole.FpcModule;
                var prevPos = fpcModule.Position;
                var velocity = fpcModule.Motor.Velocity;

                __instance._moved = true;
                AccessTools.Field(typeof(FpcBacktracker), nameof(FpcBacktracker._movedHub)).SetValue(__instance, hub);
                AccessTools.Field(typeof(FpcBacktracker), nameof(FpcBacktracker._prevPos)).SetValue(__instance, prevPos);
                AccessTools.Field(typeof(FpcBacktracker), nameof(FpcBacktracker._prevRot)).SetValue(__instance, hub.PlayerCameraReference.rotation);
                Bounds bounds = ((backtrack <= 0f) ? new Bounds(fpcModule.Position, Vector3.zero) : fpcModule.Tracer.GenerateBounds(backtrack, ignoreTp));
                if (forecast > 0f && velocity.magnitude > EntryPoint.Instance.Config.BacktrackMinVelocityRequiredToCheckForecast)
                {
                    Vector3 predictedPosition = prevPos + velocity * forecast;
                    Vector3 allowedPosition = predictedPosition;
                    if (!PluginFeature.BacktrackForecastLinecastCheck.IsKillswitched() && Physics.Linecast(prevPos, predictedPosition, out var hit, EntryPoint.Instance.WallLayerMask))
                    {
                        var directionToPlayer = (prevPos - hit.point).normalized;
                        Vector3 posCloserToPlayer = hit.point + (directionToPlayer * 0.05f);
                        allowedPosition = posCloserToPlayer;
                    }
                    bounds.Encapsulate(allowedPosition);
                }
                AccessTools.Field(typeof(FpcBacktracker), nameof(FpcBacktracker._newPos)).SetValue(__instance, bounds.ClosestPoint(claimedPos));
                fpcModule.Position = __instance._newPos;
                hub.PlayerCameraReference.rotation = claimedRot;
            }
            else
            {
                __instance._moved = false;
            }
            if (restoreUponDeath)
            {
                PlayerStats.OnAnyPlayerDied += __instance.OnDied;
                __instance._restoreUponDeath = true;
                return false;
            }
            __instance._restoreUponDeath = false;

            return false;
        }
    }
}
