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
    }
}
