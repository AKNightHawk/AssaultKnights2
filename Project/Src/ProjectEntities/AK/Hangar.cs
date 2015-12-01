using System;
using System.Collections.Generic;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Utils;

namespace GameEntities
{
    /// <summary>
    /// Summary of HangarType
    /// </summary>
    public class HangarType : AKMapObjectType
    {
    }

    /// <summary>
    /// Summary of Hangar
    /// </summary>
    public class Hangar : AKMapObject
    {
        public enum Hangar_Vehicle_Type
        {
            Mechs,
            GroundUnits,
            AirUnits,
            Jets,
        }

        [FieldSerialize]
        private List<Hangar_Vehicle_Type> vehicleType = new List<Hangar_Vehicle_Type>();

        public List<Hangar_Vehicle_Type> VehicleType
        {
            get { return vehicleType; }
            set { vehicleType = value; }
        }

        private List<Spawner> attachedSpawners = new List<Spawner>();

        //enum NetworkMessages
        //{
        //    SpawnRequestToServer,
        //    SupportedUnitsToClient,
        //}

        private HangarType _type = null;

        public new HangarType Type
        {
            get { return this._type; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (!EntitySystemWorld.Instance.IsEditor())
            {
                //get spawners for later use
                foreach (MapObjectAttachedObject ao in AttachedObjects)
                {
                    MapObjectAttachedMapObject amo = ao as MapObjectAttachedMapObject;

                    if (amo != null)
                    {
                        Spawner s = amo.MapObject as Spawner;

                        if (s != null)
                            attachedSpawners.Add(s);
                    }
                }
            }

            AddTimer();
        }

        protected override void OnTick()
        {
            base.OnTick();
        }

        public Spawner FindFirstFreeSpawner()
        {
            float blockedCheckRadius = 3;
            foreach (Spawner s in attachedSpawners)
            {
                bool busy = false;
                {
                    Bounds volume = new Bounds(s.Position);
                    volume.Expand(new Vec3(
                        blockedCheckRadius, blockedCheckRadius, blockedCheckRadius * 2));

                    Body[] result = PhysicsWorld.Instance.VolumeCast(volume,
                        (int)ContactGroup.CastOnlyContact);

                    foreach (Body body in result)
                    {
                        if (body.Static)
                            continue;

                        foreach (Shape shape in body.Shapes)
                        {
                            if (PhysicsWorld.Instance.IsContactGroupsContactable(shape.ContactGroup,
                                (int)ContactGroup.Dynamic))
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
                    return s;
            }

            return null;
        }

        public void SpawnNewUnit(UnitType unitType, int[] variant)
        {
            Spawner s = FindFirstFreeSpawner();
            /*AKunit newUnit = (AKunit)Entities.Instance.Create(unitType, Map.Instance);

            newUnit.Position = s.Position + new Vec3(0, 0, unitType.SpawnHight);
            newUnit.Rotation = s.Rotation;

            newUnit.PostCreate();*/

            AKunit newUnit = s.CreateUnit(unitType) as AKunit;

            if (variant != null)
            {
                if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
                    newUnit.Server_SetVariant(variant);
            }
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            Server_SendSupportedUnitsToClient(worlds);
        }

        public void Client_SendSpawnRequestToServer(string unitTypeName, string variant)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Hangar),
                (ushort)NetworkMessages.SpawnRequestToServer);

            writer.Write(unitTypeName);
            writer.Write(variant);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.SpawnRequestToServer)]
        private void Server_ReceiveSpawnRequest(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string unitTypeName = reader.ReadString();
            string variantText = reader.ReadString();

            if (!reader.Complete())
                return;

            UnitType ut = (UnitType)EntityTypes.Instance.GetByName(unitTypeName);

            string[] broken = variantText.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            int[] finalData = new int[broken.Length];

            for (int i = 0; i < broken.Length; i++)
                finalData[i] = int.Parse(broken[i]);

            SpawnNewUnit(ut, finalData);
        }

        private void Server_SendSupportedUnitsToClient(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(Hangar),
                (ushort)NetworkMessages.SupportedUnitsToClient);

            string supportedTypes = string.Empty;

            foreach (Hangar_Vehicle_Type t in VehicleType)
            {
                int i = (int)t;
                supportedTypes += i.ToString();
            }

            writer.Write(supportedTypes);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SupportedUnitsToClient)]
        private void Client_ReceiveSupportedUnits(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string supportedTypes = reader.ReadString();
            if (!reader.Complete())
                return;

            char[] supportedUnits = supportedTypes.ToCharArray();

            foreach (char c in supportedUnits)
            {
                int i = int.Parse(c.ToString());
                Hangar_Vehicle_Type hvt = (Hangar_Vehicle_Type)i;
                if (!VehicleType.Contains(hvt))
                    VehicleType.Add(hvt);
            }
        }
    }
}