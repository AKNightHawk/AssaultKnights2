// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
package com.neoaxis.game;
import android.util.Log;
import android.graphics.Rect;
import android.graphics.Point;
import android.view.Display;
import android.util.DisplayMetrics;
import android.graphics.PixelFormat;
import android.view.Window;
import android.os.Build;
import android.content.pm.PackageManager;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager.NameNotFoundException;
import android.app.AlertDialog;
import android.content.DialogInterface;
import java.lang.reflect.Method;
import java.lang.reflect.InvocationTargetException;

public class GameAppNativeBridge 
{
	static GameApp mGameApp;
	boolean mIsEngineInitialized;
	Display mDisplay;
	Window mWindow;
	boolean mIsFinished;
	
	static int mClientWidth;
	static int mClientHeight;
	static int eglContext;

	/////////////////////////////////////////////
	
	static final int MouseButtons_Left = 0;
	static final int MouseButtons_Right = 1;
	static final int MouseButtons_Middle = 2;
	static final int MouseButtons_XButton1 = 3;
	static final int MouseButtons_XButton2 = 4;
//	static final int MouseButtons_Count = 5;

	/////////////////////////////////////////////
	
	public static final int AndroidWindowMessages_Paint = 0;
	public static final int AndroidWindowMessages_Timer = 1;
	public static final int AndroidWindowMessages_MouseDown = 2;
	public static final int AndroidWindowMessages_MouseUp = 3;
	public static final int AndroidWindowMessages_MouseMove = 4;
	public static final int AndroidWindowMessages_KeyDown = 5;
	public static final int AndroidWindowMessages_KeyUp = 6;

	/////////////////////////////////////////////

	public GameAppNativeBridge(GameApp gameApp, Display display, Window window) 
	{
		mGameApp = gameApp;
		mDisplay = display;
		mWindow = window;
		mIsFinished  = false;
	}

	public boolean IsEngineInitialized()
	{
		return mIsEngineInitialized;
	}  

	public static void SetGLContext(int context)
	{
		eglContext = context;
	}
	
	public static void SetClientAreaSize(int width, int height)
	{
		mClientWidth = width;
		mClientHeight = height;
	}
	
	///////////////////////////////////////////////////////////////////////////////////////////

	public int GetScreenBitsPerPixel()
	{
		DisplayMetrics metrics = new DisplayMetrics(); 
		mDisplay.getMetrics(metrics);
		int pixelFormat = mDisplay.getPixelFormat();

		PixelFormat pixelFormatInfo = new PixelFormat();
		PixelFormat.getPixelFormatInfo(pixelFormat, pixelFormatInfo);
		return pixelFormatInfo.bitsPerPixel;
	}

	public Point GetScreenSize()
	{
		DisplayMetrics metrics = new DisplayMetrics(); 
		mDisplay.getMetrics(metrics);
		
		Point screenSize = new Point();
		screenSize.x = metrics.widthPixels;
		screenSize.y = metrics.heightPixels;
		return screenSize;
	}

	public Rect GetWindowRectangle()
	{
		Point screenSize = GetScreenSize();
	
		//!!!!!if windowed mode?
		Rect rect = new Rect();
		rect.left = 0;
		rect.top = 0;
		rect.right = screenSize.x;
		rect.bottom = screenSize.y;

		return rect;
	}

	public Rect GetWindowClientRect()
	{
		Rect rect = new Rect();
		rect.left = 0;
		rect.top = 0;
		rect.right = mClientWidth;
		rect.bottom = mClientHeight;
		return rect;
	}

	public int GetPlatformVersion()
	{
		//!!!!!!!!better parse from string?
		//return Build.VERSION.RELEASE;
	
		return Build.VERSION.SDK_INT;
	}

	public int GetGLContext()
	{
		return eglContext;
	}
	
	public void ShutdownApplicationWindow()
	{
		mGameApp.finish();
		mIsEngineInitialized = false;
	}
		
	public void MessageBox(final String text, final String caption)
	{
		mGameApp.runOnUiThread(new Runnable() {
			public void run() {
				AlertDialog.Builder dlgAlert  = new AlertDialog.Builder(mGameApp);                      
				dlgAlert.setTitle(caption); 
				dlgAlert.setMessage(text); 
				dlgAlert.setPositiveButton("OK", null);
				dlgAlert.setCancelable(false);
				dlgAlert.create().show();
			}
		});	
	}

	public void Fatal(final String text)
	{
		mIsFinished = true;
		mGameApp.runOnUiThread(new Runnable() {
			public void run() {
				AlertDialog.Builder dlgAlert  = new AlertDialog.Builder(mGameApp);                      
				dlgAlert.setTitle("Fatal error"); 
				dlgAlert.setMessage(text); 
				dlgAlert.setPositiveButton("OK", new DialogInterface.OnClickListener() {
					public void onClick(DialogInterface dialog, int whichButton) {
						mGameApp.finish(); 
					}
				});
				dlgAlert.setCancelable(false);
				dlgAlert.create().show();
			}
		});				
	}

	public boolean ReceiveMessage(String methodName, Object[] arguments, Object[] returnValue)
	{
		return mGameApp.ReceiveMessage(methodName, arguments, returnValue);
	}

	public boolean IsFinished()
	{
		return mIsFinished;
	}

	///////////////////////////////////////////////////////////////////////////////////////////

	public boolean Init()
	{		
		String dataDir;
		try
		{
			PackageInfo info = mGameApp.getPackageManager().getPackageInfo(
				mGameApp.getPackageName(), PackageManager.GET_ACTIVITIES);
			dataDir = info.applicationInfo.dataDir;
		}
		catch(NameNotFoundException exception)
		{
			Log.e("GameApp", exception.toString());
			return false;
		}
		
		mIsEngineInitialized = AndroidAppLauncherInit(dataDir);
		return mIsEngineInitialized;
	}

	//!!!!!!need call MouseUp and KeyUp for all buttons when application switched to not active state.

	//!!!!!never call "Timer" message	
	//!!!!!!
	public void SendWindowMessage(int message, int parameter1, int parameter2)
	{
		if(mIsEngineInitialized)
			AndroidAppLauncherSendWindowMessage(message, parameter1, parameter2);
	}

	public void Shutdown(boolean sendMessage)
	{
		//!!!!!!xx xx;
		if(mIsEngineInitialized)
            AndroidAppLauncherShutdown(sendMessage);
	}
	
	public boolean IsNeedExit()
	{
 		if(mIsEngineInitialized && !mIsFinished)
			return AndroidAppLauncherIsNeedExit();
		return false;
	}
	
	public void SendUserCustomMessage(int message)
	{
		if(mIsEngineInitialized)
			AndroidAppLauncherSendUserCustomMessage(message);
	}

	///////////////////////////////////////////////////////////////////////////////////////////

	private native boolean AndroidAppLauncherInit(String dataDir);
	private native int AndroidAppLauncherSendWindowMessage(int message, int parameter1, int parameter2);
	private native void AndroidAppLauncherShutdown(boolean sendMessage);
	private native boolean AndroidAppLauncherIsNeedExit();
	private native void AndroidAppLauncherSendUserCustomMessage(int message);

	static
	{
        System.loadLibrary("AndroidAppLauncher");
	}
}
