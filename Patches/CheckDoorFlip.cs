using EOSExt.ExtraDoor;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.ExtraDoor.Patches
{
    [HarmonyPatch]
    internal static class CheckDoorFlip
    {
        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(LG_BuildSecurityDoorLockJob), nameof(LG_BuildSecurityDoorLockJob.CheckFlip))]
        private static bool Pre_(LG_BuildSecurityDoorLockJob __instance)
        {
            var fc = __instance.m_door.GetFC();
            if (fc == null) return true;

            var door = __instance.m_door;
            var doorDir = door.transform.forward;

            var cfg = fc.Cfg;
            var from = cfg.From;

            Builder.CurrentFloor.TryGetZoneByLocalIndex(cfg.DimensionIndex, from.Layer, from.LocalIndex, out var fromZone);
            var fromArea = fromZone.m_areas[from.AreaIndex];
            var linksFrom = door.Gate.m_linksFrom;

            if (fromArea.Pointer == linksFrom.Pointer)  // why
            {
                door.FlippedForProgresion = true;
                door.transform.rotation = Quaternion.LookRotation(door.transform.forward * -1f, door.transform.up);
            }

            return false;
        }
    }
}
