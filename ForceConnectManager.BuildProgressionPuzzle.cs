using AIGraph;
using EOSExt.ExtraDoor.Config;
using ExtraObjectiveSetup.Utils;
using GameData;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor
{
    public partial class ForceConnectManager
    {
        // LG_Distribute_ProgressionPuzzle
        private void BuildProgressionPuzzle()
        {
            foreach (var kv in m_doorLookup)
            {
                var door = new LG_SecurityDoor(kv.Key);
                var fc = kv.Value;
                var from = fc.Cfg.From;
                var to = fc.Cfg.To;

                // validity check
                if (!(Builder.CurrentFloor.TryGetZoneByLocalIndex(fc.Cfg.DimensionIndex, from.Layer, from.LocalIndex, out var fromZone)
                    && 0 <= from.AreaIndex && from.AreaIndex < fromZone.m_areas.Count
                    &&
                    Builder.CurrentFloor.TryGetZoneByLocalIndex(fc.Cfg.DimensionIndex, to.Layer, to.LocalIndex, out var toZone)
                    && 0 <= to.AreaIndex && to.AreaIndex < toZone.m_areas.Count))
                {
                    continue;
                }

                if (toZone.ID <= 0) continue;

                if (toZone.m_sourceExpander == null)
                {
                    EOSLogger.Error("Found zone without sourceGate AND ID over 0, id: " + fromZone.ID);
                    continue;
                }

                var ppe = fc.Cfg.Setting.ProgressionPuzzleToEnter;
                var layer = toZone.Layer;

                switch (ppe.PuzzleType)
                {
                    case eProgressionPuzzleType.Keycard_SecurityBox:
                        {
                            GateKeyItem randomKey = Builder.CurrentFloor.GetRandomKey(Builder.SessionSeedRandom);
                            LG_Factory.InjectJob(new LG_BuildKeyItemLockJob(door, randomKey), LG_Factory.BatchName.DoorLocks);

                            if (ppe.ZonePlacementData.Count > 0)
                            {
                                LG_Zone keyZone = CreateKeyItemDistribution(randomKey,
                                        ppe.ZonePlacementData[Builder.SessionSeedRandom.Range(0, ppe.ZonePlacementData.Count, "NO_TAG")],
                                        layer);

                                ProgressionObjectivesManager.RegisterProgressionObjective(
                                    toZone.DimensionIndex, toZone.Layer.m_type, toZone.LocalIndex,
                                    new ProgressionObjective_KeyCard(randomKey, toZone, keyZone).Cast<IProgressionObjective>()
                                );
                            }
                            break;
                        }

                    case eProgressionPuzzleType.PowerGenerator_And_PowerCell:
                        {
                            AIG_CourseNode nodeForOppositeZone = toZone.m_sourceGate.CoursePortal.GetNodeForOppositeZone(toZone);
                            LG_DistributeItem item = new LG_DistributeItem(ExpeditionFunction.PowerGenerator, 1f, nodeForOppositeZone, null)
                            {
                                m_assignedGate = door.Gate
                            };
                            nodeForOppositeZone.m_zone.DistributionData.GenericFunctionItems.Enqueue(item);

                            LG_Zone cellZone = null;
                            for (int k = 0; k < ppe.PlacementCount; k++)
                            {
                                LG_Zone zone = layer.m_zones[0];

                                ZonePlacementWeights weight = new();
                                if (ppe.ZonePlacementData.Count > 0)
                                {
                                    var data = ppe.ZonePlacementData[Builder.SessionSeedRandom.Range(0, ppe.ZonePlacementData.Count, "NO_TAG")];
                                    if (layer.m_zonesByLocalIndex.TryGetValue(data.LocalIndex, out var lg_Zone2))
                                    {
                                        zone = lg_Zone2;
                                        cellZone = lg_Zone2;

                                        weight = data.Weights;
                                    }
                                    else
                                    {
                                        EOSLogger.Error($"ProgressionPuzzleToEnter, Could NOT find zone with LocalIndex {data.LocalIndex} to place powerCell in to enter Zone {toZone.LocalIndex}. Faulty zone specified!? DEFAULTING TO THE FIRST ZONE");
                                    }
                                }
                                else
                                {
                                    EOSLogger.Error($"ProgressionPuzzleToEnter has NO placement data for power cell going into {layer.m_type} lg_Zone.LocalIndex");
                                }

                                LG_Factory.InjectJob(new LG_Distribute_PickupItemsPerZone(zone, 1f, ePickupItemType.BigGenericPickup, 131U, weight), LG_Factory.BatchName.Distribution);
                            }

                            // TODO: 有多个cell时，却只注册其中一个
                            if (cellZone != null)
                            {
                                ProgressionObjectivesManager.RegisterProgressionObjective(toZone.DimensionIndex, toZone.Layer.m_type, toZone.LocalIndex,
                                    new ProgressionObjective_GeneratorCell(toZone, cellZone, nodeForOppositeZone.m_zone).Cast<IProgressionObjective>());
                            }
                            break;
                        }

                    case eProgressionPuzzleType.Locked_No_Key:
                        door.SetupAsLockedNoKey(ppe.CustomText);
                        break;
                }

            }
        }

        private LG_Zone CreateKeyItemDistribution(GateKeyItem keyItem, FCZonePlacementData placementData, LG_Layer doorLayer)
        {
            EOSLogger.Log($"CreateKeyItemDistribution, keyItem: {keyItem}, placementData: {placementData}");

            LG_Zone lg_Zone = doorLayer.m_zones[0];
            LG_Zone lg_Zone2;
            if (doorLayer.m_zonesByLocalIndex.TryGetValue(placementData.LocalIndex, out lg_Zone2))
            {
                lg_Zone = lg_Zone2;
            }
            else
            {
                EOSLogger.Error($"LG_Distribute_ProgressionPuzzles.CreateKeyItemDistribution, " +
                    $"Could NOT find zone with LocalIndex {placementData.LocalIndex} to place keycard in " +
                    $"to enter Zone {lg_Zone.LocalIndex} Faulty zone specified!? " +
                    $"Dim: {doorLayer.m_dimension.DimensionIndex} " +
                    $"| DEFAULTING TO THE FIRST ZONE"
                );
            }
            ResourceContainerSpawnData resourceContainerSpawnData = new ResourceContainerSpawnData
            {
                m_type = eResourceContainerSpawnType.Keycard,
                m_keyItem = keyItem
            };
            float randomValue = Builder.BuildSeedRandom.Value("LG_Distribute_ProgressionPuzzles.CreateKeyItemDistribution_" + keyItem.PublicName);
            AIG_CourseNode aig_CourseNode = null;
            LG_DistributeItem lg_DistributeItem;
            if (LG_DistributionJobUtils.TryGetExistingZoneFunctionDistribution(lg_Zone, ExpeditionFunction.ResourceContainerWeak, Builder.SessionSeedRandom.Value("LG_Distribute_ProgressionPuzzles.CreateKeyItemDistribution_TryGetZoneFunctionDistribution"), placementData.Weights, out lg_DistributeItem, out aig_CourseNode, true, "FindContainerFor " + keyItem))
            {
                LG_DistributeResourceContainer lg_DistributeResourceContainer = lg_DistributeItem as LG_DistributeResourceContainer;
                if (lg_DistributeResourceContainer != null)
                {
                    lg_DistributeResourceContainer.m_packs.Add(resourceContainerSpawnData);
                    lg_DistributeResourceContainer.m_locked = true;
                }
                else
                {
                    EOSLogger.Error("LG_Distribute_ProgressionPuzzles.CreateKeyItemDistribution could not cast distItem as LG_DistributeResourceContainer!!");
                }
            }
            else
            {
                aig_CourseNode = LG_DistributionJobUtils.GetRandomNodeFromZoneForFunction(lg_Zone, ExpeditionFunction.ResourceContainerWeak, randomValue, 1f);
                LG_DistributeResourceContainer item = new LG_DistributeResourceContainer(ExpeditionFunction.ResourceContainerWeak, resourceContainerSpawnData, false, aig_CourseNode, Builder.BuildSeedRandom.Range(0, int.MaxValue, "LG_Distribute_ProgressionPuzzles_New_LG_DistributeResourceContainer"), Builder.BuildSeedRandom.Value("LG_Distribute_ProgressionPuzzles_New_LG_DistributeResourceContainer_Lock"), Builder.BuildSeedRandom.Range(2, 4, "LG_Distribute_ProgressionPuzzles_New_LG_DistributeResourceContainer_StoragePotential"));
                aig_CourseNode.m_zone.DistributionData.ResourceContainerItems.Enqueue(item);
                EOSLogger.Error("LG_Distribute_ProgressionPuzzles.CreateKeyItemDistribution Had to create a new resourceContainer!!");
            }

            iLG_SpawnedInNodeHandler component = keyItem.SpawnedItem.GetComponent<iLG_SpawnedInNodeHandler>();
            if (component != null)
            {
                component.SpawnNode = aig_CourseNode;
            }
            return lg_Zone;
        }
    }
}
