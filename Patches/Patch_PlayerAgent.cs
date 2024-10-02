using EOSExt.EnvTemperature.Components;
using HarmonyLib;
using Player;

namespace EOSExt.EnvTemperature.Patches
{
    [HarmonyPatch]
    internal static class Patch_PlayerAgent
    {
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(PlayerAgent), nameof(PlayerAgent.Setup))]
        private static void Post_Setup(PlayerAgent __instance)
        {
            if (!__instance.IsLocallyOwned || __instance.gameObject.GetComponent<PlayerTemperatureManager>() != null)
                return;

            var mgr = __instance.gameObject.AddComponent<PlayerTemperatureManager>();
            
            mgr.Setup();
        }
    }
}
