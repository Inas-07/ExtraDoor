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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_BuildSecurityDoorLockJob), nameof(LG_BuildSecurityDoorLockJob.CheckFlip))]
        private static void Post_(LG_BuildSecurityDoorLockJob __instance)
        {
            var fc = __instance.m_door.GetFC();
            if (fc?.ShouldFlipDoor ?? false)
            {
                LG_SecurityDoor door = __instance.m_door;
                door.FlippedForProgresion = !door.FlippedForProgresion;
                door.transform.rotation = Quaternion.LookRotation(door.transform.forward * -1f, door.transform.up);
            }
        }
    }
}
