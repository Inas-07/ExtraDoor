//using EOSExt.ExtraDoor.Config;
//using ExtraObjectiveSetup.Utils;
//using GameData;
//using LevelGeneration;
//using SNetwork;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection.Metadata.Ecma335;
//using System.Text;
//using System.Threading.Tasks;

//namespace EOSExt.ExtraDoor
//{
//    public partial class ForceConnectManager
//    {
//        public enum FCDoorEventType
//        {
//            OpenFCDoor = 900,
//            UnlockFCDoor = 901,
//        }

//        // TODO: door indexing

//        private static void OpenFCDoor(WardenObjectiveEventData e)
//        {
//            if (!SNet.IsMaster) return;

//            var areaIndex = e.Count;
//            var doorIndex = e.FogSetting;
//            bool openAll = e.Enabled;

//            if (!Builder.Current.m_currentFloor.TryGetZoneByLocalIndex(e.DimensionIndex, e.Layer, e.LocalIndex, out var zone)
//                || areaIndex < 0 || areaIndex >= zone.m_areas.Count
//                )
//            {
//                EOSLogger.Error($"OpenFCDoor: invalid area indexer {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                return;
//            }

//            var area = zone.m_areas[areaIndex];
//            List<LG_SecurityDoor> fcDoors = new();
//            foreach (var exp in area.m_zoneExpanders)
//            {
//                if (exp.GetGate().SpawnedDoor == null) continue;
//                var maybeFCDoor = exp.GetGate().SpawnedDoor.TryCast<LG_SecurityDoor>();
//                if (maybeFCDoor == null) continue;

//                var fc = maybeFCDoor.GetFC();
//                if (fc == null) continue;

//                fcDoors.Add(maybeFCDoor);
//            }

//            if (openAll)
//            {
//                EOSLogger.Log($"OpenFCDoor: Opening all FC door in {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                fcDoors.ForEach(d => d.ForceOpenSecurityDoor());
//            }
//            else
//            {
//                if (doorIndex >= fcDoors.Count)
//                {
//                    EOSLogger.Log($"OpenFCDoor: Invalid FC door index ({doorIndex}) for this area, the valid range is [0, {fcDoors.Count})\n{(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                    return;
//                }

//                var fcDoor = fcDoors[(int)doorIndex];
//                EOSLogger.Log($"OpenFCDoor: Opening (index {doorIndex}) in {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                fcDoor.ForceOpenSecurityDoor();
//            }
//        }

//        private static void UnlockFCDoor(WardenObjectiveEventData e)
//        {
//            if (!SNet.IsMaster) return;

//            var areaIndex = e.Count;
//            var doorIndex = e.FogSetting;
//            bool unlockAll = e.Enabled;

//            if (!Builder.Current.m_currentFloor.TryGetZoneByLocalIndex(e.DimensionIndex, e.Layer, e.LocalIndex, out var zone)
//                || areaIndex < 0 || areaIndex >= zone.m_areas.Count
//                )
//            {
//                EOSLogger.Error($"UnlockFCDoor: invalid area indexer {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                return;
//            }

//            var area = zone.m_areas[areaIndex];
//            List<LG_SecurityDoor> fcDoors = new();
//            foreach (var exp in area.m_zoneExpanders)
//            {
//                if (exp.GetGate().SpawnedDoor == null) continue;
//                var maybeFCDoor = exp.GetGate().SpawnedDoor.TryCast<LG_SecurityDoor>();
//                if (maybeFCDoor == null) continue;

//                var fc = maybeFCDoor.GetFC();
//                if (fc == null) continue;

//                fcDoors.Add(maybeFCDoor);
//            }

//            if (unlockAll)
//            {
//                EOSLogger.Log($"UnlockFCDoor: Unlocking all FC doors in {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                fcDoors.ForEach(d => d.UseChainedPuzzleOrUnlock(SNet.Master));
//            }
//            else
//            {
//                if (doorIndex >= fcDoors.Count)
//                {
//                    EOSLogger.Log($"UnlockFCDoor: Invalid FC door index ({doorIndex}) for this area, the valid range is [0, {fcDoors.Count})\n{(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                    return;
//                }

//                var fcDoor = fcDoors[(int)doorIndex];
//                EOSLogger.Log($"UnlockFCDoor: unlocking (index {doorIndex}) in {(e.DimensionIndex, e.Layer, e.LocalIndex, ((char)'A' + areaIndex))}");
//                fcDoor.UseChainedPuzzleOrUnlock(SNet.Master);
//            }
//        }
//    }
//}
