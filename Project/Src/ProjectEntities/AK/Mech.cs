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
    //This is the version 0.0.1 of the MechType and Mech-class
    //Wellu

    //Mech is now depending on AKunit class, Feb 2th 2009
    //msg-gol
    public class MechType : AKunitType
    {
        [FieldSerialize]
        private bool allowLateralTorsoTilt = true;

        public bool AllowLateralTorsoTilt
        {
            get { return allowLateralTorsoTilt; }
            set { allowLateralTorsoTilt = value; }
        }

        [FieldSerialize]
        private float bobHeight = 0.5f;

        public float BobHeight
        {
            get { return bobHeight; }
            set { bobHeight = value; }
        }

        [FieldSerialize]
        private float bobSpeed = 0.1f;

        public float BobSpeed
        {
            get { return bobSpeed; }
            set { bobSpeed = value; }
        }

        [FieldSerialize]
        private float groundStepHeight = 10;

        public float GroundStepHeight
        {
            get { return groundStepHeight; }
            set { groundStepHeight = value; }
        }

        [FieldSerialize]
        private float tiltCorrectionValue = 1;

        public float TiltCorrectionValue
        {
            get { return tiltCorrectionValue; }
            set { tiltCorrectionValue = value; }
        }

        [FieldSerialize]
        private float walkingGroundTimer = 3.0f;

        [DefaultValue(3.0f)]
        public float WalkingGroundTimer
        {
            get { return walkingGroundTimer; }
            set { walkingGroundTimer = value; }
        }

        //iNCIN -- Add New code for walking up
        private const float walkUpHeightDefault = .5f;

        [FieldSerialize]
        private float walkUpHeight = walkUpHeightDefault;

        [DefaultValue(walkUpHeightDefault)]
        public float WalkUpHeight
        {
            get { return walkUpHeight; }
            set { walkUpHeight = value; }
        }

        [FieldSerialize]
        private string soundFoot;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot
        {
            get { return soundFoot; }
            set { soundFoot = value; }
        }

        [FieldSerialize]
        private int jumpJetFratio = 1;

        [DefaultValue(1)]
        public int JumpJetFratio
        {
            get { return jumpJetFratio; }
            set { jumpJetFratio = value; }
        }

        [FieldSerialize]
        private int jetFuelMax = 1000;

        [DefaultValue(1000)]
        public int JetFuelMax
        {
            get { return jetFuelMax; }
            set { jetFuelMax = value; }
        }

        [FieldSerialize]
        private float jetMaxAlt = 200.0f;

        [DefaultValue(200.0f)]
        public float JetMaxAlt
        {
            get { return jetMaxAlt; }
            set { jetMaxAlt = value; }
        }

        [FieldSerialize]
        private bool allowJet;

        [DefaultValue(false)]
        public bool AllowJet
        {
            get { return allowJet; }
            set { allowJet = value; }
        }

        [FieldSerialize]
        private string soundJet;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundJet
        {
            get { return soundJet; }
            set { soundJet = value; }
        }

        [FieldSerialize]
        private float maxForwardSpeed = 20;

        //[FieldSerialize]
        //string walkAnimatioName = "walk";

        //[FieldSerialize]
        //string idleAnimatioName = "idle";

        [FieldSerialize]
        private float maxBackwardSpeed = 10;

        [FieldSerialize]
        private float driveForwardForce = 300;

        [FieldSerialize]
        private float driveBackwardForce = 200;

        [FieldSerialize]
        private float brakeForce = 400;

        [FieldSerialize]
        private Range gunRotationAngleRange = new Range(-8, 40);

        [FieldSerialize]
        private Range optimalAttackDistanceRange;

        [FieldSerialize]
        private List<Gear> gears = new List<Gear>();

        [FieldSerialize]
        private Degree towerTurnSpeed = 60;

        [FieldSerialize]
        private string soundGearUp;

        [FieldSerialize]
        private string soundGearDown;

        [FieldSerialize]
        [DefaultValue(1.0f)]
        private float walkAnimationMultiplier = 1.0f;

        [FieldSerialize]
        private float mainGunRecoilForce;

        ///////////////////////////////////////////////////////

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

        [DefaultValue(typeof(Range), "0 0")]
        public Range OptimalAttackDistanceRange
        {
            get { return optimalAttackDistanceRange; }
            set { optimalAttackDistanceRange = value; }
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

        [DefaultValue(1.0f)]
        public float WalkAnimationMultiplier
        {
            get { return walkAnimationMultiplier; }
            set { walkAnimationMultiplier = value; }
        }

        [DefaultValue(0.0f)]
        public float MainGunRecoilForce
        {
            get { return mainGunRecoilForce; }
            set { mainGunRecoilForce = value; }
        }

        [DefaultValue(30.0f)]
        [Category("Jump Jets")]
        private float verticalJumpJetForce = 30;

        public float VerticalJumpJetForce
        {
            get { return verticalJumpJetForce; }
            set { verticalJumpJetForce = value; }
        }

        [DefaultValue(30.0f)]
        [Category("Jump Jets")]
        [FieldSerialize]
        private float forwardJumpJetForce = 30;

        public float ForwardJumpJetForce
        {
            get { return forwardJumpJetForce; }
            set { forwardJumpJetForce = value; }
        }

        [DefaultValue(30.0f)]
        [Category("Jump Jets")]
        [FieldSerialize]
        private float sidewaysJumpJetForce = 30;

        public float SidewaysJumpJetForce
        {
            get { return sidewaysJumpJetForce; }
            set { sidewaysJumpJetForce = value; }
        }

        [DefaultValue(false)]
        [FieldSerialize]
        private bool enableMasc = false;

        public bool EnableMasc
        {
            get { return enableMasc; }
            set { enableMasc = value; }
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////
    public class Mech : AKunit
    {
        private enum NetworkMessages
        {
            UpdateMech,
            UpdateMechBoster,
            UpdateMechThroth,
            UpdateMasc
        }

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float onGroundTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float notOnGroundTime;

        //moveVector from character.cs
        private Vec3 turnToPosition;

        private Radian horizontalDirectionForUpdateRotation;

        private int forceMoveVectorTimer;//if == 0 to disabled
        private Vec2 forceMoveVector;

        public int JetFuel;

        private Foot leftFoot = new Foot();
        private Foot rightFoot = new Foot();

        private class Foot
        {
            public bool onGround = true;
        }

        //from character.cs
        public void SetForceMoveVector(Vec2 vec)
        {
            forceMoveVectorTimer = 2;
            forceMoveVector = vec;
        }

        public void UpdateRotation(bool allowUpdateOldRotation)
        {
            float halfAngle = horizontalDirectionForUpdateRotation * .5f;
            Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)),
                MathFunctions.Cos(halfAngle));

            const float epsilon = .0001f;

            //update Rotation
            if (!Rotation.Equals(rot, epsilon))
            {
                //bool keepDisableControlPhysicsModelPushedToWorldFlag = DisableControlPhysicsModelPushedToWorldFlag;
                //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                //   DisableControlPhysicsModelPushedToWorldFlag = true;
                Rotation = rot;
                //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                //   DisableControlPhysicsModelPushedToWorldFlag = false;
            }

            //update OldRotation
            if (allowUpdateOldRotation)
            {
                //disable updating OldRotation property for TPSArcade demo and for PlatformerDemo
                bool updateOldRotation = true;
                if (Intellect != null && PlayerIntellect.Instance == Intellect)
                {
                    if (GameMap.Instance != null && (
                        GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
                        GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo))
                    {
                        updateOldRotation = false;
                    }
                }
                if (updateOldRotation)
                    OldRotation = rot;
            }
        }

        //from character.cs
        public void SetTurnToPosition(Vec3 pos)
        {
            turnToPosition = pos;

            Vec3 diff = turnToPosition - Position;
            horizontalDirectionForUpdateRotation = MathFunctions.ATan(diff.Y, diff.X);

            UpdateRotation(true);
        }

        public float GetElapsedTimeSinceLastGroundContact()
        {
            return notOnGroundTime;
        }

        //MapObjectAttachedMesh mainMeshObject;

        public Body chassisBody;
        private float chassisSleepTimer;

        //currently gears used only for sounds
        private MechType.Gear currentGear;

        private bool motorOn;
        private string currentMotorSoundName;
        private VirtualChannel motorSoundChannel;
        private VirtualChannel JetSoundChannel;
        private bool boosted;

        private bool firstTick = true;

        //Minefield specific
        private float minefieldUpdateTimer;

        private MechType _type = null; public new MechType Type { get { return _type; } }

        public Mech()
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

            //mainMeshObject = GetFirstAttachedObjectByAlias("animations") as MapObjectAttachedMesh;

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                MapObjectAttachedMapObject attachedMapObject = attachedObject as MapObjectAttachedMapObject;
                if (attachedMapObject == null)
                    continue;
            }

            AddTimer();

            if (EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
            {
                if (PhysicsModel == null)
                {
                    Log.Error("Mech: Physics model not exists.");
                    return;
                }

                chassisBody = PhysicsModel.GetBody("mainBody");
                if (chassisBody == null)
                {
                    Log.Error("Mech: \"mainBody\" body does not exist.");
                    return;
                }
            }

            //initialize currentGear
            currentGear = Type.Gears.Find(delegate(MechType.Gear gear)
            {
                return gear.Number == 0;
            });

            //if (loaded && EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor)
            //{
            //if (chassisBody != null)
            //    chassisBody.Static = true;
            // }
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

        protected override void Client_OnTick()
        {
            base.Client_OnTick();

            TickOnGround();
            JumpJetseffect();
            TickCurrentGear();//added incin

            if (IsOnGround())
                onGroundTime += TickDelta;
            else
                onGroundTime = 0;
            if (!IsOnGround())
                notOnGroundTime += TickDelta;
            else
                notOnGroundTime = 0;

            if (Intellect != null)
            {
                //MASC();
                TickMotorSound();
            }
            firstTick = false;
        }

        private void JumpJetseffect()
        {
            if (!Type.AllowJet) return;
            MapObjectAttachedParticle Shift1 = GetFirstAttachedObjectByAlias("Shift1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Shift2 = GetFirstAttachedObjectByAlias("Shift2") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke1 = GetFirstAttachedObjectByAlias("Smoke1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke2 = GetFirstAttachedObjectByAlias("Smoke2") as MapObjectAttachedParticle;

            if (Shift1 != null && Shift2 != null && Smoke1 != null && Smoke2 != null)
            {
                Shift1.Visible = boosted;
                Shift2.Visible = boosted;
                Smoke1.Visible = boosted;
                Smoke2.Visible = boosted;
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();
            if (this != null)
            {
                if (firstTick)
                {
                    if (chassisBody != null)
                    {
                        chassisBody.Static = false;
                        chassisBody.Sleeping = true;
                    }
                }

                if (forceMoveVectorTimer != 0)
                    forceMoveVectorTimer--;

                if (IsOnGround())
                    onGroundTime += TickDelta;
                else
                    onGroundTime = 0;
                if (!IsOnGround())
                    notOnGroundTime += TickDelta;
                else
                    notOnGroundTime = 0;

                TickOnGround();
                TickChassis();
                //TickAnimations();
                TickCurrentGear();
                TickMotorSound();
                TickMinefields();

                if (Intellect != null)
                {
                    MASC();
                    TickIntellectNet();
                    if (Type.AllowJet)
                    {
                        ShiftBooster();
                    }
                }
                firstTick = false;
            }
        }

        public override void SetNeedTurnToPosition(Vec3 pos)
        {
            if (towerBody == null)
                return;
            lookto = pos;

            if (Type.TowerTurnSpeed != 0)
            {
                Vec3 direction = pos - towerBody.Position;
                needTowerLocalDirection = SphereDir.FromVector(Rotation.GetInverse() * direction);
            }
            else
                SetMomentaryTurnToPosition(pos);
        }

        //bool corrected;
        protected override void UpdateTowerTransform()
        {
            if (towerBody == null || mainGunAttachedObject == null)
                return;

            Radian horizontalAngle = towerLocalDirection.Horizontal;
            Radian verticalAngle = towerLocalDirection.Vertical;

            Range torsoRotationRange = Type.TorsoRotationAngleRange * MathFunctions.PI / 180.0f;
            if (horizontalAngle < torsoRotationRange.Minimum)
                horizontalAngle = torsoRotationRange.Minimum;
            if (horizontalAngle > torsoRotationRange.Maximum)
                horizontalAngle = torsoRotationRange.Maximum;

            Quat q = GetInterpolatedRotation();
            towerBody.Position = GetInterpolatedPosition() + q * (towerBodyLocalPosition + new Vec3(0, 0, currentBob));

            towerBody.Rotation = q;

            if (Type.AllowLateralTorsoTilt)
            {
                Angles a1 = towerBody.Rotation.ToAngles();
                a1.Roll = 0;
                towerBody.Rotation = a1.ToQuat();
            }
            towerBody.Rotation *= new Angles(0, 0, -horizontalAngle.InDegrees()).ToQuat();
            towerBody.Rotation *= new Angles(0, verticalAngle.InDegrees(), 0).ToQuat();

            towerBody.Sleeping = true;

            foreach (Body body in PhysicsModel.Bodies)
            {
                if (body.Name == "Gun1" || body.Name == "Gun2" || body.Name == "Gun3" || body.Name == "Gun4" || body.Name == "Gun5")
                {
                    body.Rotation = GetInterpolatedRotation() *
                new Angles(0, verticalAngle.InDegrees(), -horizontalAngle.InDegrees()).ToQuat();
                }
            }
        }

        private int playedright = 0;
        private int playedleft = 0;

        private void TickOnGround()
        {
            if (chassisBody == null)
                return;

            if (chassisBody.Sleeping)
                return;

            //float rayLength = 30f;

            float rayLength = Type.GroundStepHeight;

            leftFoot.onGround = false;
            rightFoot.onGround = false;

            MapObjectAttachedHelper Leftfoot = GetFirstAttachedObjectByAlias("leftfoot") as MapObjectAttachedHelper;
            MapObjectAttachedHelper Rightfoot = GetFirstAttachedObjectByAlias("rightfoot") as MapObjectAttachedHelper;

            if (Leftfoot == null || Rightfoot == null) return;

            Vec3 pos;
            Quat rot;
            Vec3 scl;

            Vec3 direction = chassisBody.Rotation * -Vec3.ZAxis;
            Vec3 z2 = -Vec3.ZAxis * rayLength;

            float rightFootZ;
            float leftFootZ;

            //leftfoot
            {
                Leftfoot.GetGlobalTransform(out pos, out rot, out scl);
                leftFootZ = pos.Z;
                Vec3 start = Position + Leftfoot.PositionOffset;
                Ray ray = new Ray(start, z2);
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
                    leftFoot.onGround = true;
                }
            }
            //right foot
            {
                Rightfoot.GetGlobalTransform(out pos, out rot, out scl);
                rightFootZ = pos.Z;
                Vec3 start = Position + Rightfoot.PositionOffset;
                Ray ray = new Ray(start, z2);
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
                    rightFoot.onGround = true;
                }
            }

            if (!boosted && IsOnGround())
            {
                TickBob();

                if (rightFootZ > leftFootZ)
                {
                    playedright = 0;
                }
                else
                {
                    playedleft = 0;
                }

                if (rightFootZ < leftFootZ && playedright == 0)
                {
                    SoundPlay3D(Type.SoundFoot, .7f, true);
                    playedright = 1;
                    shouldBob = true;
                }

                if (leftFootZ < rightFootZ && playedleft == 0)
                {
                    SoundPlay3D(Type.SoundFoot, .7f, true);
                    playedleft = 1;
                    shouldBob = true;
                }
            }

            if (!IsOnGround())
            {
                playedright = 0;
                playedleft = 0;
                shouldBob = false;
                Angles temp = chassisBody.Rotation.ToAngles();
                float x = Type.TiltCorrectionValue;
                if (temp.Roll < 0)
                {
                    temp.Roll += x;
                    if (temp.Roll > 0)
                        temp.Roll = 0;
                }
                else if (temp.Roll > 0)
                {
                    temp.Roll -= x;
                    if (temp.Roll < 0)
                        temp.Roll = 0;
                }

                if (temp.Pitch < 0)
                {
                    temp.Pitch += x;
                    if (temp.Pitch > 0)
                        temp.Pitch = 0;
                }
                else if (temp.Pitch > 0)
                {
                    temp.Pitch -= x;
                    if (temp.Pitch < 0)
                        temp.Pitch = 0;
                }

                if (temp.Yaw != preJumpJetRotAng.Yaw)
                {
                    temp.Yaw = preJumpJetRotAng.Yaw;
                }

                chassisBody.Rotation = temp.ToQuat();
            }
            else
                preJumpJetRotAng = chassisBody.Rotation.ToAngles();
        }

        public float currentBob = 0;

        //float startZ;
        private bool down = true;

        private bool up = false;
        private bool shouldBob = false;

        private void TickBob()
        {
            if (!shouldBob)
            {
                currentBob = 0;
                //startZ = towerBody.Position.Z;
                //wheres the dance moves

                return;
            }

            float speed = Math.Abs((ThrottleF / 100));
            MathFunctions.Clamp(ref speed, -Type.BobSpeed, Type.BobSpeed);

            if (down)
            {
                currentBob -= speed;
                if (currentBob < -Type.BobHeight)
                {
                    currentBob = -Type.BobHeight;
                    down = false;
                    up = true;
                }
            }

            if (up)
            {
                currentBob += speed;
                if (currentBob > 0)
                {
                    currentBob = 0;
                    down = true;
                    up = false;
                    shouldBob = false;
                }
            }
        }

        private bool played;

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
                    MapObjectAttachedMesh CT = GetFirstAttachedObjectByAlias("CT") as MapObjectAttachedMesh;
                    if (CT != null) CT.Visible = true;
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

                float speedAbs = Math.Abs(GetFootsSpeed());

                float speedCoef = 0;
                if (speedRangeAbs.Size() != 0)
                    speedCoef = (speedAbs - speedRangeAbs.Minimum) / speedRangeAbs.Size();
                MathFunctions.Clamp(ref speedCoef, 0, 1);

                //update channel
                motorSoundChannel.Pitch = pitchRange.Minimum + speedCoef * pitchRange.Size();
                motorSoundChannel.Position = Position;
            }

            //jumpjet sound

            if (boosted && played == false)
            {
                if (!string.IsNullOrEmpty(Type.SoundJet))
                {
                    Sound sound = SoundWorld.Instance.SoundCreate(Type.SoundJet,
                        SoundMode.Mode3D | SoundMode.Loop);

                    if (sound != null)
                    {
                        JetSoundChannel = SoundWorld.Instance.SoundPlay(
                            sound, EngineApp.Instance.DefaultSoundChannelGroup, 0.7f, true);
                        JetSoundChannel.Position = Position;
                        JetSoundChannel.Pause = false;
                        played = true;
                    }
                }
            }
            else if (!boosted && JetSoundChannel != null)
            {
                JetSoundChannel.Stop();
                played = false;
            }
        }

        public void HidingThingy(bool hide)
        {
            MapObjectAttachedMesh CT = GetFirstAttachedObjectByAlias("CT") as MapObjectAttachedMesh;
            MapObjectAttachedMesh Cpit = GetFirstAttachedObjectByAlias("Cpit") as MapObjectAttachedMesh;
            if (hide)
            {
                if (Cpit != null) Cpit.Visible = true;
                if (CT != null) CT.Visible = false;
            }
            if (!hide)
            {
                if (Cpit != null) Cpit.Visible = false;
                if (CT != null) CT.Visible = true;
            }
        }

        private bool fuleupdated;
        private Angles preJumpJetRotAng;

        private void ShiftBooster()
        {
            if (!Type.AllowJet) return;

            MapObjectAttachedParticle Shift1 = GetFirstAttachedObjectByAlias("Shift1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Shift2 = GetFirstAttachedObjectByAlias("Shift2") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke1 = GetFirstAttachedObjectByAlias("Smoke1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke2 = GetFirstAttachedObjectByAlias("Smoke2") as MapObjectAttachedParticle;

            if (!fuleupdated)
            {
                JetFuel = Type.JetFuelMax;
                fuleupdated = true;
            }

            if (Intellect.IsControlKeyPressed(GameControlKeys.JumpForward) || Intellect.IsControlKeyPressed(GameControlKeys.JumpBackward) ||
                Intellect.IsControlKeyPressed(GameControlKeys.JumpLeft) || Intellect.IsControlKeyPressed(GameControlKeys.JumpRight) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Jump))
            {
                if (JetFuel > 5) //todo: should this stay 5
                {
                    //calculate the max altitude with consideration to the height of the terrain
                    float maxAltitude = Type.JetMaxAlt;
                    if (HeightmapTerrain.Instances.Count > 0)
                    {
                        foreach (HeightmapTerrain inst in HeightmapTerrain.Instances)
                        {
                            float h = inst.GetHeight(Position.ToVec2(), false);
                            if (h != float.MinValue)
                            {
                                maxAltitude += h;
                                break;
                            }
                        }
                    }

                    if (Position.Z < maxAltitude)
                    {
                        float forwardForce = 0;
                        float sidewaysForce = 0;

                        if (Intellect.IsControlKeyPressed(GameControlKeys.JumpForward))
                            forwardForce += Type.ForwardJumpJetForce;
                        if (Intellect.IsControlKeyPressed(GameControlKeys.JumpBackward))
                            forwardForce -= Type.ForwardJumpJetForce;

                        if (Intellect.IsControlKeyPressed(GameControlKeys.JumpLeft))
                            sidewaysForce += Type.SidewaysJumpJetForce;
                        if (Intellect.IsControlKeyPressed(GameControlKeys.JumpRight))
                            sidewaysForce -= Type.SidewaysJumpJetForce;

                        /* if (forwardForce > 0)
                         {
                             float rf = rightFootForce > 0 ? rightFootForce : -rightFootForce;
                             float lf = leftFootForce > 0 ? leftFootForce : -leftFootForce;

                             chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                             new Vec3(rf, 0, 0), new Vec3(0, -0.5f, 0));

                             chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                             new Vec3(lf, 0, 0), new Vec3(0, 0.5f, 0));
                         }
                         else if (forwardForce < 0)
                         {
                             float rf = rightFootForce < 0 ? rightFootForce : -rightFootForce;
                             float lf = leftFootForce < 0 ? leftFootForce : -leftFootForce;

                             chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                             new Vec3(rf, 0, 0), new Vec3(0, -0.5f, 0));

                             chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                             new Vec3(lf, 0, 0), new Vec3(0, 0.5f, 0));
                         }*/

                        chassisBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                           chassisBody.Rotation * new Vec3(forwardForce, sidewaysForce, Type.VerticalJumpJetForce) * chassisBody.Mass, Vec3.Zero);
                        JetFuel -= 3 * Type.JumpJetFratio;
                        boosted = true;
                    }
                }
            }
            else
            {
                boosted = false;
                if (JetFuel < 1000)
                {
                    JetFuel += 1;
                }
            }
            if (Shift1 != null && Shift2 != null && Smoke1 != null && Smoke2 != null)
            {
                Shift1.Visible = boosted;
                Shift2.Visible = boosted;
                Smoke1.Visible = boosted;
                Smoke2.Visible = boosted;
            }

            if (!IsOnGround())
            {
                Angles RotationNormAG = (chassisBody.Rotation.GetNormalize() *
                    chassisBody.Rotation).ToAngles();

                chassisBody.AngularVelocity = new Vec3(RotationNormAG.Roll / 180,
                    RotationNormAG.Pitch / 180, chassisBody.AngularVelocity.Z);
            }
        }

        /*private float GetRealAlt()
        {
            Vec3 downDirection = chassisBody.Rotation * new Vec3(0, 0, -100 + -Type.JetMaxAlt);

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
            EngineApp.Instance.ScreenGuiRenderer.AddText("realAlt: " + (Position.Z - collisionPos.Z).ToString(), new Vec2(.6f, .2f));
            if (collision)
                return Position.Z - collisionPos.Z;
            else
                return Position.Z;
        }*/

        //float MechBoost = 0;
        private float mascSpeed = 0;

        private float mascForce = 0;
        private bool mascOn = false;
        private float leftFootForce = 0;
        private float rightFootForce = 0;

        private void MASC()
        {
            //AKunit u = GetPlayerUnit() as AKunit;
            //if (u == null) return;

            if (Type.EnableMasc)
            {
                if (Intellect.IsControlKeyPressed(GameControlKeys.Shift))
                {
                    if (AKunitHeat < (Type.AKunitHeatMax - 200))
                    {
                        mascSpeed = Type.MaxForwardSpeed * 2;
                        mascForce = Type.DriveForwardForce * 2;
                        mascOn = true;
                        heattoadd += 10;
                    }
                    else
                    {
                        mascSpeed = 0;
                        mascForce = 0;
                        mascOn = false;
                    }
                }
                else
                {
                    mascSpeed = 0;
                    mascForce = 0;
                    mascOn = false;
                }
            }
        }

        private void TickChassis()
        {
            bool onGround = leftFoot.onGround || rightFoot.onGround;

            float leftFootThrottle = 0;
            float rightFootThrottle = 0;
            if (Intellect != null)
            {
                if (!MechShutDown)
                {
                    ShiftBooster();

                    leftFootThrottle = Throttle();
                    rightFootThrottle = Throttle();

                    if (!boosted)
                    {
                        if (Intellect.IsControlKeyPressed(GameControlKeys.Left))
                        {
                            leftFootThrottle -= 2;
                            rightFootThrottle += 2;
                        }
                        if (Intellect.IsControlKeyPressed(GameControlKeys.Right))
                        {
                            leftFootThrottle += 2;
                            rightFootThrottle -= 2;
                        }

                        MathFunctions.Clamp(ref leftFootThrottle, -1, 1);
                        MathFunctions.Clamp(ref rightFootThrottle, -1, 1);
                    }
                }
            }

            if (rightFootThrottle < 0.05f && rightFootThrottle > -0.05f)
                rightFootThrottle = 0;

            if (leftFootThrottle < 0.05f && leftFootThrottle > -0.05f)
                leftFootThrottle = 0;

            //return if no throttle and sleeping
            if (chassisBody.Sleeping && rightFootThrottle == 0 && leftFootThrottle == 0)
                return;

            Vec3 localLinearVelocity = chassisBody.LinearVelocity * chassisBody.Rotation.GetInverse();

            //add drive force

            if (onGround)
            {
                if (leftFootThrottle > 0 && localLinearVelocity.X < Type.MaxForwardSpeed + mascSpeed)
                {
                    leftFootForce = localLinearVelocity.X > 0 ? Type.DriveForwardForce + mascForce : Type.BrakeForce;
                    leftFootForce *= leftFootThrottle;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(leftFootForce, 0, 0), new Vec3(0, 0.5f, 0));
                }

                if (leftFootThrottle < 0 && -localLinearVelocity.X < Type.MaxBackwardSpeed)
                {
                    leftFootForce = localLinearVelocity.X > 0 ? Type.BrakeForce : Type.DriveBackwardForce;
                    leftFootForce *= leftFootThrottle;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(leftFootForce, 0, 0), new Vec3(0, 0.5f, 0));
                }
            }

            if (onGround)
            {
                if (rightFootThrottle > 0 && localLinearVelocity.X < Type.MaxForwardSpeed + mascSpeed)
                {
                    rightFootForce = localLinearVelocity.X > 0 ? Type.DriveForwardForce + mascForce : Type.BrakeForce;
                    rightFootForce *= rightFootThrottle;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(rightFootForce, 0, 0), new Vec3(0, -0.5f, 0));
                }

                if (rightFootThrottle < 0 && -localLinearVelocity.X < Type.MaxBackwardSpeed)
                {
                    rightFootForce = localLinearVelocity.X > 0 ? Type.BrakeForce : Type.DriveBackwardForce;
                    rightFootForce *= rightFootThrottle;
                    chassisBody.AddForce(ForceType.LocalAtLocalPos, TickDelta,
                        new Vec3(rightFootForce, 0, 0), new Vec3(0, -0.5f, 0));
                }
            }

            //LinearVelocity
            //if (onGround)// && localLinearVelocity != Vec3.Zero)
            {
                Vec3 velocity = localLinearVelocity;
                if (onGround && !boosted)
                    velocity.Y = 0;

                chassisBody.LinearVelocity = chassisBody.Rotation * velocity;
            }

            bool stop = onGround && leftFootThrottle == 0 && rightFootThrottle == 0;

            float v = boosted ? 0.5f : 5f;
            bool noLinearVelocity = chassisBody.LinearVelocity.Equals(Vec3.Zero, v);
            bool noAngularVelocity = chassisBody.AngularVelocity.Equals(Vec3.Zero, v);

            //LinearDamping
            float linearDamping;
            if (stop)
                linearDamping = noLinearVelocity ? 5 : 1;
            else
                linearDamping = .15f;
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

        public float ThrottleF;

        private float Throttle()
        {
            if (Intellect.IsControlKeyPressed(GameControlKeys.Forward) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Backward))
            {
                ThrottleF += 2f * (Intellect.GetControlKeyStrength(GameControlKeys.Forward) - Intellect.GetControlKeyStrength(GameControlKeys.Backward));
            }

            if (Intellect.IsControlKeyPressed(GameControlKeys.Mechstop))
                ThrottleF = 0f;

            MathFunctions.Clamp(ref ThrottleF, -100, 100);

            return ThrottleF / 100;
        }

        private float GetFootsSpeed()
        {
            if (chassisBody == null)
                return 0;

            Vec3 linearVelocity = chassisBody.LinearVelocity;
            Vec3 angularVelocity = chassisBody.AngularVelocity;

            //optimization
            if (linearVelocity.Equals(Vec3.Zero, .1f) && angularVelocity.Equals(Vec3.Zero, .1f))
                return 0;

            Vec3 localLinearVelocity = linearVelocity * chassisBody.Rotation.GetInverse();

            //not ideal true
            return localLinearVelocity.X + Math.Abs(angularVelocity.Z) * 2;
        }

        private void TickCurrentGear()
        {
            //currently gears used only for sounds

            if (currentGear == null)
                return;

            if (motorOn)
            {
                float speed = GetFootsSpeed();

                MechType.Gear newGear = null;

                if (speed < currentGear.SpeedRange.Minimum || speed > currentGear.SpeedRange.Maximum)
                {
                    //find new gear
                    newGear = Type.Gears.Find(delegate(MechType.Gear gear)
                    {
                        return speed >= gear.SpeedRange.Minimum && speed <= gear.SpeedRange.Maximum;
                    });
                }

                if (newGear != null && currentGear != newGear)
                {
                    //change gear
                    MechType.Gear oldGear = currentGear;
                    OnGearChange(oldGear, newGear);
                    currentGear = newGear;
                }
            }
            else
            {
                if (currentGear.Number != 0)
                {
                    currentGear = Type.Gears.Find(delegate(MechType.Gear gear)
                    {
                        return gear.Number == 0;
                    });
                }
            }
        }

        private void OnGearChange(MechType.Gear oldGear, MechType.Gear newGear)
        {
            if (!firstTick && Health != 0)
            {
                bool up = Math.Abs(newGear.Number) > Math.Abs(oldGear.Number);
                string soundName = up ? Type.SoundGearUp : Type.SoundGearDown;
                SoundPlay3D(soundName, .7f, true);
            }
        }

        protected override void OnRenderFrame()
        {
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                //AnimationTree tree = GetFirstAnimationTree();
                //if (tree != null)
                UpdateAnimationTree();
            }
            base.OnRenderFrame();
        }

        /*
        protected override void OnUpdateBaseAnimation()
        {
            base.OnUpdateBaseAnimation();

            //walk animation
            if (IsOnGround() && (tracksSpeed > 0.3f || tracksSpeed < -0.3f))
            {
                float velocity = (tracksSpeed / Type.MaxForwardSpeed) * Type.WalkAnimationMultiplier * 10;

                UpdateBaseAnimation(Type.WalkAnimationName, true, true, velocity);
                return;
            }

            if (!IsOnGround())
            {
                UpdateBaseAnimation(Type.WalkAnimationName, true, true, 0);
                return;
            }

            //idle animation
            {
                UpdateBaseAnimation(Type.IdleAnimationName, true, true, 1);
                return;
            }
            ///////////////////////////////////////////////////////////////////////////////
        }
        */

        /*

        void UpdateAnimationTree()
        {
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                AnimationTree tree = GetFirstAnimationTree();
                if (tree != null)
                {
                    bool move = false;
                    Degree moveAngle = 0;
                    float moveSpeed = 0;

                    Vec2 localVec = (Rotation.GetInverse() * chassisBody.LinearVelocity).ToVec2();

                    if (IsOnGround() && (tracksSpeed > 0.3f || tracksSpeed < -0.3f))
                    {
                        move = true;
                        float velocity = (tracksSpeed / Type.MaxForwardSpeed) * Type.WalkAnimationMultiplier;
                        Radian angle = MathFunctions.ATan(localVec.Y, localVec.X);
                        moveAngle = angle.InDegrees();
                        moveSpeed = chassisBody.LinearVelocity.ToVec2().Length(); //GroundRelativeVelocity.ToVec2().Length();
                    }

                    tree.SetParameterValue("move", move ? 1 : 0);
                    tree.SetParameterValue("moveAngle", moveAngle);
                    tree.SetParameterValue("moveSpeed", moveSpeed);
                    tree.SetParameterValue("fly", !IsOnGround() ? 1 : 0);
                }
            }
        }
         */

        private void UpdateAnimationTree()
        {
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                int count = 0;

                {
                    foreach (AnimationTree tree in GetAllAnimationTrees())
                    {
                        bool move = false;
                        Degree moveAngle = 0;
                        float moveSpeed = 0;
                        count++;
                        Vec2 localVec = (Rotation.GetInverse() * chassisBody.LinearVelocity).ToVec2();

                        if (IsOnGround() && (tracksSpeed > 0.3f || tracksSpeed < -0.3f))
                        {
                            move = true;
                            float velocity = (tracksSpeed / Type.MaxForwardSpeed) * Type.WalkAnimationMultiplier;
                            Radian angle = MathFunctions.ATan(localVec.Y, localVec.X);
                            moveAngle = angle.InDegrees();
                            moveSpeed = chassisBody.LinearVelocity.ToVec2().Length(); //GroundRelativeVelocity.ToVec2().Length();
                        }

                        tree.SetParameterValue("move", move ? 1 : 0);
                        tree.SetParameterValue("moveAngle", moveAngle);
                        tree.SetParameterValue("moveSpeed", moveSpeed);
                        tree.SetParameterValue("fly", !IsOnGround() ? 1 : 0);
                    }
                }
                //if(EngineDebugSettings.DrawGameSpecificDebugGeometry)
                //    EngineConsole.Instance.Print("Animations run per loop: " + count.ToString());
            }
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

        public Vec3 GetGroundRelativeVelocity()
        {
            return chassisBody.LinearVelocity;
        }

        //float Groundtimer;
        /*public bool IsOnGround()
        {
            if (!rightFoot.onGround && !leftFoot.onGround)
            {
                Groundtimer += TickDelta;
            }
            else
            {
                Groundtimer = 0;
            }

            if (Groundtimer > Type.WalkingGroundTimer)
                return false;
            else
                return true;
        }*/

        public bool IsOnGround()
        {
            if (!rightFoot.onGround && !leftFoot.onGround)
            {
                return false;
            }

            return true;
        }

        private AKunit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject as AKunit;
        }

        /////////////////////////////Network Mech//////////////////////////////////////////////
        private void TickIntellectNet()
        {
            //////////////Networking////////////////
            if (EntitySystemWorld.Instance.IsServer() &&
             Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendUpdateMech(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateMechBoster(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateMechThroth(EntitySystemWorld.Instance.RemoteEntityWorlds);
                Server_SendUpdateMasc(EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
        }

        private float Server_Throth;

        private void Server_SendUpdateMechThroth(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            const int epsilon = 1;
            if (Math.Abs(ThrottleF - Server_Throth) > epsilon)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Mech),
                    (ushort)NetworkMessages.UpdateMechThroth);

                writer.Write(ThrottleF);
                EndNetworkMessage();

                Server_Throth = ThrottleF;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateMechThroth)]
        private void Client_ReceiveUpdateMechThroth(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            float value = reader.ReadSingle();
            if (!reader.Complete())
                return;
            ThrottleF = value;
        }

        private float Server_jetful;

        private void Server_SendUpdateMech(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            const int epsilon = 3;
            if (Math.Abs(JetFuel - Server_jetful) > epsilon)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Mech),
                    (ushort)NetworkMessages.UpdateMech);

                writer.WriteVariableInt32(JetFuel);
                EndNetworkMessage();

                Server_jetful = JetFuel;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateMech)]
        private void Client_ReceiveUpdateMech(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;
            JetFuel = value;
        }

        private bool Server_boosted;

        private void Server_SendUpdateMechBoster(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (boosted != Server_boosted)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Mech),
                    (ushort)NetworkMessages.UpdateMechBoster);

                writer.Write(boosted);
                EndNetworkMessage();

                Server_boosted = boosted;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateMechBoster)]
        private void Client_ReceiveUpdateMechBoster(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool value = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            boosted = value;
        }

        private bool Server_masc;

        private void Server_SendUpdateMasc(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (mascOn != Server_masc)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Mech),
                    (ushort)NetworkMessages.UpdateMasc);

                writer.Write(mascOn);
                EndNetworkMessage();

                Server_masc = mascOn;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateMasc)]
        private void Client_ReceiveUpdateMasc(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool value = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            mascOn = value;

            if (mascOn)
            {
                mascSpeed = Type.MaxForwardSpeed * 2;
                mascForce = Type.DriveForwardForce * 2;
            }
            else
            {
                mascSpeed = 0;
                mascForce = 0;
            }
        }
    }
}