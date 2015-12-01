// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Diagnostics;
using Engine;
using Engine.FileSystem;
using Engine.Utils;
using MySql.Data.MySqlClient;
using ProjectCommon;

//using System.Data.Common.DbConnection;

namespace Game
{
    /// <summary>
    /// Defines an input point in the application.
    /// </summary>
    public static class Program
    {
        //assaultknights MySql Database Connection
        internal static MySqlConnection AKsqlcon;

        internal static string username;

        public static bool needRestartApplication;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (Debugger.IsAttached)
            {
                Main2();
            }
            else
            {
                try
                {
                    Main2();
                }
                catch (Exception e)
                {
                    Log.FatalAsException(e.ToString());
                }
            }
        }

        private static void Main2()
        {
            //initialize file sytem of the engine
            if (!VirtualFileSystem.Init("user:Logs/Game.log", true, null, null, null, null))
                return;

            //configure general settings
            EngineApp.ConfigName = "user:Configs/Game.config";
            if (PlatformInfo.Platform == PlatformInfo.Platforms.Windows)
                EngineApp.UseDirectInputForMouseRelativeMode = true;
            EngineApp.AllowJoysticksAndCustomInputDevices = true;
            EngineApp.AllowWriteEngineConfigFile = true;
            EngineApp.AllowChangeVideoMode = true;
            //Change Floating Point Model for FPU math calculations. Default is Strict53Bits.
            //FloatingPointModel.Model = FloatingPointModel.Models.Strict53Bits;

            //init engine application
            EngineApp.Init(new GameEngineApp());
            //enable support field and properties serialization for GameEngineApp class.
            EngineApp.Instance.Config.RegisterClassParameters(typeof(GameEngineApp));

            //update window
            //EngineApp.Instance.WindowTitle = "Game";
            EngineApp.Instance.WindowTitle = "Assault Knights: Reign of Steel";
            if (PlatformInfo.Platform == PlatformInfo.Platforms.Windows)
                EngineApp.Instance.Icon = Game.Properties.Resources.Logo;

            //create game console
            EngineConsole.Init();

            //initialize Mysql Connection
            MySqlCon();

            EngineApp.Instance.Config.RegisterClassParameters(typeof(GameEngineApp));

            //EngineApp.Instance.SuspendWorkingWhenApplicationIsNotActive = false;

            //create and run application loop.
            if (EngineApp.Instance.Create())
                EngineApp.Instance.Run();

            EngineApp.Shutdown();

            AKsqlcon.Close();

            Log.DumpToFile("Program END\r\n");

            VirtualFileSystem.Shutdown();

            if (needRestartApplication)
                Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location, "");
        }

        private static void MySqlCon()
        {
            //string connStr = "SERVER=72.232.2.58;" + "DATABASE=natypesc_AKV8;" + "UID=natypesc_akros;" + "PASSWORD=AKNightHawk1;";
            //string connStr = "SERVER=127.0.0.1;" + "DATABASE=akv8;" + "UID=root;" + "PASSWORD=activesp935;";
            //string connStr = "SERVER=sagamedb02.db.9001248.hostedresource.com;" + "DATABASE=sagamedb02;" + "UID=sagamedb02;" + "PASSWORD=SA.gamedb@2;";
            string connStr = "SERVER=dframe.mysql.guardedhost.com;" + "DATABASE=billCAT;" + "UID=BillKo;" + "PASSWORD=M@rlb0r0;";
            AKsqlcon = new MySqlConnection(connStr);
            try
            {
                AKsqlcon.Open();
            }
            catch (Exception)
            {
                Log.Error("No Internet connection available. Multiplayer will be disabled");
            }
        }

        ////Android. Entry methods from main module.
        //public static void MainModule_Message( EngineApp.MainModuleMessages message, IntPtr data )
        //{
        //   try
        //   {
        //      switch( message )
        //      {
        //      case EngineApp.MainModuleMessages.Init:

        //         unsafe
        //         {
        //            EngineApp.MainModuleInitData* initData = (EngineApp.MainModuleInitData*)data;

        //            //!!!!!!!!temp path
        //            //http://developer.android.com/guide/topics/data/data-storage.html
        //            string logFileName = initData->ExecutableDirectoryPath + "/UserSettings/Logs/Game.log";
        //            if( !VirtualFileSystem.Init( logFileName, false, initData->ExecutableDirectoryPath,
        //               null, null ) )
        //               return;

        //            //!!!!!!must be False
        //            EngineApp.AllowChangeVideoMode = true;

        //            EngineApp.Init( new GameEngineApp(), data );

        //            EngineApp.MainModule_Message( message, data );

        //            EngineConsole.Init();

        //            EngineApp.Instance.Config.RegisterClassParameters( typeof( GameEngineApp ) );

        //            EngineApp.Instance.Create();

        //            //!!!!!temp
        //            EngineApp.Instance.ShowFPS = true;
        //         }
        //         break;

        //      case EngineApp.MainModuleMessages.Shutdown:
        //         EngineApp.MainModule_Message( message, data );
        //         EngineApp.Shutdown();
        //         Log.DumpToFile( "Program END\r\n" );
        //         VirtualFileSystem.Shutdown();
        //         break;

        //      case EngineApp.MainModuleMessages.WindowMessage:
        //         EngineApp.MainModule_Message( message, data );
        //         break;

        //      case EngineApp.MainModuleMessages.IsNeedExit:
        //         unsafe
        //         {
        //            ( *(bool*)data ) = EngineApp.Instance.IsNeedExit();
        //         }
        //         break;

        //      case EngineApp.MainModuleMessages.UserCustomMessage:
        //         {
        //            //int message = (int)data.ToInt32();
        //         }
        //         break;

        //      }
        //   }
        //   catch( Exception e )
        //   {
        //      Log.FatalAsException( e.ToString() );
        //   }
        //}
        /*
        public static void WebPlayer_Message(EngineApp.WebPlayerMessages message, IntPtr data)
        {
            try
            {
                switch (message)
                {
                    case EngineApp.WebPlayerMessages.Init:

                        unsafe
                        {
                            EngineApp.WebPlayerInitData* initData = (EngineApp.WebPlayerInitData*)data;

                            if (!VirtualFileSystem.Init("user:Logs/WebPlayer.log", false,
                                initData->ExecutableDirectoryPath, null, null))
                                return;

                            //set render settings
                            //RendererWorld.InitializationOptions.MaxPixelShadersVersion = RendererWorld.MaxPixelShadersVersions.PS0;
                            //RendererWorld.InitializationOptions.MaxVertexShadersVersion = RendererWorld.MaxVertexShadersVersions.VS0;
                            //RendererWorld.InitializationOptions.FullSceneAntialiasing = 0;
                            //RendererWorld.InitializationOptions.FilteringMode = RendererWorld.FilteringModes.Trilinear;

                            EngineApp.Init(new GameEngineApp());

                            EngineApp.Instance.MaxFPS = 30;

                            EngineApp.WebPlayer_Message(message, data);

                            EngineConsole.Init();

                            EngineApp.Instance.Config.RegisterClassParameters(typeof(GameEngineApp));

                            EngineApp.Instance.Create();
                        }
                        break;

                    case EngineApp.WebPlayerMessages.Shutdown:

                        EngineApp.WebPlayer_Message(message, data);

                        EngineApp.Shutdown();
                        Log.DumpToFile("Program END\r\n");
                        VirtualFileSystem.Shutdown();

                        break;

                    case EngineApp.WebPlayerMessages.WindowMessage:
                        EngineApp.WebPlayer_Message(message, data);
                        break;
                }
            }
            catch (Exception e)
            {
                Log.FatalAsException(e.ToString());
            }
        }*/

        //public static object SupportedgameTypes { get; set; }
    }
}