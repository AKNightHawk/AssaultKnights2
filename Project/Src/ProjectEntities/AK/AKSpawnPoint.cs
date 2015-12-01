
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.MathEx;
using Engine.Utils;
namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="SpawnPoint"/> entity type.
    /// </summary>
    public class AKSpawnPointType : SpawnPointType
    {
        public enum SpawnId
        {
            NONE = 0,
            AK1 = 1,
            AK2 = 2,
            AK3 = 4,
            AK4 = 8,
            AK5 = 16,
            AK6 = 32,
            AK7 = 64,
            AK8 = 128,
            OMNI1 = 256,
            OMNI2 = 512,
            OMNI3 = 1024,
            OMNI4 = 2048,
            OMNI5 = 4096,
            OMNI6 = 8192,
            OMNI7 = 16384,
            OMNI8 = 32568,
        }


        [FieldSerialize]
        FactionType faction;

        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }



        static SpawnId spid = SpawnId.NONE;

        [FieldSerialize]
        SpawnId Spawnid = spid;

        [DefaultValue(SpawnId.NONE)]
        public SpawnId SpawnID
        {
            get { return Spawnid; }
            set { Spawnid = value; }
        }

        [FieldSerialize]
        float respawntime;

        [DefaultValue((float)5)]
        public float RespawnTime
        {
            get { return respawntime; }
            set { respawntime = value; }
        }
    }

    public class AKSpawnPoint : SpawnPoint
    {
        static List<AKSpawnPoint> instances = new List<AKSpawnPoint>();

        public static AKSpawnPoint SelectedSinglePlayerPoint = null;

        AKSpawnPointType _type = null; public new AKSpawnPointType Type { get { return _type; } }

        public static List<AKSpawnPoint> Instances()
        {
            return instances;
        }


        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            instances.Add(this);
        }

        protected override void OnDestroy()
        {
            instances.Remove(this);
            base.OnDestroy();
        }


        public static AKSpawnPoint AKGetSpawnId(AKSpawnPointType.SpawnId spawnid)
        {
            foreach (AKSpawnPoint point in AKSpawnPoint.Instances())
            {
                if (spawnid != point.Type.SpawnID)
                    continue;
                return point;
            }
            return null;
        }

        public static AKSpawnPointType.SpawnId AKGetSpawnId(AKSpawnPoint spawnpoint)
        {
            return spawnpoint.Type.SpawnID;
        }

        public bool AKIsSpawnId(AKSpawnPoint spawnpoint, AKSpawnPointType.SpawnId id)
        {
            if (spawnpoint.Type.SpawnID == id)
                return true;
            else
                return false;
        }
        //Incin return spawn Id
        #region Incin

        public static AKSpawnPointType.SpawnId AKGetSpawnIdBySpawnIdwithFacton(AKSpawnPoint spawnpoint, FactionType faction)
        {

            foreach (AKSpawnPoint sp in instances)
            {
                if (sp.faction != faction)
                    continue;

                if (sp.Type.SpawnID != spawnpoint.Type.SpawnID)
                {
                    continue;
                }

                return spawnpoint.Type.SpawnID;

            }
            return AKSpawnPointType.SpawnId.NONE;
        }

        public static AKSpawnPoint AKGetSpawnPointById(AKSpawnPointType.SpawnId id)
        {

            foreach (AKSpawnPoint sp in instances)
            {
                if (sp.Type.SpawnID != id)
                    continue;
                else if (sp.Type.SpawnID == id)
                    return sp;
                else
                    return null;
            }

            return null;
        }
        #endregion Incin

        public static AKSpawnPoint AKGetFreeRandomSpawnPoint(FactionType faction)
        {
            foreach (AKSpawnPoint sp in instances)
            {
                if (sp.Type.Faction != faction)
                    continue;

                bool busy = false;
                {
                    Bounds volume = new Bounds(sp.Position);
                    volume.Expand(new Vec3(1, 1, 2));

                    Body[] result = PhysicsWorld.Instance.VolumeCast(volume, (int)ContactGroup.CastOnlyContact);

                    foreach (Body body in result)
                    {
                        if (body.Static)
                            continue;

                        foreach (Shape shape in body.Shapes)
                        {
                            if (PhysicsWorld.Instance.IsContactGroupsContactable(shape.ContactGroup, (int)ContactGroup.Dynamic))
                            {
                                busy = true;
                                break;
                            }
                        }
                        if (busy)
                            break;
                    }
                }

                if (!busy)
                    return sp;
            }

            return null;
        }
    }
}
