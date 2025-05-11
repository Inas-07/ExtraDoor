using AIGraph;
using EOSExt.ExtraDoor.Config;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using LevelGeneration;
using System.Collections.Generic;
using UnityEngine;

namespace EOSExt.ExtraDoor
{
    public partial class ForceConnectManager : GenericExpeditionDefinitionManager<ForceConnectCfg>
    {
        public static ForceConnectManager Current { get; } = new();

        protected override string DEFINITION_NAME => "ExtraDoor";

        private Dictionary<string, LG_SecurityDoor> m_doorLookup = new();

        public void RegisterFCDoor(LG_SecurityDoor fcdoor)
        {
            var fc = fcdoor.GetFC();
            if(fc == null)
            {
                EOSLogger.Error("ExtraDoor: Registering non-forceconnect door");
                return;
            }

            if(m_doorLookup.ContainsKey(fc.Cfg.WorldEventObjectFilter))
            {
                EOSLogger.Error($"ExtraDoor: Duplicate ExtraDoor '{fc.Cfg.WorldEventObjectFilter}'");
            }

            m_doorLookup[fc.Cfg.WorldEventObjectFilter] = fcdoor;
        }

        public LG_SecurityDoor GetFCDoor(string worldEventObjectFilter) =>
            m_doorLookup.TryGetValue(worldEventObjectFilter, out var door) ? door : null;

        private void Cleanup()
        {
            m_doorLookup.Clear();
        }

        private void OnEnterLevel()
        {

        }

        private ForceConnectManager()
        {
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.PlaceAddonZones, Build);
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.Distribution, BuildProgressionPuzzle);

            EOSWardenEventManager.Current.AddEventDefinition(FCDoorEventType.OpenFCDoor.ToString(), (uint)FCDoorEventType.OpenFCDoor, OpenFCDoor);
            EOSWardenEventManager.Current.AddEventDefinition(FCDoorEventType.UnlockFCDoor.ToString(), (uint)FCDoorEventType.UnlockFCDoor, UnlockFCDoor);

            LevelAPI.OnEnterLevel += OnEnterLevel;
            LevelAPI.OnLevelCleanup += Cleanup;
        }
    }
}
