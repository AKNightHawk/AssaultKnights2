// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="Unit"/> entity type.
    /// </summary>
    public abstract class UnitType : DynamicType
    {
        [FieldSerialize]
        private bool allowPlayerControl;

        [FieldSerialize]
        private AIType initialAI;

        [FieldSerialize]
        private float viewRadius;

        [FieldSerialize]
        private Vec3 fpsCameraOffset;

        [FieldSerialize]
        private float takeItemsRadius = 2;

        /// <summary>
        /// Gets or sets a value which indicates, whether the unit can be controlled by the player.
        /// </summary>
        [Description("A value which indicates, whether the unit can be controlled by the player.")]
        [DefaultValue(false)]
        public bool AllowPlayerControl
        {
            get { return allowPlayerControl; }
            set { allowPlayerControl = value; }
        }

        //iNCIN -- AK Mod
        [FieldSerialize]
        private float spawnHeight;

        [Description("For Spawner")]
        [DefaultValue(20.0f)]
        public float SpawnHeight
        {
            get { return spawnHeight; }
            set { spawnHeight = value; }
        }

        /// <summary>
        /// Gets or sets a artificial intellect which will be appointed to a unit at its creation.
        /// </summary>
        [Description("Artificial intellect which will be appointed to a unit at its creation.")]
        public AIType InitialAI
        {
            get { return initialAI; }
            set { initialAI = value; }
        }

        /// <summary>
        /// Gets or sets the radius of visibility of a unit.
        /// </summary>
        [Description("The radius of visibility of a unit.")]
        [DefaultValue(10)]
        public float ViewRadius
        {
            get { return viewRadius; }
            set { viewRadius = value; }
        }

        /// <summary>
        /// Gets or sets the camera offset which will be considered at a FPS mode.
        /// </summary>
        [Description("The camera offset which will be considered at a FPS mode.")]
        [DefaultValue(typeof(Vec3), "0 0 0")]
        public Vec3 FPSCameraOffset
        {
            get { return fpsCameraOffset; }
            set { fpsCameraOffset = value; }
        }

        [DefaultValue(2.0f)]
        public float TakeItemsRadius
        {
            get { return takeItemsRadius; }
            set { takeItemsRadius = value; }
        }

        private static int defaultassaultcredits = 1000;

        [FieldSerialize]
        private int assaultcredits = defaultassaultcredits;

        [Description("Amount of credits given to player on killing objects")]
        [DefaultValue(1000)]
        public int AssaultCredits
        {
            get { return assaultcredits; }
            set
            {
                assaultcredits = value;
                PlayerManager.Instance.server_shouldUpdateDataToClients = true;
            }
        }

        //for hit points etc
        [FieldSerialize]
        private int pointsForHit = 50;

        public int PointsForHit
        {
            get { return pointsForHit; }
            set { pointsForHit = value; }
        }

        [FieldSerialize]
        private int pointsForKill = 100;

        public int PointsForKill
        {
            get { return pointsForKill; }
            set { pointsForKill = value; }
        }

        [FieldSerialize]
        private float AkunitRadarDistanceMax = 500.0f;

        [Description("How far Unit picks targets and Radar screen radius")]
        [DefaultValue(500.0f)]
        public float AKunitRadarDistanceMax
        {
            get { return AkunitRadarDistanceMax; }
            set { AkunitRadarDistanceMax = value; }
        }
    }

    /// <summary>
    /// Units differ from <see cref="Dynamic"/> objects that that can be controlled by
    /// intellect (<see cref="ProjectEntities.Intellect"/>).
    /// </summary>
    public abstract class Unit : Dynamic
    {
        [FieldSerialize]
        private Intellect intellect;

        [FieldSerialize]
        private bool intellectShouldDeleteAfterDetach;

        [FieldSerialize]
        private AIType initialAI;

        //This faction is set when initialAI is established
        [FieldSerialize]
        private FactionType initialFaction;

        private float takeItemsTimer;

        //influences. only for optimization
        [FieldSerialize]
        private FastMoveInfluence fastMoveInfluence;

        [FieldSerialize]
        private FreezeMoveInfluence freezeMoveInfluence;

        [FieldSerialize]
        private FastAttackInfluence fastAttackInfluence;

        [FieldSerialize]
        private BigDamageInfluence bigDamageInfluence;

        [FieldSerialize]
        private DamageInfluence damageInfluence;

        private float viewRadius;

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            IntellectToClient,
        }

        ///////////////////////////////////////////

        private UnitType _type = null; public new UnitType Type { get { return _type; } }

        public Unit()
        {
            takeItemsTimer = World.Instance.Random.NextFloat();
            SceneGraphGroup = MapObjectSceneGraphGroups.UnitGroup;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnLoad(TextBlock)"/>.</summary>
        protected override bool OnLoad(TextBlock block)
        {
            //for compatibility with old versions.
            if (block.IsAttributeExist("initFaction"))
                initialFaction = (FactionType)EntityTypes.Instance.GetByName(
                    block.GetAttribute("initFaction"));

            return base.OnLoad(block);
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            CreateInitialAI(loaded);
            SubscribeToTickEvent();

            viewRadius = Type.ViewRadius;
        }

        protected override void OnRenderFrame()
        {
            base.OnRenderFrame();

            if (!EntitySystemWorld.Instance.IsEditor())
            {
                if (intellect != null)
                    intellect.DoControlledObjectRenderFrame();
            }
        }

        /// <summary>Overridden from <see cref="Engine.MapSystem.MapObject.OnRender(Camera)"/>.</summary>
        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if (!EntitySystemWorld.Instance.IsEditor())
            {
                if (intellect != null)
                    intellect.DoControlledObjectRender(camera);
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            //take items
            if (Intellect != null && Intellect.AllowTakeItems)
                TickTakeItems();
        }

        private void TickTakeItems()
        {
            takeItemsTimer -= TickDelta;
            if (takeItemsTimer <= 0)
            {
                takeItemsTimer += .25f;

                float radius = Type.TakeItemsRadius;

                Map.Instance.GetObjects(new Sphere(Position, radius), delegate(MapObject obj)
                {
                    Item item = obj as Item;
                    if (item == null)
                        return;

                    //if( ( item.Position - Position ).LengthFast() > radius )
                    if ((item.Position - Position).LengthSqr() > radius * radius)
                        return;

                    item.Take(this);
                });
            }
        }

        protected override void OnCreateInfluence(Influence influence)
        {
            base.OnCreateInfluence(influence);

            if (influence is FastMoveInfluence)
                fastMoveInfluence = (FastMoveInfluence)influence;
            else if (influence is FreezeMoveInfluence)//Incin
                freezeMoveInfluence = (FreezeMoveInfluence)influence;
            else if (influence is FastAttackInfluence)
                fastAttackInfluence = (FastAttackInfluence)influence;
            else if (influence is BigDamageInfluence)
                bigDamageInfluence = (BigDamageInfluence)influence;
            else if (influence is DamageInfluence)
                damageInfluence = (DamageInfluence)influence;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnRemoveChild(Entity)"/></summary>
        protected override void OnRemoveChild(Entity entity)
        {
            base.OnRemoveChild(entity);

            if (fastMoveInfluence == entity)
                fastMoveInfluence = null;
            else if (freezeMoveInfluence == entity)//Incin
                freezeMoveInfluence = null;
            else if (fastAttackInfluence == entity)
                fastAttackInfluence = null;
            else if (bigDamageInfluence == entity)
                bigDamageInfluence = null;
            else if (damageInfluence == entity)
                damageInfluence = null;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDeleteSubscribedToDeletionEvent(Entity)"/></summary>
        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);

            if (intellect == entity)
                intellect = null;
        }

        private Intellect GetPlayerIntellect()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance;
        }

        protected override void OnDamage(MapObject prejudicial,
            Vec3 pos, Shape shape, float damage, bool allowMoveDamageToParent)
        {
            base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);

            if ((EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle()) &&
                PlayerManager.Instance != null)
            {
                Unit sourceUnit = null;
                int hitPointMultiplier = 1;
                {
                    Bullet bullet = prejudicial as Bullet;
                    if (bullet != null)
                    {
                        sourceUnit = bullet.SourceUnit;
                        //reward bullet hits with extra points. explsions are for lamers
                        hitPointMultiplier = bullet.Type.HitPointMultiplier;
                    }
                    Explosion explosion = prejudicial as Explosion;
                    if (explosion != null)
                        sourceUnit = explosion.SourceUnit;
                }

                if (sourceUnit != null)
                {
                    if (sourceUnit.Intellect != null)
                    {
                        PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                            ServerOrSingle_GetPlayer(sourceUnit.Intellect);
                        if (player != null)
                            player.HitPoints += (Type.PointsForHit * hitPointMultiplier);
                    }
                }
            }
        }

        protected override void OnDie(MapObject prejudicial)
        {
            base.OnDie(prejudicial);

            //frag counter (for network games).
            if ((EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle()) &&
                PlayerManager.Instance != null)
            {
                Unit sourceUnit = null;
                int killPointMultiplier = 1;
                int assaultCreditMultiplier = 1;
                {
                    Bullet bullet = prejudicial as Bullet;
                    if (bullet != null)
                    {
                        sourceUnit = bullet.SourceUnit;
                        //again, bullet kills get the goodies. explosions don't require skill, just luck
                        killPointMultiplier = bullet.Type.KillPointMultiplier;
                        assaultCreditMultiplier = bullet.Type.AssaultCreditMultiplier;
                    }
                    Explosion explosion = prejudicial as Explosion;
                    if (explosion != null)
                        sourceUnit = explosion.SourceUnit;
                }

                if (sourceUnit != null)
                {
                    if (sourceUnit.Intellect != null)
                    {
                        PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                            ServerOrSingle_GetPlayer(sourceUnit.Intellect);
                        if (player != null && GameMap.Instance.GameType != GameMap.GameTypes.AssaultKnights)
                        {
                            player.Frags++;
                        }
                        else if (player != null && GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                        {
                            player.KillPoints += (Type.PointsForKill * killPointMultiplier);
                            player.AssaultCredits += ((Type.AssaultCredits * assaultCreditMultiplier) * player.KillPoints);
                        }
                        else
                        {
                            Log.Info("wtf");
                        }
                    }
                }
                else if (GameMap.Instance.GameType != GameMap.GameTypes.AssaultKnights)
                {
                    if (Intellect != null)
                    {
                        PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                            ServerOrSingle_GetPlayer(Intellect);
                        if (player != null)
                            player.Frags--;
                    }
                }
                else if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                {
                    if (Intellect != null)
                    {
                        PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                            ServerOrSingle_GetPlayer(Intellect);
                        if (player != null)
                            player.Frags--;
                    }
                }
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            SetIntellect(null, false);
            base.OnDestroy();
        }

        [Browsable(false)]
        [LogicSystemBrowsable(true)]
        public Intellect Intellect
        {
            get { return intellect; }
        }

        public void SetIntellect(Intellect value, bool shouldDeleteAfterDetach)
        {
            Intellect oldIntellect = intellect;
            bool oldIntellectShouldDeleteAfterDetach = intellectShouldDeleteAfterDetach;

            if (intellect != null)
                UnsubscribeToDeletionEvent(intellect);

            intellect = value;
            intellectShouldDeleteAfterDetach = shouldDeleteAfterDetach;

            if (intellect != null)
                SubscribeToDeletionEvent(intellect);

            if (oldIntellect != null && oldIntellectShouldDeleteAfterDetach)
                oldIntellect.SetForDeletion(true);

            //send update to clients
            if (EntitySystemWorld.Instance.IsServer())
                Server_SendIntellectToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
        }

        private void CreateInitialAI(bool loaded)
        {
            if (EntitySystemWorld.Instance.IsEditor())
                return;
            if (loaded && EntitySystemWorld.Instance.SerializationMode == SerializationModes.World)
                return;

            if (Intellect != null)
                return;

            AIType initAI = InitialAI;
            if (initAI == null)
                initAI = Type.InitialAI;

            if (initAI == null)
                return;

            if (EntitySystemWorld.Instance.IsDedicatedServer())
            {
                if (initAI.NetworkType == EntityNetworkTypes.ClientOnly)
                    return;
            }
            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                if (initAI.NetworkType == EntityNetworkTypes.ServerOnly)
                    return;
                if (initAI.NetworkType == EntityNetworkTypes.Synchronized)
                    return;
            }

            //create intellect
            Intellect i = (Intellect)Entities.Instance.Create(initAI, World.Instance);
            i.Faction = InitialFaction;
            i.ControlledObject = this;
            i.PostCreate();
            SetIntellect(i, true);
        }

        protected virtual void OnIntellectCommand(Intellect.Command command)
        {
        }

        public void DoIntellectCommand(Intellect.Command command)
        {
            OnIntellectCommand(command);
        }

        public FactionType GetRootUnitFaction()
        {
            Unit unit = this;
            while (true)
            {
                Intellect objIntellect = unit.Intellect;
                if (objIntellect != null && objIntellect.Faction != null)
                    return objIntellect.Faction;

                Unit obj = unit.AttachedMapObjectParent as Unit;
                if (obj == null)
                    return null;
                unit = obj;
            }
        }

        [Browsable(false)]
        public FastMoveInfluence FastMoveInfluence { get { return fastMoveInfluence; } }

        [Browsable(false)] //Incin
        public FreezeMoveInfluence FreezeMoveInfluence { get { return freezeMoveInfluence; } }

        [Browsable(false)]
        public FastAttackInfluence FastAttackInfluence { get { return fastAttackInfluence; } }

        [Browsable(false)]
        public BigDamageInfluence BigDamageInfluence { get { return bigDamageInfluence; } }

        [Browsable(false)]
        public DamageInfluence DamageInfluence { get { return damageInfluence; } }

        [LogicSystemBrowsable(true)]
        public virtual FactionType InitialFaction
        {
            get { return initialFaction; }
            set { initialFaction = value; }
        }

        [Browsable(false)]
        public float ViewRadius
        {
            get { return viewRadius; }
            set { viewRadius = value; }
        }

        /// <summary>
        /// Gets or sets a artificial intelligence which will be appointed to a unit at its creation.
        /// </summary>
        [Description("Artificial intelligence which will be appointed to a unit at its creation.")]
        public AIType InitialAI
        {
            get { return initialAI; }
            set { initialAI = value; }
        }

        protected override void Server_OnClientConnectedAfterPostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            Server_SendIntellectToClients(new RemoteEntityWorld[] { remoteEntityWorld });
        }

        private void Server_SendIntellectToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Unit),
                (ushort)NetworkMessages.IntellectToClient);
            //zero will sent if intellect NetworkType != Synchronized
            writer.WriteVariableUInt32(Intellect != null ? Intellect.NetworkUIN : (uint)0);
            writer.Write(intellectShouldDeleteAfterDetach);
            if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
            {
                string s = InitialFaction != null ? InitialFaction.Name : string.Empty;
                writer.Write(s);
            }
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.IntellectToClient)]
        private void Client_ReceiveIntellect(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            uint networkUIN = reader.ReadVariableUInt32();
            bool shouldDeleteAfterDetach = reader.ReadBoolean();
            FactionType ft = null;

            if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
            {
                string factionName = reader.ReadString();
                if (factionName != string.Empty)
                    ft = (FactionType)EntityTypes.Instance.GetByName(factionName);
            }
            if (!reader.Complete())
                return;

            Intellect i = null;
            if (networkUIN != 0)
            {
                i = (Intellect)Entities.Instance.GetByNetworkUIN(networkUIN);
                if (ft != null)
                    i.Faction = ft;
            }
            SetIntellect(i, shouldDeleteAfterDetach);
        }

        //the unit mech is aiming at
        private Unit currentTarget;

        private Unit reticuleTarget;

        [Browsable(false)]
        public Unit CurrentReticuleTarget
        {
            get { return reticuleTarget; }
            set { reticuleTarget = value; }
        }

        [Browsable(false)]
        public Unit CurrentMissileTarget
        {
            get { return currentTarget; }
            set { currentTarget = value; }
        }

        private List<Unit> visibleUnits;

        [Browsable(false)]
        public List<Unit> VisibleUnits
        {
            get { return visibleUnits; }
            set { visibleUnits = value; }
        }

        public virtual void GetFirstPersonCameraPosition(out Vec3 position, out Vec3 forward, out Vec3 up)
        {
            position = GetInterpolatedPosition() + Type.FPSCameraOffset * GetInterpolatedRotation();
            forward = Vec3.XAxis;
            up = Vec3.ZAxis;
        }

        //public global::Game.GameWindow.CameraType GetCamera() //was AKGameWindow.
        //{
        //    throw new NotImplementedException();
        //}
    }
}