using EOSExt.EnvTemperature.Components;
using Gear;
using HarmonyLib;

namespace EOSExt.EnvTemperature.Patches
{
    [HarmonyPatch]
    internal static class Patch_MWS_ChargeUp_Enter
    {
        private static float DefaultChargeTime = 1f;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MWS_ChargeUp), nameof(MWS_ChargeUp.Enter))]
        private static void Postfix_Enter(MWS_ChargeUp __instance)
        {
            DefaultChargeTime = __instance.m_maxDamageTime;

            var s = PlayerTemperatureManager.GetCurrentTemperatureSetting();
            if (s != null && s.SlowDownMultiplier_Melee > 0f)
            {
                __instance.m_maxDamageTime *= s.SlowDownMultiplier_Melee;
            }
        }

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(MWS_ChargeUp), nameof(MWS_ChargeUp.Exit))]
        private static void Pre_Exit(MWS_ChargeUp __instance)
        {
            __instance.m_maxDamageTime = DefaultChargeTime;
        }
    }
}
