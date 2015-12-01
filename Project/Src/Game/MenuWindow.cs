// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;

namespace Game
{
    /// <summary>
    /// Defines a system game menu.
    /// </summary>
    public class MenuWindow : Control
    {
        protected override void OnAttach()
        {
            base.OnAttach();

            Control window = ControlDeclarationManager.Instance.CreateControl("Gui\\MenuWindow.gui");
            Controls.Add(window);

            ((Button)window.Controls["Maps"]).Click += mapsButton_Click;
            ((Button)window.Controls["LoadSave"]).Click += loadSaveButton_Click;
            ((Button)window.Controls["Options"]).Click += optionsButton_Click;
            ((Button)window.Controls["ProfilingTool"]).Click += ProfilingToolButton_Click;
            ((Button)window.Controls["About"]).Click += aboutButton_Click;
            ((Button)window.Controls["ExitToMainMenu"]).Click += exitToMainMenuButton_Click;
            ((Button)window.Controls["Exit"]).Click += exitButton_Click;
            ((Button)window.Controls["Resume"]).Click += resumeButton_Click;

            if (GameWindow.Instance == null)
                window.Controls["ExitToMainMenu"].Enable = false;

            if (GameNetworkClient.Instance != null)
                window.Controls["Maps"].Enable = false;

            if (GameNetworkServer.Instance != null || GameNetworkClient.Instance != null)
                window.Controls["LoadSave"].Enable = false;

            MouseCover = true;

            BackColor = new ColorValue(0, 0, 0, .5f);
        }

        private void mapsButton_Click(object sender)
        {
            foreach (Control control in Controls)
                control.Visible = false;
            Controls.Add(new MapsWindow());
        }

        private void loadSaveButton_Click(object sender)
        {
            foreach (Control control in Controls)
                control.Visible = false;
            Controls.Add(new WorldLoadSaveWindow());
        }

        private void optionsButton_Click(object sender)
        {
            foreach (Control control in Controls)
                control.Visible = false;
            Controls.Add(new OptionsWindow());
        }

        private void ProfilingToolButton_Click(object sender)
        {
            SetShouldDetach();
            GameEngineApp.ShowProfilingTool(true);
        }

        private void aboutButton_Click(object sender)
        {
            foreach (Control control in Controls)
                control.Visible = false;
            Controls.Add(new AboutWindow());
        }

        protected override void OnControlDetach(Control control)
        {
            base.OnControlDetach(control);

            if ((control as OptionsWindow) != null ||
                (control as MapsWindow) != null ||
                (control as WorldLoadSaveWindow) != null ||
                (control as AboutWindow) != null)
            {
                foreach (Control c in Controls)
                    c.Visible = true;
            }
        }

        private void exitToMainMenuButton_Click(object sender)
        {
            MapSystemWorld.MapDestroy();
            EntitySystemWorld.Instance.WorldDestroy();

            GameEngineApp.Instance.Server_DestroyServer("The server has been destroyed");
            GameEngineApp.Instance.Client_DisconnectFromServer();

            //close all windows
            foreach (Control control in GameEngineApp.Instance.ControlManager.Controls)
                control.SetShouldDetach();
            //create main menu
            GameEngineApp.Instance.ControlManager.Controls.Add(new MainMenuWindow());
        }

        private void exitButton_Click(object sender)
        {
            GameEngineApp.Instance.SetFadeOutScreenAndExit();
        }

        private void resumeButton_Click(object sender)
        {
            SetShouldDetach();
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
    }
}