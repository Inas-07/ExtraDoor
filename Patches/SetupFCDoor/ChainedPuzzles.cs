using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using ChainedPuzzles;
using GameData;

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
                var fc = __instance.m_door.GetFC();
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

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.SetupChainedPuzzleLock))]
        private static bool Pre_SetupCPLock(LG_SecurityDoor __instance, uint puzzleDataId)
        {
            var fc = __instance.GetFC(); 
            //if (fc == null || !fc.ShouldFlipDoor) return true; // process flipped door
            if (fc == null) return true; // process flipped door

            ChainedPuzzleDataBlock block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(puzzleDataId);
            if (__instance.GetChainedPuzzleStartPosition(out var sourcePos))
            {
                ChainedPuzzleInstance puzzleToOpen = ChainedPuzzleManager.CreatePuzzleInstance(block,
                    //__instance.Gate.ProgressionSourceArea,
                    __instance.Gate.m_linksFrom,

                    __instance.Gate.m_linksTo,

                    sourcePos, 
                    __instance.transform, 
                    false
                );

                eDoorStatus status = __instance.m_locks.SetupForChainedPuzzle(puzzleToOpen);
                __instance.m_sync.SetStateUnsynced(new pDoorState
                {
                    status = status,
                    damageTaken = 0f
                });
                if (block.ChainedPuzzle.Count == 0)
                {
                    __instance.SetupInstantOpeningDoor();
                }
                if (block.TriggerAlarmOnActivate)
                {
                    __instance.m_graphics.OnDoorState(new pDoorState
                    {
                        status = eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm
                    }, false);
                    __instance.m_mapLookatRevealer.SetLocalGUIObjStatus(eCM_GuiObjectStatus.DoorSecureApex);
                    return false;
                }
                __instance.m_graphics.OnDoorState(new pDoorState
                {
                    status = eDoorStatus.Closed_LockedWithChainedPuzzle
                }, false);
            }


            return false;
        }
    }
}
