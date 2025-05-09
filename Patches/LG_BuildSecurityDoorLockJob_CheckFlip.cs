using EOSExt.CircularZones;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.CircularZones.Patches
{
    [HarmonyPatch]
    internal static class LG_BuildSecurityDoorLockJob_CheckFlip
    {
        [HarmonyPatch(typeof(LG_BuildSecurityDoorLockJob), nameof(LG_BuildSecurityDoorLockJob.CheckFlip))]
        private static void Postfix(LG_BuildSecurityDoorLockJob __instance)
        {
            var componentInParent = __instance.m_door.gameObject.GetComponentInParent<ForceConnect>();
            bool flag = componentInParent != null && componentInParent.ShouldFlipDoor;
            if (flag)
            {
                LG_SecurityDoor door = __instance.m_door;
                door.FlippedForProgresion = !door.FlippedForProgresion;
                door.transform.rotation = Quaternion.LookRotation(door.transform.forward * -1f, door.transform.up);
            }
        }
    }
}
