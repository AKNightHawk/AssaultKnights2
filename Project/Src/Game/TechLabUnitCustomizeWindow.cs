using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using MySql.Data.MySqlClient;
using ProjectEntities;

namespace Game
{
    public class TechLabUnitCustomizeWindow : Control
    {
        #region control declarations

        public delegate void OnBuyGuiUpdate(string text, bool error);

        public delegate void OnBuyLog(string text);

        //private bool Go = false;
        //private float maxout = -800;
        //private static float maxin = 0f;
        //private static float Rate = 1000;
        //private float positionY;
        //private bool done = false;

        protected Spawner spawner;
        private Texture cameraTexture;
        private Camera rmCamera;
        private MapCamera camera;

        //public Control techlabbuywindow;
        //public Control customizewindow;
        protected Control window;

        protected PriceListC MechsPriceList;
        protected PriceListC AunitPriceList;
        protected PriceListC GunitPriceList;
        protected PriceListC JunitPriceList;

        public enum Active_List
        {
            None,
            Mech,
            AirUnit,
            GroundUnit,
            Jet
        }

        public class CustomizableUnit
        {
            private int id;

            public int ID
            {
                get { return id; }
            }

            private string name;

            public string Name
            {
                get { return name; }
            }

            public CustomizableUnit(int id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        protected List<CustomizableUnit> MechDBUnits = new List<CustomizableUnit>();
        protected List<CustomizableUnit> ADBUnits = new List<CustomizableUnit>();
        protected List<CustomizableUnit> GDBUnits = new List<CustomizableUnit>();
        protected List<CustomizableUnit> JDBUnits = new List<CustomizableUnit>();

        protected int mechListIndex = 0;
        protected int airUnitListIndex = 0;
        protected int groundUnitListIndex = 0;
        protected int jetUnitListIndex = 0;

        protected Active_List activeList = Active_List.None;

        protected int cash;

        //for selecting our units
        private Button btnMechs;

        private Button btnGroundUnits;
        private Button btnAirUnits;
        private Button btnJets;
        private Button btnNext;
        private Button btnPrevious;

        //For unit weapon slot selection
        private Button btnRA;

        private Button btnRT;
        private Button btnCT;
        private Button btnLT;
        private Button btnLA;

        //for weapon selection and placement
        private ListBox cbxWeaponSlots;

        private ListBox cbxVariantList;
        private ListBox lstWeaponList;
        private Button btnAddWeapon;
        private EditBox txtWeaponInfo;

        //misc
        private Button btnSaveVariant;

        private Button btnExit;
        private TextBox txtCash;

        private TextBox txtUnitName;

        //Control Controls;

        #endregion control declarations

        #region for saving variants

        private TextBlock variant;

        private int variantCost;

        #endregion for saving variants

        #region overrides

        protected override void OnAttach()
        {
            base.OnAttach();

            window = ControlDeclarationManager.Instance.CreateControl("Gui\\TechLabUnitCustomizeWindow.gui");
            Controls.Add(window);

            if (spawner == null)
                spawner = Entities.Instance.GetByName("TechlabMechSpawn") as Spawner;
            else
                spawner = Entities.Instance.GetByName("TechlabMechSpawn") as Spawner;

            if (camera == null)
                camera = Entities.Instance.GetByName("TechlabCam") as MapCamera;
            else
                camera = Entities.Instance.GetByName("TechlabCam") as MapCamera;

            //declare controls
            btnMechs = (Button)window.Controls["Mechs"];
            btnMechs.Click += new Button.ClickDelegate(btnMechs_Click);

            btnGroundUnits = (Button)window.Controls["Gunit"];
            btnGroundUnits.Click += new Button.ClickDelegate(btnGroundUnits_Click);

            btnAirUnits = (Button)window.Controls["Aunit"];
            btnAirUnits.Click += new Button.ClickDelegate(btnAirUnits_Click);

            btnJets = (Button)window.Controls["Junit"];
            btnJets.Click += new Button.ClickDelegate(btnJets_Click);

            btnNext = (Button)window.Controls["Next"];
            btnNext.Click += new Button.ClickDelegate(btnNext_Click);

            btnPrevious = (Button)window.Controls["Previous"];
            btnPrevious.Click += new Button.ClickDelegate(btnPrevious_Click);

            btnRA = (Button)window.Controls["RA"];
            btnRA.Click += new Button.ClickDelegate(btnRA_Click);

            btnRT = (Button)window.Controls["RT"];
            btnRT.Click += new Button.ClickDelegate(btnRT_Click);

            btnCT = (Button)window.Controls["CT"];
            btnCT.Click += new Button.ClickDelegate(btnCT_Click);

            btnLT = (Button)window.Controls["LT"];
            btnLT.Click += new Button.ClickDelegate(btnLT_Click);

            btnLA = (Button)window.Controls["LA"];
            btnLA.Click += new Button.ClickDelegate(btnLA_Click);
            //iNCIN -- SlotList
            cbxWeaponSlots = (ListBox)window.Controls["SlotList"];
            cbxWeaponSlots.SelectedIndexChange +=
                new ListBox.SelectedIndexChangeDelegate(cbxWeaponSlots_SelectedIndexChange);

            cbxVariantList = (ListBox)window.Controls["VariantList"];
            cbxVariantList.SelectedIndexChange +=
                new ListBox.SelectedIndexChangeDelegate(cbxVariantList_SelectedIndexChange);

            lstWeaponList = (ListBox)window.Controls["WeaponList"];
            lstWeaponList.SelectedIndexChange +=
                new ListBox.SelectedIndexChangeDelegate(lstWeaponList_SelectedIndexChange);

            btnAddWeapon = (Button)window.Controls["AddWeapon"];
            btnAddWeapon.Click += new Button.ClickDelegate(btnAddWeapon_Click);

            txtWeaponInfo = (EditBox)window.Controls["WeaponInfo"];
            txtCash = (TextBox)window.Controls["Cash"];

            btnSaveVariant = (Button)window.Controls["SaveCustomUnit"];
            btnSaveVariant.Click += new Button.ClickDelegate(btnSaveVariant_Click);

            btnExit = (Button)window.Controls["Quit"];
            btnExit.Click += new Button.ClickDelegate(btnExit_Click);

            txtUnitName = (TextBox)window.Controls["UnitName"];

            MechsPriceList = (PriceListC)Entities.Instance.Create("MechPriceList", Map.Instance);
            AunitPriceList = (PriceListC)Entities.Instance.Create("AunitPriceList", Map.Instance);
            GunitPriceList = (PriceListC)Entities.Instance.Create("GunitPriceList", Map.Instance);
            JunitPriceList = (PriceListC)Entities.Instance.Create("JunitPriceList", Map.Instance);

            GetListOfPlayerUnits();

            cash = GetPlayerCashSQL();

            if (MechDBUnits.Count == 0)
                btnMechs.Enable = false;

            if (ADBUnits.Count == 0)
                btnAirUnits.Enable = false;

            if (GDBUnits.Count == 0)
                btnGroundUnits.Enable = false;

            if (JDBUnits.Count == 0)
                btnJets.Enable = false;

            if (MechDBUnits.Count == 0 || ADBUnits.Count == 0 || GDBUnits.Count == 0 || JDBUnits.Count == 0)
            {
                //player has not bought any units
                Log.Info("No units purchased. TODO: ask user if they want to go to buy window");
            }

            InitCameraViewFromTarget();
            //positionY = maxout;

            spawner.UnitSpawned += new Spawner.OnUnitSpawned(spawner_UnitSpawned);
        }

        private void spawner_UnitSpawned(Unit unit)
        {
            AKunit ut = unit as AKunit;

            cbxWeaponSlots.Items.Clear();
            lstWeaponList.Items.Clear();
            cbxVariantList.Items.Clear();

            btnRA.Enable = btnRA.Active = false;
            btnRT.Enable = btnRT.Active = false;
            btnCT.Enable = btnCT.Active = false;
            btnLT.Enable = btnLT.Active = false;
            btnLA.Enable = btnLA.Active = false;

            foreach (AKunit.BP bp in ut.Bp)
            {
                if (window.Controls[bp.GUIDesplayName] != null)
                    window.Controls[bp.GUIDesplayName].Enable = true;
            }

            variant = new TextBlock();

            /*foreach (AKunit.BP bp in ut.Bp)
            {
                TextBlock bpBlock = variant.AddChild(bp.GUIDesplayName);

                foreach (AKunitType.WeaponItem w in bp.Weapons)
                {
                    TextBlock wBlock = bpBlock.AddChild(w.MapObjectAlias);

                    wBlock.SetAttribute("Ammo", w.Ammo.ToString());
                    wBlock.SetAttribute("MagazineCapacity", w.MagazineCapacity.ToString());
                    wBlock.SetAttribute("WeaponType", w.WeaponType.Name);
                }
            }*/

            //load up variant file names
            string varDir = string.Format("{0}\\Variants\\{1}", VirtualFileSystem.UserDirectoryPath,
                spawner.Spawned.Type.Name);

            if (Directory.Exists(varDir))
            {
                DirectoryInfo di = new DirectoryInfo(varDir);
                FileInfo[] files = di.GetFiles();

                foreach (FileInfo file in files)
                    cbxVariantList.Items.Add(file.Name);
            }
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
            return false;
        }

        protected override void OnDetach()
        {
            //base.OnDetach();

            if (spawner.Spawned != null)
            {
                spawner.Spawned.SetForDeletion(false);
                spawner.Spawned = null;
            }
            if (rmCamera != null)
            {
                rmCamera.Dispose();
                rmCamera = null;
            }
        }

        private void InitCameraViewFromTarget()
        {
            int textureSize = 1024;

            cameraTexture = TextureManager.Instance.Create(
                TextureManager.Instance.GetUniqueName("RemoteView"), Texture.Type.Type2D,
                new Vec2I(textureSize, textureSize), 1, 0, PixelFormat.R8G8B8, Texture.Usage.RenderTarget);

            RenderTexture renderTexture = cameraTexture.GetBuffer().GetRenderTarget();

            rmCamera = SceneManager.Instance.CreateCamera("RemoteView");
            rmCamera.ProjectionType = ProjectionTypes.Perspective;
            rmCamera.PolygonMode = PolygonMode.Wireframe;

            renderTexture.AddViewport(rmCamera);
        }

        protected void GetCameraViewFromTarget()
        {
            if (rmCamera == null)
            {
                return;
            }
            //    InitCameraViewFromTarget();

            rmCamera.LookAt(spawner.Position);
            rmCamera.FarClipDistance = (spawner.Position - rmCamera.Position).Length() * 10f;
            rmCamera.Position = camera.Position;
            rmCamera.FixedUp = (Vec3.ZAxis);
            rmCamera.Visible = true;
            rmCamera.PolygonMode = PolygonMode.Wireframe;
            rmCamera.NearClipDistance = 1.0f;
            rmCamera.Fov = camera.Fov;
            window.Controls["UnitWindow"].BackTexture = cameraTexture;
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            //Back(false);
            //if (!Go)
            //{
            //    // slide window in
            //    if (positionY < maxin)
            //        positionY += (delta * Rate);
            //    else
            //        positionY = maxin;
            //}
            //else
            //{
            //    // slide window out
            //    if (positionY > maxout)
            //        positionY -= (delta * Rate);
            //    else
            //        positionY = maxout;
            //}
            //window.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(-30, positionY));

            if (window.Controls["UnitWindow"] != null)
                GetCameraViewFromTarget();
        }

        //private void shouldgo()
        //{
        //    Go = true;
        //}

        //protected void Back(bool reallygo)
        //{
        //    if (reallygo)
        //        shouldgo();

        //    if (positionY == maxout && Go && !done)
        //    {
        //        if (Map.Instance.Name.ToString() == "MainMap")
        //        {
        //            //this.OnDetach();
        //            ////Controls.Add(new MainMenuWindow());
        //            //MainMenuWindow.Instance.Go = false;
        //            //done = true;
        //            this.OnDetach();
        //            Controls.Add(new MPAKlobbyWindow());
        //            done = true;
        //        }
        //        else
        //        {
        //            //SetShouldDetach();
        //        }
        //    }
        //}

        #endregion overrides

        private void UpdateVariantCost(int amountToAdd)
        {
            variantCost = 0;

            if (amountToAdd != -1)
            {
                AKunitType akt = spawner.Spawned.Type as AKunitType;
                for (int i = 0; i < akt.BodyParts.Count; i++)
                {
                    TextBlock bodyPartBlock = variant.FindChild(akt.BodyParts[i].GUIDesplayName);

                    if (bodyPartBlock != null)
                    {
                        int bodyPartIndex = i;
                        int bodyPartBlockIndex = variant.Children.IndexOf(bodyPartBlock);

                        AKunitType.BodyPart bodyPart = akt.BodyParts[i];

                        for (int j = 0; j < bodyPart.Weapons.Count; j++)
                        {
                            TextBlock bodyPartWeaponBlock =
                                bodyPartBlock.FindChild(bodyPart.Weapons[j].MapObjectAlias);

                            if (bodyPartWeaponBlock != null)
                            {
                                int alternateWeaponIndex = int.Parse(bodyPartWeaponBlock.Attributes[0].Value);

                                variantCost += bodyPart.Weapons[j].Alternates[alternateWeaponIndex].Price;
                            }
                        }
                    }
                }
            }

            UpdateCashText();
        }

        private void UpdateCashText()
        {
            txtCash.Text = string.Format("Cost {0} AC ({1} AC Available)", variantCost, cash);

            if (variantCost >= cash)
                txtCash.ColorMultiplier = new ColorValue(1, 0, 0);
            else
                txtCash.ColorMultiplier = new ColorValue(0, 1, 0);
        }

        #region control handlers

        private void cbxWeaponSlots_SelectedIndexChange(ListBox sender)
        {
            lstWeaponList.Items.Clear();

            if (cbxWeaponSlots.SelectedItem == null)
                return;

            AKunitType.WeaponItem wi = cbxWeaponSlots.SelectedItem as AKunitType.WeaponItem;

            foreach (AKunitType.AlternateWeaponItem awi in wi.Alternates)
                lstWeaponList.Items.Add(awi);
        }

        private void cbxVariantList_SelectedIndexChange(ListBox sender)
        {
            if (sender.SelectedItem == null)
                return;

            string varFilePath = string.Format("{0}\\Variants\\{1}\\{2}", VirtualFileSystem.UserDirectoryPath,
                spawner.Spawned.Type.Name, sender.SelectedItem.ToString());

            string error;
            TextBlock varFile = TextBlockUtils.LoadFromRealFile(varFilePath, out error);
            if (!string.IsNullOrEmpty(error))
            {
                Log.Error(error);
                return;
            }

            if (varFile != null)
            {
                AKunit u = spawner.Spawned as AKunit;
                u.SetVariant(varFile);
            }
        }

        private void lstWeaponList_SelectedIndexChange(ListBox sender)
        {
            if (lstWeaponList.SelectedItem == null)
                return;

            //get default weaponType on the selected slot
            AKunit u = spawner.Spawned as AKunit;
            AKunitType.WeaponItem wi = cbxWeaponSlots.SelectedItem as AKunitType.WeaponItem;
            AKunitType.AlternateWeaponItem awi = lstWeaponList.SelectedItem as AKunitType.AlternateWeaponItem;

            string selectedBodyPartName = GetBodyPartNameFromActiveButton();
            int selectedBodyPartIndex = GetBodyPartIndex(selectedBodyPartName);
            string weaponAlias = wi.MapObjectAlias;

            MapObjectAttachedObject o = u.GetFirstAttachedObjectByAlias(weaponAlias);
            MapObjectAttachedMapObject mo = o as MapObjectAttachedMapObject;
            u.Detach(mo);

            Weapon w = Entities.Instance.Create(awi.WeaponType, Map.Instance) as Weapon;
            w.PostCreate();
            MapObjectAttachedMapObject amo = new MapObjectAttachedMapObject();
            amo.MapObject = w;
            amo.PositionOffset = mo.PositionOffset;
            amo.Alias = mo.Alias;
            amo.Body = mo.Body;
            amo.BoneSlot = mo.BoneSlot;
            amo.RotationOffset = mo.RotationOffset;
            amo.ScaleOffset = mo.ScaleOffset;

            u.Attach(amo);

            Gun g = w as Gun;
            float multiplier = 1 / g.Type.NormalMode.BetweenFireTime;
            float rateOfFire = multiplier;

            if (g.Type.NormalMode.BulletExpense != 0)
                rateOfFire = rateOfFire * g.Type.NormalMode.BulletExpense;

            txtWeaponInfo.Text = string.Format(
                "Weapon Name: {0}\r\nWeapon Type: {1}\r\nDamage: {2}\r\nHeat: {3}\r\nRate of Fire: {4}\r\nPrice: {5}",
                w.Type.Name, "Not Implemented", g.Type.NormalMode.BulletType.Damage, g.Type.NormalMode.AKGunHeatGeneration, rateOfFire.ToString("F1"), awi.Price);
        }

        private void btnAddWeapon_Click(Button sender)
        {
            if (cbxWeaponSlots.SelectedItem == null || lstWeaponList.SelectedItem == null)
                return;
            AKunit u = spawner.Spawned as AKunit;
            AKunitType.WeaponItem wi = cbxWeaponSlots.SelectedItem as AKunitType.WeaponItem;
            AKunitType.AlternateWeaponItem awi = lstWeaponList.SelectedItem as AKunitType.AlternateWeaponItem;

            string selectedBodyPartName = GetBodyPartNameFromActiveButton();
            int selectedBodyPartIndex = GetBodyPartIndex(selectedBodyPartName);
            int weaponFireGroup =
                u.Bp[selectedBodyPartIndex].Weapons[cbxWeaponSlots.SelectedIndex].FireGroup;

            u.Bp[selectedBodyPartIndex].Weapons[cbxWeaponSlots.SelectedIndex].Ammo = awi.Ammo;
            u.Bp[selectedBodyPartIndex].Weapons[cbxWeaponSlots.SelectedIndex].MagazineCapacity =
                awi.MagazineCapacity;
            u.Bp[selectedBodyPartIndex].Weapons[cbxWeaponSlots.SelectedIndex].WeaponType = awi.WeaponType;

            TextBlock bodyPartBlock = variant.FindChild(selectedBodyPartName);

            if (bodyPartBlock == null)
                bodyPartBlock = variant.AddChild(selectedBodyPartName);

            if (bodyPartBlock != null)
            {
                TextBlock wBlock = bodyPartBlock.FindChild(wi.MapObjectAlias);

                if (wBlock == null)
                    wBlock = bodyPartBlock.AddChild(wi.MapObjectAlias);

                if (wBlock != null)
                {
                    wBlock.SetAttribute("i", lstWeaponList.SelectedIndex.ToString());
                    wBlock.SetAttribute("g", weaponFireGroup.ToString());
                }
            }

            UpdateVariantCost(awi.Price);

            PopulateWeaponSlotsDropDown(selectedBodyPartName);
        }

        private void btnSaveVariant_Click(Button sender)
        {
            VariantSaveWindow vsw = new VariantSaveWindow(variantCost, variant, (AKunit)spawner.Spawned);
            vsw.SaveVariant += new VariantSaveWindow.OnSaveVariant(vsw_SaveVariant);
            Controls.Add(vsw);
        }

        private void vsw_SaveVariant(VariantSaveWindow sender, string saveName, TextBlock variant)
        {
            BackgroundWorker bgwBuyVariant = new BackgroundWorker();
            bgwBuyVariant.DoWork += new DoWorkEventHandler(bgwBuyVariant_DoWork);
            bgwBuyVariant.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwBuyVariant_RunWorkerCompleted);

            string fullFileName = saveName + ".var";
            string saveDir = string.Format("{0}\\Variants\\{1}", VirtualFileSystem.UserDirectoryPath,
                spawner.Spawned.Type.Name);

            if (!Directory.Exists(saveDir))
                Directory.CreateDirectory(saveDir);

            string[] variantsInDir = Directory.GetFiles(saveDir);

            bool alreadyExists = false;
            foreach (string fileName in variantsInDir)
            {
                if (fileName == fullFileName)
                    alreadyExists = true;
            }

            if (alreadyExists)
            {
                sender.txtInfo.Text = "File already exists";
                return;
            }

            bgwBuyVariant.RunWorkerAsync(new string[] { fullFileName, saveDir });

            sender.SetShouldDetach();
        }

        private void bgwBuyVariant_DoWork(object sender, DoWorkEventArgs e)
        {
            string[] args = (string[])e.Argument;
            string fullFileName = args[0];
            string saveDir = args[1];

            try
            {
                //Decreasing Credit
                string sql = "UPDATE phpap_AKusers SET money = @DeductedCash WHERE Username=@User";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter DeductedCashP = new MySqlParameter();
                DeductedCashP.ParameterName = "@DeductedCash";
                DeductedCashP.Value = cash - variantCost;
                cmd.Parameters.Add(DeductedCashP);

                MySqlParameter EmailP = new MySqlParameter();
                EmailP.ParameterName = "@User";
                EmailP.Value = Program.username;
                cmd.Parameters.Add(EmailP);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    OnBuyLog update = new OnBuyLog(Log.Info);
                    update.Invoke(ex.Message);
                    return;
                }
            }

            cash -= variantCost;

            string finalFilePath = string.Format("{0}\\{1}", saveDir, fullFileName);
            File.Create(finalFilePath).Close();

            StreamWriter sw = new StreamWriter(finalFilePath);
            sw.Write(variant.DumpToString());
            sw.Close();
        }

        private void bgwBuyVariant_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateCashText();
        }

        private void btnExit_Click(Button sender)
        {
            SetShouldDetach();
            //Back(true);
        }

        private void btnRA_Click(Button sender)
        {
            SetButtonActive(sender);
            PopulateWeaponSlotsDropDown("RA");
        }

        private void btnRT_Click(Button sender)
        {
            SetButtonActive(sender);
            PopulateWeaponSlotsDropDown("RT");
        }

        private void btnCT_Click(Button sender)
        {
            SetButtonActive(sender);
            PopulateWeaponSlotsDropDown("CT");
        }

        private void btnLT_Click(Button sender)
        {
            SetButtonActive(sender);
            PopulateWeaponSlotsDropDown("LT");
        }

        private void btnLA_Click(Button sender)
        {
            SetButtonActive(sender);
            PopulateWeaponSlotsDropDown("LA");
        }

        #endregion control handlers

        #region Control handers for selecting units

        private void btnMechs_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.Mech;

            CustomizableUnit cu = MechDBUnits[mechListIndex];
            spawner.SpawnUnit(MechsPriceList.Type.PriceLists[cu.ID].PricedUnit);
            UpdateVariantCost(-1);

            txtUnitName.Text = "Unit Name: " + cu.Name;
        }

        private void btnGroundUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.GroundUnit;

            CustomizableUnit cu = GDBUnits[groundUnitListIndex];
            spawner.SpawnUnit(GunitPriceList.Type.PriceLists[cu.ID].PricedUnit);
            UpdateVariantCost(-1);

            txtUnitName.Text = "Unit Name: " + cu.Name;
        }

        private void btnAirUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.AirUnit;
            CustomizableUnit cu = ADBUnits[airUnitListIndex];
            spawner.SpawnUnit(AunitPriceList.Type.PriceLists[cu.ID].PricedUnit);
            UpdateVariantCost(-1);

            txtUnitName.Text = "Unit Name: " + cu.Name;
        }

        private void btnJets_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);
            activeList = Active_List.Jet;
            CustomizableUnit cu = JDBUnits[jetUnitListIndex];
            spawner.SpawnUnit(JunitPriceList.Type.PriceLists[cu.ID].PricedUnit);
            UpdateVariantCost(-1);

            txtUnitName.Text = "Unit Name: " + cu.Name;
        }

        private void btnNext_Click(Button sender)
        {
            if (activeList == Active_List.None)
                return;

            UpdateVariantCost(-1);
            int index = 0;
            int max = 0;
            UnitType u = null;
            string name = "No Name";

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        index = mechListIndex;
                        max = MechDBUnits.Count;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        index = airUnitListIndex;
                        max = ADBUnits.Count;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        index = groundUnitListIndex;
                        max = GDBUnits.Count;
                    }
                    break;

                case Active_List.Jet:
                    {
                        index = jetUnitListIndex;
                        max = JDBUnits.Count;
                    }
                    break;
            }

            int newIndex = ++index;
            if (newIndex >= max)
                newIndex = 0;

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        u = MechsPriceList.Type.PriceLists[MechDBUnits[newIndex].ID].PricedUnit;
                        name = MechDBUnits[newIndex].Name;
                        mechListIndex = newIndex;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        u = AunitPriceList.Type.PriceLists[ADBUnits[newIndex].ID].PricedUnit;
                        name = ADBUnits[newIndex].Name;
                        airUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        u = GunitPriceList.Type.PriceLists[GDBUnits[newIndex].ID].PricedUnit;
                        name = GDBUnits[newIndex].Name;
                        groundUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.Jet:
                    {
                        u = JunitPriceList.Type.PriceLists[JDBUnits[newIndex].ID].PricedUnit;
                        name = JDBUnits[newIndex].Name;
                        jetUnitListIndex = newIndex;
                    }
                    break;
            }

            spawner.SpawnUnit(u);

            txtUnitName.Text = "Unit Name: " + name;
        }

        private void btnPrevious_Click(Button sender)
        {
            if (activeList == Active_List.None)
                return;

            UpdateVariantCost(-1);
            int index = 0;
            int max = 0;
            UnitType u = null;
            string name = "No Name";

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        index = mechListIndex;
                        max = MechDBUnits.Count;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        index = airUnitListIndex;
                        max = ADBUnits.Count;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        index = groundUnitListIndex;
                        max = GDBUnits.Count;
                    }
                    break;

                case Active_List.Jet:
                    {
                        index = jetUnitListIndex;
                        max = JDBUnits.Count;
                    }
                    break;
            }

            int newIndex = --index;
            if (newIndex < 0)
                newIndex = max - 1;

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        u = MechsPriceList.Type.PriceLists[MechDBUnits[newIndex].ID].PricedUnit;
                        name = MechDBUnits[newIndex].Name;
                        mechListIndex = newIndex;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        u = AunitPriceList.Type.PriceLists[ADBUnits[newIndex].ID].PricedUnit;
                        name = ADBUnits[newIndex].Name;
                        airUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        u = GunitPriceList.Type.PriceLists[GDBUnits[newIndex].ID].PricedUnit;
                        name = GDBUnits[newIndex].Name;
                        groundUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.Jet:
                    {
                        u = JunitPriceList.Type.PriceLists[JDBUnits[newIndex].ID].PricedUnit;
                        name = JDBUnits[newIndex].Name;
                        jetUnitListIndex = newIndex;
                    }
                    break;
            }

            spawner.SpawnUnit(u);

            txtUnitName.Text = "Unit Name: " + name;
        }

        #endregion Control handers for selecting units

        #region methods for setting control data

        private void SetButtonActive(Button sender)
        {
            btnRA.Active = false;
            btnRT.Active = false;
            btnCT.Active = false;
            btnLT.Active = false;
            btnLA.Active = false;

            sender.Active = true;
        }

        private void SetUnitTypeButtonActive(Button sender)
        {
            btnMechs.Active = false;
            btnGroundUnits.Active = false;
            btnAirUnits.Active = false;
            btnJets.Active = false;

            sender.Active = true;
        }

        private void PopulateWeaponSlotsDropDown(string bodyPartName)
        {
            AKunit.BP selectedBodyPart = GetBodyPart(bodyPartName);

            if (selectedBodyPart == null)
                return;

            cbxWeaponSlots.Items.Clear();

            foreach (AKunitType.WeaponItem wi in selectedBodyPart.Weapons)
                cbxWeaponSlots.Items.Add(wi);

            if (cbxWeaponSlots.Items.Count > 0)
                cbxWeaponSlots.SelectedIndex = 0;
        }

        #endregion methods for setting control data

        #region methods for getting body part info

        private string GetBodyPartNameFromActiveButton()
        {
            if (btnRA.Active)
                return "RA";

            if (btnRT.Active)
                return "RT";

            if (btnCT.Active)
                return "CT";

            if (btnLA.Active)
                return "LA";

            if (btnLT.Active)
                return "LT";

            return string.Empty;
        }

        private AKunit.BP GetBodyPart(string bodyPartName)
        {
            if (spawner.Spawned as AKunit != null)
                return null;

            AKunit ut = spawner.Spawned as AKunit;

            if (ut == null)
                return null;

            AKunit.BP selectedBodyPart = null;

            foreach (AKunit.BP bp in ut.Bp)
            {
                if (bp.GUIDesplayName == bodyPartName)
                {
                    selectedBodyPart = bp;
                    break;
                }
            }

            if (selectedBodyPart == null)
                Log.Error(string.Format("Body part \"{0}\" not found on {1}", bodyPartName, spawner.Spawned.Name));

            return selectedBodyPart;
        }

        private int GetBodyPartIndex(string bodyPartName)
        {
            AKunit ut = spawner.Spawned as AKunit;

            //foreach (AKunitType.BodyPart bp in ut.BodyParts)
            for (int i = 0; i < ut.Bp.Count; i++)
            {
                AKunit.BP bp = ut.Bp[i];

                if (bp.GUIDesplayName == bodyPartName)
                    return i;
            }

            return -1;
        }

        private int GetPlayerCashSQL()
        {
            try
            {
                int commandermoney = -1;
                string sql = "SELECT Money FROM phpap_AKusers WHERE Username=@User";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter User = new MySqlParameter();
                User.ParameterName = "@User";
                User.Value = Program.username;
                cmd.Parameters.Add(User);

                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (rdr[0] != null)
                    {
                        commandermoney = (int)rdr[0];
                    }
                }

                if (commandermoney != -1)
                {
                    rdr.Close();
                    rdr.Dispose();
                }
                return commandermoney;
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Log.Info(ex.Message);
                    return -1;
                }
            }
            return -1;
        }

        private void GetListOfPlayerUnits()
        {
            try
            {
                string sql = "SELECT UnitName, UnitGameID, ListID FROM phpap_AKunits WHERE Username=@User";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter User = new MySqlParameter();
                User.ParameterName = "@User";

                User.Value = Program.username;
                cmd.Parameters.Add(User);

                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    int unitId = rdr.GetInt32("UnitGameID");
                    int listId = rdr.GetInt32("ListID");
                    string name = rdr.GetString("UnitName");

                    switch (listId)
                    {
                        case 1:
                            MechDBUnits.Add(new CustomizableUnit(unitId, name));
                            break;

                        case 2:
                            ADBUnits.Add(new CustomizableUnit(unitId, name));
                            break;

                        case 3:
                            GDBUnits.Add(new CustomizableUnit(unitId, name));
                            break;

                        case 4:
                            JDBUnits.Add(new CustomizableUnit(unitId, name));
                            break;
                    }
                }

                rdr.Close();
                rdr.Dispose();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        #endregion methods for getting body part info
    }
}