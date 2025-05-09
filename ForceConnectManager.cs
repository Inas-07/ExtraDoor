using ExtraObjectiveSetup;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using LevelGeneration;
using System.Collections.Generic;
using UnityEngine;

namespace EOSExt.CircularZones
{
    public class ForceConnectManager : GenericExpeditionDefinitionManager<ForceConnectCfg>
    {
        public static ForceConnectManager Current { get; } = new();

        protected override string DEFINITION_NAME => "CircularZones";

        private int s_DoorIndex = 0;

        private Dictionary<int, LG_SecurityDoor> m_doorLookup = new();

        internal void RegisterDoor(int id, LG_SecurityDoor door)
        {
            m_doorLookup[id] = door;
        }

        public bool TryGetDoor(int id, out LG_SecurityDoor door) => m_doorLookup.TryGetValue(id, out door);
        
        private void Cleanup()
        {
            s_DoorIndex = 0;
            m_doorLookup.Clear();
        }

        public void BuildCfg(ForceConnectCfg cfg)
        {
            eDimensionIndex dim = cfg.DimensionIndex;

            var from = cfg.From;
            var to = cfg.To;

            if(!(Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, from.Layer, from.LocalIndex, out var fromZone)
                && 0 <= from.AreaIndex && from.AreaIndex < fromZone.m_areas.Count

                &&

                Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, to.Layer, to.LocalIndex, out var toZone)
                && 0 <= to.AreaIndex && to.AreaIndex < toZone.m_areas.Count))
            {
                EOSLogger.Error($"CircularZones Build: Cannot find Zone 'From' or 'To'!\n"
    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
                return;
            }


            var fromArea = fromZone.m_areas[from.AreaIndex];
            var toArea = toZone.m_areas[to.AreaIndex];

            LG_ZoneExpander expandFrom = null, expandTo = null;
            foreach (LG_ZoneExpander fromExp in fromArea.m_zoneExpanders)
            {
                foreach (LG_ZoneExpander toExp in toArea.m_zoneExpanders)
                {
                    if (Vector3.Distance(fromExp.transform.position, toExp.transform.position) <= 0.1f)
                    {
                        expandFrom = fromExp;
                        expandTo = toExp;
                    }
                }
            }

            if(expandFrom == null || expandTo == null)
            {
                if (expandFrom == null)
                {
                    EOSLogger.Error($"CircularZones Build: Cannot find 'from' - ({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})");
                }
                else
                {
                    EOSLogger.Error($"CircularZones Build: Cannot find 'to' - ({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
                }
                return;
            }

            expandFrom.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
            expandTo.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
            expandFrom.m_linksTo = expandTo.m_linksFrom;
            expandTo.m_linksTo = expandFrom.m_linksFrom;
            LG_Plug fromPlug = expandFrom.TryCast<LG_Plug>();
            LG_Plug toPlug = expandTo.TryCast<LG_Plug>();
            if(fromPlug == null || toPlug == null) 
            {
                EOSLogger.Error($"CircularZones Build: Cannot find fromPlug or toPlug!\n" 
                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
                return;
            }

            fromPlug.m_pariedWith = toPlug;
            toPlug.m_pariedWith = fromPlug;
            fromPlug.m_isZoneSource = true;
            GameObject gameObject = fromPlug.gameObject;
            gameObject.name += "FORCE PAIR";
            var forceConnectComp = fromPlug.gameObject.AddComponent<ForceConnect>();
            forceConnectComp.ID = s_DoorIndex;
            if (expandFrom.m_linksFrom.m_zone.IDinLayer > expandTo.m_linksFrom.m_zone.IDinLayer)
            {
                forceConnectComp.ShouldFlipDoor = true;
            }

            s_DoorIndex++;
        }

        private void Build()
        {
            if(definitions.TryGetValue(CurrentMainLevelLayout, out var defs))
            {
                defs.Definitions.ForEach(BuildCfg);
            }
        }

        private void OnEnterLevel()
        {

        }


        private ForceConnectManager()
        {
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.PlaceAddonZones, Build);
            LevelAPI.OnEnterLevel += OnEnterLevel;
            LevelAPI.OnLevelCleanup += Cleanup;
        }
    }
}
