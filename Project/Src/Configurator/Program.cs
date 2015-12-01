// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Engine.FileSystem;

namespace Configurator
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        private static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                try
                {
                    SetProcessDPIAware();
                }
                catch { }
            }

            if (!VirtualFileSystem.Init(null, true, null, null, null, null))
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            VirtualFileSystem.Shutdown();
        }
    }
}