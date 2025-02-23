using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.Visibility;

namespace Mandragora.Patches
{
    [HarmonyPatch(typeof(FpcVisibilityController))]
    class Scp1344VisibilityPatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(typeof(VisibilityController), nameof(VisibilityController.IgnoredFlags))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        static InvisibilityFlags IgnoredFlags_BaseStub(FpcVisibilityController instance) => 0;


        [HarmonyPrefix()]
        [HarmonyPatch(MethodType.Getter)]
        [HarmonyPatch(nameof(FpcVisibilityController.IgnoredFlags))]
        static bool IgnoredFlagsPrefix(FpcVisibilityController __instance, ref InvisibilityFlags __result)
        {
            __result = IgnoredFlags_BaseStub(__instance);
            if (__instance._scp1344Effect.IsEnabled)
                __result |= (InvisibilityFlags.Scp268 | InvisibilityFlags.Scp106Sinkhole);

            return false;
        }
    }
}
