using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp106;
using PlayerRoles.Visibility;
using PlayerStatsSystem;
using UnityEngine;

namespace Mandragora.Patches
{
    [HarmonyPatch()]
    class Scp106StalkVisibilityFix
    {
        [HarmonyPostfix()]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags))]
        static void GetActiveFlagsPostfix(ReferenceHub observer, FpcVisibilityController __instance, ref InvisibilityFlags __result)
        {
            if (PluginFeature.Scp106StalkVisibilityFix.IsKillswitched())
                return;

            if (__result.HasFlagFast(InvisibilityFlags.OutOfRange) || !HitboxIdentity.IsEnemy(__instance.Owner, observer))
                return;

            if (__instance.Owner.roleManager.CurrentRole is not PlayerRoles.PlayableScps.Scp106.Scp106Role scp106 || scp106.VisibilityController is not Scp106VisibilityController visController)
                return;

            if (!visController._visSubroutine.SyncDamage.ContainsKey(observer.PlayerId))
                __result |= InvisibilityFlags.OutOfRange;
        }
    }
}
