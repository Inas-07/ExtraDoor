//using CullingSystem;
//using ExtraObjectiveSetup.Utils;
//using GTFO.API.Extensions;
//using HarmonyLib;
//using LevelGeneration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace EOSExt.ExtraDoor.Patches
//{
//    [HarmonyPatch]
//    internal static class SetupForceGateDoor
//    {
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(LG_BuildGateJob), nameof(LG_BuildGateJob.SetupDoor))]
//        private static bool SetupDoor_Custom(LG_BuildGateJob __instance, GameObject doorGO, ref iLG_Door_Core __result)
//        {
//            if (__instance.m_plug != null) return true;
            
//            ForceConnect fc = __instance.m_gate.gameObject.GetComponent<ForceConnect>();
//            if (fc == null) return true;

//            EOSLogger.Warning("SetupDoor: Setting up force gate");

//            if (__instance.m_gate.m_isZoneSource)
//            {
//                LG_Gate gate = __instance.m_gate;
//                gate.name += "_zoneGate";
//                LG_Zone zone = __instance.m_gate.m_linksTo.m_zone;
//                if (__instance.m_gate.m_linksFrom.m_zone.ID > __instance.m_gate.m_linksTo.m_zone.ID)
//                {
//                    LG_Zone zone2 = __instance.m_gate.m_linksFrom.m_zone;
//                }
//            }

//            __result = doorGO.GetComponentInChildren<iLG_Door_Core>();
//            var setting = fc.Cfg.Setting;
//            if (__result != null)
//            {
//                __result.Setup(__instance.m_gate);
//                if (__instance.m_gate.ForceSecurityGate || __instance.m_gate.ForceApexGate || __instance.m_gate.m_isZoneSource)
//                {
//                    LG_SecurityDoor lg_SecurityDoor = __result.Cast<LG_SecurityDoor>();
//                    uint chainedPuzzleToEnter = setting.ChainedPuzzleToEnter;
//                    if (chainedPuzzleToEnter > 0U)
//                    {
//                        LG_Factory.InjectJob(new LG_BuildChainedPuzzleDoorLockJob(lg_SecurityDoor, chainedPuzzleToEnter), LG_Factory.BatchName.DoorLocks);
//                    }
//                    else
//                    {
//                        LG_Factory.InjectJob(new LG_SimpleDoorLockJob(lg_SecurityDoor), LG_Factory.BatchName.DoorLocks);
//                    }
//                    if (setting.IsCheckpointDoor)
//                    {
//                        LG_Factory.InjectJob(new LG_SecurityDoor.LG_CheckpointScannerJob(lg_SecurityDoor), LG_Factory.BatchName.DoorLocks);
//                    }
//                }
//                else
//                {
//                    LG_WeakDoor lg_WeakDoor = __result.Cast<LG_WeakDoor>();
//                    BuilderWeightedRandom builderWeightedRandom = new BuilderWeightedRandom();
//                    builderWeightedRandom.Setup(new List<float>
//                    {
//                        RundownManager.ActiveExpeditionBalanceData.WeakDoorChanceLockWeightNoLock,
//                        RundownManager.ActiveExpeditionBalanceData.WeakDoorChanceLockWeightMeleeLock,
//                        RundownManager.ActiveExpeditionBalanceData.WeakDoorChanceLockWeightHackableLock
//                    }.ToIl2Cpp());
//                    switch (builderWeightedRandom.GetRandomIndex(__instance.m_rnd.Random.NextSubSeed()))
//                    {
//                        case 1:
//                            lg_WeakDoor.SetupWeakLock(eWeakLockType.Melee);
//                            break;
//                        case 2:
//                            lg_WeakDoor.SetupWeakLock(eWeakLockType.Hackable);
//                            break;
//                    }
//                }
//                LG_Factory.InjectJob(new LG_SetNavInfoOnDoorJob(__instance.m_gate, __result), LG_Factory.BatchName.DisplayNavigationInfo);

//                //iC_CullersWithCallbackHolder iC_CullersWithCallbackHolder = componentInChildren as iC_CullersWithCallbackHolder;
//                //List<iC_CullerWithCallbackOwner> list;
//                //if (iC_CullersWithCallbackHolder != null && iC_CullersWithCallbackHolder.TryGetCallbackCullerRoots(out list))
//                //{
//                //    for (int i = 0; i < list.Count; i++)
//                //    {
//                //        LG_Factory.InjectJob(new C_BuildCullerWithCallback(list[i]), LG_Factory.BatchName.Culling_Cullers);
//                //    }
//                //}

//                if (__instance.m_gate.CoursePortal != null)
//                {
//                    __instance.m_gate.CoursePortal.m_door = __result;
//                }
//                else
//                {
//                    LG_Gate gate2 = __instance.m_gate;
//                    gate2.name += "_NO_COURSEPORTAL!";
//                    EOSLogger.Error(string.Concat(new string[]
//                    {
//                        "gate ",
//                        __instance.m_gate.name,
//                        " has no courseportal : ",
//                        __instance.m_gate.m_linksFrom.name,
//                        " ",
//                        __instance.m_gate.m_linksFrom.m_geomorph.name
//                    }));
//                }
//                bool isTraversableFromStart = __instance.m_gate.m_isTraversableFromStart;
//            }

//            return false;
//        }

//    }
//}
