//using EOSExt.ExtraDoor.Config;
//using ExtraObjectiveSetup.Utils;
//using LevelGeneration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace EOSExt.ExtraDoor
//{
//    public partial class ForceConnectManager
//    {
//        private void Connect2Zones(ForceConnectCfg cfg, LG_Zone fromZone, LG_Zone toZone)
//        {
//            var from = cfg.From;
//            var to = cfg.To;

//            var fromArea = fromZone.m_areas[from.AreaIndex];
//            var toArea = toZone.m_areas[to.AreaIndex];

//            List<(LG_ZoneExpander expandFrom, LG_ZoneExpander expandTo)> lst = new();
//            LG_ZoneExpander expandFrom = null, expandTo = null;
//            foreach (LG_ZoneExpander fromExp in fromArea.m_zoneExpanders)
//            {
//                foreach (LG_ZoneExpander toExp in toArea.m_zoneExpanders)
//                {
//                    if (Vector3.Distance(fromExp.transform.position, toExp.transform.position) <= 0.1f)
//                    {
//                        expandFrom = fromExp;
//                        expandTo = toExp;
//                    }
//                }
//            }

//            if (expandFrom == null || expandTo == null)
//            {
//                if (expandFrom == null)
//                {
//                    EOSLogger.Error($"CircularZones Build: Cannot find 'from' - ({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})");
//                }
//                else
//                {
//                    EOSLogger.Error($"CircularZones Build: Cannot find 'to' - ({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
//                }
//                return;
//            }

//            LG_Plug fromPlug = expandFrom.TryCast<LG_Plug>();
//            LG_Plug toPlug = expandTo.TryCast<LG_Plug>();
//            if (fromPlug == null || toPlug == null)
//            {
//                EOSLogger.Error($"CircularZones Build: Cannot find fromPlug or toPlug!\n"
//                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
//                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
//                return;
//            }

//            expandFrom.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
//            expandTo.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
//            expandFrom.m_linksTo = expandTo.m_linksFrom;
//            expandTo.m_linksTo = expandFrom.m_linksFrom;

//            fromPlug.m_pariedWith = toPlug;
//            toPlug.m_pariedWith = fromPlug;
//            fromPlug.m_isZoneSource = true;
//            GameObject gameObject = fromPlug.gameObject;
//            gameObject.name += "FORCE PAIR";
//            var forceConnect = fromPlug.gameObject.AddComponent<ForceConnect>();
//            forceConnect.Cfg = cfg;

//            // TODO: IDinLayer: LAYER内构建顺序
//            //if (expandFrom.m_linksFrom.m_zone.IDinLayer > expandTo.m_linksFrom.m_zone.IDinLayer)
//            //{
//            //    forceConnect.ShouldFlipDoor = true;
//            //}
//        }

//        private void Connect2AreasInZone(ForceConnectCfg cfg, LG_Zone zone)
//        {
//            var from = cfg.From;
//            var to = cfg.To;

//            var fromArea = zone.m_areas[from.AreaIndex];
//            var toArea = zone.m_areas[to.AreaIndex];

//            List<(LG_ZoneExpander expandFrom, LG_ZoneExpander expandTo)> lst = new();
//            foreach (LG_ZoneExpander fromExp in fromArea.m_zoneExpanders)
//            {
//                foreach (LG_ZoneExpander toExp in toArea.m_zoneExpanders)
//                {
//                    if (Vector3.Distance(fromExp.transform.position, toExp.transform.position) <= 0.1f)
//                    {
//                        lst.Add((fromExp, toExp));
//                    }
//                }
//            }

//            if (lst.Count < 1)
//            {
//                EOSLogger.Error($"Connect2AreasInZone: cannot find gate! "
//                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
//                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
//                return;
//            }

//            EOSLogger.Warning($"Connect2AreasInZone: found {lst.Count} gates"
//                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
//                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");

//            foreach (var pair in lst)
//            {
//                var exp = pair.expandFrom;

//                var gate = exp.GetGate();
//                switch (cfg.Setting.SecurityGateToEnter)
//                {
//                    case GateType.Security:
//                        gate.ForceSecurityGate = true; break;

//                    case GateType.Apex:
//                        gate.ForceApexGate = true; break;

//                    case GateType.Bulkhead:
//                        gate.ForceSecurityGate = true;
//                        gate.ForceBulkheadGate = true; break;
//                }

//                var forceConnect = gate.gameObject.AddComponent<ForceConnect>();
//                forceConnect.Cfg = cfg;
//            }
//        }

//        public void BuildCfg(ForceConnectCfg cfg)
//        {
//            eDimensionIndex dim = cfg.DimensionIndex;

//            var from = cfg.From;
//            var to = cfg.To;

//            if (!(Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, from.Layer, from.LocalIndex, out var fromZone)
//                && 0 <= from.AreaIndex && from.AreaIndex < fromZone.m_areas.Count

//                &&

//                Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, to.Layer, to.LocalIndex, out var toZone)
//                && 0 <= to.AreaIndex && to.AreaIndex < toZone.m_areas.Count))
//            {
//                EOSLogger.Error($"CircularZones Build: Cannot find Zone 'From' or 'To'!\n"
//    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
//    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})");
//                return;
//            }

//            if (fromZone.Pointer != toZone.Pointer)
//            {
//                Connect2Zones(cfg, fromZone, toZone);
//            }
//            else
//            {
//                Connect2AreasInZone(cfg, fromZone);
//            }
//        }

//        private void Build()
//        {
//            if (definitions.TryGetValue(CurrentMainLevelLayout, out var defs))
//            {
//                defs.Definitions.ForEach(BuildCfg);
//            }
//        }
//    }
//}
