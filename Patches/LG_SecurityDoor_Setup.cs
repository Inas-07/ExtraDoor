using EOSExt.CircularZones;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.CircularZones.Patches
{
    [HarmonyPatch]
    internal static class LG_SecDoor
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_SecurityDoor), "Setup")]
        private static void Postfix(LG_SecurityDoor __instance)
        {
            var componentInParent = __instance.gameObject.GetComponentInParent<ForceConnect>();
            if (componentInParent != null)
            {
                componentInParent.LinkedSecDoor = __instance;
                ForceConnectManager.Current.RegisterDoor(componentInParent.ID, __instance);
            }
        }
    }
}
