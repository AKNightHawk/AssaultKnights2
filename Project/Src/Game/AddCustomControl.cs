using System.Collections.Generic;
using System.Text;
using System.IO;
using Engine;
using Engine.FileSystem;
using Engine.UISystem;
using Engine.Renderer;
using Engine.MathEx;
using Engine.MapSystem;
using Engine.Utils;
using Engine.SoundSystem;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
 
	public class AddCustomBindingControl : Control
	{
		Control window;
        window = new window("@GUI\\Add_Custom_Control.gui"); 
		#region Add_Custom_Control.gui 
		control "GUI\\Controls\\DefaultWindow.gui"
		{
			#region WindowProperties
			.position = Parent -0.01504517 -0.009175584
			.horizontalAlign = Center
			.verticalAlign = Center
			.size = ScaleByResolution 870.6875 653.9063
			.text = Add New Custom Bindings
			.mouseCover = True
			#endregion WindowProperties
			#region buttonOK
			control "GUI\\Controls\\DefaultButton.gui"
			{
				#region ButtonOk.Properties
				.position = ScaleByResolution 20 590
				.size = ScaleByResolution 378.5313 46.40005
				.name = buttonOK
				.text = OK  
				#endregion ButtonOk.Properties      
			}
			#endregion  buttonOK
			#region buttonCancel
			control "GUI\\Controls\\DefaultButton.gui"
			{
			    #region buttonCancel.Properties
		        .position = ScaleByResolution 438.75 590
		        .size = ScaleByResolution 399 46.40005
		        .name = buttonCancel
		        .text = Cancel
		        #endregion buttonCancel.Properties
			}
			#endregion buttonCancel
			#region cmbDeviceLabel
			control TextBox
			{
				.position = ScaleByResolution 5 39.99999
				.size = Parent 0.9762805 0.02717235
				.text = Device
                .name = cmbDeviceLabel //fixme
			}
			#endregion cmbDeviceLabel
			#region  cmbDevice
			control "GUI\\Controls\\DefaultComboBox.gui"
			{
				position = ScaleByResolution 20 60.00002
				size = ScaleByResolution 818.75 30
				name = cmbDevice
			}
			#endregion cmbDevice
			#region cntrlCommands
			control Control
			{
				#region cntrlCommands.Properties
				position = Parent 0.02153471 0.1529272
				size = Parent 0.9526237 0.1130226
				name = cntrlCommands
				#endregion cntrlCommands.Properties
				#region cntrlCommands.cmbCommand.Label
				control TextBox
				{
					position = ScaleByResolution 0 -3.433228E-05
					size = Parent 0.9749004 0.2712297
					text = Bind to Command
				}
				#endregion cntrlCommands.cmbCommand.Label
				#region cntrlCommands.cmbCommand
				control "GUI\\Controls\\DefaultComboBox.gui"
				{
					position = ScaleByResolution 0 29.99993
					size = ScaleByResolution 515 30.00002
					name = cmbCommand
				}
				#endregion cntrlCommands.cmbCommand

			}
			#endregion cntrlCommands
			#region  MainOptionsTabControl
			control TabControl
			{
				#region MainOptionsTabControl.Properties
				pageButtonsOffset = ScaleByResolution 170 0
				position = ScaleByResolution 18.75 170
				size = Parent 0.9687831 0.4800478
				name = MainOptionsTabControl
				#endregion MainOptionsTabControl.Properties
				#region pageButton
				pageButton "GUI\\Controls\\DefaultButton.gui"
				{
					#region MainOptionsTabControl.pageButton.Properties
					position = ScaleByResolution 0 -140
					size = ScaleByResolution 170 30
					visible = False
					enable = False
					#endregion MainOptionsTabControl.pageButton.Properties
				}
				#endregion pageButton 
				#region MainOptionsTabControl.pageMouseOptions
				page0 Control
				{
					#region MainOptionsTabControl.pageMouseOptions.Properties
					position = ScaleByResolution 0 0
					name = pageMouseOptions
					text = Mouse Options
					visible = False
					#endregion MainOptionsTabControl.pageMouseOptions
					#region MainOptionsTabControl.pageMouseOptions.MouseTabControl
					control TabControl
					{
						#region MainOptionsTabControl.pageMouseOptions.MouseTabControl.Properties
						pageButtonsOffset = ScaleByResolution 180 0
						position = ScaleByResolution 10 40
						size = Parent 0.6131252 0.7667576
						name = MouseTabControl
						#endregion MainOptionsTabControl.pageMouseOptions.MouseTabControl.Properties
						#region MainOptionsTabControl.pageMouseOptions.pageButton
						pageButton "GUI\\Controls\\DefaultButton.gui"
						{
                            #region MainOptionsTabControl.pageMouseOptions.pageButton.Properties
							position = ScaleByResolution -18.75 -100
							size = ScaleByResolution 180 30
							visible = False
							enable = False
                            #endregion MainOptionsTabControl.pageMouseOptions.pageButton.Properties
						}
						#endregion MainOptionsTabControl.pageMouseOptions.pageButton
						#region MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions
						page0 Control
						{
							#region MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions.Properties
							position = ScaleByResolution 0 19.99999
							size = Parent 0.9972729 0.9875532
							name = pageMouseButtonOptions
							text = Button Options
							#endregion MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions.Properties
							#region MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions.cmbMouseButtonChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution 10 110
								size = ScaleByResolution 487.5 30
								name = cmbMouseButtonChoices
							}
							#endregion MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions.cmbMouseButtonChoices
							control TextBox
							{
								position = ScaleByResolution 0 80
								size = Parent 0.909191 0.1292239
								text = Mouse Button Options
							}
						}
						#endregion MainOptionsTabControl.MouseTabControl.pageMouseButtonOptions
						#region MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions
						page1 Control
						{
							#region MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions.Properties
							position = ScaleByResolution 0 -30
							size = Parent 0.9972729 0.9875532
							name = pageMouseScrollOptions
							text = MouseScroll Options
							visible = False
							#endregion MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions.Properties
							#region MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions.cmbMouseScrollChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution 20 160
								size = ScaleByResolution 467.5 30
								name = cmbMouseScrollChoices
							}
							#endregion MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions.cmbMouseScrollChoices
							control TextBox
							{
								position = ScaleByResolution 10 130
								size = Parent 0.9285797 0.1292239
								text = Mouse Scroll Choices
							}
						} 
						#endregion MainOptionsTabControl.MouseTabControl.pageMouseScrollOptions
					}
					#endregion MainOptionsTabControl.pageMouseOptions.MouseTabControl
				}
				#endregion MainOptionsTabControl.pageMouseOptions

				#region MainOptionsTabControl.pageKeyboardOptions
				page1 Control
				{
					#region MainOptionsTabControl.pageKeyboardOptions.Properties
					position = ScaleByResolution 0 30
					size = Parent 0.9971486 0.7029014
					name = pageKeyboardOptions
					text = Keyboard Options
					visible = False
					#endregion MainOptionsTabControl.pageKeyboardOptions.Properties
					#region Label
					control TextBox
					{
						position = ScaleByResolution 40 30
						size = Parent 0.5602126 0.1073477
						text = KeyBoard Button Choices
					}
					#endregion
					#region MainOptionsTabControl.pageKeyboardOptions.cmbKeyboardButtonChoices
					control "GUI\\Controls\\DefaultComboBox.gui"
					{
						textIfNoSelection = "<Nothing Selected>"
						position = ScaleByResolution 40 60.00002
						size = ScaleByResolution 467.5 30
						name = cmbKeyboardButtonChoices
					}
					#endregion MainOptionsTabControl.pageKeyboardOptions.cmbKeyboardButtonChoices

				}
				#endregion MainOptionsTabControl.pageKeyboardOptions
				#region MainOptionsTabControl.pageJoystickOptions
				page2 Control
				{
					#region MainOptionsTabControl.pageJoystickOptions.Properties
					position = ScaleByResolution 0 0
					name = pageJoystickOptions
					text = Joystick Options
					#endregion MainOptionsTabControl.pageJoystickOptions.Properties
                    #region MainOptionsTabControl.pageJoystickOptions.tabctrlJoystick
					control TabControl
					{
						#region MainOptionsTabControl.pageJoystickOptions.tbctrlJoystick.Properties
						pageButtonsOffset = ScaleByResolution 170 0
						position = ScaleByResolution 0 40
						size = Parent 0.9602932 0.939539
						name = tbctrlJoystick  //fixme
						#endregion MainOptionsTabControl.pageJoystickOptions.tbctrlJoystick.Properties
						#region MainOptionsTabControl.pageJoystickOptions.pageButton
						pageButton "GUI\\Controls\\DefaultButton.gui"
						{
							position = ScaleByResolution -18.75 -100
							size = ScaleByResolution 170 30
							visible = False
							enable = False
						}
						#endregion MainOptionsTabControl.pageJoystickOptions.pageButton
						#region MainOptionsTabControl.pageJoystickOptions.pageSliderOptions
						page0 Control
						{
							position = ScaleByResolution -18.75 -3.814697E-06
							size = ScaleByResolution 828.75 350
							name = pageSliderOptions
							text = Slider Options
							#region Labels
							control TextBox
							{
								position = ScaleByResolution 150 39.99998
								size = Parent 0.2830471 0.05775774
								name = txtSliderChoices
								text = Slider Choices
								lockEditorResizing = True
							}
							control TextBox
							{
								position = ScaleByResolution 10 110
								size = Parent 0.6200978 0.05775774
								name = txtAxisChoices
								text = Axis Choices
							}
							control TextBox
							{
								position = ScaleByResolution 10 190
								size = Parent 0.6200978 0.05775774
								name = txtAxisFilterChoices
								text = Axis Filter Choices
							}
							#endregion Labels                    
							#region MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderAxisFilterChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution 10 220
								size = ScaleByResolution 509.9999 30
								name = cmbSliderAxisFilterChoices
							}
							#endregion MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderAxisFilterChoices
							#region MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderAxisFilterChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution 10 140
								size = ScaleByResolution 510 30
								name = cmbSliderAxisChoices
							}
							#endregion MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderAxisFilterChoices
							#region MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution 10 59.99997
								size = ScaleByResolution 510 30
								name = cmbSliderChoices
							}
							#endregion MainOptionsTabControl.pageJoystickOptions.pageSliderOptions.cmbSliderChoices
						}
						#endregion MainOptionsTabControl.pageAxiskOptions.pageSliderOptions
						#region MainOptionsTabControl.pageJoystickOptions.pageAxisOptions
						page1 Control
						{
							position = ScaleByResolution 0 30.00002
							size = ScaleByResolution 847.5 370
							name = pageAxisOptions
							text = Axis Options
							visible = False
							#region Labels
							control TextBox
							{
								position = ScaleByResolution -10 29.99997
								size = Parent 0.612623 0.05775774
								name = TextAxisChoices
								text = Axis Choices
							}
							control TextBox
							{
								position = ScaleByResolution -10 99.99997
								size = Parent 0.612623 0.05775774
								name = TextAxisFilterChoices
								text = Axis Filter Choices
							}
							#endregion Labels
							#region MainOptionsTabControl.pageJoystickOptions.pageAxisOptionscmbAxisFilterChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution -10 130
								size = ScaleByResolution 519.9999 30
								name = cmbAxisFilterChoices
							}
							#endregion MainOptionsTabControl.pageJoystickOptions.pageAxisOptions.cmbAxisFilterChoices
							#region MainOptionsTabControl.pageJoystickOptions.pageAxisOptions.cmbAxisChoices
							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution -10 49.99995
								size = ScaleByResolution 520 30
								name = cmbAxisChoices
							}
							#endregion MainOptionsTabControl.pageJoystickOptions.pageAxisOptions.cmbAxisChoices
						}
						#endregion MainOptionsTabControl.pageJoystickOptions.pageAxisOptions
						#region MainOptionsTabControl.pageJoystickOptions.pageJoystickButtonOptions
						page2 Control
						{
							position = ScaleByResolution 0 0
							name = pageJoystickButtonOptions //fixme
							text = Button Options
							visible = False
							#region Labels
							control TextBox
							{
								position = ScaleByResolution -10 60
								size = Parent 0.6397281 0.05775774
								name = TextAxisChoices
								text = Button Choices
							}
							#endregion Labels

							control "GUI\\Controls\\DefaultComboBox.gui"
							{
								textIfNoSelection = "<Nothing Selected>"
								position = ScaleByResolution -10 80
								size = ScaleByResolution 520 30
								name = cmbJoyButtonChoices
							}
						}
						#endregion MainOptionsTabControl.pageJoystickOptions.pageJoystickButtonOptions
					}
                    #endregion MainOptionsTabControl.pageJoystickOptions.tabctrlJoystick
				}
				#endregion MainOptionsTabControl.pageJoystickOptions
		
				#region txtboxSelectedStrength
				control TextBox
				{
					position = ScaleByResolution 28.75 340.0001
					size = Parent 0.5800797 0.07859052
					name = txtboxSelectedStrength
					text = Selected Strength
				}
				#endregion txtboxSelectedStrength
				#region scrlSelectedStrength
				control "GUI\\Controls\\DefaultHScrollBar.gui"
				{
					valueRange = -1 1
					value = 1
					position = ScaleByResolution 18.75 370.0001
					size = ScaleByResolution 510 24
					name = scrlSelectedStrength
				}
				#endregion scrlSelectedStrength
			}

			#region cntrlSelectedImage
			control Control
			{
				topMost = True
				position = ScaleByResolution 588.75 370
				size = Parent 0.2764339 0.3271208
				name = cntrlSelectedImage
				text = Image
			}
			#endregion cntrlSelectedImage
			#region cntrlSelectedDeviceImage
			control Control
			{
				topMost = True
				position = ScaleByResolution 588.75 130
				size = Parent 0.2764339 0.3271208
				name = cntrlSelectedDeviceImage
				text = Image
			}
			#endregion cntrlSelectedDeviceImage
		}
		#endregion Add_Custom_Control.gui
	}
}
