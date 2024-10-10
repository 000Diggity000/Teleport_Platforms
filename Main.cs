using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;

namespace Teleport_Thingy
{
    [BepInPlugin("com.000diggity000.teleport_platforms", "Teleport Platforms", "1.0.0")]
    public class Main : BaseUnityPlugin
    {
        private void Awake()
        {
            Harmony harmony = new Harmony("com.000diggity000.teleport_platforms");
            harmony.PatchAll(typeof(Patches));
            Logger.LogInfo("Teleport Platforms Loaded");
        }
    }
    public class Patches
    {
        [HarmonyPatch(typeof(TeleportIndicator), "teleportObject")]
        [HarmonyPrefix]
        public static bool Patch(TeleportIndicator __instance, ref FixTransform teleportedObj, ref Vec2 diff, ref int ___abilityLayer, ref int ___playerLayer, ref int ___groundLayer, ref TeleportedObjectEffect ___teleportingSpriteEffectPrefab)
        {
            Fix rotation = teleportedObj.rotation;
            if (teleportedObj.gameObject.layer == ___playerLayer || teleportedObj.gameObject.layer == ___abilityLayer)
            {
                PlayerPhysics component = teleportedObj.GetComponent<PlayerPhysics>();
                PlayerBody playerBody = (component != null) ? component.GetPlayerBody() : null;
                if (playerBody != null && !playerBody.IsDestroyed)
                {
                    playerBody.AttachRope(null, playerBody.topAttachment);
                    component.GetPlayerBody().selfImposedVelocity = Vec2.zero;
                }
                if (component != null && !component.IsDestroyed)
                {
                    component.UnGround(true, false);
                }
            }
            SpriteRenderer spriteRenderer = teleportedObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                SlimeController component2 = teleportedObj.GetComponent<SlimeController>();
                spriteRenderer = ((component2 != null) ? component2.GetPlayerSprite() : null);
            }
            if (spriteRenderer != null)
            {
                TeleportedObjectEffect teleportedObjectEffect = FixTransform.InstantiateFixed<TeleportedObjectEffect>(___teleportingSpriteEffectPrefab, teleportedObj.position, rotation);
                teleportedObjectEffect.Initialize(teleportedObj, spriteRenderer);
                SpriteRenderer component3 = teleportedObjectEffect.GetComponent<SpriteRenderer>();
                component3.sprite = spriteRenderer.sprite;
                if (spriteRenderer.gameObject.layer == LayerMask.NameToLayer("RigidBodyAffector"))
                {
                    component3.sprite = null;
                    SpriteRenderer component4 = component3.transform.GetChild(0).GetComponent<SpriteRenderer>();
                    if (component4 != null)
                    {
                        component4.sprite = null;
                    }
                }
                teleportedObj.position += diff;
                teleportedObj.gameObject.SetActive(false);
            }
            return false;
        }
    }
}
