using HarmonyLib;
using Il2CppSystem.Linq;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EOSExt.ExtraDoor.Patches
{
    [HarmonyPatch]
    internal static class OnSecurityDoorSetup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.Setup))]
        private static void Postfix(LG_SecurityDoor __instance)
        {
            var fc = __instance.GetFC();
            if (fc != null)
            {
                fc.LinkedSecDoor = __instance;


                __instance.m_sync.remove_OnDoorStateChange((Il2CppSystem.Action<pDoorState, bool>)__instance.OnSyncDoorStatusChange);
                __instance.m_sync.add_OnDoorStateChange((Il2CppSystem.Action<pDoorState, bool>)__instance.FCOnSyncDoorStatusChange);

                __instance.m_anim.remove_OnDoorOpenStarted((Il2CppSystem.Action)__instance.OnDoorOpenStarted);
                __instance.m_anim.add_OnDoorOpenStarted((Il2CppSystem.Action)__instance.FCOnDoorOpenStarted);

                __instance.m_anim.remove_OnDoorIsOpen((Il2CppSystem.Action)__instance.OnDoorIsOpened);
                __instance.m_anim.add_OnDoorOpenStarted((Il2CppSystem.Action)__instance.FCOnDoorIsOpen);

            }
        }
    }
}
