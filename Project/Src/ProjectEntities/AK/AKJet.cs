// Copyright (C) 2006-2008 NeoAxis Group Ltd. + Mohsen Sadeghi Gol (MSG_GOL)
using System;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    public class AKJetType : AKunitType
    {
        [FieldSerialize]
        private string soundWheel;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundWheel
        {
            get { return soundWheel; }
            set { soundWheel = value; }
        }

        [FieldSerialize]
        private float maxAlt = 200;

        [FieldSerialize]
        private float MaxshiftBottel = 100.0f;

        [DefaultValue(100.0f)]
        public float ENGBoosterFuelCapacity
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

    public class AKJet : AKunit
    {
        ///////////////////////////////////////////////////////////////////////////
        private AKJetType _type = null; public new AKJetType Type { get { return _type; } }

        ///////////////////////////////////////////////////////////////////////////

        private Body AKJetBody;
        private float force = 0;
        private float forceadd = 1;
        private bool AKJetOn;
        private string currentRotorSoundName;
        private VirtualChannel rotorSoundChannel;
        private float enpitch = 0;
        public float ENGFuel;
        private float MASS = 0;
        private float VSI;
        private bool Stall = false;

        private class Wheel
        {
            public bool onGround = true;
        }

        private Wheel leftWheel = new Wheel();
        private Wheel rightWheel = new Wheel();

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            AKJetBody = PhysicsModel.GetBody("Jet");
            if (AKJetBody == null)
            {
                Log.Error("AKJet: \"AKJet\" AKJetBody does not exist.");
                return;
            }
            Stall = false;
            AddTimer();
        }

        protected override void OnTick()
        {
            base.OnTick();

            TickSound();
            TickOnGround();

            if (Intellect != null)
                TickIntellect();
        }

        private int playedright = 0;
        private int playedleft = 0;

        private void TickOnGround()
        {
            if (AKJetBody == null)
                return;

            if (AKJetBody.Sleeping)
                return;

            float rayLength = .7f;

            leftWheel.onGround = false;
            rightWheel.onGround = false;

            MapObjectAttachedHelper Leftwheel = GetFirstAttachedObjectByAlias("leftwheel") as MapObjectAttachedHelper;
            MapObjectAttachedHelper Rightwheel = GetFirstAttachedObjectByAlias("rightwheel") as MapObjectAttachedHelper;

            if (Leftwheel == null || Rightwheel == null) return;

            Vec3 pos;
            Quat rot;
            Vec3 scl;
            Vec3 downDirection = AKJetBody.Rotation * new Vec3(0, 0, -rayLength);

            //leftwheel
            {
                Leftwheel.GetGlobalTransform(out pos, out rot, out scl);

                Vec3 start = pos - downDirection;

                Ray ray = new Ray(start, downDirection);
                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);

                bool collision = false;
                Vec3 collisionPos = Vec3.Zero;

                foreach (RayCastResult result in piercingResult)
                {
                    if (Array.IndexOf(PhysicsModel.Bodies, result.Shape.Body) != -1)
                        continue;
                    collision = true;
                    collisionPos = result.Position;
                    break;
                }

                if (collision)
                {
                    leftWheel.onGround = true;
                }
            }

            //right wheel
            {
                Rightwheel.GetGlobalTransform(out pos, out rot, out scl);

                Vec3 start = pos - downDirection;

                Ray ray = new Ray(start, downDirection);
                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);

                bool collision = false;
                Vec3 collisionPos = Vec3.Zero;

                foreach (RayCastResult result in piercingResult)
                {
                    if (Array.IndexOf(PhysicsModel.Bodies, result.Shape.Body) != -1)
                        continue;
                    collision = true;
                    collisionPos = result.Position;
                    break;
                }

                if (collision)
                {
                    rightWheel.onGround = true;
                }
            }

            if (rightWheel.onGround == false) playedright = 0;
            if (rightWheel.onGround == true && playedright == 0)
            {
                SoundPlay3D(Type.SoundWheel, .7f, true);
                playedright = 1;
            }

            if (leftWheel.onGround == false) playedleft = 0;
            if (leftWheel.onGround == true && playedleft == 0)
            {
                SoundPlay3D(Type.SoundWheel, .7f, true);
                playedleft = 1;
            }
        }

        private void TickSound()
        {
            bool lastMotorOn = AKJetOn;
            AKJetOn = Intellect != null && Intellect.IsActive();

            //sound on, off
            if (AKJetOn != lastMotorOn)
            {
                if (AKJetOn)
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
                    EngineOff();
                    SoundPlay3D(Type.SoundOff, 0.7f, true);
                }
            }

            string needSoundName = null;
            if (AKJetOn)
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
            //GUItest();
            JetEngineBooster();

            VSI = AKJetBody.LinearVelocity.Z - (AKJetBody.LinearVelocity.Z % 1);
            float speed = GetRealSpeed();
            float ControlersRatio;

            MapObjectAttachedParticle ENBoosterParticle2 = GetFirstAttachedObjectByAlias("JetFire") as MapObjectAttachedParticle;
            AKunit akunit = GetPlayerUnit() as AKunit;

            Vec3 dir = AKJetBody.Rotation.GetForward();
            AKJetOn = Intellect != null && Intellect.IsActive();

            MASS = 0;
            foreach (Body body in PhysicsModel.Bodies)
            {
                MASS += body.Mass;
            }

            // controlers Ratio
            if (speed > 0)
            {
                ControlersRatio = speed / 80;
                if (ControlersRatio > 1)
                {
                    ControlersRatio = 1;
                }
            }
            else
            {
                ControlersRatio = 0;
            }
            // controlers Ratio

            //////////////////////// WING WING WING WING WING WING WING WING WING WING WING /////////////////////////////////
            //wing General
            Angles BodyAngles = Rotation.GetInverse().ToAngles();

            float normalizeRoll = BodyAngles.Roll;
            if (normalizeRoll < 0)
                normalizeRoll = -normalizeRoll;

            float PitchUp = -BodyAngles.Pitch;
            if (PitchUp < 0)
                PitchUp = 0;

            if (PitchUp > 90) PitchUp = 90 - (PitchUp - 90);

            float PitchDown = BodyAngles.Pitch;
            if (PitchDown < 0)
                PitchDown = 0;

            if (PitchDown > 90) PitchDown = 90 - (PitchDown - 90);

            //End of Wing GENERAL

            //Wing Anti Gravity Force & Stall
            float WingUpForce;

            if (speed < 40f)
            {
                //stall
                if (VSI < 0 && PitchUp > 35f)
                    Stall = true;
                else
                    Stall = false;

                //force
                WingUpForce = ((speed / 4f) - 0.2f);
            }
            else if (speed > 40f)
            {
                WingUpForce = -PhysicsWorld.Instance.MainScene.Gravity.Z;
                //WingUpForce = 9.8f; //TODO:Incin change to map gravity
            }
            else
            {
                WingUpForce = 0;
            }

            //antigrav
            AKJetBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                 AKJetBody.Rotation * new Vec3(0, 0, WingUpForce) * MASS, Vec3.Zero);

            //antivelo
            AKJetBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                 AKJetBody.Rotation * new Vec3((-WingUpForce * PitchUp / 9) / 4, 0, 0) * MASS, Vec3.Zero);

            //END oF Wing Anit Gravity Force & Stall
            //Wing Decenging Force

            float DecendSpeedForce;

            if (VSI < 0 && PitchUp == 0)
            {
                DecendSpeedForce = (180 - normalizeRoll) + PitchDown;

                AKJetBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                 AKJetBody.Rotation * new Vec3(DecendSpeedForce / 20, 0, 0) * MASS, Vec3.Zero);
            }

            //End of Wing Decenging Force
            /////// END END END ///OF OF OF/// WING WING WING WING WING WING WING WING WING WING WING //////////////////////////////

            //engine force + sound pitch control
            if (Intellect.IsControlKeyPressed(GameControlKeys.Jump))
            {
                if (akunit != null)
                {
                    akunit.GunsTryFire(false);
                }
            }

            if (Intellect.IsControlKeyPressed(GameControlKeys.Forward))
            {
                force += forceadd;
            }
            if (Intellect.IsControlKeyPressed(GameControlKeys.Backward))
            {
                force -= forceadd;
            }

            if (ENBoosterParticle2 != null)
            {
                if (force > 85f)
                    ENBoosterParticle2.Visible = true;
                else
                    ENBoosterParticle2.Visible = false;
            }

            enpitch = (force / 80f);
            MathFunctions.Clamp(ref force, 0.1f, 100);
            MathFunctions.Clamp(ref enpitch, 0.8f, 1.3f);

            //update jet channel position and pitch
            if (rotorSoundChannel != null)
            {
                //update channel
                rotorSoundChannel.Pitch = enpitch;
                rotorSoundChannel.Volume = 1;
                rotorSoundChannel.Position = Position;
                //rotorSoundChannel.MinDistance = 10;
            }

            //end of engine force + sound pitch control

            //Forces
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //start jet Pitch (Y turn)
            if (Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Up) || Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Down))
            {
                float AUp = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Up) / 2;
                float ADown = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Down) / 2;
                Hpitch += (AUp - ADown);
                MathFunctions.Clamp(ref Hpitch, -10, 10);
            }
            else
            {
                if (Hpitch != 0)
                {
                    Hpitch -= Hpitch / 5;

                    if ((Hpitch - (Hpitch % 1)) == 0)
                    {
                        Hpitch = 0;
                    }
                }
                else
                {
                    float mammadpitch = (AKJetBody.AngularVelocity * AKJetBody.Rotation.GetInverse()).Y * 2;

                    MathFunctions.Clamp(ref mammadpitch, -10f, 10);
                    AKJetBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                        AKJetBody.Rotation * new Vec3(0, 0, -mammadpitch) * MASS, new Vec3(-8, 0, 0));
                }
            }

            if (Hpitch != 0)
            {
                AKJetBody.AddForce(ForceType.GlobalTorque, TickDelta,
                 AKJetBody.Rotation * new Vec3(0, ((Hpitch) * ControlersRatio), 0) * MASS, Vec3.Zero);
            }
            //end of jet pitch (Y turn)

            //start jet Z turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.Right) || Intellect.IsControlKeyPressed(GameControlKeys.Left))
            {
                float right = Intellect.GetControlKeyStrength(GameControlKeys.Right) / 2;
                float left = Intellect.GetControlKeyStrength(GameControlKeys.Left) / 2;
                TrunZ += (left - right);
                MathFunctions.Clamp(ref TrunZ, -10, 10);
            }
            else
            {
                if (TrunZ != 0)
                {
                    TrunZ -= TrunZ / 5;

                    if ((TrunZ - (TrunZ % 1)) == 0)
                    {
                        TrunZ = 0;
                    }
                }
            }

            if (TrunZ != 0)
            {
                AKJetBody.AddForce(ForceType.GlobalTorque, TickDelta,
                 AKJetBody.Rotation * new Vec3(0, 0, (TrunZ * 2) * ControlersRatio) * MASS, Vec3.Zero);
            }
            //end of jet Z turn

            //start jet X turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Right) || Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Left))
            {
                float rightX = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Right) / 2;
                float leftX = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Left) / 2;
                TrunX += (rightX - leftX);
                MathFunctions.Clamp(ref TrunX, -10, 10);
            }
            else
            {
                if (TrunX != 0)
                {
                    TrunX -= TrunX / 5;

                    if ((TrunX - (TrunX % 1)) == 0)
                    {
                        TrunX = 0;
                    }
                }
            }

            if (TrunX != 0)
            {
                AKJetBody.AddForce(ForceType.GlobalTorque, TickDelta,
                   AKJetBody.Rotation * new Vec3(((TrunX * 2) * ControlersRatio), 0, 0) * MASS, Vec3.Zero);
            }

            //Pitch on Turn Pitch

            //float TurnPitch = AKJetBody.Rotation.GetInverse().ToAngles().Roll; //1 - Rotation.GetUp().Z;
            //if (TurnPitch < 0) TurnPitch = -TurnPitch;
            //if (TurnPitch > 90) TurnPitch = 90 - (TurnPitch - 90);

            //AKJetBody.AddForce(ForceType.GlobalTorque, TickDelta,
            //       AKJetBody.Rotation * new Vec3(0, ((-TurnPitch /90) * ControlersRatio), 0) * MASS, Vec3.Zero);

            //End of Pitch on Turn
            //end of jet X turn

            //start of adding main Engine force

            MathFunctions.Clamp(ref force, 0.1f, 100);

            //if Max Alt is not reached add jet motor force
            if (AKJetBody.Position.Z < Type.MaxAlt)
            {
                float FinalEngineForce = (force / 2f) - ((PitchUp / 90) * 30);

                AKJetBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                 new Vec3(FinalEngineForce, 0, 0) * MASS, Vec3.Zero);
            }

            //dampings
            AKJetBody.AngularDamping = 2f + (speed / 60);
            AKJetBody.LinearDamping = 0.4f;
        }

        private void JetEngineBooster()
        {
            MapObjectAttachedParticle ENBoosterParticle1 = GetFirstAttachedObjectByAlias("ENBoosterParticle1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle ENBoosterParticle2 = GetFirstAttachedObjectByAlias("ENBoosterParticle2") as MapObjectAttachedParticle;

            bool BEngBoosterON = false;
            if (Intellect.IsControlKeyPressed(GameControlKeys.SHIFT))
            {
                if (ENGFuel >= 5)
                {
                    AKJetBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                       AKJetBody.Rotation * new Vec3(40, 0, 0) * MASS, Vec3.Zero);
                    ENGFuel -= 0.5f;
                    BEngBoosterON = true;
                }
            }
            else
            {
                BEngBoosterON = false;
                if (ENGFuel < Type.ENGBoosterFuelCapacity)
                {
                    ENGFuel += 0.05f;
                }
            }
            if (ENBoosterParticle1 != null)
            {
                ENBoosterParticle1.Visible = BEngBoosterON;
                ENBoosterParticle2.Visible = BEngBoosterON;
            }
        }

        private float Groundtimer;

        public bool IsOnGround()
        {
            if (!rightWheel.onGround && !leftWheel.onGround)
            {
                Groundtimer += TickDelta;
            }
            else
            {
                Groundtimer = 0;
            }

            if (Groundtimer > 2)
                return false;
            else
                return true;
        }

        private float GetRealSpeed()
        {
            return AKJetBody.LinearVelocity.Length();
        }

        private float GetVerticalSpeed()
        {
            return (AKJetBody.Rotation.GetInverse() * AKJetBody.LinearVelocity).Z;
        }

        private Unit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject;
        }

        private void EngineOff()
        {
            //turning off some stuff when you are getting out of jet

            if (rotorSoundChannel != null)
            {
                rotorSoundChannel.Stop();
                rotorSoundChannel = null;
            }
        }

        protected override void OnDestroy()
        {
            //things we should do when jet destroys
            if (rotorSoundChannel != null)
            {
                rotorSoundChannel.Stop();
                rotorSoundChannel = null;
            }
            base.OnDestroy();
        }
    }
}

// sorry for dictation mistakes didnt have time to correct it on MS word, :D have fun ;