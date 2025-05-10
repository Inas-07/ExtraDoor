using HarmonyLib;
using LevelGeneration;
using Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Patches.DisableAlarmShutdownOnTerminal
{
    [HarmonyPatch]
    internal static class DoorLock_OnDoorState
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_SecurityDoor_Locks), nameof(LG_SecurityDoor_Locks.OnDoorState))]
        private static bool Pre_(LG_SecurityDoor_Locks __instance, pDoorState state)
        {
            if (__instance.m_door == null || state.status != eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm) return true;
            var fc = __instance.m_door.GetFC();
            if (fc == null) return true;

            __instance.m_intUseKeyItem.SetActive(false);
            __instance.m_intHack.SetActive(false);
            __instance.m_intOpenDoor.SetActive(true);

            // code related to `TurnOffAlarmOnTerminal` removed

            __instance.m_intOpenDoor.InteractionMessage = Text.Format(840U, __instance.ChainedPuzzleToSolve.Data.PublicAlarmName);
            __instance.m_intCustomMessage.SetActive(false);

            __instance.m_lastStatus = state.status;
            return false;
        }
    }
}
