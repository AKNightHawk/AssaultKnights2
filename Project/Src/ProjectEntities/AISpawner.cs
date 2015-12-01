using System.Collections.Generic;
using Engine.EntitySystem;
using Engine.MathEx;

namespace ProjectEntities
{
    public class AISpawnerType : SpawnerType
    {
    }

    public class AISpawner : Spawner
    {
        public class AISpawnerItem
        {
            [FieldSerialize]
            private AIType aiType;

            [FieldSerialize]
            private UnitType unitType;

            public AIType AIType
            {
                get { return aiType; }
                set { aiType = value; }
            }

            public UnitType UnitType
            {
                get { return unitType; }
                set { unitType = value; }
            }
        }

        [FieldSerialize]
        private List<AISpawnerItem> spawnerItems = new List<AISpawnerItem>();

        public List<AISpawnerItem> SpawnerItems
        {
            get { return spawnerItems; }
        }

        [FieldSerialize]
        private FactionType faction;

        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        [FieldSerialize]
        private float spawnTime = 0;

        public float SpawnTime
        {
            get { return spawnTime; }
            set { spawnTime = value; }
        }

        private float spawnTimeElapsed = 0;

        private AISpawnerType _type = null;

        public new AISpawnerType Type
        {
            get { return _type; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (!EntitySystemWorld.Instance.IsEditor())
                SubscribeToTickEvent();
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (Spawned == null || !Spawned.IsSetForDeletion)
            {
                spawnTimeElapsed += TickDelta;

                if (spawnTimeElapsed < spawnTime)
                    return;

                spawnTimeElapsed = 0;

                int next = World.Instance.Random.Next(0, spawnerItems.Count);
                Unit newUnit = (Unit)Entities.Instance.Create(spawnerItems[next].UnitType, Parent);

                if (spawnerItems[next].AIType != null)
                    newUnit.InitialAI = spawnerItems[next].AIType;

                if (faction != null)
                    newUnit.InitialFaction = faction;

                newUnit.Position = Position + new Vec3(0, 0, spawnerItems[next].UnitType.SpawnHeight);
                newUnit.Rotation = Rotation;

                spawned = newUnit;
                newUnit.PostCreate();
                selectedUnit = null;
            }
        }
    }
}