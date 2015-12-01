// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System.Collections.Generic;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.UISystem;
using Engine.Utils;
using ProjectCommon;

namespace Game
{
    /// <summary>
    /// Defines a main menu.
    /// </summary>
    public class MainMenuWindow : Control
    {
        private static MainMenuWindow instance;

        private static List<MapCameraCurve> cameraCurves;
        private static float cameraCurvesTotalTime = 0;
        //float curveTime = 0;

        private Control window;
        //TextBox versionTextBox;

        private Map mapInstance;

        //camera
        private static float moveTime = 0;

        [Config("MainMenu", "showBackgroundMap")]
        private static bool showBackgroundMap = true;

        ///////////////////////////////////////////

        public static MainMenuWindow Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// Creates a window of the main menu and creates the background world.
        /// </summary>
        protected override void OnAttach()
        {
            instance = this;
            base.OnAttach();

            //for showBackgroundMap field.
            EngineApp.Instance.Config.RegisterClassParameters(GetType());

            //create main menu window
            window = ControlDeclarationManager.Instance.CreateControl("Gui\\MainMenuWindow.gui");

            window.ColorMultiplier = new ColorValue(1, 1, 1, 0);
            Controls.Add(window);

            //no shader model 3 warning
            if (window.Controls["NoShaderModel3"] != null)
                window.Controls["NoShaderModel3"].Visible = !RenderSystem.Instance.HasShaderModel3();

            //button handlers
            if (window.Controls["Run"] != null)
                ((Button)window.Controls["Run"]).Click += Run_Click;
            if (window.Controls["Multiplayer"] != null)
                ((Button)window.Controls["Multiplayer"]).Click += Multiplayer_Click;
            if (window.Controls["Maps"] != null)
                ((Button)window.Controls["Maps"]).Click += Maps_Click;
            if (window.Controls["LoadSave"] != null)
                ((Button)window.Controls["LoadSave"]).Click += LoadSave_Click;
            if (window.Controls["Options"] != null)
                ((Button)window.Controls["Options"]).Click += Options_Click;
            if (window.Controls["MultiView"] != null)
                ((Button)window.Controls["MultiView"]).Click += MultiView_Click;
            if (window.Controls["ProfilingTool"] != null)
                ((Button)window.Controls["ProfilingTool"]).Click += ProfilingTool_Click;
            if (window.Controls["GUISamples1"] != null)
                ((Button)window.Controls["GUISamples1"]).Click += GUISamples1_Click;
            if (window.Controls["GUISamples2"] != null)
                ((Button)window.Controls["GUISamples2"]).Click += GUISamples2_Click;
            if (window.Controls["GUISamples3"] != null)
                ((Button)window.Controls["GUISamples3"]).Click += GUISamples3_Click;
            if (window.Controls["About"] != null)
                ((Button)window.Controls["About"]).Click += About_Click;
            if (window.Controls["Exit"] != null)
                ((Button)window.Controls["Exit"]).Click += Exit_Click;

            ////add version info control
            //versionTextBox = new TextBox();
            //versionTextBox.TextHorizontalAlign = HorizontalAlign.Left;
            //versionTextBox.TextVerticalAlign = VerticalAlign.Bottom;
            //versionTextBox.Text = "Version " + EngineVersionInformation.Version;
            //versionTextBox.ColorMultiplier = new ColorValue( 1, 1, 1, 0 );
            //Controls.Add( versionTextBox );

            //showBackgroundMap check box
            CheckBox checkBox = (CheckBox)window.Controls["ShowBackgroundMap"];
            if (checkBox != null)
            {
                checkBox.Checked = showBackgroundMap;
                checkBox.Click += checkBoxShowBackgroundMap_Click;
            }

            //play background music
            //if(GameMap.Instance.GameType == GameMap.GameTypes.AssaultKnights)
            GameMusic.MusicPlay("Assault Knights\\Music\\Title-AssaultKnights.ogg", true);
            //else
            //GameMusic.MusicPlay( "Sounds\\Music\\MainMenu.ogg", true );

            //update sound listener
            SoundWorld.Instance.SetListener(new Vec3(1000, 1000, 1000),
                Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));

            //create the background world
            if (showBackgroundMap)
                CreateMap();

            //iNCIN
            GenerateCameraCurvesList();

            ResetTime();
        }

        private void checkBoxShowBackgroundMap_Click(CheckBox sender)
        {
            showBackgroundMap = sender.Checked;

            if (showBackgroundMap)
                CreateMap();
            else
                DestroyMap();
        }

        private void Run_Click(Button sender)
        {
            //shouldgo();
            GameEngineApp.Instance.SetNeedMapLoad("Demos\\MainDemo\\Map.map");
        }

        private void RunVillageDemo_Click(Button sender)
        {
            //shouldgo();
            GameEngineApp.Instance.SetNeedMapLoad("Demos\\VillageDemo\\Map\\Map.map");
        }

        private void Multiplayer_Click(Button sender)
        {
            Controls.Add(new MultiplayerLoginWindow()); //Incin -- new MultiplayerLoginWindow() revert from SQL
        }

        private void Maps_Click(Button sender)
        {
            Controls.Add(new MapsWindow());
        }

        private void LoadSave_Click(Button sender)
        {
            Controls.Add(new WorldLoadSaveWindow());
        }

        private void Options_Click(Button sender)
        {
            Controls.Add(new OptionsWindow());
        }

        private void ProfilingTool_Click(Button sender)
        {
            if (ProfilingToolWindow.Instance == null)
                Controls.Add(new ProfilingToolWindow());
        }

        private void GUISamples1_Click(Button sender)
        {
            GameEngineApp.Instance.ControlManager.Controls.Add(new GUISamples1Window());
        }

        private void GUISamples2_Click(Button sender)
        {
            GameEngineApp.Instance.ControlManager.Controls.Add(new GUISamples2Window());
        }

        private void GUISamples3_Click(Button sender)
        {
            GameEngineApp.Instance.ControlManager.Controls.Add(new GUISamples3Window());
        }

        private void MultiView_Click(Button sender)
        {
            Controls.Add(new MultiViewConfigurationWindow());
        }

        private void About_Click(Button sender)
        {
            GameEngineApp.Instance.ControlManager.Controls.Add(new AboutWindow());
        }

        private void Exit_Click(Button sender)
        {
            GameEngineApp.Instance.SetFadeOutScreenAndExit();
        }

        /// <summary>
        /// Destroys the background world at closing the main menu.
        /// </summary>
        protected override void OnDetach()
        {
            //destroy the background world
            DestroyMap();

            base.OnDetach();
            instance = null;
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            if (base.OnKeyDown(e))
                return true;

            //if( e.Key == EKeys.Escape )
            //{
            //   Controls.Add( new MenuWindow() );
            //   return true;
            //}

            return false;
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);

            //Camera timestepping
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                //iNCIN
                if (Map.Instance != null && cameraCurves != null)
                {
                    float step = RendererWorld.Instance.FrameRenderTimeStep;
                    if (EngineApp.Instance.IsKeyPressed(EKeys.C))
                        step *= 10;

                    moveTime += step;
                    if (moveTime >= cameraCurvesTotalTime)
                        moveTime = 0;
                }
            }

            //Change window transparency
            {
                float alpha = 0;

                if (Time > 3 && Time <= 5)
                    alpha = (Time - 3) / 2;
                else if (Time > 4)
                    alpha = 1;

                window.ColorMultiplier = new ColorValue(1, 1, 1, alpha);
                //versionTextBox.ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
            }

            //update sound listener
            SoundWorld.Instance.SetListener(new Vec3(1000, 1000, 1000),
                Vec3.Zero, new Vec3(1, 0, 0), new Vec3(0, 0, 1));

            //Tick a background world
            if (EntitySystemWorld.Instance != null)
                EntitySystemWorld.Instance.Tick();
        }

        private void GenerateCameraCurvesList()
        {
            //cameraCurves
            cameraCurves = new List<MapCameraCurve>();
            foreach (Entity entity in Map.Instance.Children)
            {
                MapCameraCurve curve = entity as MapCameraCurve;
                if (curve != null)
                {
                    cameraCurves.Add(curve);
                }
            }
            ListUtils.SelectionSort(cameraCurves, delegate(MapCameraCurve curve1, MapCameraCurve curve2)
            {
                return string.Compare(curve1.Name, curve2.Name);
            });

            cameraCurvesTotalTime = 0;
            foreach (MapCameraCurve curve in cameraCurves)
            {
                //curveTime += curve.Time;
                cameraCurvesTotalTime += curve.GetCurveMaxTime();
            }

            //EngineConsole.Instance.Print("CurveTime = " + curveTime.ToString() + " cameraCurvesTotalTime = "  + cameraCurvesTotalTime.ToString());
        }

        private void GetMapCameraCurvePoint(out MapCurvePoint point, float MapCameraCurvePointTime)
        {
            point = null;
            //MapCameraCurvePointTime = 0;
            MapCurve mapCurve = (MapCurve)Entities.Instance.GetByName("MM_Curve");
            //MapCameraCurvePoint curvepoint;

            //for(int i = 0; i < mapCurve.Points.Count; i++)
            foreach (MapCurvePoint curvepoint in mapCurve.Points)
            {
                //curvepoint = mapCurve.Points.

                if (curvepoint.Time >= mapCurve.GetCurveTimeRange().Minimum &&
                   curvepoint.Time >= mapCurve.GetCurveTimeRange().Maximum)
                {
                    point = curvepoint;
                    return;
                }
            }
        }

        private void GetMapCurve(out MapCameraCurve outCurve, out float outCurveTime)
        {
            outCurve = null;
            outCurveTime = 0;
            float remainingTime = moveTime;
            //float addedtime = 0;

            //iNCIN
            foreach (MapCameraCurve curve in cameraCurves)
            {
                float length = curve.GetCurveMaxTime();
                if (remainingTime <= length)
                {
                    outCurve = curve;
                    outCurveTime = remainingTime;
                    return;
                }
                remainingTime -= length;

                if (remainingTime < 0)
                    break;
            }
        }

        //Assault Knights Code
        /*
		protected override void OnRender()
		{
			base.OnRender();

			//Update camera orientation
            if (Map.Instance != null && cameraCurves != null )
			{
                float curveTime;
                Vec3 position = Vec3.Zero;
                Vec3 forward = Vec3.Zero;
                Vec3 up = Vec3.Zero;
                Degree fov = 90;

				MapCamera mapCamera = Entities.Instance.GetByName( "MapCamera_MainMenu" ) as MapCamera;

                if (mapCamera == null)
                    return;

				if( mapCamera != null )
				{
                    //position = mapCamera.Position;
                    //forward = position + mapCamera.Rotation.GetForward();
					if( mapCamera.Fov != 0 )
						fov = mapCamera.Fov;

                    //curve /////////////////////////////////////////////////////////////
                    MapCurve mapCurve = (MapCurve)Entities.Instance.GetByName("MM_Curve");

                    if (mapCurve == null)
                        return;

                    MapCameraCurve curve;
                    MapCurvePoint point;

                    GetMapCurve(out curve, out curveTime);
                    GetMapCameraCurvePoint(out point, curveTime);

                    EngineConsole.Instance.Print("Point UIN = " + point.UIN.ToString() + " cameraCurveTime = " + curveTime.ToString());

                    if (curve != null)
                    {
                        curve.CalculateCameraPositionByTime(curveTime, out position, out forward,  out up, out fov);
                        moveTime = curveTime;
				}

				    Camera camera = RendererWorld.Instance.DefaultCamera;
				    camera.NearClipDistance = Map.Instance.NearFarClipDistance.Minimum;
				    camera.FarClipDistance = Map.Instance.NearFarClipDistance.Maximum;
                    camera.FixedUp = up;
                    camera.Fov = fov;
                    camera.Position = position;
                    //camera.Rotation = mapCamera.Rotation;
                    camera.LookAt(forward);
                }
                else
                {
                    Camera camera = RendererWorld.Instance.DefaultCamera;
                    camera.NearClipDistance = Map.Instance.NearFarClipDistance.Minimum;
                    camera.FarClipDistance = Map.Instance.NearFarClipDistance.Maximum;
                    //camera.FixedUp = Vec3.ZAxis;
				    camera.Fov = fov;
                    camera.Position = position;
                    camera.LookAt(forward);
                }
				//update game specific options
				{
					//water reflection level
					foreach( WaterPlane waterPlane in WaterPlane.Instances )
						waterPlane.ReflectionLevel = GameEngineApp.WaterReflectionLevel;

					//decorative objects
					if( DecorativeObjectManager.Instance != null )
						DecorativeObjectManager.Instance.Visible = GameEngineApp.ShowDecorativeObjects;

					//HeightmapTerrain
					//enable simple rendering for Low material scheme.
					foreach( HeightmapTerrain terrain in HeightmapTerrain.Instances )
						terrain.SimpleRendering = GameEngineApp.MaterialScheme == MaterialSchemes.Low;
				}
            }
        }
        */

        protected override void OnRender()
        {
            base.OnRender();
            UpdateCamera();
        }

        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);

            if (Map.Instance == null)
            {
                renderer.AddQuad(new Rect(0, 0, 1, 1),
                    new ColorValue(.2f, .2f, .2f) * window.ColorMultiplier);
            }
        }

        private MapCamera GetMapCamera()
        {
            MapCamera mapCamera = null;
            foreach (Entity entity in Map.Instance.Children)
            {
                //MM_cam
                MapCamera camera = entity as MapCamera;
                if (camera != null)
                {
                    if (camera.Name == "MapCamera_MainMenu") //insert MapCamera to use
                    {
                        mapCamera = camera;
                        break;
                    }
                }
            }
            return mapCamera;
        }

        protected void UpdateCamera()
        {
            base.OnRender();

            //float outCurveTime;

            //Update camera orientation
            if (Map.Instance != null)
            {
                //moveTime = Time;

                Vec3 position;
                Vec3 forward;
                //Vec3 up;
                Degree fov;

                MapCamera mapCamera = GetMapCamera();
                if (mapCamera == null)
                    return;

                //iNCIN
                if (cameraCurves == null)
                    return;

                //curve /////////////////////////////////////////////////////////////
                MapCurve mapCurve = (MapCurve)Entities.Instance.GetByName("MM_Curve"); //insert curve name here

                if (moveTime >= mapCurve.GetCurveMaxTime())
                    moveTime -= mapCurve.GetCurveMaxTime();

                mapCamera.Position = mapCurve.CalculateCurvePointByTime(moveTime);
                mapCamera.Rotation = mapCurve.CalculateCurveRotationByTime(moveTime);

                //real Camera/////////////////////////////////////////////////////////////
                position = mapCamera.Position;
                forward = mapCamera.Rotation * new Vec3(1, 0, 0);
                fov = mapCamera.Fov;

                if (fov == 0)
                    fov = Map.Instance.Fov;

                Camera camera = RendererWorld.Instance.DefaultCamera;
                camera.NearClipDistance = Map.Instance.NearFarClipDistance.Minimum;
                camera.FarClipDistance = Map.Instance.NearFarClipDistance.Maximum;
                camera.FixedUp = Vec3.ZAxis;
                camera.Fov = fov;
                camera.Position = position;
                camera.Direction = forward;
            }
        }

        private void UpdateCameramain()
        {
            //Update camera orientation
            if (Map.Instance != null)
            {
                moveTime = Time;

                Vec3 position;
                Vec3 forward;
                Degree fov;

                MapCamera mapCamera = GetMapCamera();
                if (mapCamera == null)
                    return;

                //curve
                MapCurve mapCurve = (MapCurve)Entities.Instance.GetByName("MM_Curve"); //Insert curve name here

                if (moveTime >= mapCurve.GetCurveMaxTime())
                    moveTime -= mapCurve.GetCurveMaxTime();

                if (moveTime < 0)
                    moveTime = 0;

                mapCamera.Position = mapCurve.CalculateCurvePointByTime(moveTime);
                mapCamera.Rotation = mapCurve.CalculateCurveRotationByTime(moveTime);

                //real Camera/////////////////////////////////////////////////////////////
                position = mapCamera.Position;
                forward = mapCamera.Rotation * new Vec3(1, 0, 0);
                fov = mapCamera.Fov;

                if (fov == 0)
                    fov = Map.Instance.Fov;

                // damn /////////////////////////////////////////////////////////////////
                Camera camera = RendererWorld.Instance.DefaultCamera;
                camera.NearClipDistance = Map.Instance.NearFarClipDistance.Minimum;
                camera.FarClipDistance = Map.Instance.NearFarClipDistance.Maximum;
                camera.FixedUp = Vec3.ZAxis;
                camera.Fov = fov;
                camera.Position = position;
                camera.Direction = forward;// LookAt( position + forward * 100 );
            }
        }

        /// <summary>
        /// Creates the background world.
        /// </summary>
        private void CreateMap()
        {
            DestroyMap();

            string mapName = "Maps\\MainMenu\\Map.map";

            if (VirtualFile.Exists(mapName))
            {
                WorldType worldType = EntityTypes.Instance.GetByName("SimpleWorld") as WorldType;
                if (worldType == null)
                    Log.Fatal("MainMenuWindow: CreateMap: \"SimpleWorld\" type is not exists.");

                if (GameEngineApp.Instance.ServerOrSingle_MapLoad(mapName, worldType, true))
                {
                    mapInstance = Map.Instance;
                    EntitySystemWorld.Instance.Simulation = true;
                }
            }
        }

        /// <summary>
        /// Destroys the background world.
        /// </summary>
        private void DestroyMap()
        {
            if (mapInstance == Map.Instance)
            {
                MapSystemWorld.MapDestroy();
                EntitySystemWorld.Instance.WorldDestroy();
            }
        }
    }
}