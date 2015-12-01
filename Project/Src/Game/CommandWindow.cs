using System;
using Engine;
using Engine.UISystem;
using ProjectCommon;

namespace Game
{
    internal class CommandBindingWindow : OptionsWindow
    {
        private static CommandBindingWindow instance;

        public static CommandBindingWindow Instance
        {
            get
            {
                if (instance == null)
                    instance = new CommandBindingWindow();
                return instance;
            }
        }

        //Enums for selected dropdowns to save back and compare
        //List Device Selected Command Binds
        //No duplicates of originals in ControlsList
        //bindlist

        #region SelectedItimList

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

        private float Strength_selected = 1.00f;                             //always max strength
        private float old_strength = 1.00f;                                    //last scrollbar strength
        //int nothing_selected    = 0;

        #endregion SelectedItimList

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

        //private void GetParentLoadingControl( Control parent)
        //{
        //    if (parent != null)
        //        this.Parent = parent;
        //    else
        //        this.Parent = null;
        //}

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

        private void Set_SelectedItems(Control control)
        {
            float strength = 1;
            if (devicetype_selected == 0)
            {
                return;
            }

            foreach (Control cmbControls in Controls)
            {
                if (cmbControls is ComboBox)
                {
                    (cmbControls as ComboBox).SelectedIndex = 0;
                }
                else if (cmbControls is ListBox)
                {
                    (cmbControls as ListBox).SelectedIndex = 0;
                }

                switch (devicetype_selected)
                {
                    case Devices.Mouse:
                        {
                            old_strength = currentstrength;
                            break;
                        }
                    case Devices.Keyboard:
                        {
                            break;
                        }
                    case Devices.Joystick:
                        {
                            break;
                        }
                    case Devices.Custom:
                        {
                            break;
                        }
                }
            }
        }

        protected override void OnAttach()
        {
            CreateAdd_Custom_Control_Dialogue();
            base.OnAttach();
        }

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
            Add_Custom_Control.TopMost = true;
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

            // server  selected.music == "<station>"
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
                                    //Game.OptionsWindow.UpdateBindedInputControlsListBox();
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
                                //UpdateBindedInputControlsListBox();
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
                                //UpdateBindedInputControlsListBox();
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
                                //UpdateBindedInputControlsListBox();
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

                            if (cmbAxisFilterChoices.SelectedIndex == 0)
                                cmbAxisFilterChoices_selected = (JoystickAxisFilters)cmbAxisFilterChoices.SelectedItem;
                            else
                            {
                                cmbAxisFilterChoices.SelectedIndex = 0;
                                cmbAxisFilterChoices_selected = (JoystickAxisFilters)0;
                            }

                            //buttons
                            if (lstJoyButtonChoices.SelectedIndex == 0)
                                lstJoyButtonChoices_selected = (JoystickButtons)lstJoyButtonChoices.SelectedItem;
                            else
                            {
                                lstJoyButtonChoices.SelectedIndex = 0;
                                lstJoyButtonChoices_selected = (JoystickButtons)0;
                            }
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

        public float currentstrength { get; set; }
    }
}