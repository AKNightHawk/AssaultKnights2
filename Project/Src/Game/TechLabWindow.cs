using MySql.Data.MySqlClient;

using ProjectCommon;
using ProjectEntities;

using System;
using System.Collections.Generic;
using System.Text;
using Engine.UISystem;
using Engine.Renderer;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine;

namespace Game
{
    public class TechLabWindow : Control
    {
        public delegate void OnBuyGuiUpdate(string text, bool error);
        public delegate void OnBuyLog(string text);

        public bool Go = false;
        public float maxout = -800;
        public static float maxin = 0f;
        public static float Rate = 1000;
        public float positionY;
        public bool done = false;

        protected Spawner spawner;
        Texture cameraTexture;
        Camera rmCamera;
        MapCamera camera;

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

        protected override void OnAttach()
        {
            base.OnAttach();
            positionY = maxout;

            if (spawner == null)
                spawner = Entities.Instance.GetByName("TechlabMechSpawn") as Spawner;

            if (camera == null)
                camera = Entities.Instance.GetByName("TechlabCam") as MapCamera;

            MechsPriceList = (PriceListC)Entities.Instance.Create("MechPriceList", Map.Instance);
            AunitPriceList = (PriceListC)Entities.Instance.Create("AunitPriceList", Map.Instance);
            GunitPriceList = (PriceListC)Entities.Instance.Create("GunitPriceList", Map.Instance);
            JunitPriceList = (PriceListC)Entities.Instance.Create("JunitPriceList", Map.Instance);

            GetListOfPlayerUnits();

            cash = GetPlayerCashSQL();

            InitCameraViewFromTarget();
            //positionY = maxout;
        }

        //protected override void OnTick(float delta)
        //{
        //    base.OnTick(delta);
        //    //Back(false);

        //    //if (!Go)
        //    //{
        //    //    // slide window in 
        //    //    if (positionY < maxin)
        //    //        positionY += (delta * Rate);
        //    //    else
        //    //        positionY = maxin;
        //    //}
        //    //else
        //    //{
        //    //    // slide window out
        //    //    if (positionY > maxout)
        //    //        positionY -= (delta * Rate);
        //    //    else
        //    //        positionY = maxout;
        //    //}

        //    //window.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(-30, positionY));

        //    //if (Go)
        //    //    return;
        //}

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;
            if (e.Key == EKeys.Escape)
            {
                Back(true);
                return true;
            }
            return false;
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            if (spawner.Spawned != null)
            {
                spawner.Spawned.SetDeleted();
                spawner.Spawned = null;
            }

        }

        void InitCameraViewFromTarget()
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

        void shouldgo()
        {
            Go = true;
        }

        protected void Back(bool reallygo)
        {
            if (reallygo)
                shouldgo();

            if (positionY == maxout && Go && !done)
            {
                /* if (Map.Instance.Name.ToString() == "MainMap")
                {
                    this.OnDetach();
                    //Controls.Add(new MainMenuWindow());
                    MainMenuWindow.Instance.Go = false;
                    done = true;
                }
                else
                {
                    SetShouldDetach();
                 }*/

                this.OnDetach();

                Controls.Add(new MPAKlobbyWindow());

                done = true;
            }
        }
    }
}
