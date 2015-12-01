// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.IO;
using Engine;
using Engine.FileSystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;

namespace Game
{
    public class MultiplayerLobbyWindow : Control
    {
        private const string exampleOfProceduralMapCreationText = "[The example of a procedural map creation]";

        [Config("MultiplayerLobbyWindow", "lastMapName")]
        private static string lastMapName = "AKMaps\\NewAvalion\\Map.map"; //"Demos\\JigsawPuzzleGame\\Map\\Map.map";//Jigsaw puzzle by default

        private Control window;
        private ListBox listBoxMaps;
        private CheckBox checkBoxAllowToConnectDuringGame;
        private CheckBox checkBoxShowAIMaps;
        private Button buttonStart;
        private ListBox listBoxUsers;
        private EditBox editBoxChatMessage;

        ///////////////////////////////////////////

        private class MapItem
        {
            public string mapName;
            public bool recommended;
            public bool noNetworkingSupport;

            public MapItem(string mapName, bool recommended, bool noMultiplayerSupport)
            {
                this.mapName = mapName;
                this.recommended = recommended;
                this.noNetworkingSupport = noMultiplayerSupport;
            }

            public override string ToString()
            {
                string text = mapName;
                if (recommended)
                    text += " (Recommended)";
                if (noNetworkingSupport)
                    text += " (no networking support)";
                return text;
            }
        }

        ///////////////////////////////////////////

        protected override void OnAttach()
        {
            base.OnAttach();

            //register config fields
            EngineApp.Instance.Config.RegisterClassParameters(GetType());

            //create window
            window = ControlDeclarationManager.Instance.CreateControl(
                "Gui\\MultiplayerLobbyWindow.gui");
            Controls.Add(window);

            MouseCover = true;
            BackColor = new ColorValue(0, 0, 0, .5f);

            ((Button)window.Controls["Exit"]).Click += Exit_Click;

            buttonStart = (Button)window.Controls["Start"];
            if (GameNetworkServer.Instance != null)
                buttonStart.Click += Start_Click;
            if (GameNetworkClient.Instance != null)
                buttonStart.Enable = false;

            listBoxUsers = (ListBox)window.Controls["Users"];

            editBoxChatMessage = (EditBox)window.Controls["ChatMessage"];
            editBoxChatMessage.PreKeyDown += editBoxChatMessage_PreKeyDown;

            editBoxChatMessage.MouseDown += delegate
            {
                editBoxChatMessage.Text = "";
            };

            checkBoxShowAIMaps = (CheckBox)window.Controls["ShowAIMaps"];
            if (GameNetworkServer.Instance != null)
            {
                checkBoxShowAIMaps.CheckedChange += checkBoxShowAIMaps_CheckedChange;
            }
            else
            {
                checkBoxShowAIMaps.Enable = false;
            }

            //comboBoxMaps
            {
                listBoxMaps = (ListBox)window.Controls["Maps"];

                if (GameNetworkServer.Instance != null)
                {
                    //procedural map creation
                    listBoxMaps.Items.Add(new MapItem(exampleOfProceduralMapCreationText, false, false));
                    if (lastMapName == exampleOfProceduralMapCreationText)
                        listBoxMaps.SelectedIndex = listBoxMaps.Items.Count - 1;

                    string[] mapList = VirtualDirectory.GetFiles("", "*.map",
                        SearchOption.AllDirectories);

                    foreach (string mapName in mapList)
                    {
                        //check for network support
                        if (VirtualFile.Exists(string.Format("{0}\\NoNetworkSupport.txt",
                            Path.GetDirectoryName(mapName))))
                        {
                            continue;
                        }

                        if (mapName.Contains("Demo"))
                            continue;

                        if (checkBoxShowAIMaps.Checked == false && mapName.Contains("AI"))
                            continue;

                        bool recommended = mapName.Contains("NewAvalion") || mapName.Contains("MossHillSpikes");
                        //mapName.Contains( "DeathmatchDemo" );

                        listBoxMaps.Items.Add(new MapItem(mapName, recommended, true));
                        if (mapName == lastMapName)
                            listBoxMaps.SelectedIndex = listBoxMaps.Items.Count - 1;
                    }

                    listBoxMaps.SelectedIndexChange += listBoxMaps_SelectedIndexChange;

                    if (listBoxMaps.Items.Count != 0 && listBoxMaps.SelectedIndex == -1)
                        listBoxMaps.SelectedIndex = 0;
                }
                else
                {
                    ;//listBoxMaps.Enable = false;
                }
            }

            //checkBoxAllowToConnectDuringGame
            {
                checkBoxAllowToConnectDuringGame = (CheckBox)window.Controls[
                    "AllowToConnectDuringGame"];

                if (GameNetworkServer.Instance != null)
                {
                    checkBoxAllowToConnectDuringGame.CheckedChange +=
                        checkBoxAllowToConnectDuringGame_CheckedChange;
                }
                else
                {
                    checkBoxAllowToConnectDuringGame.Enable = false;
                }
            }

            //server specific
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
            {
                //for receive map name
                server.UserManagementService.AddUserEvent += Server_UserManagementService_AddUserEvent;

                //for chat support
                server.ChatService.ReceiveText += Server_ChatService_ReceiveText;
            }

            //client specific
            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
            {
                //for receive map name
                client.CustomMessagesService.ReceiveMessage +=
                    Client_CustomMessagesService_ReceiveMessage;

                //for chat support
                client.ChatService.ReceiveText += Client_ChatService_ReceiveText;

                AddMessage(string.Format("Connected to server: \"{0}\"", client.RemoteServerName));
                foreach (string serviceName in client.ServerConnectedNode.RemoteServices)
                    AddMessage(string.Format("Server service: \"{0}\"", serviceName));
            }

            UpdateControls();
        }

        protected override void OnDetach()
        {
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
            {
                //for receive map name
                server.UserManagementService.AddUserEvent -= Server_UserManagementService_AddUserEvent;

                //for chat support
                server.ChatService.ReceiveText -= Server_ChatService_ReceiveText;
            }

            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
            {
                //for receive map name
                client.CustomMessagesService.ReceiveMessage -=
                    Client_CustomMessagesService_ReceiveMessage;

                //for chat support
                client.ChatService.ReceiveText -= Client_ChatService_ReceiveText;
            }

            base.OnDetach();
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);

            UpdateUserList();

            UpdateControls();
        }

        private void editBoxChatMessage_PreKeyDown(KeyEvent e, ref bool handled)
        {
            if (e.Key == EKeys.Return && editBoxChatMessage.Focused)
            {
                SayChatMessage();
                handled = true;
                return;
            }
        }

        private void UpdateUserList()
        {
            //server
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
            {
                UserManagementServerNetworkService userService = server.UserManagementService;

                bool shouldUpdate = false;
                if (userService.Users.Count == listBoxUsers.Items.Count)
                {
                    int index = 0;

                    foreach (UserManagementServerNetworkService.UserInfo user in userService.Users)
                    {
                        if (user != listBoxUsers.Items[index])
                            shouldUpdate = true;
                        index++;
                    }
                }
                else
                    shouldUpdate = true;

                if (shouldUpdate)
                {
                    //update list box
                    listBoxUsers.Items.Clear();
                    foreach (UserManagementServerNetworkService.UserInfo user in userService.Users)
                        listBoxUsers.Items.Add(user);
                }
            }

            //client
            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
            {
                UserManagementClientNetworkService userService = client.UserManagementService;

                bool shouldUpdate = false;
                if (userService.Users.Count == listBoxUsers.Items.Count)
                {
                    int index = 0;

                    foreach (UserManagementClientNetworkService.UserInfo user in userService.Users)
                    {
                        if (user != listBoxUsers.Items[index])
                            shouldUpdate = true;
                        index++;
                    }
                }
                else
                    shouldUpdate = true;

                if (shouldUpdate)
                {
                    //update list box
                    listBoxUsers.Items.Clear();
                    foreach (UserManagementClientNetworkService.UserInfo user in userService.Users)
                        listBoxUsers.Items.Add(user);
                }
            }
        }

        public void AddMessage(string text)
        {
            ListBox listBox = (ListBox)window.Controls["Messages"];

            listBox.Items.Add(text);
            listBox.SelectedIndex = listBox.Items.Count - 1;
        }

        private void Server_UserManagementService_AddUserEvent(UserManagementServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo user)
        {
            GameNetworkServer server = GameNetworkServer.Instance;
            //send map name to new client
            server.CustomMessagesService.SendToClient(user.ConnectedNode, "Lobby_MapName",
                SelectedMapName);
            //send AllowToConnectDuringGame flag to new client
            server.CustomMessagesService.SendToClient(user.ConnectedNode,
                "Lobby_AllowToConnectDuringGame", checkBoxAllowToConnectDuringGame.Checked.ToString());
        }

        private void Server_ChatService_ReceiveText(ChatServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo fromUser, string text,
            UserManagementServerNetworkService.UserInfo privateToUser)
        {
            string userName = fromUser != null ? fromUser.Name : "(null)";
            AddMessage(string.Format("{0}: {1}", userName, text));
        }

        private void Client_ChatService_ReceiveText(ChatClientNetworkService sender,
            UserManagementClientNetworkService.UserInfo fromUser, string text)
        {
            string userName = fromUser != null ? fromUser.Name : "(null)";
            AddMessage(string.Format("{0}: {1}", userName, text));
        }

        private void Client_CustomMessagesService_ReceiveMessage(CustomMessagesClientNetworkService sender,
            string message, string data)
        {
            if (message == "Lobby_MapName")
            {
                //update map name on client
                listBoxMaps.Items.Clear();
                listBoxMaps.Items.Add(new MapItem(data, false, false));
                listBoxMaps.SelectedIndex = 0;
            }

            if (message == "Lobby_AllowToConnectDuringGame")
            {
                //update AllowToConnectDuringGame check box on client
                checkBoxAllowToConnectDuringGame.Checked = bool.Parse(data);
            }
        }

        private void checkBoxShowAIMaps_CheckedChange(CheckBox sender)
        {
            if (GameNetworkServer.Instance != null)
            {
                //dynamic map example
                //listBoxMaps.Items.Add(new MapItem(dynamicMapExampleText, false));
                //if (lastMapName == dynamicMapExampleText)
                //    listBoxMaps.SelectedIndex = listBoxMaps.Items.Count - 1;

                listBoxMaps.Items.Clear();

                string[] mapList = VirtualDirectory.GetFiles("", "*.map",
                    SearchOption.AllDirectories);

                foreach (string mapName in mapList)
                {
                    //check for network support
                    if (VirtualFile.Exists(string.Format("{0}\\NoNetworkSupport.txt",
                        Path.GetDirectoryName(mapName))))
                    {
                        continue;
                    }

                    if (mapName.Contains("Demo"))
                        continue;

                    if (checkBoxShowAIMaps.Checked == false && mapName.Contains("AI"))
                        continue;

                    bool recommended = mapName.Contains("AKMaps");
                    //mapName.Contains( "JigsawPuzzleGame" ) || mapName.Contains( "TankDemo" ) ||
                    //mapName.Contains( "DeathmatchDemo" );

                    listBoxMaps.Items.Add(new MapItem(mapName, recommended, false));
                    if (mapName == lastMapName)
                        listBoxMaps.SelectedIndex = listBoxMaps.Items.Count - 1;
                }

                listBoxMaps.SelectedIndexChange += listBoxMaps_SelectedIndexChange;

                if (listBoxMaps.Items.Count != 0 && listBoxMaps.SelectedIndex == -1)
                    listBoxMaps.SelectedIndex = 0;
            }
            else
            {
                listBoxMaps.Enable = false;
            }
        }

        private void Exit_Click(Button sender)
        {
            GameEngineApp.Instance.Server_DestroyServer("The server has been destroyed");
            GameEngineApp.Instance.Client_DisconnectFromServer();

            //close this window
            SetShouldDetach();

            //create MainMenuWindow if not already exists (when we connected to server).
            if (MainMenuWindow.Instance == null)
                GameEngineApp.Instance.ControlManager.Controls.Add(new MainMenuWindow());
        }

        private void SayChatMessage()
        {
            string text = editBoxChatMessage.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.ChatService.SayToAll(text);

            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
                client.ChatService.SayToAll(text);

            editBoxChatMessage.Text = "";
        }

        private string SelectedMapName
        {
            get
            {
                MapItem mapItem = listBoxMaps.SelectedItem as MapItem;
                if (mapItem == null)
                    return null;
                return mapItem.mapName;
            }
        }

        private void listBoxMaps_SelectedIndexChange(ListBox sender)
        {
            lastMapName = SelectedMapName;

            //send map name to clients
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.CustomMessagesService.SendToAllClients("Lobby_MapName", SelectedMapName);
        }

        private void checkBoxAllowToConnectDuringGame_CheckedChange(CheckBox sender)
        {
            //send AllowToConnectDuringGame to clients
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
            {
                server.CustomMessagesService.SendToAllClients("Lobby_AllowToConnectDuringGame",
                    checkBoxAllowToConnectDuringGame.Checked.ToString());
            }
        }

        private void UpdateControls()
        {
            if (GameNetworkServer.Instance != null)
                buttonStart.Enable = !string.IsNullOrEmpty(SelectedMapName);
        }

        private void Start_Click(Button sender)
        {
            if (string.IsNullOrEmpty(SelectedMapName))
                return;

            GameNetworkServer server = GameNetworkServer.Instance;

            //AllowToConnectDuringGame
            server.AllowToConnectNewClients = checkBoxAllowToConnectDuringGame.Checked;

            if (SelectedMapName == exampleOfProceduralMapCreationText)
                GameEngineApp.Instance.SetNeedRunExampleOfProceduralMapCreation();
            else
                GameEngineApp.Instance.SetNeedMapLoad(SelectedMapName);
        }
    }
}