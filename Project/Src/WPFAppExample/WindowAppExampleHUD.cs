// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine.UISystem;

namespace WPFAppExample
{
    public class WindowsAppExampleHUD : Control
    {
        public WindowsAppExampleHUD()
        {
        }

        protected override void OnAttach()
        {
            base.OnAttach();

            Control window = ControlDeclarationManager.Instance.CreateControl(
                "Gui\\WindowsAppExampleHUD.gui");
            Controls.Add(window);

            ((Button)window.Controls["Close"]).Click += CloseButton_Click;
        }

        private void CloseButton_Click(Button sender)
        {
            SetShouldDetach();
        }
    }
}