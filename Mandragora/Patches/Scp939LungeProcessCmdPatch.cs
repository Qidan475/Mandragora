using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;
using RelativePositioning;
using UnityEngine;
using Utils.Networking;

namespace Mandragora.Patches
{
    [HarmonyPatch]
    class Scp939LungeProcessCmdPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
        private static bool ServerProcessCmdPrefix(NetworkReader reader, Scp939LungeAbility __instance)
        {
            if (__instance.State == Scp939LungeState.Triggered)
                return true;

            ConsumeSubroutineData(reader);
            return false;
        }

        private static void ConsumeSubroutineData(NetworkReader reader)
        {
            _ = reader.ReadRelativePosition();
            _ = reader.ReadReferenceHub();
            _ = reader.ReadRelativePosition();
        }
    }
}
