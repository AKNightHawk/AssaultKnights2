using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    public class AKCarType : AKunitType
    {
        [FieldSerialize]
        private float maxForwardSpeed = 20;

        [FieldSerialize]
        private float nosBottel = 100.0f;

        [FieldSerialize]
        private float maxBackwardSpeed = 10;

        [FieldSerialize]
        private float driveBackwardForce = 200;

        [FieldSerialize]
        private List<Gear> gears = new List<Gear>();

        [FieldSerialize]
        private string soundGearUp;

        [FieldSerialize]
        private string soundGearDown;

        ///////////////////////////////////////////

        public class Gear
        {
            [FieldSerialize]
            private int number;

            [FieldSerialize]
            private float geardriveForwardForce = 300;

            [FieldSerialize]
            private float gearbrakeForce = 400;

            [FieldSerialize]
            private Range speedRange;

            [FieldSerialize]
            private string soundMotor;

            [FieldSerialize]
            [DefaultValue(typeof(Range), "1 1.2")]
            private Range soundMotorPitchRange = new Range(1, 1.2f);

            //
            [DefaultValue(300.0f)]
            public float GearDriveForwardForce
            {
                get { return geardriveForwardForce; }
                set { geardriveForwardForce = value; }
            }

            [DefaultValue(400.0f)]
            public float GearBrakeForce
            {
                get { return gearbrakeForce; }
                set { gearbrakeForce = value; }
            }

            [DefaultValue(0)]
            public int Number
            {
                get { return number; }
                set { number = value; }
            }

            [DefaultValue(typeof(Range), "0 0")]
            public Range SpeedRange
            {
                get { return speedRange; }
                set { speedRange = value; }
            }

            [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
            public string SoundMotor
            {
                get { return soundMotor; }
                set { soundMotor = value; }
            }

            [DefaultValue(typeof(Range), "1 1.2")]
            public Range SoundMotorPitchRange
            {
                get { return soundMotorPitchRange; }
                set { soundMotorPitchRange = value; }
            }

            public override string ToString()
            {
                return string.Format("Gear {0}", number);
            }
        }

        ///////////////////////////////////////////

        [FieldSerialize]
        private string blinkMaterialName;

        public string BlinkMaterialName
        {
            get { return blinkMaterialName; }
            set { blinkMaterialName = value; }
        }

        [DefaultValue(100.0f)]
        public float NOSBottel
        {
            get { return nosBottel; }
            set { nosBottel = value; }
        }

        [DefaultValue(20.0f)]
        public float MaxForwardSpeed
        {
            get { return maxForwardSpeed; }
            set { maxForwardSpeed = value; }
        }

        [DefaultValue(10.0f)]
        public float MaxBackwardSpeed
        {
            get { return maxBackwardSpeed; }
            set { maxBackwardSpeed = value; }
        }

        [DefaultValue(200.0f)]
        public float DriveBackwardForce
        {
            get { return driveBackwardForce; }
            set { driveBackwardForce = value; }
        }

        public List<Gear> Gears
        {
            get { return gears; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundGearUp
        {
            get { return soundGearUp; }
            set { soundGearUp = value; }
        }

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundGearDown
        {
            get { return soundGearDown; }
            set { soundGearDown = value; }
        }
    }

    public class AKCar : AKunit
    {
        private AKCarType _type = null; public new AKCarType Type { get { return _type; } }

        public float CUGear;
        private bool OnGround;
        private Track leftTrack = new Track();
        private Track rightTrack = new Track();
        private float tracksPositionYOffset;
        private float NOSBoost = 0;
        private Body chassisBody;
        private bool DownTime = false;
        private float DownPower = 0;

        //currently gears used only for sounds and RPM gage
        private AKCarType.Gear currentGear;

        private bool motorOn;
        private string currentMotorSoundName;
        private VirtualChannel motorSoundChannel;

        private bool firstTick = true;

        ///////////////////////////////////////////

        private class Track
        {
            public List<MapObjectAttachedHelper> trackHelpers = new List<MapObjectAttachedHelper>();
            public bool onGround = true;

            //animation
            public MeshObject meshObject;
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SubscribeToTickEvent();

            if (EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
            {
                if (PhysicsModel == null)
                {
                    Log.Error("Class AKCar: Physics model not exists.");
                    return;
                }

                chassisBody = PhysicsModel.GetBody("AKCar");
                if (chassisBody == null)
                {
                    Log.Error("Class AKCar: \"AKCar\" body dose not exists.");
                    return;
                }

                //chassisBody.Collision += chassisBody_Collision;

                foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
                {
                    if (attachedObject.Alias == "leftTrack")
                        leftTrack.trackHelpers.Add((MapObjectAttachedHelper)attachedObject);
                    if (attachedObject.Alias == "rightTrack")
                        rightTrack.trackHelpers.Add((MapObjectAttachedHelper)attachedObject);
                }

                if (leftTrack.trackHelpers.Count != 0)
                    tracksPositionYOffset = Math.Abs(leftTrack.trackHelpers[0].PositionOffset.Y);
            }

            //initialize currentGear
            currentGear = Type.Gears.Find(delegate(AKCarType.Gear gear)
            {
                return gear.Number == 0;
            });
        }

        protected override void OnDestroy()
        {
            if (motorSoundChannel != null)
            {
                motorSoundChannel.Stop();
                motorSoundChannel = null;
            }

            ShutdownTracksAnimation();

            base.OnDestroy();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            TickChassisGround();
            TickChassis();

            TickCurrentGear();
            TickMotorSound();

            firstTick = false;
        }

        private void TickMotorSound()
        {
            bool lastMotorOn = motorOn;
            motorOn = Intellect != null && Intellect.IsActive();

            //sound on, off
            if (motorOn != lastMotorOn)
            {
                if (!firstTick && Health != 0)
                {
                    if (motorOn)
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
                        SoundPlay3D(Type.SoundOff, .7f, true);
                    }
                }
            }

            string needSoundName = null;
            if (motorOn && currentGear != null)
                needSoundName = currentGear.SoundMotor;

            if (needSoundName != currentMotorSoundName)
            {
                //change motor sound

                if (motorSoundChannel != null)
                {
                    motorSoundChannel.Stop();
                    motorSoundChannel = null;
                }

                currentMotorSoundName = needSoundName;

                if (!string.IsNullOrEmpty(needSoundName))
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(needSoundName,
                        SoundMode.Mode3D | SoundMode.Loop);

                    if (sound != null)
                    {
                        motorSoundChannel = SoundWorld.Instance.SoundPlay(
                            sound, EngineApp.Instance.DefaultSoundChannelGroup, .3f, true);
                        motorSoundChannel.Position = Position;
                        motorSoundChannel.Pause = false;
                    }
                }
            }

            //update motor channel position and pitch
            if (motorSoundChannel != null)
            {
                Range speedRangeAbs = currentGear.SpeedRange;
                if (speedRangeAbs.Minimum < 0 && speedRangeAbs.Maximum < 0)
                    speedRangeAbs = new Range(-speedRangeAbs.Maximum, -speedRangeAbs.Minimum);
                Range pitchRange = currentGear.SoundMotorPitchRange;

                float speedAbs = Math.Abs(GetTracksSpeed());

                float speedCoef = 0;
                if (speedRangeAbs.Size() != 0)
                    speedCoef = (speedAbs - speedRangeAbs.Minimum) / speedRangeAbs.Size();
                MathFunctions.Clamp(ref speedCoef, 0, 1);

                float carpitch;
                //update channel
                if (!OnGround)
                {
                    if (Intellect.IsControlKeyPressed(GameControlKeys.Arrow_Up))
                    {
                        carpitch = pitchRange.Minimum + pitchRange.Size();
                    }
                    else
                    {
                        carpitch = pitchRange.Minimum;
                    }
                }
                else
                {
                    carpitch = pitchRange.Minimum + speedCoef * pitchRange.Size();
                }
                motorSoundChannel.Pitch = carpitch;
                motorSoundChannel.Position = Position;
            }
        }

        private void TickChassisGround()
        {
            if (chassisBody == null)
                return;

            if (chassisBody.Sleeping)
                return;

            //!!!!!
            float rayLength = .7f;

            leftTrack.onGround = false;
            rightTrack.onGround = false;

            float mass = 0;
            foreach (Body body in PhysicsModel.Bodies)
                mass += body.Mass;

            int helperCount = leftTrack.trackHelpers.Count + rightTrack.trackHelpers.Count;

            float verticalVelocity =
                (chassisBody.Rotation.GetInverse() * chassisBody.LinearVelocity).Z;

            for (int side = 0; side < 2; side++)
            {
                Track track = side == 0 ? leftTrack : rightTrack;

                foreach (MapObjectAttachedHelper trackHelper in track.trackHelpers)
                {
                    Vec3 pos;
                    Quat rot;
                    Vec3 scl;
                    trackHelper.GetGlobalTransform(out pos, out rot, out scl);

                    Vec3 downDirection = chassisBody.Rotation * new Vec3(0, 0, -rayLength);

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
                        track.onGround = true;

                        float distance = (collisionPos - start).Length();

                        if (distance < rayLength)
                        {
                            float needCoef = (rayLength - distance) / rayLength;

                            //!!!!!!

                            float force = 0;
                            //anti gravity
                            //force += (-PhysicsWorld.Instance.MainScene.Gravity.Z * mass) / (float)helperCount;
                            //anti vertical velocity
                            force += (-verticalVelocity * mass) / (float)helperCount;

                            //!!!!
                            force *= (needCoef + .45f);

                            chassisBody.AddForce(ForceType.GlobalAtGlobalPos,
                                TickDelta, new Vec3(0, 0, -force), pos);
                        }
                    }
                }
            }
        }

        protected override void OnIntellectCommand(Intellect.Command command)
        {
            base.OnIntellectCommand(command);

            if (command.KeyPressed)
            {
                if (command.Key == GameControlKeys.Reload)
                {
                    Reset(true);
                }
            }
        }

        private void TickChassis()
        {
            bool onGround = leftTrack.onGround || rightTrack.onGround;
            OnGround = onGround;
            float leftTrackThrottle = 0;
            float rightTrackThrottle = 0;

            if (Intellect != null)
            {
                Reset(false);
                Wheels();
                ShiftBooster();
                GearchangeDtime();

                //force for stability
                float speedkmph = GetRealSpeed() * 3;
                float speedpure = speedkmph - (speedkmph % 1);

                MapObjectAttachedBillboard ta1 = GetFirstAttachedObjectByAlias("tail1")
               as MapObjectAttachedBillboard;

                MapObjectAttachedBillboard ta2 = GetFirstAttachedObjectByAlias("tail2")
               as MapObjectAttachedBillboard;

                MapObjectAttachedBillboard ta3 = GetFirstAttachedObjectByAlias("tail3")
               as MapObjectAttachedBillboard;

                MapObjectAttachedBillboard ta4 = GetFirstAttachedObjectByAlias("tail4")
               as MapObjectAttachedBillboard;

                bool backlightred = false;
                bool backlightw = false;

                {
                    ServoMotor THREF = PhysicsModel.GetMotor("FB") as ServoMotor;
                    Radian FB = 0;

                    float forward = Intellect.GetControlKeyStrength(GameControlKeys.Forward);
                    leftTrackThrottle += forward;
                    rightTrackThrottle += forward;

                    float backward = Intellect.GetControlKeyStrength(GameControlKeys.Backward);
                    leftTrackThrottle -= backward;
                    rightTrackThrottle -= backward;

                    if (Intellect.IsControlKeyPressed(GameControlKeys.Forward))
                    {
                        if (GetRealSpeed() < 0.01) backlightred = true;
                        FB++;
                    }
                    else if (Intellect.IsControlKeyPressed(GameControlKeys.Backward))
                    {
                        FB--;
                        if (GetRealSpeed() > 0.5)
                        {
                            backlightred = true;
                        }
                        if (GetRealSpeed() < 0.01) backlightw = true;
                    }

                    MathFunctions.Clamp(ref FB,
                        new Degree(-1.0f).InRadians(), new Degree(1.0f).InRadians());

                    THREF.DesiredAngle = FB;
                    if (ta1 != null)
                    {
                        ta1.Visible = backlightred;
                        ta2.Visible = backlightred;
                        ta3.Visible = backlightw;
                        ta4.Visible = backlightw;
                    }
                }
                {
                    ServoMotor wmotor = PhysicsModel.GetMotor("wheel") as ServoMotor;
                    ServoMotor wmotor_2 = PhysicsModel.GetMotor("wheel2") as ServoMotor;

                    Radian needAngle = wmotor.DesiredAngle;

                    float left = Intellect.GetControlKeyStrength(GameControlKeys.Left);
                    float right = Intellect.GetControlKeyStrength(GameControlKeys.Right);

                    if (left > 0)
                    {
                        needAngle -= 0.06f;
                    }
                    else if (right > 0)
                    {
                        needAngle += 0.06f;
                    }
                    else
                    {
                        needAngle = 0f;
                    }

                    float TBaseForce = 0;
                    float Pspeed = GetRealSpeed();
                    if (Pspeed < 0) Pspeed = -Pspeed;
                    TBaseForce = left + (-right);

                    float speedcoef = 1;
                    if (GetRealSpeed() < 10 && GetRealSpeed() > -10)
                        speedcoef = GetRealSpeed() / 10;

                    if (speedcoef < 0) speedcoef = -speedcoef;

                    if (!Intellect.IsControlKeyPressed(GameControlKeys.Forward) && !Intellect.IsControlKeyPressed(GameControlKeys.Backward) && speedcoef != 1)
                    {
                        TBaseForce = TBaseForce * 1.5f;
                    }

                    if (GetRealSpeed() < 0) TBaseForce = -TBaseForce;

                    float SpeedD = 120 / GetRealSpeed();
                    MathFunctions.Clamp(ref SpeedD, 1, 1.6f);
                    float TMainForce = TBaseForce * chassisBody.Mass * SpeedD * 10;

                    if (OnGround)
                    {
                        chassisBody.AddForce(ForceType.LocalTorque, TickDelta,
                            new Vec3(0, 0, TMainForce * speedcoef * 2), Vec3.Zero);
                    }

                    MathFunctions.Clamp(ref needAngle,
               new Degree(-29.0f).InRadians(), new Degree(29.0f).InRadians());

                    if (wmotor != null)
                    {
                        wmotor.DesiredAngle = needAngle;
                        wmotor_2.DesiredAngle = needAngle;
                    }
                }
            }

            Vec3 localLinearVelocity = chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse();

            //add drive force

            float slopeForwardForceCoeffient;
            float slopeBackwardForceCoeffient;
            float slopeLinearDampingAddition;
            {
                Vec3 dir = chassisBody.Rotation.GetForward();
                Radian slopeAngle = MathFunctions.ATan(dir.Z, dir.ToVec2().Length());
                Radian maxAngle = MathFunctions.PI / 4;//new Degree(45)

                slopeForwardForceCoeffient = 1;
                if (slopeAngle > maxAngle)
                    slopeForwardForceCoeffient = 0;

                slopeBackwardForceCoeffient = 1;
                if (slopeAngle < -maxAngle)
                    slopeBackwardForceCoeffient = 0;

                MathFunctions.Clamp(ref slopeForwardForceCoeffient, 0, 1);
                MathFunctions.Clamp(ref slopeBackwardForceCoeffient, 0, 1);

                slopeLinearDampingAddition = localLinearVelocity.X > 0 ? slopeAngle : -slopeAngle;
                //slopeLinearDampingAddition *= 1;
                if (slopeLinearDampingAddition < 0)
                    slopeLinearDampingAddition = 0;
            }

            if (leftTrack.onGround)
            {
                if (leftTrackThrottle > 0 && localLinearVelocity.X < Type.MaxForwardSpeed)
                {
                    float force = localLinearVelocity.X > 0 ? currentGear.GearDriveForwardForce + NOSBoost + DownPower : currentGear.GearBrakeForce;
                    force *= leftTrackThrottle;
                    force *= slopeForwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, tracksPositionYOffset, 0));
                }

                if (leftTrackThrottle < 0 && (-localLinearVelocity.X) < Type.MaxBackwardSpeed)
                {
                    float force = currentGear.GearBrakeForce; //: Type.DriveBackwardForce;
                    force *= leftTrackThrottle;
                    force *= slopeBackwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, tracksPositionYOffset, 0));
                }
            }

            if (rightTrack.onGround)
            {
                if (rightTrackThrottle > 0 && localLinearVelocity.X < Type.MaxForwardSpeed)
                {
                    float force = localLinearVelocity.X > 0 ? currentGear.GearDriveForwardForce + NOSBoost + DownPower : currentGear.GearBrakeForce;
                    force *= rightTrackThrottle;
                    force *= slopeForwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, -tracksPositionYOffset, 0));
                }

                if (rightTrackThrottle < 0 && (-localLinearVelocity.X) < Type.MaxBackwardSpeed)
                {
                    float force = currentGear.GearBrakeForce; //: Type.DriveBackwardForce;
                    force *= rightTrackThrottle;
                    force *= slopeBackwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, -tracksPositionYOffset, 0));
                }
            }

            //LinearVelocity
            if (onGround && localLinearVelocity != Vec3.Zero)
            {
                Vec3 velocity = localLinearVelocity;
                velocity.Y = 0;
                chassisBody.LinearVelocity = chassisBody.Rotation * velocity;
            }

            bool stop = false; // onGround && leftTrackThrottle == 0 && rightTrackThrottle == 0;

            bool noLinearVelocity = chassisBody.LinearVelocity.Equals(Vec3.Zero, .2f);
            bool noAngularVelocity = chassisBody.AngularVelocity.Equals(Vec3.Zero, .2f);

            //AngularDamping
            if (onGround)
            {
                //LinearDamping
                float linearDamping;
                if (stop)
                    linearDamping = noLinearVelocity ? 1 : 1;
                else
                    linearDamping = .15f;
                chassisBody.LinearDamping = linearDamping + slopeLinearDampingAddition;

                if (stop && noAngularVelocity)
                    chassisBody.AngularDamping = 5;
                else
                    chassisBody.AngularDamping = 1;
            }
            else
            {
                chassisBody.AngularDamping = 0.55f;
                chassisBody.LinearDamping = 0.05f;
            }
        }

        private float DownTimer = 0;

        private void GearchangeDtime()
        {
            if (DownTime == true)
            {
                DownTimer = 12;
                DownTime = false;
            }
            if (DownTimer != 0)
            {
                DownPower = -(DownTimer * 4) + 2;
                DownTimer -= 0.5f;
            }
        }

        private void ShiftBooster()
        {
            MapObjectAttachedParticle NOS1 = GetFirstAttachedObjectByAlias("NOS1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle NOS2 = GetFirstAttachedObjectByAlias("NOS2") as MapObjectAttachedParticle;

            bool boosted = false;
            if (Intellect.IsControlKeyPressed(GameControlKeys.Shift))
            {
                if (Type.NOSBottel >= 5)
                {
                    boosted = true;
                    NOSBoost = 50;
                    Type.NOSBottel -= 0.5f;
                }
                else
                {
                    NOSBoost = 0;
                }
            }
            else
            {
                NOSBoost = 0;
                if (Type.NOSBottel < 100)
                {
                    Type.NOSBottel += 0.05f;
                }
            }
            if (NOS1 != null)
            {
                NOS1.Visible = boosted;
                NOS2.Visible = boosted;
            }
        }

        private void Reset(bool Mreset)
        {
            if (Mreset)
            {
                chassisBody.Rotation = new Quat(0, 0, chassisBody.Rotation.Z, chassisBody.Rotation.W);
                chassisBody.Position = new Vec3
                    (chassisBody.Position.X, chassisBody.Position.Y, chassisBody.Position.Y + 2);
            }
            //if car falls off map or goes below 1000 in z axis
            if (this.Position.Z < -1000)
            {
                this.Position = Position + new Vec3(0, 0, 10);
                this.Rotation = new Quat(0, 0, 0, 1);
            }
        }

        private void Wheels()
        {
            float wheelspeed = GetTracksSpeed() * MathFunctions.PI;
            Vec3 Wspeed = new Vec3(0, wheelspeed, 0) * chassisBody.Rotation.GetNormalize();

            foreach (Body PBW in PhysicsModel.Bodies)
            {
                for (int i = 1; i <= 8; i++)
                {
                    if (PBW.Name == ("wheel" + i))
                    {
                        if (OnGround)
                            PBW.AngularVelocity = Wspeed;
                    }
                }
            }
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);
        }

        private float GetTracksSpeed()
        {
            if (chassisBody == null)
                return 0;

            Vec3 linearVelocity = chassisBody.LinearVelocity;
            Vec3 angularVelocity = chassisBody.AngularVelocity;

            if (linearVelocity.Equals(Vec3.Zero, .1f) && angularVelocity.Equals(Vec3.Zero, .1f))
                return 0;

            Vec3 localLinearVelocity = linearVelocity * chassisBody.Rotation.GetInverse();
            return localLinearVelocity.X + Math.Abs(angularVelocity.Z) * 2;
        }

        private float GetRealSpeed()
        {
            return (chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse()).X;
        }

        private void TickCurrentGear()
        {
            if (currentGear == null)
                return;
            CUGear = currentGear.Number;
            if (motorOn)
            {
                float speed = GetTracksSpeed();
                AKCarType.Gear newGear = null;

                if (speed < currentGear.SpeedRange.Minimum || speed > currentGear.SpeedRange.Maximum)
                {
                    //find new gear
                    newGear = Type.Gears.Find(delegate(AKCarType.Gear gear)
                    {
                        return speed >= gear.SpeedRange.Minimum && speed <= gear.SpeedRange.Maximum;
                    });
                }

                if (newGear != null && currentGear != newGear)
                {
                    //change gear
                    AKCarType.Gear oldGear = currentGear;
                    OnGearChange(oldGear, newGear);
                    currentGear = newGear;
                    DownTime = true;
                }
            }
            else
            {
                if (currentGear.Number != 0)
                {
                    currentGear = Type.Gears.Find(delegate(AKCarType.Gear gear)
                    {
                        return gear.Number == 0;
                    });
                }
            }
        }

        private void OnGearChange(AKCarType.Gear oldGear, AKCarType.Gear newGear)
        {
            if (!firstTick && Health != 0)
            {
                bool up = Math.Abs(newGear.Number) > Math.Abs(oldGear.Number);
                string soundName = up ? Type.SoundGearUp : Type.SoundGearDown;
                SoundPlay3D(soundName, .7f, true);
            }
        }

        private void ShutdownTracksAnimation()
        {
            leftTrack.meshObject = null;
            rightTrack.meshObject = null;
        }
    }
}