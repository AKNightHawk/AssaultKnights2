// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;
using Engine.MathEx;
using Engine.UISystem;

namespace Game
{
    /// <summary>
    /// Defines a "MessageBox" window.
    /// </summary>
    public class MessageBoxWindow : Control
    {
        private string messageText;
        private string caption;
        private string windowname;

        private Button.ClickDelegate clickHandler;

        //

        public MessageBoxWindow(string messageText, string caption, string windowname, Button.ClickDelegate clickHandler)
        {
            this.messageText = messageText;
            this.caption = caption;
            this.Name = this.windowname = windowname;
            this.clickHandler = clickHandler;
        }

        protected override void OnAttach()
        {
            base.OnAttach();

            TopMost = true;

            Control window = ControlDeclarationManager.Instance.CreateControl(
                "Gui\\MessageBoxWindow.gui");
            Controls.Add(window);

            window.Controls["MessageText"].Text = messageText;

            window.Text = caption;
            window.Name = windowname;

            ((Button)window.Controls["OK"]).Click += OKButton_Click;

            BackColor = new ColorValue(0, 0, 0, .5f);

            EngineApp.Instance.RenderScene();
        }

        private void OKButton_Click(Button sender)
        {
            if (clickHandler != null)
                clickHandler(sender);

            SetShouldDetach();
        }
    }
}