// Copyright (C) 2006-2008 NeoAxis Group Ltd. + Mohsen Sadeghi Gol (MSG_GOL)
using System;
using System.ComponentModel;
using Engine;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using GameCommon;

namespace GameEntities
{
    public class HelliType : AKunitType
    {
        [FieldSerialize]
        private float maxAlt = 200;

        [FieldSerialize]
        private float maxForce = 10;

        [DefaultValue(10.0f)]
        public float MaxForce
        {
            get { return maxForce; }
            set { maxForce = value; }
        }

        [FieldSerialize]
        private float MaxshiftBottel = 100.0f;

        [DefaultValue(100.0f)]
        public float MaxShiftBottel
        {
            get { return MaxshiftBottel; }
            set { MaxshiftBottel = value; }
        }

        [DefaultValue(200.0f)]
        public float MaxAlt
        {
            get { return maxAlt; }
            set { maxAlt = value; }
        }
    }

    public class Helli : AKunit
    {
        private Body HelliBody;
        private float force = 0;
        private float forceadd = 1;
        private bool dec = false;
        private bool HelliOn;
        private string currentRotorSoundName;
        private VirtualChannel rotorSoundChannel;
        private float enpitch = 0;
        public float ShiftBottel;

        private HelliType _type = null; public new HelliType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            HelliBody = PhysicsModel.GetBody("Helli");
            if (HelliBody == null)
            {
                Log.Error("Helli: \"Helli\" HelliBody dose not exists.");
                return;
            }

            AddTimer();
        }

        protected override void OnTick()
        {
            TickSound();

            base.OnTick();

            if (Intellect != null)
                TickIntellect();
        }

        private void TickSound()
        {
            bool lastMotorOn = HelliOn;
            HelliOn = Intellect != null && Intellect.IsActive();

            //sound on, off
            if (HelliOn != lastMotorOn)
            {
                if (HelliOn)
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(Type.SoundOn, SoundMode.Mode3D);
                    if (sound != null)
                    {
                        soundOnChannel = SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup, .7f, true);
                        if (soundOnChannel != null)
                        {
                            soundOnChannel.Position = Position;
                            soundOnChannel.Pause = false;
                        }
                    }
                    //SoundPlay3D(Type.SoundOn, .7f, true);
                }
                else
                {
                    MohiOff();
                    SoundPlay3D(Type.SoundOff, 0.7f, true);
                }
            }

            string needSoundName = null;
            if (HelliOn)
                needSoundName = Type.SoundIdle;
            if (needSoundName != currentRotorSoundName)
            {
                currentRotorSoundName = needSoundName;

                if (!string.IsNullOrEmpty(needSoundName))
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(needSoundName,
                        SoundMode.Mode3D | SoundMode.Loop);

                    if (sound != null)
                    {
                        rotorSoundChannel = SoundWorld.Instance.SoundPlay(
                            sound, EngineApp.Instance.DefaultSoundChannelGroup, 1, true);
                        rotorSoundChannel.Position = Position;
                        rotorSoundChannel.Pause = false;
                    }
                }
            }
        }

        private float TrunZ = 0;
        private float TrunX = 0;
        private float Hpitch = 0;

        private void TickIntellect()
        {
            ShiftBooster();

            HelliOn = Intellect != null && Intellect.IsActive();
            // GUItest();

            //finding motors
            GearedMotor main = PhysicsModel.GetMotor("hellimain") as GearedMotor;
            GearedMotor back = PhysicsModel.GetMotor("helliback") as GearedMotor;
            if (HelliOn)
            {
                main.Enabled = true;
                back.Enabled = true;
            }

            //engine force + sound pitch control

            if (Intellect.IsControlKeyPressed(GameControlKeys.Forward))
            {
                force += forceadd;
                enpitch += 0.02f;
            }
            else if (Intellect.IsControlKeyPressed(GameControlKeys.Backward))
            {
                dec = true;
                force -= forceadd;
                enpitch -= 0.02f;
            }
            else
            {
                dec = false;
                enpitch -= 0.01f;
                if (force > 50)
                {
                    force -= forceadd;
                }
                if (force < 50)
                {
                    force += forceadd;
                }
            }

            MathFunctions.Clamp(ref force, 0.1f, 100 + Type.MaxForce);
            MathFunctions.Clamp(ref enpitch, 0.8f, 1.3f);

            //update helli channel position and pitch
            if (rotorSoundChannel != null)
            {
                //update channel
                rotorSoundChannel.Pitch = enpitch;
                rotorSoundChannel.Volume = 1;
                rotorSoundChannel.Position = Position;
                rotorSoundChannel.MinDistance = 10;
            }

            //end of engine force + sound pitch control

            //Forces
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //start helli Pitch
            if (Intellect.IsControlKeyPressed(GameControlKeys.ArrowUp) || Intellect.IsControlKeyPressed(GameControlKeys.ArrowDown))
            {
                float AUp = Intellect.GetControlKeyStrength(GameControlKeys.ArrowUp) / 2;
                float ADown = Intellect.GetControlKeyStrength(GameControlKeys.ArrowDown) / 2;
                Hpitch += (AUp - ADown);
                MathFunctions.Clamp(ref Hpitch, -10, 10);

                HelliBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                    HelliBody.Rotation * new Vec3(0, 0, Hpitch / 2) * HelliBody.Mass, new Vec3(-2, 0, 0));
            }
            else
            {
                Hpitch = 0;
            }
            //end of helli pitch

            //start helli Z turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.Right) || Intellect.IsControlKeyPressed(GameControlKeys.Left))
            {
                float right = Intellect.GetControlKeyStrength(GameControlKeys.Right) / 2;
                float left = Intellect.GetControlKeyStrength(GameControlKeys.Left) / 2;
                TrunZ += (left - right);
                MathFunctions.Clamp(ref TrunZ, -10, 10);

                HelliBody.AddForce(ForceType.GlobalTorque, TickDelta,
                   HelliBody.Rotation * new Vec3(0, 0, TrunZ) * HelliBody.Mass, Vec3.Zero);
            }
            else
            {
                TrunZ = 0;
            }

            //end of helli Z turn

            //start helli X turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.ArowRight) || Intellect.IsControlKeyPressed(GameControlKeys.ArowLeft))
            {
                float rightX = Intellect.GetControlKeyStrength(GameControlKeys.ArowRight) / 2;
                float leftX = Intellect.GetControlKeyStrength(GameControlKeys.ArowLeft) / 2;
                TrunX += (rightX - leftX);
                MathFunctions.Clamp(ref TrunX, -10, 10);

                HelliBody.AddForce(ForceType.GlobalTorque, TickDelta,
                   HelliBody.Rotation * new Vec3(TrunX / 5, 0, 0) * HelliBody.Mass, Vec3.Zero);
            }
            else
            {
                TrunX = 0;
            }
            //end of helli X turn

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //start of adding force

            MathFunctions.Clamp(ref force, 0.1f, 100);

            //anti gravity when helli is not decending
            if (dec == false)
            {
                HelliBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                   new Vec3(0, 0, 2) * HelliBody.Mass, Vec3.Zero);
            }

            //if Max Alt is not reached add helli motor force
            if (GetRealAlt() < Type.MaxAlt)
            {
                HelliBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                 HelliBody.Rotation * 2 * new Vec3(0, 0, force / 7) * HelliBody.Mass, Vec3.Zero);
            }

            //dampings
            HelliBody.AngularDamping = 1.5f;
            HelliBody.LinearDamping = 0.4f;

            //another anti gravity force
            if (HelliBody.LinearVelocity.Z < 0)
            {
                HelliBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                      HelliBody.Rotation * new Vec3(0, 0, -HelliBody.LinearVelocity.Z) * HelliBody.Mass, Vec3.Zero);
            }
        }

        private float GetRealAlt()
        {
            Vec3 downDirection = HelliBody.Rotation * new Vec3(0, 0, -200f);

            Vec3 start = Position - downDirection;

            Ray ray = new Ray(start, downDirection);
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                ray, (int)ContactGroup.CastOnlyContact);

            bool collision = false;
            Vec3 collisionPos = Vec3.Zero;

            foreach (RayCastResult result in piercingResult)
            {
                collision = true;
                collisionPos = result.Position;
                break;
            }
            EngineApp.Instance.ScreenGuiRenderer.AddText("realAlt: " + collisionPos.Z.ToString(), new Vec2(.6f, .2f));
            if (collision)
                return Position.Z - collisionPos.Z;
            else
                return Position.Z;
        }

        private void ShiftBooster()
        {
            MapObjectAttachedParticle Shift1 = GetAttachedObjectByAlias("Shift1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Shift2 = GetAttachedObjectByAlias("Shift2") as MapObjectAttachedParticle;

            bool boosted = false;
            if (Intellect.IsControlKeyPressed(GameControlKeys.SHIFT))
            {
                if (ShiftBottel >= 5)
                {
                    HelliBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                       HelliBody.Rotation * new Vec3(40, 0, 0) * HelliBody.Mass, Vec3.Zero);
                    ShiftBottel -= 0.5f;
                    boosted = true;
                }
            }
            else
            {
                boosted = false;
                if (ShiftBottel < Type.MaxShiftBottel)
                {
                    ShiftBottel += 0.05f;
                }
            }
            if (Shift1 != null)
            {
                Shift1.Visible = boosted;
                Shift2.Visible = boosted;
            }
        }

        private void GUItest()
        {
            //debug GUI stuff
            float VSI = HelliBody.LinearVelocity.Z - (HelliBody.LinearVelocity.Z % 1);
            float ALT = HelliBody.Position.Z - (HelliBody.Position.Z % 1);
            float speed = GetRealSpeed() - (GetRealSpeed() % 1);

            EngineApp.Instance.ScreenGuiRenderer.AddText("VSI: " + VSI, new Vec2(.6f, .25f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("ALT: " + ALT, new Vec2(.6f, .2f));
        }

        private float GetRealSpeed()
        {
            return (HelliBody.LinearVelocity * HelliBody.Rotation.GetInverse()).X;
        }

        private float GetVerticalSpeed()
        {
            return (HelliBody.Rotation.GetInverse() * HelliBody.LinearVelocity).Z;
        }

        private void MohiOff()
        {
            //turning off some stuff when you are getting out of helli
            GearedMotor main = PhysicsModel.GetMotor("hellimain") as GearedMotor;
            GearedMotor back = PhysicsModel.GetMotor("helliback") as GearedMotor;

            main.Enabled = false;
            back.Enabled = false;

            if (rotorSoundChannel != null)
            {
                rotorSoundChannel.Stop();
                rotorSoundChannel = null;
            }
        }

        protected override void OnDestroy()
        {
            //things we should do when helli has been destroyed
            if (rotorSoundChannel != null)
            {
                rotorSoundChannel.Stop();
                rotorSoundChannel = null;
            }
            base.OnDestroy();
        }
    }
}

// LOL sorry for dictation mistakes didnt have time to correct it on MS word XD have fun ;