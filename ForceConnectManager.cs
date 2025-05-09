using ExtraObjectiveSetup;
using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using GameData;
using GTFO.API;
using Il2CppSystem.Data;
using LevelGeneration;
using Localization;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.CircularZones
{
    public class ForceConnectManager : GenericExpeditionDefinitionManager<ForceConnectCfg>
    {
        public static ForceConnectManager Current { get; } = new();

        protected override string DEFINITION_NAME => "CircularZones";

        internal void RegisterDoor(int id, LG_SecurityDoor door)
        {
            s_DoorLookup[id] = door;
        }

        public bool TryGetDoor(int id, out LG_SecurityDoor door) => s_DoorLookup.TryGetValue(id, out door);
        
        private void Cleanup()
        {
            s_DoorIndex = 0;
            s_DoorLookup.Clear();
        }

        public int BuildCfg(ForceConnectCfg cfg)
        {
            eDimensionIndex dim = cfg.DimensionIndex;
            LG_LayerType layer = cfg.Layer;
            eLocalZoneIndex fromIndex = cfg.FromLocalIndex;
            eLocalZoneIndex toIndex = cfg.ToLocalIndex;

            int result = -1;
            if (Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, layer, fromIndex, out var lg_Zone) 
                && Builder.CurrentFloor.TryGetZoneByLocalIndex(dim, layer, toIndex, out var lg_Zone2))
            {
                List<LG_ZoneExpander> list = new List<LG_ZoneExpander>();
                List<LG_ZoneExpander> list2 = new List<LG_ZoneExpander>();
                foreach (LG_Area lg_Area in lg_Zone.m_areas)
                {
                    list.AddRange(lg_Area.m_zoneExpanders.ToArray());
                }
                foreach (LG_Area lg_Area2 in lg_Zone2.m_areas)
                {
                    list2.AddRange(lg_Area2.m_zoneExpanders.ToArray());
                }

                LG_ZoneExpander expandFrom = null, expandTo = null;
                foreach (LG_ZoneExpander lg_ZoneExpander3 in list)
                {
                    foreach (LG_ZoneExpander lg_ZoneExpander4 in list2)
                    {
                        if (Vector3.Distance(lg_ZoneExpander3.transform.position, lg_ZoneExpander4.transform.position) <= 0.1f)
                        {
                            expandFrom = lg_ZoneExpander3;
                            expandTo = lg_ZoneExpander4;
                        }
                    }
                }

                if(expandFrom == null || expandTo == null)
                {
                    if (expandFrom == null)
                    {
                        EOSLogger.Error("CircularZones Build: Cannot find 'from'");
                    }
                    else
                    {
                        EOSLogger.Error("CircularZones Build: Cannot find 'to'");
                    }

                    result = -1;
                }

                else
                {
                    expandFrom.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
                    expandTo.ExpanderStatus = LG_ZoneExpanderStatus.Connected;
                    expandFrom.m_linksTo = expandTo.m_linksFrom;
                    expandTo.m_linksTo = expandFrom.m_linksFrom;
                    LG_Plug lg_Plug = expandFrom.TryCast<LG_Plug>();
                    LG_Plug lg_Plug2 = expandTo.TryCast<LG_Plug>();
                    lg_Plug.m_pariedWith = lg_Plug2;
                    lg_Plug2.m_pariedWith = lg_Plug;
                    lg_Plug.m_isZoneSource = true;
                    GameObject gameObject = lg_Plug.gameObject;
                    gameObject.name += "FORCE PAIR";
                    var forceConnectComp = lg_Plug.gameObject.AddComponent<ForceConnect>();
                    int num = forceConnectComp.ID = s_DoorIndex;
                    bool flag6 = expandFrom.m_linksFrom.m_zone.IDinLayer > expandTo.m_linksFrom.m_zone.IDinLayer;
                    if (flag6)
                    {
                        forceConnectComp.ShouldFlipDoor = true;
                    }

                    s_DoorIndex++;
                    result = num;   
                }
            }
   
            return result;
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
            
            TryGetDoor(this._SecID_ReactorToHub, out this._Sec_ReactorToHub);
            TryGetDoor(this._SecID_HubToAttackB, out this._Sec_HubToAttackB);
            LocalizedText localizedText = new LocalizedText
            {
                Id = 0U,
                UntranslatedText = "Insufficent Power! [<color=orange>REQ::POWER_LEVEL_VI</color>]"
            };
            LocalizedText localizedText2 = new LocalizedText
            {
                Id = 0U,
                UntranslatedText = "Insufficent Power! [<color=orange>REQ::POWER_LEVEL_VII</color>]"
            };
            this._Sec_HubToAttackB.SetupAsLockedNoKey(localizedText);
            this._Sec_ReactorToHub.SetupAsLockedNoKey(localizedText2);
        }

        private int s_DoorIndex = 0;

        private static Dictionary<int, LG_SecurityDoor> s_DoorLookup = new Dictionary<int, LG_SecurityDoor>();

        private ForceConnectManager()
        {
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.PlaceAddonZones, )
            LevelAPI.OnEnterLevel += OnEnterLevel;
            LevelAPI.OnLevelCleanup += Cleanup;
        }
    }
}
