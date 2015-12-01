// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
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
    /// Defines the <see cref="Character"/> entity type.
    /// </summary>
    public class CharacterType : UnitType
    {
        //physics

        private const float heightDefault = 1.8f;

        [FieldSerialize]
        private float height = heightDefault;

        private const float radiusDefault = .4f;

        [FieldSerialize]
        private float radius = radiusDefault;

        private const float bottomRadiusDefault = .15f;

        [FieldSerialize]
        private float bottomRadius = bottomRadiusDefault;

        private const float walkUpHeightDefault = .5f;

        [FieldSerialize]
        private float walkUpHeight = walkUpHeightDefault;

        private const float massDefault = 70;

        [FieldSerialize]
        private float mass = massDefault;

        private const float minSpeedToSleepBodyDefault = .6f;

        [FieldSerialize]
        private float minSpeedToSleepBody = minSpeedToSleepBodyDefault;

        private const float heightFromPositionToGroundDefault = 1.15f;

        [FieldSerialize]
        private float heightFromPositionToGround = heightFromPositionToGroundDefault;

        //const Degree maxSlopeAngleDefault = new Degree( 60 );
        [FieldSerialize]
        private Degree maxSlopeAngle = new Degree(45);// maxSlopeAngleDefault;

        //walk

        private const float walkForwardMaxSpeedDefault = 1.0f;

        [FieldSerialize]
        private float walkForwardMaxSpeed = walkForwardMaxSpeedDefault;

        private const float walkBackwardMaxSpeedDefault = 1.0f;

        [FieldSerialize]
        private float walkBackwardMaxSpeed = walkBackwardMaxSpeedDefault;

        private const float walkSideMaxSpeedDefault = .8f;

        [FieldSerialize]
        private float walkSideMaxSpeed = walkSideMaxSpeedDefault;

        private const float walkForceDefault = 280000;

        [FieldSerialize]
        private float walkForce = walkForceDefault;

        //run

        private const float runForwardMaxSpeedDefault = 5;

        [FieldSerialize]
        private float runForwardMaxSpeed = runForwardMaxSpeedDefault;

        private const float runBackwardMaxSpeedDefault = 5;

        [FieldSerialize]
        private float runBackwardMaxSpeed = runBackwardMaxSpeedDefault;

        private const float runSideMaxSpeedDefault = 5;

        [FieldSerialize]
        private float runSideMaxSpeed = runSideMaxSpeedDefault;

        private const float runForceDefault = 420000;

        [FieldSerialize]
        private float runForce = runForceDefault;

        //fly

        private const float flyControlMaxSpeedDefault = 10;

        [FieldSerialize]
        private float flyControlMaxSpeed = flyControlMaxSpeedDefault;

        private const float flyControlForceDefault = 35000;

        [FieldSerialize]
        private float flyControlForce = flyControlForceDefault;

        //jump

        [FieldSerialize]
        private bool jumpSupport;

        private const float jumpSpeedDefault = 4;

        [FieldSerialize]
        private float jumpSpeed = jumpSpeedDefault;

        [FieldSerialize]
        private string soundJump;

        //crouching

        [FieldSerialize]
        private bool crouchingSupport;

        private const float crouchingWalkUpHeightDefault = .1f;

        [FieldSerialize]
        private float crouchingWalkUpHeight = crouchingWalkUpHeightDefault;

        private const float crouchingHeightDefault = 1.0f;

        [FieldSerialize]
        private float crouchingHeight = crouchingHeightDefault;

        private const float crouchingMaxSpeedDefault = 1;

        [FieldSerialize]
        private float crouchingMaxSpeed = crouchingMaxSpeedDefault;

        private const float crouchingForceDefault = 100000;

        [FieldSerialize]
        private float crouchingForce = crouchingForceDefault;

        private const float crouchingHeightFromPositionToGroundDefault = .55f;

        [FieldSerialize]
        private float crouchingHeightFromPositionToGround = crouchingHeightFromPositionToGroundDefault;

        //damageFastChangeSpeed

        private const float damageFastChangeSpeedMinimalSpeedDefault = 10;

        [FieldSerialize]
        private float damageFastChangeSpeedMinimalSpeed = damageFastChangeSpeedMinimalSpeedDefault;

        private const float damageFastChangeSpeedFactorDefault = 40;

        [FieldSerialize]
        private float damageFastChangeSpeedFactor = damageFastChangeSpeedFactorDefault;

        ///////////////////////////////////////////

        //physics

        [DefaultValue(heightDefault)]
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        [DefaultValue(radiusDefault)]
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [DefaultValue(bottomRadiusDefault)]
        public float BottomRadius
        {
            get { return bottomRadius; }
            set { bottomRadius = value; }
        }

        [DefaultValue(walkUpHeightDefault)]
        public float WalkUpHeight
        {
            get { return walkUpHeight; }
            set { walkUpHeight = value; }
        }

        [DefaultValue(massDefault)]
        public float Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        [DefaultValue(minSpeedToSleepBodyDefault)]
        public float MinSpeedToSleepBody
        {
            get { return minSpeedToSleepBody; }
            set { minSpeedToSleepBody = value; }
        }

        [DefaultValue(heightFromPositionToGroundDefault)]
        public float HeightFromPositionToGround
        {
            get { return heightFromPositionToGround; }
            set { heightFromPositionToGround = value; }
        }

        [DefaultValue(typeof(Degree), "45")]
        public Degree MaxSlopeAngle
        {
            get { return maxSlopeAngle; }
            set { maxSlopeAngle = value; }
        }

        //walk

        [DefaultValue(walkForwardMaxSpeedDefault)]
        public float WalkForwardMaxSpeed
        {
            get { return walkForwardMaxSpeed; }
            set { walkForwardMaxSpeed = value; }
        }

        [DefaultValue(walkBackwardMaxSpeedDefault)]
        public float WalkBackwardMaxSpeed
        {
            get { return walkBackwardMaxSpeed; }
            set { walkBackwardMaxSpeed = value; }
        }

        [DefaultValue(walkSideMaxSpeedDefault)]
        public float WalkSideMaxSpeed
        {
            get { return walkSideMaxSpeed; }
            set { walkSideMaxSpeed = value; }
        }

        [DefaultValue(walkForceDefault)]
        public float WalkForce
        {
            get { return walkForce; }
            set { walkForce = value; }
        }

        //run

        [DefaultValue(runForwardMaxSpeedDefault)]
        public float RunForwardMaxSpeed
        {
            get { return runForwardMaxSpeed; }
            set { runForwardMaxSpeed = value; }
        }

        [DefaultValue(runBackwardMaxSpeedDefault)]
        public float RunBackwardMaxSpeed
        {
            get { return runBackwardMaxSpeed; }
            set { runBackwardMaxSpeed = value; }
        }

        [DefaultValue(runSideMaxSpeedDefault)]
        public float RunSideMaxSpeed
        {
            get { return runSideMaxSpeed; }
            set { runSideMaxSpeed = value; }
        }

        [DefaultValue(runForceDefault)]
        public float RunForce
        {
            get { return runForce; }
            set { runForce = value; }
        }

        //fly

        [DefaultValue(flyControlMaxSpeedDefault)]
        public float FlyControlMaxSpeed
        {
            get { return flyControlMaxSpeed; }
            set { flyControlMaxSpeed = value; }
        }

        [DefaultValue(flyControlForceDefault)]
        public float FlyControlForce
        {
            get { return flyControlForce; }
            set { flyControlForce = value; }
        }

        //jump

        [DefaultValue(false)]
        public bool JumpSupport
        {
            get { return jumpSupport; }
            set { jumpSupport = value; }
        }

        [DefaultValue(jumpSpeedDefault)]
        public float JumpSpeed
        {
            get { return jumpSpeed; }
            set { jumpSpeed = value; }
        }

        [DefaultValue("")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundJump
        {
            get { return soundJump; }
            set { soundJump = value; }
        }

        //crouching

        [DefaultValue(false)]
        public bool CrouchingSupport
        {
            get { return crouchingSupport; }
            set { crouchingSupport = value; }
        }

        [DefaultValue(crouchingWalkUpHeightDefault)]
        public float CrouchingWalkUpHeight
        {
            get { return crouchingWalkUpHeight; }
            set { crouchingWalkUpHeight = value; }
        }

        [DefaultValue(crouchingHeightDefault)]
        public float CrouchingHeight
        {
            get { return crouchingHeight; }
            set { crouchingHeight = value; }
        }

        [DefaultValue(crouchingMaxSpeedDefault)]
        public float CrouchingMaxSpeed
        {
            get { return crouchingMaxSpeed; }
            set { crouchingMaxSpeed = value; }
        }

        [DefaultValue(crouchingForceDefault)]
        public float CrouchingForce
        {
            get { return crouchingForce; }
            set { crouchingForce = value; }
        }

        [DefaultValue(crouchingHeightFromPositionToGroundDefault)]
        public float CrouchingHeightFromPositionToGround
        {
            get { return crouchingHeightFromPositionToGround; }
            set { crouchingHeightFromPositionToGround = value; }
        }

        //damageFastChangeSpeed

        [DefaultValue(damageFastChangeSpeedMinimalSpeedDefault)]
        public float DamageFastChangeSpeedMinimalSpeed
        {
            get { return damageFastChangeSpeedMinimalSpeed; }
            set { damageFastChangeSpeedMinimalSpeed = value; }
        }

        [DefaultValue(damageFastChangeSpeedFactorDefault)]
        public float DamageFastChangeSpeedFactor
        {
            get { return damageFastChangeSpeedFactor; }
            set { damageFastChangeSpeedFactor = value; }
        }

        // jump jets

        [FieldSerialize]
        private int jumpJetFratio = 1;

        [Category("Jump Jets")]
        [DefaultValue(1)]
        public int JumpJetFratio
        {
            get { return jumpJetFratio; }
            set { jumpJetFratio = value; }
        }

        [FieldSerialize]
        private int jetFuelMax = 1000;

        [DefaultValue(1000)]
        [Category("Jump Jets")]
        public int JetFuelMax
        {
            get { return jetFuelMax; }
            set { jetFuelMax = value; }
        }

        [FieldSerialize]
        private float jetMaxAlt = 200.0f;

        [DefaultValue(200.0f)]
        [Category("Jump Jets")]
        public float JetMaxAlt
        {
            get { return jetMaxAlt; }
            set { jetMaxAlt = value; }
        }

        private static bool allowjet = true;

        [FieldSerialize]
        private bool allowJet = allowjet;

        [DefaultValue(true)]
        [Category("Jump Jets")]
        public bool AllowJet
        {
            get { return allowJet; }
            set { allowJet = value; }
        }

        [FieldSerialize]
        private string soundJet;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [Category("Jump Jets")]
        public string SoundJet
        {
            get { return soundJet; }
            set { soundJet = value; }
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

        ///////////////////////////////////////////

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            //it is not known how will be used this sound (2D or 3D?).
            //Sound will preloaded as 2D only here.
            PreloadSound(SoundJump, 0);

            if (SoundJet.ToString() != null)
                PreloadSound(SoundJet, 0);
        }

        public void GetBodyFormInfo(bool crouching, out float outHeight, out float outWalkUpHeight,
            out float outFromPositionToFloorDistance)
        {
            if (crouching)
            {
                outHeight = crouchingHeight;
                outWalkUpHeight = crouchingWalkUpHeight;
                outFromPositionToFloorDistance = crouchingHeightFromPositionToGround;
            }
            else
            {
                outHeight = height;
                outWalkUpHeight = walkUpHeight;
                outFromPositionToFloorDistance = heightFromPositionToGround;
            }
        }
    }

    /// <summary>
    /// Defines the physical characters.
    /// </summary>
    public partial class Character : Unit
    {
        //jumpjets
        private int jetFuel;

        private bool boosted;
        private VirtualChannel JetSoundChannel;
        private bool played;

        private Body mainBody;
        private Body fixRotationJointBody;
        private BallJoint fixRotationJoint;

        //on ground and flying states
        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float mainBodyGroundDistance = 1000;//from center of the body/object

        private Body groundBody;
        private float forceIsOnGroundRemainingTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float onGroundTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float notOnGroundTime;

        private Vec3 lastTickPosition;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float jumpInactiveTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float shouldJumpTime;

        private Vec3 turnToPosition;
        private Radian horizontalDirectionForUpdateRotation;

        //moveVector
        private int forceMoveVectorTimer;//if == 0 to disabled

        private Vec2 forceMoveVector;
        private Vec2 lastTickForceVector;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private Vec3 linearVelocityForSerialization;

        private Vec3 groundRelativeVelocity;
        private Vec3 server_sentGroundRelativeVelocity;
        private Vec3[] groundRelativeVelocitySmoothArray;
        private Vec3 groundRelativeVelocitySmooth;

        //damageFastChangeSpeed
        private Vec3 damageFastChangeSpeedLastVelocity = new Vec3(float.NaN, float.NaN, float.NaN);

        private float allowToSleepTime;

        //crouching
        private const float crouchingVisualSwitchTime = .3f;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private bool crouching;

        private float crouchingSwitchRemainingTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float crouchingVisualFactor;

        //wiggle camera when walking
        private float wiggleWhenWalkingSpeedFactor;

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            JumpEventToClient,
            GroundRelativeVelocityToClient,
        }

        ///////////////////////////////////////////

        private CharacterType _type = null; public new CharacterType Type { get { return _type; } }

        public void SetForceMoveVector(Vec2 vec)
        {
            forceMoveVectorTimer = 2;
            forceMoveVector = vec;
        }

        [Browsable(false)]
        public int JetFuel
        {
            get { return jetFuel; }
        }

        [Browsable(false)]
        public Body MainBody
        {
            get { return mainBody; }
        }

        [Browsable(false)]
        public Vec3 TurnToPosition
        {
            get { return turnToPosition; }
        }

        public void SetTurnToPosition(Vec3 pos)
        {
            turnToPosition = pos;

            Vec3 diff = turnToPosition - Position;
            horizontalDirectionForUpdateRotation = MathFunctions.ATan(diff.Y, diff.X);

            UpdateRotation(true);
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

        public bool IsOnGround()
        {
            if (jumpInactiveTime != 0)
                return false;
            if (forceIsOnGroundRemainingTime > 0)
                return true;

            float distanceFromPositionToFloor = crouching ?
                Type.CrouchingHeightFromPositionToGround : Type.HeightFromPositionToGround;
            const float maxThreshold = .2f;
            return mainBodyGroundDistance - maxThreshold < distanceFromPositionToFloor && groundBody != null;
        }

        public float GetElapsedTimeSinceLastGroundContact()
        {
            return notOnGroundTime;
        }

        protected override void OnSave(TextBlock block)
        {
            if (mainBody != null)
                linearVelocityForSerialization = mainBody.LinearVelocity;

            base.OnSave(block);
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SetTurnToPosition(Position + Rotation.GetForward() * 100);

            CreateMainBody();
            if (mainBody != null)
                mainBody.LinearVelocity = linearVelocityForSerialization;

            PhysicsWorld.Instance.MainScene.PostStep += MainScene_PostStep;

            SubscribeToTickEvent();
        }

        protected override void OnDestroy()
        {
            if (PhysicsWorld.Instance != null)
                PhysicsWorld.Instance.MainScene.PostStep -= MainScene_PostStep;

            base.OnDestroy();
        }

        protected override void OnSuspendPhysicsDuringMapLoading(bool suspend)
        {
            base.OnSuspendPhysicsDuringMapLoading(suspend);

            //After loading a map, the physics simulate 5 seconds, that bodies have fallen asleep.
            //During this time we will disable physics for this entity.
            if (fixRotationJointBody != null)
            {
                mainBody.PhysX_Kinematic = suspend;
                if (!suspend)
                    mainBody.Sleeping = false;
            }
            else
            {
                foreach (Body body in PhysicsModel.Bodies)
                {
                    body.Static = suspend;
                    if (!suspend)
                        mainBody.Sleeping = false;
                }
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            //clear groundBody when disposed
            if (groundBody != null && groundBody.IsDisposed)
                groundBody = null;

            TickMovement();

            if (Intellect != null)
                TickIntellect(Intellect);

            UpdateRotation(true);
            if (Type.JumpSupport)
                TickJump(false);

            if (Type.AllowJet)
            {
                ShiftBooster();
                tickJetSound();
            }

            if (IsOnGround())
                onGroundTime += TickDelta;
            else
                onGroundTime = 0;
            if (!IsOnGround())
                notOnGroundTime += TickDelta;
            else
                notOnGroundTime = 0;
            CalculateGroundRelativeVelocity();

            if (forceMoveVectorTimer != 0)
                forceMoveVectorTimer--;

            if (Type.CrouchingSupport)
                TickCrouching();

            #region Ladders

            if (Intellect != null)
                TickLadder();

            #endregion Ladders

            if (Type.DamageFastChangeSpeedFactor != 0)
                DamageFastChangeSpeedTick();

            //update fixRotationJointBody
            if (fixRotationJointBody != null)
                fixRotationJointBody.Position = Position;

            lastTickPosition = Position;

            if (forceIsOnGroundRemainingTime > 0)
            {
                forceIsOnGroundRemainingTime -= TickDelta;
                if (forceIsOnGroundRemainingTime < 0)
                    forceIsOnGroundRemainingTime = 0;
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.Client_OnTick()"/>.</summary>
        protected override void Client_OnTick()
        {
            base.Client_OnTick();

            //clear groundBody when disposed
            if (groundBody != null && groundBody.IsDisposed)
                groundBody = null;

            Vec3 addForceOnBigSlope;
            CalculateMainBodyGroundDistanceAndGroundBody(out addForceOnBigSlope);

            if (IsOnGround())
                onGroundTime += TickDelta;
            else
                onGroundTime = 0;
            if (!IsOnGround())
                notOnGroundTime += TickDelta;
            else
                notOnGroundTime = 0;
            CalculateGroundRelativeVelocity();
            lastTickPosition = Position;

            if (forceIsOnGroundRemainingTime > 0)
            {
                forceIsOnGroundRemainingTime -= TickDelta;
                if (forceIsOnGroundRemainingTime < 0)
                    forceIsOnGroundRemainingTime = 0;
            }

            JumpJetseffect();
        }

        public bool IsNeedRun()
        {
            bool run = false;
            if (Intellect != null)
                run = Intellect.IsAlwaysRun();
            else
                run = false;

            if (Intellect != null && Intellect.IsControlKeyPressed(GameControlKeys.Run))
                run = !run;

            return run;
        }

        private Vec2 GetMovementVectorByControlKeys()
        {
            //use specified force move vector
            if (forceMoveVectorTimer != 0)
                return forceMoveVector;

            //TPS arcade specific
            //vector is depending on camera orientation
            if (GameMap.Instance != null && GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade &&
                PlayerIntellect.Instance == Intellect)
            {
                //this is not adapted for networking.
                //using RendererWorld.Instance.DefaultCamera is bad.

                Vec2 localVector = Vec2.Zero;
                localVector.X += Intellect.GetControlKeyStrength(GameControlKeys.Forward);
                localVector.X -= Intellect.GetControlKeyStrength(GameControlKeys.Backward);
                localVector.Y += Intellect.GetControlKeyStrength(GameControlKeys.Left);
                localVector.Y -= Intellect.GetControlKeyStrength(GameControlKeys.Right);

                if (localVector != Vec2.Zero)
                {
                    Vec2 diff = Position.ToVec2() - RendererWorld.Instance.DefaultCamera.Position.ToVec2();
                    Degree angle = new Radian(MathFunctions.ATan(diff.Y, diff.X));
                    Degree vecAngle = new Radian(MathFunctions.ATan(-localVector.Y, localVector.X));
                    Quat rot = new Angles(0, 0, vecAngle - angle).ToQuat();
                    Vec2 vector = (rot * new Vec3(1, 0, 0)).ToVec2();
                    return vector;
                }
                else
                    return Vec2.Zero;
            }

            //PlatformerDemo specific
            if (GameMap.Instance != null && GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo &&
                PlayerIntellect.Instance == Intellect)
            {
                Vec2 vector = Vec2.Zero;
                vector.X -= Intellect.GetControlKeyStrength(GameControlKeys.Left);
                vector.X += Intellect.GetControlKeyStrength(GameControlKeys.Right);
                return vector;
            }

            //default behaviour
            {
                Vec2 localVector = Vec2.Zero;
                localVector.X += Intellect.GetControlKeyStrength(GameControlKeys.Forward);
                localVector.X -= Intellect.GetControlKeyStrength(GameControlKeys.Backward);
                localVector.Y += Intellect.GetControlKeyStrength(GameControlKeys.Left);
                localVector.Y -= Intellect.GetControlKeyStrength(GameControlKeys.Right);

                Vec2 vector = (new Vec3(localVector.X, localVector.Y, 0) * Rotation).ToVec2();
                if (vector != Vec2.Zero)
                {
                    float length = vector.Length();
                    if (length > 1)
                        vector /= length;
                }
                return vector;
            }
        }

        private void TickIntellect(Intellect intellect)
        {
            Vec2 forceVec = GetMovementVectorByControlKeys();
            if (forceVec != Vec2.Zero)
            {
                float speedCoefficient = 1;
                if (FastMoveInfluence != null)
                    speedCoefficient = FastMoveInfluence.Type.Coefficient;

                float maxSpeed;
                float force;

                if (IsOnGround())
                {
                    //calcualate maxSpeed and force on ground.

                    Vec2 localVec = (new Vec3(forceVec.X, forceVec.Y, 0) * Rotation.GetInverse()).ToVec2();

                    float absSum = Math.Abs(localVec.X) + Math.Abs(localVec.Y);
                    if (absSum > 1)
                        localVec /= absSum;

                    maxSpeed = 0;
                    force = 0;

                    if (!Crouching)
                    {
                        bool running = IsNeedRun();

                        if (Math.Abs(localVec.X) >= .001f)
                        {
                            //forward and backward
                            float speedX;
                            if (localVec.X > 0)
                                speedX = running ? Type.RunForwardMaxSpeed : Type.WalkForwardMaxSpeed;
                            else
                                speedX = running ? Type.RunBackwardMaxSpeed : Type.WalkBackwardMaxSpeed;
                            maxSpeed += speedX * Math.Abs(localVec.X);
                            force += (running ? Type.RunForce : Type.WalkForce) * Math.Abs(localVec.X);
                        }

                        if (Math.Abs(localVec.Y) >= .001f)
                        {
                            //left and right
                            maxSpeed += (running ? Type.RunSideMaxSpeed : Type.WalkSideMaxSpeed) *
                                Math.Abs(localVec.Y);
                            force += (running ? Type.RunForce : Type.WalkForce) * Math.Abs(localVec.Y);
                        }
                    }
                    else
                    {
                        maxSpeed = Type.CrouchingMaxSpeed;
                        force = Type.CrouchingForce;
                    }
                }
                else
                {
                    //calcualate maxSpeed and force when flying.
                    maxSpeed = Type.FlyControlMaxSpeed;
                    force = Type.FlyControlForce;
                }

                //speedCoefficient
                maxSpeed *= speedCoefficient;
                force *= speedCoefficient;

                if (GetLinearVelocity().ToVec2().Length() < maxSpeed)
                    mainBody.AddForce(ForceType.Global, 0, new Vec3(forceVec.X, forceVec.Y, 0) * force * TickDelta, Vec3.Zero);
            }

            lastTickForceVector = forceVec;
        }

        protected override void OnIntellectCommand(Intellect.Command command)
        {
            base.OnIntellectCommand(command);

            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                if (Type.JumpSupport && command.KeyPressed && command.Key == GameControlKeys.Jump)
                    TryJump();
            }
        }

        private void UpdateMainBodyDamping()
        {
            if (IsOnGround() && jumpInactiveTime == 0)
            {
                //small distinction of different physics libraries.
                if (PhysicsWorld.Instance.IsPhysX())
                    mainBody.LinearDamping = 9.3f;
                else
                    mainBody.LinearDamping = 10;
            }
            else
                mainBody.LinearDamping = .15f;
        }

        private void TickMovement()
        {
            //wake up when ground is moving
            if (mainBody.Sleeping && groundBody != null && !groundBody.Sleeping &&
                (groundBody.LinearVelocity.LengthSqr() > .3f ||
                groundBody.AngularVelocity.LengthSqr() > .3f))
            {
                mainBody.Sleeping = false;
            }

            Vec3 addForceOnBigSlope;
            CalculateMainBodyGroundDistanceAndGroundBody(out addForceOnBigSlope);

            if (!mainBody.Sleeping || !IsOnGround())
            {
                UpdateMainBodyDamping();

                if (IsOnGround())
                {
                    //reset angular velocity
                    mainBody.AngularVelocity = Vec3.Zero;

                    //move the object when it underground
                    if (lastTickForceVector != Vec2.Zero && forceIsOnGroundRemainingTime == 0)
                    {
                        Vec3 newPositionOffset =
                            new Vec3(lastTickForceVector.GetNormalize() * Type.WalkSideMaxSpeed * .15f, 0);

                        float upHeight;
                        ClimbObstacleTest(newPositionOffset, out upHeight);

                        //move object
                        float height;
                        float walkUpHeight;
                        float fromPositionToFloorDistance;
                        Type.GetBodyFormInfo(crouching, out height, out walkUpHeight, out fromPositionToFloorDistance);
                        if (upHeight > .01f && upHeight <= walkUpHeight && jumpInactiveTime == 0)
                        {
                            //bool keepDisableControlPhysicsModelPushedToWorldFlag = DisableControlPhysicsModelPushedToWorldFlag;
                            //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                            //   DisableControlPhysicsModelPushedToWorldFlag = true;
                            Position = Position + new Vec3(0, 0, upHeight);
                            //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                            //   DisableControlPhysicsModelPushedToWorldFlag = false;

                            forceIsOnGroundRemainingTime = .2f;
                        }
                    }
                }

                //add force to body on big slope
                if (addForceOnBigSlope != Vec3.Zero)
                    mainBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta, addForceOnBigSlope, Vec3.Zero);

                //on dynamic ground velocity
                if (IsOnGround() && groundBody != null)
                {
                    if (!groundBody.Static && !groundBody.Sleeping)
                    {
                        Vec3 groundVel = groundBody.LinearVelocity;

                        Vec3 vel = mainBody.LinearVelocity;

                        if (groundVel.X > 0 && vel.X >= 0 && vel.X < groundVel.X)
                            vel.X = groundVel.X;
                        else if (groundVel.X < 0 && vel.X <= 0 && vel.X > groundVel.X)
                            vel.X = groundVel.X;

                        if (groundVel.Y > 0 && vel.Y >= 0 && vel.Y < groundVel.Y)
                            vel.Y = groundVel.Y;
                        else if (groundVel.Y < 0 && vel.Y <= 0 && vel.Y > groundVel.Y)
                            vel.Y = groundVel.Y;

                        if (groundVel.Z > 0 && vel.Z >= 0 && vel.Z < groundVel.Z)
                            vel.Z = groundVel.Z;
                        else if (groundVel.Z < 0 && vel.Z <= 0 && vel.Z > groundVel.Z)
                            vel.Z = groundVel.Z;

                        mainBody.LinearVelocity = vel;

                        //stupid anti damping
                        mainBody.LinearVelocity += groundVel * .25f;
                    }
                }

                //sleep if on ground and zero velocity

                bool needSleep = false;
                if (IsOnGround() && groundBody != null)
                {
                    bool groundStopped = groundBody.Sleeping ||
                        (groundBody.LinearVelocity.LengthSqr() < .3f && groundBody.AngularVelocity.LengthSqr() < .3f);
                    if (groundStopped && GetLinearVelocity().ToVec2().Length() < Type.MinSpeedToSleepBody)
                        needSleep = true;
                }

                //strange fix for PhysX. The character can frezee in fly with zero linear velocity.
                if (PhysicsWorld.Instance.IsPhysX())
                {
                    if (!needSleep && mainBody.LinearVelocity == Vec3.Zero && lastTickPosition == Position)
                    {
                        mainBody.Sleeping = true;
                        needSleep = false;
                    }
                }

                if (needSleep)
                    allowToSleepTime += TickDelta;
                else
                    allowToSleepTime = 0;
                mainBody.Sleeping = allowToSleepTime > TickDelta * 2.5f;
            }
        }

        private bool VolumeCheckGetFirstNotFreePlace(Capsule[] sourceVolumeCapsules, Vec3 destVector, bool firstIteration, float step,
            out List<Body> collisionBodies, out float collisionDistance, out bool collisionOnFirstCheck)
        {
            collisionBodies = new List<Body>();
            collisionDistance = 0;
            collisionOnFirstCheck = false;

            bool firstCheck = true;

            Vec3 direction = destVector.GetNormalize();
            float totalDistance = destVector.Length();
            int stepCount = (int)((float)totalDistance / step) + 2;
            Vec3 previousOffset = Vec3.Zero;

            for (int nStep = 0; nStep < stepCount; nStep++)
            {
                float distance = (float)nStep * step;
                if (distance > totalDistance)
                    distance = totalDistance;
                Vec3 offset = direction * distance;

                foreach (Capsule sourceVolumeCapsule in sourceVolumeCapsules)
                {
                    Capsule checkCapsule = CapsuleAddOffset(sourceVolumeCapsule, offset);

                    Body[] bodies = PhysicsWorld.Instance.VolumeCast(checkCapsule, (int)ContactGroup.CastOnlyContact);
                    foreach (Body body in bodies)
                    {
                        if (body == mainBody || body == fixRotationJointBody)
                            continue;
                        collisionBodies.Add(body);
                    }
                }

                if (collisionBodies.Count != 0)
                {
                    //second iteration
                    if (nStep != 0 && firstIteration)
                    {
                        float step2 = step / 10;
                        Capsule[] sourceVolumeCapsules2 = new Capsule[sourceVolumeCapsules.Length];
                        for (int n = 0; n < sourceVolumeCapsules2.Length; n++)
                            sourceVolumeCapsules2[n] = CapsuleAddOffset(sourceVolumeCapsules[n], previousOffset);
                        Vec3 destVector2 = offset - previousOffset;

                        List<Body> collisionBodies2;
                        float collisionDistance2;
                        bool collisionOnFirstCheck2;
                        if (VolumeCheckGetFirstNotFreePlace(sourceVolumeCapsules2, destVector2, false, step2, out collisionBodies2,
                            out collisionDistance2, out collisionOnFirstCheck2))
                        {
                            collisionBodies = collisionBodies2;
                            collisionDistance = (previousOffset != Vec3.Zero ? previousOffset.Length() : 0) + collisionDistance2;
                            collisionOnFirstCheck = false;
                            return true;
                        }
                    }

                    collisionDistance = distance;
                    collisionOnFirstCheck = firstCheck;
                    return true;
                }

                firstCheck = false;
                previousOffset = offset;
            }

            return false;
        }

        private void CalculateMainBodyGroundDistanceAndGroundBody(out Vec3 addForceOnBigSlope)
        {
            addForceOnBigSlope = Vec3.Zero;

            float height;
            float walkUpHeight;
            float fromPositionToFloorDistance;
            Type.GetBodyFormInfo(crouching, out height, out walkUpHeight, out fromPositionToFloorDistance);

            Capsule[] volumeCapsules = GetVolumeCapsules();
            //make radius smaller
            for (int n = 0; n < volumeCapsules.Length; n++)
            {
                Capsule capsule = volumeCapsules[n];
                capsule.Radius *= .99f;
                volumeCapsules[n] = capsule;
            }

            mainBodyGroundDistance = 1000;
            groundBody = null;

            //1. get collision bodies
            List<Body> collisionBodies;
            float collisionOffset = 0;
            {
                Vec3 destVector = new Vec3(0, 0, -height * 1.5f);
                float step = Type.Radius / 2;
                float collisionDistance;
                bool collisionOnFirstCheck;
                VolumeCheckGetFirstNotFreePlace(volumeCapsules, destVector, true, step, out collisionBodies, out collisionDistance,
                    out collisionOnFirstCheck);

                collisionOffset = collisionDistance;

                //for( float offset = 0; offset < height * 1.5f; offset += step )
                //{
                //   foreach( Capsule sourceVolumeCapsule in volumeCapsules )
                //   {
                //      Capsule checkCapsule = CapsuleAddOffset( sourceVolumeCapsule, new Vec3( 0, 0, -offset ) );

                //      Body[] bodies = PhysicsWorld.Instance.VolumeCast( checkCapsule, (int)ContactGroup.CastOnlyContact );
                //      foreach( Body body in bodies )
                //      {
                //         if( body == mainBody || body == fixRotationJointBody )
                //            continue;
                //         collisionBodies.Add( body );
                //      }
                //   }

                //   if( collisionBodies.Count != 0 )
                //   {
                //      collisionOffset = offset;
                //      break;
                //   }

                //   firstCheck = false;
                //}
            }

            //2. check slope angle
            if (collisionBodies.Count != 0)
            {
                Capsule capsule = volumeCapsules[volumeCapsules.Length - 1];
                Vec3 rayCenter = capsule.Point1 - new Vec3(0, 0, collisionOffset);

                Body foundBodyWithGoodAngle = null;
                Vec3 bigSlopeVector = Vec3.Zero;

                const int horizontalStepCount = 16;
                const int verticalStepCount = 8;

                for (int verticalStep = 0; verticalStep < verticalStepCount; verticalStep++)
                {
                    //.8f - to disable checking by horizontal rays
                    float verticalAngle = MathFunctions.PI / 2 -
                        ((float)verticalStep / (float)verticalStepCount) * MathFunctions.PI / 2 * .8f;

                    for (int horizontalStep = 0; horizontalStep < horizontalStepCount; horizontalStep++)
                    {
                        //skip same rays on direct vertical ray
                        if (verticalStep == 0 && horizontalStep != 0)
                            continue;

                        float horizontalAngle = ((float)horizontalStep / (float)horizontalStepCount) * MathFunctions.PI * 2;

                        SphereDir sphereDir = new SphereDir(horizontalAngle, -verticalAngle);
                        Ray ray = new Ray(rayCenter, sphereDir.GetVector() * Type.Radius * 1.3f);
                        //Ray ray = new Ray( rayCenter, sphereDir.GetVector() * Type.Radius * 1.1f );
                        RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(ray,
                            (int)ContactGroup.CastOnlyContact);

                        //{
                        //   DebugGeometry debugGeometry = RendererWorld.Instance.DefaultCamera.DebugGeometry;
                        //   debugGeometry.Color = new ColorValue( 1, 0, 0 );
                        //   debugGeometry.AddLine( ray.Origin, ray.Origin + ray.Direction );
                        //}

                        if (piercingResult.Length != 0)
                        {
                            foreach (RayCastResult result in piercingResult)
                            {
                                if (result.Shape.Body != mainBody && result.Shape.Body != fixRotationJointBody)
                                {
                                    //Log.Info( result.Normal.ToString() + " " +
                                    //   MathUtils.GetVectorsAngle( result.Normal, Vec3.ZAxis ).ToString() + " " +
                                    //   Type.MaxSlopeAngle.InRadians() );

                                    //{
                                    //   DebugGeometry debugGeometry = RendererWorld.Instance.DefaultCamera.DebugGeometry;
                                    //   debugGeometry.Color = new ColorValue( 1, 1, 0 );
                                    //   debugGeometry.AddLine( ray.Origin, ray.Origin + ray.Direction );
                                    //}
                                    //{
                                    //   DebugGeometry debugGeometry = RendererWorld.Instance.DefaultCamera.DebugGeometry;
                                    //   debugGeometry.Color = new ColorValue( 0, 0, 1 );
                                    //   debugGeometry.AddLine( result.Position, result.Position + result.Normal );
                                    //}

                                    //check slope angle
                                    if (MathUtils.GetVectorsAngle(result.Normal, Vec3.ZAxis) < Type.MaxSlopeAngle.InRadians())
                                    {
                                        foundBodyWithGoodAngle = result.Shape.Body;
                                        break;
                                    }
                                    else
                                    {
                                        Vec3 vector = new Vec3(result.Normal.X, result.Normal.Y, 0);
                                        if (vector != Vec3.Zero)
                                            bigSlopeVector += vector;
                                    }
                                }
                            }

                            if (foundBodyWithGoodAngle != null)
                                break;
                        }
                    }
                    if (foundBodyWithGoodAngle != null)
                        break;
                }

                if (foundBodyWithGoodAngle != null)
                {
                    groundBody = foundBodyWithGoodAngle;
                    mainBodyGroundDistance = fromPositionToFloorDistance + collisionOffset;
                }
                else
                {
                    if (bigSlopeVector != Vec3.Zero)
                    {
                        //add force
                        bigSlopeVector.Normalize();
                        bigSlopeVector *= mainBody.Mass * 2;
                        addForceOnBigSlope = bigSlopeVector;
                    }
                }
            }
        }

        private Capsule GetWorldCapsule(CapsuleShape shape)
        {
            Capsule capsule = new Capsule();

            Vec3 pos = shape.Body.Position + shape.Body.Rotation * shape.Position;
            Quat rot = shape.Body.Rotation * shape.Rotation;

            Vec3 diff = rot * new Vec3(0, 0, shape.Length * .5f);
            capsule.Point1 = pos - diff;
            capsule.Point2 = pos + diff;
            capsule.Radius = shape.Radius;

            return capsule;
        }

        private Capsule[] GetVolumeCapsules()
        {
            Capsule[] volumeCapsules = new Capsule[mainBody.Shapes.Length];
            for (int n = 0; n < volumeCapsules.Length; n++)
                volumeCapsules[n] = GetWorldCapsule((CapsuleShape)mainBody.Shapes[n]);
            return volumeCapsules;
        }

        private Capsule CapsuleAddOffset(Capsule capsule, Vec3 offset)
        {
            return new Capsule(capsule.Point1 + offset, capsule.Point2 + offset, capsule.Radius);
        }

        private void ClimbObstacleTest(Vec3 newPositionOffset, out float upHeight)
        {
            float height;
            float walkUpHeight;
            float fromPositionToFloorDistance;
            Type.GetBodyFormInfo(crouching, out height, out walkUpHeight, out fromPositionToFloorDistance);

            Capsule[] volumeCapsules = GetVolumeCapsules();
            {
                Vec3 offset = newPositionOffset + new Vec3(0, 0, walkUpHeight);
                for (int n = 0; n < volumeCapsules.Length; n++)
                    volumeCapsules[n] = CapsuleAddOffset(volumeCapsules[n], offset);
            }

            Vec3 destVector = new Vec3(0, 0, -walkUpHeight);
            float step = Type.Radius / 2;
            List<Body> collisionBodies;
            float collisionDistance;
            bool collisionOnFirstCheck;
            bool foundCollision = VolumeCheckGetFirstNotFreePlace(volumeCapsules, destVector, true, step, out collisionBodies,
                out collisionDistance, out collisionOnFirstCheck);
            if (foundCollision)
            {
                if (collisionOnFirstCheck)
                    upHeight = float.MaxValue;
                else
                    upHeight = walkUpHeight - collisionDistance;
            }
            else
                upHeight = 0;

            //upHeight = float.MaxValue;

            //Capsule[] volumeCapsules = GetVolumeCapsules();
            //for( int n = 0; n < volumeCapsules.Length; n++ )
            //   volumeCapsules[ n ] = CapsuleAddOffset( volumeCapsules[ n ], newPositionOffset );

            //float step = .01f;

            //bool firstCheck = true;
            //bool foundNotFree = false;

            //for( float height = walkUpHeight; height >= 0; height -= step )
            //{
            //   bool free = true;

            //   foreach( Capsule sourceVolumeCapsule in volumeCapsules )
            //   {
            //      Capsule checkCapsule = CapsuleAddOffset( sourceVolumeCapsule, new Vec3( 0, 0, height ) );

            //      Body[] bodies = PhysicsWorld.Instance.VolumeCast( checkCapsule, (int)ContactGroup.CastOnlyContact );
            //      foreach( Body body in bodies )
            //      {
            //         if( body == mainBody || body == fixRotationJointBody )
            //            continue;
            //         free = false;
            //         break;
            //      }
            //   }

            //   if( !free )
            //   {
            //      if( firstCheck )
            //         upHeight = float.MaxValue;
            //      else
            //         upHeight = height;
            //      foundNotFree = true;
            //      break;
            //   }

            //   firstCheck = false;
            //}

            //if( !foundNotFree )
            //   upHeight = 0;
        }

        protected virtual void OnJump()
        {
            SoundPlay3D(Type.SoundJump, .5f, true);
        }

        private void TickJump(bool ignoreTicks)
        {
            if (!ignoreTicks)
            {
                if (shouldJumpTime != 0)
                {
                    shouldJumpTime -= TickDelta;
                    if (shouldJumpTime < 0)
                        shouldJumpTime = 0;
                }

                if (jumpInactiveTime != 0)
                {
                    jumpInactiveTime -= TickDelta;
                    if (jumpInactiveTime < 0)
                        jumpInactiveTime = 0;
                }
            }

            if (IsOnGround() && onGroundTime > TickDelta && jumpInactiveTime == 0 && shouldJumpTime != 0)
            {
                Vec3 vel = mainBody.LinearVelocity;
                vel.Z = Type.JumpSpeed;
                mainBody.LinearVelocity = vel;

                //bool keepDisableControlPhysicsModelPushedToWorldFlag = DisableControlPhysicsModelPushedToWorldFlag;
                //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                //   DisableControlPhysicsModelPushedToWorldFlag = true;
                Position += new Vec3(0, 0, .05f);
                //if( !keepDisableControlPhysicsModelPushedToWorldFlag )
                //   DisableControlPhysicsModelPushedToWorldFlag = false;

                jumpInactiveTime = .2f;
                shouldJumpTime = 0;

                UpdateMainBodyDamping();

                OnJump();

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendJumpEventToAllClients();
            }

            #region Ladders

            else if (currentLadder != null && shouldJumpTime != 0)
            {
                JumpFromLadder();
            }

            #endregion Ladders
        }

        public void TryJump()
        {
            if (!Type.JumpSupport)
                return;
            if (Crouching)
                return;

            //cannot called on client.
            if (EntitySystemWorld.Instance.IsClientOnly())
                Log.Fatal("Character: TryJump: EntitySystemWorld.Instance.IsClientOnly().");

            shouldJumpTime = .4f;
            TickJump(true);
        }

        [Browsable(false)]
        public Vec2 LastTickForceVector
        {
            get { return lastTickForceVector; }
        }

        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            Vec3 oldPos = Position;

            base.OnSetTransform(ref pos, ref rot, ref scl);

            if ((oldPos - Position).Length() > .3f)
            {
                if (PhysicsModel != null)
                {
                    foreach (Body body in PhysicsModel.Bodies)
                        body.Sleeping = false;
                }
            }
        }

        private void CalculateGroundRelativeVelocity()
        {
            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                //server or single mode

                if (mainBody != null)
                {
                    groundRelativeVelocity = GetLinearVelocity();
                    if (groundBody != null && groundBody.AngularVelocity.LengthSqr() < .3f)
                        groundRelativeVelocity -= groundBody.LinearVelocity;
                }
                else
                    groundRelativeVelocity = Vec3.Zero;

                if (EntitySystemWorld.Instance.IsServer())
                {
                    if (!groundRelativeVelocity.Equals(server_sentGroundRelativeVelocity, .1f))
                    {
                        Server_SendGroundRelativeVelocityToClients(
                            EntitySystemWorld.Instance.RemoteEntityWorlds, groundRelativeVelocity);
                        server_sentGroundRelativeVelocity = groundRelativeVelocity;
                    }
                }
            }
            else
            {
                //client

                //groundRelativeVelocity is updated from server,
                //because body velocities are not synchronized via network.
            }

            //groundRelativeVelocityToSmooth
            if (groundRelativeVelocitySmoothArray == null)
            {
                float seconds = .2f;
                float count = (seconds / TickDelta) + .999f;
                groundRelativeVelocitySmoothArray = new Vec3[(int)count];
            }
            for (int n = 0; n < groundRelativeVelocitySmoothArray.Length - 1; n++)
                groundRelativeVelocitySmoothArray[n] = groundRelativeVelocitySmoothArray[n + 1];
            groundRelativeVelocitySmoothArray[groundRelativeVelocitySmoothArray.Length - 1] = groundRelativeVelocity;
            groundRelativeVelocitySmooth = Vec3.Zero;
            for (int n = 0; n < groundRelativeVelocitySmoothArray.Length; n++)
                groundRelativeVelocitySmooth += groundRelativeVelocitySmoothArray[n];
            groundRelativeVelocitySmooth /= (float)groundRelativeVelocitySmoothArray.Length;
        }

        [Browsable(false)]
        public Vec3 GroundRelativeVelocity
        {
            get { return groundRelativeVelocity; }
        }

        [Browsable(false)]
        public Vec3 GroundRelativeVelocitySmooth
        {
            get { return groundRelativeVelocitySmooth; }
        }

        public Vec3 GetLinearVelocity()
        {
            if (EntitySystemWorld.Instance.Simulation)
                return (Position - lastTickPosition) / TickDelta;
            return Vec3.Zero;
        }

        private void Server_SendJumpEventToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Character),
                (ushort)NetworkMessages.JumpEventToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.JumpEventToClient)]
        private void Client_ReceiveJumpEvent(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            OnJump();
        }

        protected override void Server_OnClientConnectedAfterPostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };
            Server_SendGroundRelativeVelocityToClients(worlds, server_sentGroundRelativeVelocity);
        }

        private void Server_SendGroundRelativeVelocityToClients(IList<RemoteEntityWorld> remoteEntityWorlds,
            Vec3 value)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Character),
                (ushort)NetworkMessages.GroundRelativeVelocityToClient);
            writer.Write(value);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.GroundRelativeVelocityToClient)]
        private void Client_ReceiveGroundRelativeVelocity(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 value = reader.ReadVec3();
            if (!reader.Complete())
                return;
            groundRelativeVelocity = value;
        }

        public override bool IsAllowToChangeScale(out string reason)
        {
            reason = ToolsLocalization.Translate("Various", "Characters do not support scaling.");
            return false;
        }

        private void MainScene_PostStep(PhysicsScene scene)
        {
            if (mainBody != null && !mainBody.Sleeping)
                UpdateRotation(false);
        }

        public void DamageFastChangeSpeedResetLastVelocity()
        {
            damageFastChangeSpeedLastVelocity = new Vec3(float.NaN, float.NaN, float.NaN);
        }

        private void DamageFastChangeSpeedTick()
        {
            if (MainBody == null)
                return;

            if (Intellect == null)
            {
                return;
            }

            Vec3 velocity = MainBody.LinearVelocity;

            if (float.IsNaN(damageFastChangeSpeedLastVelocity.X))
                damageFastChangeSpeedLastVelocity = velocity;

            Vec3 diff = velocity - damageFastChangeSpeedLastVelocity;
            if (diff.Z > 0)
            {
                float v = diff.Z - Type.DamageFastChangeSpeedMinimalSpeed;
                if (v > 0)
                {
                    float damage = v * Type.DamageFastChangeSpeedFactor;
                    if (damage > 0)
                        DoDamage(null, Position, null, damage, true);
                }
            }

            damageFastChangeSpeedLastVelocity = velocity;
        }

        [Browsable(false)]
        public bool Crouching
        {
            get { return crouching; }
        }

        private void CreateMainBody()
        {
            DestroyMainBody();

            CreatePhysicsModel();

            float height;
            float walkUpHeight;
            float fromPositionToFloorDistance;
            Type.GetBodyFormInfo(crouching, out height, out walkUpHeight, out fromPositionToFloorDistance);

            {
                Body body = PhysicsModel.CreateBody();
                mainBody = body;
                body.Name = "main";
                body.Position = Position;
                body.Rotation = Rotation;
                body.Sleepiness = 0;
                body.AngularDamping = 10;
                //body.CenterOfMassPosition = new Vec3( 0, 0, -height / 4 );
                body.MassMethod = Body.MassMethods.Manually;
                body.Mass = Type.Mass;
                //body.InertiaTensorFactor = new Vec3( 100, 100, 100 );
                body.PhysX_SolverPositionIterations = body.PhysX_SolverPositionIterations * 2;
                body.PhysX_SolverVelocityIterations = body.PhysX_SolverVelocityIterations * 2;

                float length = height - Type.Radius * 2;
                if (length < 0)
                {
                    Log.Error("Character: OnPostCreate: height - Type.Radius * 2 < 0.");
                    return;
                }

                //create main capsule
                {
                    CapsuleShape shape = body.CreateCapsuleShape();
                    shape.Length = length;

                    float offset = fromPositionToFloorDistance - height / 2;
                    shape.Position = new Vec3(0, 0, -offset);

                    shape.Radius = Type.Radius;
                    shape.ContactGroup = (int)ContactGroup.Dynamic;
                    shape.StaticFriction = 0;
                    shape.DynamicFriction = 0;
                    shape.Restitution = 0;
                    shape.Hardness = 0;
                    shape.SpecialLiquidDensity = 1500;
                }

                //{
                //   //ODE. create two capsules

                //   xx
                //   //float offset = fromPositionToFloorDistance - height / 2;

                //   float length = height - Type.Radius * 2 - walkUpHeight;
                //   if( length < 0 )
                //   {
                //      Log.Error( "Character: CreateMainBody: height - Type.Radius * 2 - walkUpHeight < 0." );
                //      return;
                //   }

                //   //create main capsule
                //   {
                //      CapsuleShape shape = body.CreateCapsuleShape();
                //      shape.Length = length;
                //      shape.Radius = Type.Radius;
                //      shape.ContactGroup = (int)ContactGroup.Dynamic;
                //      shape.StaticFriction = 0;
                //      shape.DynamicFriction = 0;
                //      shape.Restitution = 0;
                //      shape.Hardness = 0;
                //      shape.SpecialLiquidDensity = 1500;
                //   }

                //   //create bottom capsule
                //   {
                //      CapsuleShape shape = body.CreateCapsuleShape();
                //      shape.Length = height - Type.BottomRadius * 2;
                //      shape.Radius = Type.BottomRadius;
                //      shape.Position = new Vec3( 0, 0, ( height - walkUpHeight ) / 2 - height / 2 );
                //      shape.ContactGroup = (int)ContactGroup.Dynamic;
                //      shape.StaticFriction = 0;
                //      shape.DynamicFriction = 0;
                //      shape.Restitution = 0;
                //      shape.Hardness = 0;
                //      shape.SpecialLiquidDensity = 1500;
                //   }
                //}
            }

            //create joint to fix rotation of the main body.
            //PhysX only
            if (PhysicsWorld.Instance.IsPhysX())
            {
                //create not contactable Kinematic body to attach joint
                Body body = PhysicsModel.CreateBody();
                fixRotationJointBody = body;
                body.PhysX_Kinematic = true;
                //body.Static = true;
                body.Name = "static";
                body.Position = Position;

                SphereShape shape = body.CreateSphereShape();
                shape.Radius = Type.Radius / 5;
                shape.ContactGroup = (int)ContactGroup.NoContact;

                //create joint
                fixRotationJoint = (BallJoint)PhysicsModel.CreateJoint(Joint.Type.Ball, mainBody, fixRotationJointBody);
                fixRotationJoint.ContactsEnabled = false;
                fixRotationJoint.Anchor = Position;
                fixRotationJoint.PhysX_MotionLockedAxisX = false;
                fixRotationJoint.PhysX_MotionLockedAxisY = false;
                fixRotationJoint.PhysX_MotionLockedAxisZ = false;

                fixRotationJoint.Axis1.LimitsEnabled = true;
                fixRotationJoint.Axis1.LimitLow = 0;
                fixRotationJoint.Axis1.LimitHigh = 0;
                fixRotationJoint.Axis1.Direction = Vec3.XAxis;

                fixRotationJoint.Axis2.LimitsEnabled = true;
                fixRotationJoint.Axis2.LimitLow = 0;
                fixRotationJoint.Axis2.LimitHigh = 0;
                fixRotationJoint.Axis2.Direction = Vec3.YAxis;

                fixRotationJoint.Axis3.Direction = Vec3.ZAxis;
            }

            PhysicsModel.PushToWorld();
            DisableControlPhysicsModelPushedToWorldFlag = true;
        }

        private void DestroyMainBody()
        {
            mainBody = null;
            fixRotationJointBody = null;
            fixRotationJoint = null;
            DestroyPhysicsModel();
        }

        private void ReCreateMainBody()
        {
            Vec3 oldLinearVelocity = Vec3.Zero;
            float oldLinearDamping = 0;
            if (mainBody != null)
            {
                oldLinearVelocity = mainBody.LinearVelocity;
                oldLinearDamping = mainBody.LinearDamping;
            }

            CreateMainBody();
            if (mainBody != null)
            {
                mainBody.LinearVelocity = oldLinearVelocity;
                mainBody.LinearDamping = oldLinearDamping;
            }
        }

        private void TickCrouching()
        {
            if (crouchingSwitchRemainingTime > 0)
            {
                crouchingSwitchRemainingTime -= TickDelta;
                if (crouchingSwitchRemainingTime < 0)
                    crouchingSwitchRemainingTime = 0;
            }

            if (Intellect != null && crouchingSwitchRemainingTime == 0)
            {
                bool needCrouching = Intellect.IsControlKeyPressed(GameControlKeys.Crouching);

                if (crouching != needCrouching)
                {
                    Vec3 newPosition;
                    {
                        float diff = Type.HeightFromPositionToGround - Type.CrouchingHeightFromPositionToGround;
                        if (needCrouching)
                            newPosition = Position + new Vec3(0, 0, -diff);
                        else
                            newPosition = Position + new Vec3(0, 0, diff);
                    }

                    bool freePlace = true;
                    {
                        Capsule capsule;
                        {
                            float radius = Type.Radius - .01f;

                            float length;
                            if (needCrouching)
                                length = Type.CrouchingHeight - radius * 2 - Type.CrouchingWalkUpHeight;
                            else
                                length = Type.Height - radius * 2 - Type.WalkUpHeight;

                            capsule = new Capsule(
                                newPosition + new Vec3(0, 0, -length / 2),
                                newPosition + new Vec3(0, 0, length / 2), radius);
                        }

                        Body[] bodies = PhysicsWorld.Instance.VolumeCast(capsule, (int)ContactGroup.CastOnlyContact);
                        foreach (Body body in bodies)
                        {
                            if (body == mainBody)
                                continue;

                            freePlace = false;
                            break;
                        }
                    }

                    if (freePlace)
                    {
                        crouching = needCrouching;
                        crouchingSwitchRemainingTime = .3f;

                        ReCreateMainBody();

                        Position = newPosition;
                        OldPosition = Position;

                        Vec3 addForceOnBigSlope;
                        CalculateMainBodyGroundDistanceAndGroundBody(out addForceOnBigSlope);
                    }
                }
            }
        }

        protected override void OnRenderFrame()
        {
            base.OnRenderFrame();

            if ((crouching && crouchingVisualFactor < 1) || (!crouching && crouchingVisualFactor > 0))
            {
                float delta = RendererWorld.Instance.FrameRenderTimeStep / crouchingVisualSwitchTime;
                if (crouching)
                {
                    crouchingVisualFactor += delta;
                    if (crouchingVisualFactor > 1)
                        crouchingVisualFactor = 1;
                }
                else
                {
                    crouchingVisualFactor -= delta;
                    if (crouchingVisualFactor < 0)
                        crouchingVisualFactor = 0;
                }
            }
        }

        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if (Visible && EngineDebugSettings.DrawGameSpecificDebugGeometry && !EntitySystemWorld.Instance.IsEditor())
            {
                float height;
                float walkUpHeight;
                float fromPositionToFloorDistance;
                Type.GetBodyFormInfo(crouching, out height, out walkUpHeight, out fromPositionToFloorDistance);

                camera.DebugGeometry.Color = new ColorValue(1, 0, 0, 1);

                //unit center
                camera.DebugGeometry.AddSphere(new Sphere(Position, .05f), 16);

                //ground height
                camera.DebugGeometry.AddSphere(new Sphere(Position - new Vec3(0, 0, fromPositionToFloorDistance), .05f), 16);

                //stand up height
                {
                    Vec3 pos = Position - new Vec3(0, 0, fromPositionToFloorDistance - walkUpHeight);
                    camera.DebugGeometry.AddLine(pos + new Vec3(.2f, 0, 0), pos - new Vec3(.2f, 0, 0));
                    camera.DebugGeometry.AddLine(pos + new Vec3(0, .2f, 0), pos - new Vec3(0, .2f, 0));
                }
            }
        }

        public override void GetFirstPersonCameraPosition(out Vec3 position, out Vec3 forward, out Vec3 up)
        {
            position = GetInterpolatedPosition() + Type.FPSCameraOffset * GetInterpolatedRotation();
            forward = Vec3.XAxis;
            up = Vec3.ZAxis;

            if (Type.CrouchingSupport)
            {
                if ((crouching && crouchingVisualFactor != 1) || (!crouching && crouchingVisualFactor != 0))
                {
                    float diff = Type.HeightFromPositionToGround - Type.CrouchingHeightFromPositionToGround;
                    if (!crouching)
                        position -= new Vec3(0, 0, diff * crouchingVisualFactor);
                    else
                        position += new Vec3(0, 0, diff * (1.0f - crouchingVisualFactor));
                }
            }

            //Character: wiggle camera when walking
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                //update wiggleWhenWalkingSpeedFactor
                {
                    float destinationFactor;
                    if (IsOnGround())
                    {
                        destinationFactor = GroundRelativeVelocitySmooth.Length() * .3f;
                        if (destinationFactor < .5f)
                            destinationFactor = 0;
                        if (destinationFactor > 1)
                            destinationFactor = 1;
                    }
                    else
                        destinationFactor = 0;

                    float step = RendererWorld.Instance.FrameRenderTimeStep * 5;
                    if (wiggleWhenWalkingSpeedFactor < destinationFactor)
                    {
                        wiggleWhenWalkingSpeedFactor += step;
                        if (wiggleWhenWalkingSpeedFactor > destinationFactor)
                            wiggleWhenWalkingSpeedFactor = destinationFactor;
                    }
                    else
                    {
                        wiggleWhenWalkingSpeedFactor -= step;
                        if (wiggleWhenWalkingSpeedFactor < destinationFactor)
                            wiggleWhenWalkingSpeedFactor = destinationFactor;
                    }
                }

                //change position
                {
                    float angle = EngineApp.Instance.Time * 10;
                    float radius = wiggleWhenWalkingSpeedFactor * .04f;
                    Vec3 localPosition = new Vec3(0,
                        MathFunctions.Cos(angle) * radius,
                        Math.Abs(MathFunctions.Sin(angle) * radius));
                    position += localPosition * GetInterpolatedRotation();
                }

                //change up vector
                {
                    float angle = EngineApp.Instance.Time * 20;
                    float radius = wiggleWhenWalkingSpeedFactor * .003f;
                    Vec3 localUp = new Vec3(
                        MathFunctions.Cos(angle) * radius,
                        MathFunctions.Sin(angle) * radius, 1);
                    localUp.Normalize();
                    up = localUp * GetInterpolatedRotation();
                }
            }

            //calculate forward
            PlayerIntellect intellect = Intellect as PlayerIntellect;
            if (intellect != null)
                forward = intellect.LookDirection.GetVector();
        }

        private bool fuleupdated;
        //private Angles preJumpJetRotAng;

        private void ShiftBooster()
        {
            if (!Type.AllowJet)
            {
                boosted = false;
            }

            if (Intellect == null)
                return;

            MapObjectAttachedParticle Shift1 = GetFirstAttachedObjectByAlias("Shift1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Shift2 = GetFirstAttachedObjectByAlias("Shift2") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke1 = GetFirstAttachedObjectByAlias("Smoke1") as MapObjectAttachedParticle;
            MapObjectAttachedParticle Smoke2 = GetFirstAttachedObjectByAlias("Smoke2") as MapObjectAttachedParticle;

            if (!fuleupdated)
            {
                jetFuel = Type.JetFuelMax;
                fuleupdated = true;
            }

            if (Intellect.IsControlKeyPressed(GameControlKeys.PCJumpForward) || Intellect.IsControlKeyPressed(GameControlKeys.PCJumpBackward) ||
                Intellect.IsControlKeyPressed(GameControlKeys.PCJumpLeft) || Intellect.IsControlKeyPressed(GameControlKeys.PCJumpRight) ||
                Intellect.IsControlKeyPressed(GameControlKeys.Jump) || Intellect.IsControlKeyPressed(GameControlKeys.PCDown))
            {
                if (jetFuel > 5) //todo: should this stay 5
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
                        float upwardforce = 0;

                        if (Intellect.IsControlKeyPressed(GameControlKeys.PCJumpForward))
                        {
                            forwardForce += Type.ForwardJumpJetForce;
                        }

                        if (Intellect.IsControlKeyPressed(GameControlKeys.PCJumpBackward))
                        {
                            forwardForce -= Type.ForwardJumpJetForce;
                        }

                        if (Intellect.IsControlKeyPressed(GameControlKeys.PCJumpLeft))
                        {
                            sidewaysForce += Type.SidewaysJumpJetForce;
                        }

                        if (Intellect.IsControlKeyPressed(GameControlKeys.PCJumpRight))
                        {
                            sidewaysForce -= Type.SidewaysJumpJetForce;
                        }

                        if (Intellect.IsControlKeyPressed(GameControlKeys.Jump) || Intellect.IsControlKeyPressed(GameControlKeys.PCJumpForward) ||
                            Intellect.IsControlKeyPressed(GameControlKeys.PCJumpLeft) || Intellect.IsControlKeyPressed(GameControlKeys.PCJumpRight) ||
                            Intellect.IsControlKeyPressed(GameControlKeys.PCJumpBackward))
                        {
                            // if (!IsOnGround())
                            upwardforce = Type.VerticalJumpJetForce;
                        }

                        if (Intellect.IsControlKeyPressed(GameControlKeys.PCDown))
                        {
                            //if (!IsOnGround())
                            upwardforce = -Type.VerticalJumpJetForce;
                        }

                        //else if (Intellect.IsControlKeyPressed(GameControlKeys.Forward) || Intellect.IsControlKeyPressed(GameControlKeys.Backward) ||
                        //    Intellect.IsControlKeyPressed(GameControlKeys.Left) || Intellect.IsControlKeyPressed(GameControlKeys.Right))
                        //{
                        //    if (!IsOnGround())
                        //    {
                        //        upwardforce = 1;
                        //        mainBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta,
                        //  mainBody.Rotation * new Vec3(forwardForce, sidewaysForce, upwardforce) * mainBody.Mass, Vec3.Zero);
                        //    }
                        //}

                        //Incin - Test localvectortest
                        //Change player LookDirection at rotation
                        //bool lookvectortest = true;
                        //PlayerIntellect intellect = Intellect as PlayerIntellect;
                        //Vec3 lookVector = new Vec3(0, 0, 0);
                        //if (intellect != null && lookvectortest == true)
                        //{
                        //    lookVector = intellect.LookDirection.GetVector();// *100;
                        //    //lookVector *= OldRotation.GetInverse();
                        //    //lookVector *= Rotation;
                        //    //intellect.LookDirection = SphereDir.FromVector(lookVector);
                        //    //this works with just up
                        //    mainBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta, mainBody.Rotation * lookVector * new Vec3(forwardForce, sidewaysForce, upwardforce) * mainBody.Mass, Vec3.Zero);
                        //}
                        //else
                        //{
                        mainBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta, mainBody.Rotation * new Vec3(forwardForce, sidewaysForce, upwardforce) * mainBody.Mass, Vec3.Zero);
                        //}

                        jetFuel -= 3 * Type.JumpJetFratio;
                        boosted = true;
                    }
                }
            }
            else
            {
                boosted = false;
                if (jetFuel < 1000)
                {
                    jetFuel += 1;
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
                Angles RotationNormAG = (mainBody.Rotation.GetNormalize() *
                    mainBody.Rotation).ToAngles();

                mainBody.AngularVelocity = new Vec3(RotationNormAG.Roll / 180,
                    RotationNormAG.Pitch / 180, mainBody.AngularVelocity.Z);
            }
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

        private void tickJetSound()
        {
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
    }
}