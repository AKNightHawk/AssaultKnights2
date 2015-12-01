using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using MySql.Data.MySqlClient;

//using System.Data;
using ProjectEntities;

//using System.Windows.Forms;

namespace Game
{
    /// <summary>
    /// Defines a window of map choice.
    /// </summary>
    public class PlayerBuyWindow : Control
    {
        private Button SelectedB;

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

        private PriceListC MechsPriceList;
        private PriceListC AunitPriceList;
        private PriceListC GunitPriceList;
        private PriceListC JunitPriceList;

        private Button btnMechs;
        private Button btnGroundUnits;
        private Button btnAirUnits;
        private Button btnJets;
        private Button btnBuy;

        private Button btnNext;
        private Button btnPrevious;
        private TextBox txtPageInfo;

        private Hangar mechHangar;
        private Hangar groundUnitHangar;
        private Hangar airUnitHangar;
        private Hangar jetHangar;

        private ComboBox variantList;

        private Control window;

        private int currentPage;
        private int maxPages;
        private List<CustomizableUnit> currentList;
        private PriceListC currentPriceList;

        public PlayerBuyWindow(Hangar mh, Hangar guh, Hangar vp, Hangar jh)
        {
            mechHangar = mh;
            groundUnitHangar = guh;
            airUnitHangar = vp;
            jetHangar = jh;
        }

        protected override void OnAttach()
        {
            base.OnAttach();

            window = ControlDeclarationManager.Instance.CreateControl("Gui\\PlayerBuyWindow.gui");
            Controls.Add(window);

            MechsPriceList = (PriceListC)Entities.Instance.Create("MechPriceList", Map.Instance);
            AunitPriceList = (PriceListC)Entities.Instance.Create("AunitPriceList", Map.Instance);
            GunitPriceList = (PriceListC)Entities.Instance.Create("GunitPriceList", Map.Instance);
            JunitPriceList = (PriceListC)Entities.Instance.Create("JunitPriceList", Map.Instance);

            ((Button)window.Controls["Close"]).Click += delegate(Button sender) { SetShouldDetach(); };

            btnBuy = (Button)window.Controls["Buy"];
            btnBuy.Click += new Button.ClickDelegate(btnBuy_Click);
            btnBuy.Enable = false;

            btnMechs = window.Controls["MechsB"] as Button;
            btnAirUnits = window.Controls["Aunits"] as Button;
            btnGroundUnits = window.Controls["Gunits"] as Button;
            btnJets = window.Controls["Junits"] as Button;

            btnNext = (Button)window.Controls["NextB"];
            btnNext.Click += new Button.ClickDelegate(btnNext_Click);

            btnPrevious = (Button)window.Controls["PreviousB"];
            btnPrevious.Click += new Button.ClickDelegate(btnPrevious_Click);

            txtPageInfo = (TextBox)window.Controls["Pageinfo"];

            variantList = (ComboBox)window.Controls["CustomUnit"];
            variantList.Items.Add("Stock");
            variantList.SelectedIndex = 0;
            variantList.SelectedIndexChange += new ComboBox.SelectedIndexChangeDelegate(variantList_SelectedIndexChange);

            if (mechHangar != null)
                btnMechs.Click += new Button.ClickDelegate(btnMechs_Click);
            else
                btnMechs.Enable = false;

            if (groundUnitHangar != null)
                btnGroundUnits.Click += new Button.ClickDelegate(btnGroundUnits_Click);
            else
                btnGroundUnits.Enable = false;

            if (airUnitHangar != null)
                btnAirUnits.Click += new Button.ClickDelegate(btnAirUnits_Click);
            else
                btnAirUnits.Enable = false;

            if (jetHangar != null)
                btnJets.Click += new Button.ClickDelegate(btnJets_Click);
            else
                btnJets.Enable = false;

            GetListOfPlayerUnits();

            SetupFirstList();
        }

        private void SetupFirstList()
        {
            if (mechHangar != null)
            {
                SetUnitTypeButtonActive(btnMechs);
                UpdateDisplayedList(MechDBUnits, MechsPriceList);
            }
            else if (groundUnitHangar != null)
            {
                SetUnitTypeButtonActive(btnGroundUnits);
                UpdateDisplayedList(GDBUnits, GunitPriceList);
            }
            else if (airUnitHangar != null)
            {
                SetUnitTypeButtonActive(btnAirUnits);
                UpdateDisplayedList(ADBUnits, AunitPriceList);
            }
            else if (jetHangar != null)
            {
                SetUnitTypeButtonActive(btnJets);
                UpdateDisplayedList(JDBUnits, JunitPriceList);
            }
        }

        private void btnBuy_Click(Button sender)
        {
            //find hangar for what we are trying to spawn

            Hangar selectedHangar = null;

            if (currentList == MechDBUnits)
                selectedHangar = mechHangar;

            if (currentList == GDBUnits)
                selectedHangar = groundUnitHangar;

            if (currentList == ADBUnits)
                selectedHangar = airUnitHangar;

            if (currentList == JDBUnits)
                selectedHangar = jetHangar;

            string un = currentPriceList.Type.PriceLists[
                int.Parse(SelectedB.Controls["RelatedUnitID"].Text)].PricedUnit.Name;

            UnitType ut = (UnitType)EntityTypes.Instance.GetByName(un);

            TextBlock variant = null;

            if (variantList.SelectedIndex > 0)
            {
                string varName = string.Format("{0}\\Variants\\{1}\\{2}",
                    VirtualFileSystem.UserDirectoryPath, ut.Name, variantList.SelectedItem.ToString());

                variant = TextBlockUtils.LoadFromRealFile(varName);
            }

            //parse TextBlock to int[] so it can be networked more efficiently

            AKunitType akt = ut as AKunitType;

            int[] varData = null;
            string varDataString = string.Empty;

            if (variant != null)
            {
                //calculate the elements needed for the compressed variant data
                {
                    int elementCount = 0;
                    foreach (TextBlock c in variant.Children)
                        elementCount += c.Children.Count;

                    elementCount *= 4;

                    varData = new int[elementCount];
                }

                int x = 0;
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
                                int bodyPartWeaponIndex = j;

                                int alternateWeaponIndex = int.Parse(bodyPartWeaponBlock.Attributes[0].Value);
                                int fireGroup = int.Parse(bodyPartWeaponBlock.Attributes[1].Value);
                                varData[x] = bodyPartIndex;
                                varData[x + 1] = bodyPartWeaponIndex;
                                varData[x + 2] = alternateWeaponIndex;
                                varData[x + 3] = fireGroup;

                                if (varDataString != string.Empty)
                                    varDataString += ":";

                                varDataString += string.Format("{0}:{1}:{2}:{3}",
                                    bodyPartIndex, bodyPartWeaponIndex, alternateWeaponIndex, fireGroup);
                                x += 4;
                            }
                        }
                    }
                }
            }

            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                selectedHangar.Client_SendSpawnRequestToServer(ut.Name, varDataString);
            }
            else
            {
                selectedHangar.SpawnNewUnit(ut, varData);
            }

            SetShouldDetach();
        }

        private void btnMechs_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);
            ClearOtherButtons(string.Empty);
            currentList = MechDBUnits;
            UpdateDisplayedList(MechDBUnits, MechsPriceList);
        }

        private void btnGroundUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);
            ClearOtherButtons(string.Empty);
            currentList = GDBUnits;
            UpdateDisplayedList(GDBUnits, GunitPriceList);
        }

        private void btnAirUnits_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);
            ClearOtherButtons(string.Empty);
            currentList = ADBUnits;
            UpdateDisplayedList(ADBUnits, AunitPriceList);
        }

        private void btnJets_Click(Button sender)
        {
            SetUnitTypeButtonActive(sender);
            ClearOtherButtons(string.Empty);
            UpdateDisplayedList(JDBUnits, JunitPriceList);
        }

        private void btnNext_Click(Button sender)
        {
            ++currentPage;
            UpdateDisplayedList(null, null);
        }

        private void btnPrevious_Click(Button sender)
        {
            --currentPage;
            UpdateDisplayedList(null, null);
        }

        private void SetUnitTypeButtonActive(Button sender)
        {
            btnMechs.Active = false;
            btnGroundUnits.Active = false;
            btnAirUnits.Active = false;
            btnJets.Active = false;

            sender.Active = true;

            currentPage = 1;
        }

        private void ClearOtherButtons(string except)
        {
            for (int i = 0; i < 15; i++)
                ((Button)window.Controls["BB" + (i + 1).ToString()]).Active = false;

            variantList.Items.Clear();
            variantList.Items.Add("Stock");
            variantList.SelectedIndex = 0;

            Button activeButton = window.Controls[except] as Button;
            if (activeButton != null)
            {
                btnBuy.Enable = true;
                activeButton.Active = true;

                string varDir = string.Format("{0}\\Variants\\{1}", VirtualFileSystem.UserDirectoryPath,
                    activeButton.Controls["MechName"].Text);

                if (Directory.Exists(varDir))
                {
                    DirectoryInfo di = new DirectoryInfo(varDir);
                    FileInfo[] files = di.GetFiles();

                    foreach (FileInfo file in files)
                        variantList.Items.Add(file.Name);
                }
            }
            else
                btnBuy.Enable = false;
        }

        private void UpdateDisplayedList(List<CustomizableUnit> unitList, PriceListC unitPriceList)
        {
            if (unitList != null)
                currentList = unitList;

            if (unitPriceList != null)
                currentPriceList = unitPriceList;

            maxPages = 1 + (int)(MechDBUnits.Count / 15);

            UpdatePageDisplay();

            int maxIndex = (currentPage - 1) * 15;

            for (int i = 0; i < 15; i++)
            {
                Button BB = window.Controls["BB" + (i + 1).ToString()] as Button;
                TextBox Mechname = BB.Controls["MechName"] as TextBox;
                TextBox MechVar = BB.Controls["MechVar"] as TextBox;
                Control MechIcon = BB.Controls["MechIcon"] as Control;
                Control RelatedUnitID = BB.Controls["RelatedUnitID"] as Control;

                if (i + maxIndex >= currentList.Count)
                {
                    BB.Visible = false;
                }
                else
                {
                    int FinalUnitIndex = currentList[i + maxIndex].ID;

                    BB.Visible = true;

                    UnitType ut = currentPriceList.Type.PriceLists[FinalUnitIndex].PricedUnit as UnitType;

                    Mechname.Text = currentPriceList.Type.PriceLists[FinalUnitIndex].Name;
                    MechVar.Text = string.Empty;
                    MechIcon.BackTexture = TextureManager.Instance.Load(
                        "Assault Knights\\Huds\\UnitReadouts\\" + ut.Name.ToString());
                    RelatedUnitID.Text = FinalUnitIndex.ToString();

                    BB.Click += delegate(Button sender)
                    {
                        BB.Active = true;
                        ClearOtherButtons(BB.Name);
                        SelectedB = BB;
                    };
                }
            }
        }

        private void UpdatePageDisplay()
        {
            btnNext.Enable = (currentPage < maxPages);
            btnPrevious.Enable = (currentPage > 1);

            txtPageInfo.Text = string.Format("Page: {0} of {1}", currentPage, maxPages);
        }

        private void variantList_SelectedIndexChange(ComboBox sender)
        {
            //todo: load variants available for this unit
            //throw new NotImplementedException();
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
                //rdr.Dispose();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Log.Error(ex.Message);
                }
            }
        }

        private Unit GetPlayerUnit()
        {
            if (PlayerIntellect.Instance == null)
                return null;
            return PlayerIntellect.Instance.ControlledObject;
        }

        private void SetInfo(string text, bool error)
        {
            TextBox textBoxInfo = (TextBox)window.Controls["Info"];
            textBoxInfo.Text = text;
            textBoxInfo.TextColor = error ? new ColorValue(1, 0, 0) : new ColorValue(0, 1, 0);
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