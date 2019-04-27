using BepInEx;
using EntityStates;
using MonoMod.Cil;
using RoR2;
using RoR2.Orbs;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Tempo
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Tempo.OVerhaul", "Overhaul Mod", "1.0.0")]
    public class Overhaul : BaseUnityPlugin
    {
    public void Awake()
        {
            On.EntityStates.Huntress.ArrowRain.OnEnter += (orig, self) =>
            {

                GetPlayAnimationMethod(self, "FullBody, Override", "LoopArrowRain");
                Util.PlaySound(EntityStates.Huntress.ArrowRain.beginLoopSoundString, gameObject);
                if (NetworkServer.active)
                {
                    self.outer.commonComponents.characterBody.AddTimedBuff(BuffIndex.CommandoBoost, 5f);
                }
                self.outer.SetNextStateToMain();
            };

            On.EntityStates.Huntress.ArrowRain.OnExit += (orig, self) =>
            {
                GetPlayAnimationMethod(self,"FullBody, Override", "FireArrowRain");
                Util.PlaySound(EntityStates.Huntress.ArrowRain.endLoopSoundString, base.gameObject);
                Util.PlaySound(EntityStates.Huntress.ArrowRain.fireSoundString, base.gameObject);
                EffectManager.instance.SimpleMuzzleFlash(EntityStates.Huntress.ArrowRain.muzzleflashEffect, base.gameObject, "Muzzle", false);

                if (self.outer.commonComponents.cameraTargetParams)
                {
                    self.outer.commonComponents.cameraTargetParams.aimMode = CameraTargetParams.AimType.Standard;
                }

                //self.outer.SetNextStateToMain();
            };

            On.EntityStates.Huntress.ArrowRain.FixedUpdate += (orig, self) =>
             {
                 return;
             };

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnEnter += (orig, self) =>
             {
                 orig(self);
             };

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.OnExit += (orig, self) =>
            {
                if (!GetHasFiredBool(self))
                {
                    if (self.outer.commonComponents.characterBody.HasBuff(BuffIndex.CommandoBoost))
                    {
                        GetFireOrbArrowMethod(self);
                        GetHasFiredBool(self, 1);
                        GetFireOrbArrowMethod(self);
                        GetHasFiredBool(self, 1);
                        GetFireOrbArrowMethod(self);
                    }
                    else
                    {
                        GetFireOrbArrowMethod(self);
                    }
                }
            };

            On.EntityStates.Huntress.HuntressWeapon.FireSeekingArrow.FixedUpdate += (orig, self) =>
            {
                GetStopwatchFloat(self, Time.fixedDeltaTime);
                if (this.GetAnimator(self).GetFloat("FireSeekingShot.fire") > 0f && !this.GetHasFiredBool(self))
                {
                    Debug.LogError(self.outer.commonComponents.characterBody);
                    if (self.outer.commonComponents.characterBody.HasBuff(BuffIndex.CommandoBoost))
                    {
                        GetFireOrbArrowMethod(self);
                        GetHasFiredBool(self, 1);
                        GetFireOrbArrowMethod(self);
                        GetHasFiredBool(self, 1);
                        GetFireOrbArrowMethod(self);
                    }
                    else
                    {
                        GetFireOrbArrowMethod(self);
                    }
                }
                if (GetStopwatchFloat(self) > GetDurationFloat(self) && GetIsAuthority(self))
                {
                    self.outer.SetNextStateToMain();
                    return;
                }
            };
        }
        public float GetStopwatchFloat(object ourObject, float addToValue = 0f)
        {
            var field = ourObject.GetType().GetField("stopwatch", BindingFlags.NonPublic | BindingFlags.Instance);
            if (Mathf.Abs(addToValue) > 0f)
            {
                field.SetValue(ourObject, (float)field.GetValue(ourObject) + addToValue);
            }
            return (float)field.GetValue(ourObject);
        }

        public bool GetIsAuthority(object ourObject)
        {
            var property = ourObject.GetType().GetProperty("isAuthority", BindingFlags.NonPublic | BindingFlags.Instance);
            return (bool)property.GetValue(ourObject);
        }

        public CameraTargetParams GetCameraTargetParams(object ourObject, int toChangeToStandard = 0)
        {
            var property = ourObject.GetType().GetProperty("cameraTargetParams", BindingFlags.NonPublic | BindingFlags.Instance);
            if (Mathf.Abs(toChangeToStandard) > 0f)
            {
                Convert.ChangeType((CameraTargetParams)ourObject, property.PropertyType);
                property.SetValue(ourObject, CameraTargetParams.AimType.Standard , null);
            }
            return (CameraTargetParams)property.GetValue(ourObject);
        }

        public float GetDurationFloat(object ourObject)
        {
            var field = ourObject.GetType().GetField("duration", BindingFlags.NonPublic | BindingFlags.Instance);
            return (float)field.GetValue(ourObject);
        }
        public Animator GetAnimator(object ourObject)
        {
            var field = ourObject.GetType().GetField("animator", BindingFlags.NonPublic | BindingFlags.Instance);
            return (Animator)field.GetValue(ourObject);
        }

        public void GetFireOrbArrowMethod(object ourObject)
        {
            MethodInfo fireOrbArrowMethod = ourObject.GetType().GetMethod("FireOrbArrow", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance);
            fireOrbArrowMethod.Invoke(ourObject, null);
        }
        public bool GetHasFiredBool(object ourObject, int ourBool = 0)
        {
            var field = ourObject.GetType().GetField("hasFiredArrow", BindingFlags.NonPublic | BindingFlags.Instance);
            if (Mathf.Abs(ourBool) > 0)
            {
                field.SetValue(ourObject, !(bool)field.GetValue(ourObject));
            }
            return (bool)field.GetValue(ourObject);
        }

        public void GetPlayAnimationMethod(object ourObject, string layerName, string animationStateName)
        {
            MethodInfo playAnimationMethod = ourObject.GetType().GetMethod("PlayAnimation", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(string), typeof(string) }, null);
            playAnimationMethod.Invoke(ourObject, new string[] { layerName,animationStateName });
        }

        /*public MethodInfo GetPlayAnimationMethod(object ourObject, string layerName, string animationStateName, string playbackRateParam, float duration)
        {
            MethodInfo playAnimationMethod = ourObject.GetType().GetMethod("PlayAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
            return playAnimationMethod;
            //playAnimationMethod.Invoke(this, new object[] { layerName, animationStateName,playbackRateParam,duration });
        }*/

    }

   
}