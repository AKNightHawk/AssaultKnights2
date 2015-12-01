// Copyright (C) 2006-2009 NeoAxis Group Ltd.
using System.Collections.Generic;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    /// <summary>
    /// Defines a window of map choice.
    /// </summary>
    public class PlayerSpawnWindow : Control
    {
        private Control window;
        private Button btn;
        private TextBox txt;
        private Button done;
        private Button suicide;
        private Button AKB;
        private Button OmniB;
        private Control spawnPoints;

        public class PossibleSpawnPoint
        {
            public Button btn;
            public SpawnPoint sp;
            public string spid; //spawnId
            public TextBox text;
        }

        private List<PossibleSpawnPoint> possibleSpawnPoints = new List<PossibleSpawnPoint>();

        private PossibleSpawnPoint selectedSpawnPoint;

        private void Client_CustomMessagesService_ReceiveMessage(CustomMessagesClientNetworkService sender,
            string message, string data)
        {
            //process custom messages from server

            //if( message == "SpawnInfoToClient" )
            //{
            //GameNetworkClient.Instance.UserManagementService.ThisUser.Faction = selectedSpawnPoint.sp.Faction.Name;
            //GameNetworkClient.Instance.UserManagementService.ThisUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;
            //SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;
            GameEngineApp.Instance.CreateGameWindowForMap();
            //}
        }

        protected override void OnAttach()
        {
            if (GameMap.Instance.GameType != GameMap.GameTypes.AssaultKnights)
            {
                return;
            }

            base.OnAttach();

            if (EntitySystemWorld.Instance.IsClientOnly())//&& !EntitySystemWorld.Instance.IsEditor())
            {
                GameNetworkClient.Instance.CustomMessagesService.ReceiveMessage += new CustomMessagesClientNetworkService.ReceiveMessageDelegate(Client_CustomMessagesService_ReceiveMessage);
            }

            window = ControlDeclarationManager.Instance.CreateControl("Gui\\PlayerSpawnWindow.gui");
            Controls.Add(window);

            spawnPoints = window.Controls["spawnPoints"];

            if (Map.Instance == null)
                return;

            //Need to Get Control Manager layers from map
            //Get Basemap value

            //layers
            //{
            //    item
            //    {
            //        name = base
            //        baseMap = "Assault Knights\\AKMaps\\AriFlats\\Textures\\flatsCM.png"
            //        baseScale = 10000
            //        detailMap = "Assault Knights\\Maps\\Map Textures\\Terrain\\DetailTextures\\64d0b218.png"
            //        detailScale = 20
            //        detailNormalMap = "Assault Knights\\Maps\\Map Textures\\Terrain\\DetailTextures\\cracked_ground_normal.tga_converted.dds"
            //    }

            //Incin -- better way each file is Assault Knights\\AKMaps\\"MapName"\\Textures\\overview.png
            //each file is 1024 x 1024 .png file
            string mapvirtualpath = Map.Instance.GetVirtualFileDirectory().ToString();
            Texture backtexture = TextureManager.Instance.Load(mapvirtualpath + "\\Textures\\overview.png");

            if (backtexture != null)
                spawnPoints.BackTexture = backtexture;
            else
                Log.Info("Invalid Path or filename of spawnPoints.BackTexture window: {0}, verify location of file or file exists!", backtexture);

            Rect screenMapRect = spawnPoints.GetScreenRectangle();
            Bounds initialBounds = Map.Instance.InitialCollisionBounds;
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), initialBounds.Maximum.ToVec2());

            foreach (Entity entity in Map.Instance.Children)
            {
                if (GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
                {
                    SpawnPoint sp = entity as SpawnPoint;
                    if (sp != null)
                    {
                        btn = ControlDeclarationManager.Instance.CreateControl(
                            "Gui\\Controls\\AKDefaultBaseButton.gui") as Button;

                        txt = new TextBox();
                        txt.AutoSize = true;
                        txt.Text = sp.Text;

                        PossibleSpawnPoint psp = new PossibleSpawnPoint();
                        psp.btn = btn;
                        psp.sp = sp;
                        psp.spid = sp.SpawnID.ToString(); //set spawnid of spawnpoint -- needed network passing
                        psp.text = txt;
                        btn.UserData = psp;
                        btn.Click += new Button.ClickDelegate(btn_Click);
                        float x = 0.5f + sp.Position.X / mapRect.Size.X;
                        float y = 0.5f + sp.Position.Y / mapRect.Size.Y;

                        btn.Position = new ScaleValue(ScaleType.Parent, new Vec2(x, y));
                        txt.Position = new ScaleValue(ScaleType.Parent, new Vec2(x + 0.05f, y));
                        spawnPoints.Controls.Add(btn);
                        spawnPoints.Controls.Add(txt);
                        possibleSpawnPoints.Add(psp);
                    }
                }
            }

            done = window.Controls["Done"] as Button;
            done.Click += new Button.ClickDelegate(done_Click);

            suicide = window.Controls["Sucide"] as Button;
            suicide.Click += new Button.ClickDelegate(suicide_Click);
            AKB = window.Controls["AK"] as Button;
            AKB.Click += new Button.ClickDelegate(AKB_Click);
            OmniB = window.Controls["Omni"] as Button;
            OmniB.Click += new Button.ClickDelegate(OmniB_Click);

            AKB.Active = true;
        }

        private void btn_Click(Button sender)
        {
            PossibleSpawnPoint sp = sender.UserData as PossibleSpawnPoint;
            selectedSpawnPoint = sp;

            foreach (PossibleSpawnPoint psp in possibleSpawnPoints)
                psp.btn.Active = false;

            selectedSpawnPoint.btn.Active = true;
        }

        private void done_Click(Button sender)
        {
            Done();
        }

        private void suicide_Click(Button sender)
        {
            if (GetPlayerUnit() != null)
                GetPlayerUnit().Die();
        }

        private void AKB_Click(Button sender)
        {
            AKB.Active = true;
            OmniB.Active = false;

            selectedSpawnPoint = null;

            Rect screenMapRect = spawnPoints.GetScreenRectangle();
            Bounds initialBounds = Map.Instance.InitialCollisionBounds;
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), initialBounds.Maximum.ToVec2());

            foreach (PossibleSpawnPoint psp in possibleSpawnPoints)
            {
                psp.btn.Active = false;
                psp.btn.Enable = psp.text.Enable = (psp.sp.Faction.Name == "AssaultKnights");
            }
        }

        private void OmniB_Click(Button sender)
        {
            AKB.Active = false;
            OmniB.Active = true;

            selectedSpawnPoint = null;

            foreach (PossibleSpawnPoint psp in possibleSpawnPoints)
            {
                psp.btn.Active = false;
                psp.btn.Enable = psp.text.Enable = (psp.sp.Faction.Name == "Omni");
            }
        }

        private Unit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject;
        }

        private Intellect GetPlayerIntellect()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance;
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;
            if (e.Key == EKeys.Escape)
            {
                SetShouldDetach();
                return true;
            }

            if (e.Key == EKeys.Enter)
            {
                return Done();
            }
            return false;
        }

        //AK 0.85 code
        private bool Done()
        {
            if (selectedSpawnPoint == null)
                return false;

            if (GameNetworkServer.Instance != null)
            {
                SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp; //iNCIN added this -- test
                GameNetworkServer.Instance.UserManagementService.ServerUser.Faction = selectedSpawnPoint.sp.Faction.Name;
                GameNetworkServer.Instance.UserManagementService.ServerUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;
                GameNetworkServer.Instance.UserManagementService.ServerUser.SpawnId = selectedSpawnPoint.sp.SpawnID.ToString();
                GameEngineApp.Instance.CreateGameWindowForMap();
            }
            else if (GameNetworkClient.Instance != null)
            {
                //string error;
                GameNetworkClient.Instance.UserManagementService.ThisUser.Faction = selectedSpawnPoint.sp.Faction.Name;
                GameNetworkClient.Instance.UserManagementService.ThisUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;
                SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;

                GameNetworkClient.Instance.CustomMessagesService.SendToServer("spawnInfoToServer", GameNetworkClient.Instance.UserManagementService.ThisUser + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
                //GameNetworkClient.Instance.UserManagementService.RecieveMessage_SpawnInformationToClient(GameNetworkClient.Instance.UserManagementService.Owner.ServerConnectedNode,
                //    "spawnInfoToServer", selectedSpawnPoint.sp.NetworkUIN.ToString() + ";" + selectedSpawnPoint.sp.Faction.Name.ToString(), ref error);
                //        //GameNetworkClient.Instance.UserManagementService.RecieveMessage_SpawnInformationToServer(
                //        //GameNetworkClient.Instance.UserManagementService.ThisUser, selectedSpawnPoint.sp.NetworkUIN,
                //        //selectedSpawnPoint.sp.Faction.Name);

                //GameNetworkClient.Instance.CustomMessagesService.ReceiveMessage +=new CustomMessagesClientNetworkService.ReceiveMessageDelegate(CustomMessagesService_ReceiveMessage);
                //GameEngineApp.Instance.CreateGameWindowForMap();
            }
            else
            {
                PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.ServerOrSingle_Players[0];
                FactionType playerFaction = EntityTypes.Instance.GetByName(selectedSpawnPoint.sp.Faction.Name) as FactionType;
                player.Intellect.Faction = playerFaction;
                SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;
                GameEngineApp.Instance.CreateGameWindowForMap();
            }
            //GameEngineApp.Instance.CreateGameWindowForMap();
            return true;
        }

        //Second version
        //private bool Done()
        //{
        //    if (selectedSpawnPoint == null)
        //        return false;

        //    if (GameNetworkServer.Instance != null)
        //    {
        //        SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp; //iNCIN added this -- test
        //        GameNetworkServer.Instance.UserManagementService.ServerUser.Faction = selectedSpawnPoint.sp.Faction.Name;
        //        GameNetworkServer.Instance.UserManagementService.ServerUser.SpawnId = selectedSpawnPoint.sp.SpawnID.ToString();

        //        string player = GameNetworkServer.Instance.UserManagementService.ServerUser.Name;
        //        //GameNetworkServer.Instance.CustomMessagesService.SendToAllClients("spawnInfoToServer", GameNetworkServer.Instance.UserManagementService.Identifier + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
        //        GameNetworkServer.Instance.CustomMessagesService.SendToClient(GameNetworkServer.Instance.UserManagementService.ServerUser.ConnectedNode, "spawnInfoToServer", GameNetworkServer.Instance.UserManagementService.ServerUser + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
        //        //GameWorld.Instance.AKServerOrSingle_CreatePlayerUnit(PlayerManager.Instance.ServerOrSingle_GetPlayer(player), selectedSpawnPoint.sp as MapObject, selectedSpawnPoint.sp.Faction.ToString());

        //    }

        //    else if (GameNetworkClient.Instance != null)
        //    {
        //        SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;
        //        GameNetworkClient.Instance.UserManagementService.ThisUser.Faction = selectedSpawnPoint.sp.Faction.Name;
        //        GameNetworkClient.Instance.UserManagementService.ThisUser.SpawnId = selectedSpawnPoint.spid;//.sp.GetSpawnId(selectedSpawnPoint.sp);
        //        GameNetworkClient.Instance.CustomMessagesService.SendToServer("spawnInfoToServer", GameNetworkClient.Instance.UserManagementService.ThisUser + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
        //    }
        //    else
        //    {
        //        if (GameNetworkServer.Instance != null)
        //        {
        //        }
        //        else
        //        {
        //            PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.ServerOrSingle_Players[0];
        //            SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;
        //            FactionType playerFaction = EntityTypes.Instance.GetByName(selectedSpawnPoint.sp.Faction.Name) as FactionType;
        //            player.Intellect.Faction = playerFaction;
        //            SpawnPoint.SpawnId id = selectedSpawnPoint.sp.SpawnID;
        //        }
        //    }
        //    GameEngineApp.Instance.CreateGameWindowForMap();
        //    return true;
        //}

        //Incin --- saved content for network messaging samples
        ////////private bool Done()
        ////////{
        ////////    if (selectedSpawnPoint == null)
        ////////        return false;

        ////////    if (GameNetworkServer.Instance != null)
        ////////    {
        ////////        SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp; //iNCIN added this -- test
        ////////        GameNetworkServer.Instance.UserManagementService.ServerUser.Faction = selectedSpawnPoint.sp.Faction.Name;
        ////////        //GameNetworkServer.Instance.UserManagementService.ServerUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;
        ////////        GameNetworkServer.Instance.UserManagementService.ServerUser.SpawnId = selectedSpawnPoint.sp.SpawnID.ToString();
        ////////        //    GameNetworkServer.Instance.UserManagementService.ReceiveMessage_SpawnInformationToServer(
        ////////        //    GameNetworkServer.Instance.UserManagementService.ServerUser.ConnectedNode, selectedSpawnPoint.sp.NetworkUIN,
        ////////        //    selectedSpawnPoint.sp.Faction.Name);//, (uint)selectedSpawnPoint.sp.SpawnID);
        ////////        //GameNetworkServer.Instance.CustomMessagesService.SendToClient(GameNetworkServer.Instance.UserManagementService.ServerUser.ConnectedNode, "spawnInfoToServer", GameNetworkServer.Instance.UserManagementService.ServerUser + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
        ////////    }

        ////////    if (GameNetworkClient.Instance != null)
        ////////    {
        ////////        //SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp; //iNCIN added this -- test
        ////////        //GameNetworkServer.Instance.UserManagementService.ServerUser.Faction = selectedSpawnPoint.sp.Faction.Name;
        ////////        //GameNetworkServer.Instance.UserManagementService.ServerUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;

        ////////        //GameNetworkServer.Instance.UserManagementService.ReceiveMessage_SpawnInformationToServer(
        ////////        //GameNetworkServer.Instance.UserManagementService.ServerUser.ConnectedNode, selectedSpawnPoint.sp.NetworkUIN,
        ////////        //selectedSpawnPoint.sp.Faction.Name);//, (uint)selectedSpawnPoint.sp.SpawnID);

        ////////        SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;
        ////////        GameNetworkClient.Instance.UserManagementService.ThisUser.Faction = selectedSpawnPoint.sp.Faction.Name;
        ////////        //GameNetworkClient.Instance.UserManagementService.ThisUser.DefaultSpawnPoint = selectedSpawnPoint.sp.NetworkUIN;
        ////////        GameNetworkClient.Instance.UserManagementService.ThisUser.SpawnId = selectedSpawnPoint.spid;//.sp.GetSpawnId(selectedSpawnPoint.sp);

        ////////        //GameNetworkClient.Instance.UserManagementService.RecieveMessage_SpawnInformationToServer(
        ////////        //GameNetworkClient.Instance.UserManagementService.ThisUser, selectedSpawnPoint.sp.NetworkUIN,
        ////////        //selectedSpawnPoint.sp.Faction.Name);
        ////////        //, (uint)selectedSpawnPoint.sp.SpawnID);
        ////////        //GameNetworkClient.Instance.UserManagementService.SpawnInfoEvent += GameNetworkClient.Instance.UserManagementService.RecieveMessage_SpawnInfoToServer//DedicatedServer.MainForm.SpawnInfo;
        ////////        GameNetworkClient.Instance.CustomMessagesService.SendToServer("spawnInfoToServer", GameNetworkClient.Instance.UserManagementService.ThisUser + ";" + selectedSpawnPoint.spid + ";" + selectedSpawnPoint.sp.Faction.Name);
        ////////        //return true;

        ////////    }
        ////////    else
        ////////    {
        ////////        //incin buggins
        ////////        PlayerManager.ServerOrSingle_Player player = PlayerManager.Instance.ServerOrSingle_GetPlayer(this.GetPlayerIntellect());//.ServerOrSingle_Players[0];

        ////////        SpawnPoint.SelectedSinglePlayerPoint = selectedSpawnPoint.sp;

        ////////        FactionType playerFaction = EntityTypes.Instance.GetByName(selectedSpawnPoint.sp.Faction.Name) as FactionType;
        ////////        player.Intellect.Faction = playerFaction;

        ////////        //your so close already
        ////////        SpawnPoint.SpawnId id = selectedSpawnPoint.sp.SpawnID;

        ////////        if (GameNetworkServer.Instance != null)
        ////////        {
        ////////            //for server as a client...
        ////////            GameWorld.Instance.AKServerOrSingle_CreatePlayerUnit(PlayerManager.Instance.ServerOrSingle_GetPlayer(player.Name), selectedSpawnPoint.sp as MapObject, player.Intellect.Faction.ToString());
        ////////        }
        ////////    }
        ////////    GameEngineApp.Instance.CreateGameWindowForMap();
        ////////    return true;
        ////////}
    }
}