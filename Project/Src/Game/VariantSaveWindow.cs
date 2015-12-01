using Engine.FileSystem;
using Engine.MapSystem;
using Engine.UISystem;
using ProjectEntities;

namespace Game
{
    public class VariantSaveWindow : Control
    {
        public class VariantWeaponGroupItem
        {
            private string bodyPartName;

            public string BodyPartName
            {
                get { return bodyPartName; }
                set { bodyPartName = value; }
            }

            private string weaponName;

            public string WeaponName
            {
                get { return weaponName; }
                set { weaponName = value; }
            }

            private string attachedMapObjectTypeName;

            public string AttachedMapObjectTypeName
            {
                get { return attachedMapObjectTypeName; }
                set { attachedMapObjectTypeName = value; }
            }

            private int oldWeaponGroup;

            public int OldWeaponGroup
            {
                get { return oldWeaponGroup; }
                set { oldWeaponGroup = value; }
            }

            private int newWeaponGroup;

            public int NewWeaponGroup
            {
                get { return newWeaponGroup; }
                set { newWeaponGroup = value; }
            }

            public override string ToString()
            {
                return attachedMapObjectTypeName;
            }
        }

        public delegate void OnSaveVariant(VariantSaveWindow sender, string saveName, TextBlock variant);

        public event OnSaveVariant SaveVariant;

        private Button btnSaveVariant;
        private Button btnBack;
        private EditBox txtVariantName;
        private ListBox lstWeaponList;
        private EditBox txtWeaponGroup;

        //lol, now i'm as bad as msg
        public TextBox txtInfo;

        private Control window;

        private int cost;

        private TextBlock variant;
        private AKunit spawned;

        public VariantSaveWindow(int variantCost, TextBlock variant, AKunit spawned)
        {
            this.spawned = spawned;
            this.variant = variant;
            cost = variantCost;
        }

        protected override void OnAttach()
        {
            if (variant == null)
                return;

            base.OnAttach();

            window = ControlDeclarationManager.Instance.CreateControl("Gui\\VariantSaveWindow.gui");
            Controls.Add(window);

            btnSaveVariant = (Button)window.Controls["SaveVariant"];
            btnBack = (Button)window.Controls["Back"];
            txtVariantName = (EditBox)window.Controls["VariantName"];
            txtInfo = (TextBox)window.Controls["Info"];

            txtInfo.Text = "Total Cost of Variant: " + cost.ToString();

            btnBack.Click += new Button.ClickDelegate(btnBack_Click);

            lstWeaponList = (ListBox)window.Controls["WeaponList"];
            lstWeaponList.SelectedIndexChange += new ListBox.SelectedIndexChangeDelegate(lstWeaponList_SelectedIndexChange);

            txtWeaponGroup = (EditBox)window.Controls["Group"];
            txtWeaponGroup.TextChange += new DefaultEventDelegate(txtWeaponGroup_TextChange);

            btnSaveVariant.Click += new Button.ClickDelegate(btnSaveVariant_Click);

            foreach (TextBlock childBlock in variant.Children)
            {
                foreach (TextBlock grandChildBlock in childBlock.Children)
                {
                    VariantWeaponGroupItem vwgi = new VariantWeaponGroupItem();
                    vwgi.BodyPartName = childBlock.Name;
                    vwgi.WeaponName = grandChildBlock.Name;
                    MapObjectAttachedMapObject obj = spawned.GetFirstAttachedObjectByAlias(vwgi.WeaponName) as MapObjectAttachedMapObject;
                    vwgi.AttachedMapObjectTypeName = obj.MapObject.Type.Name;
                    vwgi.OldWeaponGroup = int.Parse(grandChildBlock.Attributes[1].Value);
                    vwgi.NewWeaponGroup = vwgi.OldWeaponGroup;
                    lstWeaponList.Items.Add(vwgi);
                }
            }
        }

        private void lstWeaponList_SelectedIndexChange(ListBox sender)
        {
            if (sender.SelectedIndex == -1)
            {
                txtWeaponGroup.Text = string.Empty;
                return;
            }

            VariantWeaponGroupItem vwgi = sender.SelectedItem as VariantWeaponGroupItem;
            txtWeaponGroup.Text = vwgi.NewWeaponGroup.ToString();
        }

        private void txtWeaponGroup_TextChange(Control sender)
        {
            if (lstWeaponList.SelectedIndex == -1)
                return;

            int newGroup = -1;

            if (!int.TryParse(sender.Text, out newGroup))
                return;

            VariantWeaponGroupItem vwgi = lstWeaponList.SelectedItem as VariantWeaponGroupItem;
            vwgi.NewWeaponGroup = newGroup;
        }

        private void btnSaveVariant_Click(Button sender)
        {
            if (string.IsNullOrEmpty(txtVariantName.Text))
            {
                txtInfo.Text = "Variant name needed.";
                return;
            }

            //check if the file exists

            foreach (object item in lstWeaponList.Items)
            {
                VariantWeaponGroupItem vwgi = item as VariantWeaponGroupItem;

                TextBlock bodyPartBlock = variant.FindChild(vwgi.BodyPartName);
                TextBlock weaponBlock = bodyPartBlock.FindChild(vwgi.WeaponName);
                weaponBlock.SetAttribute("g", vwgi.NewWeaponGroup.ToString());
            }

            if (SaveVariant != null)
                SaveVariant(this, txtVariantName.Text.Trim(), variant);
        }

        private void btnBack_Click(Button sender)
        {
            SetShouldDetach();
        }
    }
}