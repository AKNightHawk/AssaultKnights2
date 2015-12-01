// Copyright (C) 2006-2010 Dark Realm Game Studio, Msg_gol Assault Knights Project
using System;
using System.Collections.Generic;
using Engine;
using Engine.MathEx;
using Engine.Networking;
using Engine.UISystem;
//using MySql.Data.MySqlClient;
using ProjectCommon;

//using System.Windows.Forms;

namespace Game
{
    /// <summary>
    /// Defines a about us window.
    /// </summary>
    public class MPAKlobbyWindow : Control
    {
        private bool notDisposeClientOnDetach;

        private class AKservers
        {
            internal int ID;
            internal string Mapname;
            internal string Servername;
            internal string Version;
            internal int Port;
            internal string IP;
            internal string ServerPassword;
            internal int ServerPrivate;
        }

        private List<AKservers> AKSERVERS = new List<AKservers>();

        //private bool Go;
        //private float maxout = -800;
        //private float maxin = 0f;
        //private float Rate = 1000;
        //private float positionY;
        //private bool done;

        private Control window;
        private ListBox listBox;
        private EditBox createServerPort;

        private Button buttonCreateServer;
        private CheckBox CheckPrivate;
        private PasswordBox PassBox;

        private bool privateserver;
        private string clientpassword = "";
        private string serverpassword = "";
        private string createServerName = "Assault Knights 3.3.1 Server";
        private int createserverport = 65533;

        protected override void OnAttach()
        {
            window = ControlDeclarationManager.Instance.CreateControl("Gui\\MPAKlobbyWindow.gui");
            Controls.Add(window);

            //positionY = maxout;

            listBox = (ListBox)window.Controls["Servers"];

            ((Button)window.Controls["Quit"]).Click += delegate(Button sender)
            {
                SetShouldDetach();// Back(true);
            };

            //positionY = maxout;

            ((Button)window.Controls["TechLab"]).Click += delegate(Button sender)
            {
                this.OnDetach();
                Controls.Add(new TechLabUnitBuyWindow());
            };

            ((Button)window.Controls["Refresh"]).Click += delegate(Button sender)
            {
                refreshlistbox();
            };

            ((Button)window.Controls["Customize"]).Click += delegate(Button sender)
            {
                this.OnDetach();
                Controls.Add(new TechLabUnitCustomizeWindow());
            };

            //Run button event handler
            ((Button)window.Controls["Run"]).Click += delegate(Button sender)
            {
                if (listBox.SelectedIndex != -1)
                    RunMap(AKSERVERS[listBox.SelectedIndex]);
            };

            createServerPort = window.Controls["Serverport"] as EditBox;
            if (createServerPort != null)
            {
                createServerPort.TextChange += delegate(Control sender)
                {
                    createServerPort.TextChange += createServerPort_TextChange;
                };
                createserverport = int.Parse(createServerPort.Text.Trim());
            }

            PassBox = window.Controls["passwordBox"] as PasswordBox;
            if (PassBox != null)
            {
                PassBox.TextChange += delegate(Control sender)
                {
                    PassBox.TextChange += PassBox_TextChange;
                    if (PassBox.Text.Length > 0)
                    {
                        clientpassword = PassBox.Text.Trim();
                        serverpassword = clientpassword;
                    }
                    else
                    {
                        clientpassword = serverpassword = "";
                    }
                };
            }

            CheckPrivate = window.Controls["chkserverPrivate"] as CheckBox;
            if (CheckPrivate != null)
            {
                CheckPrivate.CheckedChange += delegate(CheckBox sender)
                {
                    if (CheckPrivate.Checked == true)
                    {
                        privateserver = true;
                        SetInfo("Server set to private", false);
                    }
                    else
                    {
                        privateserver = false;
                    }
                };
            }

            EditBox IPtext = window.Controls["DirectConnectionTextBox"] as EditBox;

            ((Button)window.Controls["DirectIPCon"]).Click += delegate(Button sender)
            {
                GameNetworkClient client = new GameNetworkClient(true);
                client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
                string error;
                if (!client.BeginConnect(IPtext.Text.ToString(), int.Parse(createserverport.ToString()), EngineVersionInformation.Version,
                    Program.username, clientpassword, out error))
                {
                    Log.Error(error);
                    DisposeClient();
                    return;
                }
            };

            ((Button)window.Controls["singlePlayer"]).Click += delegate(Button sender)
            {
                GameEngineApp.Instance.SetNeedMapLoad("Maps\\MainDemo\\Map.map");
            };

            //TextBox ComWelN = window.Controls["ComWelNote"] as TextBox;
            //if (ComWelN != null)
            //{
            //    ComWelN.Text = "Welcome Commander " + MySqlGetUserName();
            //}

            buttonCreateServer = (Button)window.Controls["CreateServer"];
            buttonCreateServer.Click += CreateServer_Click;

            //refreshlistbox();

            base.OnAttach();
        }

        private void CreateServer_Click(Button sender)
        {
            //if (string.IsNullOrEmpty(serverUserName))
            if (string.IsNullOrEmpty(Program.username))
            {
                SetInfo("Invalid user name.", true);
                return;
            }

            SetInfo("Creating server...", false);

            GameNetworkServer server = new GameNetworkServer(createServerName.ToString(),
                EngineVersionInformation.Version, 128, true);

            //int port = 229;

            string error;
            if (!server.BeginListen(createserverport, out error))
            {
                SetInfo("Server Listen Error: " + error, true);
                server.Dispose("");
                return;
            }

            //create user for server
            server.UserManagementService.CreateServerUser(Program.username);

            //close all windows
            foreach (Control control in GameEngineApp.Instance.ControlManager.Controls)
                control.SetShouldDetach();
            //create lobby window
            MultiplayerLobbyWindow lobbyWindow = new MultiplayerLobbyWindow();
            GameEngineApp.Instance.ControlManager.Controls.Add(lobbyWindow);

            GameEngineApp.Instance.Server_OnCreateServer();
        }

        private void RunMap(object SelectedItem)
        {
            AKservers selectedserver = SelectedItem as AKservers;
            if (SelectedItem != null)
            {
                GameNetworkClient client = new GameNetworkClient(true);
                client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
                string error;
                if (!client.BeginConnect(selectedserver.IP, selectedserver.Port, EngineVersionInformation.Version,
                    Program.username, serverpassword, out error))
                {
                    Log.Error("Running Map Error: " + error);
                    DisposeClient();
                    return;
                }
            }
        }

        //private void refreshlistbox()
        //{
        //    if (AKSERVERS.Count != 0)
        //        AKSERVERS.Clear();

        //    if (listBox.Items.Count != 0)
        //        listBox.Items.Clear();

        //    try
        //    {
        //        string sql = "SELECT * FROM phpap_AKservers";
        //        MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);
        //        MySqlDataReader rdr = cmd.ExecuteReader();

        //        while (rdr.Read())
        //        {
        //            AKservers readerserver = new AKservers();
        //            readerserver.ID = (int)rdr["ServerID"];
        //            readerserver.Servername = rdr["ServerName"].ToString();
        //            readerserver.IP = rdr["ServerIP"].ToString();
        //            readerserver.Mapname = rdr["ServerMapname"].ToString();
        //            readerserver.Port = (int)rdr["ServerPort"];
        //            readerserver.ServerPassword = rdr["ServerPassword"].ToString();
        //            readerserver.ServerPrivate = (int)rdr["ServerPrivate"];
        //            readerserver.Version = "V 3.3.1.1"; //iNCIN AK version

        //            //if (readerserver.ServerPrivate == 0) //server is set to private, don't show
        //            //{
        //            AKSERVERS.Add(readerserver);
        //            //}
        //        }

        //        rdr.Close();
        //        rdr.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex != null)
        //        {
        //            EngineConsole.Instance.Print("No servers found \n");
        //            EngineConsole.Instance.Print(ex.Message);
        //            return;
        //        }
        //    }

        //    foreach (AKservers AKserveritem in AKSERVERS)
        //    {
        //        if (AKserveritem.ServerPrivate == 0)
        //        {
        //            listBox.Items.Add(GetLobbyServerText(AKserveritem));
        //        }
        //    }
        //}

        private String GetLobbyServerText(AKservers AKSitem)
        {
            char[] delimiterChars = { '\\' };
            string[] words = AKSitem.Mapname.Split(delimiterChars);
            string mapnameClean = words[1].ToString();
            string Space = "||";

            string final = AKSitem.ID.ToString() + Space + AKSitem.Servername.ToString() + ":" + AKSitem.Port.ToString() + Space + mapnameClean;
            //AKSitem.ID.ToString() + Space + mapnameClean + Space + AKSitem.Version.ToString();

            return final;
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;
            if (e.Key == EKeys.Escape)
            {
                SetShouldDetach();//Back(true);
                return true;
            }
            return false;
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            //Back(false);

            //if (!Go)
            //{
            //    // slide window in
            //    if (positionY < maxin)
            //    {
            //        positionY += (delta * Rate);
            //    }
            //    else
            //    {
            //        positionY = maxin;
            //    }
            //}
            //else
            //{
            //    // slide window out
            //    if (positionY > maxout)
            //    {
            //        positionY -= (delta * Rate);
            //    }
            //    else
            //    {
            //        positionY = maxout;
            //    }
            //}

            //window.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(-30, positionY));
        }

        //private void shouldgo()
        //{
        //    Go = true;
        //}

        //private string MySqlGetUserName()
        //{
        //    try
        //    {
        //        string commandername = "Unknown";
        //        string sql = "SELECT Name FROM phpap_AKusers WHERE Username=@User";
        //        MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

        //        MySqlParameter User = new MySqlParameter();
        //        User.ParameterName = "@User";
        //        User.Value = Program.username;
        //        cmd.Parameters.Add(User);

        //        MySqlDataReader rdr = cmd.ExecuteReader();
        //        while (rdr.Read())
        //        {
        //            if (rdr[0] != null)
        //            {
        //                commandername = rdr[0].ToString();
        //            }
        //        }

        //        if (commandername != "Unknown")
        //        {
        //            rdr.Close();
        //            rdr.Dispose();
        //        }
        //        return commandername;
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex != null)
        //        {
        //            EngineConsole.Instance.Print(ex.Message);
        //            return "Unknown";
        //        }
        //    }
        //    return "Unknown";
        //}

        private void Client_ConnectionStatusChanged(NetworkClient sender, NetworkConnectionStatuses status)
        {
            switch (status)
            {
                case NetworkConnectionStatuses.Disconnected:
                    {
                        string text = "Unable to connect to Server:";
                        if (sender.DisconnectionReason != "")
                            text += ". " + sender.DisconnectionReason;
                        SetInfo(text, true);

                        DisposeClient();
                    }
                    break;

                case NetworkConnectionStatuses.Connecting:
                    SetInfo("Connecting to Server...", false);
                    break;

                case NetworkConnectionStatuses.Connected:
                    SetInfo("Connected to Server", false);

                    //no work with client from this class anymore
                    RemoveEventsForClient();
                    notDisposeClientOnDetach = true;

                    //close all windows
                    foreach (Control control in GameEngineApp.Instance.ControlManager.Controls)
                        control.SetShouldDetach();
                    //create lobby window
                    //MultiplayerLobbyWindow lobbyWindow = new MultiplayerLobbyWindow();
                    GameEngineApp.Instance.ControlManager.Controls.Add(new MultiplayerLobbyWindow());

                    GameEngineApp.Instance.Client_OnConnectedToServer();

                    break;
            }
        }

        private void SetInfo(string text, bool error)
        {
            TextBox textBoxInfo = (TextBox)window.Controls["Info"];

            textBoxInfo.Text = text;
            textBoxInfo.TextColor = error ? new ColorValue(1, 0, 0) : new ColorValue(1, 1, 1);
        }

        private void DisposeClient()
        {
            RemoveEventsForClient();

            if (GameNetworkClient.Instance != null)
                GameNetworkClient.Instance.Dispose();
        }

        private void RemoveEventsForClient()
        {
            if (GameNetworkClient.Instance != null)
                GameNetworkClient.Instance.ConnectionStatusChanged -= Client_ConnectionStatusChanged;
        }

        //private void Back(bool reallygo)
        //{
        //    if (reallygo)
        //        shouldgo();

        //    if (positionY == maxout && Go && !done)
        //    {
        //        if (Map.Instance.Name.ToString() == "MainMap")
        //        {
        //            this.OnDetach();
        //            //Controls.Add(new MainMenuWindow());
        //            MainMenuWindow.Instance.Go = false;
        //            done = true;
        //        }
        //        else
        //        {
        //            SetShouldDetach();
        //        }
        //    }
        //}

        protected override bool OnKeyPress(KeyPressEvent e)
        {
            return base.OnKeyPress(e);
        }

        protected override void OnDetach()
        {
            if (!notDisposeClientOnDetach)
                DisposeClient();

            //restore check for disconnection flag
            GameEngineApp.Instance.Client_AllowCheckForDisconnection = true;
            SetShouldDetach(); //fixes akunit buy and customize
            base.OnDetach();
        }

        private void createServerPort_TextChange(Control sender)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(createServerPort.Text.ToString(), "\\d+"))
            {
                int temp = int.Parse(createServerPort.Text.Trim());
                if (temp > 0 && temp <= 65535)
                {
                    createserverport = temp;
                    SetInfo("Valid port set, Using " + temp, false);
                }
                else
                {
                    createServerPort.Text = "";
                    SetInfo("invalid port info, must be 1 to 65535", true);
                }
            }
            else
            {
                createServerPort.Text = "";
                SetInfo("invalid port info, must be 1 to 65535", true);
            }
        }

        private void createServerPorttext_KeyPress(KeyPressEvent e)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), "\\d+"))
            {
                SetInfo("Invalid port, try a numbers value > 0 and less then 65535", true);
            }
        }

        //\d{2}[a-zA-Z0-9](-\d{3}){2}[A-Za-z0-9]$
        private void PassBox_TextChange(Control sender)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(PassBox.Text.ToString(), "^[a-zA-Z0-9]$"))
            {
                SetInfo("Invalid characters in password, use a-,z, A-Z, 0-9", true);
                PassBox.Text = "";
            }
            else
            {
                clientpassword = PassBox.Text.Trim();
                serverpassword = PassBox.Text.Trim();
                SetInfo("password is set", false);
            }
        }
    }
}