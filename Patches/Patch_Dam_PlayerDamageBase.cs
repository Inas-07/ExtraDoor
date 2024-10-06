using EOSExt.EnvTemperature.Components;
using GameEvent;
using HarmonyLib;
using Player;
using SNetwork;

namespace EOSExt.EnvTemperature.Patches
{
    [HarmonyPatch]
    internal static class Patch_Dam_PlayerDamageBase
    {
        internal static bool s_disableDialog = false;

        [HarmonyPrefix]
        [HarmonyWrapSafe]
        [HarmonyPatch(typeof(Dam_PlayerDamageLocal), nameof(Dam_PlayerDamageLocal.ReceiveFallDamage))]
        private static bool Pre_ReceiveFallDamage(Dam_PlayerDamageLocal __instance, pMiniDamageData data)
        {
            var damage = data.damage.Get(__instance.HealthMax);
            //__instance.OnIncomingDamage(d, );
            __instance.m_nextRegen = Clock.Time + __instance.Owner.PlayerData.healthRegenStartDelayAfterDamage;
            if (__instance.Owner.IsLocallyOwned)
            {
                DramaManager.CurrentState.OnLocalDamage(damage);
                GameEventManager.PostEvent(eGameEvent.player_take_damage, __instance.Owner, damage);
            }
            else
                DramaManager.CurrentState.OnTeammatesDamage(damage);
            if (__instance.IgnoreAllDamage)
                return false;
            if (SNet.IsMaster)
            {
                bool flag = __instance.RegisterDamage(damage);
                if (flag)
                {
                    __instance.SendSetDead();
                }
                else
                {
                    __instance.SendSetHealth(__instance.Health);
                }
            }

            __instance.Hitreact(data.damage.Get(__instance.HealthMax), UnityEngine.Vector3.zero, triggerCameraShake: true, triggerGenericDialog: !s_disableDialog);
            return false;
        }
    }
}
