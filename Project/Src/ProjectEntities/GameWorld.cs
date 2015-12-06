// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Networking;
using ProjectCommon;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="GameWorld"/> entity type.
    /// </summary>
    public class GameWorldType : WorldType
    {
    }

    public class GameWorld : World
    {
        private static GameWorld instance;

        //for moving player character between maps
        private string needChangeMapName;

        private string needChangeMapSpawnPointName;
        private PlayerCharacter.ChangeMapInformation needChangeMapPlayerCharacterInformation;
        private string needChangeMapPreviousMapName;

        private bool needWorldDestroy;

        //

        private GameWorldType _type = null; public new GameWorldType Type { get { return _type; } }

        public GameWorld()
        {
            instance = this;
        }

        public static new GameWorld Instance
        {
            get { return instance; }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            SubscribeToTickEvent();

            //create PlayerManager
            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                if (PlayerManager.Instance == null)
                {
                    PlayerManager manager = (PlayerManager)Entities.Instance.Create(
                        "PlayerManager", this);
                    manager.PostCreate();
                }
	            if (EntitySystemWorld.Instance.IsServer())
	            {
		            GameNetworkServer.Instance.CustomMessagesService.ReceiveMessage += SpawnInfo;
	            }

			}
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
        protected override void OnDestroy()
        {
			if( EntitySystemWorld.Instance.IsServer() )
			{
				GameNetworkServer.Instance.CustomMessagesService.ReceiveMessage -= SpawnInfo;
			}
			base.OnDestroy();

            instance = null;
			
		}

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            //single mode. recreate player units if need
            if (EntitySystemWorld.Instance.IsSingle())
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.Action ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TurretDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.VillageDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                {
                    if (PlayerManager.Instance != null)
                    {
                        foreach (PlayerManager.ServerOrSingle_Player player in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {
                            if (player.Intellect == null || player.Intellect.ControlledObject == null)
                            {
                                ServerOrSingle_CreatePlayerUnit(player);
                            }
                        }
                    }
                }
            }

            //networking mode
            if (EntitySystemWorld.Instance.IsServer())
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.Action ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TurretDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.VillageDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights) //iNCIN -- OK NOW
                {
                    if (PlayerManager.Instance != null)
                    {
                        UserManagementServerNetworkService userManagementService =
                            GameNetworkServer.Instance.UserManagementService;

                        //remove users
                    again:
                        foreach (PlayerManager.ServerOrSingle_Player player in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {
                            if (player.User != null && player.User != userManagementService.ServerUser)
                            {
                                NetworkNode.ConnectedNode connectedNode = player.User.ConnectedNode;
                                if (connectedNode == null ||
                                    connectedNode.Status != NetworkConnectionStatuses.Connected)
                                {
                                    if (player.Intellect != null)
                                    {
                                        PlayerIntellect playerIntellect = player.Intellect as PlayerIntellect;
                                        if (playerIntellect != null)
                                            playerIntellect.TryToRestoreMainControlledUnit();

                                        if (player.Intellect.ControlledObject != null)
                                            player.Intellect.ControlledObject.Die();
                                        player.Intellect.SetForDeletion(true);
                                        player.Intellect = null;
                                    }

                                    PlayerManager.Instance.ServerOrSingle_RemovePlayer(player);

                                    goto again;
                                }
                            }
                        }

                        //add users
                        foreach (UserManagementServerNetworkService.UserInfo user in
                            userManagementService.Users)
                        {
                            //check whether "EntitySystem" service on the client
                            if (user.ConnectedNode != null)
                            {
                                if (!user.ConnectedNode.RemoteServices.Contains("EntitySystem"))
                                    continue;
                            }

                            PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                                ServerOrSingle_GetPlayer(user);

                            if (player == null)
                            {
                                player = PlayerManager.Instance.Server_AddClientPlayer(user);

                                PlayerIntellect intellect = (PlayerIntellect)Entities.Instance.
                                    Create("PlayerIntellect", World.Instance);
                                intellect.PostCreate();

                                player.Intellect = intellect;

                                if (GameNetworkServer.Instance.UserManagementService.ServerUser != user)
                                {
                                    //player on client
                                    RemoteEntityWorld remoteEntityWorld = GameNetworkServer.Instance.
                                        EntitySystemService.GetRemoteEntityWorld(user);
                                    intellect.Server_SendSetInstanceToClient(remoteEntityWorld);
                                }
                                else
                                {
                                    //player on this server
                                    PlayerIntellect.SetInstance(intellect);
                                }

                                //player.Intellect = intellect;
                                if (player.User.Faction == null)
                                    return;
                                FactionType f = (FactionType)EntityTypes.Instance.GetByName(player.User.Faction);
                                player.Intellect.Faction = f;
                            }
                        }

                        //create units
                        foreach (PlayerManager.ServerOrSingle_Player player in
                            PlayerManager.Instance.ServerOrSingle_Players)
                        {
                            if (player.Intellect != null && player.Intellect.ControlledObject == null)
                            {
	                            if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
	                            {
									ServerOrSingle_CreatePlayerUnit( player );
								}
									else
								{
                                    ServerOrSingle_CreatePlayerUnit(player);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void DoActionsAfterMapCreated()
        {
            if (EntitySystemWorld.Instance.IsSingle())
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.Action ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TPSArcade ||
                    GameMap.Instance.GameType == GameMap.GameTypes.TurretDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.VillageDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.PlatformerDemo ||
                    GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                {
                    string playerName = "__SinglePlayer__";

                    //create Player
                    PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.
                        ServerOrSingle_GetPlayer(playerName);
                    if (player == null)
                        player = PlayerManager.Instance.Single_AddSinglePlayer(playerName);

                    //create PlayerIntellect
                    PlayerIntellect intellect = null;
                    {
                        //find already created PlayerIntellect
                        foreach (Entity entity in World.Instance.Children)
                        {
                            intellect = entity as PlayerIntellect;
                            if (intellect != null)
                                break;
                        }

                        if (intellect == null)
                        {
                            intellect = (PlayerIntellect)Entities.Instance.Create("PlayerIntellect",
                                World.Instance);
                            intellect.PostCreate();

                            player.Intellect = intellect;
                        }

                        //set instance
                        if (PlayerIntellect.Instance == null)
                            PlayerIntellect.SetInstance(intellect);
                    }

                    //create unit
                    if (intellect.ControlledObject == null)
                    {
                        MapObject spawnPoint = null;
                        if (!string.IsNullOrEmpty(needChangeMapSpawnPointName))
                        {
                            spawnPoint = Entities.Instance.GetByName(needChangeMapSpawnPointName) as MapObject;
                            if (spawnPoint == null)
                            {
                                Log.Warning("GameWorld: Object with name \"{0}\" does not exist.",
                                    needChangeMapSpawnPointName);
                            }
                        }

                        Unit unit;
                        if (spawnPoint != null)
                            unit = ServerOrSingle_CreatePlayerUnit(player, spawnPoint);
                        else
                            unit = ServerOrSingle_CreatePlayerUnit(player);

                        if (needChangeMapPlayerCharacterInformation != null)
                        {
                            PlayerCharacter playerCharacter = (PlayerCharacter)unit;
                            playerCharacter.ApplyChangeMapInformation(
                                needChangeMapPlayerCharacterInformation, spawnPoint);
                        }
                        else
                        {
                            if (unit != null)
                            {
                                intellect.LookDirection = SphereDir.FromVector(
                                    unit.Rotation.GetForward());
                            }
                        }
                    }
                }
            }

            needChangeMapName = null;
            needChangeMapSpawnPointName = null;
            needChangeMapPlayerCharacterInformation = null;
        }

		//public Unit AKServerOrSingle_CreatePlayerUnit(PlayerManager.ServerOrSingle_Player player,
		//    MapObject spawnPoint, string faction)
		//{
		//    string unitTypeName;
		//    FactionType AKFaction;

		//    //PlayerIntellect intellect = player.Intellect as PlayerIntellect; //invalid
		//    if (faction.ToString().Equals("AssaultKnights") || faction.ToString().Equals("AssaultKnights (Faction)"))
		//        AKFaction = (FactionType)Entities.Instance.Create("AssaultKnights", Map.Instance).Type;
		//    else
		//        AKFaction = (FactionType)Entities.Instance.Create("Omni", Map.Instance).Type;

		//    if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
		//    {
		//        if (player.Intellect.Faction == null)
		//            player.Intellect.Faction = AKFaction;
		//        else if (player.Intellect.Faction != AKFaction)
		//            player.Intellect.Faction = AKFaction;
		//        else
		//            player.Intellect.Faction = AKFaction;
		//    }

		//    if (!player.Bot)
		//    {
		//        if (player.Intellect.Faction.Name == "AssaultKnights" && GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
		//            unitTypeName = "AKSoldier";
		//        else if (player.Intellect.Faction.Name == "Omni" && GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
		//            unitTypeName = "OmniSoldier";
		//        else if (GameMap.Instance.PlayerUnitType != null)
		//            unitTypeName = GameMap.Instance.PlayerUnitType.Name;
		//        else
		//            unitTypeName = "Girl";
		//    }
		//    else if (player.Bot)
		//        unitTypeName = player.Name;
		//    else
		//        unitTypeName = "Rabbit";

		//    Unit unit = (Unit)Entities.Instance.Create(unitTypeName, Map.Instance);

		//    Vec3 posOffset = new Vec3(0, 0, 1.5f);
		//    unit.Position = spawnPoint.Position + posOffset;
		//    unit.Rotation = spawnPoint.Rotation;

		//    if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
		//        unit.InitialFaction = AKFaction;//player.Intellect.Faction;

		//    unit.PostCreate();

		//    if (player.Intellect != null)
		//    {
		//        player.Intellect.Faction = AKFaction;//player.Intellect.Faction;
		//        player.Intellect.ControlledObject = unit;
		//        unit.SetIntellect(player.Intellect, false);
		//    }

		//    Teleporter teleporter = spawnPoint as Teleporter;
		//    if (teleporter != null)
		//        teleporter.ReceiveObject(unit, null);

		//    BoxTeleporter boxteleporter = spawnPoint as BoxTeleporter;
		//    if (boxteleporter != null)
		//        boxteleporter.ReceiveObject(unit, null);

		//    return unit;
		//}

		public static void SpawnInfo( CustomMessagesServerNetworkService sender,
		   NetworkNode.ConnectedNode info, string message, string data )
		{
			if( message == "SpawnInfoToServer" )
			{
				string[] parameters = data.Split( ';' );
				string userid = parameters[ 0 ];
				string selectedspawnid = parameters[ 1 ];
				string selectedfaction = parameters[ 2 ];
				//SpawnPoint target = null;

				var user = GameNetworkServer.Instance.UserManagementService.GetUser( uint.Parse( userid ) );

				user.SpawnId = uint.Parse( selectedspawnid );
				var player = PlayerManager.Instance.ServerOrSingle_GetPlayer( user );
				player.Intellect.Faction = EntityTypes.Instance.GetByName( selectedfaction ) as FactionType;

			}
		}

		//original 1.32 code -- Incin
		private Unit ServerOrSingle_CreatePlayerUnit(PlayerManager.ServerOrSingle_Player player)
        {
            SpawnPoint spawnPoint = null;

            if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
            {
                List<SpawnPoint> instancePoints = SpawnPoint.Instances();
                foreach (SpawnPoint sp in instancePoints)
                {
                    if (sp == null || sp.Faction == null) //Incin -- additional check 
                        return null;

                    if (sp.Faction != player.Intellect.Faction)
                        continue;
	                if (sp.SpawnID != (SpawnPoint.SpawnId) player.User.SpawnId)
		                continue;
					//spawnPoint = SpawnPoint.GetDefaultSpawnPoint();
	                spawnPoint = sp;

					if (spawnPoint != null)
                        return ServerOrSingle_CreatePlayerUnit(player, spawnPoint);

                    ////Other Game Modes
                    //if (spawnPoint == null)
                    //    spawnPoint = SpawnPoint.GetDefaultSpawnPoint();

                    //if (spawnPoint == null)
                    //    spawnPoint = SpawnPoint.GetFreeRandomSpawnPoint();

                    if (spawnPoint == null)
                        return null;

                    return ServerOrSingle_CreatePlayerUnit(player, spawnPoint);
                }
            }
            else
            {
                //Other Game Modes
                if (spawnPoint == null)
                    spawnPoint = SpawnPoint.GetDefaultSpawnPoint();

                if (spawnPoint == null)
                    spawnPoint = SpawnPoint.GetFreeRandomSpawnPoint();

                if (spawnPoint == null)
                    return null;

                return ServerOrSingle_CreatePlayerUnit(player, spawnPoint);
            }

            return null;
        }

        private Unit ServerOrSingle_CreatePlayerUnit(PlayerManager.ServerOrSingle_Player player,
            MapObject spawnPoint)
        {
            string unitTypeName;
            if (!player.Bot)
            {
                if (player.Intellect.Faction.Name == "AssaultKnights" && GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                    unitTypeName = "AKSoldier";
                else if (player.Intellect.Faction.Name == "Omni" && GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                    unitTypeName = "OmniSoldier";
                else if (GameMap.Instance.PlayerUnitType != null)
                    unitTypeName = GameMap.Instance.PlayerUnitType.Name;
                else
                    unitTypeName = "Girl";
            }
            else if (player.Bot)
                unitTypeName = player.Name;
            else
                unitTypeName = "Rabbit";

            Unit unit = (Unit)Entities.Instance.Create(unitTypeName, Map.Instance);

            Vec3 posOffset = new Vec3(0, 0, 1.5f);
            unit.Position = spawnPoint.Position + posOffset;
            unit.Rotation = spawnPoint.Rotation;
            unit.PostCreate();

            if (player.Intellect != null)
            {
                player.Intellect.ControlledObject = unit;
                unit.SetIntellect(player.Intellect, false);
            }

            Teleporter teleporter = spawnPoint as Teleporter;
            if (teleporter != null)
                teleporter.ReceiveObject(unit, null);

            //Incin -- Custom Box teleporter
            BoxTeleporter boxteleporter = spawnPoint as BoxTeleporter; 
            if (boxteleporter != null)
                boxteleporter.ReceiveObject(unit, null);

            return unit;
        }

        /*
		Unit ServerOrSingle_CreatePlayerUnit( PlayerManager.ServerOrSingle_Player player )
		{
            FactionType faction = player.Intellect.Faction;
            List<SpawnPoint> instancePoints = SpawnPoint.Instances();
            SpawnPoint spawnPoint = null;

            if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights && SpawnPoint.Instances().Count >= 2)
            {
                //if (ProjectEntities.SpawnPoint.SelectedSinglePlayerPoint.RespawnTime <= respawntime)
                //    return null;
                if (GameNetworkServer.Instance != null || GameNetworkClient.Instance != null)
                {
                    foreach (SpawnPoint sp in instancePoints)
                    {
                        SpawnPoint.SpawnId spawnid = ProjectEntities.SpawnPoint.SelectedSinglePlayerPoint.SpawnID;
                        if (sp.SpawnID != spawnid)
                            continue;
                        spawnPoint = SpawnPoint.AKGetSpawnPointById(spawnid);
                    }
                    //spawnPoint = SpawnPoint.AKGetSpawnIdBySpawnIdwithFacton(instancePoints .SelectedSinglePlayerPoint.SpawnID);
                    if (spawnPoint != null)
                        return ServerOrSingle_CreatePlayerUnit(player, spawnPoint);
                }

                if (spawnPoint != null)
                {
                    if (ProjectEntities.SpawnPoint.SelectedSinglePlayerPoint == null) //release fix -- need this not to be null 1st
                        return null;
                    else
                        spawnPoint = SpawnPoint.AKGetSpawnPointById(ProjectEntities.SpawnPoint.SelectedSinglePlayerPoint.SpawnID);
                }
                else if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                {
                    spawnPoint = SpawnPoint.AKGetFreeRandomSpawnPoint(faction);
                }

                //3 more chances for spawnpoints
                if (spawnPoint == null)
                {
                    spawnPoint = SpawnPoint.GetDefaultSpawnPoint();
                }

                if (spawnPoint == null)
                    spawnPoint = SpawnPoint.GetFreeRandomSpawnPoint();

                if (spawnPoint == null)
                    return null;

                return ServerOrSingle_CreatePlayerUnit(player, spawnPoint);
            }
            else if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
            {
                spawnPoint = SpawnPoint.AKGetFreeRandomSpawnPoint(faction);
            }

            //3 more chances for spawnpoints
            if (spawnPoint == null)
        {
                spawnPoint = SpawnPoint.GetDefaultSpawnPoint();
            }

			if( spawnPoint == null )
				spawnPoint = SpawnPoint.GetFreeRandomSpawnPoint();

			if( spawnPoint == null )
				return null;
			return ServerOrSingle_CreatePlayerUnit( player, spawnPoint );
		}
        */

        public string NeedChangeMapName
        {
            get { return needChangeMapName; }
        }

        public string NeedChangeMapSpawnPointName
        {
            get { return needChangeMapSpawnPointName; }
        }

        public string NeedChangeMapPreviousMapName
        {
            get { return needChangeMapPreviousMapName; }
        }

        public void NeedChangeMap(string mapName, string spawnPointName,
            PlayerCharacter.ChangeMapInformation playerCharacterInformation)
        {
            if (needChangeMapName != null)
                return;
            needChangeMapName = mapName;
            needChangeMapSpawnPointName = spawnPointName;
            needChangeMapPlayerCharacterInformation = playerCharacterInformation;
            needChangeMapPreviousMapName = Map.Instance.VirtualFileName;
        }

        [Browsable(false)]
        public bool NeedWorldDestroy
        {
            get { return needWorldDestroy; }
            set { needWorldDestroy = value; }
        }
    }
}