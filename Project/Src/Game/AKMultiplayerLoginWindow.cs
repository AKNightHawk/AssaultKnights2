// Copyright (C) 2006-2013 NeoAxis Group Ltd.
using System;
using Engine;
using Engine.MathEx;
using Engine.UISystem;
using Engine.Utils;
using MySql.Data.MySqlClient;
using ProjectCommon;

namespace Game
{
    public class AKMultiplayerLoginWindow : Control
    {
        [Config("Multiplayer", "UserName")]
        private static string userName;

        [Config("Multiplayer", "Password")]
        private static string password;

        //private bool Go = false;
        //private float maxout = -800;
        //private float maxin = 0f;
        //private float Rate = 1000;
        //private float positionY;
        //private bool done;

        private Control window;
        private EditBox editBoxUserName;
        private PasswordBox editBoxUserPass;
        private Button buttonConnect;
        private CheckBox savePass;

        ///////////////////////////////////////////

        protected override void OnAttach()
        {
            base.OnAttach();

            //disable check for disconnection
            GameEngineApp.Instance.Client_AllowCheckForDisconnection = false;

            //register config fields
            EngineApp.Instance.Config.RegisterClassParameters(GetType());

            //create window
            window = ControlDeclarationManager.Instance.CreateControl(
                "Gui\\MultiplayerLoginWindow.gui");
            Controls.Add(window);

            //MouseCover = true;
            //BackColor = new ColorValue( 0, 0, 0, .5f );

            //initialize controls

            buttonConnect = (Button)window.Controls["Connect"];
            buttonConnect.Click += Connect_Click;

            //( (Button)window.Controls[ "Exit" ] ).Click += Exit_Click;
            //Quit button event handler
            ((Button)window.Controls["Exit"]).Click += delegate(Button sender)
            {
                SetShouldDetach();
                //Back(true);
            };

            //positionY = maxout;

            //generate user name
            if (string.IsNullOrEmpty(userName))
            {
                EngineRandom random = new EngineRandom();
                userName = "Player" + random.Next(1000).ToString("D03");
            }

            savePass = (CheckBox)window.Controls["savePass"];

            editBoxUserPass = (PasswordBox)window.Controls["password"];
            if (!string.IsNullOrEmpty(password))
            {
                editBoxUserPass.Text = password;
                savePass.Checked = true;
            }

            //configure password feature
            editBoxUserPass.UpdatingTextControl = delegate(EditBox sender, ref string text)
            {
                text = new string('*', sender.Text.Length);
                if (sender.Focused)
                    text += "_";
                savePass.Checked = true;
            };

            //editBoxUserPass.TextChange += passwordEditBox_TextChange;

            savePass.CheckedChange += savePass_CheckedChange;

            editBoxUserPass.TextChange += editBoxUserPass_TextChange;

            editBoxUserName = (EditBox)window.Controls["UserName"];
            editBoxUserName.Text = userName;
            editBoxUserName.TextChange += editBoxUserName_TextChange;

            ((Button)window.Controls["Register"]).Click += Register_Click;

            SetInfo("", false);
        }

        private void savePass_CheckedChange(CheckBox sender)
        {
            if (savePass.Checked)
                password = editBoxUserPass.Text.Trim();
            else
                password = string.Empty;
        }

        private void Register_Click(Button sender)
        {
            Controls.Add(new MultiplayerRegisterWindow());
        }

        protected override void OnDetach()
        {
            //restore check for disconnection flag
            GameEngineApp.Instance.Client_AllowCheckForDisconnection = true;
            SetShouldDetach();
            base.OnDetach();
        }

        //private void shouldgo()
        //{
        //    Go = true;
        //}

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

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;

            if (e.Key == EKeys.Escape)
            {
                SetShouldDetach();
                //Back(true);
                return true;
            }

            if (e.Key == EKeys.Enter)
            {
                DoConnect();
                return true;
            }

            return false;
        }

        private void editBoxUserName_TextChange(Control sender)
        {
            userName = editBoxUserName.Text.Trim();
        }

        private void editBoxUserPass_TextChange(Control sender)
        {
            if (savePass.Checked)
                password = editBoxUserPass.Text.Trim();
        }

        private void CreateServer_Click(Button sender)
        {
            if (string.IsNullOrEmpty(userName))
            {
                SetInfo("Invalid user name.", true);
                return;
            }

            SetInfo("Creating server...", false);

            GameNetworkServer server = new GameNetworkServer("Assault Knights Server",
                EngineVersionInformation.Version, 128, "", true);

            int port = 65533;

            string error;
            if (!server.BeginListen(port, out error))
            {
                SetInfo("Error: " + error, true);
                server.Dispose("");
                return;
            }

            //create user for server
            server.UserManagementService.CreateServerUser(userName);

            //close all windows
            foreach (Control control in GameEngineApp.Instance.ControlManager.Controls)
                control.SetShouldDetach();
            //create lobby window
            MultiplayerLobbyWindow lobbyWindow = new MultiplayerLobbyWindow();
            GameEngineApp.Instance.ControlManager.Controls.Add(lobbyWindow);

            GameEngineApp.Instance.Server_OnCreateServer();
        }

        private void Connect_Click(Button sender)
        {
            DoConnect();
        }

        private void DoConnect()
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(editBoxUserPass.Text))
            {
                SetInfo("Please enter correct Username and Password.", true);
                return;
            }
            userName = editBoxUserName.Text.Trim();
            password = editBoxUserPass.Text.Trim();

            if (!MySqlUserConnection(userName, password))
            {
                SetInfo("Wrong username or password", true);
                return;
            }

            SetInfo("Connecting to the server...", false);

            editBoxUserName.Enable = false;
            buttonConnect.Enable = false;

            this.OnDetach();
            // MPAKlobbyWindow lobbyWindow = new MPAKlobbyWindow();
            // lobbyWindow.ServerUserName = userName;
            GameEngineApp.Instance.ControlManager.Controls.Add(new MPAKlobbyWindow());
            //shouldgo();
        }

        private bool MySqlUserConnection(string loginName, string password)
        {
            try
            {
                string sql = "SELECT Username, Password FROM phpap_AKusers WHERE Username=@User AND Password=@pass";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter User = new MySqlParameter();
                MySqlParameter Pass = new MySqlParameter();
                User.ParameterName = "@User";
                Pass.ParameterName = "@pass";
                User.Value = loginName;
                Pass.Value = password;
                cmd.Parameters.Add(User);
                cmd.Parameters.Add(Pass);

                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (rdr["Username"].Equals(loginName))
                    {
                        Program.username = loginName;
                        rdr.Close();
                        return true;
                    }
                    else
                    {
                        rdr.Close();
                        return false;
                    }
                }
                rdr.Dispose();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    EngineConsole.Instance.Print(ex.Message);
                    return false;
                }
            }
            return false;
        }

        //void Exit_Click( Button sender )
        //{
        //    shouldgo();
        //    this.OnDetach();
        //    //Controls.Add(new MainMenuWindow());
        //    MainMenuWindow.Instance.Go = false;
        //}

        private void SetInfo(string text, bool error)
        {
            TextBox textBoxInfo = (TextBox)window.Controls["Info"];
            textBoxInfo.Text = text;
            textBoxInfo.TextColor = error ? new ColorValue(1, 0, 0) : new ColorValue(1, 1, 1);
        }
    }
}