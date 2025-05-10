using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using System.Reflection.Metadata.Ecma335;

namespace EOSExt.ExtraDoor.Patches.SetupDoor
{
    [HarmonyPatch]
    internal static class ChainedPuzzles
    {
        // ->  LG_BuildChainedPuzzleDoorLockJob 
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_BuildChainedPuzzleDoorLockJob), nameof(LG_BuildChainedPuzzleDoorLockJob.Build))]
        private static bool Pre_(LG_BuildChainedPuzzleDoorLockJob __instance, ref bool __result)
        {
            try
            {
                var door = __instance.m_door;
                var fc = __instance.m_door.gameObject.GetComponentInParent<ForceConnect>();
                if (fc == null) return true;

                // ====================== build override =========================
                __instance.CheckFlip();
                door.name += "_ChainedPuzzleLock";

                uint id = fc.Cfg.Setting.ChainedPuzzleToEnter;
                if (id > 0)
                {
                    door.SetupChainedPuzzleLock(id);
                }

                __result = true;
                return false;
            }
            catch(Exception e)
            {
                EOSLogger.Error($"Exception occurred when building ChainedPuzzleDoorLock: {e}");
                EOSLogger.Error($"Falling back to vanilla");
                return true;
            }
        }
    }
}
