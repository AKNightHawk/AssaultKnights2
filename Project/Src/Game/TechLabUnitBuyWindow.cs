using System;
using System.Collections.Generic;

//using System.Data;
//using System.Windows.Forms;
using System.ComponentModel;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using MySql.Data.MySqlClient;
using ProjectEntities;

namespace Game
{
    public class TechLabUnitBuyWindow : Control
    {
        public delegate void OnBuyGuiUpdate(string text, bool error);

        public delegate void OnBuyLog(string text);

        //for selecting our units
        private Button btnMechs;

        private Button btnGroundUnits;
        private Button btnAirUnits;
        private Button btnJets;
        private Button btnNext;
        private Button btnPrevious;

        //unit info
        private TextBox txtUnitName;

        private TextBox txtUnitCost;
        private TextBox txtCash;

        //misc
        private Button btnBuy;

        private Button btnExit;

        private ListBox lstWeapons;

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

        //Control Controls;

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

        protected override void OnAttach()
        {
            window = ControlDeclarationManager.Instance.CreateControl("Gui\\TechLabUnitBuyWindow.gui");
            Controls.Add(window);

            if (spawner == null)
                spawner = Entities.Instance.GetByName("TechlabMechSpawn") as Spawner;
            else
                spawner = Entities.Instance.GetByName("TechlabMechSpawn") as Spawner;

            if (camera == null)
                camera = Entities.Instance.GetByName("TechlabCam") as MapCamera;
            else
                camera = Entities.Instance.GetByName("TechlabCam") as MapCamera;

            btnMechs = (Button)window.Controls["Mechs"];
            btnMechs.Click += new Button.ClickDelegate(btnMechs_Click);

            btnGroundUnits = (Button)window.Controls["Gunit"];
            btnGroundUnits.Click += new Button.ClickDelegate(btnGroundUnits_Click);

            btnAirUnits = (Button)window.Controls["Aunit"];
            btnAirUnits.Click += new Button.ClickDelegate(btnAirUnits_Click);

            btnJets = (Button)window.Controls["Junit"];
            btnJets.Click += new Button.ClickDelegate(btnJets_Click);

            btnExit = (Button)window.Controls["Quit"];
            btnExit.Click += new Button.ClickDelegate(btnExit_Click);

            btnNext = (Button)window.Controls["Next"];
            btnNext.Click += new Button.ClickDelegate(btnNext_Click);

            btnPrevious = (Button)window.Controls["Previous"];
            btnPrevious.Click += new Button.ClickDelegate(btnPrevious_Click);

            btnBuy = (Button)window.Controls["Buy"];
            btnBuy.Click += new Button.ClickDelegate(btnBuy_Click);

            txtUnitName = (TextBox)window.Controls["UnitName"];
            txtUnitCost = (TextBox)window.Controls["UnitCost"];
            txtCash = (TextBox)window.Controls["Cash"];

            txtCash.Text = "Cash: " + cash.ToString();

            lstWeapons = (ListBox)window.Controls["Weapons"];

            spawner.UnitSpawned += new Spawner.OnUnitSpawned(spawner_UnitSpawned);

            InitCameraViewFromTarget();
            //positionY = maxout;

            MechsPriceList = (PriceListC)Entities.Instance.Create("MechPriceList", Map.Instance);
            AunitPriceList = (PriceListC)Entities.Instance.Create("AunitPriceList", Map.Instance);
            GunitPriceList = (PriceListC)Entities.Instance.Create("GunitPriceList", Map.Instance);
            JunitPriceList = (PriceListC)Entities.Instance.Create("JunitPriceList", Map.Instance);

            GetListOfPlayerUnits();

            cash = GetPlayerCashSQL();

            base.OnAttach();
        }

        private void spawner_UnitSpawned(Unit unit)
        {
            AKunit u = unit as AKunit;

            if (u == null)
                return;

            lstWeapons.Items.Clear();

            foreach (AKunit.WeaponItem item in u.Weapons)
            {
                lstWeapons.Items.Add(item.Weapon.Type.Name);
            }
        }

        private void btnBuy_Click(Button sender)
        {
            BackgroundWorker buyWorker = new BackgroundWorker();
            SetInfo("Sending purchase request...", false);
            buyWorker.DoWork += new DoWorkEventHandler(buyWorker_DoWork);
            buyWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(buyWorker_RunWorkerCompleted);
            buyWorker.RunWorkerAsync();
        }

        private void buyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetInfo("Purchase complete. Have a nice day", false);
        }

        private void buyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (activeList == Active_List.None)
                return;

            string unitName = string.Empty;
            int unitCost = 0;
            int listId = 0;
            int listIndex = 0;
            List<CustomizableUnit> boughtUnitList = null;

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        unitName = MechsPriceList.Type.PriceLists[mechListIndex].Name;
                        unitCost = MechsPriceList.Type.PriceLists[mechListIndex].Price;
                        listId = 1;
                        listIndex = mechListIndex;
                        boughtUnitList = MechDBUnits;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        unitName = AunitPriceList.Type.PriceLists[airUnitListIndex].Name;
                        unitCost = AunitPriceList.Type.PriceLists[airUnitListIndex].Price;
                        listId = 2;
                        listIndex = airUnitListIndex;
                        boughtUnitList = ADBUnits;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        unitName = GunitPriceList.Type.PriceLists[groundUnitListIndex].Name;
                        unitCost = GunitPriceList.Type.PriceLists[groundUnitListIndex].Price;
                        listId = 3;
                        listIndex = groundUnitListIndex;
                        boughtUnitList = GDBUnits;
                    }
                    break;

                case Active_List.Jet:
                    {
                        unitName = JunitPriceList.Type.PriceLists[jetUnitListIndex].Name;
                        unitCost = JunitPriceList.Type.PriceLists[jetUnitListIndex].Price;
                        listId = 4;
                        listIndex = jetUnitListIndex;
                        boughtUnitList = JDBUnits;
                    }
                    break;
            }

            try
            {
                //Decreasing Credit
                string sql = "UPDATE phpap_AKusers SET money = @DeductedCash WHERE Username=@User";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter DeductedCashP = new MySqlParameter();
                DeductedCashP.ParameterName = "@DeductedCash";
                DeductedCashP.Value = cash - unitCost;
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
                    OnBuyGuiUpdate update = new OnBuyGuiUpdate(SetInfo);
                    update.Invoke(ex.Message, true);
                }
            }

            try
            {
                string sql = "INSERT INTO phpap_AKunits (UnitName, Username, UnitGameID, ListID) VALUES (@Unitname, @Username, @Unitgameid, @Listid)";
                MySqlCommand cmd = new MySqlCommand(sql, Program.AKsqlcon);

                MySqlParameter Unitname = new MySqlParameter();
                MySqlParameter Username = new MySqlParameter();
                MySqlParameter Unitgameid = new MySqlParameter();
                MySqlParameter Listid = new MySqlParameter();

                Unitname.ParameterName = "@Unitname";
                Username.ParameterName = "@Username";
                Unitgameid.ParameterName = "@Unitgameid";
                Listid.ParameterName = "@Listid";

                Unitname.Value = unitName;
                Username.Value = Program.username;
                Unitgameid.Value = listIndex;
                Listid.Value = listId;
                //cmd.Connection.Open();
                cmd.Parameters.Add(Unitname); cmd.Parameters.Add(Username); cmd.Parameters.Add(Unitgameid); cmd.Parameters.Add(Listid);

                cmd.ExecuteNonQuery();

                cash -= unitCost;
                txtCash.Text = "Cash: " + cash.ToString();
                boughtUnitList.Add(new CustomizableUnit(listId, unitName));
                btnBuy.Enable = false;
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    OnBuyGuiUpdate update = new OnBuyGuiUpdate(SetInfo);
                    update.Invoke(ex.Message, true);
                }
            }
        }

        private void btnMechs_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.Mech;

            spawner.SpawnUnit(MechsPriceList.Type.PriceLists[mechListIndex].PricedUnit);

            string name = MechsPriceList.Type.PriceLists[mechListIndex].Name;
            int cost = MechsPriceList.Type.PriceLists[mechListIndex].Price;
            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in MechDBUnits)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);
        }

        private void btnGroundUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.GroundUnit;

            spawner.SpawnUnit(GunitPriceList.Type.PriceLists[groundUnitListIndex].PricedUnit);

            string name = GunitPriceList.Type.PriceLists[groundUnitListIndex].Name;
            int cost = GunitPriceList.Type.PriceLists[groundUnitListIndex].Price;
            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in GDBUnits)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);
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
            //base.OnDetach(); //fix akunitbuy and akcustomize INCIN
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
            //    //new rmCamera=
            //}
            //else
            //{
            rmCamera.LookAt(spawner.Position);
            rmCamera.FarClipDistance = (spawner.Position - rmCamera.Position).Length() * 10f;
            rmCamera.Position = camera.Position;
            rmCamera.FixedUp = (Vec3.ZAxis);
            rmCamera.Visible = true;
            rmCamera.PolygonMode = PolygonMode.Wireframe;
            rmCamera.NearClipDistance = 1.0f;
            rmCamera.Fov = camera.Fov;
            //}
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

        private void btnAirUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.AirUnit;

            spawner.SpawnUnit(AunitPriceList.Type.PriceLists[airUnitListIndex].PricedUnit);

            string name = AunitPriceList.Type.PriceLists[airUnitListIndex].Name;
            int cost = AunitPriceList.Type.PriceLists[airUnitListIndex].Price;
            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in ADBUnits)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);
        }

        private void btnJets_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);

            activeList = Active_List.Jet;

            spawner.SpawnUnit(JunitPriceList.Type.PriceLists[jetUnitListIndex].PricedUnit);

            string name = JunitPriceList.Type.PriceLists[jetUnitListIndex].Name;
            int cost = JunitPriceList.Type.PriceLists[jetUnitListIndex].Price;
            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in JDBUnits)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);
        }

        private void btnNext_Click(Button sender)
        {
            if (activeList == Active_List.None)
                return;

            int index = 0;
            int max = 0;
            UnitType u = null;
            string name = "No Name";
            int cost = 0;

            List<CustomizableUnit> list = null;

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        index = mechListIndex;
                        max = MechsPriceList.Type.PriceLists.Count;
                        list = MechDBUnits;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        index = airUnitListIndex;
                        max = AunitPriceList.Type.PriceLists.Count;
                        list = ADBUnits;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        index = groundUnitListIndex;
                        max = GunitPriceList.Type.PriceLists.Count;
                        list = GDBUnits;
                    }
                    break;

                case Active_List.Jet:
                    {
                        index = jetUnitListIndex;
                        max = JunitPriceList.Type.PriceLists.Count;
                        list = JDBUnits;
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
                        u = MechsPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = MechsPriceList.Type.PriceLists[newIndex].Name;
                        cost = MechsPriceList.Type.PriceLists[newIndex].Price;
                        mechListIndex = newIndex;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        u = AunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = AunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = AunitPriceList.Type.PriceLists[newIndex].Price;
                        airUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        u = GunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = GunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = GunitPriceList.Type.PriceLists[newIndex].Price;
                        groundUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.Jet:
                    {
                        u = JunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = JunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = JunitPriceList.Type.PriceLists[newIndex].Price;
                        jetUnitListIndex = newIndex;
                    }
                    break;
            }

            spawner.SpawnUnit(u);

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in list)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);

            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();
        }

        private void btnPrevious_Click(Button sender)
        {
            if (activeList == Active_List.None)
                return;

            int index = 0;
            int max = 0;
            UnitType u = null;
            string name = "No Name";
            int cost = 0;

            List<CustomizableUnit> list = null;

            switch (activeList)
            {
                case Active_List.Mech:
                    {
                        index = mechListIndex;
                        max = MechsPriceList.Type.PriceLists.Count;
                        list = MechDBUnits;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        index = airUnitListIndex;
                        max = AunitPriceList.Type.PriceLists.Count;
                        list = ADBUnits;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        index = groundUnitListIndex;
                        max = GunitPriceList.Type.PriceLists.Count;
                        list = GDBUnits;
                    }
                    break;

                case Active_List.Jet:
                    {
                        index = jetUnitListIndex;
                        max = JunitPriceList.Type.PriceLists.Count;
                        list = JDBUnits;
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
                        u = MechsPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = MechsPriceList.Type.PriceLists[newIndex].Name;
                        cost = MechsPriceList.Type.PriceLists[newIndex].Price;
                        mechListIndex = newIndex;
                    }
                    break;

                case Active_List.AirUnit:
                    {
                        u = AunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = AunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = AunitPriceList.Type.PriceLists[newIndex].Price;
                        airUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.GroundUnit:
                    {
                        u = GunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = GunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = GunitPriceList.Type.PriceLists[newIndex].Price;
                        groundUnitListIndex = newIndex;
                    }
                    break;

                case Active_List.Jet:
                    {
                        u = JunitPriceList.Type.PriceLists[newIndex].PricedUnit;
                        name = JunitPriceList.Type.PriceLists[newIndex].Name;
                        cost = JunitPriceList.Type.PriceLists[newIndex].Price;
                        jetUnitListIndex = newIndex;
                    }
                    break;
            }
            spawner.SpawnUnit(u);

            bool alreadyPurchased = false;
            foreach (CustomizableUnit cu in list)
            {
                if (cu.Name == name)
                {
                    alreadyPurchased = true;
                    break;
                }
            }

            btnBuy.Enable = !(alreadyPurchased || cash < cost);

            txtUnitName.Text = "Name: " + name;
            txtUnitCost.Text = "Cost: " + cost.ToString();
        }

        private void btnExit_Click(Button sender)
        {
            SetShouldDetach();
            //Back(true);
        }

        private void SetUnitTypeButtonActive(Button sender)
        {
            btnMechs.Active = false;
            btnGroundUnits.Active = false;
            btnAirUnits.Active = false;
            btnJets.Active = false;

            sender.Active = true;
        }

        private void SetInfo(string text, bool error)
        {
            TextBox textBoxInfo = (TextBox)window.Controls["Info"];
            textBoxInfo.Text = text;
            textBoxInfo.TextColor = error ? new ColorValue(1, 0, 0) : new ColorValue(1, 1, 1);
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
    }
}