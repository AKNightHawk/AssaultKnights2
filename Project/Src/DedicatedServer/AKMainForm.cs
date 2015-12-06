// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Networking;
using ProjectCommon;
using ProjectEntities;
using WinFormsAppFramework;

namespace DedicatedServer
{
    public partial class AKMainForm : Form
    {
        [Config("DedicatedServer", "lastMapName")]
        private static string lastMapName = "Demos\\JigsawPuzzleGame\\Map\\Map.map";//Jigsaw puzzle by default

        [Config("DedicatedServer", "loadMapAtStartup")]
        private static bool loadMapAtStartup;

        [Config("DedicatedServer", "allowCustomClientCommands")]
        private static bool allowCustomClientCommands = true;

        private static string mapname;

        [Config("DedicatedServer", "serverName")]
        private static string serverName = "Assault Knights Server ((NA)Build \\$1.\\$2.\\$3.\\$4.\\$5.\\$6.\\$7.\\%8=7)";//, NAVERSION , NASUBVERSION1, NASUBVERSION2, MODVERSION, SUBMOD, SUBMOD, SUBMOD, COUNT

        //
        [Config("DedicatedServer", "serverPassword")]
        private static string serverPassword = "";

        [Config("DedicatedServer", "port")]
        private static int port = 65533;

        [Config("DedicatedServer", "PrivateServer")]
        private static bool makePrivate = false;

        [Config("DedicatedServer", "serverTime")]
        private static int serverTime = 30;

        [Config("DedicatedServer", "MapLoop")]
        private List<string> maploop = new List<string>();

        [Config("DedicatedServer", "CoopMapSequence")]
        private List<string> CoopMapSequence = new List<string>();

        [Config("DedicatedServer", "GameType")]
        private List<string> GameTypes = new List<string>();

        private static bool servercreated = false;
        private static bool maploaded = false;
        private static float currentmaptime = 0;

        public AKMainForm()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Program.AKsqlcon.Close();
            SqlRemove();
            servercreated = false;
            maploaded = false;
            Close();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            Create();
            PortTextBox.Enabled = false;
            servercreated = true;
            maploaded = true;
        }

        private void buttonDestroy_Click(object sender, EventArgs e)
        {
            SqlRemove();
            Destroy();
            PortTextBox.Enabled = true;
            servercreated = false;
            maploaded = false;
        }

        private void SqlRemove()
        {
            if (string.IsNullOrEmpty(mapname))
                return;

            try
            {
                string URL;
                string ip;
                ip =
                URL = "http://www.expertctf.net/AK/AKserverincom.php" + "?port=" + port.ToString() + "&mapname=" + mapname + "&add=remove";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader input = new StreamReader(response.GetResponseStream());

                DataSet dsTest = new DataSet();
                dsTest.ReadXml(input);

                int i, j, varTotCol = dsTest.Tables[0].Columns.Count, varTotRow = dsTest.Tables[0].Rows.Count;
                for (j = 0; j < varTotRow; j++)
                {
                    for (i = 0; i < varTotCol; i++)
                    {
                        Log("removed from AKMainServer: " + dsTest.Tables[0].Columns[i].ToString() + ": " + dsTest.Tables[0].Rows[j].ItemArray[i].ToString());
                    }
                }
                servercreated = false;
                maploaded = false;
            }
            catch (Exception Except)
            {
                MessageBox.Show(Except.ToString());
            }
        }

        private void SqlUpdate()
        {
            try
            {
                int privateserver;
                if (checkPrivateServer.Checked == true)
                    privateserver = 1;
                else
                    privateserver = 0;

                string URL;
                URL = "http://www.expertctf.net/AK/AKserverincom.php" + "?port=" + port.ToString() + "&mapname=" + mapname.ToString() + "&servername=" + serverName.ToString() + "&spd=" + serverPassword.ToString() + "&serverprivate=" + privateserver.ToString() + "&add=update";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader input = new StreamReader(response.GetResponseStream());

                DataSet dsTest = new DataSet();
                dsTest.ReadXml(input);

                int i, j, varTotCol = dsTest.Tables[0].Columns.Count, varTotRow = dsTest.Tables[0].Rows.Count;
                for (j = 0; j < varTotRow; j++)
                {
                    for (i = 0; i < varTotCol; i++)
                    {
                        Log("Sent to AKMainServer Updated: " + dsTest.Tables[0].Columns[i].ToString() + ": " + dsTest.Tables[0].Rows[j].ItemArray[i].ToString());
                    }
                }
                servercreated = true;
                maploaded = true;
            }
            catch (Exception Except)
            {
                MessageBox.Show(Except.ToString());
            }
        }

        private void SqlAdd()
        {
            try
            {
                int privateserver;
                if (checkPrivateServer.Checked == true)
                    privateserver = 1;
                else
                    privateserver = 0;

                string URL;
                URL = "http://www.expertctf.net/AK/AKserverincom.php" + "?port=" + port.ToString() + "&mapname=" + mapname.ToString() + "&servername=" + serverName.ToString() + "&spd=" + serverPassword.ToString() + "&serverprivate=" + privateserver.ToString() + "&add=add";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader input = new StreamReader(response.GetResponseStream());

                DataSet dsTest = new DataSet();
                dsTest.ReadXml(input);

                int i, j, varTotCol = dsTest.Tables[0].Columns.Count, varTotRow = dsTest.Tables[0].Rows.Count;
                for (j = 0; j < varTotRow; j++)
                {
                    for (i = 0; i < varTotCol; i++)
                    {
                        Log("Sent to AKMainServer: " + dsTest.Tables[0].Columns[i].ToString() + ": " + dsTest.Tables[0].Rows[j].ItemArray[i].ToString());
                    }
                }
                servercreated = true;
                maploaded = true;
            }
            catch (Exception Except)
            {
                MessageBox.Show(Except.ToString());
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //NeoAxis initialization
            EngineApp.ConfigName = "user:Configs/DedicatedServer.config";
            EngineApp.ReplaceRenderingSystemComponentName = "RenderingSystem_NULL";
            EngineApp.ReplaceSoundSystemComponentName = "SoundSystem_NULL";
            if (!WinFormsAppWorld.Init(new WinFormsAppEngineApp(EngineApp.ApplicationTypes.Simulation), this,
                "user:Logs/DedicatedServer.log", true, null, null, null, null))
            {
                Close();
                return;
            }
            WinFormsAppEngineApp.Instance.AutomaticTicks = false;

            Engine.Log.Handlers.InfoHandler += delegate(string text, ref bool dumpToLogFile)
            {
                Log("Log: " + text);
            };

            Engine.Log.Handlers.ErrorHandler += delegate(string text, ref bool handled, ref bool dumpToLogFile)
            {
                handled = true;
                timer1.Stop();
                MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                timer1.Start();
            };

            Engine.Log.Handlers.FatalHandler += delegate(string text, string createdLogFilePath,
                ref bool handled)
            {
                handled = true;
                timer1.Stop();
                MessageBox.Show(text, "Fatal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };

            //register config fields of this class
            EngineApp.Instance.Config.RegisterClassParameters(GetType());

            //generate map list
            {
                string[] mapList = VirtualDirectory.GetFiles("", "*.map", SearchOption.AllDirectories);
                foreach (string mapName in mapList)
                {
                    //check for network support
                    if (VirtualFile.Exists(string.Format("{0}\\NoNetworkSupport.txt",
                        Path.GetDirectoryName(mapName))))
                    {
                        Log("File has no Network support: " + mapName + ", removing from list");
                        continue;
                    }

                    if (mapName.Contains("AKMaps")) //filter AK maps -- Incin
                    {
                        comboBoxMaps.Items.Add(mapName);
                    }
                    else
                    {
                        Log("Not Assault Knights Map, filtered " + mapName);
                    }

                    if (mapName == lastMapName)
                        comboBoxMaps.SelectedIndex = comboBoxMaps.Items.Count - 1;
                }

                comboBoxMaps.SelectedIndexChanged += comboBoxMaps_SelectedIndexChanged;
            }

            checkBoxLoadMapAtStartup.Checked = loadMapAtStartup;
            checkPrivateServer.Checked = makePrivate;
            checkBoxAllowCustomClientCommands.Checked = allowCustomClientCommands;

            serverName = textServerName.Text.Trim();
            serverTime = int.Parse(ntbMapTime.Text);
            port = int.Parse(PortTextBox.Text);

            servercreated = false;
            maploaded = false;

            //load map at startup
            if (loadMapAtStartup && comboBoxMaps.SelectedItem != null)
            {
                Create();
                string mapName = comboBoxMaps.SelectedItem as string;
                if (!MapLoad(mapName))
                    return;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Destroy();
            SqlRemove();

            //NeoAxis shutdown
            WinFormsAppWorld.Shutdown();
        }

        private string GetMapLoopFromConfig()
        {
            //EngineApp.Instance.Config.Parameters.
            string error;
            TextBlock block = TextBlockUtils.LoadFromVirtualFile("user:Configs/DedicatedServer.config", out error);
            if (block != null)
                return block.GetAttribute("MapLoop");
            return "";
        }

        private void Create()
        {
            if (GameNetworkServer.Instance != null)
            {
                Log("Error: Server already created");
                return;
            }

            if (serverName == "" || serverName == null)
            {
                Log("Set Server Name first, then we can add your server");
                return;
            }
            else
            {
                serverName = textServerName.Text.ToString();
            }

            if (PortTextBox.Text == "" || PortTextBox.Text == null)
            {
                Log("Invalid server port set, please set 1 to 65535");
                return;
            }
            else
            {
                port = int.Parse(PortTextBox.Text);
            }

            GameNetworkServer server = new GameNetworkServer(serverName,
                EngineVersionInformation.Version, 128, serverPassword, true);

            server.UserManagementService.AddUserEvent += UserManagementService_AddUserEvent;
            server.UserManagementService.RemoveUserEvent += UserManagementService_RemoveUserEvent;
            server.ChatService.ReceiveText += ChatService_ReceiveText;
            server.CustomMessagesService.ReceiveMessage += CustomMessagesService_ReceiveMessage;
           // server.CustomMessagesService.ReceiveMessage += SpawnInfo;

            string error;
            if (!server.BeginListen(port, out error))
            {
                Log("Error: " + error);
                Destroy();
                servercreated = false;
                maploaded = false;
                return;
            }
            else
            {
                servercreated = true;
            }

            //load map at startup

            if (comboBoxMaps.SelectedItem != null)
            {
                //Create();

                string mapName = comboBoxMaps.SelectedItem as string;

                if (!MapLoad(mapName))
                    return;

                mapname = mapName;

                //if (makePrivate == false)
                //{
                //    Log("Server has been made public.");
                //}
                //else
                //{
                //    Log("Server has been set private.");
                //}

                SqlAdd();

                if (Program.AKsqlcon.State.ToString() == "Open")
                {
                    SQLCon.Text = "Connection Active"; ;
                    SQLCon.BackColor = Color.LightGreen;
                }
                else
                {
                    SQLCon.Text = "Connection Lost"; ;
                    SQLCon.BackColor = Color.Red;
                }
            }

            Log("Server has been created");
            Log("Listening port {0}...", port);

            buttonCreate.Enabled = false;
            buttonDestroy.Enabled = true;
            buttonMapLoad.Enabled = true;
            checkPrivateServer.Enabled = false;
            ntbMapTime.Enabled = false;
        }

        private bool MapLoad(string fileName)
        {
            MapDestroy(false);

            Log("Loading map \"{0}\"...", fileName);

            WorldType worldType = EntitySystemWorld.Instance.DefaultWorldType;

            GameNetworkServer server = GameNetworkServer.Instance;
            if (!EntitySystemWorld.Instance.WorldCreate(WorldSimulationTypes.DedicatedServer,
                worldType, server.EntitySystemService.NetworkingInterface))
            {
                Log("Error: EntitySystemWorld.Instance.WorldCreate failed.");
                return false;
            }

            if (!MapSystemWorld.MapLoad(fileName))
            {
                MapDestroy(false);
                onMapEnd();
                return false;
            }
            else
            {
                onMapStart();
            }

            //run simulation
            EntitySystemWorld.Instance.Simulation = true;

            GameNetworkServer.Instance.EntitySystemService.WorldWasCreated();

            Log("Map loaded");

            buttonMapLoad.Enabled = false;
            buttonMapUnload.Enabled = true;
            buttonMapChange.Enabled = true;
            checkPrivateServer.Enabled = false;
            ntbMapTime.Enabled = false;

            return true;
        }

        private void onMapEnd()
        {
            maploaded = false;
        }

        private void onMapStart()
        {
            maploaded = true;
        }

        private void onServerCreated()
        {
            servercreated = true;
        }

        private void onServerDestroyed()
        {
            servercreated = false;
        }

        private void MapDestroy(bool newMapWillBeLoaded)
        {
            bool mapWasDestroyed = Map.Instance != null;

            MapSystemWorld.MapDestroy();

            if (EntitySystemWorld.Instance != null)
                EntitySystemWorld.Instance.WorldDestroy();

            if (mapWasDestroyed)
            {
                GameNetworkServer.Instance.EntitySystemService.WorldWasDestroyed(newMapWillBeLoaded);
                onMapEnd();
                Log("Map destroyed");
            }

            buttonMapLoad.Enabled = true;
            buttonMapUnload.Enabled = false;
            buttonMapChange.Enabled = false;
            checkPrivateServer.Enabled = true;
            ntbMapTime.Enabled = true;
        }

        private void Destroy()
        {
            MapDestroy(false);
            onMapEnd();

            if (GameNetworkServer.Instance != null)
            {
                GameNetworkServer.Instance.Dispose("The server has been destroyed");

                onServerDestroyed();
                buttonCreate.Enabled = true;
                buttonDestroy.Enabled = false;
                buttonMapLoad.Enabled = false;
                buttonMapChange.Enabled = false;
                buttonMapUnload.Enabled = false;
                checkPrivateServer.Enabled = true;
                ntbMapTime.Enabled = true;
                listBoxUsers.Items.Clear();

                Log("Server destroyed");
            }
        }

        private void Log(string text, params object[] args)
        {
            while (listBoxLog.Items.Count > 300)
                listBoxLog.Items.RemoveAt(0);
            int index = listBoxLog.Items.Add(string.Format(text, args));
            listBoxLog.SelectedIndex = index;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.Update();

            if (WinFormsAppEngineApp.Instance != null)
            {
                WinFormsAppEngineApp.Instance.DoTick();
                currentmaptime += timer1.Interval;
            }
        }

        private void UserManagementService_AddUserEvent(UserManagementServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo user)
        {
            Log("User connected: " + user.ToString());
            listBoxUsers.Items.Add(user);
        }

        private void UserManagementService_RemoveUserEvent(UserManagementServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo user)
        {
            listBoxUsers.Items.Remove(user);
            Log("User disconnected: " + user.ToString());
        }

        private void ChatService_ReceiveText(ChatServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo fromUser, string text,
            UserManagementServerNetworkService.UserInfo privateToUser)
        {
            string userName = fromUser != null ? fromUser.Name : "(null)";
            string toUserName = privateToUser != null ? privateToUser.Name : "All";
            Log("Chat: {0} -> {1}: {2}", userName, toUserName, text);
        }

        private void comboBoxMaps_SelectedIndexChanged(object sender, EventArgs e)
        {
            lastMapName = comboBoxMaps.SelectedItem as string;
            Log("Map changed to: " + lastMapName + " or first map loading");
        }

        //private void buttonDoSomething_Click(object sender, EventArgs e)
        //{
        //    MessageBox.Show("You can write code for testing here.", "Warning");

        //    //example
        //    //MapObject mapObject = (MapObject)Entities.Instance.Create( "Box", Map.Instance );
        //    //mapObject.Position = new Vec3( 0, 0, 30 );
        //    //mapObject.PostCreate();
        //}

        private void checkBoxLoadMapAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            loadMapAtStartup = checkBoxLoadMapAtStartup.Checked;
        }

        private void buttonMapLoad_Click(object sender, EventArgs e)
        {
            string mapName = comboBoxMaps.SelectedItem as string;
            if (string.IsNullOrEmpty(mapName))
            {
                Log("Error: No map selected");
                return;
            }

            if (!MapLoad(mapName))
                return;
            else
                Log("Map: " + mapName + " Loaded !!");

            //currentmaptime = 0;
        }

        private void buttonMapUnload_Click(object sender, EventArgs e)
        {
            MapDestroy(false);
        }

        private void buttonMapChange_Click(object sender, EventArgs e)
        {
            MapDestroy(true);

            string mapName = comboBoxMaps.SelectedItem as string;
            if (string.IsNullOrEmpty(mapName))
            {
                Log("Error: No map selected");
                return;
            }

            if (!MapLoad(mapName))
            {
                Log("Server: Map Change failed, what the ...");
                return;
            }
            else
            {
                SqlUpdate();
            }
        }

        public static void SpawnInfo(CustomMessagesServerNetworkService sender,
            NetworkNode.ConnectedNode info, string message, string data)
        {
            if (message == "spawnInfoToServer")
            {
                string[] parameters = data.Split(';');
                string userid = parameters[0];
                string selectedspawnid = parameters[1];
                string selectedfaction = parameters[2];
               


                foreach (ProjectCommon.UserManagementServerNetworkService.UserInfo info2 in GameNetworkServer.Instance.UserManagementService.Users)
                {
                    if (info2.Name.Equals(userid))
                    {
                        GameWorld.Instance.AKServerOrSingle_CreatePlayerUnit(PlayerManager.Instance.ServerOrSingle_GetPlayer(userid), target, selectedfaction);
                        GameNetworkServer.Instance.CustomMessagesService.SendToClient(info, message, data);
                        //GameEngineApp.Instance.CreateGameWindowForMap();
                    }
                }
            }
        }

        private void CustomMessagesService_ReceiveMessage(CustomMessagesServerNetworkService sender,
            NetworkNode.ConnectedNode source, string message, string data)
        {
            //Warning! Messages must be checked by security reasons.
            //Modified client application can send any message with any data.
            SpawnInfo(sender, source, message, data);

            if (allowCustomClientCommands)
            {
                //load map
                if (message == "Example_MapLoad")
                {
                    string mapName = data;
                    MapDestroy(true);
                    if (!MapLoad(mapName))
                        return;
                    return;
                }

                //create map object
                if (message == "Example_CreateMapObject")
                {
                    string[] parameters = data.Split(';');
                    string typeName = parameters[0];
                    Vec3 position = Vec3.Parse(parameters[1]);
                    Quat rotation = Quat.Parse(parameters[2]);

                    if (Map.Instance != null)
                    {
                        MapObject entity = (MapObject)Entities.Instance.Create(typeName, Map.Instance);
                        entity.Position = position;
                        entity.Rotation = rotation;
                        entity.PostCreate();
                    }

                    return;
                }
            }
        }

        private void checkBoxAllowCustomClientCommands_CheckedChanged(object sender, EventArgs e)
        {
            allowCustomClientCommands = checkBoxAllowCustomClientCommands.Checked;
            if (allowCustomClientCommands == true)
            {
                Log("Server will allow custom client commands.");
            }
            else
            {
                Log("Server will not allow custom client commands.");
            }
        }

        private void PortTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+"))
                e.Handled = true;
        }

        private void checkPrivateServer_CheckedChanged(object sender, EventArgs e)
        {
            if (checkPrivateServer.Checked)
            {
                makePrivate = true;
                Log("Server will be made Private");
            }
            else
            {
                makePrivate = false;
                Log("Server will be made Public");
            }
        }

        private void textServerName_TextChanged(object sender, EventArgs e)
        {
            if (textServerName.Text.Length > 0)
            {
                serverName = textServerName.Text.ToString();
            }
            else
            {
                Log("Server name must not be empty, setting default");
                serverName = "Assault Knights Server 1.32.050";
                textServerName.Text = serverName;
            }
        }

        private void ntbMapTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+"))
            {
                if (PortTextBox.Text != "" || PortTextBox != null)
                {
                    int temp = int.Parse(PortTextBox.Text);
                    if (temp > 0 && temp <= 65535)
                        port = int.Parse(PortTextBox.Text);
                    else
                    {
                        Log("Port Error: Server Port must be between 0 and 65535, setting to defaults");
                        PortTextBox.Text = "65535";
                    }
                }
                else
                {
                    Log("Port Error: Numbers only, between 0 and 65535");
                }
                e.Handled = true;
            }
            else
            {
                Log("Invalid Key");
            }
        }

        private void PortTextBox_TextChanged(object sender, EventArgs e)
        {
            if (PortTextBox.Text != "" || PortTextBox != null)
            {
                int temp = int.Parse(PortTextBox.Text);
                if (temp > 0 && temp <= 65535)
                {
                    port = int.Parse(PortTextBox.Text);
                    Log("Port set to: Server Port: " + port);
                }
                else
                {
                    Log("Port Error: Server Port must be between 0 and 65535, setting to defaults");
                    PortTextBox.Text = "65533";
                }
            }
            else
            {
                Log("Port Error: Numbers only, between 0 and 65535");
            }
        }

        private void ntbMapTime_TextChanged(object sender, EventArgs e)
        {
            if (ntbMapTime.Text != "" || ntbMapTime != null)
            {
                int temp = int.Parse(ntbMapTime.Text);
                if (temp > 0)
                {
                    serverTime = int.Parse(ntbMapTime.Text);
                    Log("Map Timer: Set to " + serverTime + " minutes");
                }
                else
                {
                    Log("Map Timer: must be greater then 0, setting to 30");
                    ntbMapTime.Text = "30";
                }
            }
            else
            {
                Log("Map Timer Error: Numbers only, greater then really 15?");
            }
        }

        private void btnCommand_Click(object sender, EventArgs e)
        {
            //load list of game or server commands
            if (textBoxCommands.Text != "" || textBoxCommands.Text != null)
            {
                try
                {
                    Log("Server Command: " + textBoxCommands.Text);
                }
                catch
                {
                    Log("Invalid server Command: " + textBoxCommands.Text + "; Please try again");
                }
            }
            else
            {
                Log("server command: Nothing to do, write something");
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
        }

        private void textBoxServerPassword_TextChanged(object sender, EventArgs e)
        {
            if (textBoxServerPassword.Text.Length > 0)
            {
                serverPassword = textBoxServerPassword.Text.ToString();
                Log("Server Password has been set");
            }
            else
            {
                Log("Server Password is empty, no password will be set on server");
                textBoxServerPassword.Text = serverPassword = "";
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            maploop.Clear();
            listMaploop.Items.Clear();
            Log("Cleared Map Loop configuration");
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listMaploop.Items.Count > 0 && listMaploop.SelectedIndex != -1)
            {
                // int x = listMaploop.SelectedIndex;
                if (listMaploop.Text.Equals(listMaploop.SelectedItem.ToString()))
                {
                    Log("Removing " + listMaploop.Text.ToString() + " from Map Loop");
                    listMaploop.Items.RemoveAt(listMaploop.SelectedIndex);
                    maploop.Remove(listMaploop.Text.ToString());
                }
            }
        }

        private void BtnAddMap_Click(object sender, EventArgs e)
        {
            if (comboBoxMaps.SelectedIndex != -1)
            {
                listMaploop.Items.Add(comboBoxMaps.Text.ToString());
                maploop.Add(comboBoxMaps.Text.ToString());
                Log("Added " + comboBoxMaps.Text.ToString() + " to Map Loop");
            }
        }

        private void label9_Click(object sender, EventArgs e)
        {
        }

        private void label5_Click(object sender, EventArgs e)
        {
        }

        private void label10_Click(object sender, EventArgs e)
        {
        }
    }
}