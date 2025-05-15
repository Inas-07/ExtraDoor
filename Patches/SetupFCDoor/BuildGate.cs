using EOSExt.ExtraDoor.Config;
using Expedition;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using System;
using UnityEngine;

namespace EOSExt.ExtraDoor.Patches.SetupFCDoor
{
    [HarmonyPatch]
    internal static class BuildGate
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_BuildGateJob), nameof(LG_BuildGateJob.Build))]
        private static bool Pre_(LG_BuildGateJob __instance, ref bool __result)
        {
            ForceConnect fc = null;
            if (__instance.m_plug != null)
            {
                fc = __instance.m_plug.gameObject.GetComponent<ForceConnect>();
            }
            else
            {
                fc = __instance.m_gate.gameObject.GetComponent<ForceConnect>();
            }

            if (fc == null) return true;

            try
            {
                if (__instance.m_gate.m_wasProcessed)
                {
                    __result = true;
                    return false;
                }

                __instance.m_gate.m_wasProcessed = true;
                if (__instance.m_plug != null)
                {
                    __instance.m_gate.m_linksFrom = __instance.m_plug.m_linksFrom;
                    __instance.m_gate.ExpanderStatus = __instance.m_plug.ExpanderStatus;
                    if (__instance.m_plug.CoursePortal != null)
                    {
                        __instance.m_gate.CoursePortal = __instance.m_plug.CoursePortal;
                        __instance.m_gate.CoursePortal.Gate = __instance.m_gate;
                        __instance.m_gate.m_needsBorderLinks = true;
                        __instance.m_gate.m_linksTo = __instance.m_plug.m_linksTo;
                    }
                }
                else if (__instance.m_gate.CoursePortal != null)
                {
                    __instance.m_gate.CoursePortal.Gate = __instance.m_gate;
                }
                if (__instance.m_gate.ExpanderStatus == LG_ZoneExpanderStatus.Blocked)
                {
                    __instance.SetupCappedGate();
                }
                else
                {
                    __instance.CalcOpenAtStart();
                    switch (__instance.m_gate.Type)
                    {
                        case LG_GateType.Small:
                        case LG_GateType.Medium:
                        case LG_GateType.Large:
                            __instance.CalcOpenAtStart();
                            if (__instance.m_gate.m_isTraversableFromStart && __instance.m_gate.HasGateWallRemover && Builder.BuildSeedRandom.Value("LG_BuildGateJob") < RundownManager.ActiveExpeditionBalanceData.WeakDoorOpenChanceForWallRemoverUsed)
                            {
                                __instance.m_gate.EnableGateWallRemover(__instance.m_rnd.Random.NextSubSeed());
                            }
                            else
                            {
                                GameObject gateGO = null;
                                LG_Area progressionSourceArea = __instance.m_gate.ProgressionSourceArea;
                                __instance.m_gate.GetOppositeArea(progressionSourceArea);
                                if (__instance.m_gate.ForceBulkheadGate)
                                {
                                    if(fc.Cfg.Setting.SecurityGateToEnter == GateType.Bulkhead_Main)
                                    {
                                        uint seed = __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed());
                                        gateGO = Builder.ComplexResourceSetBlock.GetMainPathBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                        if (gateGO == null)
                                        {
                                            EOSLogger.Error("Could not get main path bulkhead door prefab: defaulting back to a normal bulkhead door! (You're Complex Resource Set probably don't have the main path bulkhead door references set).");
                                            gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                        }
                                    }
                                    else
                                    {
                                        gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                    }
                                }
                                else if (__instance.m_gate.ForceApexGate)
                                {
                                    gateGO = Builder.ComplexResourceSetBlock.GetApexGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                }
                                else if (__instance.m_gate.ForceSecurityGate)
                                {
                                    gateGO = Builder.ComplexResourceSetBlock.GetSecurityGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                }
                                else if (__instance.m_gate.m_isZoneSource)
                                {
                                    __instance.m_gate.IsCheckpointDoor = fc.Cfg.Setting.IsCheckpointDoor;
                                    if (__instance.m_gate.IsLayerSource(out var fromLayer, out var toLayer))
                                    {
                                        gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                    }

                                    //else if (__instance.m_gate.m_linksTo.m_zone.Layer.m_buildData.m_layerGameData.ZonesWithBulkheadEntrance.Contains(__instance.m_gate.m_linksTo.m_zone.LocalIndex))
                                    //{
                                    //    Debug.Log(Deb.LG(">>>>>>>>>> ZONE " + __instance.m_gate.m_linksTo.m_zone.Alias + " IS IN ZonesWithBulkheadEntrance!", 0f));
                                    //    uint seed = __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed());
                                    //    gameObject = Builder.ComplexResourceSetBlock.GetMainPathBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                    //    if (gameObject == null)
                                    //    {
                                    //        EOSLogger.Error("Could not get main path bulkhead door prefab: defaulting back to a normal bulkhead door! (You're Complex Resource Set probably don't have the main path bulkhead door references set).");
                                    //        gameObject = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                    //    }
                                    //}

                                    else
                                    {
                                        var type = fc.Cfg.Setting.SecurityGateToEnter;
                                        if (type == GateType.Apex)
                                        {
                                            gateGO = Builder.ComplexResourceSetBlock.GetApexGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                        }
                                        else if(type == GateType.Security)
                                        {
                                            gateGO = Builder.ComplexResourceSetBlock.GetSecurityGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                        }
                                        else
                                        {
                                            if (fc.Cfg.Setting.SecurityGateToEnter == GateType.Bulkhead_Main)
                                            {
                                                uint seed = __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed());
                                                gateGO = Builder.ComplexResourceSetBlock.GetMainPathBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                                if (gateGO == null)
                                                {
                                                    EOSLogger.Error("Could not get main path bulkhead door prefab: defaulting back to a normal bulkhead door! (You're Complex Resource Set probably don't have the main path bulkhead door references set).");
                                                    gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                                }
                                            }
                                            else
                                            {
                                                gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                            }

                                            //uint seed = __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed());
                                            //gateGO = Builder.ComplexResourceSetBlock.GetMainPathBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                            //if (gateGO == null)
                                            //{
                                            //    EOSLogger.Error("Could not get main path bulkhead door prefab: defaulting back to a normal bulkhead door! (You're Complex Resource Set probably don't have the main path bulkhead door references set).");
                                            //    gateGO = Builder.ComplexResourceSetBlock.GetBulkheadGate(__instance.m_gate.Type, seed, SubComplex.All);
                                            //}
                                        }
                                    }
                                }
                                else
                                {
                                    gateGO = Builder.ComplexResourceSetBlock.GetWeakGate(__instance.m_gate.Type, __instance.m_rnd.Seed.SubSeed(__instance.m_rnd.Random.NextSubSeed()), SubComplex.All);
                                }

                                Vector3 position = __instance.m_gate.transform.position;
                                Quaternion rotation;
                                if (__instance.m_plugWasFlipped || __instance.m_gate.ExpanderRotation == LG_ZoneExpanderRotation.Gate_RelinkedAndFlipped)
                                {
                                    rotation = Quaternion.LookRotation(__instance.m_gate.transform.forward * -1f, Vector3.up);
                                    __instance.m_gate.m_hasBeenFlipped = true;
                                }
                                else
                                {
                                    rotation = __instance.m_gate.transform.rotation;
                                    __instance.m_gate.m_hasBeenFlipped = false;
                                }

                                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gateGO, position, rotation, __instance.m_gate.transform);
                                //__instance.m_gate.SpawnedDoor = __instance.SetupDoor(gameObject2);
                                __instance.m_gate.SpawnedDoor = SetupDoor(__instance, gameObject2, fc);
                                __instance.SetupSpawners(gameObject2);
                                __instance.ProcessDivider(gameObject2, __instance.m_gate.m_hasBeenFlipped);
                                Debug.DrawLine(__instance.m_gate.transform.position, __instance.m_gate.m_linksFrom.Position, new Color(0f, 0.5f, 1f, 1f), 10000f, false);
                                Debug.DrawLine(__instance.m_gate.transform.position, __instance.m_gate.m_linksTo.Position, new Color(0f, 1f, 0.5f, 0.25f), 10000f, false);
                                Debug.DrawRay(gameObject2.transform.position, gameObject2.transform.forward * 10f, new Color(0f, 1f, 0f, 1f), 10000f, false);
                            }
                            LG_Factory.InjectJob(new LG_LinkAIGateJob(__instance.m_gate), LG_Factory.BatchName.AIGraph_LinkGates);
                            break;
                        case LG_GateType.GeoBorderLinker:
                            Debug.LogError("ERROR : Do we need to use this anymore?");
                            break;
                        case LG_GateType.FreePassage:
                            __instance.m_gate.m_isTraversableFromStart = true;
                            __instance.m_gate.m_wasProcessed = true;
                            LG_Gate gate = __instance.m_gate;
                            gate.name += " freePassage!";
                            if (__instance.m_plugWasFlipped || __instance.m_gate.ExpanderRotation == LG_ZoneExpanderRotation.Gate_RelinkedAndFlipped)
                            {
                                __instance.m_gate.m_hasBeenFlipped = true;
                            }
                            else
                            {
                                __instance.m_gate.m_hasBeenFlipped = false;
                            }
                            LG_Factory.InjectJob(new LG_LinkAIGateJob(__instance.m_gate), LG_Factory.BatchName.AIGraph_LinkGates);
                            break;
                            
                    }
                }

                __result = true;
                return false;
            }

            catch(Exception e)
            {
                EOSLogger.Error($"Exception occurred when building Gate: {e}");
                EOSLogger.Error($"Falling back to vanilla");
                __instance.m_gate.m_wasProcessed = false;
                return true;
            }
        }

        private static iLG_Door_Core SetupDoor(LG_BuildGateJob __instance, GameObject doorGO, ForceConnect fc)
        {
            var core = __instance.SetupDoor(doorGO);
            if (__instance.m_gate.ForceSecurityGate || __instance.m_gate.ForceApexGate || __instance.m_gate.m_isZoneSource)
            {
                LG_SecurityDoor door = core.Cast<LG_SecurityDoor>();
                if (fc.Cfg.Setting.IsCheckpointDoor)
                {
                    LG_Factory.InjectJob(new LG_SecurityDoor.LG_CheckpointScannerJob(door), LG_Factory.BatchName.DoorLocks);
                }
                if (fc.Cfg.Setting.ActiveEnemyWave.HasActiveEnemyWave)
                {
                    door.SetupActiveEnemyWaveData(fc.Cfg.Setting.ActiveEnemyWave);
                }
            }

            return core;
        }
    }
}
