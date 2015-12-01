using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    public class DamagerBallType : AKunitType
    {
        //need to roll the ball
        //need to rotate the model with the camera direction using current velocity so it stays straight forward and on center(sp)(tank wheel)
        //need to add input controls and make the ball roll
        //need direction forces
        //The code I hope to set up will determine which key is being pressed..
        //front back left right etc and camera hopefully into account, and use the opposite for dropping items quadrant based,
        //so you dont run over your own turret or bomb you dropped
        //turrets need a time to deploy -- game window
        //turrets can be destroyed after a 10 second delay -- game window
        //turrets can be stolen at 7 seconds , if stolen you lose that item -- game window

        //Enum NamedSkill
        //{
        //    NOSKILL = -1, //CHEAT CODE
        //    HOTBALL = 1, //HOTBALL
        //    TORNADO = 2, //TORNADO
        //    TURRET = 4, //INVISIBLE LAND MINE
        //    ALL = 256,

        //}
    }

    public class DamagerBall : AKunit
    {
        private static float commondefault = 0.5f;

        [FieldSerialize]
        private float ballradius = commondefault;

        [DefaultValue(0.5f)]
        public float BallRadius
        {
            get { return ballradius; }
            set { ballradius = value; }
        }

        [FieldSerialize]
        private float fwdspeed;

        [DefaultValue(420000.0f)]
        public float SpeedForwardBack
        {
            get { return fwdspeed; }
            set { fwdspeed = value; }
        }

        //[FieldSerialize]
        //float bckspeed;

        //[DefaultValue(1.0f)]
        //public float SpeedBackward
        //{
        //    get { return bckspeed; }
        //    set { bckspeed = value; }
        //}

        [FieldSerialize]
        private float strafespeed;

        [DefaultValue(420000.0f)]
        public float SpeedStrafe
        {
            get { return strafespeed; }
            set { strafespeed = value; }
        }

        [FieldSerialize]
        private float maxspeed;

        [DefaultValue(840000f)]
        public float MaxSpeed
        {
            get { return maxspeed; }
            set { maxspeed = value; }
        }

        [FieldSerialize]
        private float speedmultiplier;

        [DefaultValue(50.0f)]
        public float SpeedMultiplier
        {
            get { return speedmultiplier; }
            set { speedmultiplier = value; }
        }

        //physics shape properties
        [FieldSerialize]
        private float bounciness = commondefault;

        [DefaultValue(0.5f)]
        public float PhysicsBounciness
        {
            get { return bounciness; }
            set { bounciness = value; }
        }

        //physics shape properties
        [FieldSerialize]
        private float density = commondefault;

        [DefaultValue(0.5f)]
        public float PhysicsDensity
        {
            get { return density; }
            set { density = value; }
        }

        //physics shape properties
        [FieldSerialize]
        private float dynamicfriction = commondefault;

        [DefaultValue(0.5f)]
        public float PhysicsDynamicFriction
        {
            get { return dynamicfriction; }
            set { dynamicfriction = value; }
        }

        //physics shape properties
        [FieldSerialize]
        private float staticfriction = commondefault;

        [DefaultValue(0.5f)]
        public float PhysicsStaticFriction
        {
            get { return staticfriction; }
            set { staticfriction = value; }
        }

        //physics shape properties
        [FieldSerialize]
        private float liquiddensity;

        [DefaultValue(2f)]
        public float PhysicsLiquidDensity
        {
            get { return liquiddensity; }
            set { liquiddensity = value; }
        }

        private static float grav = -9.87f;

        [FieldSerialize]
        private float localgravity = grav;

        [DefaultValue(-9.87f)]
        public float PhysicsGravity
        {
            get { return localgravity; }
            set { localgravity = value; }
        }

        ///////////////////////////////////////////

        //[FieldSerialize]
        //Vec3 notAnimatedWeaponAttachPosition;

        //[FieldSerialize]
        //List<WeaponItem> weapons = new List<WeaponItem>();

        //public class WeaponItem
        //{
        //    [FieldSerialize]
        //    WeaponType weaponType;

        //    public WeaponType WeaponType
        //    {
        //        get { return weaponType; }
        //        set { weaponType = value; }
        //    }

        //    public override string ToString()
        //    {
        //        if (weaponType == null)
        //            return "(not initialized)";
        //        return weaponType.Name;
        //    }
        //}

        ///////////////////////////////////////////
        //[DefaultValue(typeof(Vec3), "0 0 1.5")]
        //public Vec3 NotAnimatedWeaponAttachPosition
        //{
        //    get { return notAnimatedWeaponAttachPosition; }
        //    set { notAnimatedWeaponAttachPosition = value; }
        //}

        //public List<WeaponItem> Weapons
        //{
        //    get { return weapons; }
        //}
        //-----------------------------------------------------------------------//
        private Body Damager_Ball;

        private Body Damager_Ball_Player;
        //static Vec2 movementvector = Vec2.Zero; //movement vector passed from gamewindow

        //Incin -- saves previous so no wiping on player death
        private Turret turret;

        private Vec3 turnToPosition;
        private Radian horizontalDirectionForUpdateRotation;

        //static List<Turret> turrets = new List<Turret>();

        private static int activeturrets = 0;  //monitor turret count
        //static float TURRETMAXOUT = 10f; //max time before "killing" the turret
        //static float turretTimer = 0f; //can add remove after 10 seconds
        //static int activeMaxTurrets = 1; //Max Turrets out

        //static bool Hotball; //toggles
        //static Sphere hotballsphere; //uses powerupeffect to use features for damage area of effect

        //static int activeSkill;
        //[FieldSerialize]
        //float velocity = 0;

        //[FieldSerialize]
        //float defaultmaxvelocity = ;

        [FieldSerialize]
        private float maxvelocity = 420000;

        [FieldSerialize]
        private float contusionTimeRemaining;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        private Vec3 linearVelocityForSerialization;

        private Vec3 groundRelativeVelocity;
        private Vec3 server_sentGroundRelativeVelocity;

        [Browsable(false)]
        public Vec3 GroundRelativeVelocity
        {
            get { return groundRelativeVelocity; }
            set { groundRelativeVelocity = value; }
        }

        [Browsable(false)]
        public float AirVelocity
        {
            get { return airvelocity; }
            set { airvelocity = value; }
        }

        [Browsable(false)]
        public float MaxVelocity
        {
            get { return maxvelocity; }
            set { maxvelocity = value; }
        }

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            GroundRelativeVelocityToClient,
            ContusionTimeRemainingToClient,
        }

        ///////////////////////////////////////////
        private DamagerBallType _type = null; public new DamagerBallType Type { get { return _type; } }

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

            //UpdateRotation();
        }

        //bowling ball == = "90=-\\\||///-=90"
        public void UpdateRotation()
        {
            float halfAngle = horizontalDirectionForUpdateRotation * .5f;
            Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)), MathFunctions.Cos(halfAngle));
            //noWakeBodies = true;
            Rotation = rot;
            //noWakeBodies = false;

            //bool updateOldRotation = true;

            //no update OldRotation for TPSArcade demo and for PlatformerDemo
            //if (Intellect != null && PlayerIntellect.Instance == Intellect)
            //{
            //    if (GameMap.Instance != null && (
            //        GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
            //        GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo))
            //    {
            //        updateOldRotation = false;
            //    }
            //}

            //if (updateOldRotation)
            //	OldRotation = rot;
        }

        protected override void OnSave(TextBlock block)
        {
            if (Damager_Ball_Player != null)
                linearVelocityForSerialization = Damager_Ball_Player.LinearVelocity;

            base.OnSave(block);
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SetTurnToPosition(Position + Rotation.GetForward() * 100);
            if (PhysicsModel != null)
            {
                PhysicsModel.PopFromWorld();

                Damager_Ball = PhysicsModel.GetBody("Damager_Ball");
                Damager_Ball_Player = PhysicsModel.GetBody("CT");

                //if (Damager_Ball_Player != null)
                //{
                //    //this.Type.BallRadius = .5f;//((Damager_Ball_Player.Shapes[0].Volume * 3f) / (4f * (float)Math.PI) * .333333f);
                //    foreach (Body body in PhysicsModel.Bodies)
                //        body.Collision += new Body.CollisionDelegate(body_Collision);
                //}

                if (!EntitySystemWorld.Instance.IsEditor())
                {
                    Damager_Ball_Player.Shapes[0].Density = PhysicsDensity;
                    Damager_Ball_Player.Shapes[0].DynamicFriction = PhysicsDynamicFriction;
                    Damager_Ball_Player.Shapes[0].Restitution = PhysicsBounciness; //Bounciness
                    Damager_Ball_Player.Shapes[0].StaticFriction = PhysicsStaticFriction;
                    Damager_Ball_Player.Shapes[0].SpecialLiquidDensity = PhysicsLiquidDensity;
                    Damager_Ball_Player.Shapes[0].ContactGroup = (int)ContactGroup.Dynamic;
                    Damager_Ball_Player.EnableGravity = true;
                    Damager_Ball_Player.LinearVelocity = linearVelocityForSerialization;
                }

                if (!EntitySystemWorld.Instance.IsEditor())
                {
                    if (Damager_Ball_Player == null)
                        Log.Error("Damager_Ball.type: \"Damager_Ball\" body does not exist. The Physics Body is Named incorrectly?");
                }
                SubscribeToTickEvent();
                CheckPhysicsProperties();
                PhysicsModel.PushToWorld();
            }

            //Incin
            //if activeMaxTurrets turret was out from before RemoveMiniTurret it
            //if (turret != null)
            //{
            //    turret.SetForDeletion(false);
            //    turretTimer = 0; //reset remove timer
            //    activeturrets = 0;  //reset turret counts
            //}
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate2(bool)"/>.</summary>
        protected override void OnPostCreate2(bool loaded)
        {
            base.OnPostCreate2(loaded);
        }

        private void DoForce(Body otherbody, Body thisbody)
        {
            float velocity = 0;
            //if(MaxVelocity <= Velocity)

            //if(MaxVelocity <= Velocity) //)this.GroundRelativeVelocity.Length())
            //{
            velocity = this.GroundRelativeVelocity.Length() + 1000f; //(speedpersecpersec)
            //}
            //else
            //{
            //    if (velocity >= MaxVelocity)
            //        velocity = MaxVelocity;
            //    else if (Velocity < 1)
            //    {
            //        velocity = 0f;
            //    }

            //}

            Vec3 vector = otherbody.Position - thisbody.Position;
            //vector.Z += 3f;
            vector.Normalize();
            otherbody.AddForce(ForceType.GlobalAtLocalPos, TickDelta, vector * (velocity * 2), otherbody.Position);
            //thisbody.AddForce(ForceType.GlobalAtGlobalPos, TickDelta, -vector * velocity, thisbody.Position);
        }

        private void body_Collision(ref CollisionEvent collisionEvent)
        {
            Body otherBody = collisionEvent.OtherShape.Body;

            if (HeightmapTerrain.GetTerrainByBody(otherBody) != null)
                return;

            if (otherBody.Static && otherBody.Name.Contains("Map")) //LDASH custom map names
                return;

            //Note: Incin ---- Dynamic_Collision needs to be removed

            Body thisBody = collisionEvent.ThisShape.Body;

            //if (!otherBody.Static)
            //    DoForce(thisBody);

            if (otherBody == null && thisBody != null)
                DoForce(null, thisBody);

            if (!otherBody.Static)
                DoForce(otherBody, thisBody); //collisionEvent.Position, collisionEvent.Normal);

            float otherMass = otherBody.Mass;
            float impulse = 0;
            impulse += thisBody.LastStepLinearVelocity.Length() * thisBody.Mass;

            if (otherMass != 0)
                impulse += otherBody.LastStepLinearVelocity.Length() * otherMass;

            float damage = impulse; // *Type.ImpulseDamageCoefficient;

            MapObject mapobj = MapSystemWorld.GetMapObjectByBody(otherBody);

            if (mapobj != null)
            {
                Dynamic obj = mapobj as Dynamic;
                if (obj != null)
                {
                    if (obj.Name.Contains("House"))
                    {
                        //damage house
                        if (obj.Type.ImpulseDamageCoefficient != 0)
                        {
                            damage = impulse * obj.Type.ImpulseDamageCoefficient;
                        }
                        else
                        {
                            float health = obj.Health / 2;
                            damage = health;
                        }
                        OnDamage(mapobj, collisionEvent.Position, collisionEvent.OtherShape, damage, true);

                        //damage player if too fast
                        if (Type.ImpulseDamageCoefficient != 0)
                            damage = impulse * Type.ImpulseDamageCoefficient;
                        else
                            damage = impulse;

                        //if minimal damage do player damage
                        ////if (damage >= Type.ImpulseMinimalDamage)
                        ////{
                        ////    //OnDamage(null, collisionEvent.Position, collisionEvent.ThisShape, damage, true);//damage the other guy here
                        ////    OnDamage(null, collisionEvent.Position, collisionEvent.ThisShape, damage, true);
                        ////}
                    }
                    else //still object type damage
                    {
                        if (obj.Type.ImpulseDamageCoefficient != 0)
                            damage = impulse * obj.Type.ImpulseDamageCoefficient;
                        else
                            damage = impulse * 0.5f;

                        OnDamage(mapobj, collisionEvent.Position, collisionEvent.OtherShape, damage, true);
                    }
                }
            }
            //else  //damage self
            //{
            //    //if (Type.ImpulseDamageCoefficient != 0)
            //    //    damage = impulse * Type.ImpulseDamageCoefficient;
            //    //else
            //    //    damage = impulse;

            //    //if (damage >= Type.ImpulseMinimalDamage)
            //    //{
            //    //    OnDamage(null, collisionEvent.Position, collisionEvent.ThisShape, damage, true);
            //    //}
            //}
        }

        protected override void OnSuspendPhysicsDuringMapLoading(bool suspend)
        {
            base.OnSuspendPhysicsDuringMapLoading(suspend);

            //After loading a map, the physics simulate 5 seconds, that bodies have fallen asleep.
            //During this time we will disable physics for this entity.

            foreach (Body body in PhysicsModel.Bodies)
                body.Static = suspend;
        }

        protected void CheckPhysicsProperties()
        {
            if (Damager_Ball_Player != null)
            {
                PhysicsModel.PopFromWorld();
                Damager_Ball_Player.Shapes[0].Density = PhysicsDensity;
                Damager_Ball_Player.Shapes[0].DynamicFriction = PhysicsDynamicFriction;
                Damager_Ball_Player.Shapes[0].Restitution = PhysicsBounciness;//Bounciness
                Damager_Ball_Player.Shapes[0].StaticFriction = PhysicsStaticFriction;
                Damager_Ball_Player.Shapes[0].SpecialLiquidDensity = PhysicsLiquidDensity;
                Damager_Ball_Player.Shapes[0].ContactGroup = (int)ContactGroup.Dynamic;
                Damager_Ball_Player.EnableGravity = true;

                PhysicsModel.PushToWorld();
                //GameEngineApp.Instance.AddScreenMessage("Physics Updated");
            }
        }

        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            base.OnSetTransform(ref pos, ref rot, ref scl);

            //if (PhysicsModel != null)
            //{
            //    foreach (Body body in PhysicsModel.Bodies)
            //        body.Sleeping = false;
            //}
        }

        private void CalculateRelativeVelocity()
        {
            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                //server or single mode
                //if (Damager_Ball_Player.AngularVelocity.LengthSqr() < .3f)
                groundRelativeVelocity = Damager_Ball_Player.LinearVelocity; // -Damager_Ball_Player.LastStepLinearVelocity;
                //else
                //    groundRelativeVelocity = Vec3.Zero;

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

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            CalculateRelativeVelocity();

            base.OnTick();

            TickContusionTime();

            //if (Damager_Ball_Player != null && Damager_Ball_Player.LastStepLinearVelocity != null)
            //     RelativeVelocity += Damager_Ball_Player.LastStepLinearVelocity / skilllevel;
            //else
            //     RelativeVelocity = currentvelocity + (forward(strength)) + (right(strength));

            float speed = GroundRelativeVelocity.Length();

            if (speed >= 1)
                speed = 1f;

            if (Damager_Ball_Player != null)
                Damager_Ball_Player.AddForce(ForceType.Global, TickDelta, new Vec3(0, 0, speed *= PhysicsGravity), Position);

            TickMovement();

            if (Intellect != null)
                TickIntellect(Intellect);

            //UpdateRotation();
            //TickJump(false);

            //if (IsOnGround())
            //    onGroundTime += TickDelta;
            //else
            //    onGroundTime = 0;

            //if (forceMoveVectorTimer != 0)
            //    forceMoveVectorTimer--;

            //if (turret != null)
            //    turretTimer += TickDelta; //can add remove after 10 seconds
        }

        private void TickMovement()
        {
            //not needed
            //GetMovementByControlKeys();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.Client_OnTick()"/>.</summary>
        protected override void Client_OnTick()
        {
            //we call this before OnTick for using old value of MainBody.LinearVelocity
            CalculateRelativeVelocity();
            base.Client_OnTick();
            TickContusionTime();

            float speed = this.GroundRelativeVelocity.Length();
            if (speed <= 1)
                speed = 1f;
            else if (speed < .01)
                speed = 0f;

            Damager_Ball_Player.AddForce(ForceType.Global, TickDelta, new Vec3(0, 0, speed *= PhysicsGravity), Position);

            TickMovement();

            if (Intellect != null)
                TickIntellect(Intellect);

            //UpdateRotation();
            //TickJump(false);

            //if (IsOnGround())
            //    onGroundTime += TickDelta;
            //else
            //    onGroundTime = 0;

            //if (forceMoveVectorTimer != 0)
            //    forceMoveVectorTimer--;

            //if (turret != null)
            //    turretTimer += TickDelta; //can add remove after 10 seconds
        }

        private void TickContusionTime()
        {
            if (contusionTimeRemaining != 0)
            {
                contusionTimeRemaining -= TickDelta;
                if (contusionTimeRemaining < 0)
                    contusionTimeRemaining = 0;
            }
        }

        public float ContusionTimeRemaining
        {
            get { return contusionTimeRemaining; }
            set
            {
                if (contusionTimeRemaining == value)
                    return;

                contusionTimeRemaining = value;

                if (EntitySystemWorld.Instance.IsServer())
                {
                    Server_SendContusionTimeRemainingToClients(
                        EntitySystemWorld.Instance.RemoteEntityWorlds);
                }
            }
        }

        private Vec2 GetMovementByControlKeys()
        {
            Vec2 localVector = Vec2.Zero;

            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsDedicatedServer())
            {
                PlayerManager.ServerOrSingle_Player player = null;
                player = PlayerManager.Instance.ServerOrSingle_GetPlayer(Intellect);
                if (GameMap.Instance != null && player.Intellect == Intellect) //PlayerIntellect.Instance == Intellect)
                {
                    localVector.X -= Intellect.GetControlKeyStrength(GameControlKeys.Forward);// +this.Type.SpeedForward;
                    localVector.X += Intellect.GetControlKeyStrength(GameControlKeys.Backward);// +this.Type.SpeedBackward;
                    localVector.Y -= Intellect.GetControlKeyStrength(GameControlKeys.Left);// +this.Type.SpeedLeft;
                    localVector.Y += Intellect.GetControlKeyStrength(GameControlKeys.Right);// +this.Type.SpeedRight;

                    if (localVector != Vec2.Zero)
                    {
                        Vec2 diff = Position.ToVec2() - TurnToPosition.ToVec2(); ;// *Rotation.GetForward().ToVec2();//RendererWorld.Instance.DefaultCamera.Position.ToVec2();
                        Degree angle = new Radian(MathFunctions.ATan(diff.Y, diff.X));
                        Degree vecAngle = new Radian(MathFunctions.ATan(-localVector.Y, localVector.X));
                        Quat rot = new Angles(0, 0, vecAngle - angle).ToQuat();
                        Vec2 vector = (rot * new Vec3(1, 0, 0)).ToVec2();
                        return vector;
                    }
                    else
                        return Vec2.Zero;
                }
                return Vec2.Zero;
            }
            else if (EntitySystemWorld.Instance.IsSingle())
            {
                if (GameMap.Instance != null && PlayerIntellect.Instance == Intellect)
                {
                    localVector.X -= Intellect.GetControlKeyStrength(GameControlKeys.Forward);// +this.Type.SpeedForward;
                    localVector.X += Intellect.GetControlKeyStrength(GameControlKeys.Backward);// +this.Type.SpeedBackward;
                    localVector.Y -= Intellect.GetControlKeyStrength(GameControlKeys.Left);// +this.Type.SpeedLeft;
                    localVector.Y += Intellect.GetControlKeyStrength(GameControlKeys.Right);// +this.Type.SpeedRight;

                    if (localVector != Vec2.Zero)
                    {
                        Vec2 diff = Position.ToVec2() - TurnToPosition.ToVec2(); ;// *Rotation.GetForward().ToVec2();//RendererWorld.Instance.DefaultCamera.Position.ToVec2();
                        Degree angle = new Radian(MathFunctions.ATan(diff.Y, diff.X));
                        Degree vecAngle = new Radian(MathFunctions.ATan(-localVector.Y, localVector.X));
                        Quat rot = new Angles(0, 0, vecAngle - angle).ToQuat();
                        Vec2 vector = (rot * new Vec3(1, 0, 0)).ToVec2();
                        return vector;
                    }
                    else
                        return Vec2.Zero;
                }
                return Vec2.Zero;
            }
            return Vec2.Zero;
            ////    if (localVector != Vec2.Zero)
            ////    {
            ////        localVector.Normalize();

            ////        //calculate force vector with considering camera orientation
            ////        Vec2 diff = Position.ToVec2() - RendererWorld.Instance.DefaultCamera.Position.ToVec2();
            ////        //new Vec3( RendererWorld.Instance.DefaultCamera.Position.X,RendererWorld.Instance.DefaultCamera.Position.Y, Damager_Ball_Player.Position.Z);
            ////        //diff.Z += this.Type.BallRadius;

            ////        Degree angle = new Radian(MathFunctions.ATan(diff.Y, diff.X));
            ////        Degree vecAngle = new Radian(MathFunctions.ATan(-vec.Y, vec.X));
            ////        Quat rot = new Angles(0, 0, vecAngle - angle).ToQuat();

            ////        Vec2 forceVector2 = (rot * new Vec3(1, 0, 0)).ToVec2();

            ////        Vec2 forceVector = new Vec2(forceVector2.X, forceVector2.Y);

            ////        return forceVector;
            ////    }
            ////    else return vec;
            ////}
            ////else
            ////    return vec;
        }

        private void TickIntellect(Intellect intellect)
        {
            Vec2 forceVec = GetMovementByControlKeys();
            if (forceVec != Vec2.Zero)
            {
                float speedCoefficient = 1;
                if (FastMoveInfluence != null)
                    speedCoefficient = FastMoveInfluence.Type.Coefficient;

                float force = 0f;

                Vec2 localVec = (new Vec3(forceVec.X, forceVec.Y, 0) * Rotation.GetInverse()).ToVec2();

                float absSum = Math.Abs(localVec.X) + Math.Abs(localVec.Y);
                if (absSum > 1)
                    localVec /= absSum;

                if (Math.Abs(localVec.X) >= .001f)
                {
                    //forward and backward
                    force += SpeedForwardBack * SpeedMultiplier * Math.Abs(localVec.X) * speedCoefficient;
                }

                if (Math.Abs(localVec.Y) >= .001f)
                {
                    //left and right
                    force += SpeedStrafe * SpeedMultiplier * Math.Abs(localVec.Y) * speedCoefficient;
                }

                //speedCoefficient
                //*= speedCoefficient;
                force *= speedCoefficient;
                Vec3 forcesoffset = new Vec3(forceVec.X, forceVec.Y, PhysicsGravity * TickDelta) * force * TickDelta;
                //forcesoffset.Z += .5f;
                if (Damager_Ball_Player.LinearVelocity.Length() < MaxSpeed)
                {
                    Damager_Ball_Player.AddForce(ForceType.Global, 0, forcesoffset, Vec3.Zero);
                }
                //else
                //{
                //     Damager_Ball_Player.AddForce(ForceType.Global, 0, new Vec3(0, 0, Type.PhysicsGravity ) * TickDelta, Vec3.Zero);
                //}
            }

            //lastTickForceVector = forceVec;
        }

        protected override void OnIntellectCommand(Intellect.Command command)
        {
            base.OnIntellectCommand(command);
            /*
                        if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
                        {
                            //if (command.KeyPressed)
                            //{
                            //    //add special key commands here like jump or turret loading
                            //    if (command.Key == GameControlKeys.UpdatePhysics)
                            //    {
                            //        CheckPhysicsProperties();
                            //    }

                            //    if (command.Key == GameControlKeys.SetMiniTurret)
                            //    {
                            //        if (activeturrets < activeMaxTurrets)
                            //        {
                            //            activeturrets++;
                            //            SetMiniTurret(this as Unit);
                            //        }
                            //    }

                            //    if (command.Key == GameControlKeys.RemoveMiniTurret)
                            //    {
                            //        if (activeturrets <= activeMaxTurrets && turretTimer >= TURRETMAXOUT)
                            //        {
                            //            RemoveMiniTurret(this as Unit);
                            //            turretTimer = 0f; //reset timer
                            //        }
                            //    }
                            //}
                        }
             */
        }

        //void RemoveMiniTurret(Unit player)
        //{
        //    if (player == null)
        //    {
        //        return;
        //    }

        //    if (turret != null)
        //    {
        //        //turret.UserData = null;
        //        turret.SetForDeletion(false); //SetShouldDelete();
        //        activeturrets = 0;
        //        turretTimer = 0;
        //    }
        //}

        private MapObjectAttachedMesh GetAliasMesh(Unit player, int selecteditem)
        {
            foreach (MapObjectAttachedMesh obj in player.AttachedObjects)
            {
                if (obj as MapObjectAttachedMesh == null)
                    continue;

                if (obj.Alias == "BallGlow" && selecteditem == 1)
                {
                    return obj;
                }
                else if (obj.Alias == "MainBall" && selecteditem == 2)
                {
                    return obj;
                }
                //else
                //    return null;
            }
            return null;
        }

        private MapObjectAttachedObject GetAliasPoint(Unit player, int position)
        {
            if (player == null || (position > 7 || position < 1))
                return null;

            foreach (MapObjectAttachedObject obj in player.AttachedObjects)
            {
                if (obj as MapObjectAttachedHelper == null)
                    continue;

                if (obj.Alias == "MainBall" && position == 6)
                    return obj;
                else if (obj.Alias == "MountLeft" && position == 1)
                    return obj;
                else if (obj.Alias == "MountRight" && position == 2)
                    return obj;
                else if (obj.Alias == "MountRear" && position == 3)
                    return obj;
                else if (obj.Alias == "MountFront" && position == 4)
                    return obj;
                else if (obj.Alias == "BallGlow" && position == 5)
                    return obj;
            }
            return null;
        }

        private MapObjectAttachedObject GetRandomAliasPoint(Unit player)
        {
            int position = (int)World.Instance.Random.NextFloat() * 4;

            if (position == 0)
                position = 1;

            foreach (MapObjectAttachedObject obj in player.AttachedObjects)
            {
                if (obj as MapObjectAttachedHelper == null)
                    continue;

                if (obj.Alias == "MountLeft" && position == 1)
                    return obj;
                else if (obj.Alias == "MountRight" && position == 2)
                    return obj;
                else if (obj.Alias == "MountRear" && position == 3)
                    return obj;
                else if (obj.Alias == "MountFront" && position == 4)
                    return obj;
            }
            return null;
        }

        private void SetMiniTurret(Unit player)
        {
            if (player == null)
                return;

            Vec3 location = new Vec3(1.5f, 1.5f, .5f); //set default above to start of player ball

            if (activeturrets == 1)
            {
                //Get Current position
                //Spawner spawner = new Spawner();

                //GameEntities.TurretAI turretAi = new GameEntities.TurretAI();
                //turretAi.unitWeapons
                turret = (Turret)Entities.Instance.Create("TurretScaled", Map.Instance);
                turret.Position = this.Position + location;
                turret.ViewRadius = turret.Type.ViewRadius;
                turret.Health = turret.Type.HealthMax;

                //if(player != null)
                //	turret.UserData = (Object)player;

                //turret.Armor = turret.Type.ArmorMax;
                turret.SubscribeToDeletionEvent((Entity)turret);
                //turret.InitialAI = turret.Type.InitialAI;
                //turret.InitialFaction = player.InitialFaction; //(FactionType)EntityTypes.Instance.GetByName("BadFaction");
                turret.PostCreate();

                //PhysicsModel.PopFromWorld();

                //////bool foundspot = true; //trick to make it reloop for next for loop

                //////for (int i = 1; i < 5; i++) //Change this for more alias MapObjectAttachedHelper points
                //////{
                //////    MapObjectAttachedObject helperpoint = GetAliasPoint(player, i);

                //////    if (helperpoint == null)
                //////        return;

                //////    location = Position + helperpoint.PositionOffset;

                //////    foreach (Body body in turret.PhysicsModel.Bodies)
                //////    {
                //////        if (foundspot == false) //flagging back to true to break foreach loop
                //////        {
                //////            //this should goto for loop
                //////            foundspot = true;
                //////            break;
                //////        }

                //////        //look down for a clear spot if none found break from for each
                //////        RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(new Ray( location, new Vec3(0, 0, -100)),
                //////        (int)body.Shapes[0].ContactGroup);

                //////        //find anything?
                //////        if (piercingResult.Length == 0)
                //////            continue;

                //////        foreach (RayCastResult result in piercingResult)
                //////        {
                //////            if (result.Shape.Body == body) //if self physics body
                //////                continue;

                //////            //check if throwing inside a mapobjects walls I am trying to do?
                //////            Bounds bounds = result.Shape.GetGlobalBounds().Intersect(body.Shapes[0].GetGlobalBounds());
                //////            if (bounds!= null &&
                //////                //body.GetGlobalBounds().IsContainsPoint(result.Shape.GetGlobalBounds().GetSize()))
                //////                //!result.Shape.GetGlobalBounds().IsContainsPoint(body.GetGlobalBounds().GetSize()))
                //////            {
                //////                if (result.Shape.ShapeType == Shape.Type.HeightField)
                //////                {
                //////                    turret.Position = new Vec3(location.X, location.Y, result.Position.Z + 1f); //1f is offset from floor
                //////                    //positioned = true;
                //////                    return;
                //////                }
                //////                else if (result.Shape.ShapeType == Shape.Type.Mesh || result.Shape.Body.Static)
                //////                {
                //////                    turret.Position = new Vec3(location.X, location.Y, result.Position.Z + 1f); //1f is offset from floor
                //////                    //positioned = true;
                //////                    return;
                //////                }
                //////                else
                //////                {
                //////                    foundspot = false;
                //////                    break;
                //////                }
                //////            }
                //////            else
                //////            {
                //////                //bad position
                //////                foundspot = false;
                //////                break;
                //////            }

                //////            //if (result.Shape.ShapeType == Shape.Type.HeightField)
                //////            //    turret.Position = new Vec3(location.X, location.Y, result.Position.Z + 1f); //1f is offset from floor
                //////            // {
                //////            //   //positioned = true;
                //////            //    return;
                //////            //}

                //////        }
                //////    }
                //////}

                foreach (Body body in turret.PhysicsModel.Bodies)
                {
                    RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(new Ray(this.Position + location, new Vec3(0, 0, -100)),
                     (int)body.Shapes[0].ContactGroup); //use only "base"

                    if (piercingResult.Length == 0)
                        continue;

                    foreach (RayCastResult result in piercingResult)
                    {
                        if (result.Shape.Body == body) //if self physics body
                            continue;

                        Bounds bounds = result.Shape.GetGlobalBounds().Intersect(body.Shapes[0].GetGlobalBounds());

                        if (bounds != null && result.Shape.Name.Contains("Map"))//.ShapeType != Shape.Type.HeightField)
                        {
                            turret.Position = this.Position + location;
                            return;
                        }
                        else if (result.Shape.ShapeType == Shape.Type.HeightField && result.Shape.Position != Vec3.Zero)
                        {
                            turret.Position = this.Position + location;//new Vec3(turret.Position.X, turret.Position.Y, result.Position.Z + 1f) + location; //1f is offset from floor
                            return;
                        }
                        else if (result.Distance <= 2f)
                        {
                            turret.Position = this.Position + location;
                            return;
                        }
                        else
                        {
                            // turret.Position = new Vec3(turret.Position.X, turret.Position.Y, turret.Position.Z) + location;
                        }
                    }
                }

                //PhysicsModel.PushToWorld();
            }
            else
            {
                return;
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            if (PhysicsModel != null)
            {
                foreach (Body body in PhysicsModel.Bodies)
                    body.Collision -= new Body.CollisionDelegate(body_Collision);
            }

            //if (turret != null)
            //    turret.SetDeleted();

            base.OnDestroy();
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
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(DamagerBall),
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

        private void Server_SendContusionTimeRemainingToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(DamagerBall),
                (ushort)NetworkMessages.ContusionTimeRemainingToClient);
            writer.Write(contusionTimeRemaining);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ContusionTimeRemainingToClient)]
        private void Client_ReceiveContusionTimeRemaining(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            float value = reader.ReadSingle();
            if (!reader.Complete())
                return;
            ContusionTimeRemaining = value;
        }

        //This method is called when the entity receives damages
        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Shape shape, float damage, bool allowMoveDamageToParent)
        {
            base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);

            if (Damager_Ball_Player != null)
            {
            }
        }

        public float airvelocity { get; set; }

        //public float airmaxvelocity;{ get; set;}
    }
}