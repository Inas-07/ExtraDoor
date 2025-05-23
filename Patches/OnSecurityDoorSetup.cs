﻿using ExtraObjectiveSetup.Utils;
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
                var cfg = fc.Cfg;
                var from = cfg.From;
                var to = cfg.To;

                EOSLogger.Debug($"ForceConnect: {cfg.DimensionIndex}, ({from.Layer}, {from.LocalIndex}, {(char)('A' + from.AreaIndex)}) <-> ({to.Layer}, {to.LocalIndex}, {(char)('A' + to.AreaIndex)})\n" +
                    $"WorldEventObjectFilter: '{cfg.WorldEventObjectFilter}', FromDoorIndex: {cfg.FromDoorIndex}");
                
                ForceConnectManager.Current.RegisterFCDoor(__instance);
                fc.LinkedSecDoor = __instance;

                __instance.LinksToLayerType = to.Layer;

                var sync = __instance.m_sync.Cast<LG_Door_Sync>();
                sync.OnDoorStateChange = (Il2CppSystem.Action<pDoorState, bool>)__instance.FCOnSyncDoorStatusChange;

                var anim = __instance.m_anim.TryCast<LG_SecurityDoor_Anim>();
                if(anim != null)
                {
                    anim.OnDoorOpenStarted = (Il2CppSystem.Action)__instance.FCOnDoorOpenStarted;
                    anim.OnDoorIsOpen = (Il2CppSystem.Action)__instance.FCOnDoorIsOpen;
                }
                else
                {
                    var apexAnim = __instance.m_anim.TryCast<LG_ApexDoor_Anim>();
                    if(apexAnim != null)
                    {
                        apexAnim.OnDoorOpenStarted = (Il2CppSystem.Action)__instance.FCOnDoorOpenStarted;
                        apexAnim.OnDoorIsOpen = (Il2CppSystem.Action)__instance.FCOnDoorIsOpen;
                    }
                    else
                    {
                        EOSLogger.Error("LG_SecurityDoor: Cannot cast iLG_Door_Anim");
                    }
                }
            }
        }
    }
}
