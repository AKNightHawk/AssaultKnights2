// Copyright (C) 2006-2009 NeoAxis Group Ltd.
using System;
using Engine;
using Engine.MathEx;
using Engine.UISystem;
using MySql.Data.MySqlClient;
using ProjectCommon;

//using System.Windows.Forms;

namespace Game
{
    public class MultiplayerRegisterWindow : Control
    {
        private Control window;

        private EditBox RealName;
        private EditBox UserName;
        private PasswordBox Pass1;
        private PasswordBox Pass2;
        private EditBox Email;

        ///////////////////////////////////////////

        protected override void OnAttach()
        {
            base.OnAttach();

            //create window
            window = ControlDeclarationManager.Instance.CreateControl(
                "Gui\\RegisterWindow.gui");
            Controls.Add(window);

            MouseCover = true;
            BackColor = new ColorValue(0, 0, 0, .5f);

            RealName = (EditBox)window.Controls["RealName"];
            UserName = (EditBox)window.Controls["UserName"];
            
            //Age = (EditBox)window.Controls["Age"];
            //OfAge = (CheckBox)window.Controls["OfAge"];

            Pass1 = (PasswordBox)window.Controls["Pass1"];
            Pass2 = (PasswordBox)window.Controls["Pass2"];
            Email = (EditBox)window.Controls["Email"];

            ((Button)window.Controls["Exit"]).Click += Exit_Click;
            ((Button)window.Controls["Register"]).Click += Register_Click;
            ((Button)window.Controls["Cancel"]).Click += Cancel_Click;
        }

        protected override void OnDetach()
        {
            base.OnDetach();
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

            return false;
        }

        private void Exit_Click(Button sender)
        {
            SetShouldDetach();
        }

        private void Cancel_Click(Button sender)
        {
            SetShouldDetach();
        }

        private void Register_Click(Button sender)
        {
            if (string.IsNullOrEmpty(RealName.Text) || string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(Pass1.Text)
                || string.IsNullOrEmpty(Pass2.Text) || string.IsNullOrEmpty(Email.Text))
            {
                SetInfo("Please fill all fileds", true);
                return;
            }

            if (!Pass2.Text.Equals(Pass1.Text))
            {
                SetInfo("Passwords dont match, Please re-enter", true);
                Pass2.Text = "";
                Pass1.Text = "";
                return;
            }

            if (!Email.Text.Contains("@"))
            {
                SetInfo("need valid email", true);
                Email.Text = "";
            }

            //mystart

            string Error = "";
            bool failed = false;

            try
            {
                //UserCheck
                string sql = "SELECT Username FROM phpap_AKusers WHERE Username=@User";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter User = new MySqlParameter();
                User.ParameterName = "@User";
                User.Value = UserName.Text;
                cmd.Parameters.Add(User);

                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (rdr["Username"].Equals(UserName.Text))
                    {
                        failed = true;
                        Error = "Username already exists!";
                        SetInfo(Error, true);
                        rdr.Close();
                    }
                }
                rdr.Dispose();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    SetInfo(ex.Message, true);
                }
            }

            if (failed)
                return;

            try
            {
                //EmailCheck
                string sql = "SELECT Email FROM phpap_AKusers WHERE Email=@Email";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter EmailP = new MySqlParameter();
                EmailP.ParameterName = "@Email";
                EmailP.Value = Email.Text;
                cmd.Parameters.Add(EmailP);

                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (rdr["Email"].Equals(Email.Text))
                    {
                        failed = true;
                        Error = "Email already exists";
                        SetInfo(Error, true);
                        rdr.Close();
                    }
                }
                rdr.Dispose();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    SetInfo(ex.Message, true);
                }
            }

            if (failed)
                return;

            //Registrationg
            try
            {
                string sql = "INSERT INTO phpap_AKusers (Username, Password, Email, Name) VALUES (@User, @Pass, @email, @Name)";

                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter User = new MySqlParameter();
                MySqlParameter Pass = new MySqlParameter();
                MySqlParameter email = new MySqlParameter();
                MySqlParameter Name2 = new MySqlParameter();

                User.ParameterName = "@User";
                Pass.ParameterName = "@pass";
                email.ParameterName = "@email";
                Name2.ParameterName = "@Name";

                User.Value = UserName.Text;
                Pass.Value = Pass1.Text;
                email.Value = Email.Text;
                Name2.Value = RealName.Text;

                cmd.Parameters.Add(User); cmd.Parameters.Add(Pass); cmd.Parameters.Add(email); cmd.Parameters.Add(Name2);

                cmd.ExecuteNonQuery();

                SetInfo("Registration completed you can login NOW", false);
                //iNCIN .. disable all controls except exit
                RealName.Enable = false;
                UserName.Enable = false;
                Pass1.Enable = false;
                Pass2.Enable = false;
                Email.Enable = false;
                ((Button)window.Controls["Register"]).Enable = false;
                ((Button)window.Controls["Cancel"]).Enable = false;
                ((Button)window.Controls["Exit"]).Enable = true;
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    SetInfo(ex.Message, true);
                }
            }
        }

        private void SetInfo(string text, bool error)
        {
            TextBox textBoxInfo = (TextBox)window.Controls["Info"];

            textBoxInfo.Text = text;
            textBoxInfo.TextColor = error ? new ColorValue(1, 0, 0) : new ColorValue(1, 1, 1);
        }

        private enum NetworkMessages
        {
            RegisterInfo,
        }
    }
}