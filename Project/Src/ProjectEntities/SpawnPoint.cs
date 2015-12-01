// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="SpawnPoint"/> entity type.
    /// </summary>
    public class SpawnPointType : AKMapObjectType
    {
    }

    public class SpawnPoint : AKMapObject
    {
        private static List<SpawnPoint> instances = new List<SpawnPoint>();

        public static SpawnPoint SelectedSinglePlayerPoint = null;

        [FieldSerialize]
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

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
        [DefaultValue(SpawnId.NONE)]
        private SpawnId spawnid;

        public SpawnId SpawnID
        {
            get { return spawnid; }
            set { spawnid = value; }
        }

        [FieldSerialize]
        private float respawntime;

        [DefaultValue((float)5)]
        public float RespawnTime
        {
            get { return respawntime; }
            set { respawntime = value; }
        }

        [FieldSerialize]
        private bool defaultPoint;

        private static bool noSpawnPointLogInformed;

        //

        private SpawnPointType _type = null; public new SpawnPointType Type { get { return _type; } }

        [DefaultValue(false)]
        public bool DefaultPoint
        {
            get { return defaultPoint; }
            set { defaultPoint = value; }
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

        public static SpawnPoint GetRandomSpawnPoint()
        {
            if (instances.Count == 0)
                return null;
            return instances[World.Instance.Random.Next(instances.Count)];
        }

        public static SpawnPoint GetDefaultSpawnPoint()
        {
            foreach (SpawnPoint spawnPoint in instances)
                if (spawnPoint.DefaultPoint)
                    return spawnPoint;
            return null;
        }

        public static List<SpawnPoint> Instances()
        {
            return instances;
        }

        //iNCIN return spawn Id
        public bool IsSpawnId(SpawnPoint spawnpoint)
        {
            if (spawnpoint.SpawnID == this.SpawnID)
                return true;
            else
                return false;
        }

        public SpawnId GetSpawnId(SpawnPoint spawnpoint)
        {
            return spawnpoint.spawnid;
        }

        //public static int AKGetSpawnIdBySpawnIdwithFacton(SpawnPoint spawnpoint, FactionType faction)
        //{
        //    foreach (SpawnPoint sp in instances)
        //    {
        //        if (sp.faction != faction)
        //            continue;

        //        if (sp.SpawnID != spawnpoint.SpawnID)
        //        {
        //            continue;
        //        }

        //        return (int)spawnpoint.SpawnID;

        //    }
        //    return 0;
        //}

        public static SpawnPoint AKGetSpawnIdBySpawnIdwithFaction(uint spawnid, FactionType faction)
        {
            foreach (SpawnPoint sp in instances)
            {
                if (sp.faction != faction)
                    continue;

                if (sp.SpawnID != (SpawnId)spawnid)
                {
                    continue;
                }

                return sp;
            }
            return null;
        }

        public static SpawnPoint AKGetSpawnPointById(int spawnid)
        {
            foreach (SpawnPoint sp in instances)
            {
                if (sp.SpawnID == (SpawnId)spawnid)
                    return sp;
                else
                    continue;
            }

            return null;
        }

        public static SpawnPoint AKGetFreeRandomSpawnPoint(FactionType faction)
        {
            foreach (SpawnPoint sp in instances)
            {
                if (sp.faction != faction)
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

        public static SpawnPoint GetFreeRandomSpawnPoint()
        {
            for (int n = 0; n < 10; n++)
            {
                SpawnPoint spawnPoint = GetRandomSpawnPoint();

                if (spawnPoint == null)
                {
                    if (!noSpawnPointLogInformed)
                    {
                        Log.Warning("No spawn points.");
                        noSpawnPointLogInformed = true;
                    }
                    return null;
                }

                bool busy = false;
                {
                    Bounds volume = new Bounds(spawnPoint.Position);
                    volume.Expand(new Vec3(1, 1, 2));

                    Body[] result = PhysicsWorld.Instance.VolumeCast(volume,
                        (int)ContactGroup.CastOnlyContact);

                    foreach (Body body in result)
                    {
                        if (body.Static)
                            continue;

                        foreach (Shape shape in body.Shapes)
                        {
                            if (PhysicsWorld.Instance.MainScene.IsContactGroupsContactable(shape.ContactGroup,
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
                    return spawnPoint;
            }
            return null;
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            if (!string.IsNullOrEmpty(text))
                Server_SendTextToClients(worlds);
            Server_SendSpawnId(worlds); //SpawnId
        }

        private void Server_SendTextToClients(IList<RemoteEntityWorld> worlds)
        {
            SendDataWriter writer = BeginNetworkMessage(worlds, typeof(SpawnPoint), (ushort)NetworkMessages.TextToClient);

            writer.Write(text);
            //Log.Info("Server_SendTextToClients string: " + text);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TextToClient)]
        private void Client_ReceiveFaction(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string s = reader.ReadString();
            //Log.Error("Client_ReceiveFaction string: " + s);
            if (!reader.Complete())
                return;
            text = s;
            //faction = (FactionType)EntityTypes.Instance.GetByName(s);
        }

        //Incin -- SpawnId Network Messages
        public bool IsSpawnId(SpawnPoint spawnpoint, SpawnId id)
        {
            if (spawnpoint.SpawnID == id)
                return true;
            else
                return false;
        }

        public static SpawnId AKGetSpawnIdBySpawnIdwithFacton(SpawnPoint spawnpoint, FactionType faction)
        {
            foreach (SpawnPoint sp in instances)
            {
                if (sp.faction != faction)
                    continue;

                if (sp.SpawnID != spawnpoint.SpawnID)
                {
                    continue;
                }

                return spawnpoint.SpawnID;
            }
            return SpawnId.NONE;
        }

        public static SpawnPoint AKGetSpawnPointById(SpawnId id)
        {
            foreach (SpawnPoint sp in instances)
            {
                if (sp.SpawnID != id)
                    continue;
                else if (sp.SpawnID == id)
                    return sp;
                else
                    return null;
            }

            return null;
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SpawnIdToClient)]
        private void Client_ReceiveSpawnId(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int spawnId = reader.ReadVariableInt32();
            if (!reader.Complete())
                return;
            spawnid = (SpawnId)spawnId;// EntityTypes.Instance.GetByName(s);
        }

        private void Server_SendSpawnId(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(SpawnPoint),
                (ushort)NetworkMessages.SpawnIdToClient);
            writer.WriteVariableInt32((int)spawnid);
            EndNetworkMessage();
        }

        //[NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.NetworkUINToClient)]
        //void Client_ReceiveNetworkUIN(RemoteEntityWorld sender, ReceiveDataReader reader)
        //{
        //    int networkUIN = reader.ReadVariableInt32();
        //    if (!reader.Complete())
        //        return;
        //    this.NetworkUIN = (uint)networkUIN;// EntityTypes.Instance.GetByName(s);
        //}

        //void Server_SendNetworkUIN(IList<RemoteEntityWorld> remoteEntityWorlds)
        //{
        //    SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(SpawnPoint),
        //        (ushort)NetworkMessages.NetworkUINToClient);
        //    writer.WriteVariableInt32((int)this.NetworkUIN);
        //    EndNetworkMessage();
        //}
    }
}