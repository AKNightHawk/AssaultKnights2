using System.Collections.Generic;
using Engine.EntitySystem;
using Engine.Utils;

namespace ProjectEntities
{
    /// <summary>
    /// Summary of AKMapObjectType
    /// </summary>
    public class AKMapObjectType : DynamicType
    {
    }

    /// <summary>
    /// Summary of AKMapObject
    /// </summary>
    public class AKMapObject : Dynamic
    {
        [FieldSerialize]
        protected FactionType faction;

        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        public enum NetworkMessages
        {
            SpawnIdToClient,
            FactionToClient,

            //NetworkUINToClient,
            TextToClient,

            SpawnRequestToServer,
            SupportedUnitsToClient
        }

        public enum AKFilterGroups
        {
            AssaultKnights,
            OMNI,
            AI,
            AI_Players_Recordings,
            Player,
            Count,
        }

        private uint akunitgroup = 0;

        public uint FilterGroups
        {
            get { return akunitgroup; }
            set { akunitgroup = value; }
        }

        public AKMapObject()
        {
            FilterGroups |= MapObjectSceneGraphGroups.UnitGroupMask;
        }

        //public AKMapObject()
        //{
        //    FilterGroups |= MapObjectSceneGraphGroups.UnitGroupMask;
        //}

        private AKMapObjectType _type = null;

        public new AKMapObjectType Type
        {
            get { return this._type; }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            AddTimer();
        }

        private void AddTimer()
        {
            SubscribeToTickEvent();
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            IList<RemoteEntityWorld> worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            if (faction != null)
                Server_SendFactionToClients(worlds);
        }

        private void Server_SendFactionToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(AKMapObject),
                (ushort)NetworkMessages.FactionToClient);

            writer.Write(faction.Name);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.FactionToClient)]
        private void Client_ReceiveFaction(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            string s = reader.ReadString();
            if (!reader.Complete())
                return;

            faction = (FactionType)EntityTypes.Instance.GetByName(s);
        }
    }
}