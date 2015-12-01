// Copyright (C) 2006-2008 NeoAxis Group Ltd.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Engine;
using Engine.EntitySystem;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.UISystem;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    /// <summary>
    /// Defines a game window for FPS and TPS games.
    /// </summary>
    public class AKGameWindow : GameWindow
    {
        private class PlayerSpawnPosition
        {
            public SpawnPoint spawnPoint;
            public Control Control;
        }

        //List<FlagIcon> flagIcons = new List<FlagIcon>();
        //List<Flag> flags = new List<Flag>();

        private List<PlayerSpawnPosition> playerSpawnPositions = new List<PlayerSpawnPosition>();
        private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        private static CameraType lastCameraType = CameraType.FPS;

        private enum CameraType
        {
            FPS,
            TPS,
            Free,
            CamForward,
            CamBack,
            CamLeft,
            CamRight,
            Count,
        }

        [Config("Camera", "cameraType")]
        private static CameraType cameraType;

        [Config("Camera", "tpsCameraDistance")]
        private static float tpsCameraDistance = 4;

        [Config("Camera", "tpsCameraCenterOffset")]
        private static float tpsCameraCenterOffset = 1.6f;

        [Config("Camera", "tpsVehicleCameraDistance")]
        private static float tpsVehicleCameraDistance = 8.7f;

        [Config("Camera", "tpsVehicleCameraCenterOffset")]
        private static float tpsVehicleCameraCenterOffset = 3.8f;

        //For management of pressing of the player on switches and management ingame GUI
        private const float playerUseDistance = 3;

        private const float playerUseDistanceTPS = 10;

        //Current ingame GUI which with which the player can cooperate
        private MapObjectAttachedGui currentAttachedGuiObject;

        //Which player can switch the current switch
        private ProjectEntities.Switch currentSwitch;

        //For an opportunity to change an active unit and for work with float switches
        private bool switchUsing;

        //HUD screen
        private Control hudControl;

        //do this somewhere just once!!!
        private Texture textureGood;

        private Texture textureBad;
        private Texture textureWhite;

        private Texture missileLockDone;
        private Texture missileLockClose;
        private Texture missileLockStarted;

        private Texture goodRadarTarget;
        private Texture badRadarTarget;

        private Camera rmCamera;
        private Texture cameraTexture;

        //iNCIN -- Bug

        //void InitCameraViewFromTarget()
        //{
        //    int textureSize = 1024;

        //    cameraTexture = TextureManager.Instance.Create(
        //        TextureManager.Instance.GetUniqueName("RemoteView"), Texture.Type.Type2D,
        //        new Vec2I(textureSize, textureSize), 1, 0, PixelFormat.R8G8B8, Texture.Usage.RenderTarget);

        //    RenderTexture renderTexture = cameraTexture.GetBuffer().GetRenderTarget();

        //    //you can update render texture manually by means renderTexture.Update() method. For this task set AutoUpdate = false;
        //    renderTexture.AutoUpdate = true;

        //    //create camera
        //    string cameraName = SceneManager.Instance.GetUniqueCameraName("RemoteView");

        //    if(rmCamera == null)
        //        rmCamera = SceneManager.Instance.CreateCamera(cameraName);

        //    rmCamera.ProjectionType = ProjectionTypes.Perspective;
        //    rmCamera.PolygonMode = PolygonMode.Wireframe;

        //    renderTexture.AddViewport(rmCamera);
        //}
        //iNCIN --- end

        //Data for an opportunity of the player to control other objects. (for Example: Turret control)
        private Unit currentSeeUnitAllowPlayerControl;

        //For optimization of search of the nearest point on a map curve.
        //only for GetNearestPointToMapCurve()
        private MapCurve observeCameraMapCurvePoints;

        private List<Vec3> observeCameraMapCurvePointsList = new List<Vec3>();

        //The list of ObserveCameraArea's for faster work
        private List<ObserveCameraArea> observeCameraAreas = new List<ObserveCameraArea>();

        protected override void OnAttach()
        {
            base.OnAttach();

            //To load the HUD screen
            hudControl = ControlDeclarationManager.Instance.CreateControl("Gui\\AKActionHUD.gui");

            Controls.Add(hudControl);

            //not working very well. remove it -- iNCIN AK bug?
            InitCameraViewFromTarget();

            //Load target reticules

            textureGood = TextureManager.Instance.Load("Assault Knights\\Huds\\targetGood");
            textureBad = TextureManager.Instance.Load("Assault Knights\\Huds\\targetBad");
            textureWhite = TextureManager.Instance.Load("Assault Knights\\Huds\\targetWhite");

            missileLockClose = TextureManager.Instance.Load("Assault Knights\\Huds\\Hud2");
            missileLockDone = TextureManager.Instance.Load("Assault Knights\\Huds\\Hud3");
            missileLockStarted = TextureManager.Instance.Load("Assault Knights\\Huds\\Hud4");

            goodRadarTarget = TextureManager.Instance.Load("Assault Knights\\Huds\\FriendlyRadarDot");
            badRadarTarget = TextureManager.Instance.Load("Assault Knights\\Huds\\EnemyRadarDot");

            //CutSceneManager specific
            if (CutSceneManager.Instance != null)
            {
                CutSceneManager.Instance.CutSceneEnableChange += delegate(CutSceneManager manager)
                {
                    if (manager.CutSceneEnable)
                    {
                        //Cut scene activated. All keys and buttons need to reset.
                        EngineApp.Instance.KeysAndMouseButtonUpAll();
                        GameControlsManager.Instance.DoKeyUpAll();
                    }
                };
            }

            //fill observeCameraRegions list
            foreach (Entity entity in Map.Instance.Children)
            {
                ObserveCameraArea area = entity as ObserveCameraArea;
                if (area != null)
                    observeCameraAreas.Add(area);
            }

            FreeCameraEnabled = cameraType == CameraType.Free;
            lastCameraType = cameraType;

            //add game specific console command
            EngineConsole.Instance.AddCommand("movePlayerUnitToCamera", ConsoleCommand_MovePlayerUnitToCamera);

            EngineConsole.Instance.AddCommand("ChatCommand", ConsoleCommand_ChatCommand);
            EngineConsole.Instance.AddCommand("ToggleNightVision", ConsoleCommand_ToggleNightVision);
            EngineConsole.Instance.AddCommand("ToggleHeatVision", ConsoleCommand_ToggleHeatVision);
            EngineConsole.Instance.AddCommand("ChangeCamera", ConsoleCommand_ChangeCamera);
            EngineConsole.Instance.AddCommand("LoadBuyWindow", ConsoleCommand_LoadBuyWindow);
            //add Xanders Debug console command
            //EngineConsole.Instance.AddCommand("Xander",
            //    ConsoleCommand_Xander);

            //EngineConsole.Instance.AddCommand("ZoomInOut",
            //    ConsoleCommand_ZoomInOut);

            //accept commands of the player
            GameControlsManager.Instance.GameControlsEvent += GameControlsManager_GameControlsEvent;

            //MotionBlur for a player unit contusion
            {
                Compositor compositor = CompositorManager.Instance.GetByName("MotionBlur");
                if (compositor != null && compositor.IsSupported())
                    RendererWorld.Instance.DefaultViewport.AddCompositor("MotionBlur");
            }

            {//heat vision
                Compositor compositor = CompositorManager.Instance.GetByName("HeatVision");
                if (compositor != null && compositor.IsSupported())
                    RendererWorld.Instance.DefaultViewport.AddCompositor("HeatVision");
            }

            {//night vision
                Compositor compositor = CompositorManager.Instance.GetByName("NightVision");
                if (compositor != null && compositor.IsSupported())
                    RendererWorld.Instance.DefaultViewport.AddCompositor("NightVision");
            }

            //chat stuff

            //for chat support
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.ChatService.ReceiveText += Server_ChatService_ReceiveText;
            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
                client.ChatService.ReceiveText += Client_ChatService_ReceiveText;

            //get the list of flags
            foreach (Entity entity in Map.Instance.Children)
            {
                SpawnPoint sp = entity as SpawnPoint;
                if (sp != null && !spawnPoints.Contains(sp))
                {
                    PlayerSpawnPosition psp = new PlayerSpawnPosition();
                    psp.spawnPoint = sp;
                    playerSpawnPositions.Add(psp);
                }
            }
        }

        //////Chat for AKgamewindow

        //screenNessages
        private class ScreenMessage
        {
            public string text;
            public float timeRemaining;
        }

        private List<ScreenMessage> screenMessages = new List<ScreenMessage>();

        private EditBox chatMessageEditBox;

        private void ChatMessageEditBox_PreKeyDown(KeyEvent e, ref bool handled)
        {
            if (e.Key == EKeys.Return && chatMessageEditBox.Focused)
            {
                SayChatMessage();
                handled = true;
                chatMessageEditBox.Unfocus();
                chatfirstclean = false;
                return;
            }
        }

        private void SayChatMessage()
        {
            string text = chatMessageEditBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.ChatService.SayToAll(text);
            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
                client.ChatService.SayToAll(text);

            chatMessageEditBox.Text = "";
        }

        private void AddScreenMessage(string text)
        {
            ScreenMessage message = new ScreenMessage();
            message.text = text;
            message.timeRemaining = 30;
            screenMessages.Add(message);

            while (screenMessages.Count > 20)
                screenMessages.RemoveAt(0);
        }

        private void Server_ChatService_ReceiveText(ChatServerNetworkService sender,
            UserManagementServerNetworkService.UserInfo fromUser, string text,
            UserManagementServerNetworkService.UserInfo privateToUser)
        {
            string userName = fromUser != null ? fromUser.Name : "(null)";
            AddScreenMessage(string.Format("{0}: {1}", userName, text));
        }

        private void Client_ChatService_ReceiveText(ChatClientNetworkService sender,
            UserManagementClientNetworkService.UserInfo fromUser, string text)
        {
            string userName = fromUser != null ? fromUser.Name : "(null)";
            AddScreenMessage(string.Format("{0}: {1}", userName, text));
        }

        //static void ConsoleCommand_Xander(string arguments)
        //{
        //    EntitySystemWorld.Instance.Simulation = !EntitySystemWorld.Instance.Simulation;
        //    //Engine.Utils.DebugWindow.Instance.Show();
        //}

        protected override void OnDetach()
        {
            //MotionBlur for a player unit contusion
            RendererWorld.Instance.DefaultViewport.SetCompositorEnabled("MotionBlur", false);

            //accept commands of the player
            GameControlsManager.Instance.GameControlsEvent -= GameControlsManager_GameControlsEvent;

            //for chat support
            GameNetworkServer server = GameNetworkServer.Instance;
            if (server != null)
                server.ChatService.ReceiveText -= Server_ChatService_ReceiveText;
            GameNetworkClient client = GameNetworkClient.Instance;
            if (client != null)
                client.ChatService.ReceiveText -= Client_ChatService_ReceiveText;

            base.OnDetach();
        }

        private void ConsoleCommand_ChangeCamera(string args)
        {
            //cameraType = (CameraType)((int)cameraType + 1);
            ////wellu

            //if (cameraType == CameraType.Free)
            //    cameraType = CameraType.Count;

            //if (cameraType == CameraType.Count)
            //    cameraType = (CameraType)0;

            //if (GetPlayerUnit() == null)
            //    cameraType = CameraType.Free;

            //FreeCameraEnabled = cameraType == CameraType.Free;
            //if (e.Key == EKeys.C)

            AKunit unit = PlayerIntellect.Instance.ControlledObject as AKunit;
            if (unit != null)
            {
                cameraType = (CameraType)((int)cameraType + 1);
                lastCameraType = cameraType;
                //wellu

                if (cameraType == CameraType.Count)
                {
                    cameraType = (CameraType)0;
                    lastCameraType = cameraType;
                }
                if (cameraType == CameraType.Free)
                {
                    cameraType = CameraType.Free;
                    lastCameraType = cameraType;
                }
                //if (cameraType == CameraType.Count)
                //    cameraType = (CameraType)0;

                if (GetPlayerUnit() == null)
                {
                    cameraType = CameraType.Free;
                    lastCameraType = cameraType;
                }

                FreeCameraEnabled = cameraType == CameraType.Free;
                lastCameraType = cameraType;

                GameEngineApp.Instance.AddScreenMessage("Camera type: " + cameraType.ToString());

                //return true;
            }
        }

        private void ConsoleCommand_ChatCommand(string args)
        {
            if (hudControl != null)
                hudControl.Controls["Game/ChatMessage"].Focus();
            else
                return;
        }

        private void ConsoleCommand_ToggleNightVision(string args)
        {
            //ToggleNightVision();
            AKunit unit = PlayerIntellect.Instance.ControlledObject as AKunit;
            if (unit != null && unit.Type.UseHeatVision)
            {
                ToggleNightVision();
            }
        }

        private void ConsoleCommand_ToggleHeatVision(string args)
        {
            //ToggleHeatVision();
            AKunit unit = PlayerIntellect.Instance.ControlledObject as AKunit;
            if (unit != null && unit.Type.UseHeatVision)
            {
                ToggleHeatVision();
            }
        }

        private void ConsoleCommand_LoadBuyWindow(string args)
        {
            Unit u = GetPlayerUnit() as Unit;
            Hangar mechHangar = null;
            Hangar groundUnitHangar = null;
            Hangar vtolPad = null;
            Hangar jetHangar = null;

            if (u is PlayerCharacter && u != null)
            {
                Map.Instance.GetObjects(new Sphere(u.Position, GameMap.Instance.HangarUseRadius),
                MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                {
                    Hangar h = mapObject as Hangar;
                    if (h != null)
                    {
                        if (h.Faction != null && (h.Faction == u.InitialFaction || h.Faction == u.Intellect.Faction))
                        {
                            if (mechHangar == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.Mechs))
                                mechHangar = h;

                            if (groundUnitHangar == null &&
                                h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.GroundUnits))
                                groundUnitHangar = h;

                            if (vtolPad == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.AirUnits))
                                vtolPad = h;

                            if (jetHangar == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.Jets))
                                jetHangar = h;
                        }
                    }
                });

                if (mechHangar != null || groundUnitHangar != null || vtolPad != null || jetHangar != null)
                {
                    //we have a hangar nearby. show the window
                    //EngineApp.Instance.KeysAndMouseButtonUpAll();
                    //Controls.Add(new PlayerBuyWindow(mechHangar, groundUnitHangar, vtolPad, jetHangar));
                    GameEngineApp.Instance.ControlManager.Controls.Add(new PlayerBuyWindow(mechHangar, groundUnitHangar, vtolPad, jetHangar));
                    return;// true;
                }
                else
                {
                    GameEngineApp.Instance.AddScreenMessage("No Hangars available to Use this command");
                }
            }
        }

        protected override bool OnKeyDown(KeyEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnKeyDown(e);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
            {
                if (currentAttachedGuiObject.ControlManager.DoKeyDown(e))
                    return true;
            }

            //if (GameType != AssaultKnights && e.Key == EKeys.F7)
            //{
            //    lastCameraType = cameraType;

            //    if (cameraType == CameraType.CamBack || cameraType == CameraType.CamForward ||
            //        cameraType == CameraType.CamLeft || cameraType == CameraType.CamRight)
            //    {
            //       cameraType = (CameraType)((int)cameraType + 1);
            //    }

            //    if (cameraType != CameraType.CamBack || cameraType != CameraType.CamForward ||
            //        cameraType != CameraType.CamLeft || cameraType != CameraType.CamRight)
            //   {
            //        cameraType = lastCameraType;
            //    }
            //}

            //camera type change
            if (e.Key == EKeys.C)
            {
                cameraType = (CameraType)((int)cameraType + 1);
                //wellu
                //if (cameraType == CameraType.CamBack || cameraType == CameraType.CamForward ||
                //    cameraType == CameraType.CamLeft || cameraType == CameraType.CamRight)
                //{
                //cameraType = (CameraType)0;
                //    lastCameraType = cameraType;
                //}

                if (cameraType == CameraType.Count)
                {
                    cameraType = (CameraType)0;
                    lastCameraType = cameraType;
                }

                if (cameraType == CameraType.Free && GetPlayerUnit() as AKunit != null)
                {
                    cameraType = (CameraType)((int)cameraType + 1);
                    lastCameraType = cameraType;
                }
                //if (cameraType == CameraType.Count)
                //    cameraType = (CameraType)0;

                if (GetPlayerUnit() == null)
                {
                    cameraType = CameraType.Free;
                    lastCameraType = cameraType;
                }

                FreeCameraEnabled = cameraType == CameraType.Free;
                lastCameraType = cameraType;

                GameEngineApp.Instance.AddScreenMessage("Camera type: " + cameraType.ToString());

                return true;
            }

            //if (e.Key == EKeys.J)
            //{
            //    hudControl.Controls["Game/ChatMessage"].Focus();
            //}

            if (e.Key == EKeys.N)
            {
                ToggleNightVision();
            }

            if (e.Key == EKeys.H)
            {
                ToggleHeatVision();
            }

            //GameControlsManager
            if (EntitySystemWorld.Instance.Simulation)
            {
                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled())
                {
                    if (GameControlsManager.Instance.DoKeyDown(e))
                        return true;
                }
            }

            if (e.Key == EKeys.B)
            {
                Unit u = GetPlayerUnit();
                Hangar mechHangar = null;
                Hangar groundUnitHangar = null;
                Hangar vtolPad = null;
                Hangar jetHangar = null;

                if (u is PlayerCharacter && u != null)
                {
                    Map.Instance.GetObjects(new Sphere(u.Position, GameMap.Instance.HangarUseRadius),
                    MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                    {
                        Hangar h = mapObject as Hangar;
                        if (h != null)
                        {
                            if (h.Faction != null && (h.Faction == u.InitialFaction || h.Faction == u.Intellect.Faction))
                            {
                                if (mechHangar == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.Mechs))
                                    mechHangar = h;

                                if (groundUnitHangar == null &&
                                    h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.GroundUnits))
                                    groundUnitHangar = h;

                                if (vtolPad == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.AirUnits))
                                    vtolPad = h;

                                if (jetHangar == null && h.VehicleType.Contains(Hangar.Hangar_Vehicle_Type.Jets))
                                    jetHangar = h;
                            }
                        }
                    });

                    if (mechHangar != null || groundUnitHangar != null || vtolPad != null || jetHangar != null)
                    {
                        //we have a hangar nearby. show the window
                        EngineApp.Instance.KeysAndMouseButtonUpAll();
                        //Controls.Add(new PlayerBuyWindow(mechHangar, groundUnitHangar, vtolPad, jetHangar));
                        GameEngineApp.Instance.ControlManager.Controls.Add(new PlayerBuyWindow(mechHangar, groundUnitHangar, vtolPad, jetHangar));
                        return true;
                    }
                    else
                    {
                        GameEngineApp.Instance.AddScreenMessage("No Hangars available to Use this command");
                    }
                }
            }

            return base.OnKeyDown(e);
        }

        private void ToggleHeatVision()
        {
            AKunit unit = PlayerIntellect.Instance.ControlledObject as AKunit;
            if (unit != null && unit.Type.UseHeatVision)
            {
                CompositorInstance c = RendererWorld.Instance.DefaultViewport.GetCompositorInstance("HeatVision");
                c.Enabled = !c.Enabled;
                RendererWorld.Instance.DefaultViewport.SetCompositorEnabled("HeatVision", c.Enabled);
            }
        }

        private void ToggleNightVision()
        {
            AKunit unit = PlayerIntellect.Instance.ControlledObject as AKunit;
            if (unit != null && unit.Type.UseHeatVision)
            {
                CompositorInstance c = RendererWorld.Instance.DefaultViewport.GetCompositorInstance("NightVision");
                c.Enabled = !c.Enabled;
                RendererWorld.Instance.DefaultViewport.SetCompositorEnabled("NightVision", c.Enabled);
            }
        }

        protected override bool OnKeyPress(KeyPressEvent e)
        {
            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
            {
                currentAttachedGuiObject.ControlManager.DoKeyPress(e);
                return true;
            }

            return base.OnKeyPress(e);
        }

        protected override bool OnKeyUp(KeyEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnKeyUp(e);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
                currentAttachedGuiObject.ControlManager.DoKeyUp(e);

            //GameControlsManager
            GameControlsManager.Instance.DoKeyUp(e);

            return base.OnKeyUp(e);
        }

        protected override bool OnMouseDown(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseDown(button);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
            {
                currentAttachedGuiObject.ControlManager.DoMouseDown(button);
                return true;
            }

            //GameControlsManager
            if (EntitySystemWorld.Instance.Simulation)
            {
                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled())
                {
                    if (GameControlsManager.Instance.DoMouseDown(button))
                        return true;
                }
            }

            return base.OnMouseDown(button);
        }

        protected override bool OnMouseUp(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseUp(button);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
                currentAttachedGuiObject.ControlManager.DoMouseUp(button);

            //GameControlsManager
            GameControlsManager.Instance.DoMouseUp(button);

            return base.OnMouseUp(button);
        }

        protected override bool OnMouseDoubleClick(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseDoubleClick(button);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
            {
                currentAttachedGuiObject.ControlManager.DoMouseDoubleClick(button);
                return true;
            }

            return base.OnMouseDoubleClick(button);
        }

        protected override void OnMouseMove()
        {
            base.OnMouseMove();

            //If atop openly any window to not process
            if (Controls.Count != 1)
                return;

            //ignore mouse move events if DebugInformationWindow enabled without background mode

            if (ProfilingToolWindow.Instance != null && !ProfilingToolWindow.Instance.Background)
                return;

            //GameControlsManager
            if (EntitySystemWorld.Instance.Simulation && EngineApp.Instance.MouseRelativeMode)
            {
                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled())
                {
                    Vec2 mouseOffset = MousePosition;
                    //zoom for player
                    if (EngineApp.Instance.IsMouseButtonPressed(EMouseButtons.Middle))
                        if (GetPlayerUnit() as PlayerCharacter != null)// && GetPlayerUnit() is PlayerCharacter)
                            if (GetRealCameraType() == CameraType.FPS || GetRealCameraType() == CameraType.TPS)
                                mouseOffset /= 3;

                    //zoom for AKunit
                            else if (EngineApp.Instance.IsMouseButtonPressed(EMouseButtons.Middle))
                                if (GetPlayerUnit() != null && GetPlayerUnit() is AKunit)
                                    mouseOffset /= 3;

                            // zoom for Turret
                                else if (EngineApp.Instance.IsMouseButtonPressed(EMouseButtons.Middle))
                                    if (GetPlayerUnit() != null && GetPlayerUnit() is Turret)
                                        if (GetRealCameraType() == CameraType.TPS)
                                            mouseOffset /= 3;

                    GameControlsManager.Instance.DoMouseMoveRelative(mouseOffset);
                }
            }
        }

        protected override bool OnMouseWheel(int delta)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseWheel(delta);

            //currentAttachedGuiObject
            if (currentAttachedGuiObject != null)
            {
                currentAttachedGuiObject.ControlManager.DoMouseWheel(delta);
                return true;
            }
            GameControlsManager.Instance.DoMouseMouseWheel(delta);
            return base.OnMouseWheel(delta);
        }

        protected override bool OnJoystickEvent(JoystickInputEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnJoystickEvent(e);

            //GameControlsManager
            if (EntitySystemWorld.Instance.Simulation)
            {
                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled())
                {
                    if (GameControlsManager.Instance.DoJoystickEvent(e))
                        return true;
                }
            }

            return base.OnJoystickEvent(e);
        }

        private bool chatfirstclean;

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);
            TickAKunitCamera(delta);

            //chatshit
            if (chatMessageEditBox != null)
            {
                chatMessageEditBox = hudControl.Controls["Game/ChatMessage"] as EditBox;
                chatMessageEditBox.PreKeyDown += ChatMessageEditBox_PreKeyDown;

                if (EntitySystemWorld.Instance.IsSingle())
                {
                    //hide chat edit box for single mode
                    if (hudControl.Controls["ChatText"] != null)
                        hudControl.Controls["ChatText"].Visible = false;

                    if (hudControl.Controls["Game/ChatMessage"] != null)
                        hudControl.Controls["Game/ChatMessage"].Visible = false;
                }

                if (!chatfirstclean && chatMessageEditBox.Text == "j")
                {
                    chatMessageEditBox.Text = "";
                    chatfirstclean = true;
                }
            }

            //NeedWorldDestroy
            if (GameWorld.Instance.NeedWorldDestroy)
            {
                if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
                    EntitySystemWorld.Instance.Simulation = false;
                MapSystemWorld.MapDestroy();
                EntitySystemWorld.Instance.WorldDestroy();

                GameEngineApp.Instance.Server_DestroyServer("The server has been destroyed");
                GameEngineApp.Instance.Client_DisconnectFromServer();

                //close all windows
                foreach (Control control in GameEngineApp.Instance.ControlManager.Controls)
                    control.SetShouldDetach();
                //create main menu
                GameEngineApp.Instance.ControlManager.Controls.Add(new MainMenuWindow());
                return;
            }

            //If atop openly any window to not process
            if (Controls.Count != 1)
                return;

            if (GameEngineApp.Instance.ControlManager.IsControlFocused())
                return;

            //update mouse relative mode
            {
                //!!!!!!mb not here
                if (GetRealCameraType() == CameraType.Free && !FreeCameraMouseRotating)
                    EngineApp.Instance.MouseRelativeMode = false;

                if (EntitySystemWorld.Instance.Simulation && GetRealCameraType() != CameraType.Free)
                    EngineApp.Instance.MouseRelativeMode = true;

                if (GameEngineApp.Instance.ControlManager != null &&
                GameEngineApp.Instance.ControlManager.IsControlFocused())
                    EngineApp.Instance.MouseRelativeMode = false;
            }

            if (GetRealCameraType() == CameraType.TPS && !IsCutSceneEnabled() &&
                !EngineConsole.Instance.Active)
            {
                Range distanceRange = new Range(2, 200);
                Range centerOffsetRange = new Range(0, 10);

                float cameraDistance;
                float cameraCenterOffset;

                if (IsPlayerUnitVehicle())
                {
                    cameraDistance = tpsVehicleCameraDistance;
                    cameraCenterOffset = tpsVehicleCameraCenterOffset;
                }
                else
                {
                    cameraDistance = tpsCameraDistance;
                    cameraCenterOffset = tpsCameraCenterOffset;
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.PageUp))
                {
                    cameraDistance -= delta * distanceRange.Size() / 20.0f;
                    if (cameraDistance < distanceRange[0])
                        cameraDistance = distanceRange[0];
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.PageDown))
                {
                    cameraDistance += delta * distanceRange.Size() / 20.0f;
                    if (cameraDistance > distanceRange[1])
                        cameraDistance = distanceRange[1];
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.Home))
                {
                    cameraCenterOffset += delta * centerOffsetRange.Size() / 4.0f;
                    if (cameraCenterOffset > centerOffsetRange[1])
                        cameraCenterOffset = centerOffsetRange[1];
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.End))
                {
                    cameraCenterOffset -= delta * centerOffsetRange.Size() / 4.0f;
                    if (cameraCenterOffset < centerOffsetRange[0])
                        cameraCenterOffset = centerOffsetRange[0];
                }

                if (IsPlayerUnitVehicle())
                {
                    tpsVehicleCameraDistance = cameraDistance;
                    tpsVehicleCameraCenterOffset = cameraCenterOffset;
                }
                else
                {
                    tpsCameraDistance = cameraDistance;
                    tpsCameraCenterOffset = cameraCenterOffset;
                }
            }

            //GameControlsManager
            if (EntitySystemWorld.Instance.Simulation)
            {
                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled())
                    GameControlsManager.Instance.DoTick(delta);
            }

            {
                for (int n = 0; n < screenMessages.Count; n++)
                {
                    screenMessages[n].timeRemaining -= delta;
                    if (screenMessages[n].timeRemaining <= 0)
                    {
                        screenMessages.RemoveAt(n);
                        n--;
                    }
                }
            }

            //FindNearFlag();
        }

        /*private void FindNearFlag()
        {
            if (GetPlayerUnit() == null)
                return;

            Bounds volume = new Bounds(GetPlayerUnit().Position);
            volume.Expand(new Vec3(10, 10, 20));

            Body[] result = PhysicsWorld.Instance.VolumeCast(volume,
                (int)ContactGroup.CastOnlyContact);

            foreach (Body body in result)
            {
                MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
                if (obj != null)
                {
                    Flag FindFlag = obj as Flag;
                    if (FindFlag != null)
                    {
                        NearFlag = FindFlag;
                        isNearFlag = true;
                    }
                    else
                        isNearFlag = false;
                }
            }
        }*/

        /// <summary>
        /// Updates objects on which the player can to operate.
        /// Such as which the player can supervise switches, ingameGUI or control units.
        /// </summary>
        private void UpdateCurrentPlayerUseObjects()
        {
            Camera camera = RendererWorld.Instance.DefaultCamera;

            Unit playerUnit = GetPlayerUnit();

            float maxDistance = (GetRealCameraType() == CameraType.FPS) ?
                playerUseDistance : playerUseDistanceTPS;

            Ray ray = camera.GetCameraToViewportRay(new Vec2(.5f, .5f));
            ray.Direction = ray.Direction.GetNormalize() * maxDistance;

            //currentAttachedGuiObject
            {
                MapObjectAttachedGui attachedGuiObject = null;
                Vec2 screenPosition = Vec2.Zero;

                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled() &&
                    EntitySystemWorld.Instance.Simulation)
                {
                    Map.Instance.GetObjectsAttachedGuiObject(ray,
                        out attachedGuiObject, out screenPosition);
                }

                //ignore empty gui objects
                if (attachedGuiObject != null)
                {
                    In3dControlManager manager = attachedGuiObject.ControlManager;

                    if (manager.Controls.Count == 0 ||
                        (manager.Controls.Count == 1 && !manager.Controls[0].Enable))
                    {
                        attachedGuiObject = null;
                    }
                }

                if (attachedGuiObject != currentAttachedGuiObject)
                {
                    if (currentAttachedGuiObject != null)
                        currentAttachedGuiObject.ControlManager.LostManagerFocus();
                    currentAttachedGuiObject = attachedGuiObject;
                }

                if (currentAttachedGuiObject != null)
                    currentAttachedGuiObject.ControlManager.DoMouseMove(screenPosition);
            }

            //currentFloatSwitch
            {
                ProjectEntities.Switch overSwitch = null;

                Map.Instance.GetObjects(ray, delegate(MapObject obj, float scale)
                {
                    ProjectEntities.Switch s = obj as ProjectEntities.Switch;
                    if (s != null)
                    {
                        if (s.UseAttachedMesh != null)
                        {
                            Bounds bounds = ((MapObjectAttachedMesh)s.UseAttachedMesh).SceneNode.
                                GetWorldBounds();

                            if (bounds.RayIntersection(ray))
                            {
                                overSwitch = s;
                                return false;
                            }
                        }
                        else
                        {
                            overSwitch = s;
                            return false;
                        }
                    }

                    return true;
                });

                //draw border
                if (overSwitch != null)
                {
                    camera.DebugGeometry.Color = new ColorValue(1, 1, 1);
                    if (overSwitch.UseAttachedMesh != null)
                    {
                        camera.DebugGeometry.AddBounds(overSwitch.UseAttachedMesh.SceneNode.
                            GetWorldBounds());
                    }
                    else
                        camera.DebugGeometry.AddBounds(overSwitch.MapBounds);
                }

                if (overSwitch != currentSwitch)
                {
                    FloatSwitch floatSwitch = currentSwitch as FloatSwitch;
                    if (floatSwitch != null)
                        floatSwitch.UseEnd();

                    currentSwitch = overSwitch;
                }
            }

            //Use player control unit
            if (playerUnit != null)
            {
                currentSeeUnitAllowPlayerControl = null;

                if (PlayerIntellect.Instance != null &&
                    PlayerIntellect.Instance.MainNotActiveUnit == null &&
                    GetRealCameraType() != CameraType.Free)
                {
                    Ray unitFindRay = ray;

                    //special ray for TPS camera
                    if (GetRealCameraType() == CameraType.TPS)
                    {
                        unitFindRay = new Ray(playerUnit.Position,
                            playerUnit.Rotation * new Vec3(playerUseDistance, 0, 0));
                    }

                    Map.Instance.GetObjects(unitFindRay, delegate(MapObject obj, float scale)
                    {
                        Dynamic dynamic = obj as Dynamic;

                        if (dynamic == null)
                            return true;

                        if (!dynamic.Visible)
                            return true;

                        Unit u = dynamic.GetParentUnit();
                        if (u == null)
                            return true;

                        if (u == GetPlayerUnit())
                            return true;

                        if (!u.Type.AllowPlayerControl)
                            return true;

                        if (u.Intellect != null)
                            return true;

                        if (!u.MapBounds.RayIntersection(unitFindRay))
                            return true;

                        currentSeeUnitAllowPlayerControl = u;

                        return false;
                    });
                }

                //draw border
                if (currentSeeUnitAllowPlayerControl != null)
                {
                    camera.DebugGeometry.Color = new ColorValue(1, 1, 1);
                    camera.DebugGeometry.AddBounds(currentSeeUnitAllowPlayerControl.MapBounds);
                }
            }

            //draw "Press Use" text
            if (currentSwitch != null || currentSeeUnitAllowPlayerControl != null)
            {
                ColorValue color;
                if ((Time % 2) < 1)
                    color = new ColorValue(1, 1, 0);
                else
                    color = new ColorValue(0, 1, 0);

                EngineApp.Instance.ScreenGuiRenderer.AddText("Press \"Use Key Command\n Check Options Menu\"",
                    new Vec2(.5f, .9f), HorizontalAlign.Center, VerticalAlign.Center, color);
            }
        }

        protected override void OnRender()
        {
            base.OnRender();

            UpdateHUD();
            GetCameraViewFromTarget();
            UpdateCurrentPlayerUseObjects();
            UpdatePlayerContusionMotionBlur();
            TargetUnLock();

            //UpdateFlagGUI();
        }

        /*private void UpdateFlagGUI()
        {
            Control FlagBarBack = hudControl.Controls["Game/FPBBack"];
            Control FlagBar = hudControl.Controls["Game/FPBBack/FPB"];

            if (FlagBarBack != null && FlagBar != null)
            {
                //shoing and hiding all
                FlagBarBack.Visible = isNearFlag;
                FlagBar.Visible = isNearFlag;

                if(NearFlag == null)
                return;

                //Flag Progress bar
                {
                    float coef = NearFlag.status;
                    coef /= 1000;
                    Vec2 originalSize = new Vec2(188, 18);
                    Vec2 interval = new Vec2(0, 188);
                    float sizeX = coef * (interval[1] - interval[0]);
                    FlagBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    FlagBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                }
                //change Flag PB color
                if (NearFlag.FactionInt == 1)
                {
                    FlagBar.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\Flagstuff\\ProgressBarAK");
                }
                else if (NearFlag.FactionInt == 2)
                {
                    FlagBar.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\Flagstuff\\ProgressBarOmni");
                }
                else
                {
                    FlagBar.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\Flagstuff\\ProgressBar");
                }
            }
        }*/

        /*
        void UpdatePlayerContusionMotionBlur()
        {
            //disable updating if post effects windows activated
            if (PostEffectsWindow.Instance != null)
                return;

            PlayerCharacter playerCharacter = GetPlayerUnit() as PlayerCharacter;

            //calculate blur factor
            float blur = 0;
            if (playerCharacter != null && GetRealCameraType() == CameraType.FPS &&
                EntitySystemWorld.Instance.Simulation)
            {
                blur = playerCharacter.ContusionTimeRemaining;
                if (blur > .8f)
                    blur = .8f;
            }

            //update compositor
            MotionBlurCompositorInstance instance = (MotionBlurCompositorInstance)
                RendererWorld.Instance.DefaultViewport.GetCompositorInstance("MotionBlur");
            if (instance != null)
            {
                instance.Enabled = blur != 0;
                MotionBlurCompositorInstance.Blur = blur;
            }
        }
        */

        private void UpdatePlayerContusionMotionBlur()
        {
            PlayerCharacter playerCharacter = GetPlayerUnit() as PlayerCharacter;

            //calculate blur factor
            float blur = 0;
            if (playerCharacter != null && GetRealCameraType() == CameraType.FPS &&
                EntitySystemWorld.Instance.Simulation)
            {
                blur = playerCharacter.ContusionTimeRemaining;
                if (blur > .8f)
                    blur = .8f;
            }

            //update MotionBlur item of MapCompositorManager
            //MapCompositorManager will be created if it not exist.

            bool enable = blur > 0;

            Compositor compositor = CompositorManager.Instance.GetByName("MotionBlur");
            if (compositor != null && compositor.IsSupported())
            {
                //create MapCompositorManager
                if (enable && MapCompositorManager.Instance == null)
                {
                    Entity manager = Entities.Instance.Create("MapCompositorManager", Map.Instance);
                    manager.PostCreate();
                }

                //update MotionBlur item
                if (MapCompositorManager.Instance != null)
                {
                    MotionBlurCompositorParameters item = (MotionBlurCompositorParameters)
                        MapCompositorManager.Instance.GetItem("MotionBlur");
                    if (enable && item == null)
                        item = (MotionBlurCompositorParameters)MapCompositorManager.Instance.AddItem("MotionBlur");
                    if (item != null)
                    {
                        item.Enabled = enable;
                        item.Blur = blur;
                    }
                }
            }
        }

        private void UpdateHUDControlIcon(Control control, string iconName)
        {
            if (control == null)
                return;

            if (!string.IsNullOrEmpty(iconName))
            {
                string fileName = string.Format("Gui\\HUD\\Icons\\{0}.png", iconName);

                bool needUpdate = false;

                if (control.BackTexture != null)
                {
                    string current = control.BackTexture.Name;
                    current = current.Replace('/', '\\');

                    if (string.Compare(fileName, current, true) != 0)
                        needUpdate = true;
                }
                else
                    needUpdate = true;

                if (needUpdate)
                {
                    if (VirtualFile.Exists(fileName))
                        control.BackTexture = TextureManager.Instance.Load(fileName, Texture.Type.Type2D, 0);
                    else
                        control.BackTexture = null;
                }
            }
            else
                control.BackTexture = null;
        }

        /// <summary>
        /// Updates HUD screen
        /// </summary>
        private void UpdateHUD()
        {
            if (hudControl == null)
                return;
            ////Hiding the gun for others
            //foreach (Entity entity in Map.Instance.Children)
            //{
            //    PlayerCharacter PC = entity as PlayerCharacter;
            //    if (PC != null)
            //    {
            //        if (PC != GetPlayerUnit() as PlayerCharacter)
            //        {
            //            if (PC.ActiveWeapon != null)
            //            {
            //               PC.ActiveWeapon.Visible = false;
            //            }
            //        }
            //    }
            //}

            Unit playerUnit = GetPlayerUnit();

            MechCharacter Myplayer = playerUnit as MechCharacter;  //iNCIN MechCharacter
            PlayerCharacter playerCharacter = playerUnit as PlayerCharacter;

            if (playerCharacter != null && hudControl.FileNameCreated != "Gui\\AKActionHUD.gui")
                return; // don't try to update player HUD, if we are in a mech
            else if (Myplayer != null && hudControl.FileNameCreated != "Assault Knights\\Huds\\AKunitHud.gui")
                return;
            else
                ;

            hudControl.Visible = EngineDebugSettings.DrawGui;

            //Game

            hudControl.Controls["Game"].Visible = GetRealCameraType() != CameraType.Free &&
                !IsCutSceneEnabled();

            //Player
            string playerTypeName = playerUnit != null ? playerUnit.Type.Name : "";

            UpdateHUDControlIcon(hudControl.Controls["Game/PlayerIcon"], playerTypeName);
            hudControl.Controls["Game/Player"].Text = playerTypeName.ToString();

            //HealthBar
            {
                if (Myplayer != null || playerCharacter != null)
                {
                    float coef = 0;
                    if (playerUnit != null)
                        coef = playerUnit.Health / playerUnit.Type.HealthMax;

                    Control healthBar = hudControl.Controls["Game/HealthBar"];
                    Vec2 originalSize = new Vec2(200, 32);
                    Vec2 interval = new Vec2(117, 304);
                    float sizeX = (200 - 82) + coef * (interval[1] - interval[0]);
                    healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                }
            }

            //ShieldBar
            {
                float coef = 0;
                if (playerCharacter != null)
                {
                    if (playerUnit != null)
                        coef = playerUnit.Shield / playerUnit.Type.ShieldMax;

                    Control shieldBar = hudControl.Controls["Game/ShieldBar"];
                    Vec2 originalSize = new Vec2(200, 32);
                    Vec2 interval = new Vec2(117, 304);
                    float sizeX = (200 - 82) + coef * (interval[1] - interval[0]);
                    shieldBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    shieldBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                }
            }

            //EnergyBar
            {
                //PlayerCharacter Myplayer = playerUnit as PlayerCharacter;
                if (Myplayer != null || playerCharacter != null)
                {
                    float coef = 0.3f;
                    Control energyBar = hudControl.Controls["Game/EnergyBar"];

                    if (playerCharacter != null)
                    {
                        //booster = true;
                        coef = playerCharacter.JetFuel;
                        coef /= playerCharacter.Type.JetFuelMax;
                    }
                    else if (Myplayer != null)
                    {
                        coef = Myplayer.RunEnergy / Myplayer.Type.RunEnergyMax;
                    }

                    if (energyBar != null)
                    {
                        Vec2 originalSize = new Vec2(200, 32);
                        Vec2 interval = new Vec2(117, 304);
                        float sizeX = (117 - 82) + coef * (interval[1] - interval[0]);
                        energyBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                        energyBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                    }
                }
            }

            //Weapon
            {
                string weaponName = "";
                string magazineCountNormal = "";
                string bulletCountNormal = "";
                string bulletCountAlternative = "";

                Weapon weapon = null;
                {
                    //PlayerCharacter specific

                    if (playerCharacter != null)
                        weapon = playerCharacter.ActiveWeapon;

                    //Turret specific
                    Turret turret = playerUnit as Turret;
                    if (turret != null)
                        weapon = turret.MainGun;

                    //Tank specific
                    Tank tank = playerUnit as Tank;
                    if (tank != null)
                        weapon = tank.MainGun;
                }

                if (weapon != null)
                {
                    weaponName = weapon.Type.FullName;

                    Gun gun = weapon as Gun;
                    if (gun != null)
                    {
                        if (gun.Type.NormalMode.BulletType != null)
                        {
                            //magazineCountNormal
                            if (gun.Type.NormalMode.MagazineCapacity != 0)
                            {
                                magazineCountNormal = gun.NormalMode.BulletMagazineCount.ToString() + "/" +
                                    gun.Type.NormalMode.MagazineCapacity.ToString();
                            }
                            //bulletCountNormal
                            if (gun.Type.NormalMode.BulletExpense != 0)
                            {
                                bulletCountNormal = (gun.NormalMode.BulletCount -
                                    gun.NormalMode.BulletMagazineCount).ToString() + "/" +
                                    gun.Type.NormalMode.BulletCapacity.ToString();
                            }
                        }

                        if (gun.Type.AlternativeMode.BulletType != null)
                        {
                            //bulletCountAlternative
                            if (gun.Type.AlternativeMode.BulletExpense != 0)
                                bulletCountAlternative = gun.AlternativeMode.BulletCount.ToString() + "/" +
                                    gun.Type.AlternativeMode.BulletCapacity.ToString();
                        }
                    }
                }
                if (playerCharacter != null)
                {
                    hudControl.Controls["Game/Weapon"].Text = weaponName.ToString();
                    hudControl.Controls["Game/WeaponMagazineCountNormal"].Text = magazineCountNormal.ToString();
                    hudControl.Controls["Game/WeaponBulletCountNormal"].Text = bulletCountNormal.ToString();
                    hudControl.Controls["Game/WeaponBulletCountAlternative"].Text = bulletCountAlternative.ToString();
                    UpdateHUDControlIcon(hudControl.Controls["Game/WeaponIcon"], weaponName);
                }
            }

            //CutScene
            {
                if (playerCharacter != null)
                {
                    hudControl.Controls["CutScene"].Visible = IsCutSceneEnabled();

                    if (CutSceneManager.Instance != null)
                    {
                        //CutSceneFade
                        float fadeCoef = 0;
                        if (CutSceneManager.Instance != null)
                            fadeCoef = CutSceneManager.Instance.GetFadeCoefficient();
                        hudControl.Controls["CutSceneFade"].BackColor = new ColorValue(0, 0, 0, fadeCoef);

                        //Message
                        {
                            string text;
                            ColorValue color;
                            CutSceneManager.Instance.GetMessage(out text, out color);
                            if (text == null)
                                text = "";

                            TextBox textBox = (TextBox)hudControl.Controls["CutScene/Message"];
                            textBox.Text = text;
                            textBox.TextColor = color;
                        }
                    }
                }
            }
        }

        private float RadarRange = 0;
        private float AKunitRadarRange;

        private void drawRadar(GuiRenderer renderer)
        {
            AKunit akunit = GetPlayerUnit() as AKunit;
            Unit playerunit = GetPlayerUnit() as Unit;

            if (akunit == null && playerunit as PlayerCharacter != null) //PLAYERCHARACTER -- incin
            {
                //radar bar

                if (playerunit.Intellect != null)
                {
                    if (playerunit.Intellect.IsControlKeyPressed(GameControlKeys.RadarZoomIn) || playerunit.Intellect.IsControlKeyPressed(GameControlKeys.RadarZoomOut))
                    {
                        float In = playerunit.Intellect.GetControlKeyStrength(GameControlKeys.RadarZoomIn) * 8;
                        float Out = playerunit.Intellect.GetControlKeyStrength(GameControlKeys.RadarZoomOut) * 8;
                        RadarRange += (In - Out);
                        MathFunctions.Clamp(ref RadarRange, (playerunit.Type.AKunitRadarDistanceMax % 15), playerunit.Type.AKunitRadarDistanceMax);
                    }
                }
                AKunitRadarRange = playerunit.Type.AKunitRadarDistanceMax - RadarRange;

                hudControl.Controls["Game/Radar2"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\RadFull4");
                //TextureManager.Instance.Load("Gui\\PCGUI\\PCRadar2");
                //("Assault Knights\\Huds\\RadFull4");

                Vec2 pos = hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftTop;
                pos = (pos + hudControl.Controls["Game/Radar1"].GetScreenRectangle().RightBottom) / 2;

                float range = AKunitRadarRange > 0 ? AKunitRadarRange : 1f;
                float size = hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftBottom.Y -
                    hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftTop.Y;

                float ratio = size / AKunitRadarRange;

                float coef = AKunitRadarRange / playerunit.Type.AKunitRadarDistanceMax;

                //Control healthBar = hudControl.Controls["Game/RadarGizzi"];

                //DisplayTextureOverPosition(new Vec2(0.5f, 0.5f),
                //TextureManager.Instance.Load("Assault Knights\\Huds\\RadarGizmo"), renderer, new ColorValue(1f, 1f, 1f));

                Map.Instance.GetObjects(new Sphere(playerunit.Position, AKunitRadarRange),
                    MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                    {
                        Unit unit = mapObject as Unit;

                        //if there is atleast one then we won't spawn

                        if (unit != null && (unit.Position - playerunit.Position).Length() <= range && unit != playerunit)
                        {
                            float dist = (unit.Position.ToVec2() - playerunit.Position.ToVec2()).Length();

                            Vec3 dir = (unit.GetInterpolatedPosition() - playerunit.Position);

                            //Vec3 unitDir = akunit.MainGun.Rotation.GetForward();
                            //Radian unitAngle = MathFunctions.ATan(unitDir.Y, unitDir.X);

                            //PlayerCharacter specific
                            //Weapon weapon = null;
                            PlayerCharacter playerCharacter = playerunit as PlayerCharacter;

                            //if (playerCharacter != null)
                            //    return;
                            if (playerCharacter.ActiveWeapon == null)
                                return;

                            Radian unitAngle = (playerCharacter.ActiveWeapon.Rotation.ToAngles().Yaw) / -57.29578f;
                            Radian needAngle = MathFunctions.ATan(dir.Y, dir.X);
                            Radian diffAngle = needAngle - unitAngle;

                            double plotX = -Math.Sin(diffAngle) * dist * ratio;
                            double plotY = -Math.Cos(diffAngle) * dist * ratio;

                            plotX = (plotX - (plotX * 1 / 4));

                            //IFF
                            //if the unit is empty
                            if (unit.Intellect == null)
                            {
                                if (unit as Helli != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\Helli2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Tank != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\Tank2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Mech != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\Mech2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\RadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                            }
                            //if unit is enemy
                            else if (playerunit.Intellect.Faction != unit.Intellect.Faction)
                            {
                                if (unit as Helli != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\BadHelli2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Tank != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\BadTank2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Mech != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\BadMech2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\EnemyRadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                            }
                            //if unit is friendly
                            else if (akunit != null && akunit.Intellect.Faction == unit.Intellect.Faction)
                            {
                                if (unit as Helli != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\GoodHelli2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Tank != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\GoodTank2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else if (unit as Mech != null)
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\GoodMech2"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                                else
                                {
                                    DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                    TextureManager.Instance.Load("Assault Knights\\Huds\\FriendlyRadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                }
                            }
                        }
                    });
            }
            else if (akunit != null)
            {
                if (akunit.MechShutDown != true)
                {
                    //radar bar
                    if (akunit.Intellect != null)
                    {
                        if (akunit.Intellect.IsControlKeyPressed(GameControlKeys.RadarZoomIn) || akunit.Intellect.IsControlKeyPressed(GameControlKeys.RadarZoomOut))
                        {
                            float In = akunit.Intellect.GetControlKeyStrength(GameControlKeys.RadarZoomIn) * 8;
                            float Out = akunit.Intellect.GetControlKeyStrength(GameControlKeys.RadarZoomOut) * 8;
                            RadarRange += (In - Out);
                            MathFunctions.Clamp(ref RadarRange, (akunit.Type.AKunitRadarDistanceMax % 15), akunit.Type.AKunitRadarDistanceMax);
                        }
                    }
                    AKunitRadarRange = akunit.Type.AKunitRadarDistanceMax - RadarRange;

                    hudControl.Controls["Game/Radar2"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\RadFull4");

                    Vec2 pos = hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftTop;
                    pos = (pos + hudControl.Controls["Game/Radar1"].GetScreenRectangle().RightBottom) / 2;

                    float range = AKunitRadarRange > 0 ? AKunitRadarRange : 1f;
                    float size = hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftBottom.Y -
                        hudControl.Controls["Game/Radar1"].GetScreenRectangle().LeftTop.Y;

                    float ratio = size / AKunitRadarRange;

                    float coef = AKunitRadarRange / akunit.Type.AKunitRadarDistanceMax;

                    //Control healthBar = hudControl.Controls["Game/RadarGizzi"];

                    //DisplayTextureOverPosition(new Vec2(0.5f, 0.5f),
                    //TextureManager.Instance.Load("Assault Knights\\Huds\\RadarGizmo"), renderer, new ColorValue(1f, 1f, 1f));

                    Map.Instance.GetObjects(new Sphere(akunit.Position, AKunitRadarRange),
                        MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                        {
                            Unit unit = mapObject as Unit;

                            //if there is atleast one then we won't spawn

                            if (unit != null && (unit.Position - akunit.Position).Length() <= range && unit != akunit)
                            {
                                float dist = (unit.Position.ToVec2() - akunit.Position.ToVec2()).Length();

                                Vec3 dir = (unit.GetInterpolatedPosition() - akunit.Position);

                                //Vec3 unitDir = akunit.MainGun.Rotation.GetForward();
                                //Radian unitAngle = MathFunctions.ATan(unitDir.Y, unitDir.X);

                                Radian unitAngle = (akunit.MainGun.Rotation.ToAngles().Yaw) / -57.29578f;
                                Radian needAngle = MathFunctions.ATan(dir.Y, dir.X);
                                Radian diffAngle = needAngle - unitAngle;

                                double plotX = -Math.Sin(diffAngle) * dist * ratio;
                                double plotY = -Math.Cos(diffAngle) * dist * ratio;

                                plotX = (plotX - (plotX * 1 / 4));

                                //IFF
                                //if the unit is empty
                                if (unit.Intellect == null)
                                {
                                    if (unit as Helli != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\Helli2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Tank != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\Tank2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Mech != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\Mech2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\RadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                }
                                //if unit is enemy
                                else if (akunit.Intellect.Faction != unit.Intellect.Faction)
                                {
                                    if (unit as Helli != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\BadHelli2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Tank != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\BadTank2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Mech != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\BadMech2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\EnemyRadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                }
                                //if unit is friendly
                                else if (akunit.Intellect.Faction == unit.Intellect.Faction)
                                {
                                    if (unit as Helli != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\GoodHelli2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Tank != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                            TextureManager.Instance.Load("Assault Knights\\Huds\\GoodTank2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else if (unit as Mech != null)
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\GoodMech2"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                    else
                                    {
                                        DisplayTextureOverPosition(new Vec2(pos.X + (float)plotX, pos.Y + (float)plotY),
                                        TextureManager.Instance.Load("Assault Knights\\Huds\\FriendlyRadarDot"), renderer, new ColorValue(1f, 1f, 1f));
                                    }
                                }
                            }
                        });
                }
                else
                {
                    hudControl.Controls["Game/Radar2"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\RadarOH");
                }
            }
        }

        static public Vec2 TR(Rect r, Vec2 pt)
        {
            Vec2 res = new Vec2(
                r.Left + (r.Right - r.Left) * pt.X,
                r.Top + (r.Bottom - r.Top) * pt.Y
            );
            return res;
        }

        private bool AirUnit = false;

        private void UpdateAKunitHUD(GuiRenderer renderer)
        {
            AKunit akunit = GetPlayerUnit() as AKunit;
            Unit playerunit = GetPlayerUnit() as Unit as PlayerCharacter;

            if (akunit == null && playerunit == null)
                return;
            //else if (akunit != null)
            //    ;
            //else if (playerunit as PlayerCharacter == null)
            //    return;

            float coef = 0;

            //if (hudControl.FileNameCreated != "Assault Knights\\Huds\\AKunitHud.gui" && hudControl.FileNameCreated != akunit.Type.AKunitControlGui)
            //    return;// don't try to update AK HUD, if we are on foot

            /*if (akunit as AKJet != null)
                AirUnit = true;
            else
                AirUnit = false;
            //VSI OH SHIT Damn this is going to be hard to crack Edit: Done, cracked WHOOOOOHOOOOOOOO!!!
            if (AirUnit)
            {
                Control sender = hudControl.Controls["Game/VSI"];
                float Angle = akunit.Rotation.GetInverse().ToAngles().Roll;
                float Pitch = akunit.Rotation.GetInverse().ToAngles().Pitch;
                float PosY = 300 + Pitch / 2;
                sender.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sender.Position.Value.X, PosY));

                Gauge(renderer, sender, Angle);
                Gauge(renderer, sender, Angle + 180);
            }*/
            //status buttons

            {
                Control coolantF = hudControl.Controls["Game/Status/COLANT"];
                if (coolantF != null)
                {
                    if (akunit.colantflush == true)
                        coolantF.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\COLANT_R");
                    else
                        coolantF.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\COLANT_G");
                }

                Control OverHeating = hudControl.Controls["Game/Status/OVERHEATING"];
                if (OverHeating != null)
                {
                    if (akunit.AKunitHeat > akunit.Type.AKunitHeatMax - 100)
                        OverHeating.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\OVERHEATING_R");
                    else
                        OverHeating.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\OVERHEATING_G");
                }

                Control SHUTDOWN = hudControl.Controls["Game/Status/SHUTDOWN"];
                if (SHUTDOWN != null)
                {
                    if (akunit.MechShutDown == true)
                        SHUTDOWN.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\SHUTDOWN_R");
                    else
                        SHUTDOWN.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\STATUS\\SHUTDOWN_G");
                }
            }
            if (AirUnit == false && akunit != null)
            {
                //Gun Angle GUI
                {
                    Control GunAGUI = hudControl.Controls["Game/GunA"];
                    Vec2 originalSize = new Vec2(5, 30);
                    Vec2 interval = new Vec2(0, 30);
                    float sizeY = akunit.GunA * 1.5f * (interval[1] - interval[0]);
                    GunAGUI.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(originalSize.X, -sizeY));
                    // GunAGUI.BackTextureCoord = new Rect(0, 0, 1, sizeY / originalSize.Y);
                }
                //Torso Rotation GUI
                {
                    Control TorsoRGUI = hudControl.Controls["Game/TorsoR"];
                    Vec2 originalSize = new Vec2(30, 5);
                    Vec2 interval = new Vec2(0, 30);
                    float sizeX = akunit.TorsoR * (interval[1] - interval[0]);
                    TorsoRGUI.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    //  TorsoRGUI.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                }
            }
            //readoutControl
            {
                Control ReadOutControl = hudControl.Controls["Game/ReadOut"];
                if (ReadOutControl != null && akunit.GetParentUnit() as Mech != null)
                {
                    if (akunit.Health > (akunit.Type.HealthMax * 0.6f))
                    {
                        ReadOutControl.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\MechReadouts\\"
                            + akunit.Type.Name + "Readout");
                    }
                    else if (akunit.Health > (akunit.Type.HealthMax * 0.3f))
                    {
                        ReadOutControl.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\MechReadouts\\"
                            + akunit.Type.Name + "Readout3");
                    }
                    else if (akunit.Health < (akunit.Type.HealthMax * 0.3f))
                    {
                        ReadOutControl.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\MechReadouts\\"
                            + akunit.Type.Name + "Readout2");
                    }
                }
            }
            //Heat Bar
            {
                //if (playerunit as PlayerCharacter != null)
                //{
                //    ;
                //}
                //else
                if (akunit != null)
                {
                    float coef2 = akunit.AKunitHeat;

                    if (akunit.AKunitHeat >= akunit.Type.AKunitHeatMax)
                        akunit.AKunitHeat = akunit.Type.AKunitHeatMax;

                    coef2 /= akunit.Type.AKunitHeatMax;

                    //if (coef2 > 1) coef2 = 1;
                    //Log.Info(akunit.AKunitHeat.ToString());

                    Control HBar = hudControl.Controls["Game/Heat/HeatBar"];

                    Vec2 BPoriginalSize = new Vec2(35, 114);
                    Vec2 BPinterval = new Vec2(0, BPoriginalSize.Y);
                    float BPsizeY = coef2 * (BPinterval[1] - BPinterval[0]);

                    Vec2 interval = new Vec2(BPoriginalSize.Y, 0);
                    float PosY = 119 + coef2 * (interval[1] - interval[0]); //Incin - used to be 119? 114 or 113?0

                    HBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(BPoriginalSize.X, BPsizeY));
                    HBar.BackTextureCoord = new Rect(0, 0, 1, BPsizeY / BPoriginalSize.Y);
                    HBar.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(HBar.Position.Value.X, PosY));
                }
            }

            //Compass Bar
            {
                float coef2 = 0;
                Control CBar = null;
                if (playerunit != null)
                {
                    //PlayerCharacter playerCharacter = playerunit as PlayerCharacter;
                    coef2 = playerunit.Rotation.ToAngles().Yaw / 360f;//.ActiveWeapon
                    CBar = hudControl.Controls["Game/Compass/COMP"];
                }
                else if (akunit.MainGun != null)
                {
                    coef2 = akunit.MainGun.Rotation.ToAngles().Yaw / 360f;
                    CBar = hudControl.Controls["Compass/COMP"];
                }
                else
                    coef2 = akunit.Rotation.ToAngles().Yaw / 360f;

                Vec2 Cords = new Vec2(0.345f, 0.645f);

                if (coef2 > 0)
                {
                    float cocoords = coef2 + 0.495f;
                    Cords = new Vec2(cocoords - 0.15f, cocoords + 0.15f);
                }
                else if (coef2 < 0)
                {
                    coef2 = -coef2;
                    float cocords = 0.495f - coef2;
                    Cords = new Vec2(cocords - 0.15f, cocords + 0.15f);
                }

                CBar.BackTextureCoord = new Rect(Cords.X, 0, Cords.Y, 1);

                // compass Flah Icon

                foreach (PlayerSpawnPosition psp in playerSpawnPositions)
                {
                    if (psp.Control == null)
                    {
                        Control Ec = ControlDeclarationManager.Instance.CreateControl("Gui\\Controls\\AKDefaultFlagIcon.gui") as Control;
                        Ec.HorizontalAlign = Engine.Renderer.HorizontalAlign.Center;
                        psp.Control = Ec;
                    }
                    else
                    {
                        if (!hudControl.Controls.Contains(psp.Control))
                        {
                            hudControl.Controls.Add(psp.Control);
                        }
                    }

                    SpawnPoint spawnPoint = psp.spawnPoint;
                    Control ficon = psp.Control;

                    float dist = 0f;
                    Vec3 dir = Vec3.Zero;
                    Radian unitAngle = 0;

                    if (playerunit != null) //PLAYER
                    {
                        //PlayerCharacter playerCharacter = playerunit as PlayerCharacter;
                        dist = (spawnPoint.Position.ToVec2() - playerunit.Position.ToVec2()).Length();
                        dir = (spawnPoint.GetInterpolatedPosition() - playerunit.Position);
                        unitAngle = (playerunit.Rotation.ToAngles().Yaw) / -57.29578f;
                    }
                    else //AKUNIT
                    {
                        dist = (spawnPoint.Position.ToVec2() - akunit.Position.ToVec2()).Length();
                        dir = (spawnPoint.GetInterpolatedPosition() - akunit.Position);
                        unitAngle = (akunit.MainGun.Rotation.ToAngles().Yaw) / -57.29578f;
                    }

                    Radian needAngle = MathFunctions.ATan(dir.Y, dir.X);
                    Radian diffAngle = needAngle - unitAngle;

                    float DiffDeg = diffAngle * 57.29578f;
                    //EngineApp.Instance.ScreenGuiRenderer.AddText("FlagDir: " + DiffDeg.ToString(), new Vec2(.1f, .3f));
                    if (DiffDeg < 50 && DiffDeg > -50)
                    {
                        ficon.Visible = true;
                        ColorValue FlagColor;
                        //colors and Factions
                        if (spawnPoint.Faction == null)
                        {
                            //non captured Flag
                            FlagColor = new ColorValue(1, 1, 1, 0.8f);
                        }
                        else if (spawnPoint.Faction.Name == "AssaultKnights")
                        {
                            //AK flag
                            FlagColor = new ColorValue(0, 0, 1, 0.8f);
                        }
                        else if (spawnPoint.Faction.Name == "Omni")
                        {
                            //Omni Flag
                            FlagColor = new ColorValue(1, 0, 0, 0.8f);
                        }
                        else
                        {
                            // Unusual flag
                            FlagColor = new ColorValue(0, 1, 0, 0.8f);
                        }

                        //Position handeling
                        float PosX = (DiffDeg / 50) * -0.140f;

                        ficon.Position = new ScaleValue(ScaleType.Parent, new Vec2(PosX, 0.05f));
                        ficon.ColorMultiplier = FlagColor;

                        float DistanceNorm = dist - (dist % 1);
                        EngineApp.Instance.ScreenGuiRenderer.AddText(DistanceNorm.ToString(), new Vec2(PosX + 0.5f, 0.1f), HorizontalAlign.Center, VerticalAlign.Top, FlagColor);
                    }
                    else
                    {
                        ficon.Visible = false;
                    }
                }
            }

            //Speed Bar
            {
                Mech PlayerMech = akunit as Mech;
                if (playerunit != null)
                {
                    ;//do nothing
                }
                else if (PlayerMech != null)
                {
                    hudControl.Controls["Game/SpeedGizi"].Visible = true;
                    hudControl.Controls["Game/speedbar"].Visible = true;
                    float coef2 = PlayerMech.ThrottleF;
                    coef2 /= 100f;

                    Vec2 interval = new Vec2(75, 0);
                    float PosY = 660 + coef2 * (interval[1] - interval[0]);
                    Control healthBar = hudControl.Controls["Game/SpeedGizi"];
                    healthBar.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(250, PosY));
                }
                else
                {
                    hudControl.Controls["Game/SpeedGizi"].Visible = false;
                    hudControl.Controls["Game/speedbar"].Visible = false;
                }
            }

            //Damage Bars
            {
                //BodyParts Db
                {
                    if (playerunit as PlayerCharacter != null || akunit as DamagerBall != null)
                    {
                        ;//do nothing
                    }
                    else
                        if (akunit != null)
                        {
                            for (int i = 0; i < akunit.Bp.Count; i++)
                            {
                                AKunit.BP BP = akunit.Bp[i];
                                if (hudControl.Controls["Game/OwnBodyParts/BPB" + (i + 1)] == null) break;

                                hudControl.Controls["Game/OwnBodyParts/BPT" + (i + 1)].Text = BP.GUIDesplayName.ToString();
                                //Damage bars
                                {
                                    coef = BP.HitPoints;
                                    coef /= BP.HitpointsMax;

                                    Control PBBar = hudControl.Controls["Game/OwnBodyParts/BPB" + (i + 1)];
                                    Vec2 BPoriginalSize = new Vec2(15, 90);
                                    Vec2 BPinterval = new Vec2(0, BPoriginalSize.Y);
                                    float BPsizeY = coef * (BPinterval[1] - BPinterval[0]);

                                    Vec2 interval = new Vec2(BPoriginalSize.Y, 0);
                                    float PosY = 105 + coef * (interval[1] - interval[0]);

                                    PBBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(BPoriginalSize.X, BPsizeY));
                                    PBBar.BackTextureCoord = new Rect(0, 0, 1, BPsizeY / BPoriginalSize.Y);
                                    PBBar.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(PBBar.Position.Value.X, PosY));

                                    //float red;
                                    //float green;
                                    //red = 255 - (coef * 255);
                                    //green = (coef * 255);
                                    //PBBar.ColorMultiplier = new ColorValue(red, green, 0, 255);
                                }
                                for (int J = 6; J > akunit.Type.BodyParts.Count - 1; J--)
                                {
                                    hudControl.Controls["Game/OwnBodyParts/BPB" + (J + 1)].Visible = false;
                                    hudControl.Controls["Game/OwnBodyParts/BPT" + (J + 1)].Visible = false;
                                    hudControl.Controls["Game/OwnBodyParts/BPBG" + (J + 1)].Visible = false;
                                }
                            }
                        }
                }

                //single Db
                {
                    if (playerunit as PlayerCharacter != null)
                    {
                        coef = 1f;
                        if (playerunit != null && playerunit.Type.HealthMax > 0f)
                        {
                            coef = playerunit.Health / playerunit.Type.HealthMax;

                            Control healthBar = hudControl.Controls["Game/HealthBar"];
                            Vec2 originalSize = new Vec2(200, 32);
                            Vec2 interval = new Vec2(117, 304);
                            float sizeX = (117 - 82) + coef * (interval[1] - interval[0]);
                            healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                            healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                            //healthBar.ColorMultiplier = new ColorValue(255 - (coef * 255), coef * 255, 0, 1);
                        }
                        //ShieldBar
                        coef = 1;
                        if (playerunit != null && playerunit.Type.ShieldMax > 0f)
                        {
                            coef = playerunit.Shield / playerunit.Type.ShieldMax;
                            Control shieldBar = hudControl.Controls["Game/ShieldBar"];
                            Vec2 originalSize = new Vec2(200, 32);
                            Vec2 interval = new Vec2(117, 304);
                            float sizeX = (117 - 82) + coef * (interval[1] - interval[0]);
                            shieldBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                            shieldBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                        }
                    }
                    else if (akunit != null && akunit.Type.HealthMax == 0)
                    {
                        coef = 1;
                        Control healthBar = hudControl.Controls["Game/HPBar"];
                        Vec2 originalSize = new Vec2(224, 20);
                        Vec2 interval = new Vec2(0, 224);
                        float sizeX = coef * (interval[1] - interval[0]);
                        healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                        healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                        healthBar.ColorMultiplier = new ColorValue(255 - (coef * 255), coef * 255, 0, 1);
                        //damage percent
                        Control healthPer = hudControl.Controls["Game/HPper"];
                        float DmPer = coef * 100;
                        float DMGPPER = DmPer - (DmPer % 1);
                        healthPer.Text = DMGPPER.ToString();
                    }
                }
            }

            //SHIFT BAR
            {
                Helli helli = akunit as Helli;
                Mech mecha = akunit as Mech;
                bool booster = false;
                if (helli != null)
                {
                    booster = true;
                    coef = helli.ShiftBottel;
                    coef /= helli.Type.MaxShiftBottel;
                }
                if (mecha != null)
                {
                    booster = true;
                    coef = mecha.JetFuel;
                    coef /= mecha.Type.JetFuelMax;
                }

                if (booster)
                {
                    Control healthBar = hudControl.Controls["Game/SHIFTBar"];
                    Vec2 originalSize = new Vec2(224, 20);
                    Vec2 interval = new Vec2(0, 224);
                    float sizeX = coef * (interval[1] - interval[0]);
                    healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);

                    //damage percent
                    Control healthPer = hudControl.Controls["Game/SHIFTPer"];
                    float DmPer = coef * 100;
                    float DMGPPER = DmPer - (DmPer % 1);
                    healthPer.Text = DMGPPER.ToString();
                    hudControl.Controls["Game/SHIFTPer"].Visible = true;
                    hudControl.Controls["Game/SHIFTBar"].Visible = true;
                }
                else
                {
                    //if (playerunit as PlayerCharacter != null)
                    //{
                    //}
                    //else
                    if (akunit != null)
                    {
                        hudControl.Controls["Game/SHIFTPer"].Visible = false;
                        hudControl.Controls["Game/SHIFTBar"].Visible = false;
                    }
                }
            }

            //target bar
            {
                if (playerunit as PlayerCharacter != null)
                {
                    if (playerunit.CurrentReticuleTarget != null)
                    {
                        Unit target = playerunit.CurrentReticuleTarget;
                        coef = target.Health / target.Type.HealthMax;
                        Control healthBar = hudControl.Controls["Game/TargetBar"];
                        Vec2 originalSize = new Vec2(210, 20);
                        Vec2 interval = new Vec2(0, 210);
                        float sizeX = coef * (interval[1] - interval[0]);
                        healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                        healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);

                        //damage percent
                        Control healthPer = hudControl.Controls["Game/TargetPer"];
                        float DmPer = coef * 100;
                        float DMGPPER = DmPer - (DmPer % 1);
                        healthPer.Text = DMGPPER.ToString();
                        hudControl.Controls["Game/TargetPer"].Visible = true;
                        hudControl.Controls["Game/TargetBar"].Visible = true;
                    }
                    else
                    {
                        hudControl.Controls["Game/TargetPer"].Visible = false;
                        hudControl.Controls["Game/TargetBar"].Visible = false;
                    }
                }
                else if (akunit.CurrentReticuleTarget != null)
                {
                    Unit target = akunit.CurrentReticuleTarget;
                    coef = target.Health / target.Type.HealthMax;
                    Control healthBar = hudControl.Controls["Game/TargetBar"];
                    Vec2 originalSize = new Vec2(210, 20);
                    Vec2 interval = new Vec2(0, 210);
                    float sizeX = coef * (interval[1] - interval[0]);
                    healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                    healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);

                    //damage percent
                    Control healthPer = hudControl.Controls["Game/TargetPer"];
                    float DmPer = coef * 100;
                    float DMGPPER = DmPer - (DmPer % 1);
                    healthPer.Text = DMGPPER.ToString();
                    hudControl.Controls["Game/TargetPer"].Visible = true;
                    hudControl.Controls["Game/TargetBar"].Visible = true;
                }
                else
                {
                    hudControl.Controls["Game/TargetPer"].Visible = false;
                    hudControl.Controls["Game/TargetBar"].Visible = false;
                }
            }

            hudControl.Visible = EngineDebugSettings.DrawGui;

            hudControl.Controls["Game"].Visible = GetRealCameraType() != CameraType.Free &&
                !IsCutSceneEnabled();

            if (playerunit as PlayerCharacter != null)
            {
                ;
            }
            else if (akunit.CurrentFireMode.ToString() == "Link")
            {
                hudControl.Controls["Game/WM"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\WeaponsLink");
            }
            else if (akunit.CurrentFireMode.ToString() == "Group")
            {
                hudControl.Controls["Game/WM"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\WeaponsGroup");
            }
            else if (akunit.CurrentFireMode.ToString() == "Alpha")
            {
                hudControl.Controls["Game/WM"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\WeaponsAlpha");
            }

            if (playerunit != null && (playerunit.CurrentReticuleTarget == null || playerunit.CurrentReticuleTarget.Died))
            {
                hudControl.Controls["Game/Enemy"].Text = "No Target";
            }
            else if (playerunit != null && (playerunit.CurrentReticuleTarget != null || !playerunit.CurrentReticuleTarget.Died))
            {
                hudControl.Controls["Game/Enemy"].Text = playerunit.CurrentReticuleTarget.Type.Name.ToString();
            }
            else if (akunit.CurrentReticuleTarget == null || akunit.CurrentReticuleTarget.Died)
            {
                hudControl.Controls["Game/Enemy"].Text = "No Target";
            }
            else if (akunit.CurrentReticuleTarget != null || !akunit.CurrentReticuleTarget.Died)
            {
                hudControl.Controls["Game/Enemy"].Text = akunit.CurrentReticuleTarget.Type.Name.ToString();
            }

            //Weapons GUI handling
            if (playerunit as PlayerCharacter != null)
            {
                ;
            }
            else if (akunit != null)
            {
                for (int i = 0; i < akunit.Weapons.Count; i++)
                {
                    Gun gun = (Gun)akunit.Weapons[i].Weapon;

                    if (hudControl.Controls["Weapons/Weapon" + (i + 1)] == null || hudControl.Controls["Weapons/Weapon" + (i + 1) + "/Weapon" + (i + 1) + "Text"] == null) break;

                    hudControl.Controls["Weapons/Weapon" + (i + 1) + "/Weapon" + (i + 1) + "Text"].Text = gun.Type.Name;

                    if (gun.Damaged != true)
                    {
                        if (akunit.CurrentFireMode == AKunit.FireModes.Alpha ||
                            (akunit.CurrentFireMode == AKunit.FireModes.Group && akunit.Weapons[i].FireGroup == akunit.CurrentFireGroup)
                            || (akunit.MainGun == akunit.Weapons[i].Weapon && akunit.CurrentFireMode == AKunit.FireModes.Link))
                        {
                            hudControl.Controls["Weapons/Weapon" + (i + 1)].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\WActive");
                        }
                        else
                            hudControl.Controls["Weapons/Weapon" + (i + 1)].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\Wslot");
                    }
                    else
                    {
                        hudControl.Controls["Weapons/Weapon" + (i + 1)].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\DM");
                        hudControl.Controls["Weapons/Weapon" + (i + 1) + "/Weapon" + (i + 1) + "Text"].Text = "";
                        hudControl.Controls["Weapons/Weapon" + (i + 1)].BackColor = new ColorValue(255, 255, 255, 1);
                    }

                    //Reload Bar
                    {
                        //float reloadtime;
                        //float Coef2;

                        //float TypeReload = gun.Type.NormalMode.BetweenFireTime;
                        //if (gun.NormalMode.BulletCount == 0)
                        //{
                        //    TypeReload += gun.Type.ReloadTime;
                        //}
                        //reloadtime = TypeReload - gun.ReadyTime;
                        //Coef2 = reloadtime;
                        // Coef2 /= TypeReload;

                        //if (Coef2 > 1)
                        //    Coef2 = 1;

                        Control ReloadBar = hudControl.Controls["Weapons/Weapon" + (i + 1) + "Ammo"];
                        Vec2 originalSize = new Vec2(94, 18);
                        //Vec2 interval = new Vec2(0, 94);
                        //float sizeX = Coef2 * (interval[1] - interval[0]);

                        float sizeX = originalSize.X;
                        if (gun.ReadyTime < gun.ResetTime)
                            sizeX = originalSize.X - ((gun.ReadyTime / gun.ResetTime) * originalSize.X);
                        ReloadBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
                        ReloadBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
                    }

                    if (gun.ReadyTime >= gun.ResetTime)
                    {
                        hudControl.Controls["Weapons/Weapon" + (i + 1) + "Ammo"].BackTexture =
                            TextureManager.Instance.Load("Assault Knights\\Huds\\Green");
                    }
                    else
                    {
                        hudControl.Controls["Weapons/Weapon" + (i + 1) + "Ammo"].BackTexture =
                           TextureManager.Instance.Load("Assault Knights\\Huds\\Red");
                    }

                    if (gun.NormalMode.BulletCount == 0)
                    {
                        hudControl.Controls["Weapons/Weapon" + (i + 1) + "/" + "Ammo"].Text = string.Empty;
                    }
                    else
                    {
                        if (gun.NeedReloadMessageReceived)
                            hudControl.Controls["Weapons/Weapon" + (i + 1) + "/" + "Ammo"].Text = "RELOADING";
                        else
                        {
                            if (gun.ReadyTime >= gun.ResetTime && gun.NormalMode.BulletMagazineCount == 0)
                                hudControl.Controls["Weapons/Weapon" + (i + 1) + "/" + "Ammo"].Text = "RELOAD";
                            else
                                hudControl.Controls["Weapons/Weapon" + (i + 1) + "/" + "Ammo"].Text =
                                    gun.NormalMode.BulletMagazineCount + "/" + gun.NormalMode.BulletCount;
                        }
                    }
                }
                for (int i = 9; i > akunit.weapons.Count - 1; i--)
                {
                    hudControl.Controls["Weapons/Weapon" + (i + 1) + "Ammo"].Visible = false;
                    hudControl.Controls["Weapons/Weapon" + (i + 1) + "/" + "Ammo"].Visible = false;
                    hudControl.Controls["Weapons/Weapon" + (i + 1)].Visible = false;
                }
            }
        }

        private void Gauge(GuiRenderer renderer, Control sender, float Angle)
        {
            /*Rect controlRect = sender.GetScreenRectangle();
            Vec2 size = controlRect.GetSize();

            float angle = MathFunctions.DegToRad(180 + Angle);
            float smallRadius = 0.03f;
            float bigRadius = 0.8f;
            float width = 0.05f;

            Vec2 U = new Vec2(+(float)Math.Cos(angle), +(float)Math.Sin(angle));
            Vec2 V = new Vec2(-(float)Math.Sin(angle), +(float)Math.Cos(angle));
            Vec2 P1 = bigRadius * U + V * width / 2;
            Vec2 P2 = bigRadius * U - V * width / 2;
            Vec2 P3 = smallRadius * U + V * width / 2;
            Vec2 P4 = smallRadius * U - V * width / 2;

            float X1 = Vec2.Dot(P1, Vec2.XAxis);
            float Y1 = Vec2.Dot(P1, Vec2.YAxis);
            float X2 = Vec2.Dot(P2, Vec2.XAxis);
            float Y2 = Vec2.Dot(P2, Vec2.YAxis);
            float X3 = Vec2.Dot(P3, Vec2.XAxis);
            float Y3 = Vec2.Dot(P3, Vec2.YAxis);
            float X4 = Vec2.Dot(P4, Vec2.XAxis);
            float Y4 = Vec2.Dot(P4, Vec2.YAxis);

            X1 = controlRect.Left + (X1 + 1) / 2 * size.X;
            Y1 = controlRect.Top + (Y1 + 1) / 2 * size.Y;
            X2 = controlRect.Left + (X2 + 1) / 2 * size.X;
            Y2 = controlRect.Top + (Y2 + 1) / 2 * size.Y;
            X3 = controlRect.Left + (X3 + 1) / 2 * size.X;
            Y3 = controlRect.Top + (Y3 + 1) / 2 * size.Y;
            X4 = controlRect.Left + (X4 + 1) / 2 * size.X;
            Y4 = controlRect.Top + (Y4 + 1) / 2 * size.Y;

            List<GuiRenderer.TriangleVertex> vert = new List<GuiRenderer.TriangleVertex>(6);
            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X1, Y1), new ColorValue(1, 1, 1, 1), new Vec2(0, 0)));
            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X2, Y2), new ColorValue(1, 1, 1, 1), new Vec2(1, 0)));
            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X4, Y4), new ColorValue(1, 1, 1, 1), new Vec2(0, 1)));

            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X3, Y3), new ColorValue(1, 1, 1, 1), new Vec2(1, 1)));
            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X4, Y4), new ColorValue(1, 1, 1, 1), new Vec2(0, 1)));
            vert.Add(new GuiRenderer.TriangleVertex(new Vec2(X1, Y1), new ColorValue(1, 1, 1, 1), new Vec2(1, 0)));
            renderer.AddTriangles(vert, sender.BackTexture, false);*/
        }

        /// <summary>
        /// Draw a target at center of screen
        /// </summary>
        /// <param name="renderer"></param>
        private void DrawTarget(GuiRenderer renderer)
        {
            AKunit akunit = GetPlayerUnit() as AKunit;
            Unit playerunit = GetPlayerUnit() as Unit;
            //Draw icons over every friendly and enemy unit in viewport but not in RE or ME
            if (EntitySystemWorld.Instance.WorldSimulationType != WorldSimulationTypes.Editor && (akunit != null || playerunit as PlayerCharacter != null))
            {
                //PlayerCharacter player = (PlayerCharacter) playerunit;

                if (akunit != null)
                    akunit.VisibleUnits = new List<Unit>();
                else if (playerunit != null)
                    playerunit.VisibleUnits = new List<Unit>();

                bool stillOnScreen = false;

                Map.Instance.GetObjectsByScreenRectangle(new Rect(0.0f, 0.0f, 1.0f, 1.0f), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject candidateMapObject)
                {
                    Unit unit = candidateMapObject as Unit;
                    if (unit != null && akunit != null && akunit as Unit != unit && unit as PlayerCharacter == null)
                    {
                        float dist = (unit.Position - akunit.Position).Length();
                        if (dist < AKunitRadarRange)
                        {
                            akunit.VisibleUnits.Add(unit);
                            if (akunit.CurrentReticuleTarget == null) akunit.CurrentReticuleTarget = unit;

                            Texture texture = unit.Intellect != null && akunit.Intellect.Faction == unit.Intellect.Faction ? textureGood : textureWhite;

                            if (akunit.CurrentReticuleTarget == unit)
                            {
                                stillOnScreen = true;
                                DisplayTextureOverPosition(akunit.CurrentReticuleTarget.Position, textureBad, renderer, new ColorValue(1f, 1f, 1f));
                            }
                            else
                            {
                                DisplayTextureOverPosition(unit.Position, texture, renderer, new ColorValue(1f, 1f, 1f));
                            }
                            if (akunit.CurrentMissileTarget != null && unit == akunit.CurrentMissileTarget)
                            {
                                DisplayTextureOverPosition(unit.Position, missileLockDone, renderer, new ColorValue(1f, 1f, 1f));
                            }
                        }
                    }
                    else if (unit != null && playerunit != null && playerunit as Unit != unit && unit as PlayerCharacter == null)
                    {
                        float dist = (unit.Position - playerunit.Position).Length();
                        if (dist < AKunitRadarRange)
                        {
                            playerunit.VisibleUnits.Add(unit);
                            if (playerunit.CurrentReticuleTarget == null) playerunit.CurrentReticuleTarget = unit;

                            Texture texture = unit.Intellect != null && playerunit.Intellect.Faction == unit.Intellect.Faction ? textureGood : textureWhite;

                            if (playerunit.CurrentReticuleTarget == unit)
                            {
                                stillOnScreen = true;
                                DisplayTextureOverPosition(playerunit.CurrentReticuleTarget.Position, textureBad, renderer, new ColorValue(1f, 1f, 1f));
                            }
                            else
                            {
                                DisplayTextureOverPosition(unit.Position, texture, renderer, new ColorValue(1f, 1f, 1f));
                            }
                            if (playerunit.CurrentMissileTarget != null && unit == playerunit.CurrentMissileTarget)
                            {
                                DisplayTextureOverPosition(unit.Position, missileLockDone, renderer, new ColorValue(1f, 1f, 1f));
                            }
                        }
                    }
                });

                if (!stillOnScreen && akunit != null) akunit.CurrentReticuleTarget = null;
                else if (!stillOnScreen && playerunit != null) playerunit.CurrentReticuleTarget = null;
                // do this here, coz you can't have multiple GetObjectsByScreenRectangle inside each other
                DrawTargetLock(renderer);
            }

            Ray lookRay = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                new Vec2(.5f, .5f));

            //Vec3 lookTo;
            Body body = null;

            Vec3 lookFrom = lookRay.Origin;
            Vec3 lookDir = Vec3.Normalize(lookRay.Direction);
            float distance = 1000.0f;// RendererWorld.Instance.DefaultCamera.FarClipDistance;

            Unit playerUnit = GetPlayerUnit();

            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                new Ray(lookFrom, lookDir * distance), (int)ContactGroup.CastOnlyContact);

            foreach (RayCastResult result in piercingResult)
            {
                bool ignore = false;

                MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);

                Dynamic dynamic = obj as Dynamic;
                Unit Dunit = obj as Unit;
                if (dynamic != null && playerUnit != null && dynamic.GetParentUnit() == GetPlayerUnit())
                    ignore = true;

                if (dynamic != null && dynamic.GetParentUnit() != GetPlayerUnit())
                {
                    targetgizmo(true);
                    if (Dunit != null && akunit != null)
                    {
                        if ((Dunit.Intellect != null && Dunit.Intellect.Faction != akunit.Intellect.Faction)
                           || (EngineApp.Instance.IsKeyPressed(EKeys.Q))

                           )
                            akunit.CurrentReticuleTarget = Dunit;
                    }
                    else if (Dunit != null && playerunit != null)
                    {
                        if ((Dunit.Intellect != null && Dunit.Intellect.Faction != playerunit.Intellect.Faction)
                           || (EngineApp.Instance.IsKeyPressed(EKeys.Q))

                           )
                            playerunit.CurrentReticuleTarget = Dunit;
                    }
                }
                else
                {
                    targetgizmo(false);
                }

                if (!ignore)
                {
                    body = result.Shape.Body;
                    break;
                }
            }

            if (akunit == null)
            {
                renderer.AddText("¤", new Vec2(.5f, .5f), HorizontalAlign.Center, VerticalAlign.Center);
                renderer.AddText("(  )", new Vec2(.5f, .5f), HorizontalAlign.Center, VerticalAlign.Center);
            }

            if (playerunit == null)
            {
                renderer.AddText("¤", new Vec2(.5f, .5f), HorizontalAlign.Center, VerticalAlign.Center);
                renderer.AddText("(  )", new Vec2(.5f, .5f), HorizontalAlign.Center, VerticalAlign.Center);
            }

            //if (body != null)
            //{
            //    MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
            //    if (obj != null && (obj as GameGuiObject) == null)
            //    {
            //        renderer.AddText(obj.Type.Name, new Vec2(.5f, .525f),
            //            HorizontalAlign.Center, VerticalAlign.Center);

            //        Dynamic dynamic = obj as Dynamic;
            //        if (dynamic != null)
            //        {
            //            if (dynamic.Type.LifeMax != 0)
            //            {
            //                float lifecoef = dynamic.Life / dynamic.Type.LifeMax;

            //                renderer.AddText("||||||||||", new Vec2(.5f - .04f, .55f), HorizontalAlign.Left,
            //                    VerticalAlign.Center, new ColorValue(.5f, .5f, .5f, .5f));

            //                float count = lifecoef * 10;
            //                String s = "";
            //                for (int n = 0; n < count; n++)
            //                    s += "|";

            //                renderer.AddText(s, new Vec2(.5f - .04f, .55f),
            //                    HorizontalAlign.Left, VerticalAlign.Center, new ColorValue(0, 1, 0, 1));
            //            }
            //            // we dont need to show mass

            //            //if (dynamic.PhysicsModel != null)
            //            //{
            //            //    float mass = 0;
            //            //    foreach (Body s in dynamic.PhysicsModel.Bodies)
            //            //        mass += s.Mass;
            //            //    string ss = string.Format("mass {0}", mass);
            //            //    renderer.AddText(ss, new Vec2(.5f - .04f, .6f),
            //            //        HorizontalAlign.Left, VerticalAlign.Center, new ColorValue(0, 1, 0, 1));
            //            //}
            //        }
            //    }
            //}

            //Tank specific
            DrawTankGunTarget(renderer);

            //AKunit spesific
            //DrawAKunitGunTarget(renderer);
        }

        private void targetgizmo(bool p)
        {
            // targetter
            {
                Control Targetgizmo = hudControl.Controls["Game/Targeting"];
                if (Targetgizmo != null)
                {
                    if (p)
                    {
                        Targetgizmo.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\AKRedT");
                    }
                    else
                    {
                        Targetgizmo.BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\AKGreenT");
                    }
                }
            }
        }

        //Tank specific
        private void DrawTankGunTarget(GuiRenderer renderer)
        {
            Tank tank = GetPlayerUnit() as Tank;
            if (tank == null)
                return;

            Gun gun = tank.MainGun;
            if (gun == null)
                return;

            Vec3 gunPosition = gun.GetInterpolatedPosition();
            Vec3 gunDirection = gun.GetInterpolatedRotation() * new Vec3(1, 0, 0);

            RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                new Ray(gunPosition, gunDirection * 1000),
                (int)ContactGroup.CastOnlyContact);

            bool finded = false;
            Vec3 pos = Vec3.Zero;

            foreach (RayCastResult result in piercingResult)
            {
                bool ignore = false;

                MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);

                Dynamic dynamic = obj as Dynamic;
                if (dynamic != null && dynamic.GetParentUnit() == tank)
                    ignore = true;

                if (!ignore)
                {
                    finded = true;
                    pos = result.Position;
                    break;
                }
            }

            if (!finded)
                pos = gunPosition + gunDirection * 1000;

            Vec2 screenPos;
            RendererWorld.Instance.DefaultCamera.ProjectToScreenCoordinates(pos, out screenPos);

            //draw quad
            {
                Texture texture = TextureManager.Instance.Load("Cursors/Target.png");
                float size = .015f;
                float aspect = RendererWorld.Instance.DefaultCamera.AspectRatio;
                Rect rectangle = new Rect(
                    screenPos.X - size, screenPos.Y - size * aspect,
                    screenPos.X + size, screenPos.Y + size * aspect);
                renderer.AddQuad(rectangle, new Rect(0, 0, 1, 1), texture,
                    new ColorValue(0, 1, 0));
            }
        }

        /// <summary>
        /// To draw some information of a player
        /// </summary>
        /// <param name="renderer"></param>
        private void DrawPlayerInformation(GuiRenderer renderer)
        {
            if (GetRealCameraType() == CameraType.Free)
                return;

            if (IsCutSceneEnabled())
                return;

            //debug draw an influences.
            {
                float posy = .8f;

                foreach (Entity entity in GetPlayerUnit().Children)
                {
                    Influence influence = entity as Influence;
                    if (influence == null)
                        continue;

                    renderer.AddText(influence.Type.Name, new Vec2(.7f, posy),
                        HorizontalAlign.Left, VerticalAlign.Center);

                    int count = (int)((float)influence.RemainingTime * 2.5f);
                    if (count > 50)
                        count = 50;
                    string str = "";
                    for (int n = 0; n < count; n++)
                        str += "I";

                    renderer.AddText(str, new Vec2(.85f, posy),
                        HorizontalAlign.Left, VerticalAlign.Center);

                    posy -= .025f;
                }
            }
        }

        private void DrawPlayersStatistics(GuiRenderer renderer)
        {
            if (IsCutSceneEnabled())
                return;

            if (PlayerManager.Instance == null)
                return;

            renderer.AddQuad(new Rect(.1f, .2f, .9f, .8f), new ColorValue(0, 0, 1, .5f));

            renderer.AddText("Players statistics", new Vec2(.5f, .25f),
                HorizontalAlign.Center, VerticalAlign.Center);

            if (EntitySystemWorld.Instance.IsServer() || EntitySystemWorld.Instance.IsSingle())
            {
                float posy = .3f;

                foreach (PlayerManager.ServerOrSingle_Player player in
                    PlayerManager.Instance.ServerOrSingle_Players)
                {
                    //string text = string.Format( "{0},   Frags: {1},   Ping: {2} ms", player.Name,
                    //	player.Frags, (int)( player.Ping * 1000 ) );
                    string text = string.Format(
                        "{0},   Hit Points: {1},   Kill Points: {2},   Assault Credits: {3},   Ping: {4} ms",
                        player.Name, player.HitPoints, player.KillPoints, player.AssaultCredits, (int)(player.Ping * 1000));
                    renderer.AddText(text, new Vec2(.2f, posy), HorizontalAlign.Left,
                        VerticalAlign.Center);

                    posy += .025f;
                }
            }

            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                float posy = .3f;

                foreach (PlayerManager.Client_Player player in PlayerManager.Instance.Client_Players)
                {
                    //string text = string.Format( "{0},   Frags: {1},   Ping: {2} ms", player.Name,
                    //	player.Frags, (int)( player.Ping * 1000 ) );
                    string text = string.Format(
                        "{0},   Hit Points: {1},   Kill Points: {2},   Assault Credits: {3},   Ping: {4} ms",
                        player.Name, player.HitPoints, player.KillPoints, player.AssaultCredits, (int)(player.Ping * 1000));
                    renderer.AddText(text, new Vec2(.2f, posy), HorizontalAlign.Left,
                        VerticalAlign.Center);

                    posy += .025f;
                }
            }
        }

        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);

            //Draw some HUD information
            if (GetPlayerUnit() != null)
            {
                UpdateHuds();

                if (GetRealCameraType() != CameraType.Free && !IsCutSceneEnabled() &&
                    GetActiveObserveCameraArea() == null)
                {
                    DrawTarget(renderer);

                    if (GetPlayerUnit() as AKunit != null || GetPlayerUnit() as Unit != null || GetPlayerUnit() as PlayerCharacter != null)
                    {
                        drawRadar(renderer);
                        UpdateAKunitHUD(renderer);
                    }
                }

                DrawPlayerInformation(renderer);

                if (EngineApp.Instance.IsKeyPressed(EKeys.F1) && !EngineConsole.Instance.Active)
                    DrawPlayersStatistics(renderer);

                if (GameNetworkServer.Instance != null || GameNetworkClient.Instance != null)
                {
                    renderer.AddText("\"F1\" for players statistics", new Vec2(.01f, .1f),
                        HorizontalAlign.Left, VerticalAlign.Top, new ColorValue(1, 1, 1, .5f));
                }
            }

            //Game is paused on server
            if (EntitySystemWorld.Instance.IsClientOnly() && !EntitySystemWorld.Instance.Simulation)
            {
                renderer.AddText("Game is paused on server", new Vec2(.5f, .5f),
                    HorizontalAlign.Center, VerticalAlign.Center, new ColorValue(1, 0, 0));
            }

            //screenMessages
            {
                Vec2 pos = new Vec2(.01f, .65f);
                for (int n = screenMessages.Count - 1; n >= 0; n--)
                {
                    ScreenMessage message = screenMessages[n];

                    ColorValue color = new ColorValue(1, 1, 1, message.timeRemaining);
                    if (color.Alpha > 1)
                        color.Alpha = 1;

                    renderer.AddText(message.text, pos, HorizontalAlign.Left, VerticalAlign.Top,
                        color);
                    pos.Y -= renderer.DefaultFont.Height;
                }
            }
        }

        private CameraType GetRealCameraType()
        {
            //Replacement the camera type depending on a current unit.
            Unit playerUnit = GetPlayerUnit();

            if (playerUnit != null)
            {
                //mech specific
                if (playerUnit as Mech != null)
                {
                    Mech mecha = playerUnit as Mech;
                    if (cameraType == CameraType.FPS || (cameraType >= CameraType.CamForward && cameraType <= CameraType.Count))
                    {
                        mecha.HidingThingy(true);
                    }
                    else
                    {
                        mecha.HidingThingy(false);
                    }

                    switch (cameraType)
                    {
                        case CameraType.CamForward:
                            {
                                cameraType = CameraType.CamForward;
                                mecha.HidingThingy(false);
                                break;
                            }

                        case CameraType.CamBack:
                            {
                                cameraType = CameraType.CamBack;
                                mecha.HidingThingy(false);
                                break;
                            }

                        case CameraType.CamLeft:
                            {
                                cameraType = CameraType.CamLeft;
                                mecha.HidingThingy(false);
                                break;
                            }

                        case CameraType.CamRight:
                            {
                                cameraType = CameraType.CamRight;
                                mecha.HidingThingy(false);
                                break;
                            }
                    }
                }

                //Turret specific
                if (playerUnit as Turret != null)
                {
                    if (cameraType == CameraType.FPS || (cameraType >= CameraType.CamForward && cameraType <= CameraType.Count))
                        return CameraType.TPS;
                }

                //Crane specific
                if (playerUnit as Crane != null)
                {
                    if (cameraType == CameraType.TPS || (cameraType >= CameraType.CamForward && cameraType <= CameraType.Count))
                        return CameraType.TPS;
                }

                //Crane specific
                if (playerUnit as Crane != null)
                {
                    if (cameraType == CameraType.TPS || (cameraType >= CameraType.CamForward && cameraType <= CameraType.Count))
                        return CameraType.FPS;
                }

                //Tank specific
                if (playerUnit as Tank != null)
                {
                    if (cameraType == CameraType.FPS || (cameraType >= CameraType.CamForward && cameraType <= CameraType.Count))
                        return CameraType.FPS;
                }

                //AKunit specific
                if (playerUnit as AKunit != null)
                {
                    if (cameraType == CameraType.FPS)
                        return CameraType.FPS;
                }
            }

            return cameraType;
        }

        //Unit GetPlayerUnit()
        //{
        //    if (PlayerIntellect.Instance == null)
        //        return null;
        //    return PlayerIntellect.Instance.ControlledObject;
        //}

        private bool SwitchUseStart()
        {
            if (switchUsing)
                return false;

            if (currentSwitch == null)
                return false;

            FloatSwitch floatSwitch = currentSwitch as FloatSwitch;
            if (floatSwitch != null)
                floatSwitch.UseStart();

            ProjectEntities.BooleanSwitch booleanSwitch = currentSwitch as ProjectEntities.BooleanSwitch;
            if (booleanSwitch != null)
                booleanSwitch.Press();

            switchUsing = true;

            return true;
        }

        private void SwitchUseEnd()
        {
            switchUsing = false;

            if (currentSwitch == null)
                return;

            FloatSwitch floatSwitch = currentSwitch as FloatSwitch;
            if (floatSwitch != null)
                floatSwitch.UseEnd();
        }

        private bool CurrentUnitAllowPlayerControlUse()
        {
            if (PlayerIntellect.Instance != null)
            {
                //change player unit
                if (currentSeeUnitAllowPlayerControl != null)
                {
                    PlayerIntellect.Instance.TryToChangeMainControlledUnit(
                        currentSeeUnitAllowPlayerControl);
                    return true;
                }

                //restore player unit
                if (PlayerIntellect.Instance.MainNotActiveUnit != null)
                {
                    PlayerIntellect.Instance.TryToRestoreMainControlledUnit();
                    return true;
                }
            }
            return false;
        }

        private bool IsCutSceneEnabled()
        {
            return CutSceneManager.Instance != null && CutSceneManager.Instance.CutSceneEnable;
        }

        private Vec3 CalculateTPSCameraPosition(Vec3 lookAt, Vec3 direction, float maxCameraDistance, MapObject ignoreObject)
        {
            const float sphereRadius = .5f;
            const float roughStep = .1f;
            const float detailedStep = .005f;

            //calculate max distance
            float maxDistance = maxCameraDistance;
            {
                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                    new Ray(lookAt, direction * maxCameraDistance), (int)ContactGroup.CastOnlyContact);
                foreach (RayCastResult result in piercingResult)
                {
                    bool ignore = false;

                    MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);
                    if (obj != null && obj == ignoreObject)
                        ignore = true;

                    if ((lookAt - result.Position).LengthSqr() < .001f)
                        ignore = true;

                    if (!ignore)
                    {
                        maxDistance = result.Distance;
                        break;
                    }
                }
            }

            //calculate with rough step
            float roughDistance = 0;
            {
                for (float distance = maxDistance; distance > 0; distance -= roughStep)
                {
                    Vec3 pos = lookAt + direction * distance;

                    //Using capsule volume to check.
                    //ODE: Sphere volume casting works bad on big precision on ODE.
                    Body[] bodies = PhysicsWorld.Instance.VolumeCast(
                        new Capsule(pos, pos + new Vec3(0, 0, .1f), sphereRadius),
                        (int)ContactGroup.CastOnlyContact);
                    //Body[] bodies = PhysicsWorld.Instance.VolumeCast( new Sphere( pos, sphereRadius ),
                    //   (int)ContactGroup.CastOnlyContact );

                    bool free = true;
                    foreach (Body body in bodies)
                    {
                        MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
                        if (obj != null && obj == ignoreObject)
                            continue;
                        free = false;
                        break;
                    }

                    if (free)
                    {
                        roughDistance = distance;
                        break;
                    }
                }
            }

            //calculate with detailed step and return
            if (roughDistance != 0)
            {
                for (float distance = roughDistance + roughStep; distance > 0; distance -= detailedStep)
                {
                    Vec3 pos = lookAt + direction * distance;

                    //Using capsule volume to check.
                    //ODE: Sphere volume casting works bad on big precision on ODE.
                    Body[] bodies = PhysicsWorld.Instance.VolumeCast(
                        new Capsule(pos, pos + new Vec3(0, 0, .1f), sphereRadius),
                        (int)ContactGroup.CastOnlyContact);
                    //Body[] bodies = PhysicsWorld.Instance.VolumeCast( new Sphere( pos, sphereRadius ),
                    //   (int)ContactGroup.CastOnlyContact );

                    bool free = true;
                    foreach (Body body in bodies)
                    {
                        MapObject obj = MapSystemWorld.GetMapObjectByBody(body);
                        if (obj != null && obj == ignoreObject)
                            continue;
                        free = false;
                        break;
                    }

                    if (free)
                        return pos;
                }
                return lookAt + direction * roughDistance;
            }

            return lookAt + direction * .01f;
        }

        private float Timeshoot = 1;

        protected override void OnGetCameraTransform(out Vec3 position, out Vec3 forward,
            out Vec3 up, ref Degree cameraFov)
        {
            position = Vec3.Zero;
            forward = Vec3.XAxis;
            up = Vec3.ZAxis;

            Unit unit = GetPlayerUnit();
            if (unit == null)
                return;

            PlayerIntellect.Instance.FPSCamera = false;

            //To use data about orientation the camera if the cut scene is switched on
            if (IsCutSceneEnabled())
                if (CutSceneManager.Instance.GetCamera(out position, out forward, out up, out cameraFov))
                    return;

            //To receive orientation the camera if the player is in a observe camera area
            if (GetActiveObserveCameraAreaCameraOrientation(out position, out forward, out up, ref cameraFov))
                return;

            Vec3 cameraLookDir = PlayerIntellect.Instance.LookDirection.GetVector();

            switch (GetRealCameraType())
            {
                case CameraType.TPS:
                    {
                        float cameraDistance;
                        float cameraCenterOffset;

                        if (IsPlayerUnitVehicle())
                        {
                            cameraDistance = tpsVehicleCameraDistance;
                            cameraCenterOffset = tpsVehicleCameraCenterOffset;
                        }
                        else
                        {
                            cameraDistance = tpsCameraDistance;
                            cameraCenterOffset = tpsCameraCenterOffset;
                        }

                        PlayerIntellect.Instance.UpdateTransformBeforeCameraPositionCalculation();

                        //To calculate orientation of a TPS camera.
                        Vec3 lookAt = unit.GetInterpolatedPosition() + new Vec3(0, 0, cameraCenterOffset);
                        Vec3 cameraPos = lookAt - cameraLookDir * cameraDistance;

                        RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                            new Ray(lookAt, cameraPos - lookAt), (int)ContactGroup.CastOnlyContact);
                        foreach (RayCastResult result in piercingResult)
                        {
                            bool ignore = false;

                            MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);

                            if (obj == unit)
                                ignore = true;

                            if ((lookAt - result.Position).LengthSqr() < .001f)
                                ignore = true;

                            //cut ignore objects here
                            //..

                            if (!ignore)
                            {
                                cameraPos = result.Position;
                                break;
                            }
                        }

                        position = cameraPos;
                        forward = (lookAt - position).GetNormalize();
                        up = unit.GetInterpolatedRotation().GetUp();
                    }
                    break;
                //Incin -- added camera views for mechs just uses helpers and quat rotations for forcing look directions
                case CameraType.FPS:
                case CameraType.CamForward:
                case CameraType.CamBack:
                case CameraType.CamLeft:
                case CameraType.CamRight:
                    {
                        //To calculate orientation of a FPS camera.

                        PlayerIntellect.Instance.UpdateTransformBeforeCameraPositionCalculation();
                        //if (unit is DamagerBall)
                        //{
                        //    //forward = cameraLookDir;
                        //    //Calculate orientation of a TPS camera.
                        //    DamagerBall ball = unit as DamagerBall;
                        //    float cameraDistance;
                        //    cameraDistance = .5f; //tpsCameraDistance;
                        //    //cameraCenterOffset = 1; //tpsCameraCenterOffset;
                        //    PlayerIntellect.Instance.UpdateTransformBeforeCameraPositionCalculation();

                        //    //To calculate orientation of a TPS camera.
                        //    Vec3 lookAt = ball.GetInterpolatedPosition() + new Vec3(0, 0, ball.BallRadius);

                        //    position = CalculateTPSCameraPosition(lookAt, cameraLookDir, cameraDistance, ball);
                        //    forward = (lookAt - position).GetNormalize();

                        //}
                        //else
                        if (unit is Turret)
                        {
                            //Turret specific
                            Gun mainGun = ((Turret)unit).MainGun;
                            position = mainGun.GetInterpolatedPosition();
                            position += unit.Type.FPSCameraOffset * mainGun.GetInterpolatedRotation();
                        }
                        else if (unit is Tank)
                        {
                            //Tank specific
                            Gun mainGun = ((Tank)unit).MainGun;
                            position = mainGun.GetInterpolatedPosition();
                            position += unit.Type.FPSCameraOffset * mainGun.GetInterpolatedRotation();
                        }
                        else if (unit is Mech)
                        {
                            Mech mech = GetPlayerUnit() as Mech;
                            Vec3 cameraPos = new Vec3(0, 0, 0);
                            Vec3 lookAt = new Vec3(0, 0, 0);
                            //Quat rotation = new Quat(0, 0, 0, 0);
                            MapObjectAttachedHelper helper;
                            if (mech != null)
                            {
                                //MapObjectAttachedMesh CT = mech.GetFirstAttachedObjectByAlias("CT") as MapObjectAttachedMesh;
                                if (cameraType == CameraType.FPS)
                                {
                                    helper = mech.CockpitLocation;
                                }
                                else if (cameraType == CameraType.CamBack)
                                {
                                    helper = mech.CamBackward;
                                    helper.RotationOffset = new Angles(0, 0, 180f).ToQuat();
                                    helper.PositionOffset = new Vec3(30, 0, 0);
                                }
                                else if (cameraType == CameraType.CamForward)
                                {
                                    helper = mech.CamForward;
                                    helper.PositionOffset = new Vec3(-30, 0, 0);
                                }
                                else if (cameraType == CameraType.CamLeft)
                                {
                                    helper = mech.CamLeft;
                                    helper.RotationOffset = new Angles(0, 0, 270f).ToQuat();
                                    helper.PositionOffset = new Vec3(0, -30, 0);
                                }
                                else if (cameraType == CameraType.CamRight)
                                {
                                    helper = mech.CamRight;
                                    helper.RotationOffset = new Angles(0, 0, 90f).ToQuat();
                                    helper.PositionOffset = new Vec3(0, 30, 0);
                                }
                                else
                                    helper = mech.CockpitLocation;

                                if (helper != null)
                                {
                                    position = (mech.towerBody.Rotation * helper.PositionOffset * helper.RotationOffset) + ((mech.GetInterpolatedPosition() + mech.GetInterpolatedRotation() * (mech.towerBodyLocalPosition + new Vec3(0, 0, mech.currentBob))));
                                    Vec3 cameraCenterOffset = position;// helper.PositionOffset;// +new Vec3(0, 0, 2.8f);
                                    lookAt = position + cameraCenterOffset;
                                }
                                else
                                {
                                    position = unit.GetInterpolatedPosition();
                                    position += unit.Type.FPSCameraOffset;
                                }

                                //position = cameraPos;
                                forward = (lookAt - position).GetNormalize();
                                up = unit.GetInterpolatedRotation().GetUp();// +helper.PositionOffset;

                                //Incin camera views
                                if (cameraType == CameraType.FPS || cameraType == CameraType.CamForward)
                                {
                                    forward = cameraLookDir;
                                    break;
                                }
                                else if (cameraType == CameraType.CamLeft || cameraType == CameraType.CamRight ||
                                        cameraType == CameraType.CamBack)
                                {
                                    forward = cameraLookDir * helper.RotationOffset;
                                    break;
                                }
                                //end camera views
                            }
                            else
                            {
                                position = unit.GetInterpolatedPosition();
                                position += unit.Type.FPSCameraOffset;
                                up = unit.GetInterpolatedRotation().GetUp();
                            }
                        }
                        else if (unit is AKunit)
                        {
                            //AKunit specific

                            position = unit.GetInterpolatedPosition();
                            position += unit.Type.FPSCameraOffset;
                            up = unit.GetInterpolatedRotation().GetUp();
                        }
                        else
                        {
                            //Characters, etc
                            position = unit.GetInterpolatedPosition();
                            position += unit.Type.FPSCameraOffset * unit.GetInterpolatedRotation();
                            up = unit.GetInterpolatedRotation().GetUp();
                        }
                        forward = cameraLookDir;
                    }
                    break;
            }

            //To update data in player intellect about type of the camera
            PlayerIntellect.Instance.FPSCamera = GetRealCameraType() == CameraType.FPS;

            float cameraOffset;
            if (IsPlayerUnitVehicle())
                cameraOffset = tpsVehicleCameraCenterOffset;
            else
                cameraOffset = tpsCameraCenterOffset;

            PlayerIntellect.Instance.TPSCameraCenterOffset = cameraOffset;

            //zoom AKUNIT
            AKunit akunit = GetPlayerUnit() as AKunit;
            if (EngineApp.Instance.IsMouseButtonPressed(EMouseButtons.Right))
            {
                if (akunit != null || GetPlayerUnit() as PlayerCharacter != null)
                {
                    Timeshoot += 0.1f;
                }

                if (GetPlayerUnit() as Turret != null)
                {
                    if (GetRealCameraType() == CameraType.TPS)
                        cameraFov /= 3;
                }
                else if (GetPlayerUnit() as PlayerCharacter != null)
                {
                    Timeshoot += 0.1f;
                    if (GetRealCameraType() == CameraType.TPS || GetRealCameraType() == CameraType.FPS)
                        cameraFov /= 3;
                }
            }
            else
            {
                Timeshoot -= 0.1f;
            }
            MathFunctions.Clamp(ref Timeshoot, 1, 3);
            cameraFov /= Timeshoot;
        }

        /// <summary>
        /// Finds observe area in which there is a player.
        /// </summary>
        /// <returns><b>ObserveCameraArea</b>if the player is in area; otherwise <b>null</b>.</returns>
        private ObserveCameraArea GetActiveObserveCameraArea()
        {
            Unit unit = GetPlayerUnit();
            if (unit == null)
                return null;

            foreach (ObserveCameraArea area in observeCameraAreas)
            {
                //check invalid area
                if (area.MapCamera == null && area.MapCurve == null)
                    continue;

                if (area.GetBox().IsContainsPoint(unit.Position))
                    return area;
            }
            return null;
        }

        /// <summary>
        /// Finds the nearest point to a map curve.
        /// </summary>
        /// <param name="destPos">The point to which is searched the nearest.</param>
        /// <param name="mapCurve">The map curve.</param>
        /// <returns>The nearest point to a map curve.</returns>
        private Vec3 GetNearestPointToMapCurve(Vec3 destPos, MapCurve mapCurve)
        {
            //Calculate cached points
            if (observeCameraMapCurvePoints != mapCurve)
            {
                observeCameraMapCurvePoints = mapCurve;

                observeCameraMapCurvePointsList.Clear();

                float curveLength = 0;
                {
                    ReadOnlyCollection<MapCurvePoint> points = mapCurve.Points;
                    for (int n = 0; n < points.Count - 1; n++)
                        curveLength += (points[n].Position - points[n + 1].Position).Length();
                }

                float step = 1.0f / curveLength / 100;
                for (float c = 0; c < 1; c += step)
                    observeCameraMapCurvePointsList.Add(mapCurve.CalculateCurvePointByCoefficient(c));
            }

            //calculate nearest point
            Vec3 nearestPoint = Vec3.Zero;
            float nearestDistanceSqr = float.MaxValue;

            foreach (Vec3 point in observeCameraMapCurvePointsList)
            {
                float distanceSqr = (point - destPos).LengthSqr();
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestPoint = point;
                    nearestDistanceSqr = distanceSqr;
                }
            }
            return nearestPoint;
        }

        /// <summary>
        /// Receives orientation of the camera in the observe area of in which there is a player.
        /// </summary>
        /// <param name="position">The camera position.</param>
        /// <param name="forward">The forward vector.</param>
        /// <param name="up">The up vector.</param>
        /// <param name="cameraFov">The camera FOV.</param>
        /// <returns><b>true</b>if the player is in any area; otherwise <b>false</b>.</returns>
        private bool GetActiveObserveCameraAreaCameraOrientation(out Vec3 position, out Vec3 forward,
            out Vec3 up, ref Degree cameraFov)
        {
            position = Vec3.Zero;
            forward = Vec3.XAxis;
            up = Vec3.ZAxis;

            ObserveCameraArea area = GetActiveObserveCameraArea();
            if (area == null)
                return false;

            Unit unit = GetPlayerUnit();

            if (area.MapCurve != null)
            {
                Vec3 unitPos = unit.GetInterpolatedPosition();
                Vec3 nearestPoint = GetNearestPointToMapCurve(unitPos, area.MapCurve);

                position = nearestPoint;
                forward = (unit.GetInterpolatedPosition() - position).GetNormalize();
                up = Vec3.ZAxis;

                if (area.MapCamera != null && area.MapCamera.Fov != 0)
                    cameraFov = area.MapCamera.Fov;
            }

            if (area.MapCamera != null)
            {
                position = area.MapCamera.Position;
                forward = area.MapCamera.Rotation * new Vec3(1, 0, 0);
                up = area.MapCamera.Rotation * new Vec3(0, 0, 1);

                if (area.MapCamera.Fov != 0)
                    cameraFov = area.MapCamera.Fov;
            }

            return true;
        }

        private bool IsPlayerUnitVehicle()
        {
            Unit playerUnit = GetPlayerUnit();

            //Tank specific
            if (playerUnit as Tank != null)
                return true;

            if (playerUnit as AKunit != null)
                return true;

            return false;
        }

        private static void ConsoleCommand_MovePlayerUnitToCamera(string arguments)
        {
            if (Map.Instance == null)
                return;
            if (PlayerIntellect.Instance == null)
                return;

            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                Log.Warning("You cannot to do it on the client.");
                return;
            }

            Unit unit = PlayerIntellect.Instance.ControlledObject;
            if (unit == null)
                return;

            Ray lookRay = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                new Vec2(.5f, .5f));

            RayCastResult result = PhysicsWorld.Instance.RayCast(
                lookRay, (int)ContactGroup.CastOnlyContact);

            if (result.Shape != null)
                unit.Position = result.Position + new Vec3(0, 0, unit.MapBounds.GetSize().Z);
        }

        private void GameControlsManager_GameControlsEvent(GameControlsEventData e)
        {//
            //GameControlsKeyDownEventData
            {
                GameControlsKeyDownEventData evt = e as GameControlsKeyDownEventData;
                if (evt != null)
                {
                    //"Use" control key
                    if (evt.ControlKey == GameControlKeys.Use)
                    {
                        //currentAttachedGuiObject
                        if (currentAttachedGuiObject != null)
                        {
                            currentAttachedGuiObject.ControlManager.DoMouseDown(EMouseButtons.Left);
                            return;
                        }

                        //key down for switch use
                        if (SwitchUseStart())
                            return;

                        if (CurrentUnitAllowPlayerControlUse())
                            return;
                    }

                    return;
                }
            }

            //GameControlsKeyUpEventData
            {
                GameControlsKeyUpEventData evt = e as GameControlsKeyUpEventData;
                if (evt != null)
                {
                    //"Use" control key
                    //foreach (evt.ControlKey key in GameControlKeys evt.ControlKeys().Count)

                    if (evt.ControlKey == GameControlKeys.Use)
                    {
                        //currentAttachedGuiObject
                        if (currentAttachedGuiObject != null)
                            currentAttachedGuiObject.ControlManager.DoMouseUp(EMouseButtons.Left);

                        //key up for switch use
                        SwitchUseEnd();
                    }

                    return;
                }
            }
        }

        private bool needupdate;
        private bool needupdateRabbit;

        private void UpdateHuds()
        {
            //Change player controlled unit
            if (PlayerIntellect.Instance.ControlledObject is AKunit)
            {
                needupdateRabbit = false;
                if (!needupdate)
                {
                    Controls.Clear();

                    AKunit mech = PlayerIntellect.Instance.ControlledObject as AKunit;

                    hudControl = ControlDeclarationManager.Instance.CreateControl(mech.Type.AKunitControlGui);

                    if (hudControl == null)
                    {
                        hudControl = ControlDeclarationManager.Instance.CreateControl("Assault Knights\\Huds\\AKunitHud.gui");
                    }

                    Controls.Add(hudControl);
                    needupdate = true;
                }
            }
            else
            {
                needupdate = false;
                if (!needupdateRabbit)
                {
                    Controls.Clear();
                    hudControl = ControlDeclarationManager.Instance.CreateControl("Gui\\AKActionHUD.gui");
                    Controls.Add(hudControl);
                    needupdateRabbit = true;
                }
            }
        }

        private void DisplayTextureOverPosition(Vec3 position, Texture texture, GuiRenderer renderer, ColorValue color)
        {
            Vec2I size = texture.SourceSize / 2;
            float sizeX = (float)size.X / (float)EngineApp.Instance.VideoMode.X;
            float sizeY = (float)size.Y / (float)EngineApp.Instance.VideoMode.Y;

            Vec2 screenPos;
            RendererWorld.Instance.DefaultCamera.ProjectToScreenCoordinates(position, out screenPos);

            if (screenPos.X != 0 && screenPos.Y != 0)
            {
                renderer.AddQuad(new Rect(new Vec2(screenPos.X - sizeX / 2, screenPos.Y - sizeY / 2), new Vec2(screenPos.X + sizeX / 2, screenPos.Y + sizeY / 2)), new Rect(0, 0, 1f, 1f), texture, color);
            }
        }

        private void DisplayTextureOverPosition(Vec2 screenPos, Texture texture, GuiRenderer renderer, ColorValue color)
        {
            Vec2I size = texture.SourceSize / 2;
            float sizeX = (float)size.X / (float)EngineApp.Instance.VideoMode.X;
            float sizeY = (float)size.Y / (float)EngineApp.Instance.VideoMode.Y;

            if (screenPos.X != 0 && screenPos.Y != 0)
            {
                renderer.AddQuad(new Rect(new Vec2(screenPos.X - sizeX / 2, screenPos.Y - sizeY / 2), new Vec2(screenPos.X + sizeX / 2, screenPos.Y + sizeY / 2)), new Rect(0, 0, 1f, 1f), texture, color);
            }
        }

        private void TargetUnLock()
        {
            AKunit akunit = GetPlayerUnit() as AKunit;
            if (akunit == null) return;

            float dist;
            Unit unit = null;

            Map.Instance.GetObjectsByScreenRectangle(new Rect(0.49f, 0.49f, .51f, .51f), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject candidateMapObject)
            {
                unit = candidateMapObject as Unit;
            });

            if (unit != null)
            {
                dist = (unit.Position - akunit.Position).Length();
            }

            if (akunit.CurrentMissileTarget != null && (unit == null || unit != akunit.CurrentMissileTarget))
            {
                akunit.UnlockP(true);
            }
        }

        private void DrawTargetLock(GuiRenderer renderer)
        {
            AKunit akunit = GetPlayerUnit() as AKunit;
            PlayerCharacter playercharacter = GetPlayerUnit() as PlayerCharacter;
            //if crosshair is pointing near enough
            if (akunit != null)
            {
                Map.Instance.GetObjectsByScreenRectangle(new Rect(0.49f, 0.49f, .51f, .51f), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject candidateMapObject)
                {
                    Unit unit = candidateMapObject as Unit;
                    if (unit == null || (akunit == null && playercharacter == null)) return;

                    float dist = 1024;

                    if (akunit != null)
                    {
                        if (akunit.CurrentReticuleTarget == null || akunit.CurrentReticuleTarget != unit || akunit.MainGun == null)
                        {
                            akunit = null;
                            return;
                        }
                        else
                            dist = (unit.Position - akunit.Position).Length();
                        if (dist < AKunitRadarRange && akunit != null)
                        {
                            float done = akunit.GetLockingCompletionPercentage();

                            if (akunit.IsTargetLocked(unit))
                            {
                                DisplayTextureOverPosition(unit.Position, missileLockDone, renderer, new ColorValue(1f, 1f, 1f));
                                akunit.PlayLockingSound();
                            }
                            else if (done > 0.50f)
                            {
                                DisplayTextureOverPosition(unit.Position, missileLockClose, renderer, new ColorValue(1f, 1f, 1f));
                            }
                            else
                            {
                                DisplayTextureOverPosition(unit.Position, missileLockStarted, renderer, new ColorValue(1f, 1f, 1f));
                            }
                        }
                    }//else return not needed
                });
            }
            //Incin modified to work with player base no rendered target for now for player, sorry...
            else if (playercharacter != null)
            {
                Map.Instance.GetObjectsByScreenRectangle(new Rect(0.49f, 0.49f, .51f, .51f), MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject candidateMapObject)
                {
                    Unit unit = candidateMapObject as Unit;
                    if (unit == null || playercharacter == null)
                        return;

                    float dist;

                    if (playercharacter.CurrentReticuleTarget == null || playercharacter.CurrentReticuleTarget != unit || playercharacter.ActiveWeapon == null)
                        return;
                    dist = (unit.Position - playercharacter.Position).Length();

                    if (dist < AKunitRadarRange)
                    {
                        //float done = playercharacter.GetLockingCompletionPercentage();
                        if (dist < 5000 && playercharacter.CurrentReticuleTarget != null && playercharacter.ActiveWeapon != null)
                        //if (playercharacter.IsTargetLocked(unit))
                        {
                            DisplayTextureOverPosition(unit.Position, missileLockDone, renderer, new ColorValue(1f, 1f, 1f));
                            //akunit.PlayLockingSound();
                        }
                        else if (dist < 10000)
                        //else if (done > 0.50f)
                        {
                            DisplayTextureOverPosition(unit.Position, missileLockClose, renderer, new ColorValue(1f, 1f, 1f));
                        }
                        else
                        {
                            DisplayTextureOverPosition(unit.Position, missileLockStarted, renderer, new ColorValue(1f, 1f, 1f));
                        }
                    }
                });
            }
        }

        private void InitCameraViewFromTarget()
        {
            int textureSize = 1024;

            cameraTexture = TextureManager.Instance.Create(
                TextureManager.Instance.GetUniqueName("RemoteView"), Texture.Type.Type2D,
                new Vec2I(textureSize, textureSize), 1, 0, PixelFormat.R8G8B8, Texture.Usage.RenderTarget);

            RenderTexture renderTexture = cameraTexture.GetBuffer().GetRenderTarget();

            //you can update render texture manually by means renderTexture.Update() method. For this task set AutoUpdate = false;
            renderTexture.AutoUpdate = true;

            //create camera
            string cameraName = SceneManager.Instance.GetUniqueCameraName("RemoteView");
            //rmCamera.Position = new Vec3(0, 0, 30);
            //rmCamera.LookAt(new Vec3(0,-.75f, 0));
            rmCamera = SceneManager.Instance.CreateCamera(cameraName);

            rmCamera.ProjectionType = ProjectionTypes.Perspective;
            rmCamera.PolygonMode = PolygonMode.Wireframe;
            //rmCamera.Position = new Vec3(0,0,500);
            //rmCamera.LookAt(new Vec3(0, -30, -30));

            renderTexture.AddViewport(rmCamera);
        }

        private void GetCameraViewFromTarget()
        {
            //if (hudControl.FileNameCreated == "Gui\\AKActionHUD.gui")
            //    return;
            AKunit akunit = GetPlayerUnit() as AKunit;

            Unit playerunit = GetPlayerUnit() as Unit;
            Unit unit = null;
            if (playerunit as PlayerCharacter != null)
            {
                unit = playerunit.CurrentReticuleTarget;
                if (unit == null)
                    return;
                rmCamera.LookAt(playerunit.CurrentReticuleTarget.Position);
                rmCamera.FarClipDistance = (playerunit.CurrentReticuleTarget.Position - rmCamera.Position).Length() * 1.3f;
                rmCamera.Position = playerunit.CurrentReticuleTarget.Position + playerunit.CurrentReticuleTarget.Rotation.GetUp() * 10f;

                if (playerunit != null && playerunit.CurrentMissileTarget != null)
                {
                    hudControl.Controls["Game/TargetWindow/Locked"].Visible = true;
                }
                else
                {
                    hudControl.Controls["Game/TargetWindow/Locked"].Visible = false;
                }

                if (playerunit.CurrentReticuleTarget as AKunit != null)
                    ShowEnemyBP(true);
                else
                    ShowEnemyBP(false);
            }

            if (akunit != null && akunit.CurrentReticuleTarget != null)
            {
                unit = akunit.CurrentReticuleTarget;
                rmCamera.LookAt(akunit.CurrentReticuleTarget.Position);
                rmCamera.FarClipDistance = (akunit.CurrentReticuleTarget.Position - rmCamera.Position).Length() * 1.3f;
                rmCamera.Position = akunit.CurrentReticuleTarget.Position + akunit.CurrentReticuleTarget.Rotation.GetUp() * 10f;

                if (unit == akunit.CurrentMissileTarget)
                {
                    hudControl.Controls["Game/TargetWindow/Locked"].Visible = true;
                }
                else
                {
                    hudControl.Controls["Game/TargetWindow/Locked"].Visible = false;
                }

                if (akunit.CurrentReticuleTarget as AKunit != null)
                    ShowEnemyBP(true);
                else
                    ShowEnemyBP(false);
            }
            else if (akunit != null)
            {
                ShowEnemyBP(false);
                hudControl.Controls["Game/TargetWindow"].BackTexture = null;
                rmCamera.Visible = false;
                return;
            }

            if (akunit == null) return;
            rmCamera.FixedUp = (Vec3.ZAxis); //object rotation -- iNCIN look at
            rmCamera.Visible = true;
            rmCamera.PolygonMode = PolygonMode.Wireframe;
            rmCamera.NearClipDistance = 1.0f;

            if (!akunit.MechShutDown)
            {
                hudControl.Controls["Game/TargetWindow"].BackTexture = cameraTexture;
            }
            else
            {
                hudControl.Controls["Game/TargetWindow"].BackTexture = TextureManager.Instance.Load("Assault Knights\\Huds\\TargetOH");
            }
        }

        private void ShowEnemyBP(bool show)
        {
            if (show)
            {
                AKunit player = GetPlayerUnit() as AKunit;
                PlayerCharacter pcharacter = GetPlayerUnit() as PlayerCharacter;
                AKunit Enemy;
                if (player != null)
                {
                    Enemy = player.CurrentReticuleTarget as AKunit;
                }
                else
                {
                    Enemy = pcharacter.CurrentReticuleTarget as AKunit;
                }

                if (Enemy == null)
                    return;

                hudControl.Controls["Game/EnemyBP"].Visible = true;
                for (int i = 0; i < Enemy.Bp.Count; i++)
                {
                    AKunit.BP BP = Enemy.Bp[i];
                    if (hudControl.Controls["Game/EnemyBP/BPB" + (i + 1)] == null) break;

                    hudControl.Controls["Game/EnemyBP/BPT" + (i + 1)].Text = BP.GUIDesplayName;
                    //Damage bars
                    {
                        float coef;
                        coef = BP.HitPoints;
                        coef /= BP.HitpointsMax;

                        hudControl.Controls["Game/EnemyBP/BPB" + (i + 1)].Visible = true;
                        hudControl.Controls["Game/EnemyBP/BPT" + (i + 1)].Visible = true;
                        hudControl.Controls["Game/EnemyBP/BPBG" + (i + 1)].Visible = true;

                        Control PBBar = hudControl.Controls["Game/EnemyBP/BPB" + (i + 1)];

                        Vec2 BPoriginalSize = new Vec2(15, 90);
                        Vec2 BPinterval = new Vec2(0, BPoriginalSize.Y);
                        float BPsizeY = coef * (BPinterval[1] - BPinterval[0]);

                        Vec2 interval = new Vec2(BPoriginalSize.Y, 0);
                        float PosY = 120 + coef * (interval[1] - interval[0]);

                        PBBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(BPoriginalSize.X, BPsizeY));
                        PBBar.BackTextureCoord = new Rect(0, 0, 1, BPsizeY / BPoriginalSize.Y);
                        PBBar.Position = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(PBBar.Position.Value.X, PosY));

                        //float red;
                        //float green;
                        //red = 255 - (coef * 255);
                        //green = (coef * 255);
                        //PBBar.ColorMultiplier = new ColorValue(red, green, 0, 255);
                    }
                }
                for (int i = 6; i > Enemy.Type.BodyParts.Count - 1; i--)
                {
                    hudControl.Controls["Game/EnemyBP/BPB" + (i + 1)].Visible = false;
                    hudControl.Controls["Game/EnemyBP/BPT" + (i + 1)].Visible = false;
                    hudControl.Controls["Game/EnemyBP/BPBG" + (i + 1)].Visible = false;
                }
            }
            else
            {
                hudControl.Controls["Game/EnemyBP"].Visible = false;
            }
        }

        //msg helli
        private void TickAKunitCamera(float delta)
        {
            AKVTOL Vtol = GetPlayerUnit() as AKVTOL;
            Helli playerHelli = GetPlayerUnit() as Helli;
            AKJet AKjet = GetPlayerUnit() as AKJet;
            //DamagerBall ball = GetPlayerUnit() as DamagerBall;
            ////Mech mech = GetPlayerUnit() as Mech;
            //if (ball != null)
            //{
            //    ;
            //}
            //else
            if (playerHelli == null && AKjet == null && Vtol == null)// && mech == null)
                return;

            if (EngineApp.Instance.IsKeyPressed(EKeys.Control))
            { return; }

            SphereDir cardir = SphereDir.FromVector(GetPlayerUnit().Rotation.GetForward()); ;

            if (playerHelli != null)
                cardir = SphereDir.FromVector(playerHelli.Rotation.GetForward());

            if (AKjet != null)
                cardir = SphereDir.FromVector(AKjet.Rotation.GetForward());

            SphereDir dir = PlayerIntellect.Instance.LookDirection;
            //horizantal
            dir.Horizontal = MathFunctions.RadiansNormalize360(dir.Horizontal);
            cardir.Horizontal = MathFunctions.RadiansNormalize360(cardir.Horizontal);

            // if (a){};
            float fix = 1;
            if (GetRealCameraType() == CameraType.FPS) fix = 3;
            delta = delta * 3 * fix; //increase delta to make the camera more steady

            if (dir.Horizontal <= cardir.Horizontal)
            {
                if ((cardir.Horizontal - dir.Horizontal) < Math.PI)
                    dir.Horizontal += (cardir.Horizontal - dir.Horizontal) * delta;
                else
                    if ((2 * Math.PI - (cardir.Horizontal - dir.Horizontal)) <= Math.PI)
                        dir.Horizontal -= ((float)(2 * Math.PI) - (cardir.Horizontal - dir.Horizontal)) * delta;
            }
            else
            {
                if ((dir.Horizontal - cardir.Horizontal) < Math.PI)
                    dir.Horizontal -= (dir.Horizontal - cardir.Horizontal) * delta;
                else
                    if ((2 * Math.PI - (dir.Horizontal - cardir.Horizontal)) <= Math.PI)
                        dir.Horizontal += ((float)(2 * Math.PI) - (dir.Horizontal - cardir.Horizontal)) * delta;
            }

            dir.Vertical = cardir.Vertical;
            PlayerIntellect.Instance.LookDirection = dir;
            if (AKjet != null)
                RendererWorld.Instance.DefaultCamera.Direction = AKjet.Rotation.GetForward();

            if (playerHelli != null)
                RendererWorld.Instance.DefaultCamera.Direction = playerHelli.Rotation.GetForward();

            if (Vtol != null)
                RendererWorld.Instance.DefaultCamera.Direction = Vtol.Rotation.GetForward();

            //if (mech != null)
            //{
            //    if (GetRealCameraType() == CameraType.FPS)
            //    {
            //        PlayerIntellect.Instance.LookDirection.GetVector();
            //        PlayerIntellect.Instance.UpdateTransformBeforeCameraPositionCalculation();
            //        MapObjectAttachedHelper helper = mech.CockpitLocation;
            //        Vec3 vec = helper.TypeObject.Position;
            //        RendererWorld.Instance.DefaultCamera.Position = mech.TowerBody.GetInterpolatedPosition() * vec;
            //    }
            //    RendererWorld.Instance.DefaultCamera.Direction = mech.TowerBody.Rotation.GetForward();
            //}
        }
    }
}