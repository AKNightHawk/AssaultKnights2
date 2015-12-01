// Copyright (C) 2006-2007 NeoAxis Group
using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using ProjectEntities;

namespace GameEntities.Community
{
    /// <summary>
    /// Defines the <see cref="SelectiveSpawnPoint"/> entity type.
    /// </summary>
    public class SelectiveSpawnPointType : MapObjectType
    {
    }

    public class SelectiveSpawnPoint : MapObject
    {
        [FieldSerialize]
        private AIType aiType;

        [FieldSerialize]
        private FactionType faction;

        [FieldSerialize]
        private UnitType spawnedUnit;

        [FieldSerialize]
        private int spawnCount = 1;

        [FieldSerialize]
        private float spawnTime = 20;

        [FieldSerialize]
        private float spawnRadius = 10;

        [FieldSerialize]
        private float triggerRadius = 10;

        /// <summary>
        /// Counter for remaining time.
        /// </summary>
        private float spawnCounter;

        /// <summary>
        /// The number of entities left to spawn.
        /// </summary>
        private int popAmount;

        private SelectiveSpawnPointType _type = null; public new SelectiveSpawnPointType Type { get { return _type; } }

        [Description("The default AI for the spawned units.")]
        [DefaultValue(null)]
        public AIType AIType
        {
            get { return aiType; }
            set { aiType = value; }
        }

        [Description("The initial faction, or null for neutral.")]
        [DefaultValue(null)]
        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        [Description("The number of units that will be created.")]
        [DefaultValue(1)]
        public int SpawnCount
        {
            get { return spawnCount; }
            set { spawnCount = value; }
        }

        [Description("Time in seconds between spawns.")]
        [DefaultValue(20.0f)]
        public float SpawnTime
        {
            get { return spawnTime; }
            set { spawnTime = value; }
        }

        [Description("Spawn radius that has to be free of other units before spawning a new one.")]
        [DefaultValue(10.0f)]
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set { spawnRadius = value; }
        }

        [Description("The type of Unit to spawn.")]
        [DefaultValue(null)]
        public UnitType SpawnUnit
        {
            get { return spawnedUnit; }
            set { spawnedUnit = value; }
        }

        [Description("The radius to check for the player in before spawing any entities.")]
        [DefaultValueAttribute(10.0f)]
        public float TriggerRadius
        {
            get { return triggerRadius; }
            set { triggerRadius = value; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            spawnCounter = 0.0f;
            SubscribeToTickEvent();
        }

        protected override void OnTick()
        {
            base.OnTick();
            spawnCounter += TickDelta;
            if (spawnCounter >= SpawnTime) //time to start spawning
            {
                spawnCounter = 0.0f;
                if (!isSpawnPositionFree() || !isCloseToPoint())
                    return;
                popAmount++;
                if (popAmount <= SpawnCount)
                {
                    Unit i = (Unit)Entities.Instance.Create(SpawnUnit, Parent);
                    if (AIType != null)
                        i.InitialAI = AIType;
                    if (i == null)
                        return;
                    i.Position = FindFreePositionForUnit(i, Position);
                    i.Rotation = Rotation;
                    if (Faction != null)
                        i.InitialFaction = Faction;
                    i.PostCreate();
                }
            }
        }

        private Vec3 FindFreePositionForUnit(Unit unit, Vec3 center)
        {
            Vec3 volumeSize = unit.MapBounds.GetSize() + new Vec3(2, 2, 0);
            for (float zOffset = 0; true; zOffset += .3f)
            {
                for (float radius = 3; radius < 8; radius += .6f)
                {
                    for (float angle = 0; angle < MathFunctions.PI * 2; angle += MathFunctions.PI / 32)
                    {
                        Vec3 pos = center + new Vec3(MathFunctions.Cos(angle), MathFunctions.Sin(angle), 0) * radius + new Vec3(0, 0, zOffset);
                        Bounds volume = new Bounds(pos);
                        volume.Expand(volumeSize * .5f);
                        Body[] bodies = PhysicsWorld.Instance.VolumeCast(volume, (int)ContactGroup.CastOnlyContact);
                        if (bodies.Length == 0)
                            return pos;
                    }
                }
            }
        }

        private bool isCloseToPoint()
        {
            bool isPlayerClose = TriggerRadius <= 0;
            if (!isPlayerClose)
            {
                Map.Instance.GetObjects(new Sphere(Position, TriggerRadius), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                {
                    PlayerCharacter pchar = mapObject as PlayerCharacter;
                    if (pchar != null)
                        isPlayerClose = true;
                });
            }
            return isPlayerClose;
        }

        private bool isSpawnPositionFree()
        {
            bool isAreaFree = true;
            Map.Instance.GetObjects(new Sphere(Position, SpawnRadius), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
            {
                Unit unit = mapObject as Unit;
                //if there is at least one then we won't spawn
                if (unit != null)
                    isAreaFree = false;
            });

            return isAreaFree;
        }
    }
}