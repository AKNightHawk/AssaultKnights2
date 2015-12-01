// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
package com.neoaxis.game;
import android.app.Activity;
import android.os.Bundle;
import android.view.WindowManager;
import android.view.Display;
import android.view.Window;
import com.neoaxis.utilities.AssetExtractor;

public class GameApp extends Activity 
{
	GameAppGLSurfaceView mGameAppGLSurfaceView;
	private static GameAppNativeBridge mGameAppNativeBridge;

	@Override protected void onCreate(Bundle savedInstanceState) 
	{
		super.onCreate(savedInstanceState);

		//!!!!!
		boolean isFullScreen = false;

		ChangeFullScreenMode(isFullScreen);

		AssetExtractor.extractAssets(this, true);

		Display display = getWindowManager().getDefaultDisplay();
		Window window = getWindow();

		mGameAppNativeBridge = new GameAppNativeBridge(this, display, window);

		mGameAppGLSurfaceView = new GameAppGLSurfaceView(this, getApplication(), mGameAppNativeBridge);
		setContentView(mGameAppGLSurfaceView);
	}
	
	public void ChangeFullScreenMode(boolean fullScreen)
	{
		if(fullScreen)
		{
			requestWindowFeature(Window.FEATURE_NO_TITLE);	
			getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
			WindowManager.LayoutParams.FLAG_FULLSCREEN);
		}
	}

	@Override protected void onPause() 
	{
		super.onPause();
		mGameAppGLSurfaceView.onPause();
	}

	@Override protected void onResume() 
	{
		super.onResume();
		mGameAppGLSurfaceView.onResume();
	}
	
	@Override public void onStop() 
	{
		super.onStop();
	}
	
	public void finish()
	{
		//!!!!!!xx xx;
		mGameAppNativeBridge.Shutdown(false);
		System.runFinalizersOnExit(true);
		System.exit(0);
	}
	
	//!!!!!returnValue is array?
	public boolean ReceiveMessage(String methodName, Object[] arguments, Object[] returnValue)
	{
		//note: arguments can be Integer, Float, Double, Boolean or String
		//returnValue can be Integer (or int), Float (or float), Double (or double), 
		//Boolean (or boolean) or String
		return true;
	}
}
