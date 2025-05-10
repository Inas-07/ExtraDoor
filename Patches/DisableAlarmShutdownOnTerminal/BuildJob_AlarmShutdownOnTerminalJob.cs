using HarmonyLib;
using LevelGeneration;

namespace EOSExt.ExtraDoor.Patches.DisableAlarmShutdownOnTerminal
{
    [HarmonyPatch]
    internal static class BuildJob_AlarmShutdownOnTerminalJob
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LG_AlarmShutdownOnTerminalJob), nameof(LG_AlarmShutdownOnTerminalJob.Build))]
        private static bool Pre_(LG_AlarmShutdownOnTerminalJob __instance) => __instance.m_securityDoor.GetFC() == null;
    }
}
