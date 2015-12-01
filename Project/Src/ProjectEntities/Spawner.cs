// Copyright (C) 2006-2007 NeoAxis Group
using Engine.EntitySystem;
using Engine.MathEx;

namespace ProjectEntities
{
    public class SpawnerType : DynamicType
    {
    }

    /// <summary>
    /// Most basic spawner. mostly for use with the tech lab
    /// </summary>
    public class Spawner : Dynamic
    {
        public delegate void OnUnitSpawned(Unit unit);

        public event OnUnitSpawned UnitSpawned;

        private SpawnerType _type = null; public new SpawnerType Type { get { return _type; } }

        protected Unit spawned;

        public Unit Spawned
        {
            get { return spawned; }
            set { spawned = value; }
        }

        protected UnitType selectedUnit;

        //if spawned == null, we spawn a new unit immediately. if there is a spawned unit
        //we destroy it and defer the spawning of the new unit until that process has
        //completed by listening to the units destroyed event
        public void SpawnUnit(UnitType unit)
        {
            if (HasSpawnedUnit())
            {
                selectedUnit = unit;
                spawned.Destroying += new DestroyingDelegate(spawned_Destroying);
                spawned.SetForDeletion(false);
                spawned = null;
            }
            else
            {
                CreateUnit(unit);
            }
        }

        private void spawned_Destroying(Entity entity)
        {
            CreateUnit(selectedUnit);
        }

        public Unit CreateUnit(UnitType unit)
        {
            Unit newUnit = (Unit)Entities.Instance.Create(unit, Parent);

            newUnit.Position = Position + new Vec3(0, 0, unit.SpawnHeight);
            newUnit.Rotation = Rotation;

            spawned = newUnit;
            newUnit.PostCreate();
            selectedUnit = null;
            if (UnitSpawned != null)
                UnitSpawned(newUnit);

            return newUnit;
        }

        protected bool HasSpawnedUnit()
        {
            if (spawned != null && !spawned.Died)
                return true;
            else
                return false;
        }
    }
}