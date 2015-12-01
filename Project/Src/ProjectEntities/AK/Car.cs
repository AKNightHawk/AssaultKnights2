// Nmechanics (car physic) Source License V1.
// Copyright (C) 2009 Nmechanics GSA.
// www.nmechanics.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.Utils;
using GameCommon;

namespace GameEntities
{
    /// <summary>
    /// Defines the <see cref="Car"/> entity type.
    /// </summary>

    #region Parameters of the car ( resource editor )

    public class CarType : AKunitType
    {
        /*[FieldSerialize]
     bool visible;

     [FieldSerialize]
     float oncarparticle_applyspeed = 10.0f;*/

        [FieldSerialize]
        private float carspeed_applyparticle = 30.0f;

        [FieldSerialize]
        private float wheel_friction = 0.6f;

        [FieldSerialize]
        private float hand_brake_wheelfriction = 0.4f;

        [FieldSerialize]
        private float shock_absorber = 0.002f;

        [FieldSerialize]
        private float ray_length = 0.7f;

        [FieldSerialize]
        private float force_freestop_car = 0.3f;

        [FieldSerialize]
        private float max_force_angle = 8.0f;

        [FieldSerialize]
        private float speed10_rotation = 8;

        //
        [FieldSerialize]
        private float speed20_rotation = 6;

        //
        [FieldSerialize]
        private float speed30_rotation = 3;

        //
        [FieldSerialize]
        private float speed40_rotation = 1.5f;

        //
        [FieldSerialize]
        private float speed50_rotation = 1;

        //
        [FieldSerialize]
        private float speed60_rotation = 1;

        //
        [FieldSerialize]
        private float speed70_rotation = 1;

        //
        [FieldSerialize]
        private float speed80_rotation = 1;

        //
        [FieldSerialize]
        private float speed90_rotation = 1;

        //
        [FieldSerialize]
        private float speed100_rotation = 1;

        [FieldSerialize]
        private float max_forward_speed = 100;

        [FieldSerialize]
        private float max_backward_speed = 100;

        [FieldSerialize]
        private float drive_forward_force = 220;

        [FieldSerialize]
        private float drive_backward_force = 100;

        [FieldSerialize]
        private float brake_force = 80;

        [FieldSerialize]
        private float hand_brake = 900.0f;

        [FieldSerialize]
        [DefaultValue(typeof(Vec2), "1 0")]
        private Vec2 wheelsAnimationMultiplier = new Vec2(1, 0);

        [FieldSerialize]
        private List<Gear> gears = new List<Gear>();

        [FieldSerialize]
        private string soundGearUp;

        [FieldSerialize]
        private string soundGearDown;

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

        [DefaultValue(30.0f)]
        public float Carspeed_applyparticle
        {
            get { return carspeed_applyparticle; }
            set { carspeed_applyparticle = value; }
        }

        [DefaultValue(0.6f)]
        public float Wheel_friction
        {
            get { return wheel_friction; }
            set { wheel_friction = value; }
        }

        [DefaultValue(0.4f)]
        public float Hand_brake_wheelfriction
        {
            get { return hand_brake_wheelfriction; }
            set { hand_brake_wheelfriction = value; }
        }

        [DefaultValue(0.002f)]
        public float Shock_absorber
        {
            get { return shock_absorber; }
            set { shock_absorber = value; }
        }

        [DefaultValue(0.7f)]
        public float Ray_length
        {
            get { return ray_length; }
            set { ray_length = value; }
        }

        [DefaultValue(0.3f)]
        public float Force_freestop_car
        {
            get { return force_freestop_car; }
            set { force_freestop_car = value; }
        }

        [DefaultValue(8.0f)]
        public float Max_force_angle
        {
            get { return max_force_angle; }
            set { max_force_angle = value; }
        }

        [DefaultValue(8.0f)]
        public float Speed10_rotation
        {
            get { return speed10_rotation; }
            set { speed10_rotation = value; }
        }

        [DefaultValue(6.0f)]
        public float Speed20_rotation
        {
            get { return speed20_rotation; }
            set { speed20_rotation = value; }
        }

        [DefaultValue(3.0f)]
        public float Speed30_rotation
        {
            get { return speed30_rotation; }
            set { speed30_rotation = value; }
        }

        [DefaultValue(1.5f)]
        public float Speed40_rotation
        {
            get { return speed40_rotation; }
            set { speed40_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed50_rotation
        {
            get { return speed50_rotation; }
            set { speed50_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed60_rotation
        {
            get { return speed60_rotation; }
            set { speed60_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed70_rotation
        {
            get { return speed70_rotation; }
            set { speed70_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed80_rotation
        {
            get { return speed80_rotation; }
            set { speed80_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed90_rotation
        {
            get { return speed90_rotation; }
            set { speed90_rotation = value; }
        }

        [DefaultValue(1.0f)]
        public float Speed100_rotation
        {
            get { return speed100_rotation; }
            set { speed100_rotation = value; }
        }

        [DefaultValue(100.0f)]
        public float Max_forward_speed
        {
            get { return max_forward_speed; }
            set { max_forward_speed = value; }
        }

        [DefaultValue(100.0f)]
        public float Max_backward_speed
        {
            get { return max_backward_speed; }
            set { max_backward_speed = value; }
        }

        [DefaultValue(220.0f)]
        public float Drive_forward_force
        {
            get { return drive_forward_force; }
            set { drive_forward_force = value; }
        }

        [DefaultValue(100.0f)]
        public float Drive_backward_force
        {
            get { return drive_backward_force; }
            set { drive_backward_force = value; }
        }

        [DefaultValue(900.0f)]
        public float Hand_brake
        {
            get { return hand_brake; }
            set { hand_brake = value; }
        }

        [DefaultValue(80.0f)]
        public float Brake_force
        {
            get { return brake_force; }
            set { brake_force = value; }
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

    #endregion Parameters of the car ( resource editor )

    #region Car unit

    public class Car : AKunit
    {
        private Wheel LFWheel = new Wheel();
        private Wheel RFWheel = new Wheel();
        private Wheel LRWheel = new Wheel();
        private Wheel RRWheel = new Wheel();

        private float LFWheelPositionYOffset;
        private float RFWheelPositionYOffset;
        private float LRWheelPositionYOffset;
        private float RRWheelPositionYOffset;

        //Minefield specific
        private float minefieldUpdateTimer;

        private Body chassisBody;
        private VirtualChannel motorSoundChannel;
        private bool firstTick = true;
        private float chassisSleepTimer = 0;

        private class Wheel
        {
            public List<MapObjectAttachedHelper> wheelHelpers = new List<MapObjectAttachedHelper>();
            public bool onGround = true;
        }

        ///////////////////////////////////////////
        private CarType _type = null; public new CarType Type { get { return _type; } }

        public Car()
        {
            //Minefield specific
            minefieldUpdateTimer = World.Instance.Random.NextFloat();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            if (!EntitySystemWorld.Instance.IsEditor())
            {
                if (PhysicsModel == null)
                {
                    Log.Error("Vehicle: Physics model not exists.");
                    return;
                }

                chassisBody = PhysicsModel.GetBody("chassis");
                if (chassisBody == null)
                {
                    Log.Error("Vehicle: \"chassis\" body not exists.");
                    return;
                }

                foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
                {
                    if (attachedObject.Alias == "LFWheel")
                        LFWheel.wheelHelpers.Add((MapObjectAttachedHelper)attachedObject);
                    if (attachedObject.Alias == "RFWheel")
                        RFWheel.wheelHelpers.Add((MapObjectAttachedHelper)attachedObject);
                    if (attachedObject.Alias == "LRWheel")
                        LRWheel.wheelHelpers.Add((MapObjectAttachedHelper)attachedObject);
                    if (attachedObject.Alias == "RRWheel")
                        RRWheel.wheelHelpers.Add((MapObjectAttachedHelper)attachedObject);
                }
                if (LFWheel.wheelHelpers.Count != 0)
                    LFWheelPositionYOffset = Math.Abs(LFWheel.wheelHelpers[0].PositionOffset.Y);
                if (LRWheel.wheelHelpers.Count != 0)
                    LRWheelPositionYOffset = Math.Abs(LRWheel.wheelHelpers[0].PositionOffset.Y);
                if (RFWheel.wheelHelpers.Count != 0)
                    RFWheelPositionYOffset = Math.Abs(RFWheel.wheelHelpers[0].PositionOffset.Y);
                if (RRWheel.wheelHelpers.Count != 0)
                    RRWheelPositionYOffset = Math.Abs(RRWheel.wheelHelpers[0].PositionOffset.Y);

                //initialize currentGear
                currentGear = Type.Gears.Find(delegate(CarType.Gear gear)
                {
                    return gear.Number == 0;
                });
            }

            AddTimer();
        }

        protected override void OnDestroy()
        {
            if (motorSoundChannel != null)
            {
                motorSoundChannel.Stop();
                motorSoundChannel = null;
            }

            base.OnDestroy();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            TickShock_absorber_backward();
            TickShock_absorber_forward();
            TickChassis_backward_wheel();
            TickChassis_forward_wheel();
            TickChassis_hand_brake();
            TickRotation();

            TickCurrentGear();
            TickMotorSound();

            firstTick = false;
        }

    #endregion Car unit

        #region Shock absorber backward

        private void TickShock_absorber_backward()
        {
            if (chassisBody == null) return;
            if (chassisBody.Sleeping) return;

            //!!!!!
            float rayLength = Type.Ray_length;

            LRWheel.onGround = false;
            RRWheel.onGround = false;

            float mass = 0;
            foreach (Body body in PhysicsModel.Bodies)
                mass += body.Mass;

            int helperCount = LRWheel.wheelHelpers.Count + RRWheel.wheelHelpers.Count;

            float verticalVelocity = (chassisBody.Rotation.GetInverse() * chassisBody.LinearVelocity).Z;

            for (int side = 0; side < 2; side++)
            {
                Wheel wheel = side == 0 ? LRWheel : RRWheel;
                foreach (MapObjectAttachedHelper wheelHelper in wheel.wheelHelpers)
                {
                    Vec3 pos;
                    Quat rot;
                    Vec3 scl;

                    wheelHelper.GetGlobalTransform(out pos, out rot, out scl);

                    Vec3 downDirection = chassisBody.Rotation * new Vec3(0, 0, -rayLength);
                    Vec3 start = pos - downDirection;

                    Ray ray = new Ray(start, downDirection);
                    RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);

                    bool collision = false;
                    Vec3 collisionPos = Vec3.Zero;

                    foreach (RayCastResult result in piercingResult)
                    {
                        if (Array.IndexOf(PhysicsModel.Bodies, result.Shape.Body) != -1) continue;
                        collision = true;

                        collisionPos = result.Position;
                        break;
                    }
                    if (collision)
                    {
                        wheel.onGround = true;
                        float distance = (collisionPos - start).Length();

                        if (distance < rayLength)
                        {
                            float needCoef = (rayLength - distance) / rayLength;
                            float force = 0;

                            //anti gravity
                            force += (-PhysicsWorld.Instance.MainScene.Gravity.Z * mass) / (float)helperCount;

                            //anti vertical velocity
                            force += (-verticalVelocity * mass) / (float)helperCount;

                            force *= (needCoef + Type.Shock_absorber);

                            chassisBody.AddForce(ForceType.GlobalAtGlobalPos,
                                TickDelta, new Vec3(0, 0, force), pos);
                        }
                    }
                }
            }
        }

        #endregion Shock absorber backward

        #region Shock absorber forward

        private void TickShock_absorber_forward()
        {
            if (chassisBody == null)
                return;
            if (chassisBody.Sleeping)
                return;

            //!!!!!
            float rayLength = Type.Ray_length;

            LFWheel.onGround = false;
            RFWheel.onGround = false;

            float mass = 0;
            foreach (Body body in PhysicsModel.Bodies)
                mass += body.Mass;

            int helperCount = LFWheel.wheelHelpers.Count + RFWheel.wheelHelpers.Count;

            float verticalVelocity = (chassisBody.Rotation.GetInverse() * chassisBody.LinearVelocity).Z;

            for (int side = 0; side < 2; side++)
            {
                Wheel wheel = side == 0 ? LFWheel : RFWheel;
                foreach (MapObjectAttachedHelper wheelHelper in wheel.wheelHelpers)
                {
                    Vec3 pos;
                    Quat rot;
                    Vec3 scl;
                    wheelHelper.GetGlobalTransform(out pos, out rot, out scl);

                    Vec3 downDirection = chassisBody.Rotation * new Vec3(0, 0, -rayLength);
                    Vec3 start = pos - downDirection;

                    Ray ray = new Ray(start, downDirection);
                    RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    ray, (int)ContactGroup.CastOnlyContact);
                    bool collision = false;

                    Vec3 collisionPos = Vec3.Zero;
                    foreach (RayCastResult result in piercingResult)
                    {
                        if (Array.IndexOf(PhysicsModel.Bodies, result.Shape.Body) != -1) continue;
                        collision = true;

                        collisionPos = result.Position;
                        break;
                    }
                    if (collision)
                    {
                        wheel.onGround = true;
                        float distance = (collisionPos - start).Length();

                        if (distance < rayLength)
                        {
                            float needCoef = (rayLength - distance) / rayLength;
                            float force = 0;

                            //anti gravity
                            force += (-PhysicsWorld.Instance.MainScene.Gravity.Z * mass) / (float)helperCount;

                            //anti vertical velocity
                            force += (-verticalVelocity * mass) / (float)helperCount;
                            force *= (needCoef + Type.Shock_absorber);

                            chassisBody.AddForce(ForceType.GlobalAtGlobalPos,
                            TickDelta, new Vec3(0, 0, force), pos);
                        }
                    }
                }
            }
        }

        #endregion Shock absorber forward

        #region Backward wheel

        private void TickChassis_backward_wheel()
        {
            bool onGround = LRWheel.onGround || RRWheel.onGround;

            float LRWheelThrottle = 0;
            float RRWheelThrottle = 0;

            if (Intellect != null)
            {
                float forward = Intellect.GetControlKeyStrength(GameControlKeys.Forward);
                LRWheelThrottle += forward;
                RRWheelThrottle += forward;

                float backward = Intellect.GetControlKeyStrength(GameControlKeys.Backward);
                LRWheelThrottle -= backward;
                RRWheelThrottle -= backward;

                MathFunctions.Clamp(ref LRWheelThrottle, -1, 1);
                MathFunctions.Clamp(ref RRWheelThrottle, -1, 1);
            }

            //return if no throttle and sleeping
            if (chassisBody.Sleeping && LRWheelThrottle == 0 && RRWheelThrottle == 0)
                return;

            Vec3 localLinearVelocity = chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse();

            if (LRWheel.onGround)
            {
                if (LRWheelThrottle > 0 && localLinearVelocity.X < Type.Max_forward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.Drive_forward_force : Type.Brake_force;
                    force *= LRWheelThrottle;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }

                if (LRWheelThrottle < 0 && (-localLinearVelocity.X) < Type.Max_backward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.Brake_force : Type.Drive_backward_force;
                    force *= LRWheelThrottle;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }
            }

            if (RRWheel.onGround)
            {
                if (RRWheelThrottle > 0 && localLinearVelocity.X < Type.Max_forward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.Drive_forward_force : Type.Brake_force;
                    force *= RRWheelThrottle;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }

                if (RRWheelThrottle < 0 && (-localLinearVelocity.X) < Type.Max_backward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.Brake_force : Type.Drive_backward_force;
                    force *= RRWheelThrottle;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }
            }

        #endregion Backward wheel

            #region Wheel friction

            float hand_brake_wheelfriction = 0;
            if (Intellect != null)
            {
                float hbwf = Intellect.GetControlKeyStrength(GameControlKeys.Jump);
                hand_brake_wheelfriction += hbwf;
                MathFunctions.Clamp(ref hand_brake_wheelfriction, 0, 1);
            }

            if (hand_brake_wheelfriction == 0 && onGround && localLinearVelocity != Vec3.Zero)
            {
                Vec3 velocity = localLinearVelocity;
                float wheel_friction = Type.Wheel_friction;
                if (velocity.Y > 0.2f)
                {
                    velocity.Y -= wheel_friction;
                    if (velocity.Y == 0) { wheel_friction = 0; }
                }
                if (-velocity.Y > 0.2f)
                {
                    velocity.Y += wheel_friction;
                    if (velocity.Y == 0) { wheel_friction = 0; }
                }

                chassisBody.LinearVelocity = chassisBody.Rotation * velocity;
            }
            if (hand_brake_wheelfriction > 0 && onGround && localLinearVelocity != Vec3.Zero)
            {
                Vec3 velocity = localLinearVelocity;
                float wheel_friction = Type.Hand_brake_wheelfriction;
                if (velocity.Y > 0.2f)
                {
                    velocity.Y -= wheel_friction;
                    if (velocity.Y == 0) { wheel_friction = 0; }
                }
                if (-velocity.Y > 0.2f)
                {
                    velocity.Y += wheel_friction;
                    if (velocity.Y == 0) { wheel_friction = 0; }
                }

                chassisBody.LinearVelocity = chassisBody.Rotation * velocity;
            }

            #endregion Wheel friction

            #region Force free stop car

            bool stop = onGround && LRWheelThrottle == 0 && RRWheelThrottle == 0;

            bool noLinearVelocity = chassisBody.LinearVelocity.Equals(Vec3.Zero, .2f);
            bool noAngularVelocity = chassisBody.AngularVelocity.Equals(Vec3.Zero, .2f);

            //LinearDamping
            float linearDamping;
            if (stop)
                linearDamping = noLinearVelocity ? 5 : Type.Force_freestop_car;
            else
                linearDamping = Type.Force_freestop_car;
            chassisBody.LinearDamping = linearDamping;

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

            #endregion Force free stop car

        #region Hand brake

        private void TickChassis_hand_brake()
        {
            skidsound(false);
            bool onGround = LRWheel.onGround || RRWheel.onGround;

            float hand_brake_f = 0;
            float hand_brake_b = 0;

            if (Intellect != null)
            {
                Ñar_particle();
                float h_brake = Intellect.GetControlKeyStrength(GameControlKeys.Jump);
                hand_brake_f += h_brake;
                hand_brake_b -= h_brake;
                MathFunctions.Clamp(ref hand_brake_f, -1, 1);
                MathFunctions.Clamp(ref hand_brake_b, -1, 1);
            }

            Vec3 localLinearVelocity = chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse();
            float tm_hb = Type.Hand_brake / 2;
            if (LRWheel.onGround && RRWheel.onGround)
            {
                if (hand_brake_f > 0 && localLinearVelocity.X < Type.Max_forward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? tm_hb : Type.Hand_brake;
                    force *= hand_brake_f;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }

                if (hand_brake_b < 0 && (-localLinearVelocity.X) < Type.Max_backward_speed)
                {
                    float force = localLinearVelocity.X > 0 ? Type.Hand_brake : tm_hb;
                    force *= hand_brake_b;

                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, 0, 0));
                }
            }
        }

        #endregion Hand brake

        #region Ñar particle

        private void Ñar_particle()
        {
            float carspeed = GetWheelsSpeed();

            MapObjectAttachedParticle Carspeed_applyparticle1 = GetAttachedObjectByAlias("Carspeed_applyparticle1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Carspeed_applyparticle2 = GetAttachedObjectByAlias("Carspeed_applyparticle2") as MapObjectAttachedParticle;

            bool Hs1 = false;
            bool Cp1 = false;
            float speed_cp = GetWheelsSpeed();
            bool on_whell = LRWheel.onGround || RRWheel.onGround;
            if (speed_cp > Type.Carspeed_applyparticle & on_whell) { Cp1 = true; }
            float ang = chassisBody.AngularVelocity.Z;
            float ang2 = ang * carspeed * 2;
            if (carspeed > 20)
            {
                ang2 = ang * carspeed / 2;
            }
            else if (carspeed > 50)
            {
                ang2 = ang * carspeed / 10;
            }
            if (Intellect.IsControlKeyPressed(GameControlKeys.Jump))
            {
                skidsound(true);
                //EngineApp.Instance.ScreenGuiRenderer.AddText("ang: " + ang2, new Vec2(.6f, .4f));
                if (carspeed > 10 & on_whell)
                {
                    Hs1 = true;
                    chassisBody.AngularDamping = 0;
                }
            }
            else
            {
                //dumpings
                chassisBody.AngularDamping = 3;
            }

            Carspeed_applyparticle1.Visible = Hs1;
            Carspeed_applyparticle2.Visible = Cp1;
        }

        private void skidsound(bool SS)
        {
            bool on_whell_sound = LRWheel.onGround || RRWheel.onGround;
            float carspeed = GetWheelsSpeed();
            MapObjectAttachedSound HB = GetAttachedObjectByAlias("Hand_brake") as MapObjectAttachedSound;
            if (HB == null)
                return;
            if (!SS) { HB.Volume = 0; return; }
            float hb = 0;
            MathFunctions.Clamp(ref hb, 0, 1);
            if (carspeed > 10 & on_whell_sound)
            {
                hb = carspeed / 30;
            }
            HB.Volume = hb;
        }

        #endregion Ñar particle

        #region Forward wheel

        private void TickChassis_forward_wheel()
        {
            bool onGround = LFWheel.onGround || RFWheel.onGround;

            float LFWheelThrottle = 0;
            float RFWheelThrottle = 0;

            if (Intellect != null)
            {
                float left = Intellect.GetControlKeyStrength(GameControlKeys.Left);
                LFWheelThrottle -= left * 2;
                RFWheelThrottle += left * 2;

                float right = Intellect.GetControlKeyStrength(GameControlKeys.Right);
                LFWheelThrottle += right * 2;
                RFWheelThrottle -= right * 2;

                MathFunctions.Clamp(ref LFWheelThrottle, -1, 1);
                MathFunctions.Clamp(ref RFWheelThrottle, -1, 1);
            }

            //return if no throttle and sleeping
            if (chassisBody.Sleeping && LFWheelThrottle == 0 && RFWheelThrottle == 0)
                return;

            //Vec3 localLinearVelocity = chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse();

            float sp = GetWheelsSpeed();
            //add drive force
            float slopeLeftForceCoeffient;
            float slopeRightForceCoeffient;
            float radius = 0;

            {
                Vec3 dir = chassisBody.Rotation.GetForward();
                Radian slopeAngle = MathFunctions.ATan(dir.Z, dir.GetNormalize().Length());
                Radian maxAngle = MathFunctions.PI / 4;

                //%_forward_speed==============================================
                float sf1 = (Type.Max_forward_speed * 1) / 100;
                float sf10 = (Type.Max_forward_speed * 10) / 100;
                float sf20 = (Type.Max_forward_speed * 20) / 100;
                float sf30 = (Type.Max_forward_speed * 30) / 100;
                float sf40 = (Type.Max_forward_speed * 40) / 100;
                float sf50 = (Type.Max_forward_speed * 50) / 100;
                float sf60 = (Type.Max_forward_speed * 60) / 100;
                float sf70 = (Type.Max_forward_speed * 70) / 100;
                float sf80 = (Type.Max_forward_speed * 80) / 100;
                float sf90 = (Type.Max_forward_speed * 90) / 100;
                float sf110 = (Type.Max_forward_speed * 110) / 100;
                //%_forward_speed==============================================

                if (sp < 1)
                    radius = Type.Speed10_rotation;
                if (sp >= sf1 & sp < sf10)
                    radius = Type.Speed10_rotation;
                if (sp >= sf10 & sp < sf20)
                    radius = Type.Speed20_rotation;
                if (sp >= sf20 & sp < sf30)
                    radius = Type.Speed30_rotation;
                if (sp >= sf30 & sp < sf40)
                    radius = Type.Speed40_rotation;
                if (sp >= sf40 & sp < sf50)
                    radius = Type.Speed50_rotation;
                if (sp >= sf50 & sp < sf60)
                    radius = Type.Speed60_rotation;
                if (sp >= sf60 & sp < sf70)
                    radius = Type.Speed70_rotation;
                if (sp >= sf70 & sp < sf80)
                    radius = Type.Speed80_rotation;
                if (sp >= sf80 & sp < sf90)
                    radius = Type.Speed90_rotation;
                if (sp >= sf90 & sp < sf110)
                    radius = Type.Speed100_rotation;

                //==================================
                slopeLeftForceCoeffient = radius;
                //zero
                if (slopeAngle > maxAngle)
                    slopeLeftForceCoeffient = 0;
                slopeRightForceCoeffient = radius;
                //zero
                if (slopeAngle < -maxAngle)
                    slopeRightForceCoeffient = 0;
                //==================================

                MathFunctions.Clamp(ref slopeLeftForceCoeffient, -Type.Max_force_angle, Type.Max_force_angle);
                MathFunctions.Clamp(ref slopeRightForceCoeffient, -Type.Max_force_angle, Type.Max_force_angle);
            }

            if (LFWheel.onGround)
            {
                if (LFWheelThrottle > 0)
                {
                    float force = sp;
                    force *= LFWheelThrottle;
                    force *= slopeLeftForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, LFWheelPositionYOffset, 0));
                }

                if (LFWheelThrottle < 0)
                {
                    float force = sp;
                    force *= LFWheelThrottle;
                    force *= slopeLeftForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, LFWheelPositionYOffset, 0));
                }
            }

            if (RFWheel.onGround)
            {
                if (RFWheelThrottle > 0)
                {
                    float force = sp;
                    force *= RFWheelThrottle;
                    force *= slopeRightForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, -RFWheelPositionYOffset, 0));
                }

                if (RFWheelThrottle < 0)
                {
                    float force = sp;
                    force *= RFWheelThrottle;
                    force *= slopeRightForceCoeffient;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(force, 0, 0), new Vec3(0, -RFWheelPositionYOffset, 0));
                }
            }
        }

        #endregion Forward wheel

        #region Animation wheel rotation

        private void TickRotation()
        {
            bool onGround = LFWheel.onGround || RFWheel.onGround;

            float carspeed = GetWheelsSpeed();
            float LFWheelThrottle = 0;
            float RFWheelThrottle = 0;

            if (Intellect != null)
            {
                Body WFR = PhysicsModel.GetBody("FR");
                Body WFL = PhysicsModel.GetBody("FL");
                Body WRR = PhysicsModel.GetBody("RR");
                Body WRL = PhysicsModel.GetBody("RL");

                float wheelspeed = GetWheelsSpeed() * 2;
                Vec3 Wspeed = new Vec3(0, wheelspeed,
                0) * chassisBody.Rotation.GetNormalize();

                if (onGround)
                {
                    WFR.AngularVelocity = Wspeed;
                    WFL.AngularVelocity = Wspeed;
                    WRR.AngularVelocity = Wspeed;
                    WRL.AngularVelocity = Wspeed;
                }
                {
                    ServoMotor motor_1 = PhysicsModel.GetMotor("wheel1") as ServoMotor;
                    ServoMotor motor_2 = PhysicsModel.GetMotor("wheel2") as ServoMotor;

                    if (motor_1 != null)
                    {
                        if (motor_2 != null)
                        {
                            float turnspeed = 0.5f;
                            MathFunctions.Clamp(ref turnspeed, 0.5f, 0.5f);
                            Radian needAngle = motor_1.DesiredAngle;
                            if (Intellect.IsControlKeyPressed(GameControlKeys.Left))
                            {
                                if (carspeed > 2)
                                {
                                    LFWheelThrottle -= turnspeed;
                                    RFWheelThrottle += turnspeed;
                                }
                                if (carspeed < -1)
                                {
                                    LFWheelThrottle += turnspeed;
                                    RFWheelThrottle -= turnspeed;
                                }
                                needAngle -= 0.2f;
                            }
                            else if (Intellect.IsControlKeyPressed(GameControlKeys.Right))
                            {
                                if (carspeed > 2)
                                {
                                    LFWheelThrottle += turnspeed;
                                    RFWheelThrottle -= turnspeed;
                                }
                                if (carspeed < -1)
                                {
                                    LFWheelThrottle -= turnspeed;
                                    RFWheelThrottle += turnspeed;
                                }
                                needAngle += 0.2f;
                            }
                            else
                            {
                                needAngle = 0f;
                            }

                            MathFunctions.Clamp(ref needAngle, new Degree(-37.0f).InRadians(),
                            new Degree(37.0f).InRadians());
                            motor_1.DesiredAngle = needAngle;
                            motor_2.DesiredAngle = needAngle;
                        }
                    }
                }
            }
        }

        #endregion Animation wheel rotation

        #region Get wheels speed

        private float GetWheelsSpeed()
        {
            if (chassisBody == null)
                return 0;

            Vec3 linearVelocity = chassisBody.LinearVelocity;
            Vec3 angularVelocity = chassisBody.AngularVelocity;

            //optimization
            if (linearVelocity.Equals(Vec3.Zero, .2f) && angularVelocity.Equals(Vec3.Zero, .2f))
                return 0;

            Vec3 localLinearVelocity = linearVelocity * chassisBody.Rotation.GetInverse();

            //not ideal true
            return localLinearVelocity.X + Math.Abs(angularVelocity.Z) * 2;
        }

        #endregion Get wheels speed

        private CarType.Gear currentGear;
        private bool motorOn;
        private string currentMotorSoundName;

        private void TickCurrentGear()
        {
            if (currentGear == null)
                return;
            if (motorOn)
            {
                float speed = GetWheelsSpeed();
                CarType.Gear newGear = null;

                if (speed < currentGear.SpeedRange.Minimum || speed > currentGear.SpeedRange.Maximum)
                {
                    //find new gear
                    newGear = Type.Gears.Find(delegate(CarType.Gear gear)
                    {
                        return speed >= gear.SpeedRange.Minimum && speed <= gear.SpeedRange.Maximum;
                    });
                }

                if (newGear != null && currentGear != newGear)
                {
                    //change gear
                    CarType.Gear oldGear = currentGear;
                    OnGearChange(oldGear, newGear);
                    currentGear = newGear;
                }
            }
            else
            {
                if (currentGear.Number != 0)
                {
                    currentGear = Type.Gears.Find(delegate(CarType.Gear gear)
                    {
                        return gear.Number == 0;
                    });
                }
            }
        }

        private void OnGearChange(CarType.Gear oldGear, CarType.Gear newGear)
        {
            if (!firstTick && Life != 0)
            {
                bool up = Math.Abs(newGear.Number) > Math.Abs(oldGear.Number);
                string soundName = up ? Type.SoundGearUp : Type.SoundGearDown;
                SoundPlay3D(soundName, .7f, true);
            }
        }

        private void TickMotorSound()
        {
            bool lastMotorOn = motorOn;
            motorOn = Intellect != null && Intellect.IsActive();

            //sound on, off
            if (motorOn != lastMotorOn)
            {
                if (!firstTick && Life != 0)
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

                float speedAbs = Math.Abs(GetWheelsSpeed());

                float speedCoef = 0;
                if (speedRangeAbs.Size() != 0)
                    speedCoef = (speedAbs - speedRangeAbs.Minimum) / speedRangeAbs.Size();
                MathFunctions.Clamp(ref speedCoef, 0, 1);

                float carpitch;
                //update channel

                /*bool onGround = LFWheel.onGround || RFWheel.onGround;
                if (!onGround)
                {
                    if (Intellect.IsControlKeyPressed(GameControlKeys.ArowUp))
                    {
                        carpitch = pitchRange.Minimum + pitchRange.Size();
                    }
                    else
                    {
                        carpitch = pitchRange.Minimum;
                    }
                }
                else*/
                {
                    carpitch = pitchRange.Minimum + speedCoef * pitchRange.Size();
                }
                motorSoundChannel.Pitch = carpitch;
                motorSoundChannel.Position = Position;
            }
        }
    }
}