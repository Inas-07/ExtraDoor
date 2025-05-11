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

            List<(LG_ZoneExpander expandFrom, LG_ZoneExpander expandTo)> lst = ForceConnectDoorUtils.GetExpandersBetween(fromArea, toArea);

            if(lst.Count < 1)
            {
                EOSLogger.Error($"ExtraDoor: cannot find gate / plug! "
                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})"
                    );
                return;
            }


            //if (cfg.FromDoorIndex < 0)
            //{
            //    EOSLogger.Warning($"ExtraDoor: FromDoorIndex < 1 ({cfg.FromDoorIndex}), will setup all doors inbetween"
            //        + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
            //        + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})"
            //    );

            //    lst.ForEach(t => Setup(t.expandFrom, t.expandTo));
            //}
            //else
            //{
            //    if (cfg.FromDoorIndex >= lst.Count)
            //    {
            //        EOSLogger.Error($"ExtraDoor: FromDoorIndex ({cfg.FromDoorIndex}) is out of range [0, {lst.Count})"
            //            + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
            //            + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})"
            //        );
            //        return;
            //    }

            //    Setup(lst[cfg.FromDoorIndex].expandFrom, lst[cfg.FromDoorIndex].expandTo);
            //}

            if (cfg.FromDoorIndex < 0 || cfg.FromDoorIndex >= lst.Count)
            {
                EOSLogger.Error($"ExtraDoor: FromDoorIndex ({cfg.FromDoorIndex}) is out of range [0, {lst.Count})"
                    + $"From:({(cfg.DimensionIndex, from.Layer, from.LocalIndex, (char)('A' + from.AreaIndex))})"
                    + $"To:({(cfg.DimensionIndex, to.Layer, to.LocalIndex, (char)('A' + to.AreaIndex))})"
                );
                return;
            }

            Setup(lst[cfg.FromDoorIndex].expandFrom, lst[cfg.FromDoorIndex].expandTo);

            void Setup(LG_ZoneExpander expandFrom, LG_ZoneExpander expandTo)
            {
                LG_Plug fromPlug = expandFrom.TryCast<LG_Plug>();
                LG_Plug toPlug = expandTo.TryCast<LG_Plug>();

                // connect by blocked plug
                if (fromPlug != null && toPlug != null)
                {
                    fromPlug.m_pariedWith = toPlug;
                    toPlug.m_pariedWith = fromPlug;

                    fromPlug.m_isZoneSource = true;
                    expandFrom.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
                    expandTo.ExpanderStatus = LG_ZoneExpanderStatus.Connected;

                    // the 2 lines are required for plug, dont remove!
                    // however, for gate, we dont need to do this
                    expandFrom.m_linksTo = expandTo.m_linksFrom;
                    expandTo.m_linksTo = expandFrom.m_linksFrom;

                    fromPlug.gameObject.name += "FORCE PAIR";
                    fromPlug.gameObject.AddComponent<ForceConnect>().Cfg = cfg;
                }

                // connect by existing gate
                else
                {
                    var gate = expandFrom.GetGate();
                    switch (cfg.Setting.SecurityGateToEnter)
                    {
                        case GateType.Security:
                            gate.ForceSecurityGate = true; break;

                        case GateType.Apex:
                            gate.ForceApexGate = true; break;

                        case GateType.Bulkhead:
                            gate.ForceSecurityGate = true;
                            gate.ForceBulkheadGate = true; break;
                    }

                    if (fromArea.m_zone.Pointer != toArea.m_zone.Pointer)
                    {
                        expandFrom.m_isZoneSource = true;

                        expandFrom.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
                        expandTo.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
                    }

                    gate.gameObject.name += "FORCE PAIR";
                    gate.gameObject.AddComponent<ForceConnect>().Cfg = cfg;
                }
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
