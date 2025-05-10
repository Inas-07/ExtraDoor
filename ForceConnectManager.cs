using AIGraph;
using EOSExt.ExtraDoor.Config;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.BaseClasses;
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

        private Dictionary<System.IntPtr, ForceConnect> m_doorLookup = new();

        //internal void Register(LG_SecurityDoor door, ForceConnect fc) => m_doorLookup[door.Pointer] = fc;

        //public bool TryGetForceConnectCfg(LG_SecurityDoor door, out ForceConnect cfg) => m_doorLookup.TryGetValue(door.Pointer, out cfg);

        public void RegisterFCDoor(LG_SecurityDoor fcdoor) => m_doorLookup[fcdoor.Pointer] = fcdoor.GetFC();

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

            LevelAPI.OnEnterLevel += OnEnterLevel;
            LevelAPI.OnLevelCleanup += Cleanup;
        }
    }
}
