using EOSExt.ExtraDoor.Config;
using ExtraObjectiveSetup.Utils;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.ExtraDoor
{
    public partial class ForceConnectManager
    {
        public void BuildCfg(ForceConnectCfg cfg)
        {
            eDimensionIndex dim = cfg.DimensionIndex;

            var from = cfg.From;
            var to = cfg.To;

            if (!(Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, from.Layer, from.LocalIndex, out var fromZone)
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

            if (expandFrom == null || expandTo == null)
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
            if (fromPlug == null || toPlug == null)
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
            var forceConnect = fromPlug.gameObject.AddComponent<ForceConnect>();
            forceConnect.Cfg = cfg;

            if (expandFrom.m_linksFrom.m_zone.IDinLayer > expandTo.m_linksFrom.m_zone.IDinLayer)
            {
                forceConnect.ShouldFlipDoor = true;
            }
        }

        private void Build()
        {
            if (definitions.TryGetValue(CurrentMainLevelLayout, out var defs))
            {
                defs.Definitions.ForEach(BuildCfg);
            }
        }


    }
}
