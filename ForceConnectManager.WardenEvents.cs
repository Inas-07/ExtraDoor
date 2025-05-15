using AK;
using EOSExt.ExtraDoor.Config;
using ExtraObjectiveSetup.Utils;
using GameData;
using LevelGeneration;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor
{
    public partial class ForceConnectManager
    {
        public enum FCDoorEventType
        {
            OpenFCDoor = 900,
            UnlockFCDoor = 901,
            CloseFCDoor = 902,
            LockFCDoor = 903,
        }

        private static void OpenFCDoor(WardenObjectiveEventData e)
        {
            if (!SNet.IsMaster) return;

            var fcdoor = ForceConnectManager.Current.GetFCDoor(e.WorldEventObjectFilter);
            if(fcdoor == null)
            {
                EOSLogger.Error($"OpenFCDoor: Cannot find FC door with '{e.WorldEventObjectFilter}'");
                return;
            }

            fcdoor.ForceOpenSecurityDoor();
        }

        private static void UnlockFCDoor(WardenObjectiveEventData e) 
        {
            if (!SNet.IsMaster) return;

            var fcdoor = ForceConnectManager.Current.GetFCDoor(e.WorldEventObjectFilter);
            if (fcdoor == null)
            {
                EOSLogger.Error($"UnlockFCDoor: Cannot find FC door with '{e.WorldEventObjectFilter}'");
                return;
            }

            fcdoor.UseChainedPuzzleOrUnlock(SNet.Master);
        }

        private static void CloseFCDoor(WardenObjectiveEventData e)
        {
            if (!SNet.IsMaster) return;

            var fcdoor = ForceConnectManager.Current.GetFCDoor(e.WorldEventObjectFilter);
            if (fcdoor == null)
            {
                EOSLogger.Error($"UnlockFCDoor: Cannot find FC door with '{e.WorldEventObjectFilter}'");
                return;
            }

            pDoorState currentSyncState1 = fcdoor.m_sync.GetCurrentSyncState();
            if (currentSyncState1.status != eDoorStatus.Open && currentSyncState1.status != eDoorStatus.Opening)
                return;
            LG_Door_Sync lgDoorSync = fcdoor.m_sync.TryCast<LG_Door_Sync>();

            if (lgDoorSync == null) return;

            pDoorState currentSyncState2 = lgDoorSync.GetCurrentSyncState() with
            {
                status = eDoorStatus.Closed,
                hasBeenOpenedDuringGame = false
            };

            lgDoorSync.m_stateReplicator.State = currentSyncState2;
            LG_Gate gate = fcdoor.Gate;
            gate.HasBeenOpenedDuringPlay = false;
            gate.IsTraversable = false;

            var fc = fcdoor.GetFC();
            var setting = fc.Cfg.Setting;
            if (setting.ActiveEnemyWave != null && setting.ActiveEnemyWave.HasActiveEnemyWave)
            {
                fcdoor.m_sound.Post(EVENTS.MONSTER_RUCKUS_FROM_BEHIND_SECURITY_DOOR_LOOP_START);
            }
            EOSLogger.Debug("Door Closed!");
        }

        private static void LockFCDoor(WardenObjectiveEventData e)
        {
            if (!SNet.IsMaster) return;

            var fcdoor = ForceConnectManager.Current.GetFCDoor(e.WorldEventObjectFilter);
            if (fcdoor == null)
            {
                EOSLogger.Error($"UnlockFCDoor: Cannot find FC door with '{e.WorldEventObjectFilter}'");
                return;
            }

            //pDoorState currentSyncState1 = fcdoor.m_sync.GetCurrentSyncState();
            //if (currentSyncState1.status == eDoorStatus.Closed_LockedWithKeyItem 
                
            //    || currentSyncState1.status == eDoorStatus.Closed_LockedWithBulkheadDC
            //    || currentSyncState1.status == eDoorStatus.Closed_LockedWithPowerGenerator
                
            //    || currentSyncState1.status == )
            //    return;
            LG_Door_Sync lgDoorSync = fcdoor.m_sync.TryCast<LG_Door_Sync>();

            if (lgDoorSync == null) return;

            pDoorState currentSyncState2 = lgDoorSync.GetCurrentSyncState() with
            {
                status = eDoorStatus.Closed_LockedWithNoKey,
            };

            lgDoorSync.m_stateReplicator.State = currentSyncState2;
            EOSLogger.Debug("Door Locked!");
        }
    }
}
