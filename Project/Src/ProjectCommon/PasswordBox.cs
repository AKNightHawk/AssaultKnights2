using System.ComponentModel;
using Engine.UISystem;

namespace ProjectCommon
{
    public class PasswordBox : EditBox
    {
        private char passwordChar = '*';

        [Description("Displayed symbol"), Serialize]
        public char PasswordChar
        {
            get { return passwordChar; }
            set { passwordChar = value; }
        }

        protected TextBox PasswordControl;

        protected override void OnAttach()
        {
            base.OnAttach();

            PasswordControl = new TextBox();
            if (TextControl != null)
            {
                PasswordControl.Size = TextControl.Size;
                PasswordControl.Position = TextControl.Position;
                TextControl.Visible = false;
            }
            PasswordControl.TextHorizontalAlign = Engine.Renderer.HorizontalAlign.Left;
            Controls.Add(PasswordControl);
            this.TextChange += delegate { UpdateText(); };
            UpdateText();
        }

        protected override void OnResize()
        {
            base.OnResize();

            //   if (TextControl == null)
            //      return;

            //PasswordControl.Size = TextControl.Size;
            //PasswordControl.Position = TextControl.Position;
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);

            if (TextControl != null)
            {
                if (this.Focused)
                {
                    if (TextControl.Text.Length == PasswordControl.Text.Length + 1)
                        PasswordControl.Text += "_";
                }
                else
                {
                    if (TextControl.Text.Length + 1 == PasswordControl.Text.Length)
                        PasswordControl.Text = PasswordControl.Text.Remove(PasswordControl.Text.Length - 1);
                }
            }
        }

        private void UpdateText()
        {
            PasswordControl.Text = string.Empty;
            string passwordString = string.Empty;

            //create password string of same length
            for (int i = 0; i < Text.Length; i++)
                passwordString += passwordChar;

            PasswordControl.Text = passwordString;
        }

        public void ToggleRealTextVisible()
        {
            if (PasswordControl == null || TextControl == null)
            {
                PasswordControl.Visible = !PasswordControl.Visible;
                TextControl.Visible = !TextControl.Visible;
            }
        }
    }
}