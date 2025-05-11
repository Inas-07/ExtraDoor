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

            if (fromArea.Pointer != linksFrom.Pointer 
                && !door.Gate.m_hasBeenFlipped) // flipped because of plug and all that
            {
                door.FlippedForProgresion = true;

                // do flip
                door.Gate.m_hasBeenFlipped = !door.Gate.m_hasBeenFlipped;
                door.transform.rotation = Quaternion.LookRotation(door.transform.forward * -1f, door.transform.up);
            }

            return false;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(LG_BuildSecurityDoorLockJob), nameof(LG_BuildSecurityDoorLockJob.CheckFlip))]
        //private static void Post_(LG_BuildSecurityDoorLockJob __instance)
        //{
        //    var fc = __instance.m_door.GetFC();
        //    if (fc?.ShouldFlipDoor ?? false)
        //    {
        //        LG_SecurityDoor door = __instance.m_door;
        //        door.FlippedForProgresion = !door.FlippedForProgresion;
        //        door.transform.rotation = Quaternion.LookRotation(door.transform.forward * -1f, door.transform.up);
        //    }
        //}
    }
}
