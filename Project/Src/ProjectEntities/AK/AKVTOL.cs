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
    public class AKVTOLType : AKunitType
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

    public class AKVTOL : AKunit
    {
        private Body AKVTOLBody;
        private float force = 0;
        private float EngineDir;
        private float forceadd = 1;
        private bool AKVTOLOn;
        private string currentRotorSoundName;
        private VirtualChannel rotorSoundChannel;
        private float enpitch = 0;
        public float ShiftBottel;
        private float MASS = 0;

        private Wheel leftWheel = new Wheel();
        private Wheel rightWheel = new Wheel();

        private class Wheel
        {
            public bool onGround = true;
        }

        private AKVTOLType _type = null; public new AKVTOLType Type { get { return _type; } }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            AKVTOLBody = PhysicsModel.GetBody("Jet");
            if (AKVTOLBody == null)
            {
                Log.Error("AKVTOL: \"AKVTOL\" AKVTOLBody dose not exists.");
                return;
            }

            AddTimer();

            EngineDir = 90f;
        }

        protected override void OnTick()
        {
            TickSound();

            TickOnGround();

            base.OnTick();

            if (Intellect != null)
                TickIntellect();
        }

        private int playedright = 0;
        private int playedleft = 0;

        private void TickOnGround()
        {
            if (AKVTOLBody == null)
                return;

            if (AKVTOLBody.Sleeping)
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
            Vec3 downDirection = AKVTOLBody.Rotation * new Vec3(0, 0, -rayLength);

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
            bool lastMotorOn = AKVTOLOn;
            AKVTOLOn = Intellect != null && Intellect.IsActive();

            //sound on, off
            if (AKVTOLOn != lastMotorOn)
            {
                if (AKVTOLOn)
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
            if (AKVTOLOn)
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

        private Unit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject;
        }

        private void TickIntellect()
        {
            GUIshit();
            MapObjectAttachedParticle JetFire1 = GetFirstAttachedObjectByAlias("JetFire1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle JetFire2 = GetFirstAttachedObjectByAlias("JetFire2") as MapObjectAttachedParticle;
            float speed = GetRealSpeed();

            AKunit akunit = GetPlayerUnit() as AKunit;

            Vec3 dir = AKVTOLBody.Rotation.GetForward();
            Radian slopeAngle = MathFunctions.ATan(dir.Z, dir.ToVec2().Length());

            MASS = 0;
            foreach (Body body in PhysicsModel.Bodies)
            {
                MASS += body.Mass;
            }

            ShiftBooster();

            AKVTOLOn = Intellect != null && Intellect.IsActive();
            // GUItest();
            ALTray();

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
            else if (Intellect.IsControlKeyPressed(GameControlKeys.Backward))
            {
                force -= forceadd;
            }
            else
            {
            }

            if (Intellect.IsControlKeyPressed(GameControlKeys.VerticleTakeOff_L_EngineUp))
            {
                EngineDir += 2f;
            }
            else if (Intellect.IsControlKeyPressed(GameControlKeys.VerticleTakeOff_L_EngineDown))
            {
                EngineDir -= 2f;
            }
            else
            {
            }

            MathFunctions.Clamp(ref  EngineDir, 0, 90);

            EngineApp.Instance.ScreenGuiRenderer.AddText("Throttle: " + force, new Vec2(.6f, .1f));

            if (JetFire1 != null && JetFire2 != null)
            {
                if (force > 85f)
                {
                    JetFire1.Visible = true;
                    JetFire2.Visible = true;
                }
                else
                {
                    JetFire1.Visible = false;
                    JetFire2.Visible = false;
                }
            }

            enpitch = 0.8f + (0.6f * (force / 100));
            MathFunctions.Clamp(ref force, 0.1f, 100);
            MathFunctions.Clamp(ref enpitch, 0.8f, 1.4f);

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

            //start VTOL Pitch (Y turn)
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
                    float mammadpitch = (AKVTOLBody.AngularVelocity * AKVTOLBody.Rotation.GetInverse()).Y * 2;

                    MathFunctions.Clamp(ref mammadpitch, -10f, 10);
                    AKVTOLBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                        AKVTOLBody.Rotation * new Vec3(0, 0, -mammadpitch) * MASS, new Vec3(-8, 0, 0));

                    EngineApp.Instance.ScreenGuiRenderer.AddText("MammadPitch: " + mammadpitch.ToString(), new Vec2(.1f, .2f));
                }
            }

            if (Hpitch != 0)
            {
                AKVTOLBody.AddForce(ForceType.GlobalTorque, TickDelta,
                 AKVTOLBody.Rotation * new Vec3(0, Hpitch, 0) * MASS, Vec3.Zero);
            }
            //end of VTOL pitch (Y turn)

            //start jet Z turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.Right) || Intellect.IsControlKeyPressed(GameControlKeys.Left))
            {
                float right = Intellect.GetControlKeyStrength(GameControlKeys.Right) / 2;
                float left = Intellect.GetControlKeyStrength(GameControlKeys.Left) / 2;
                TrunZ += (left - right);
                MathFunctions.Clamp(ref TrunZ, -10, 10);

                AKVTOLBody.AddForce(ForceType.LocalTorque, TickDelta,
                   new Vec3(0, 0, TrunZ * 2) * MASS, Vec3.Zero);
            }
            else
            {
                TrunZ = 0;
            }
            //end of jet Z turn

            //start jet X turn
            if (Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Right) || Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Left))
            {
                float rightX = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Right) / 2;
                float leftX = Intellect.GetControlKeyStrength(GameControlKeys.Arrow_Left) / 2;
                TrunX += (rightX - leftX);
                MathFunctions.Clamp(ref TrunX, -10, 10);

                AKVTOLBody.AddForce(ForceType.GlobalTorque, TickDelta,
                   AKVTOLBody.Rotation * new Vec3(TrunX * 2, 0, 0) * MASS, Vec3.Zero);
            }
            else
            {
                TrunX = 0;
            }

            float SHOOT = AKVTOLBody.Rotation.GetInverse().ToAngles().Roll; //1 - Rotation.GetUp().Z;
            if (SHOOT < 0) SHOOT = -SHOOT;
            if (SHOOT > 90) SHOOT = 90 - (SHOOT - 90);

            AKVTOLBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                   AKVTOLBody.Rotation * new Vec3(0, 0, -SHOOT / 180) * MASS, new Vec3(-8, 0, 0));

            //end of jet X turn

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //adding anty  Y movment force
            float Yshit = (AKVTOLBody.LinearVelocity * Rotation.GetInverse()).Y;

            EngineApp.Instance.ScreenGuiRenderer.AddText("Yshit: " + Yshit.ToString(), new Vec2(.6f, .3f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("roll: " + Rotation.ToAngles().Roll, new Vec2(.6f, .35f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("roll: " + (Rotation.GetInverse().ToAngles()).ToString(), new Vec2(.6f, .4f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("Edir: " + EngineDir.ToString(), new Vec2(.1f, .6f));

            //start of adding force

            MathFunctions.Clamp(ref force, 0.1f, 100);

            EngineApp.Instance.ScreenGuiRenderer.AddText("speed: " + GetRealSpeed().ToString(), new Vec2(.6f, .15f));
            GUItest();
            //anti gravity when jet have speed (wings force)

            float antyshityforcy = GetRealSpeed() / 10;

            float slopeangleshit = 1.5f;

            MathFunctions.Clamp(ref antyshityforcy, 0, 10);
            if (slopeAngle > 0)
            {
                slopeangleshit = 1.5f - slopeAngle;
            }
            else
            {
                slopeangleshit = 0.5f;
            }

            if (GetRealSpeed() > 0)
            {
                AKVTOLBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                   new Vec3(0, 0, antyshityforcy * slopeangleshit) * MASS, Vec3.Zero);
            }

            //if Max Alt is not reached add jet motor force
            if (AKVTOLBody.Position.Z < Type.MaxAlt)
            {
                AKVTOLBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                AKVTOLBody.Rotation * new Vec3(force / 2f * ((90 - EngineDir) / 90), 0, force / 8f * (EngineDir / 90)) * MASS, Vec3.Zero);
            }

            //dampings
            AKVTOLBody.AngularDamping = 2f + (speed / 60);
            AKVTOLBody.LinearDamping = 0.6f;

            ServoMotor Lenginem = PhysicsModel.GetMotor("LEngineM") as ServoMotor;
            ServoMotor Renginem = PhysicsModel.GetMotor("REngineM") as ServoMotor;

            if (Lenginem != null && Renginem != null)
            {
                float EngingDirRad = EngineDir * MathFunctions.PI / 180;
                Renginem.DesiredAngle = EngingDirRad;
                Lenginem.DesiredAngle = EngingDirRad;
            }
        }

        private void ShiftBooster()
        {
            MapObjectAttachedParticle Shift1 = GetFirstAttachedObjectByAlias("Shift1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Shift2 = GetFirstAttachedObjectByAlias("Shift2") as MapObjectAttachedParticle;

            bool boosted = false;
            if (Intellect.IsControlKeyPressed(GameControlKeys.SHIFT))
            {
                if (ShiftBottel >= 5)
                {
                    AKVTOLBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                       AKVTOLBody.Rotation * new Vec3(40, 0, 0) * MASS, Vec3.Zero);
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
            }
            if (Shift2 != null)
            {
                Shift2.Visible = boosted;
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

        private void ALTray()
        {
            //geting real ALT by ray casting
            float realalt = 0;
            Vec3 downDirection = AKVTOLBody.Rotation * new Vec3(0, 0, -0.7f);
            Vec3 start = AKVTOLBody.Position - downDirection;

            Ray ray = new Ray(start, downDirection);
            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                ray, (int)ContactGroup.CastOnlyContact);

            foreach (RayCastResult result in piercingResult)
            {
                realalt = result.Distance;
                break;
            }
        }

        private void GUItest()
        {
            //debug GUI stuff
            float VSI = AKVTOLBody.LinearVelocity.Z - (AKVTOLBody.LinearVelocity.Z % 1);
            float ALT = AKVTOLBody.Position.Z - (AKVTOLBody.Position.Z % 1);
            float speed = GetRealSpeed() - (GetRealSpeed() % 1);

            EngineApp.Instance.ScreenGuiRenderer.AddText("VSI: " + VSI, new Vec2(.6f, .25f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("ALT: " + ALT, new Vec2(.6f, .2f));
            EngineApp.Instance.ScreenGuiRenderer.AddText("Gravity: " + PhysicsWorld.Instance.MainScene.Gravity.ToString(), new Vec2(.4f, .1f));
        }

        private float GetRealSpeed()
        {
            return AKVTOLBody.LinearVelocity.Length(); // * AKVTOLBody.Rotation.GetInverse()).X;
        }

        private float GetVerticalSpeed()
        {
            return (AKVTOLBody.Rotation.GetInverse() * AKVTOLBody.LinearVelocity).Z;
        }

        private void MohiOff()
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
            //things we should do when jet has been destroyed
            if (rotorSoundChannel != null)
            {
                rotorSoundChannel.Stop();
                rotorSoundChannel = null;
            }
            base.OnDestroy();
        }

        private void GUIshit()
        {
        }
    }
}

// LOL sorry for dictation mistakes didnt have time to correct it on MS word XD have fun ;