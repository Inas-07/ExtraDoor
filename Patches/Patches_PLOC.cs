using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.EnvTemperature.Patches
{
    [HarmonyPatch]
    internal static class Patches_PLOC
    {
        private const float MIN_MOD = 0.1f;

        private static float s_moveSpeedMult = 1f;

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(PLOC_Run), nameof(PLOC_Run.FixedUpdate))]
        //private static bool Pre_FixedUpdate(PLOC_Run __instance)
        //{
        //    __instance.m_owner.Stamina.UseRunStamina(Clock.FixedDelta);
        //    float stamina_mod = __instance.m_owner.Stamina.MoveSpeedModifier;
        //    float enemy_mod = __instance.m_owner.EnemyCollision.MoveSpeedModifier;

        //    float moveSpeed = Mathf.Max(
        //        __instance.m_owner.PlayerData.walkMoveSpeed * __instance.m_speedScale * s_moveSpeedMult, 
        //        __instance.m_owner.PlayerData.runMoveSpeed * __instance.m_speedScale * stamina_mod * s_moveSpeedMult
        //    )  * enemy_mod;

        //    __instance.UpdateHorizontalVelocityOnGround(moveSpeed);
        //    __instance.UpdateVerticalVelocityOnGround(__instance.m_owner.PlayerData.throttleSmoothVertical);
        //    __instance.m_owner.PlayerCharacterController.Move((__instance.m_owner.Locomotion.HorizontalVelocity + __instance.m_owner.Locomotion.VerticalVelocity) * Clock.SmoothFixedDelta);

        //    return false;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PLOC_Base), nameof(PLOC_Base.GetHorizontalVelocityFromInput))]
        private static void Pre_GetHorizontalVelocityFromInput(ref float moveSpeed)
        {
            moveSpeed *= s_moveSpeedMult;
        }

        internal static void SetMoveSpeedModifier(float m) => s_moveSpeedMult = Math.Max(MIN_MOD, m);

        internal static void ResetMoveSpeedModifier() => s_moveSpeedMult = 1f;
    }
}
