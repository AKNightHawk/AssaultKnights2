// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Engine.FileSystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.UISystem;
using Engine.Utils;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    /// <summary>
    /// Defines a window of options.
    /// </summary>
    public class OptionsWindow : Control
    {
        //Enum device  Types installed on system
        public enum Devices
        {
            Nothing_Selected = 0,
            Keyboard, //what type of keyboard? phone? keyboard? or default values
            Mouse, //name by name of device or default values
            Joystick, //named by name of device or default values
            Joystick_Xbox360, //name by name of device or default values
            Joystick_WII,
            Joystick_Playstation,
            RemoteControl, //A controller for TV.. bluetoothed?
            Custom, //add customized Controls.. any custom Controls designed
            //Custom_Audio, //named by device or device defaults example (zoom device to control guitar inputs)
            //All_Devices // Index ... No added devices?
        }

        private static int lastPageIndex;

        private Control window;
        private Button[] pageButtons = new Button[5];
        private TabControl tabControl;

        private ComboBox comboBoxResolution;
        private ComboBox comboBoxInputDevices;
        private CheckBox checkBoxDepthBufferAccess;
        private ComboBox comboBoxAntialiasing;

        //Incin -- Need class access to these items
        private ComboBox cmbBoxDevice; //need local access to this

        //private Control commandwindow;
        private ListBox controlsList = null; //need local access for this

        private JoystickAxisFilters axisfilterselection = JoystickAxisFilters.DEADZONE; //this is used to select filter axis of each
        private Button axisfilterbutton;
        private Button btnAddBinding; //for adding commands to binding window
        ///////////////////////////////////////////

        private class ComboBoxItem
        {
            private string identifier;
            private string displayName;

            public ComboBoxItem(string identifier, string displayName)
            {
                this.identifier = identifier;
                this.displayName = displayName;
            }

            public string Identifier
            {
                get { return identifier; }
            }

            public string DisplayName
            {
                get { return displayName; }
            }

            public override string ToString()
            {
                return displayName;
            }
        }

        ///////////////////////////////////////////

        public class ShadowTechniqueItem
        {
            private ShadowTechniques technique;
            private string text;

            public ShadowTechniqueItem(ShadowTechniques technique, string text)
            {
                this.technique = technique;
                this.text = text;
            }

            public ShadowTechniques Technique
            {
                get { return technique; }
            }

            public override string ToString()
            {
                return text;
            }
        }

        ///////////////////////////////////////////

        protected override void OnAttach()
        {
            base.OnAttach();

            ComboBox comboBox;
            ScrollBar scrollBar;
            CheckBox checkBox;
            TextBox textBox;

            window = ControlDeclarationManager.Instance.CreateControl("Gui\\OptionsWindow.gui");
            Controls.Add(window);

            tabControl = (TabControl)window.Controls["TabControl"];

            BackColor = new ColorValue(0, 0, 0, .5f);
            MouseCover = true;

            //load Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            TextBlock rendererBlock = null;
            if (engineConfigBlock != null)
                rendererBlock = engineConfigBlock.FindChild("Renderer");

            //page buttons
            pageButtons[0] = (Button)window.Controls["ButtonVideo"];
            pageButtons[1] = (Button)window.Controls["ButtonShadows"];
            pageButtons[2] = (Button)window.Controls["ButtonSound"];
            pageButtons[3] = (Button)window.Controls["ButtonControls"];
            pageButtons[4] = (Button)window.Controls["ButtonLanguage"];
            foreach (Button pageButton in pageButtons)
                pageButton.Click += new Button.ClickDelegate(pageButton_Click);

            //Close button
            ((Button)window.Controls["Close"]).Click += delegate(Button sender)
            {
                SetShouldDetach();
            };

            //pageVideo
            {
                Control pageVideo = tabControl.Controls["Video"];

                Vec2I currentMode = EngineApp.Instance.VideoMode;

                //screenResolutionComboBox
                comboBox = (ComboBox)pageVideo.Controls["ScreenResolution"];
                comboBox.Enable = !EngineApp.Instance.MultiMonitorMode;
                comboBoxResolution = comboBox;

                if (EngineApp.Instance.MultiMonitorMode)
                {
                    comboBox.Items.Add(string.Format("{0}x{1} (multi-monitor)", currentMode.X,
                        currentMode.Y));
                    comboBox.SelectedIndex = 0;
                }
                else
                {
                    foreach (Vec2I mode in DisplaySettings.VideoModes)
                    {
                        if (mode.X < 640)
                            continue;

                        comboBox.Items.Add(string.Format("{0}x{1}", mode.X, mode.Y));

                        if (mode == currentMode)
                            comboBox.SelectedIndex = comboBox.Items.Count - 1;
                    }

                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        ChangeVideoMode();
                    };
                }

                //gamma
                scrollBar = (ScrollBar)pageVideo.Controls["Gamma"];
                scrollBar.Value = GameEngineApp._Gamma;
                scrollBar.Enable = true;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    float value = float.Parse(sender.Value.ToString("F1"));
                    GameEngineApp._Gamma = value;
                    pageVideo.Controls["GammaValue"].Text = value.ToString("F1");
                };
                pageVideo.Controls["GammaValue"].Text = GameEngineApp._Gamma.ToString("F1");

                //MaterialScheme
                {
                    comboBox = (ComboBox)pageVideo.Controls["MaterialScheme"];
                    foreach (MaterialSchemes materialScheme in
                        Enum.GetValues(typeof(MaterialSchemes)))
                    {
                        comboBox.Items.Add(materialScheme.ToString());

                        if (GameEngineApp.MaterialScheme == materialScheme)
                            comboBox.SelectedIndex = comboBox.Items.Count - 1;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        if (sender.SelectedIndex != -1)
                            GameEngineApp.MaterialScheme = (MaterialSchemes)sender.SelectedIndex;
                    };
                }

                //fullScreen
                checkBox = (CheckBox)pageVideo.Controls["FullScreen"];
                checkBox.Enable = !EngineApp.Instance.MultiMonitorMode;
                checkBox.Checked = EngineApp.Instance.FullScreen;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    EngineApp.Instance.FullScreen = sender.Checked;
                };

                //RenderTechnique
                {
                    comboBox = (ComboBox)pageVideo.Controls["RenderTechnique"];
                    comboBox.Items.Add(new ComboBoxItem("RecommendedSetting", Translate("Recommended setting")));
                    comboBox.Items.Add(new ComboBoxItem("Standard", Translate("Low Dynamic Range (Standard)")));
                    comboBox.Items.Add(new ComboBoxItem("HDR", Translate("High Dynamic Range (HDR)")));

                    string renderTechnique = "";
                    if (rendererBlock != null && rendererBlock.IsAttributeExist("renderTechnique"))
                        renderTechnique = rendererBlock.GetAttribute("renderTechnique");

                    for (int n = 0; n < comboBox.Items.Count; n++)
                    {
                        ComboBoxItem item = (ComboBoxItem)comboBox.Items[n];
                        if (item.Identifier == renderTechnique)
                            comboBox.SelectedIndex = n;
                    }
                    if (comboBox.SelectedIndex == -1)
                        comboBox.SelectedIndex = 0;

                    comboBox.SelectedIndexChange += comboBoxRenderTechnique_SelectedIndexChange;
                }

                //Filtering
                {
                    comboBox = (ComboBox)pageVideo.Controls["Filtering"];

                    Type enumType = typeof(RendererWorld.FilteringModes);
                    LocalizedEnumConverter enumConverter = new LocalizedEnumConverter(enumType);

                    RendererWorld.FilteringModes filtering = RendererWorld.FilteringModes.RecommendedSetting;
                    //get value from Engine.config.
                    if (rendererBlock != null && rendererBlock.IsAttributeExist("filtering"))
                    {
                        try
                        {
                            filtering = (RendererWorld.FilteringModes)Enum.Parse(enumType, rendererBlock.GetAttribute("filtering"));
                        }
                        catch { }
                    }

                    RendererWorld.FilteringModes[] values = (RendererWorld.FilteringModes[])Enum.GetValues(enumType);
                    for (int n = 0; n < values.Length; n++)
                    {
                        RendererWorld.FilteringModes value = values[n];
                        string valueStr = enumConverter.ConvertToString(value);
                        comboBox.Items.Add(new ComboBoxItem(value.ToString(), Translate(valueStr)));
                        if (filtering == value)
                            comboBox.SelectedIndex = comboBox.Items.Count - 1;
                    }
                    if (comboBox.SelectedIndex == -1)
                        comboBox.SelectedIndex = 0;

                    comboBox.SelectedIndexChange += comboBoxFiltering_SelectedIndexChange;
                }

                //DepthBufferAccess
                {
                    checkBox = (CheckBox)pageVideo.Controls["DepthBufferAccess"];
                    checkBoxDepthBufferAccess = checkBox;

                    bool depthBufferAccess = true;
                    //get value from Engine.config.
                    if (rendererBlock != null && rendererBlock.IsAttributeExist("depthBufferAccess"))
                        depthBufferAccess = bool.Parse(rendererBlock.GetAttribute("depthBufferAccess"));
                    checkBox.Checked = depthBufferAccess;

                    checkBox.CheckedChange += checkBoxDepthBufferAccess_CheckedChange;
                }

                //FSAA
                {
                    comboBox = (ComboBox)pageVideo.Controls["FSAA"];
                    comboBoxAntialiasing = comboBox;

                    UpdateComboBoxAntialiasing();

                    string fullSceneAntialiasing = "";
                    if (rendererBlock != null && rendererBlock.IsAttributeExist("fullSceneAntialiasing"))
                        fullSceneAntialiasing = rendererBlock.GetAttribute("fullSceneAntialiasing");
                    for (int n = 0; n < comboBoxAntialiasing.Items.Count; n++)
                    {
                        ComboBoxItem item = (ComboBoxItem)comboBoxAntialiasing.Items[n];
                        if (item.Identifier == fullSceneAntialiasing)
                            comboBoxAntialiasing.SelectedIndex = n;
                    }

                    comboBoxAntialiasing.SelectedIndexChange += comboBoxAntialiasing_SelectedIndexChange;
                }

                //VerticalSync
                {
                    checkBox = (CheckBox)pageVideo.Controls["VerticalSync"];

                    bool verticalSync = RendererWorld.InitializationOptions.VerticalSync;
                    //get value from Engine.config.
                    if (rendererBlock != null && rendererBlock.IsAttributeExist("verticalSync"))
                        verticalSync = bool.Parse(rendererBlock.GetAttribute("verticalSync"));
                    checkBox.Checked = verticalSync;

                    checkBox.CheckedChange += checkBoxVerticalSync_CheckedChange;
                }

                //VideoRestart
                {
                    Button button = (Button)pageVideo.Controls["VideoRestart"];
                    button.Click += buttonVideoRestart_Click;
                }

                //waterReflectionLevel
                comboBox = (ComboBox)pageVideo.Controls["WaterReflectionLevel"];
                foreach (WaterPlane.ReflectionLevels level in Enum.GetValues(
                    typeof(WaterPlane.ReflectionLevels)))
                {
                    comboBox.Items.Add(level);
                    if (GameEngineApp.WaterReflectionLevel == level)
                        comboBox.SelectedIndex = comboBox.Items.Count - 1;
                }
                comboBox.SelectedIndexChange += delegate(ComboBox sender)
                {
                    GameEngineApp.WaterReflectionLevel = (WaterPlane.ReflectionLevels)sender.SelectedItem;
                };

                //showDecorativeObjects
                checkBox = (CheckBox)pageVideo.Controls["ShowDecorativeObjects"];
                checkBox.Checked = GameEngineApp.ShowDecorativeObjects;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    GameEngineApp.ShowDecorativeObjects = sender.Checked;
                };

                //showSystemCursorCheckBox
                checkBox = (CheckBox)pageVideo.Controls["ShowSystemCursor"];
                checkBox.Checked = GameEngineApp._ShowSystemCursor;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    GameEngineApp._ShowSystemCursor = sender.Checked;
                    sender.Checked = GameEngineApp._ShowSystemCursor;
                };

                //showFPSCheckBox
                checkBox = (CheckBox)pageVideo.Controls["ShowFPS"];
                checkBox.Checked = GameEngineApp._DrawFPS;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    GameEngineApp._DrawFPS = sender.Checked;
                    sender.Checked = GameEngineApp._DrawFPS;
                };
            }

            //pageShadows
            {
                Control pageShadows = tabControl.Controls["Shadows"];

                //ShadowTechnique
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowTechnique"];

                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.None, "None"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapLow, "Shadowmap Low"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapMedium, "Shadowmap Medium"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapHigh, "Shadowmap High"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapLowPSSM, "PSSMx3 Low"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapMediumPSSM, "PSSMx3 Medium"));
                    comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.ShadowmapHighPSSM, "PSSMx3 High"));
                    //comboBox.Items.Add(new ShadowTechniqueItem(ShadowTechniques.Stencil, "Stencil"));

                    for (int n = 0; n < comboBox.Items.Count; n++)
                    {
                        ShadowTechniqueItem item = (ShadowTechniqueItem)comboBox.Items[n];
                        if (item.Technique == GameEngineApp.ShadowTechnique)
                            comboBox.SelectedIndex = n;
                    }

                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        if (sender.SelectedIndex != -1)
                        {
                            ShadowTechniqueItem item = (ShadowTechniqueItem)sender.SelectedItem;
                            GameEngineApp.ShadowTechnique = item.Technique;
                        }
                        UpdateShadowControlsEnable();
                    };
                    UpdateShadowControlsEnable();
                }

                //ShadowUseMapSettings
                {
                    checkBox = (CheckBox)pageShadows.Controls["ShadowUseMapSettings"];
                    checkBox.Checked = GameEngineApp.ShadowUseMapSettings;
                    checkBox.CheckedChange += delegate(CheckBox sender)
                    {
                        GameEngineApp.ShadowUseMapSettings = sender.Checked;
                        if (sender.Checked && Map.Instance != null)
                        {
                            GameEngineApp.ShadowPSSMSplitFactors = Map.Instance.InitialShadowPSSMSplitFactors;
                            GameEngineApp.ShadowFarDistance = Map.Instance.InitialShadowFarDistance;
                            GameEngineApp.ShadowColor = Map.Instance.InitialShadowColor;
                        }

                        UpdateShadowControlsEnable();

                        if (sender.Checked)
                        {
                            ((ScrollBar)pageShadows.Controls["ShadowFarDistance"]).Value =
                                GameEngineApp.ShadowFarDistance;

                            pageShadows.Controls["ShadowFarDistanceValue"].Text =
                                ((int)GameEngineApp.ShadowFarDistance).ToString();

                            ColorValue color = GameEngineApp.ShadowColor;
                            ((ScrollBar)pageShadows.Controls["ShadowColor"]).Value =
                                (color.Red + color.Green + color.Blue) / 3;
                        }
                    };
                }

                //ShadowPSSMSplitFactor1
                scrollBar = (ScrollBar)pageShadows.Controls["ShadowPSSMSplitFactor1"];
                scrollBar.Value = GameEngineApp.ShadowPSSMSplitFactors[0];
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameEngineApp.ShadowPSSMSplitFactors = new Vec2(
                        sender.Value, GameEngineApp.ShadowPSSMSplitFactors[1]);
                    pageShadows.Controls["ShadowPSSMSplitFactor1Value"].Text =
                        (GameEngineApp.ShadowPSSMSplitFactors[0].ToString("F2")).ToString();
                };
                pageShadows.Controls["ShadowPSSMSplitFactor1Value"].Text =
                    (GameEngineApp.ShadowPSSMSplitFactors[0].ToString("F2")).ToString();

                //ShadowPSSMSplitFactor2
                scrollBar = (ScrollBar)pageShadows.Controls["ShadowPSSMSplitFactor2"];
                scrollBar.Value = GameEngineApp.ShadowPSSMSplitFactors[1];
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameEngineApp.ShadowPSSMSplitFactors = new Vec2(
                        GameEngineApp.ShadowPSSMSplitFactors[0], sender.Value);
                    pageShadows.Controls["ShadowPSSMSplitFactor2Value"].Text =
                        (GameEngineApp.ShadowPSSMSplitFactors[1].ToString("F2")).ToString();
                };
                pageShadows.Controls["ShadowPSSMSplitFactor2Value"].Text =
                    (GameEngineApp.ShadowPSSMSplitFactors[1].ToString("F2")).ToString();

                //ShadowFarDistance
                scrollBar = (ScrollBar)pageShadows.Controls["ShadowFarDistance"];
                scrollBar.Value = GameEngineApp.ShadowFarDistance;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameEngineApp.ShadowFarDistance = sender.Value;
                    pageShadows.Controls["ShadowFarDistanceValue"].Text =
                        ((int)GameEngineApp.ShadowFarDistance).ToString();
                };
                pageShadows.Controls["ShadowFarDistanceValue"].Text =
                    ((int)GameEngineApp.ShadowFarDistance).ToString();

                //ShadowColor
                scrollBar = (ScrollBar)pageShadows.Controls["ShadowColor"];
                scrollBar.Value = (GameEngineApp.ShadowColor.Red + GameEngineApp.ShadowColor.Green +
                    GameEngineApp.ShadowColor.Blue) / 3;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    float color = sender.Value;
                    GameEngineApp.ShadowColor = new ColorValue(color, color, color, color);
                };

                //ShadowDirectionalLightTextureSize
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowDirectionalLightTextureSize"];
                    for (int value = 256, index = 0; value <= 8192; value *= 2, index++)
                    {
                        comboBox.Items.Add(value);
                        if (GameEngineApp.ShadowDirectionalLightTextureSize == value)
                            comboBox.SelectedIndex = index;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        GameEngineApp.ShadowDirectionalLightTextureSize = (int)sender.SelectedItem;
                    };
                }

                ////ShadowDirectionalLightMaxTextureCount
                //{
                //   comboBox = (EComboBox)pageVideo.Controls[ "ShadowDirectionalLightMaxTextureCount" ];
                //   for( int n = 0; n < 3; n++ )
                //   {
                //      int count = n + 1;
                //      comboBox.Items.Add( count );
                //      if( count == GameEngineApp.ShadowDirectionalLightMaxTextureCount )
                //         comboBox.SelectedIndex = n;
                //   }
                //   comboBox.SelectedIndexChange += delegate( EComboBox sender )
                //   {
                //      GameEngineApp.ShadowDirectionalLightMaxTextureCount = (int)sender.SelectedItem;
                //   };
                //}

                //ShadowSpotLightTextureSize
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowSpotLightTextureSize"];
                    for (int value = 256, index = 0; value <= 8192; value *= 2, index++)
                    {
                        comboBox.Items.Add(value);
                        if (GameEngineApp.ShadowSpotLightTextureSize == value)
                            comboBox.SelectedIndex = index;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        GameEngineApp.ShadowSpotLightTextureSize = (int)sender.SelectedItem;
                    };
                }

                //ShadowSpotLightMaxTextureCount
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowSpotLightMaxTextureCount"];
                    for (int n = 0; n < 3; n++)
                    {
                        int count = n + 1;
                        comboBox.Items.Add(count);
                        if (count == GameEngineApp.ShadowSpotLightMaxTextureCount)
                            comboBox.SelectedIndex = n;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        GameEngineApp.ShadowSpotLightMaxTextureCount = (int)sender.SelectedItem;
                    };
                }

                //ShadowPointLightTextureSize
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowPointLightTextureSize"];
                    for (int value = 256, index = 0; value <= 8192; value *= 2, index++)
                    {
                        comboBox.Items.Add(value);
                        if (GameEngineApp.ShadowPointLightTextureSize == value)
                            comboBox.SelectedIndex = index;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        GameEngineApp.ShadowPointLightTextureSize = (int)sender.SelectedItem;
                    };
                }

                //ShadowPointLightMaxTextureCount
                {
                    comboBox = (ComboBox)pageShadows.Controls["ShadowPointLightMaxTextureCount"];
                    for (int n = 0; n < 3; n++)
                    {
                        int count = n + 1;
                        comboBox.Items.Add(count);
                        if (count == GameEngineApp.ShadowPointLightMaxTextureCount)
                            comboBox.SelectedIndex = n;
                    }
                    comboBox.SelectedIndexChange += delegate(ComboBox sender)
                    {
                        GameEngineApp.ShadowPointLightMaxTextureCount = (int)sender.SelectedItem;
                    };
                }
            }

            //pageSound
            {
                bool enabled = SoundWorld.Instance.DriverName != "NULL";

                Control pageSound = tabControl.Controls["Sound"];

                //soundVolumeCheckBox
                scrollBar = (ScrollBar)pageSound.Controls["SoundVolume"];
                scrollBar.Value = enabled ? GameEngineApp.SoundVolume : 0;
                scrollBar.Enable = enabled;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameEngineApp.SoundVolume = sender.Value;
                };

                //musicVolumeCheckBox
                scrollBar = (ScrollBar)pageSound.Controls["MusicVolume"];
                scrollBar.Value = enabled ? GameEngineApp.MusicVolume : 0;
                scrollBar.Enable = enabled;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameEngineApp.MusicVolume = sender.Value;
                };
            }

            #region pageControls

            //pageControls
            {
                Control pageControls = tabControl.Controls["Controls"];

                cmbBoxDevice = (ComboBox)pageControls.Controls["InputDevices"];
                axisfilterbutton = ((Button)pageControls.Controls["ChangeAxisfilter"]);
                btnAddBinding = ((Button)pageControls.Controls["btnAddBinding"]);
                btnAddBinding.Click += delegate(Button sender)
                {
                    //if (btnAddBinding == null)
                    //    return;
                    CreateAdd_Custom_Control_Dialogue();
                    //Control commandwindow = new CommandBindingWindow();
                    //GameEngineApp.Instance.ControlManager.Controls.Add(commandwindow);
                    //commandwindow.
                };

                controlsList = pageControls.Controls["ListControls"] as ListBox;

                //MouseHSensitivity
                scrollBar = (ScrollBar)pageControls.Controls["MouseHSensitivity"];
                scrollBar.Value = GameControlsManager.Instance.MouseSensitivity.X;
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    Vec2 value = GameControlsManager.Instance.MouseSensitivity;
                    value.X = sender.Value;
                    GameControlsManager.Instance.MouseSensitivity = value;

                    #region warcryZoom

                    //For Zoom
                    GameControlsManager.Instance.BaseSensitivity = value;

                    #endregion warcryZoom
                };

                //MouseVSensitivity
                scrollBar = (ScrollBar)pageControls.Controls["MouseVSensitivity"];
                scrollBar.Value = Math.Abs(GameControlsManager.Instance.MouseSensitivity.Y);
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    Vec2 value = GameControlsManager.Instance.MouseSensitivity;
                    bool invert = ((CheckBox)pageControls.Controls["MouseVInvert"]).Checked;
                    value.Y = sender.Value * (invert ? -1.0f : 1.0f);
                    GameControlsManager.Instance.MouseSensitivity = value;

                    #region warcryZoom

                    //For Zoom
                    GameControlsManager.Instance.BaseSensitivity = value;

                    #endregion warcryZoom
                };

                //MouseVInvert
                checkBox = (CheckBox)pageControls.Controls["MouseVInvert"];
                checkBox.Checked = GameControlsManager.Instance.MouseSensitivity.Y < 0;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    Vec2 value = GameControlsManager.Instance.MouseSensitivity;
                    value.Y =
                        ((ScrollBar)pageControls.Controls["MouseVSensitivity"]).Value *
                        (sender.Checked ? -1.0f : 1.0f);
                    GameControlsManager.Instance.MouseSensitivity = value;

                    #region warcryZoom

                    //For Zoom
                    GameControlsManager.Instance.BaseSensitivity = value;

                    #endregion warcryZoom
                };

                //AlwaysRun
                checkBox = (CheckBox)pageControls.Controls["AlwaysRun"];
                checkBox.Checked = GameControlsManager.Instance.AlwaysRun;
                checkBox.CheckedChange += delegate(CheckBox sender)
                {
                    GameControlsManager.Instance.AlwaysRun = sender.Checked;
                };

                //Devices
                comboBoxInputDevices = cmbBoxDevice;
                cmbBoxDevice.Items.Add("Keyboard/Mouse");
                if (InputDeviceManager.Instance != null)
                {
                    foreach (InputDevice device in InputDeviceManager.Instance.Devices)
                        cmbBoxDevice.Items.Add(device);
                }
                cmbBoxDevice.SelectedIndex = 0;
                UpdateBindedInputControlsListBox();

                cmbBoxDevice.SelectedIndexChange += delegate(ComboBox sender)
                {
                    if (axisfilterbutton != null)
                        axisfilterbutton.Enable = false;

                    if (controlsList.SelectedIndex != -1)
                        controlsList.SelectedIndex = 0;

                    UpdateBindedInputControlsListBox();
                };

                scrollBar = (ScrollBar)pageControls.Controls["DeadzoneVScroll"];
                scrollBar.Value = GameControlsManager.Instance.DeadZone;
                textBox = (TextBox)pageControls.Controls["DeadZoneValue"];
                textBox.Text = GameControlsManager.Instance.DeadZone.ToString();
                scrollBar.ValueChange += delegate(ScrollBar sender)
                {
                    GameControlsManager.Instance.DeadZone = sender.Value;
                    textBox.Text = sender.Value.ToString();
                };

                ((Button)pageControls.Controls["ControlSave"]).Click += delegate(Button sender)
                {
                    GameControlsManager.Instance.SaveCustomConfig();
                };

                Control message = window.Controls["TabControl/Controls/ListControls/Message"];
                controlsList.ItemMouseDoubleClick += delegate(object sender, ListBox.ItemMouseEventArgs e)
                {
                    message.Text = "Type the new key (ESC to cancel)";
                    message.ColorMultiplier = new ColorValue(1, 0, 0);
                    Controls.Add(new KeyListener(sender));
                };

                controlsList.SelectedIndexChange += delegate(ListBox sender)
                {
                    if (controlsList.SelectedItem == null || axisfilterbutton == null || !(controlsList.SelectedItem is GameControlsManager.SystemJoystickValue))
                        return;

                    var item = controlsList.SelectedItem as GameControlsManager.SystemJoystickValue;

                    axisfilterbutton.Enable = item.Type == GameControlsManager.SystemJoystickValue.Types.Axis || item.Type == GameControlsManager.SystemJoystickValue.Types.Slider;
                };

                ((Button)pageControls.Controls["Default"]).Click += delegate(Button sender)
                {
                    GameControlsManager.Instance.ResetKeyMouseSettings();
                    GameControlsManager.Instance.ResetJoystickSettings();
                    UpdateBindedInputControlsListBox();
                };

                //Incin -- change Axis Filter alone
                axisfilterbutton.Click += delegate(Button sender)
                {
                    if (controlsList.SelectedItem == null || axisfilterbutton == null || !(controlsList.SelectedItem is GameControlsManager.SystemJoystickValue))
                        return;

                    var item = controlsList.SelectedItem as GameControlsManager.SystemJoystickValue;

                    if (item.Type == GameControlsManager.SystemJoystickValue.Types.Axis || item.Type == GameControlsManager.SystemJoystickValue.Types.Slider)
                        CreateAxisFilterDialogue();
                };
                axisfilterbutton.Enable = false;

                {
                    if (controlsList.SelectedItem == null || axisfilterbutton == null || !(controlsList.SelectedItem is GameControlsManager.SystemJoystickValue))
                        return;

                    var item = controlsList.SelectedItem as GameControlsManager.SystemJoystickValue;

                    if (item.Type == GameControlsManager.SystemJoystickValue.Types.Axis || item.Type == GameControlsManager.SystemJoystickValue.Types.Slider)
                        CreateAxisFilterDialogue();
                };

                //Controls
                UpdateBindedInputControlsListBox();

                if (controlsList.SelectedIndex != -1)
                    controlsList.SelectedIndex = 0;
            }

            #endregion pageControls

            //pageLanguage
            {
                Control pageLanguage = tabControl.Controls["Language"];

                //Language
                {
                    comboBox = (ComboBox)pageLanguage.Controls["Language"];

                    List<string> languages = new List<string>();
                    {
                        languages.Add("Autodetect");
                        string[] directories = VirtualDirectory.GetDirectories(LanguageManager.LanguagesDirectory, "*.*",
                            SearchOption.TopDirectoryOnly);
                        foreach (string directory in directories)
                            languages.Add(Path.GetFileNameWithoutExtension(directory));
                    }

                    string language = "Autodetect";
                    if (engineConfigBlock != null)
                    {
                        TextBlock localizationBlock = engineConfigBlock.FindChild("Localization");
                        if (localizationBlock != null && localizationBlock.IsAttributeExist("language"))
                            language = localizationBlock.GetAttribute("language");
                    }

                    foreach (string lang in languages)
                    {
                        string displayName = lang;
                        if (lang == "Autodetect")
                            displayName = Translate(lang);

                        comboBox.Items.Add(new ComboBoxItem(lang, displayName));
                        if (string.Compare(language, lang, true) == 0)
                            comboBox.SelectedIndex = comboBox.Items.Count - 1;
                    }
                    if (comboBox.SelectedIndex == -1)
                        comboBox.SelectedIndex = 0;

                    comboBox.SelectedIndexChange += comboBoxLanguage_SelectedIndexChange;
                }

                //LanguageRestart
                {
                    Button button = (Button)pageLanguage.Controls["LanguageRestart"];
                    button.Click += buttonLanguageRestart_Click;
                }
            }

            tabControl.SelectedIndex = lastPageIndex;
            tabControl.SelectedIndexChange += tabControl_SelectedIndexChange;
            UpdatePageButtonsState();
        }

        private void UpdateShadowControlsEnable()
        {
            Control pageVideo = window.Controls["TabControl"].Controls["Shadows"];

            bool textureShadows =
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapLow ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapMedium ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapHigh ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapLowPSSM ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapMediumPSSM ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapHighPSSM;

            bool pssm = GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapLowPSSM ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapMediumPSSM ||
                GameEngineApp.ShadowTechnique == ShadowTechniques.ShadowmapHighPSSM;

            bool allowShadowColor = GameEngineApp.ShadowTechnique != ShadowTechniques.None;

            pageVideo.Controls["ShadowColor"].Enable =
                !GameEngineApp.ShadowUseMapSettings && allowShadowColor;

            pageVideo.Controls["ShadowPSSMSplitFactor1"].Enable =
                !GameEngineApp.ShadowUseMapSettings && pssm;

            pageVideo.Controls["ShadowPSSMSplitFactor2"].Enable =
                !GameEngineApp.ShadowUseMapSettings && pssm;

            pageVideo.Controls["ShadowFarDistance"].Enable =
                !GameEngineApp.ShadowUseMapSettings &&
                GameEngineApp.ShadowTechnique != ShadowTechniques.None;

            pageVideo.Controls["ShadowDirectionalLightTextureSize"].Enable = textureShadows;
            //pageVideo.Controls[ "ShadowDirectionalLightMaxTextureCount" ].Enable = textureShadows;
            pageVideo.Controls["ShadowSpotLightTextureSize"].Enable = textureShadows;
            pageVideo.Controls["ShadowSpotLightMaxTextureCount"].Enable = textureShadows;
            pageVideo.Controls["ShadowPointLightTextureSize"].Enable = textureShadows;
            pageVideo.Controls["ShadowPointLightMaxTextureCount"].Enable = textureShadows;
        }

        private void ChangeVideoMode()
        {
            Vec2I size;
            {
                size = EngineApp.Instance.VideoMode;

                if (comboBoxResolution.SelectedIndex != -1)
                {
                    string s = (string)(comboBoxResolution).SelectedItem;
                    s = s.Replace("x", " ");
                    size = Vec2I.Parse(s);
                }
            }

            EngineApp.Instance.VideoMode = size;
        }

        public void UpdateBindedInputControlsListBox()
        {
            Control pageControls = window.Controls["TabControl"].Controls["Controls"];
            controlsList = pageControls.Controls["ListControls"] as ListBox;

            controlsList.Items.Clear();

            var device = Devices.Keyboard;
            if (comboBoxInputDevices.SelectedIndex != 0)
            {
                if (comboBoxInputDevices.SelectedItem.ToString().ToLower().Contains("xbox360"))
                {
                    device = Devices.Joystick_Xbox360;
                }
                else
                {
                    device = Devices.Joystick;
                }
            }

            foreach (GameControlsManager.GameControlItem item in GameControlsManager.Instance.Items)
            {
                if (device == Devices.Keyboard)
                {
                    if (item.BindedKeyboardMouseValues.Count > 0)
                    {
                        foreach (var key in item.BindedKeyboardMouseValues)
                        {
                            controlsList.Items.Add(key);
                        }
                    }
                    else
                    {
                        controlsList.Items.Add(new GameControlsManager.SystemKeyboardMouseValue() { Parent = item, Unbound = true });
                    }
                }
                else
                {
                    var unbound = true;
                    foreach (var key in item.bindedJoystickValues)
                    {
                        if (device == Devices.Joystick_Xbox360)
                        {
                            if (key.Type == GameControlsManager.SystemJoystickValue.Types.Button)
                            {
                                if (key.Button.ToString().ToLower().Contains("xbox360"))
                                {
                                    controlsList.Items.Add(key);
                                    unbound = false;
                                }
                            }
                            else if (key.Type == GameControlsManager.SystemJoystickValue.Types.Axis)
                            {
                                if (key.Axis.ToString().ToLower().Contains("xbox360"))
                                {
                                    controlsList.Items.Add(key);
                                    unbound = false;
                                }
                            }
                        }
                        else
                        {
                            if (key.Type == GameControlsManager.SystemJoystickValue.Types.Button)
                            {
                                if (key.Button.ToString().ToLower().Contains("xbox360"))
                                {
                                    continue;
                                }
                            }
                            else if (key.Type == GameControlsManager.SystemJoystickValue.Types.Axis)
                            {
                                if (key.Axis.ToString().ToLower().Contains("xbox360"))
                                {
                                    continue;
                                }
                            }
                            controlsList.Items.Add(key);
                            unbound = false;
                        }
                    }
                    if (unbound)
                    {
                        controlsList.Items.Add(new GameControlsManager.SystemJoystickValue() { Parent = item, Unbound = true });
                    }
                }
            }
        }

        private void CreateAxisFilterDialogue()
        {
            ComboBox comboBox;
            Control AxisFilterControl = ControlDeclarationManager.Instance.CreateControl(@"GUI\AxisFilter.gui");
            AxisFilterControl.MouseCover = true;
            Controls.Add(AxisFilterControl);

            comboBox = (ComboBox)AxisFilterControl.Controls["cmbAxisFilter"];
            //foreach( var value in Enum.GetValues( typeof( JoystickAxisFilters ) ) )
            //{
            //    comboBox.Items.Add( value );
            //}
            comboBox.Items.Add(JoystickAxisFilters.GreaterZero);
            comboBox.Items.Add(JoystickAxisFilters.LessZero);
            comboBox.Items.Add(JoystickAxisFilters.OnlyGreaterZero);
            comboBox.Items.Add(JoystickAxisFilters.OnlyLessZero);

            int index = (int)(controlsList.SelectedItem as GameControlsManager.SystemJoystickValue).AxisFilter;
            index = index == 4 ? 0 : index;//hack to get the good index
            comboBox.SelectedIndex = index;

            ((Button)AxisFilterControl.Controls["OK"]).Click += delegate(Button sender)
            {
                AxisFilterControl.SetShouldDetach();
                axisfilterselection = (JoystickAxisFilters)comboBox.SelectedItem;
                (controlsList.SelectedItem as GameControlsManager.SystemJoystickValue).AxisFilter = axisfilterselection;
                controlsList.ItemButtons[controlsList.SelectedIndex].Text = controlsList.SelectedItem.ToString();
                axisfilterselection = JoystickAxisFilters.DEADZONE; //set back to Deadzone
            };

            ((Button)AxisFilterControl.Controls["Cancel"]).Click += delegate(Button sender)
            {
                AxisFilterControl.SetShouldDetach();
            };
        }

        /// <summary>
        /// CreateAdd_Custom_Control_Dialogue() support code
        /// </summary>
        //device
        //control
        private TabControl MainOptionsTabControl;

        private TabControl tabJoystickControlOptions;
        private Button[] pageControlsButtons = new Button[3];
        private Button[] pagejoystickButtons = new Button[3];
        private static string message = "Nothing Selected";
        private static int lastPageIndex2;
        private static int lastPageIndex3;
        //float[] device_strength = new float[1024]; //combo and list controls count; //all devices

        private void UpdatetabJoystickControlOptionsPageButtonsState()
        {
            for (int n = 0; n < pageControlsButtons.Length; n++)
            {
                Button button = pagejoystickButtons[n];
                button.Active = tabJoystickControlOptions.SelectedIndex == n;
            }
        }

        private void tabJoystickControlOptions_SelectedIndexChange(TabControl sender)
        {
            lastPageIndex3 = sender.SelectedIndex;
            UpdatetabJoystickControlOptionsPageButtonsState();
        }

        private void joystickPageTabButtons_Click(Button sender)
        {
            int index = Array.IndexOf(pagejoystickButtons, sender);
            tabJoystickControlOptions.SelectedIndex = index;
        }

        private void UpdateMainOptionsPageButtonsState()
        {
            for (int n = 0; n < pageControlsButtons.Length; n++)
            {
                Button button = pageControlsButtons[n];
                button.Active = MainOptionsTabControl.SelectedIndex == n;
            }
        }

        private void MainOptionsTabControl_SelectedIndexChange(TabControl sender)
        {
            lastPageIndex2 = sender.SelectedIndex;
            UpdateMainOptionsPageButtonsState();
        }

        private void pageControlsButton_Click(Button sender)
        {
            int index = Array.IndexOf(pageControlsButtons, sender);
            MainOptionsTabControl.SelectedIndex = index;
        }

        private Control messageBox;

        private void OKOnlyButton_Click(object sender)
        {
            messageBox.SetShouldDetach();
        }

        private void CreateMessageBox(string message)
        {
            messageBox = ControlDeclarationManager.Instance.CreateControl(@"GUI\Confirm.gui");
            Controls.Add(messageBox);
            messageBox.Controls["MessageBox"].Text = message;
            messageBox.MouseCover = true;
            ((Button)messageBox.Controls["Cancel"]).Visible = false;
            ((Button)messageBox.Controls["Clear"]).Visible = false;
            ((Button)messageBox.Controls["OK"]).Click += OKOnlyButton_Click;
        }

        #region SelectedItimList

        //Enums for selected dropdowns to save back and compare
        //List Device Selected Command Binds
        //No duplicates of originals in ControlsList
        //bindlist
        //{
        private Devices devicetype_selected;                                // = (Devices)cmbDeviceType.SelectedItem;

        private GameControlKeys command_selected;                           // = (Control)lstCommand.SelectedItem;

        // group Keyboard
        private EKeys lstKeyboardButtonChoices_selected;                    // = (EKeys)lstKeyboardButtonChoices.SelectedItem;

        // group Mouse
        private EMouseButtons cmbMouseButtonChoices_selected;               // = (EMouseButtons)cmbMouseButtonChoices.SelectedItem;

        private MouseScroll cmbMouseScrollChoices_selected;                 //= (MouseScroll)cmbMouseScrollChoices.SelectedItem;

        //group joystick //slider options
        private JoystickSliders cmbSliderChoices_selected;                  //= (JoystickSliders)cmbSliderChoices.SelectedItem;

        private JoystickSliderAxes cmbSliderAxisChoices_selected;           //= (JoystickSliderAxes)cmbSliderAxisChoices.Sel ControlsLisrtectedItem;
        private JoystickAxisFilters cmbSliderAxisFilterChoices_selected;    // = (JoystickAxisFilters)cmbSliderAxisFilterChoices.SelectedItem;

        //axis filter
        private JoystickSliderAxes cmbAxisChoices_selected;                 // = (JoystickSliderAxes)cmbAxisChoices.SelectedItem;

        private JoystickAxisFilters cmbAxisFilterChoices_selected;          // = (JoystickAxisFilters)cmbAxisFilterChoices.SelectedItem;

        //buttons
        private JoystickButtons lstJoyButtonChoices_selected;               // = (JoystickButtons)lstJoyButtonChoices.SelectedItem;

        //}
        private float Strength_selected = 1.00f;                             //always max strength

        private float old_strength = 1.00f;                                    //last scrollbar strength
        //int nothing_selected    = 0;

        #endregion SelectedItimList

        ///<summary>
        /// ____MADE IN THE +
        /// USA + France +
        /// ___ InCin filtering by combo boxes and listboxes --- all me
        ///void CreateAdd_Custom_Control_Dialogue()
        ///populate all drop downs and comboboxes
        ///filter by device or device Type
        ///hide unneeded info or unhide pages
        /// </summary>
        private void CreateAdd_Custom_Control_Dialogue()
        {
            //load custom binding window
            Control Add_Custom_Control = ControlDeclarationManager.Instance.CreateControl(@"GUI\Add_Custom_Control.gui");

            Add_Custom_Control.MouseCover = true;

            //Add
            Controls.Add(Add_Custom_Control);

            #region AddCustomControl.Gui

            #region MainControls

            ComboBox cmbDeviceType;
            cmbDeviceType = (ComboBox)Add_Custom_Control.Controls["cmbDeviceType"];
            //cmbDeviceType.Items.Add("Nothing_Selected"); //using enum Nothing_Selected
            foreach (var value in Enum.GetValues(typeof(Devices)))
            {
                //if(!(value.ToString().Contains(Devices.GetServices.ToString())) && !(value.ToString().Contains(Devices.All_Devices.ToString()))) //exclude for internal use

                cmbDeviceType.Items.Add(value);
            }
            cmbDeviceType.SelectedIndex = 0;
            ComboBox cmbDevice;
            cmbDevice = (ComboBox)Add_Custom_Control.Controls["cmbDevice"];
            cmbDevice.Items.Add("Nothing_Selected");
            cmbDevice.Items.Add("Keyboard"); //unhandled object as multiple devices
            cmbDevice.Items.Add("Mouse");   //unhandled object as a multiple devices
            if (InputDeviceManager.Instance != null)
            {
                foreach (InputDevice devicename in InputDeviceManager.Instance.Devices)
                    cmbDevice.Items.Add(devicename); //handled objects
                //filter
            }
            cmbDevice.SelectedIndex = 0;

            Control cntrlCommands = (Control)Add_Custom_Control.Controls["cntrlCommands"];
            cntrlCommands.Visible = false;
            //Commands Available
            ListBox lstCommand;
            lstCommand = (ListBox)Add_Custom_Control.Controls["cntrlCommands"].Controls["lstCommand"];
            lstCommand.Items.Add("Nothing_Selected");
            lstCommand.SelectedIndex = 0;
            foreach (var value in Enum.GetValues(typeof(GameControlKeys)))
            {
                lstCommand.Items.Add(value);
            }

            //control Tab Controls
            //TabControl MainOptionsTabControl;
            MainOptionsTabControl = (TabControl)Add_Custom_Control.Controls["MainOptionsTabControl"];
            MainOptionsTabControl.SelectedIndexChange += MainOptionsTabControl_SelectedIndexChange;
            MainOptionsTabControl.Visible = true;

            MainOptionsTabControl.Visible = false; //hide all subcontrols for now

            pageControlsButtons[0] = (Button)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["btnMouseOptions"];
            pageControlsButtons[1] = (Button)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["btnKeyboardOptions"];
            pageControlsButtons[2] = (Button)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["btnJoystickOptions"];
            foreach (Button pageButton in pageControlsButtons)
            {
                pageButton.Click += new Button.ClickDelegate(pageControlsButton_Click);
            }

            pageControlsButtons[0].PerformClick();

            TextBox lblMessage = (TextBox)Add_Custom_Control.Controls["lblMessage"]; // holds message of selected items
            lblMessage.Text = "Nothing_Selected";

            ScrollBar scrlSelectedStrength;
            scrlSelectedStrength = (ScrollBar)MainOptionsTabControl.Controls["scrlSelectedStrength"];

            //incin -- not updating the strength atm, gotta figure this out..
            scrlSelectedStrength.ValueChange += delegate(ScrollBar sender)
            {
                while (sender.Value != Strength_selected)
                {
                    Strength_selected = sender.Value;
                }
                if (message != null)
                {
                    if (message.Contains(" Strength: "))
                    {
                        message = message.Replace(" Strength: " + old_strength.ToString("F2"), " Strength: " + Strength_selected.ToString("F2"));
                        lblMessage.Text = message;
                    }
                }
                old_strength = Strength_selected;
            };

            #endregion MainControls

            #region pageMouseoptions

            Control pageMouseOptions;
            pageMouseOptions = (Control)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["pageMouseOptions"];
            //Page visible= false || true;

            #region MouseTabControls

            //MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions
            TabControl MouseTabControl = (TabControl)pageMouseOptions.Controls["MouseTabControl"];

            ComboBox cmbMouseButtonChoices;
            cmbMouseButtonChoices = (ComboBox)MouseTabControl.Controls["pageMouseButtonOptions"].Controls["cmbMouseButtonChoices"];
            cmbMouseButtonChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(EMouseButtons)))
            {
                cmbMouseButtonChoices.Items.Add(value);
            }

            cmbMouseButtonChoices.SelectedIndex = 0;

            //MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions.cmbMouseScrollChoices
            ComboBox cmbMouseScrollChoices;
            cmbMouseScrollChoices = (ComboBox)MouseTabControl.Controls["pageMouseScrollOptions"].Controls["cmbMouseScrollChoices"];
            cmbMouseScrollChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(MouseScroll)))
            {
                cmbMouseScrollChoices.Items.Add(value);
            }
            cmbMouseScrollChoices.SelectedIndex = 0;

            #endregion MouseTabControls

            #endregion pageMouseoptions

            #region pageKeyboardOptions

            Control pageKeyboardOptions = (Control)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["pageKeyboardOptions"];
            //visible false true?

            ListBox lstKeyboardButtonChoices;
            lstKeyboardButtonChoices = (ListBox)pageKeyboardOptions.Controls["lstKeyboardButtonChoices"];
            lstKeyboardButtonChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(EKeys)))
            {
                lstKeyboardButtonChoices.Items.Add(value);
            }
            lstKeyboardButtonChoices.SelectedIndex = 0;
            //if (lstKeyboardButtonChoices.ItemButtons["lstKeyboardButtonChoices"]Text.Contains("Nothing_Selected") != null)
            //    lstKeyboardButtonChoices.SelectedIndex = -1;

            #endregion pageKeyboardOptions

            //MainOptionsTabControl.pageJoystickOptions
            //tabJoystickControlOptions

            #region pageJoystickOptions

            Control pageJoystickOptions;
            pageJoystickOptions = (Control)Add_Custom_Control.Controls["MainOptionsTabControl"].Controls["pageJoystickOptions"];//.Controls["tabJoystickControlOptions"];

            tabJoystickControlOptions = (TabControl)pageJoystickOptions.Controls["tabJoystickControlOptions"];
            tabJoystickControlOptions.SelectedIndexChange += tabJoystickControlOptions_SelectedIndexChange;

            pagejoystickButtons[0] = (Button)tabJoystickControlOptions.Controls["btnSliderOptions"];
            pagejoystickButtons[1] = (Button)tabJoystickControlOptions.Controls["btnAxisOptions"];
            pagejoystickButtons[2] = (Button)tabJoystickControlOptions.Controls["btnButtonOptions"];

            foreach (Button pageButton in pagejoystickButtons)
            {
                pageButton.Click += new Button.ClickDelegate(joystickPageTabButtons_Click);
            }

            #region pageSliderOptions

            Control pageSliderOptions = tabJoystickControlOptions.Controls["pageSliderOptions"];

            #region cmbSliderChoices

            ComboBox cmbSliderChoices;

            cmbSliderChoices = (ComboBox)pageSliderOptions.Controls["cmbSliderChoices"];
            cmbSliderChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickSliders)))
            {
                cmbSliderChoices.Items.Add(value);
            }
            cmbSliderChoices.SelectedIndex = 0;

            ComboBox cmbSliderAxisChoices;
            cmbSliderAxisChoices = (ComboBox)pageSliderOptions.Controls["cmbSliderAxisChoices"];
            cmbSliderAxisChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickSliderAxes)))
            {
                cmbSliderAxisChoices.Items.Add(value);
            }
            cmbSliderAxisChoices.SelectedIndex = 0;

            ComboBox cmbSliderAxisFilterChoices;
            cmbSliderAxisFilterChoices = (ComboBox)pageSliderOptions.Controls["cmbSliderAxisFilterChoices"];
            cmbSliderAxisFilterChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickAxisFilters)))
            {
                cmbSliderAxisFilterChoices.Items.Add(value);
            }
            cmbSliderAxisFilterChoices.SelectedIndex = 0;

            #endregion cmbSliderChoices

            #endregion pageSliderOptions

            #region pageAxisOptions

            Control pageAxisOptions = tabJoystickControlOptions.Controls["pageAxisOptions"];
            ComboBox cmbAxisChoices;
            cmbAxisChoices = (ComboBox)pageAxisOptions.Controls["cmbAxisChoices"];
            cmbAxisChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickSliderAxes)))
            {
                cmbAxisChoices.Items.Add(value);
            }
            cmbAxisChoices.SelectedIndex = 0;

            ComboBox cmbAxisFilterChoices;
            cmbAxisFilterChoices = (ComboBox)pageAxisOptions.Controls["cmbAxisFilterChoices"];
            cmbAxisFilterChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickAxisFilters)))
            {
                cmbAxisFilterChoices.Items.Add(value);
            }
            cmbAxisFilterChoices.SelectedIndex = 0;

            Control pageJoystickButtonOptions = tabJoystickControlOptions.Controls["pageJoystickButtonOptions"];
            ListBox lstJoyButtonChoices = (ListBox)pageJoystickButtonOptions.Controls["lstJoyButtonChoices"];
            lstJoyButtonChoices.Items.Add("Nothing_Selected");
            foreach (var value in Enum.GetValues(typeof(JoystickButtons)))
            {
                lstJoyButtonChoices.Items.Add(value);
            }
            lstJoyButtonChoices.SelectedIndex = 0;

            #endregion pageAxisOptions

            #endregion pageJoystickOptions

            //___ InCin filtering by combo boxes and listboxes --- all me
            // setting all indexes at 0 -- tracks "Nothing_Selected"

            #region IndexChanged

            //if(page visible == enabled){
            cmbMouseButtonChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0) //Nothing_Selected
                    return;

                if (sender.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " MouseButton: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " MouseButton: " + sender.SelectedItem.ToString();
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                lblMessage.Text = message;
            };

            cmbMouseScrollChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0)// Nothing_Selected
                    return;

                if (sender.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " MouseScroll: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    lstKeyboardButtonChoices.Visible = false;
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " MouseScroll: " + sender.SelectedItem.ToString();
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    lstKeyboardButtonChoices.Visible = true;
                }
                lblMessage.Text = message;
            };

            lstKeyboardButtonChoices.SelectedIndexChange += delegate(ListBox sender)
        {
            message = null;
            //MainOptionsTabControl.Visible = false;
            if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0)
                return;

            if (sender.SelectedIndex == 0) // Nothing_Selected
            {
                //Nothing_Selected
                //message = "Device: " + cmbDevice.SelectedItem.ToString();
                message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                message += " Command: " + lstCommand.SelectedItem.ToString();
                message += " KeyboardButton: Nothing_Selected";
                message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
            }
            else
            {
                //message = "Device: " + cmbDevice.SelectedItem.ToString();
                message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                message += " Command: " + lstCommand.SelectedItem.ToString();
                message += " KeyboardButton: " + sender.SelectedItem.ToString();
                message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
            }
            lblMessage.Text = message;
        };

            //}
            //if(page visible == enabled)
            cmbSliderChoices.SelectedIndexChange += delegate(ComboBox sender)
        {
            message = null;
            //MainOptionsTabControl.Visible = false;
            if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0)  // Nothing_Selected
            {
                return;
            }

            if (sender.SelectedIndex == 0)
            {
                //message = "Device: " + cmbDevice.SelectedItem.ToString();
                message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                message += " Command: " + lstCommand.SelectedItem.ToString();
                message += " Slider: Nothing_Selected";
                message += " Axis: Nothing_Selected";
                message += " AxisFilter: Nothing_Selected";
                message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                //cmbSliderChoices.Visible = false;
            }
            else //if( sender.SelectedIndex != 0)
            {
                //message = "Device: " + cmbDevice.SelectedItem.ToString();
                message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                message += " Command: " + lstCommand.SelectedItem.ToString();
                message += " Slider: " + sender.SelectedItem.ToString(); //sender
                message += " Axis: Nothing_Selected";
                message += " AxisFilter: Nothing_Selected";
                //event update
                message += " Strength: " + scrlSelectedStrength.Value.ToString("F2"); //recursive scroll change
                //cmbSliderAxisChoices.Visible = true;
            }
            lblMessage.Text = message;
        };

            cmbSliderAxisChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0 || cmbSliderChoices.SelectedIndex == 0) // Nothing_Selected
                    return;

                if (sender.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Slider: " + cmbSliderChoices.SelectedItem.ToString();
                    message += " Axis: Nothing_Selected";
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    cmbSliderAxisFilterChoices.Visible = false;
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Slider: " + cmbSliderChoices.SelectedItem.ToString();
                    message += " Axis: " + sender.SelectedItem.ToString();
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    cmbSliderAxisFilterChoices.Visible = true;
                }
                lblMessage.Text = message;
            };

            cmbSliderAxisFilterChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0 || cmbSliderChoices.SelectedIndex == 0 || cmbSliderAxisChoices.SelectedIndex == 0)// Nothing_Selected
                    return;

                if (sender.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Slider: " + cmbSliderChoices.SelectedItem.ToString();
                    message += " Axis: " + cmbSliderAxisChoices.SelectedItem.ToString();
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Slider: " + cmbSliderChoices.SelectedItem;
                    message += " Axis: " + cmbSliderChoices.SelectedItem.ToString();
                    message += " AxisFilter: " + sender.SelectedItem.ToString();
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                lblMessage.Text = message;
            };
            //}
            //if(page visible == enabled){
            cmbAxisChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0)// Nothing_Selected
                {
                    cntrlCommands.Visible = false;
                    //MainOptionsTabControl.Visible = false;
                    return;
                }

                if (cmbAxisChoices.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Axis: Nothing_Selected";
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Axis: " + sender.SelectedItem.ToString();
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                lblMessage.Text = message;
            };

            cmbAxisFilterChoices.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0 || cmbAxisChoices.SelectedIndex == 0)
                {
                    //MainOptionsTabControl.Visible = false;
                    return;
                }
                if (cmbAxisFilterChoices.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Axis: " + cmbAxisChoices.SelectedItem.ToString();
                    message += " AxisFilter: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Axis: " + cmbAxisChoices.SelectedItem.ToString();
                    message += " AxisFilter: " + sender.SelectedItem.ToString();
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                lblMessage.Text = message;
            };

            lstJoyButtonChoices.SelectedIndexChange += delegate(ListBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0 || lstCommand.SelectedIndex == 0)
                {
                    //MainOptionsTabControl.Visible = false;

                    return;
                }
                if (lstJoyButtonChoices.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " JoystickButton: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " JoystickButton: " + sender.SelectedItem.ToString();
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                }
                lblMessage.Text = message;
            };
            //}

            lstCommand.SelectedIndexChange += delegate(ListBox sender)
            {
                message = null;
                if (cmbDeviceType.SelectedIndex == 0)
                    return;

                if (sender.SelectedIndex == 0)
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + "Nothing_Selected";
                    message += " Bind: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    MainOptionsTabControl.Visible = false;
                }
                else
                {
                    //message = "Device: " + cmbDevice.SelectedItem.ToString();
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: " + lstCommand.SelectedItem.ToString();
                    message += " Bind: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    MainOptionsTabControl.Visible = true;
                }
                lblMessage.Text = message;
            };

            #region comboDeviceType

            // Filters down through each TabControl hides what isn't needed
            // MainOptionsTabControl
            // MouseTabControl
            //  Primary choices

            cmbDeviceType.SelectedIndexChange += delegate(ComboBox sender)
            {
                message = null;
                if (sender.SelectedIndex == -1)
                {
                    cntrlCommands.Visible = false;
                    MainOptionsTabControl.Visible = false;
                    return;
                }
                if (sender.SelectedIndex != 0)
                {
                    Devices devicetype = devicetype_selected = (Devices)sender.SelectedItem;
                    cntrlCommands.Visible = true;
                    MainOptionsTabControl.Visible = false;
                    switch (devicetype)
                    {
                        case Devices.Mouse:
                            {
                                pageControlsButton_Click(pageControlsButtons[0]);
                                pageControlsButtons[0].Enable = true;
                                pageControlsButtons[1].Enable = false;
                                pageControlsButtons[2].Enable = false;
                                break;
                            }
                        case Devices.Keyboard:
                            {
                                pageControlsButton_Click(pageControlsButtons[1]);
                                pageControlsButtons[0].Enable = false;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = false;
                                break;
                            }

                        case Devices.Joystick:
                            {
                                pageControlsButton_Click(pageControlsButtons[2]);
                                pageControlsButtons[0].Enable = false;
                                pageControlsButtons[1].Enable = false;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }

                        case Devices.Joystick_Xbox360:
                            {
                                pageControlsButton_Click(pageControlsButtons[2]);
                                pageControlsButtons[0].Enable = false;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }
                        case Devices.Joystick_Playstation:
                            {
                                pageControlsButton_Click(pageControlsButtons[2]);
                                pageControlsButtons[0].Enable = false;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }
                        case Devices.Joystick_WII:
                            {
                                pageControlsButton_Click(pageControlsButtons[2]);
                                pageControlsButtons[0].Enable = false;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }
                        //case Devices.Custom_Audio:
                        //    {
                        //        pageControlsButton_Click(pageControlsButtons[2]);
                        //        pageControlsButtons[0].Enable = true;
                        //        pageControlsButtons[1].Enable = true;
                        //        pageControlsButtons[2].Enable = true;
                        //        break;
                        //    }
                        case Devices.Custom:
                            {
                                pageControlsButton_Click(pageControlsButtons[0]);
                                pageControlsButtons[0].Enable = true;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }
                        default:
                            {
                                pageControlsButton_Click(pageControlsButtons[0]);
                                pageControlsButtons[0].Enable = true;
                                pageControlsButtons[1].Enable = true;
                                pageControlsButtons[2].Enable = true;
                                break;
                            }
                    }
                    message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                    message += " Command: Nothing_Selected";
                    message += " Bind: Nothing_Selected";
                    message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");

                    if (lstCommand.SelectedIndex != 0)
                    {
                        message = "DeviceType: " + cmbDeviceType.SelectedItem.ToString();
                        message += " Command: " + lstCommand.SelectedItem.ToString();
                        message += " Bind: Nothing_Selected";
                        message += " Strength: " + scrlSelectedStrength.Value.ToString("F2");
                    }
                }
                else
                {
                    message = " Nothing_Selected";
                    cntrlCommands.Visible = false;
                    MainOptionsTabControl.Visible = false;
                    pageControlsButtons[0].Enable = true;
                    pageControlsButtons[1].Enable = true;
                    pageControlsButtons[2].Enable = true;
                }
                lblMessage.Text = message;
            };

            #endregion comboDeviceType

            #region comboDevice

            //cmbDevice.SelectedIndexChange += delegate(ComboBox sender)
            //{
            //    //if (sender.SelectedIndex != -1)
            //    //{
            //    //    InputDevice inputdevices = (InputDevice)sender.SelectedItem;
            //    //    //Set deviceType
            //    //    // continue
            //    //}
            //    //lblMessage.Text = message;
            //    ;
            //    //hidden for now
            //};

            #endregion comboDevice

            #endregion IndexChanged

            #region ButtonOK

            ((Button)Add_Custom_Control.Controls["buttonOK"]).Click += delegate(Button sender)
            {
                if (cmbDeviceType.SelectedIndex == 0)
                {
                    CreateMessageBox("No Device selected, select a device first!");
                    return;
                }
                else //(cmbDeviceType.SelectedIndex != 0)
                    devicetype_selected = (Devices)cmbDeviceType.SelectedItem;

                if (lstCommand.SelectedIndex == 0)
                {
                    CreateMessageBox("No Command binding selected, select a Command first!");
                    return;
                }
                else // if (lstCommand.SelectedIndex != 0)
                    command_selected = (GameControlKeys)lstCommand.SelectedItem;

                float currentstrength = float.Parse(Strength_selected.ToString("F2"));
                //if (pageControlsButtons[0].Enable == true || pageControlsButtons[1].Enable == true || pageControlsButtons[2].Enable == true)

                switch (devicetype_selected) //devicetype
                {
                    case Devices.Nothing_Selected:
                        {
                            CreateMessageBox("No Device selected, select a device first!");
                            return;
                        }
                    //break;
                    case Devices.Keyboard:
                        {
                            // group Keyboard
                            if (lstKeyboardButtonChoices.SelectedIndex != 0)
                            {
                                GameControlsManager.SystemKeyboardMouseValue key;
                                lstKeyboardButtonChoices_selected = (EKeys)lstKeyboardButtonChoices.SelectedItem;
                                Strength_selected = currentstrength;

                                if (!GameControlsManager.Instance.IsAlreadyBinded((EKeys)lstKeyboardButtonChoices_selected/*, currentstrength*/, out key) && key == null)
                                {
                                    //GameControlsManager.Instance.GetItemByControlKey(command_selected).BindedKeyboardMouseValues.Add(new GameControlsManager.SystemKeyboardMouseValue(lstKeyboardButtonChoices_selected)); //devicetype_selected));
                                    var controlItem = GameControlsManager.Instance.GetItemByControlKey(command_selected);
                                    var newSystemValue = new GameControlsManager.SystemKeyboardMouseValue(lstKeyboardButtonChoices_selected);
                                    newSystemValue.Parent = controlItem;
                                    newSystemValue.Strength = Strength_selected;
                                    controlItem.BindedKeyboardMouseValues.Add(newSystemValue);
                                    UpdateBindedInputControlsListBox();
                                }
                                else
                                {
                                    message = "Key" + cmbMouseButtonChoices_selected + ": Command: " + command_selected + " Strength: 1 is already bound";
                                    lblMessage.Text = message;
                                    CreateMessageBox(message);
                                    message = "";
                                    cmbMouseButtonChoices_selected = 0;
                                    lstCommand.SelectedIndex = 0;
                                    return;
                                }
                            }
                            else
                            {
                                lstKeyboardButtonChoices.SelectedIndex = 0;
                                lstKeyboardButtonChoices_selected = (EKeys)0;
                            }

                            break;
                        }
                    case Devices.Mouse:
                        {
                            // group Mouse

                            if (cmbMouseButtonChoices.SelectedIndex != 0)
                            {
                                ////check for existing matching binds before saving
                                GameControlsManager.SystemKeyboardMouseValue key;
                                cmbMouseButtonChoices_selected = (EMouseButtons)cmbMouseButtonChoices.SelectedItem;

                                if (GameControlsManager.Instance.IsAlreadyBinded((EMouseButtons)cmbMouseButtonChoices_selected, out key)) //currentstrength,
                                {
                                    message = "Mouse Button" + cmbMouseButtonChoices_selected + ": Command: " + command_selected + " is already bound"; //" Strength: " + currentstrength +
                                    //new MessageBoxWindow( text, "Key already bound. Clearing , try again", null );
                                    CreateMessageBox(message);
                                    cmbMouseButtonChoices_selected = 0;
                                    cmbMouseButtonChoices.SelectedIndex = 0;
                                    lstCommand.SelectedIndex = 0;
                                    message = "";
                                    return;
                                }

                                var cmbMouseButtonChoicesControlItem = GameControlsManager.Instance.GetItemByControlKey(command_selected);
                                var cmbMouseButtonChoicesSystemValue = new GameControlsManager.SystemKeyboardMouseValue(cmbMouseButtonChoices_selected, currentstrength);
                                cmbMouseButtonChoicesSystemValue.Parent = cmbMouseButtonChoicesControlItem;
                                cmbMouseButtonChoicesControlItem.BindedKeyboardMouseValues.Add(cmbMouseButtonChoicesSystemValue);
                                UpdateBindedInputControlsListBox();
                            }
                            else if (cmbMouseScrollChoices.SelectedIndex != 0)
                            {
                                GameControlsManager.SystemKeyboardMouseValue key;
                                cmbMouseScrollChoices_selected = (MouseScroll)cmbMouseScrollChoices.SelectedItem;

                                if (GameControlsManager.Instance.IsAlreadyBinded((MouseScroll)cmbMouseScrollChoices_selected, currentstrength, out key))
                                {
                                    message = "Mouse Button" + cmbMouseScrollChoices_selected + ": Command: " + command_selected + " is already bound"; //+ " Strength: " + currentstrength
                                    //new MessageBoxWindow( text, "Key already bound. Clearing , try again", null );
                                    CreateMessageBox(message);
                                    message = "";
                                    return;
                                }

                                var cmbMouseScrollChoicescontrolItem = GameControlsManager.Instance.GetItemByControlKey(command_selected);
                                var cmbMouseScrollChoicesSystemValue = new GameControlsManager.SystemKeyboardMouseValue(cmbMouseScrollChoices_selected, currentstrength);
                                cmbMouseScrollChoicesSystemValue.Parent = cmbMouseScrollChoicescontrolItem;
                                //cmbMouseScrollChoicesSystemValue.Strength = Strength_selected;
                                cmbMouseScrollChoicescontrolItem.BindedKeyboardMouseValues.Add(cmbMouseScrollChoicesSystemValue);
                                UpdateBindedInputControlsListBox();
                            }
                            else
                            {
                                cmbMouseButtonChoices.SelectedIndex = 0;
                                cmbMouseButtonChoices_selected = (EMouseButtons)0;
                                cmbMouseScrollChoices.SelectedIndex = 0;
                                cmbMouseScrollChoices_selected = (MouseScroll)0;
                            }
                            break;
                        }

                    case Devices.Joystick:
                    case Devices.Joystick_Playstation:
                    case Devices.Joystick_WII:
                    case Devices.Joystick_Xbox360:
                        {
                            //Filter by tab
                            //group joystick //slider options

                            if (cmbSliderChoices.SelectedIndex != 0 && cmbSliderAxisChoices.SelectedIndex != 0 && cmbSliderAxisFilterChoices.SelectedIndex != 0)
                            {
                                //if (cmbSliderChoices.SelectedIndex != 0)
                                cmbSliderChoices_selected = (JoystickSliders)cmbSliderChoices.SelectedItem;
                                //if (cmbSliderAxisChoices.SelectedIndex != 0)
                                cmbSliderAxisChoices_selected = (JoystickSliderAxes)cmbSliderAxisChoices.SelectedItem;
                                //if (cmbSliderAxisFilterChoices.SelectedIndex != 0)
                                cmbSliderAxisFilterChoices_selected = (JoystickAxisFilters)cmbSliderAxisFilterChoices.SelectedItem;

                                var cmbSliderChoicescontrolItem = GameControlsManager.Instance.GetItemByControlKey(command_selected);
                                var cmbSliderChoicesSystemValue = new GameControlsManager.SystemJoystickValue(cmbSliderChoices_selected, cmbSliderAxisChoices_selected, cmbSliderAxisFilterChoices_selected, currentstrength);
                                cmbSliderChoicesSystemValue.Parent = cmbSliderChoicescontrolItem;
                                cmbSliderChoicescontrolItem.bindedJoystickValues.Add(cmbSliderChoicesSystemValue);
                                UpdateBindedInputControlsListBox();
                                break;
                            }
                            else
                            {
                                cmbSliderChoices.SelectedIndex = 0;
                                cmbSliderChoices_selected = (JoystickSliders)0;
                                cmbSliderAxisChoices.SelectedIndex = 0;
                                cmbSliderAxisChoices_selected = (JoystickSliderAxes)0;
                                cmbSliderAxisFilterChoices.SelectedIndex = 0;
                                cmbSliderAxisFilterChoices_selected = (JoystickAxisFilters)0;
                            }

                            //axis filter
                            if (cmbAxisChoices.SelectedIndex != 0)
                                cmbAxisChoices_selected = (JoystickSliderAxes)cmbAxisChoices.SelectedItem;
                            else
                            {
                                cmbAxisChoices.SelectedIndex = 0;
                                cmbAxisChoices_selected = (JoystickSliderAxes)0;
                            }

                            if (cmbAxisFilterChoices.SelectedIndex != 0)
                                cmbAxisFilterChoices_selected = (JoystickAxisFilters)cmbAxisFilterChoices.SelectedItem;
                            else
                            {
                                cmbAxisFilterChoices.SelectedIndex = 0;
                                cmbAxisFilterChoices_selected = (JoystickAxisFilters)0;
                            }

                            //buttons
                            if (lstJoyButtonChoices.SelectedIndex != 0)
                                lstJoyButtonChoices_selected = (JoystickButtons)lstJoyButtonChoices.SelectedItem;
                            else
                            {
                                lstJoyButtonChoices.SelectedIndex = 0;
                                lstJoyButtonChoices_selected = (JoystickButtons)0;
                            }
                            message = "Missing Fields" + cmbAxisFilterChoices_selected;
                            break;
                        }

                    case Devices.Custom:
                        {
                            break;
                        }
                    //case Devices.Custom_Audio:
                    //    {
                    //        break;
                    //    }
                }
                //GameControlsManager.Instance.GetItemByControlKey(command_selected).BindedKeyboardMouseValues.Add(new GameControlsManager.SystemKeyboardMouseValue(lstKeyboardButtonChoices_selected));
                //save bind

                //GameControlsManager.Instance.SaveCustomConfig();
                //GameControlsManager.Instance.LoadCustomConfig();
                Add_Custom_Control.SetShouldDetach();
            };

            #endregion ButtonOK

            ((Button)Add_Custom_Control.Controls["buttonReset"]).Click += delegate(Button sender)
            {
                cmbDevice.SelectedIndex = 0;
                cmbDeviceType.SelectedIndex = 0;
                lstCommand.SelectedIndex = 0;
                cmbMouseButtonChoices.SelectedIndex = 0;
                cmbMouseScrollChoices.SelectedIndex = 0;
                lstKeyboardButtonChoices.SelectedIndex = 0;
                cmbSliderChoices.SelectedIndex = 0;
                cmbSliderAxisChoices.SelectedIndex = 0;
                cmbSliderAxisFilterChoices.SelectedIndex = 0;
                cmbAxisChoices.SelectedIndex = 0;
                cmbAxisFilterChoices.SelectedIndex = 0;
                lstJoyButtonChoices.SelectedIndex = 0;
                cntrlCommands.Visible = false;
                scrlSelectedStrength.Value = 1f;
                MainOptionsTabControl.Visible = false;
                message = " Nothing_Selected";
                lblMessage.Text = message;
            };

            ((Button)Add_Custom_Control.Controls["buttonCancel"]).Click += delegate(Button sender)
            {
                Add_Custom_Control.SetShouldDetach();
            };

            #endregion AddCustomControl.Gui

            MainOptionsTabControl.SelectedIndex = lastPageIndex2;
            UpdateMainOptionsPageButtonsState();
        }

        protected override void OnControlDetach(Control control)
        {
            if (control as KeyListener != null && window.Controls.Count > 0)
            {
                Control message = window.Controls["TabControl/Controls/ListControls/Message"];
                message.Text = " Double click to change the key";
                message.ColorMultiplier = new ColorValue(1, 1, 1);
                UpdateBindedInputControlsListBox();
            }

            if (control as CommandBindingWindow != null && window.Controls.Count > 0)
            {
                UpdateBindedInputControlsListBox();
            }

            base.OnControlDetach(control);
        }

        protected override bool OnMouseWheel(int delta)
        {
            if (base.OnMouseWheel(delta))
                return true;
            return false;
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

        protected override void OnDetach()
        {
            GameControlsManager.Instance.SaveCustomConfig();
            base.OnDetach();
        }

        private void tabControl_SelectedIndexChange(TabControl sender)
        {
            lastPageIndex = sender.SelectedIndex;
            UpdatePageButtonsState();
        }

        private TextBlock LoadEngineConfig()
        {
            string fileName = VirtualFileSystem.GetRealPathByVirtual("user:Configs/Engine.config");
            string error;
            return TextBlockUtils.LoadFromRealFile(fileName, out error);
        }

        private void SaveEngineConfig(TextBlock engineConfigBlock)
        {
            string fileName = VirtualFileSystem.GetRealPathByVirtual("user:Configs/Engine.config");
            try
            {
                string directoryName = Path.GetDirectoryName(fileName);
                if (directoryName != "" && !Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    writer.Write(engineConfigBlock.DumpToString());
                }
            }
            catch (Exception e)
            {
                Log.Warning("Unable to save file \"{0}\". {1}", fileName, e.Message);
            }
        }

        private void comboBoxRenderTechnique_SelectedIndexChange(ComboBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock rendererBlock = engineConfigBlock.FindChild("Renderer");
            if (rendererBlock == null)
                rendererBlock = engineConfigBlock.AddChild("Renderer");
            ComboBoxItem item = (ComboBoxItem)sender.SelectedItem;
            rendererBlock.SetAttribute("renderTechnique", item.Identifier);
            SaveEngineConfig(engineConfigBlock);

            EnableVideoRestartButton();
        }

        private void comboBoxFiltering_SelectedIndexChange(ComboBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock rendererBlock = engineConfigBlock.FindChild("Renderer");
            if (rendererBlock == null)
                rendererBlock = engineConfigBlock.AddChild("Renderer");
            ComboBoxItem item = (ComboBoxItem)sender.SelectedItem;
            rendererBlock.SetAttribute("filtering", item.Identifier);
            SaveEngineConfig(engineConfigBlock);

            EnableVideoRestartButton();
        }

        private void checkBoxDepthBufferAccess_CheckedChange(CheckBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock rendererBlock = engineConfigBlock.FindChild("Renderer");
            if (rendererBlock == null)
                rendererBlock = engineConfigBlock.AddChild("Renderer");
            rendererBlock.SetAttribute("depthBufferAccess", sender.Checked.ToString());
            SaveEngineConfig(engineConfigBlock);

            EnableVideoRestartButton();

            UpdateComboBoxAntialiasing();
        }

        private void comboBoxAntialiasing_SelectedIndexChange(ComboBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock rendererBlock = engineConfigBlock.FindChild("Renderer");
            if (rendererBlock == null)
                rendererBlock = engineConfigBlock.AddChild("Renderer");
            if (comboBoxAntialiasing.SelectedIndex != -1)
            {
                ComboBoxItem item = (ComboBoxItem)comboBoxAntialiasing.SelectedItem;
                rendererBlock.SetAttribute("fullSceneAntialiasing", item.Identifier);
            }
            else
                rendererBlock.DeleteAttribute("fullSceneAntialiasing");
            SaveEngineConfig(engineConfigBlock);

            EnableVideoRestartButton();
        }

        private void UpdateComboBoxAntialiasing()
        {
            int lastSelectedIndex = comboBoxAntialiasing.SelectedIndex;

            comboBoxAntialiasing.Items.Clear();

            comboBoxAntialiasing.Items.Add(new ComboBoxItem("RecommendedSetting", Translate("Recommended setting")));
            comboBoxAntialiasing.Items.Add(new ComboBoxItem("0", Translate("No")));
            if (!checkBoxDepthBufferAccess.Checked)
            {
                comboBoxAntialiasing.Items.Add(new ComboBoxItem("2", "2"));
                comboBoxAntialiasing.Items.Add(new ComboBoxItem("4", "4"));
                comboBoxAntialiasing.Items.Add(new ComboBoxItem("6", "6"));
                comboBoxAntialiasing.Items.Add(new ComboBoxItem("8", "8"));
            }
            comboBoxAntialiasing.Items.Add(new ComboBoxItem("FXAA", Translate("Fast Approximate AA (FXAA)")));

            if (lastSelectedIndex >= 0 && lastSelectedIndex <= 1)
                comboBoxAntialiasing.SelectedIndex = lastSelectedIndex;
            else
                comboBoxAntialiasing.SelectedIndex = 0;
        }

        private void checkBoxVerticalSync_CheckedChange(CheckBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock rendererBlock = engineConfigBlock.FindChild("Renderer");
            if (rendererBlock == null)
                rendererBlock = engineConfigBlock.AddChild("Renderer");
            rendererBlock.SetAttribute("verticalSync", sender.Checked.ToString());
            SaveEngineConfig(engineConfigBlock);

            EnableVideoRestartButton();
        }

        private void EnableVideoRestartButton()
        {
            Control pageVideo = window.Controls["TabControl"].Controls["Video"];
            Button button = (Button)pageVideo.Controls["VideoRestart"];
            button.Enable = true;
        }

        private void buttonVideoRestart_Click(Button sender)
        {
            Program.needRestartApplication = true;
            EngineApp.Instance.SetNeedExit();
        }

        private void comboBoxLanguage_SelectedIndexChange(ComboBox sender)
        {
            //update Engine.config
            TextBlock engineConfigBlock = LoadEngineConfig();
            if (engineConfigBlock == null)
                engineConfigBlock = new TextBlock();
            TextBlock localizationBlock = engineConfigBlock.FindChild("Localization");
            if (localizationBlock == null)
                localizationBlock = engineConfigBlock.AddChild("Localization");
            ComboBoxItem item = (ComboBoxItem)sender.SelectedItem;
            localizationBlock.SetAttribute("language", item.Identifier);
            SaveEngineConfig(engineConfigBlock);

            EnableLanguageRestartButton();
        }

        private void EnableLanguageRestartButton()
        {
            Control pageLanguage = window.Controls["TabControl"].Controls["Language"];
            Button button = (Button)pageLanguage.Controls["LanguageRestart"];
            button.Enable = true;
        }

        private void buttonLanguageRestart_Click(Button sender)
        {
            Program.needRestartApplication = true;
            EngineApp.Instance.SetNeedExit();
        }

        private string Translate(string text)
        {
            return LanguageManager.Instance.Translate("UISystem", text);
        }

        private void pageButton_Click(Button sender)
        {
            int index = Array.IndexOf(pageButtons, sender);
            tabControl.SelectedIndex = index;
        }

        private void UpdatePageButtonsState()
        {
            for (int n = 0; n < pageButtons.Length; n++)
            {
                Button button = pageButtons[n];
                button.Active = tabControl.SelectedIndex == n;
            }
        }
    }
}