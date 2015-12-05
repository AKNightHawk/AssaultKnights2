// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Engine;
using Engine.EntitySystem;
using Engine.Utils;
//using MySql.Data.MySqlClient;
using ProjectCommon;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="PlayerManager"/> entity type.
    /// </summary>
    public class PlayerManagerType : EntityType
    {
    }

    public class PlayerManager : Entity
    {
        private static PlayerManager instance;

        public string myName;

        //server side or single mode
        [FieldSerialize]
        private uint serverOrSingle_playerIdentifierCounter;

        [FieldSerialize]
        private List<ServerOrSingle_Player> serverOrSingle_players;

        private ReadOnlyCollection<ServerOrSingle_Player> serverOrSingle_playersAsReadOnly;

        public bool server_shouldUpdateDataToClients;
        public float server_updateDataToClientsLastTime;

        //client side
        private List<Client_Player> client_players;

        private ReadOnlyCollection<Client_Player> client_playersAsReadOnly;

        ///////////////////////////////////////////

        public class ServerOrSingle_Player
        {
            [FieldSerialize]
            private uint identifier;//used only for network synchronization

            [FieldSerialize]
            private string name;

            [FieldSerialize]
            private bool bot;

            private UserManagementServerNetworkService.UserInfo user;

            //[FieldSerialize]
            //int frags;

            [FieldSerialize]
            private int hitPoints;

            public int HitPoints
            {
                get { return hitPoints; }
                set
                {
                    hitPoints = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            [FieldSerialize]
            private int killPoints;

            public int KillPoints
            {
                get { return killPoints; }
                set
                {
                    killPoints = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            [FieldSerialize]
            private int assaultcredits;

            public int AssaultCredits
            {
                get { return assaultcredits; }
                set
                {
                    assaultcredits = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            [FieldSerialize]
            private int frags;

            [FieldSerialize]
            private float ping;

            [FieldSerialize]
            private Intellect intellect;

            //for serialization
            public ServerOrSingle_Player()
            {
            }

            public ServerOrSingle_Player(uint identifier, string name, bool bot,
                UserManagementServerNetworkService.UserInfo user)
            {
                this.identifier = identifier;
                this.name = name;
                this.bot = bot;
                this.user = user;
            }

            /// <summary>
            /// used only for network synchronization
            /// </summary>
            public uint Identifier
            {
                get { return identifier; }
            }

            public string Name
            {
                get { return name; }
            }

            public bool Bot
            {
                get { return bot; }
            }

            public UserManagementServerNetworkService.UserInfo User
            {
                get { return user; }
            }

            public int Frags
            {
                get { return frags; }
                set
                {
                    frags = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            public float Ping
            {
                get { return ping; }
                set
                {
                    ping = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            public Intellect Intellect
            {
                get { return intellect; }
                set
                {
                    if (intellect != null)
                        PlayerManager.Instance.UnsubscribeToDeletionEvent(intellect);
                    intellect = value;
                    if (intellect != null)
                        PlayerManager.Instance.SubscribeToDeletionEvent(intellect);
                }
            }
        }

        ///////////////////////////////////////////

        public class Client_Player
        {
            private uint identifier;
            private string name;
            private bool bot;
            private UserManagementClientNetworkService.UserInfo user;

            private int frags;
            private float ping;

            public Client_Player(uint identifier, string name, bool bot,
                UserManagementClientNetworkService.UserInfo user)
            {
                this.identifier = identifier;
                this.name = name;
                this.bot = bot;
                this.user = user;
            }

            public uint Identifier
            {
                get { return identifier; }
            }

            public string Name
            {
                get { return name; }
            }

            public bool Bot
            {
                get { return bot; }
            }

            public UserManagementClientNetworkService.UserInfo User
            {
                get { return user; }
            }

            public int Frags
            {
                get { return frags; }
                set { frags = value; }
            }

            [FieldSerialize]
            private int hitPoints;

            public int HitPoints
            {
                get { return hitPoints; }
                set
                {
                    hitPoints = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            [FieldSerialize]
            private int killPoints;

            public int KillPoints
            {
                get { return killPoints; }
                set
                {
                    killPoints = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            [FieldSerialize]
            private int assaultcredits;

            public int AssaultCredits
            {
                get { return assaultcredits; }
                set
                {
                    assaultcredits = value;
                    PlayerManager.Instance.server_shouldUpdateDataToClients = true;
                }
            }

            public float Ping
            {
                get { return ping; }
                set { ping = value; }
            }
        }

        ///////////////////////////////////////////

        private enum NetworkMessages
        {
            AddUserToClient,
            RemoveUserToClient,
            UpdateDataToClient,
        }

        ///////////////////////////////////////////

        private PlayerManagerType _type = null; public new PlayerManagerType Type { get { return _type; } }

        public PlayerManager()
        {
            if (instance != null)
                Log.Fatal("PlayerManager: PlayerManager is already created.");
            instance = this;
        }

        public static PlayerManager Instance
        {
            get { return instance; }
        }

        protected override void OnPreCreate()
        {
            base.OnPreCreate();

            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                serverOrSingle_playerIdentifierCounter = 1;
                serverOrSingle_players = new List<ServerOrSingle_Player>();
                serverOrSingle_playersAsReadOnly = new ReadOnlyCollection<ServerOrSingle_Player>(
                    serverOrSingle_players);
            }

            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                client_players = new List<Client_Player>();
                client_playersAsReadOnly = new ReadOnlyCollection<Client_Player>(client_players);
            }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SubscribeToTickEvent();
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (client_players != null)
            {
                Client_Player cp = Client_GetPlayer(myName);

                if (cp != null)
                {
                    UploadAssaultCredits(cp);
                }
            }

            instance = null;
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_UpdatePing();
                Server_TickUpdateDataToClients();
            }
        }

        protected override void OnDeleteSubscribedToDeletionEvent(Entity entity)
        {
            base.OnDeleteSubscribedToDeletionEvent(entity);

            if (serverOrSingle_players != null)
            {
                foreach (ServerOrSingle_Player player in serverOrSingle_players)
                {
                    if (player.Intellect == entity)
                        player.Intellect = null;
                }
            }
        }

        ///////////////////////////////////////////
        // Server side
        ///////////////////////////////////////////

        public IList<ServerOrSingle_Player> ServerOrSingle_Players
        {
            get { return serverOrSingle_playersAsReadOnly; }
        }

        public ServerOrSingle_Player Server_AddClientPlayer(
            UserManagementServerNetworkService.UserInfo user)
        {
            uint identifier = serverOrSingle_playerIdentifierCounter;
            serverOrSingle_playerIdentifierCounter++;

            ServerOrSingle_Player player = new ServerOrSingle_Player(identifier, user.Name, false,
                user);
            serverOrSingle_players.Add(player);

            Server_SendAddPlayerToClients(EntitySystemWorld.Instance.RemoteEntityWorlds, player);

            return player;
        }

        public ServerOrSingle_Player Single_AddSinglePlayer(string name)
        {
            uint identifier = serverOrSingle_playerIdentifierCounter;
            serverOrSingle_playerIdentifierCounter++;

            ServerOrSingle_Player player = new ServerOrSingle_Player(identifier, name, false, null);
            serverOrSingle_players.Add(player);
            return player;
        }

        public ServerOrSingle_Player ServerOrSingle_AddBotPlayer(string name)
        {
            uint identifier = serverOrSingle_playerIdentifierCounter;
            serverOrSingle_playerIdentifierCounter++;

            ServerOrSingle_Player player = new ServerOrSingle_Player(identifier, name, true, null);
            serverOrSingle_players.Add(player);

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendAddPlayerToClients(EntitySystemWorld.Instance.RemoteEntityWorlds, player);

            return player;
        }

        public void ServerOrSingle_RemovePlayer(ServerOrSingle_Player player)
        {
            if (!serverOrSingle_players.Contains(player))
                Log.Fatal("PlayerManager: ServerOrSingle_RemovePlayer: player is not exists.");

            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_SendRemovePlayerToClients(EntitySystemWorld.Instance.RemoteEntityWorlds,
                    player);
            }

            serverOrSingle_players.Remove(player);
        }

        //TODO -- Fix assault credits to local file
        private static void UploadAssaultCredits(Client_Player player)
        { 
        
        }

        //private static void UploadAssaultCredits(Client_Player player)
        //{
        //    try
        //    {
        //        //string connStr = "SERVER=;" + "DATABASE=;" + "UID=;" + "PASSWORD=;";
        //        //string connStr = "SERVER=;" + "DATABASE=" + "UID=" + "PASSWORD=;";
        //        string localhost = "SERVER=" + "DATABASE=" + "UID=" + "PASSWORD="; 
        //        MySqlConnection con = new MySqlConnection(connStr);  //localhost
        //        con.Open();

        //        int money = 0;

        //        { //select current money amount
        //            string query = "SELECT Money FROM phpap_AKusers WHERE Username=@User";

        //            MySqlCommand cmd = new MySqlCommand(query, con);

        //            MySqlParameter User = new MySqlParameter();
        //            User.ParameterName = "@User";
        //            User.Value = player.Name;
        //            cmd.Parameters.Add(User);

        //            MySqlDataReader reader = cmd.ExecuteReader();

        //            if (reader.HasRows && reader.Read())
        //            {
        //                money = reader.GetInt32(0);

        //                reader.Close();
        //                //reader.Dispose();
        //            }
        //        }

        //        {
        //            string query = "UPDATE phpap_AKusers SET Money=@Mon WHERE Username=@User";

        //            MySqlCommand cmd = new MySqlCommand(query, con);

        //            MySqlParameter User = new MySqlParameter();
        //            User.ParameterName = "@User";
        //            User.Value = player.Name;
        //            cmd.Parameters.Add(User);

        //            MySqlParameter M = new MySqlParameter();
        //            M.ParameterName = "@Mon";
        //            M.Value = money += player.AssaultCredits;
        //            cmd.Parameters.Add(M);

        //            int i = cmd.ExecuteNonQuery();
        //        }

        //        con.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex != null)
        //        {
        //            EngineConsole.Instance.Print(ex.Message);
        //        }
        //    }
        //}

        public ServerOrSingle_Player ServerOrSingle_GetPlayer(string name)
        {
            //it is can be slowly. need to use Dictionary.
            foreach (ServerOrSingle_Player player in serverOrSingle_players)
            {
                if (player.Name == name)
                    return player;
            }
            return null;
        }

        public ServerOrSingle_Player ServerOrSingle_GetPlayer(
            UserManagementServerNetworkService.UserInfo user)
        {
            if (user == null)
                Log.Fatal("PlayerManager: ServerOrSingle_GetPlayerByIntellect: user == null.");

            //it is can be slowly. need to use Dictionary.
            foreach (ServerOrSingle_Player player in serverOrSingle_players)
            {
                if (player.User == user)
                    return player;
            }
            return null;
        }

        public ServerOrSingle_Player ServerOrSingle_GetPlayer(Intellect intellect)
        {
            if (intellect == null)
                Log.Fatal("PlayerManager: ServerOrSingle_GetPlayerByIntellect: intellect == null.");

            //it is can be slowly. need to use Dictionary.
            foreach (ServerOrSingle_Player player in serverOrSingle_players)
            {
                if (player.Intellect == intellect)
                    return player;
            }
            return null;
        }

        protected override void Server_OnClientConnectedBeforePostCreate(
            RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedBeforePostCreate(remoteEntityWorld);

            RemoteEntityWorld[] worlds = new RemoteEntityWorld[] { remoteEntityWorld };

            //send player information to client
            foreach (ServerOrSingle_Player player in serverOrSingle_players)
                Server_SendAddPlayerToClients(worlds, player);

            Server_SendUpdateDataToClients(worlds);
        }

        private void Server_SendAddPlayerToClients(IList<RemoteEntityWorld> remoteEntityWorlds,
            ServerOrSingle_Player player)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(PlayerManager),
                (ushort)NetworkMessages.AddUserToClient);
            writer.WriteVariableUInt32(player.Identifier);
            writer.Write(player.Name);
            writer.Write(player.Bot);
            writer.WriteVariableUInt32(player.User != null ? player.User.Identifier : (uint)0);
            EndNetworkMessage();
        }

        private void Server_SendRemovePlayerToClients(IList<RemoteEntityWorld> remoteEntityWorlds,
            ServerOrSingle_Player player)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(PlayerManager),
                (ushort)NetworkMessages.RemoveUserToClient);
            writer.WriteVariableUInt32(player.Identifier);
            EndNetworkMessage();
        }

        private void Server_UpdatePing()
        {
            foreach (ServerOrSingle_Player player in ServerOrSingle_Players)
            {
                if (player.User != null && player.User.ConnectedNode != null)
                    player.Ping = player.User.ConnectedNode.LastRoundtripTime;
            }
        }

        private void Server_TickUpdateDataToClients()
        {
            if (server_shouldUpdateDataToClients)
            {
                const float timeInterval = .25f;

                float time = EngineApp.Instance.Time;
                if (time >= server_updateDataToClientsLastTime + timeInterval)
                {
                    Server_SendUpdateDataToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);

                    server_shouldUpdateDataToClients = false;
                    server_updateDataToClientsLastTime = time;
                }
            }
        }

        private void Server_SendUpdateDataToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(PlayerManager),
                (ushort)NetworkMessages.UpdateDataToClient);

            foreach (ServerOrSingle_Player player in serverOrSingle_players)
            {
                writer.WriteVariableUInt32(player.Identifier);
                writer.WriteVariableInt32(player.HitPoints);
                writer.WriteVariableInt32(player.KillPoints);
                writer.WriteVariableInt32(player.AssaultCredits);
                writer.Write(player.Ping);
            }

            EndNetworkMessage();
        }

        ///////////////////////////////////////////
        // Client side
        ///////////////////////////////////////////

        public IList<Client_Player> Client_Players
        {
            get { return client_playersAsReadOnly; }
        }

        public Client_Player Client_GetPlayer(string name)
        {
            //slowly. need Dictionary.
            foreach (Client_Player player in client_players)
            {
                if (player.Name == name)
                    return player;
            }
            return null;
        }

        public Client_Player Client_GetPlayer(
            UserManagementClientNetworkService.UserInfo user)
        {
            //slowly. need Dictionary.
            foreach (Client_Player player in client_players)
            {
                if (player.User == user)
                    return player;
            }
            return null;
        }

        private Client_Player Client_GetPlayer(uint identifier)
        {
            //slowly. need Dictionary.
            foreach (Client_Player player in client_players)
            {
                if (player.Identifier == identifier)
                    return player;
            }
            return null;
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.AddUserToClient)]
        private void Client_ReceiveAddUser(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            uint identifier = reader.ReadVariableUInt32();
            string name = reader.ReadString();
            bool bot = reader.ReadBoolean();
            uint userIdentifier = reader.ReadVariableUInt32();

            if (!reader.Complete())
                return;

            //check for already exists
            {
                Client_Player playerForCheck = Client_GetPlayer(identifier);

                if (playerForCheck != null)
                {
                    Log.Fatal("PlayerManager: Client_ReceiveAddUserToClient: Player " +
                        "with identifier \"{0}\" is already exists.", identifier);
                }
            }

            UserManagementClientNetworkService.UserInfo user = null;
            if (userIdentifier != 0)
                user = GameNetworkClient.Instance.UserManagementService.GetUser(userIdentifier);

            Client_Player player = new Client_Player(identifier, name, bot, user);
            client_players.Add(player);

            if (GameNetworkClient.Instance.UserManagementService.ThisUser == user)
            {
                myName = user.Name;
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.RemoveUserToClient)]
        private void Client_ReceiveRemoveUser(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            uint identifier = reader.ReadVariableUInt32();

            if (!reader.Complete())
                return;

            Client_Player player = Client_GetPlayer(identifier);
            if (player == null)
                return;

            uint ui = GameNetworkClient.Instance.UserManagementService.ThisUser.Identifier;

            client_players.Remove(player);
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UpdateDataToClient)]
        private void Client_ReceiveUpdateData(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            while (reader.BitPosition < reader.EndBitPosition)
            {
                uint identifier = reader.ReadVariableUInt32();
                int hitPoints = reader.ReadVariableInt32();
                int killPoints = reader.ReadVariableInt32();
                int assaultCredits = reader.ReadVariableInt32();
                float ping = reader.ReadSingle();

                Client_Player player = Client_GetPlayer(identifier);

                if (player != null)
                {
                    player.HitPoints = hitPoints;
                    player.KillPoints = killPoints;
                    player.AssaultCredits = assaultCredits;
                    player.Ping = ping;
                }
            }
        }
    }
}