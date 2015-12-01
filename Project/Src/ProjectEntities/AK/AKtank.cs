// Copyright (C) 2006-2008 NeoAxis Group Ltd.
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
    /// <summary>
    /// Defines the <see cref="AKtank"/> entity type.
    /// </summary>
    public class AKtankType : AKunitType
    {
        [FieldSerialize]
        private float maxForwardSpeed = 20;

        [FieldSerialize]
        private float maxBackwardSpeed = 10;

        [FieldSerialize]
        private float driveForwardForce = 300;

        [FieldSerialize]
        private float driveBackwardForce = 200;

        [FieldSerialize]
        private float brakeForce = 400;

        [FieldSerialize]
        private List<Gear> gears = new List<Gear>();

        [FieldSerialize]
        private string soundGearUp;

        [FieldSerialize]
        private string soundGearDown;

        [FieldSerialize]
        [DefaultValue(typeof(Vec2), "1 0")]
        private Vec2 tracksAnimationMultiplier = new Vec2(1, 0);

        ///////////////////////////////////////////

        public class Gear
        {
            [FieldSerialize]
            private int number;

            [FieldSerialize]
            private Range speedRange;

            [FieldSerialize]
            private string soundMotor;

            [FieldSerialize]
            [DefaultValue(typeof(Range), "1 1.2")]
            private Range soundMotorPitchRange = new Range(1, 1.2f);

            //

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

        [DefaultValue(300.0f)]
        public float DriveForwardForce
        {
            get { return driveForwardForce; }
            set { driveForwardForce = value; }
        }

        [DefaultValue(200.0f)]
        public float DriveBackwardForce
        {
            get { return driveBackwardForce; }
            set { driveBackwardForce = value; }
        }

        [DefaultValue(400.0f)]
        public float BrakeForce
        {
            get { return brakeForce; }
            set { brakeForce = value; }
        }

        public List<Gear> Gears
        {
            get { return gears; }
            //get { return gearsauto }; // bool -- shift or autoshift
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

        [DefaultValue(typeof(Vec2), "1 0")]
        public Vec2 TracksAnimationMultiplier
        {
            get { return tracksAnimationMultiplier; }
            set { tracksAnimationMultiplier = value; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            if (!string.IsNullOrEmpty(SoundOn))
                SoundWorld.Instance.SoundCreate(SoundOn, SoundMode.Mode3D);
            if (!string.IsNullOrEmpty(SoundOff))
                SoundWorld.Instance.SoundCreate(SoundOff, SoundMode.Mode3D);
            if (!string.IsNullOrEmpty(SoundGearUp))
                SoundWorld.Instance.SoundCreate(SoundGearUp, SoundMode.Mode3D);
            if (!string.IsNullOrEmpty(SoundGearDown))
                SoundWorld.Instance.SoundCreate(SoundGearDown, SoundMode.Mode3D);
            if (!string.IsNullOrEmpty(SoundTowerTurn))
                SoundWorld.Instance.SoundCreate(SoundTowerTurn, SoundMode.Mode3D | SoundMode.Loop);

            foreach (Gear gear in gears)
            {
                if (!string.IsNullOrEmpty(gear.SoundMotor))
                {
                    SoundWorld.Instance.SoundCreate(gear.SoundMotor, SoundMode.Mode3D |
                        SoundMode.Loop);
                }
            }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    public class AKtank : AKunit
    {
        private Track leftTrack = new Track();
        private Track rightTrack = new Track();
        private float tracksPositionYOffset;

        private Body chassisBody;
        private float chassisSleepTimer;

        private AKtankType.Gear currentGear;

        private bool motorOn;
        private string currentMotorSoundName;
        private VirtualChannel motorSoundChannel;

        private bool firstTick = true;

        //Minefield specific
        private float minefieldUpdateTimer;

        ///////////////////////////////////////////

        private class Track
        {
            public class WheelObject
            {
                public MapObjectAttachedMesh mesh;
                public float circumference;
            }

            public List<WheelObject> wheels = new List<WheelObject>();
            public List<MapObjectAttachedHelper> trackHelpers = new List<MapObjectAttachedHelper>();
            public bool onGround = true;

            //animation
            public List<MeshObject> meshObjects = new List<MeshObject>();

            public Vec2 materialScrollValue;

            public float speed;
            public float server_sentSpeed;
        }

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            TracksSpeedToClient,
        }

        private AKtankType _type = null; public new AKtankType Type { get { return _type; } }

        public AKtank()
        {
            //Minefield specific
            minefieldUpdateTimer = World.Instance.Random.NextFloat();
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            AddTimer();

            if (EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
            {
                if (PhysicsModel == null)
                {
                    Log.Error("AKtank: Physics model not exists.");
                    return;
                }

                chassisBody = PhysicsModel.GetBody("chassis");
                if (chassisBody == null)
                {
                    Log.Error("AKtank: \"chassis\" body not exists.");
                    return;
                }

                foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
                {
                    if (attachedObject.Alias == "leftTrack")
                        leftTrack.trackHelpers.Add((MapObjectAttachedHelper)attachedObject);
                    if (attachedObject.Alias == "rightTrack")
                        rightTrack.trackHelpers.Add((MapObjectAttachedHelper)attachedObject);

                    if (attachedObject.Alias == "leftWheel")
                    {
                        Track.WheelObject wo = new Track.WheelObject();
                        wo.mesh = (MapObjectAttachedMesh)attachedObject;
                        float rad = wo.mesh.MeshObject.Bounds.GetRadius();
                        wo.circumference = 2 * MathFunctions.PI * rad;
                        leftTrack.wheels.Add(wo);
                    }
                    if (attachedObject.Alias == "rightWheel")
                    {
                        Track.WheelObject wo = new Track.WheelObject();
                        wo.mesh = (MapObjectAttachedMesh)attachedObject;
                        float rad = wo.mesh.MeshObject.Bounds.GetRadius();
                        wo.circumference = 2 * MathFunctions.PI * rad;
                        leftTrack.wheels.Add(wo);
                    }
                }

                if (leftTrack.trackHelpers.Count != 0)
                    tracksPositionYOffset = Math.Abs(leftTrack.trackHelpers[0].PositionOffset.Y);
            }

            //initialize currentGear
            currentGear = Type.Gears.Find(delegate(AKtankType.Gear gear)
            {
                return gear.Number == 0;
            });

            //replace track materials
            if (EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
                InitTracksAnimation();
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

        protected override void OnSuspendPhysicsDuringMapLoading(bool suspend)
        {
            base.OnSuspendPhysicsDuringMapLoading(suspend);

            //After loading a map, the physics simulate 5 seconds, that bodies have fallen asleep.
            //During this time we will disable physics for this entity.
            if (PhysicsModel != null)
            {
                foreach (Body body in PhysicsModel.Bodies)
                    body.Static = suspend;
            }
        }

        private void CalculateTracksSpeed()
        {
            leftTrack.speed = 0;
            rightTrack.speed = 0;

            if (chassisBody == null)
                return;

            if (chassisBody.Sleeping)
                return;

            Vec3 linearVelocity = chassisBody.LinearVelocity;
            Vec3 angularVelocity = chassisBody.AngularVelocity;

            //optimization
            if (linearVelocity.Equals(Vec3.Zero, .1f) && angularVelocity.Equals(Vec3.Zero, .1f))
                return;

            Vec3 localLinearVelocity = linearVelocity * chassisBody.Rotation.GetInverse();
            leftTrack.speed = localLinearVelocity.X - angularVelocity.Z * 2;
            rightTrack.speed = localLinearVelocity.X + angularVelocity.Z * 2;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            CalculateTracksSpeed();
            if (EntitySystemWorld.Instance.IsServer())
                Server_SendTracksSpeedToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);

            TickChassisGround();
            TickChassis();

            TickCurrentGear();
            TickMotorSound();
            TickWheelRotation();
            TickTurnOver();

            //Minefield specific
            TickMinefields();

            firstTick = false;
        }

        protected override void Client_OnTick()
        {
            base.Client_OnTick();

            TickCurrentGear();
            TickMotorSound();
            TickWheelRotation();
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
                        SoundPlay3D(Type.SoundOff, .7f, true);
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

                float speedAbs = Math.Max(Math.Abs(leftTrack.speed), Math.Abs(rightTrack.speed));

                float speedCoef = 0;
                if (speedRangeAbs.Size() != 0)
                    speedCoef = (speedAbs - speedRangeAbs.Minimum) / speedRangeAbs.Size();
                MathFunctions.Clamp(ref speedCoef, 0, 1);

                //update channel
                motorSoundChannel.Pitch = pitchRange.Minimum + speedCoef * pitchRange.Size();
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
                            force += (-PhysicsWorld.Instance.MainScene.Gravity.Z * mass) / (float)helperCount;
                            //anti vertical velocity
                            force += (-verticalVelocity * mass) / (float)helperCount;

                            //!!!!
                            force *= (needCoef + .45f);

                            chassisBody.AddForce(ForceType.GlobalAtGlobalPos,
                                TickDelta, new Vec3(0, 0, force), pos);
                        }
                    }
                }
            }
        }

        private void TickChassis()
        {
            bool onGround = leftTrack.onGround || rightTrack.onGround;

            float leftTrackThrottle = 0;
            float rightTrackThrottle = 0;
            if (Intellect != null)
            {
                float forward = Intellect.GetControlKeyStrength(GameControlKeys.Forward);
                leftTrackThrottle += forward;
                rightTrackThrottle += forward;

                float backward = Intellect.GetControlKeyStrength(GameControlKeys.Backward);
                leftTrackThrottle -= backward;
                rightTrackThrottle -= backward;

                float left = Intellect.GetControlKeyStrength(GameControlKeys.Left);
                leftTrackThrottle -= left * 2;
                rightTrackThrottle += left * 2;

                float right = Intellect.GetControlKeyStrength(GameControlKeys.Right);
                leftTrackThrottle += right * 2;
                rightTrackThrottle -= right * 2;

                MathFunctions.Clamp(ref leftTrackThrottle, -1, 1);
                MathFunctions.Clamp(ref rightTrackThrottle, -1, 1);
            }

            //return if no throttle and sleeping
            if (chassisBody.Sleeping && rightTrackThrottle == 0 && leftTrackThrottle == 0)
                return;

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
                    float force = localLinearVelocity.X > 0 ? Type.DriveForwardForce : Type.BrakeForce;
                    force *= leftTrackThrottle;
                    force *= slopeForwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, tracksPositionYOffset, 0));
                }

                if (leftTrackThrottle < 0 && (-localLinearVelocity.X) < Type.MaxBackwardSpeed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.BrakeForce : Type.DriveBackwardForce;
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
                    float force = localLinearVelocity.X > 0 ? Type.DriveForwardForce : Type.BrakeForce;
                    force *= rightTrackThrottle;
                    force *= slopeForwardForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, -tracksPositionYOffset, 0));
                }

                if (rightTrackThrottle < 0 && (-localLinearVelocity.X) < Type.MaxBackwardSpeed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.BrakeForce : Type.DriveBackwardForce;
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

            bool stop = onGround && leftTrackThrottle == 0 && rightTrackThrottle == 0;

            bool noLinearVelocity = chassisBody.LinearVelocity.Equals(Vec3.Zero, .2f);
            bool noAngularVelocity = chassisBody.AngularVelocity.Equals(Vec3.Zero, .2f);

            //LinearDamping
            float linearDamping;
            if (stop)
                linearDamping = noLinearVelocity ? 5 : 1;
            else
                linearDamping = .15f;
            chassisBody.LinearDamping = linearDamping + slopeLinearDampingAddition;

            //AngularDamping
            if (onGround)
            {
                if (stop && noAngularVelocity)
                    chassisBody.AngularDamping = 5;
                else
                    chassisBody.AngularDamping = 1;
            }
            else
                chassisBody.AngularDamping = .15f;

            //sleeping
            if (!chassisBody.Sleeping && stop && noLinearVelocity && noAngularVelocity)
            {
                chassisSleepTimer += TickDelta;
                if (chassisSleepTimer > 1)
                    chassisBody.Sleeping = true;
            }
            else
                chassisSleepTimer = 0;
        }

        private float rollAngle = 0;

        private void TickWheelRotation()
        {
            for (int nTrack = 0; nTrack < 2; nTrack++)
            {
                Track track = nTrack == 0 ? leftTrack : rightTrack;

                //update value
                if (EntitySystemWorld.Instance.Simulation &&
                    !EntitySystemWorld.Instance.SystemPauseOfSimulation)
                {
                    foreach (Track.WheelObject wheel in track.wheels)
                    {
                        if (wheel.mesh == null)
                            continue;

                        float value = -(track.speed / wheel.circumference);
                        wheel.mesh.RotationOffset = new Angles(0, rollAngle += value, 0).ToQuat();
                    }
                }
            }
        }

        private void TickCurrentGear()
        {
            //currently gears used only for sounds

            if (currentGear == null)
                return;

            if (motorOn)
            {
                float speed = Math.Max(leftTrack.speed, rightTrack.speed);

                AKtankType.Gear newGear = null;

                if (speed < currentGear.SpeedRange.Minimum || speed > currentGear.SpeedRange.Maximum)
                {
                    //find new gear
                    newGear = Type.Gears.Find(delegate(AKtankType.Gear gear)
                    {
                        return speed >= gear.SpeedRange.Minimum && speed <= gear.SpeedRange.Maximum;
                    });
                }

                if (newGear != null && currentGear != newGear)
                {
                    //change gear
                    AKtankType.Gear oldGear = currentGear;
                    OnGearChange(oldGear, newGear);
                    currentGear = newGear;
                }
            }
            else
            {
                if (currentGear.Number != 0)
                {
                    currentGear = Type.Gears.Find(delegate(AKtankType.Gear gear)
                    {
                        return gear.Number == 0;
                    });
                }
            }
        }

        private void OnGearChange(AKtankType.Gear oldGear, AKtankType.Gear newGear)
        {
            if (!firstTick && Health != 0)
            {
                bool up = Math.Abs(newGear.Number) > Math.Abs(oldGear.Number);
                string soundName = up ? Type.SoundGearUp : Type.SoundGearDown;
                SoundPlay3D(soundName, .7f, true);
            }
        }

        private void InitTracksAnimation()
        {
            for (int nTrack = 0; nTrack < 2; nTrack++)
            {
                Track track = nTrack == 0 ? leftTrack : rightTrack;
                string alias = nTrack == 0 ? "leftTrackMesh" : "rightTrackMesh";

                foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
                {
                    if (attachedObject.Alias == alias)
                    {
                        MapObjectAttachedMesh attachedMesh = GetFirstAttachedObjectByAlias(alias)
                            as MapObjectAttachedMesh;
                        if (attachedMesh != null)
                        {
                            track.meshObjects.Add(attachedMesh.MeshObject);
                            track.meshObjects[track.meshObjects.Count - 1].AddToRenderQueue +=
                                TrackMeshObject_AddToRenderQueue;
                        }
                    }
                }
            }
        }

        private void ShutdownTracksAnimation()
        {
            leftTrack.meshObjects.Clear();
            rightTrack.meshObjects.Clear();
        }

        private float tracksTextureAnimationRenderTime;

        private void TrackMeshObject_AddToRenderQueue(MovableObject sender, Camera camera,
            bool onlyShadowCasters, ref bool allowRender)
        {
            float renderTime = RendererWorld.Instance.FrameRenderTime;
            if (tracksTextureAnimationRenderTime != renderTime)
            {
                tracksTextureAnimationRenderTime = renderTime;

                UpdateTracksTextureAnimation();
            }
        }

        private void UpdateTracksTextureAnimation()
        {
            for (int nTrack = 0; nTrack < 2; nTrack++)
            {
                Track track = nTrack == 0 ? leftTrack : rightTrack;
                float s = (track.speed * RendererWorld.Instance.FrameRenderTimeStep);
                //update value
                if (EntitySystemWorld.Instance.Simulation &&
                    !EntitySystemWorld.Instance.SystemPauseOfSimulation)
                {
                    Vec2 value = track.materialScrollValue + Type.TracksAnimationMultiplier *
                        (track.speed * RendererWorld.Instance.FrameRenderTimeStep);

                    while (value.X < 0) value.X++;
                    while (value.X >= 1) value.X--;
                    while (value.Y < 0) value.Y++;
                    while (value.Y >= 1) value.Y--;

                    track.materialScrollValue = value;
                }

                { //update track scroll
                    MathFunctions.Clamp(ref s, -1, 1);
                    Vec4 value = new Vec4(track.materialScrollValue.X, 0, 0, 0);
                    Vec4 v = new Vec4(s, 0, 0, 0);
                    foreach (MeshObject meshObject in track.meshObjects)
                    {
                        foreach (MeshObject.SubObject subObject in meshObject.SubObjects)
                        {
                            //update SubObject dynamic gpu parameter
                            subObject.SetCustomGpuParameter(
                                (int)ShaderBaseMaterial.GpuParameters.diffuse1MapTransformAdd, value);
                            subObject.SetCustomGpuParameter(
                                (int)ShaderBaseMaterial.GpuParameters.specularMapTransformAdd, value);
                            subObject.SetCustomGpuParameter(
                                (int)ShaderBaseMaterial.GpuParameters.normalMapTransformAdd, value);
                        }
                    }
                }
            }
        }

        private void TickTurnOver()
        {
            if (Rotation.GetUp().Z < .2f)
                Die();
        }

        //Minefield specific
        private void TickMinefields()
        {
            minefieldUpdateTimer -= TickDelta;
            if (minefieldUpdateTimer > 0)
                return;
            minefieldUpdateTimer += 1;

            if (chassisBody != null && chassisBody.LinearVelocity != Vec3.Zero)
            {
                Minefield minefield = Minefield.GetMinefieldByPosition(Position);
                if (minefield != null)
                {
                    Die();
                }
            }
        }

        protected override void Server_OnClientConnectedAfterPostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            RemoteEntityWorld[] worlds = new RemoteEntityWorld[] { remoteEntityWorld };
            Server_SendTracksSpeedToClients(worlds);
        }

        private void Server_SendTracksSpeedToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            const float epsilon = .25f;

            bool leftUpdate = Math.Abs(leftTrack.speed - leftTrack.server_sentSpeed) > epsilon ||
                (leftTrack.speed == 0 && leftTrack.server_sentSpeed != 0);
            bool rightUpdate = Math.Abs(rightTrack.speed - rightTrack.server_sentSpeed) > epsilon ||
                (rightTrack.speed == 0 && rightTrack.server_sentSpeed != 0);

            if (leftUpdate || rightUpdate)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKtank),
                    (ushort)NetworkMessages.TracksSpeedToClient);
                writer.Write(leftTrack.speed);
                writer.Write(rightTrack.speed);
                EndNetworkMessage();

                leftTrack.server_sentSpeed = leftTrack.speed;
                rightTrack.server_sentSpeed = rightTrack.speed;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TracksSpeedToClient)]
        private void Client_ReceiveTracksSpeed(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            float value1 = reader.ReadSingle();
            float value2 = reader.ReadSingle();
            if (!reader.Complete())
                return;
            leftTrack.speed = value1;
            rightTrack.speed = value2;
        }
    }
}