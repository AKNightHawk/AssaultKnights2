// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using Engine;

namespace ProjectCommon
{
    public enum CommandList
    {
        VerticleTakeOff_L_EngineUp,
        VerticleTakeOff_L_EngineDown,
        Mechstop,
        Light,
        Shut_Down_Override,
        Shift,
        Z,
        Croutch,
        RadarZoomIn,
        RadarZoomOut,
        Heatvision,
        Nightvision,
        Lights,
        SHIFT,
        Arrow_Up,
        Arrow_Down,
        Arrow_Right,
        Arrow_Left,
        Previous,
        Next,
        Mode,
        Target,
        PCJumpForward,
        PCJumpBackward,
        PCJumpLeft,
        PCJumpRight,
        PCDown,
        JumpForward,
        JumpBackward,
        JumpLeft,
        JumpRight,
        Forward,
        Backward,
        Left,
        Right,
        LookUp,
        LookDown,
        LookLeft,
        LookRight,
        Fire1,
        Fire2,
        Jump,
        Crouching,
        Reload,
        Use,
        PreviousWeapon,
        NextWeapon,
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Weapon5,
        Weapon6,
        Weapon7,
        Weapon8,
        Weapon9,
        Run,
        VehicleGearUp,
        VehicleGearDown,
        VehicleHandbrake,
        ChatCommand,
        ToggleNightVision,
        ToggleHeatVision,
        ChangeCamera,
        LoadBuyWindow,
        CoolantFlush,
        Count,
    }

    public enum GameControlKeys
    {
        //VTOl

        [DefaultKeyboardMouseValue(EKeys.Add)]
        VerticleTakeOff_L_EngineUp,

        [DefaultKeyboardMouseValue(EKeys.Subtract)]
        VerticleTakeOff_L_EngineDown,

        //P1
        [DefaultKeyboardMouseValue(EKeys.X)]
        Mechstop,

        [DefaultKeyboardMouseValue(EKeys.L)]
        Light,

        [DefaultKeyboardMouseValue(EKeys.O)]
        Shut_Down_Override,

        [DefaultKeyboardMouseValue(EKeys.Shift)]
        Shift,

        [DefaultKeyboardMouseValue(EKeys.Z)]
        Z,

        [DefaultKeyboardMouseValue(EKeys.Control)]
        Croutch,

        // radar
        [DefaultKeyboardMouseValue(EKeys.Q)]
        RadarZoomIn,

        [DefaultKeyboardMouseValue(EKeys.Z)]
        RadarZoomOut,

        [DefaultKeyboardMouseValue(EKeys.H)]
        Heatvision,

        [DefaultKeyboardMouseValue(EKeys.N)]
        Nightvision,

        [DefaultKeyboardMouseValue(EKeys.L)]
        Lights,

        //helli
        [DefaultKeyboardMouseValue(EKeys.Shift)]
        SHIFT,

        //[DefaultKeyboardMouseValue(EKeys.Up)] //modified per Night hawk
        [DefaultKeyboardMouseValue(EKeys.NumPad8)]
        Arrow_Up,

        //[DefaultKeyboardMouseValue(EKeys.Down)] //modified per Night hawk
        [DefaultKeyboardMouseValue(EKeys.NumPad2)]
        Arrow_Down,

        //[DefaultKeyboardMouseValue(EKeys.Right)]//modified per Night hawk
        [DefaultKeyboardMouseValue(EKeys.NumPad6)]
        Arrow_Right,

        //[DefaultKeyboardMouseValue(EKeys.Left)]//modified per Night hawk
        [DefaultKeyboardMouseValue(EKeys.NumPad2)]
        Arrow_Left,

        //keys for mech start
        [DefaultKeyboardMouseValue(EKeys.Oemcomma)]
        Previous,

        [DefaultKeyboardMouseValue(EKeys.OemPeriod)]
        Next,

        [DefaultKeyboardMouseValue(EKeys.OemMinus)]
        Mode,

        [DefaultKeyboardMouseValue(EKeys.Tab)]
        Target,

        ////keys for mech end

        //Player Character JumpJets
        [DefaultKeyboardMouseValue(EKeys.I)]
        PCJumpForward,

        [DefaultKeyboardMouseValue(EKeys.K)]
        PCJumpBackward,

        [DefaultKeyboardMouseValue(EKeys.J)]
        PCJumpLeft,

        [DefaultKeyboardMouseValue(EKeys.L)]
        PCJumpRight,

        [DefaultKeyboardMouseValue(EKeys.L)]
        PCDown,

        [DefaultKeyboardMouseValue(EKeys.Up)]
        JumpForward,

        [DefaultKeyboardMouseValue(EKeys.Down)]
        JumpBackward,

        [DefaultKeyboardMouseValue(EKeys.Left)]
        JumpLeft,

        [DefaultKeyboardMouseValue(EKeys.Right)]
        JumpRight,

        ///////////////////////////////////////////
        //Moving

        [DefaultKeyboardMouseValue(EKeys.W)]
        [DefaultKeyboardMouseValue(EKeys.Up)]
        [DefaultJoystickValue(JoystickAxes.Y, JoystickAxisFilters.GreaterZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickY, JoystickAxisFilters.GreaterZero, 1)]
        [DefaultJoystickValue(JoystickSliders.Slider1, JoystickSliderAxes.X, JoystickAxisFilters.OnlyGreaterZero, 1)]
        Forward,

        [DefaultKeyboardMouseValue(EKeys.S)]
        [DefaultKeyboardMouseValue(EKeys.Down)]
        [DefaultJoystickValue(JoystickAxes.Y, JoystickAxisFilters.LessZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickY, JoystickAxisFilters.LessZero, 1)]
        [DefaultJoystickValue(JoystickSliders.Slider1, JoystickSliderAxes.X, JoystickAxisFilters.OnlyLessZero, 1)]
        Backward,

        [DefaultKeyboardMouseValue(EKeys.A)]
        [DefaultKeyboardMouseValue(EKeys.Left)]
        [DefaultJoystickValue(JoystickAxes.X, JoystickAxisFilters.LessZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickX, JoystickAxisFilters.LessZero, 1)]
        Left,

        [DefaultKeyboardMouseValue(EKeys.D)]
        [DefaultKeyboardMouseValue(EKeys.Right)]
        [DefaultJoystickValue(JoystickAxes.X, JoystickAxisFilters.GreaterZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickX, JoystickAxisFilters.GreaterZero, 1)]
        Right,

        ///////////////////////////////////////////
        //Looking

        [DefaultJoystickValue(JoystickAxes.Rz, JoystickAxisFilters.GreaterZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickY, JoystickAxisFilters.GreaterZero, 1)]
        //MouseMove (in the PlayerIntellect)
        LookUp,

        [DefaultJoystickValue(JoystickAxes.Rz, JoystickAxisFilters.LessZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickY, JoystickAxisFilters.LessZero, 1)]
        //MouseMove (in the PlayerIntellect)
        LookDown,

        [DefaultJoystickValue(JoystickAxes.Z, JoystickAxisFilters.LessZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickX, JoystickAxisFilters.LessZero, 1)]
        //MouseMove (in the PlayerIntellect)
        LookLeft,

        [DefaultJoystickValue(JoystickAxes.Z, JoystickAxisFilters.GreaterZero, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickX, JoystickAxisFilters.GreaterZero, 1)]
        //MouseMove (in the PlayerIntellect)
        LookRight,

        ///////////////////////////////////////////
        //Actions

        [DefaultKeyboardMouseValue(EMouseButtons.Left)]
        [DefaultJoystickValue(JoystickButtons.Button1, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightTrigger, JoystickAxisFilters.GreaterZero, 1)]
        Fire1,

        [DefaultKeyboardMouseValue(EMouseButtons.Right)]
        [DefaultJoystickValue(JoystickButtons.Button2, 1)]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftTrigger, JoystickAxisFilters.GreaterZero, 1)]
        Fire2,

        [DefaultKeyboardMouseValue(EKeys.Space)]
        [DefaultJoystickValue(JoystickButtons.Button3, 1)]
        [DefaultJoystickValue(JoystickButtons.XBox360_A, 1)]
        Jump,

        [DefaultKeyboardMouseValue(EKeys.C)]
        [DefaultJoystickValue(JoystickButtons.Button6, 1)]
        [DefaultJoystickValue(JoystickButtons.XBox360_B, 1)]
        Crouching,

        [DefaultKeyboardMouseValue(EKeys.R)]
        [DefaultJoystickValue(JoystickButtons.Button4, 1)]
        [DefaultJoystickValue(JoystickButtons.XBox360_LeftShoulder, 1)]
        Reload,

        [DefaultKeyboardMouseValue(EKeys.E)]
        [DefaultJoystickValue(JoystickButtons.Button5, 1)]
        [DefaultJoystickValue(JoystickButtons.XBox360_RightShoulder, 1)]
        Use,

        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.West, 1)]
        PreviousWeapon,

        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.East, 1)]
        NextWeapon,

        [DefaultKeyboardMouseValue(EKeys.D1)]
        Weapon1,

        [DefaultKeyboardMouseValue(EKeys.D2)]
        Weapon2,

        [DefaultKeyboardMouseValue(EKeys.D3)]
        Weapon3,

        [DefaultKeyboardMouseValue(EKeys.D4)]
        Weapon4,

        [DefaultKeyboardMouseValue(EKeys.D5)]
        Weapon5,

        [DefaultKeyboardMouseValue(EKeys.D6)]
        Weapon6,

        [DefaultKeyboardMouseValue(EKeys.D7)]
        Weapon7,

        [DefaultKeyboardMouseValue(EKeys.D8)]
        Weapon8,

        [DefaultKeyboardMouseValue(EKeys.D9)]
        Weapon9,

        [DefaultKeyboardMouseValue(EKeys.Shift)]
        Run,

        //Vehicle
        [DefaultKeyboardMouseValue(EKeys.Z)]
        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.North, 1)]
        VehicleGearUp,

        [DefaultKeyboardMouseValue(EKeys.X)]
        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.South, 1)]
        VehicleGearDown,

        [DefaultKeyboardMouseValue(EKeys.Space)]
        VehicleHandbrake,

        [DefaultKeyboardMouseValue(EKeys.T)]
        ChatCommand,

        [DefaultKeyboardMouseValue(EKeys.N)]
        ToggleNightVision,

        [DefaultKeyboardMouseValue(EKeys.H)]
        ToggleHeatVision,

        [DefaultKeyboardMouseValue(EKeys.C)]
        ChangeCamera,

        [DefaultKeyboardMouseValue(EKeys.B)]
        LoadBuyWindow,

        //Count,
        [DefaultKeyboardMouseValue(EKeys.F)]
        CoolantFlush,
    }
}