// Copyright (C) 2006-2009 NeoAxis Group Ltd.
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
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Character"/> entity type.
    /// </summary>

    //AKVTOL_True_Name = "Vertical Take Off and Landing Unit";

    public class MechCharacterType : UnitType
    {
        //start of foots
        [FieldSerialize]
        private string soundFoot1;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot1
        {
            get { return soundFoot1; }
            set { soundFoot1 = value; }
        }

        [FieldSerialize]
        private string soundFoot2;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot2
        {
            get { return soundFoot2; }
            set { soundFoot2 = value; }
        }

        [FieldSerialize]
        private string soundFoot3;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot3
        {
            get { return soundFoot3; }
            set { soundFoot3 = value; }
        }

        [FieldSerialize]
        private string soundFoot4;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot4
        {
            get { return soundFoot4; }
            set { soundFoot4 = value; }
        }

        [FieldSerialize]
        private string soundFoot5;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundFoot5
        {
            get { return soundFoot5; }
            set { soundFoot5 = value; }
        }

        //end of foots
        [FieldSerialize]
        private float runEnergyMax = 1000;

        [DefaultValue(1000f)]
        public float RunEnergyMax
        {
            get { return runEnergyMax; }
            set { runEnergyMax = value; }
        }

        private const float heightDefaultMax = 1.8f;

        [FieldSerialize]
        private float heightMax = heightDefaultMax;

        [DefaultValue(heightDefaultMax)]
        public float HeightMax
        {
            get { return heightMax; }
            set { heightMax = value; }
        }

        private const float heightDefault = 1.8f;

        [FieldSerialize]
        private float height = heightDefault;

        private const float radiusDefault = .4f;

        [FieldSerialize]
        private float radius = radiusDefault;

        private const float bottomRadiusDefault = .4f / 8;

        [FieldSerialize]
        private float bottomRadius = bottomRadiusDefault;

        private const float walkUpHeightDefault = .5f;

        [FieldSerialize]
        private float walkUpHeight = walkUpHeightDefault;

        private const float massDefault = 1;

        [FieldSerialize]
        private float mass = massDefault;

        private const float walkMaxVelocityDefault = 20;

        [FieldSerialize]
        private float walkMaxVelocity = walkMaxVelocityDefault;

        private const float walkForceDefault = 1500;

        [FieldSerialize]
        private float walkForce = walkForceDefault;

        private const float flyControlMaxVelocityDefault = 30;

        [FieldSerialize]
        private float flyControlMaxVelocity = flyControlMaxVelocityDefault;

        private const float flyControlForceDefault = 150;

        [FieldSerialize]
        private float flyControlForce = flyControlForceDefault;

        private const float jumpVelocityDefault = 4;

        [FieldSerialize]
        private float jumpVelocity = jumpVelocityDefault;

        [FieldSerialize]
        private string soundJump;

        private float floorHeight;//height from center to floor

        //

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

        [DefaultValue(walkMaxVelocityDefault)]
        public float WalkMaxVelocity
        {
            get { return walkMaxVelocity; }
            set { walkMaxVelocity = value; }
        }

        [DefaultValue(walkForceDefault)]
        public float WalkForce
        {
            get { return walkForce; }
            set { walkForce = value; }
        }

        [DefaultValue(flyControlMaxVelocityDefault)]
        public float FlyControlMaxVelocity
        {
            get { return flyControlMaxVelocity; }
            set { flyControlMaxVelocity = value; }
        }

        [DefaultValue(flyControlForceDefault)]
        public float FlyControlForce
        {
            get { return flyControlForce; }
            set { flyControlForce = value; }
        }

        [DefaultValue(jumpVelocityDefault)]
        public float JumpVelocity
        {
            get { return jumpVelocity; }
            set { jumpVelocity = value; }
        }

        [DefaultValue("")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string SoundJump
        {
            get { return soundJump; }
            set { soundJump = value; }
        }

        [Browsable(false)]
        public float FloorHeight
        {
            get { return floorHeight; }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            floorHeight = (height - walkUpHeight) * .5f + walkUpHeight;
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            //it is not known how will be used this sound (2D or 3D?).
            //Sound will preloaded as 2D only here.
            if (!string.IsNullOrEmpty(SoundJump))
                SoundWorld.Instance.SoundCreate(SoundJump, 0);
        }
    }

    /// <summary>
    /// Defines the physical characters.
    /// </summary>
    public class MechCharacter : Unit
    {
        private Foot leftFoot = new Foot();
        private Foot rightFoot = new Foot();

        private class Foot
        {
            public bool onGround = true;
        }

        [FieldSerialize]
        public int RunEnergy;

        private Body mainBody;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float mainBodyGroundDistance = 1000;//from center of body

        private Body groundBody;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float jumpInactiveTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float shouldJumpTime;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private float onGroundTime;

        private Vec3 turnToPosition;
        private Radian horizontalDirectionForUpdateRotation;

        //moveVector
        private int forceMoveVectorTimer;//if == 0 to disabled

        private Vec2 forceMoveVector;

        private Vec2 lastTickForceVector;

        private bool noWakeBodies;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private Vec3 linearVelocityForSerialization;

        private Vec3 groundRelativeVelocity;
        private Vec3 server_sentGroundRelativeVelocity;

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            JumpEventToClient,
            GroundRelativeVelocityToClient,
            UpdateRunEnergyToClient,
        }

        ///////////////////////////////////////////

        private MechCharacterType _type = null; public new MechCharacterType Type { get { return _type; } }

        public void SetForceMoveVector(Vec2 vec)
        {
            forceMoveVectorTimer = 2;
            forceMoveVector = vec;
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

            UpdateRotation();
        }

        public void UpdateRotation()
        {
            float halfAngle = horizontalDirectionForUpdateRotation * .5f;
            Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)),
                MathFunctions.Cos(halfAngle));
            noWakeBodies = true;
            Rotation = rot;
            noWakeBodies = false;

            OldRotation = rot;
        }

        public bool IsOnGround()
        {
            return mainBodyGroundDistance - .05f < Type.FloorHeight && groundBody != null;
        }

        protected override void OnSave(TextBlock block)
        {
            if (mainBody != null)
                linearVelocityForSerialization = mainBody.LinearVelocity;

            base.OnSave(block);
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SetTurnToPosition(Position + Rotation.GetForward() * 100);

            CreatePhysicsModel();

            Body body = PhysicsModel.CreateBody();
            mainBody = body;
            body.Name = "main";
            body.Position = Position;
            body.Rotation = Rotation;
            body.Sleepiness = 0;
            body.LinearVelocity = linearVelocityForSerialization;

            float length = Type.Height - Type.Radius * 2 - Type.WalkUpHeight;
            if (length < 0)
            {
                length = 2.5f;
                //Log.Error("Length < 0");
                //return;
            }

            //Bone headbone = null;
            MapObjectAttachedHelper Headhelper = GetFirstAttachedObjectByAlias("Head") as MapObjectAttachedHelper;
            //MapObjectAttachedMesh mainMesh = GetFirstAttachedObjectByAlias("Main") as MapObjectAttachedMesh;
            //foreach (Bone BN in mainMesh.MeshObject.Mesh.Skeleton.Bones)
            //{
            //    if (BN.Name == "ValveBiped_forward")
            //    {
            //        headbone = BN;
            //        break;
            //    }
            //}

            ////create Head Sphear
            //{
            //    Body head = PhysicsModel.CreateBody();
            //    head.Name = "HeadBody";
            //    head.Position = Position + new Vec3(0, 0, 2f);
            //    head.Rotation = Rotation;
            //    body.Sleepiness = 0;
            //    body.LinearVelocity = linearVelocityForSerialization;
            {
                SphereShape shape = body.CreateSphereShape();
                shape.Radius = 0.2f;
                shape.ContactGroup = (int)ContactGroup.Dynamic;
                shape.StaticFriction = 0;
                shape.DynamicFriction = 0;
                shape.Hardness = 0;
                shape.Name = "Head";
                if (Headhelper != null)
                {
                    shape.Position = Headhelper.PositionOffset;
                }

                //if (headbone != null)
                //{
                //    shape.Position = headbone.Position;
                //}
            }
            //}

            //create main capsule
            {
                CapsuleShape shape = body.CreateCapsuleShape();
                shape.Length = length;
                shape.Radius = Type.Radius;
                shape.ContactGroup = (int)ContactGroup.Dynamic;
                shape.StaticFriction = 0;
                shape.DynamicFriction = 0;
                //shape.Bounciness = 0;
                shape.Hardness = 0;
                float r = shape.Radius;
                shape.Density = Type.Mass / (MathFunctions.PI * r * r * shape.Length +
                    (4.0f / 3.0f) * MathFunctions.PI * r * r * r);
                shape.SpecialLiquidDensity = .5f;
                shape.Name = "main";
            }

            //create down capsule
            {
                CapsuleShape shape = body.CreateCapsuleShape();
                shape.Length = Type.Height - Type.BottomRadius * 2;
                shape.Radius = Type.BottomRadius;
                shape.Position = new Vec3(0, 0,
                    (Type.Height - Type.WalkUpHeight) / 2 - Type.Height / 2);
                shape.ContactGroup = (int)ContactGroup.Dynamic;
                //shape.Bounciness = 0;
                shape.Hardness = 0;
                shape.Density = 0;
                shape.SpecialLiquidDensity = .5f;
                shape.Name = "down";

                shape.StaticFriction = 0;
                shape.DynamicFriction = 0;
            }

            PhysicsModel.PushToWorld();

            AddTimer();
        }

        protected override void OnSuspendPhysicsDuringMapLoading(bool suspend)
        {
            base.OnSuspendPhysicsDuringMapLoading(suspend);

            //After loading a map, the physics simulate 5 seconds, that bodies have fallen asleep.
            //During this time we will disable physics for this entity.
            foreach (Body body in PhysicsModel.Bodies)
                body.Static = suspend;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            //we call this before OnTick for using old value of MainBody.LinearVelocity
            CalculateGroundRelativeVelocity();

            base.OnTick();

            TickMovement();

            if (Intellect != null)
                TickIntellect(Intellect);

            UpdateRotation();
            TickJump(false);

            if (IsOnGround())
                onGroundTime += TickDelta;
            else
                onGroundTime = 0;

            if (forceMoveVectorTimer != 0)
                forceMoveVectorTimer--;
        }

        private int playedright = 0;
        private int playedleft = 0;
        private float duplicatestoper;
        private bool bduplicatestoperbool;

        private void Footsteps()
        {
            if (string.IsNullOrEmpty(Type.SoundFoot1))
                return;

            if (mainBody == null)
                return;

            if (mainBody.Sleeping)
                return;

            float rayLength = .7f;

            leftFoot.onGround = false;
            rightFoot.onGround = false;

            MapObjectAttachedHelper Leftfoot = GetFirstAttachedObjectByAlias("leftfoot") as MapObjectAttachedHelper;
            MapObjectAttachedHelper Rightfoot = GetFirstAttachedObjectByAlias("rightfoot") as MapObjectAttachedHelper;

            if (Leftfoot == null || Rightfoot == null) return;

            Vec3 pos;
            Quat rot;
            Vec3 scl;
            Vec3 downDirection = mainBody.Rotation * new Vec3(0, 0, -rayLength);

            //leftfoot
            {
                Leftfoot.GetGlobalTransform(out pos, out rot, out scl);

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
                    leftFoot.onGround = true;
                }
            }
            //right foot
            {
                Rightfoot.GetGlobalTransform(out pos, out rot, out scl);

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
                    rightFoot.onGround = true;
                }
            }

            if (rightFoot.onGround == false) playedright = 0;
            if (rightFoot.onGround == true && playedright == 0)
            {
                playedright = 1;
                if (PlayFootSound())
                {
                    bduplicatestoperbool = true;
                }
            }
            if (leftFoot.onGround == false) playedleft = 0;
            if (leftFoot.onGround == true && playedleft == 0)
            {
                playedleft = 1;
                if (PlayFootSound())
                {
                    bduplicatestoperbool = true;
                }
            }

            if (bduplicatestoperbool)
            {
                duplicatestoper += TickDelta;

                if (duplicatestoper > 0.3f)
                {
                    bduplicatestoperbool = false;
                    duplicatestoper = 0;
                }
            }
        }

        private string Soundfoot;

        private bool PlayFootSound()
        {
            if (bduplicatestoperbool)
                return false;

            //Foot Sound Randomiser
            float Randomsoundfoot = World.Instance.Random.NextFloat() * 6;

            if (Randomsoundfoot > 0f && Randomsoundfoot < 1f)
            {
                Soundfoot = Type.SoundFoot1;
            }
            else if (Randomsoundfoot >= 1f && Randomsoundfoot <= 2f)
            {
                Soundfoot = Type.SoundFoot2;
            }
            else if (Randomsoundfoot >= 2f && Randomsoundfoot <= 3f)
            {
                Soundfoot = Type.SoundFoot3;
            }
            else if (Randomsoundfoot >= 3f && Randomsoundfoot <= 4f)
            {
                Soundfoot = Type.SoundFoot4;
            }
            else if (Randomsoundfoot >= 5f && Randomsoundfoot <= 6f)
            {
                Soundfoot = Type.SoundFoot5;
            }
            if (!string.IsNullOrEmpty(Soundfoot))
            {
                SoundPlay3D(Soundfoot, .4f, true);
                return true;
            }
            else
                return false;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.Client_OnTick()"/>.</summary>
        protected override void Client_OnTick()
        {
            //we call this before OnTick for using old value of MainBody.LinearVelocity
            CalculateGroundRelativeVelocity();

            base.Client_OnTick();

            //foot
            Footsteps();

            Vec3 shouldAddForce;
            CalculateMainBodyGroundDistanceAndGroundBody(out shouldAddForce);

            if (IsOnGround())
                onGroundTime += TickDelta;
            else
                onGroundTime = 0;
        }

        public bool Z = true;
        public bool croutch = false;
        public bool Run = false;

        private void TickIntellect(Intellect intellect)
        {
            Vec2 forceVec = Vec2.Zero;

            if (forceMoveVectorTimer != 0)
            {
                forceVec = forceMoveVector;
            }
            else
            {
                Vec2 vec = Vec2.Zero;

                vec.X += intellect.GetControlKeyStrength(GameControlKeys.Forward);
                vec.X -= intellect.GetControlKeyStrength(GameControlKeys.Backward);
                vec.Y += intellect.GetControlKeyStrength(GameControlKeys.Left);
                vec.Y -= intellect.GetControlKeyStrength(GameControlKeys.Right);

                forceVec = (new Vec3(vec.X, vec.Y, 0) * Rotation).ToVec2();

                if (forceVec != Vec2.Zero)
                {
                    float length = forceVec.Length();
                    if (length > 1)
                        forceVec /= length;
                }
            }

            if (forceVec != Vec2.Zero)
            {
                float velocityCoefficient = 1;
                if (FastMoveInfluence != null)
                    velocityCoefficient = FastMoveInfluence.Type.Coefficient;

                float maxVelocity;
                float force;

                if (IsOnGround())
                {
                    maxVelocity = Type.WalkMaxVelocity;
                    force = Type.WalkForce;
                }
                else
                {
                    maxVelocity = Type.FlyControlMaxVelocity;
                    force = Type.FlyControlForce;
                }

                maxVelocity *= forceVec.Length();

                //velocityCoefficient
                maxVelocity *= velocityCoefficient;
                force *= velocityCoefficient;

                if (mainBody.LinearVelocity.Length() < maxVelocity)
                    mainBody.AddForce(ForceType.Global, 0, new Vec3(forceVec.X, forceVec.Y, 0) *
                        force * TickDelta, Vec3.Zero);
            }

            if (!Z) return;

            CapsuleShape maincap = mainBody.Shapes[1] as CapsuleShape;
            CapsuleShape downcap = mainBody.Shapes[2] as CapsuleShape;

            if (Intellect.IsControlKeyPressed(GameControlKeys.Croutch))
            {
                croutch = true;
                Type.Height = Type.HeightMax / 3;
            }
            else if (Type.FPSCameraOffset != new Vec3(0, 0, -0.5f))
            {
                Type.Height = Type.HeightMax;
                croutch = false;
            }
            float length2 = Type.Height - Type.Radius * 2 - Type.WalkUpHeight;
            if (croutch)
            {
                Type.FPSCameraOffset = Vec3.Zero;
                Type.WalkMaxVelocity = 3;
                Type.WalkForce = 3000f;
                maincap.Position = new Vec3(0, 0, -0.5f);
                maincap.Length = length2 / 2;

                downcap.Position = new Vec3(0, 0, -0.5f);
                downcap.Length = 1.2f;
            }
            else if (!croutch && Type.FPSCameraOffset != new Vec3(0, 0, -0.5f))
            {
                Type.WalkMaxVelocity = 7;
                Type.WalkForce = 6000f;
                Type.FPSCameraOffset = new Vec3(0, 0, 0.5f);
                maincap.Length = length2;
                maincap.Position = Vec3.Zero;

                downcap.Length = Type.Height - Type.BottomRadius * 2;
                downcap.Position = new Vec3(0, 0,
                    (Type.Height - Type.WalkUpHeight) / 2 - Type.Height / 2);
            }
            TickRun();
            lastTickForceVector = forceVec;

            //Update Run energy
            if (EntitySystemWorld.Instance.IsServer() &&
              Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendUpdateRunEnergyToClients(
                            EntitySystemWorld.Instance.RemoteEntityWorlds);
            }
        }

        private void TickRun()
        {
            if (!Z || croutch) return;
            if (Intellect.IsControlKeyPressed(GameControlKeys.Shift) && Z && !croutch)
            {
                if (RunEnergy >= 5)
                {
                    Run = true;
                    Type.WalkMaxVelocity = 20;
                    Type.WalkForce = 15000f;
                    RunEnergy -= 5;
                }
            }
            else
            {
                Run = false;
                Type.WalkMaxVelocity = 7;
                Type.WalkForce = 6000f;
                if (RunEnergy < Type.RunEnergyMax)
                {
                    RunEnergy += 1;
                }
            }
        }

        protected override void OnIntellectCommand(Intellect.Command command)
        {
            base.OnIntellectCommand(command);

            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                if (command.KeyPressed)
                {
                    if (command.Key == GameControlKeys.Jump)
                    {
                        TryJump();
                    }
                    if (command.Key == GameControlKeys.Z)
                    {
                        TryZ();
                    }
                }
            }
        }

        private void TryZ()
        {
            if (croutch) return;
            CapsuleShape maincap = mainBody.Shapes[1] as CapsuleShape;
            CapsuleShape downcap = mainBody.Shapes[2] as CapsuleShape;

            if (Type.FPSCameraOffset != new Vec3(0, 0, -0.5f))
            {
                Z = false;
            }
            else
            {
                Z = true;
            }

            if (!Z)
            {
                Type.Height = Type.HeightMax / 3;
            }
            else
            {
                Type.Height = Type.HeightMax;
            }

            float length2 = Type.Height - Type.Radius * 2 - Type.WalkUpHeight;
            if (!Z)
            {
                Type.WalkMaxVelocity = 2;
                Type.WalkForce = 2000f;

                Type.FPSCameraOffset = new Vec3(0, 0, -0.5f);

                maincap.Position = new Vec3(0, 0, -0.5f);
                maincap.Length = length2 / 2;

                downcap.Position = new Vec3(0, 0, -0.5f);
                downcap.Length = 1.2f;
            }
            if (Z)
            {
                Type.WalkMaxVelocity = 7;
                Type.WalkForce = 6000f;

                Type.FPSCameraOffset = new Vec3(0, 0, 0.5f);
                maincap.Length = length2;
                maincap.Position = Vec3.Zero;

                downcap.Length = Type.Height - Type.BottomRadius * 2;
                downcap.Position = new Vec3(0, 0,
                    (Type.Height - Type.WalkUpHeight) / 2 - Type.Height / 2);
            }
        }

        //small distinction of different physics libraries.
        private static bool physicsLibraryDetected;

        private static bool isPhysX;

        private bool IsPhysX()
        {
            if (!physicsLibraryDetected)
            {
                isPhysX = PhysicsWorld.Instance.DriverName.Contains("PhysX");
                physicsLibraryDetected = true;
            }
            return isPhysX;
        }

        private void UpdateMainBodyDamping()
        {
            if (IsOnGround() && jumpInactiveTime == 0)
            {
                //small distinction of different physics libraries.
                if (IsPhysX())
                    mainBody.LinearDamping = 9.3f;
                else
                    mainBody.LinearDamping = 10;
            }
            else
                mainBody.LinearDamping = .15f;
        }

        private void TickMovement()
        {
            if (groundBody != null && groundBody.IsDisposed)
                groundBody = null;

            if (mainBody.Sleeping && groundBody != null && !groundBody.Sleeping &&
                (groundBody.LinearVelocity.LengthSqr() > .3f ||
                groundBody.AngularVelocity.LengthSqr() > .3f))
            {
                mainBody.Sleeping = false;
            }

            if (mainBody.Sleeping && IsOnGround())
                return;

            UpdateMainBodyDamping();

            if (IsOnGround())
            {
                mainBody.AngularVelocity = Vec3.Zero;

                if (mainBodyGroundDistance + .05f < Type.FloorHeight && jumpInactiveTime == 0)
                {
                    noWakeBodies = true;
                    Position = Position + new Vec3(0, 0, Type.FloorHeight - mainBodyGroundDistance);
                    noWakeBodies = false;
                }
            }

            Vec3 shouldAddForce;
            CalculateMainBodyGroundDistanceAndGroundBody(out shouldAddForce);

            //add force to body if need
            if (shouldAddForce != Vec3.Zero)
            {
                mainBody.AddForce(ForceType.GlobalAtLocalPos, TickDelta, shouldAddForce,
                    Vec3.Zero);
            }

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

            if (IsOnGround())
            {
                bool groundStopped = groundBody.Sleeping ||
                    (groundBody.LinearVelocity.LengthSqr() < .3f &&
                    groundBody.AngularVelocity.LengthSqr() < .3f);

                if (groundStopped && mainBody.LinearVelocity.LengthSqr() < 1.0f)
                    needSleep = true;
            }

            mainBody.Sleeping = needSleep;
        }

        private void CalculateMainBodyGroundDistanceAndGroundBody(out Vec3 shouldAddForce)
        {
            shouldAddForce = Vec3.Zero;

            mainBodyGroundDistance = 1000;
            groundBody = null;

            for (int n = 0; n < 5; n++)
            {
                Vec3 offset = Vec3.Zero;

                float step = Type.BottomRadius;

                switch (n)
                {
                    case 0: offset = new Vec3(0, 0, 0); break;
                    case 1: offset = new Vec3(-step, -step, step/* * .1f*/ ); break;
                    case 2: offset = new Vec3(step, -step, step/* * .1f*/ ); break;
                    case 3: offset = new Vec3(step, step, step/* * .1f*/ ); break;
                    case 4: offset = new Vec3(-step, step, step/* * .1f*/ ); break;
                }

                Vec3 pos = Position - new Vec3(0, 0, Type.FloorHeight -
                    Type.WalkUpHeight + .01f) + offset;
                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    new Ray(pos, new Vec3(0, 0, -Type.Height * 1.5f)),
                    (int)mainBody.Shapes[1].ContactGroup);

                if (piercingResult.Length == 0)
                    continue;

                foreach (RayCastResult result in piercingResult)
                {
                    if (result.Shape.Body == mainBody)
                        continue;

                    float dist = Position.Z - result.Position.Z;
                    if (dist < mainBodyGroundDistance)
                    {
                        bool bigSlope = false;

                        //max slope check
                        const float maxSlopeCoef = .7f;// MathFunctions.Sin( new Degree( 60.0f ).InRadians() );
                        if (result.Normal.Z < maxSlopeCoef)
                        {
                            Vec3 vector = new Vec3(result.Normal.X, result.Normal.Y, 0);
                            if (vector != Vec3.Zero)
                            {
                                bigSlope = true;

                                //add force
                                vector.Normalize();
                                vector *= mainBody.Mass * 2;
                                shouldAddForce += vector;
                            }
                        }

                        if (!bigSlope)
                        {
                            mainBodyGroundDistance = dist;
                            groundBody = result.Shape.Body;
                        }
                    }
                }
            }
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

                vel.Z = Type.JumpVelocity;
                mainBody.LinearVelocity = vel;
                Position += new Vec3(0, 0, .05f);

                jumpInactiveTime = .2f;
                shouldJumpTime = 0;

                UpdateMainBodyDamping();

                OnJump();

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendJumpEventToAllClients();
            }
        }

        public void TryJump()
        {
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
            base.OnSetTransform(ref pos, ref rot, ref scl);

            if (PhysicsModel != null && !noWakeBodies)
            {
                foreach (Body body in PhysicsModel.Bodies)
                    body.Sleeping = false;
            }
        }

        private void CalculateGroundRelativeVelocity()
        {
            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                //server or single mode
                if (IsOnGround() && groundBody.AngularVelocity.LengthSqr() < .3f)
                    groundRelativeVelocity = mainBody.LinearVelocity - groundBody.LinearVelocity;
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
        }

        [Browsable(false)]
        public Vec3 GroundRelativeVelocity
        {
            get { return groundRelativeVelocity; }
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
        private void Client_ReceiveWeaponVerticalAngle(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 value = reader.ReadVec3();
            if (!reader.Complete())
                return;
            groundRelativeVelocity = value;
        }

        /////////////////////Run/////////////////////////
        private int server_runenergy = 0;

        private void Server_SendUpdateRunEnergyToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            if (Math.Abs(RunEnergy - server_runenergy) > 3)
            {
                SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Character),
                    (ushort)NetworkMessages.UpdateRunEnergyToClient);

                writer.WriteVariableInt32(RunEnergy);
                EndNetworkMessage();

                server_runenergy = RunEnergy;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateRunEnergyToClient)]
        private void Client_ReceiveUpdateRunEnergy(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;

            RunEnergy = value;
        }
    }
}