using AIGraph;
using AK;
using ChainedPuzzles;
using Enemies;
using GameData;
using LevelGeneration;
using LogUtils;
using SNetwork;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExtraObjectiveSetup.Utils;
using BepInEx.Unity.IL2CPP.Utils;
using GTFO.API.Extensions;

namespace EOSExt.ExtraDoor
{
    internal static class ForceConnectDoorUtils
    {
        public static ForceConnect? GetFC(this LG_SecurityDoor fcdoor) => fcdoor.gameObject.GetComponentInParent<ForceConnect>(); // get door from plug

        public static List<(LG_ZoneExpander fromExp, LG_ZoneExpander toExp)> GetExpandersBetween(LG_Area fromArea, LG_Area toAreaB)
        {
            var lst = new List<(LG_ZoneExpander fromExp, LG_ZoneExpander toExp)>();
            // O(N^2), required for setting up plug,
            // for gate, we could make it O(N) but whatever
            foreach (LG_ZoneExpander fromExp in fromArea.m_zoneExpanders)
            {
                foreach (LG_ZoneExpander toExp in toAreaB.m_zoneExpanders)
                {
                    Vector2 fromPos = new(fromExp.transform.position.x, fromExp.transform.position.z);
                    Vector2 toPos = new(toExp.transform.position.x, toExp.transform.position.z);

                    // FIXME: will break if having plug here is impossible
                    if (Vector2.Distance(fromPos, toPos) <= 0.1f)
                    {
                        lst.Add((fromExp, toExp));
                        // We can actually find some same plug tho
                        //if (fromExp.Pointer == toExp.Pointer)
                        //{
                        //    EOSLogger.Warning("Same plug!");
                        //}
                    }
                }
            }

            return lst;
        }

        public static void FCOnDoorOpenStarted(this LG_SecurityDoor fcdoor)
        {
            if (fcdoor.m_securityDoorType == eSecurityDoorType.Bulkhead)
            {
                switch (fcdoor.LinksToLayerType)
                {
                    case LG_LayerType.MainLayer:
                        fcdoor.m_sound.Post(EVENTS.BULKHEAD_ENTRANCE_THREAT_HIGH_LOOP, true);
                        return;
                    case LG_LayerType.SecondaryLayer:
                        fcdoor.m_sound.Post(EVENTS.BULKHEAD_ENTRANCE_THREAT_EXTREME_LOOP, true);
                        return;
                    case LG_LayerType.ThirdLayer:
                        fcdoor.m_sound.Post(EVENTS.BULKHEAD_ENTRANCE_THREAT_OVERLOAD_LOOP, true);
                        break;
                    default:
                        return;
                }
            }
        }

        public static void FCOnDoorIsOpen(this LG_SecurityDoor fcdoor)
        {
            var fc = fcdoor.GetFC();

            EOSLogger.Log($"OnDoorIsOpened: ForceConnect: {fcdoor.LinkedToZoneData}");
            if (fc != null)
            {
                var evts = fc.Cfg.Setting.EventsOnEnter;
                EOSLogger.Log("OnDoorIsOpened, FC.EventsOnEnter");
                if (evts?.Count > 0)
                {
                    evts.ForEach(e => fcdoor.StartCoroutine(fcdoor.TriggerLevelEventWithDelay(e)));
                }

                WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnOpenDoor.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);
            }
        }

        public static void FCOnSyncDoorStatusChange(this LG_SecurityDoor fcdoor, pDoorState state, bool isRecall)
        {
            var fc = fcdoor.GetFC();
            EOSLogger.Log($"OnSyncDoorStatusChange: m_lastState.hasBeenApproached: {fcdoor.m_lastState.hasBeenApproached}, state.hasBeenApproached: {state.hasBeenApproached}, ForceConnect: {fc}");

            if (fcdoor.m_lastState.hasBeenApproached != state.hasBeenApproached && state.hasBeenApproached && fc != null && !isRecall)
            {
                WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnApproachDoor.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);
                fcdoor.LastStatus = state.status;
                fcdoor.m_lastState = state;
                return;
            }

            fcdoor.Gate.HasBeenOpenedDuringPlay = state.hasBeenOpenedDuringGame;
            fcdoor.m_graphics.OnDoorState(state, isRecall);
            fcdoor.m_anim.OnDoorState(state, isRecall);
            fcdoor.m_locks.OnDoorState(state, isRecall);
            bool isBulkheadDoor = fcdoor.m_securityDoorType == eSecurityDoorType.Bulkhead;
            switch (state.status)
            {
                case eDoorStatus.Closed:
                case eDoorStatus.Closed_BrokenCantOpen:
                case eDoorStatus.Closed_LockedWithChainedPuzzle:
                case eDoorStatus.Closed_LockedWithPowerGenerator:
                case eDoorStatus.Closed_LockedWithNoKey:
                case eDoorStatus.ChainedPuzzleActivated:
                case eDoorStatus.Unlocked:
                    if (state.status == eDoorStatus.ChainedPuzzleActivated && fcdoor.OnChainPuzzleActivate != null)
                    {
                        WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnDoorScanStart.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);
                        if (fcdoor.LinkedToZoneData.PlayScannerVoiceAudio)
                        {
                            fcdoor.m_sound.Post(EVENTS.TTS_PLEASE_STEP_INTO_THE_BIOSCAN, true);
                        }
                        fcdoor.OnChainPuzzleActivate.Invoke();
                    }

                    if (state.status == eDoorStatus.Unlocked && fcdoor.OnChainPuzzleActivate != null)
                    {
                        WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnDoorScanDone.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);

                        if (fc.Cfg.Setting.PlayScannerVoiceAudio)
                        {
                            fcdoor.m_sound.Post(EVENTS.TTS_BIOSCAN_SEQUENCE_COMPLETED, true);
                        }
                    }

                    if ((state.status == eDoorStatus.Unlocked || state.status == eDoorStatus.Closed_LockedWithChainedPuzzle) && fcdoor.m_lastState.status == eDoorStatus.Closed_LockedWithKeyItem)
                    {
                        WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnUnlockDoor.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);
                    }
                    fcdoor.m_mapLookatRevealer.SetLocalGUIObjStatus(isBulkheadDoor ? eCM_GuiObjectStatus.DoorBulkheadClosed : eCM_GuiObjectStatus.DoorSecureClosed);
                    fcdoor.CheckSetNavmarkerForBulkheadDoors();
                    break;
                case eDoorStatus.Closed_LockedWithKeyItem:
                    {
                        fcdoor.m_mapLookatRevealer.SetLocalGUIObjStatus(isBulkheadDoor ? eCM_GuiObjectStatus.DoorBulkheadClosed : eCM_GuiObjectStatus.DoorSecureKeycard);
                        PlaceNavMarkerOnGO navMarkerPlacer = fcdoor.m_navMarkerPlacer;
                        if (navMarkerPlacer != null)
                        {
                            navMarkerPlacer.SetMarkerVisible(false);
                        }
                        break;
                    }
                case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                    if (fcdoor.m_lastState.status == eDoorStatus.Closed_LockedWithKeyItem)
                    {
                        WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(fc.Cfg.Setting.EventsOnUnlockDoor.ToIl2Cpp(), eWardenObjectiveEventTrigger.None, true, 0f);
                    }
                    fcdoor.m_mapLookatRevealer.SetLocalGUIObjStatus(isBulkheadDoor ? eCM_GuiObjectStatus.DoorBulkheadClosed : eCM_GuiObjectStatus.DoorSecureApex);
                    fcdoor.CheckSetNavmarkerForBulkheadDoors();
                    break;
                case eDoorStatus.Open:
                    {
                        PlaceNavMarkerOnGO navMarkerPlacer2 = fcdoor.m_navMarkerPlacer;
                        if (navMarkerPlacer2 != null)
                        {
                            navMarkerPlacer2.SetMarkerVisible(false);
                        }
                        fcdoor.m_mapLookatRevealer.SetLocalGUIObjStatus(isBulkheadDoor ? eCM_GuiObjectStatus.DoorBulkheadOpen : eCM_GuiObjectStatus.DoorSecureOpen);
                        var aew = fc.Cfg.Setting.ActiveEnemyWave;
                        if (aew != null && aew.HasActiveEnemyWave)
                        {
                            fcdoor.m_sound.Post(EVENTS.MONSTER_RUCKUS_FROM_BEHIND_SECURITY_DOOR_LOOP_STOP, true);
                            if (!isRecall && SNet.IsMaster)
                            {
                                var cfg = fc.Cfg;
                                //AIG_CourseNode courseNode = fcdoor.Gate.GetOppositeArea(fcdoor.Gate.ProgressionSourceArea).m_courseNode;
                                Builder.CurrentFloor.TryGetZoneByLocalIndex(cfg.DimensionIndex, cfg.To.Layer, cfg.To.LocalIndex, out var enemyZone);
                                var toNode = enemyZone.m_areas[cfg.To.AreaIndex].m_courseNode;

                                // TODO: DirectionTowardsNode
                                //Vector3 position = fcdoor.transform.position + fcdoor.Gate.CoursePortal.DirectionTowardsNode(toNode) * 1.5f + Vector3.up * 0.3f;
                                Vector3 position = fcdoor.transform.position +  fcdoor.transform.forward * -1.5f + Vector3.up * 0.3f;
                                Vector3 gateCrossingVec = fcdoor.Gate.GetGateCrossingVec();

                                if (aew.EnemyGroupInfrontOfDoor != 0U)
                                {
                                    EnemyGroupDataBlock block = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(aew.EnemyGroupInfrontOfDoor);
                                    if (block == null)
                                    {
                                        EOSLogger.Error($"Can not spawn group. EnemyGroupDataBlock.GetBlock(ActiveEnemyWaveData.EnemyGroupInfrontOfDoor = {aew.EnemyGroupInfrontOfDoor}) returned null.");
                                    }
                                    else
                                    {
                                        Mastermind.Current.SpawnGroup(position, toNode, EnemyGroupType.Hunters, eEnemyGroupSpawnType.OnLine, block, -1f, gateCrossingVec, null);
                                    }
                                }
                                if (aew.EnemyGroupInArea != 0U)
                                {
                                    EnemyGroupDataBlock block2 = GameDataBlockBase<EnemyGroupDataBlock>.GetBlock(aew.EnemyGroupInArea);
                                    if (block2 == null)
                                    {
                                        EOSLogger.Error($"Can not spawn groups.EnemyGroupDataBlock.GetBlock(ActiveEnemyWaveData.EnemyGroupInArea = {aew.EnemyGroupInArea} ) returned null.");
                                    }
                                    else
                                    {
                                        for (int i = 0; i < aew.EnemyGroupsInArea; i++)
                                        {
                                            Mastermind.Current.SpawnGroup(position, toNode, EnemyGroupType.Hunters, eEnemyGroupSpawnType.RandomInArea, block2, -1f, default(Vector3), null);
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                case eDoorStatus.Closed_LockedWithBulkheadDC:
                    {
                        fcdoor.m_mapLookatRevealer.SetLocalGUIObjStatus(isBulkheadDoor ? eCM_GuiObjectStatus.DoorBulkheadClosed : eCM_GuiObjectStatus.DoorSecureClosed);
                        PlaceNavMarkerOnGO navMarkerPlacer3 = fcdoor.m_navMarkerPlacer;
                        if (navMarkerPlacer3 != null)
                        {
                            navMarkerPlacer3.SetMarkerVisible(false);
                        }
                        break;
                    }
                case eDoorStatus.Opening:
                    if (isRecall && SNet.IsMaster)
                    {
                        fcdoor.m_sync.AttemptDoorInteraction(eDoorInteractionType.Open, 0f, 0f, default(Vector3), null);
                    }
                    else if (SNet.IsMaster)
                    {
                        fcdoor.m_checkpointPuzzle.AttemptInteract(eChainedPuzzleInteraction.Activate);
                    }
                    break;
            }
            fcdoor.LastStatus = state.status;
            fcdoor.m_lastState = state;
        }
    }
}
