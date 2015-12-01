// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
package com.neoaxis.game;
import android.content.Context;
import android.graphics.PixelFormat;
import android.opengl.GLSurfaceView;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.os.Bundle;  
import java.util.Queue;
import java.util.LinkedList;
import javax.microedition.khronos.egl.EGL10;
import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.egl.EGLContext;
import javax.microedition.khronos.egl.EGLDisplay;
import javax.microedition.khronos.opengles.GL10;

class GameAppGLSurfaceView extends GLSurfaceView 
{    
	//!!!!!пересмотреть
	//!!!!!!!!инициализацию opengl

    private static class MultithreadedMessage
    {
        public int messageType;
        public int parameter1;
        public int parameter2;

        public MultithreadedMessage(int messageType, int parameter1, int parameter2)
        {
            this.messageType = messageType;
            this.parameter1 = parameter1;
            this.parameter2 = parameter2;
        }
    }

	private static String TAG = "GameAppGLSurfaceView";
	private static final boolean DEBUG = false;
	private static GameAppNativeBridge mGameAppNativeBridge;
	private static GameAppGLRenderer mRenderer;
	private static GameAppGLSurfaceView instance;
	private static GameApp mGameApp;

	private static Queue<MultithreadedMessage> multithreadedMessages = new LinkedList<MultithreadedMessage>();

	private int lastMousePositionX;
	private int lastMousePositionY;
	
	//

	public GameAppGLSurfaceView(GameApp gameApp, Context context, GameAppNativeBridge gameAppNativeBridge) 
	{
		super(context);
		instance = this;
		mGameAppNativeBridge = gameAppNativeBridge;
		mGameApp = gameApp;
		//!!!!!!
		//!!!!!есть мысль что сначала нужно depth 32 проверить, потом 24 и потом 16.
		Init(true, 16, 0);
		//Init(false, 0, 0);
		setFocusable(true);
		setFocusableInTouchMode(true);
	}

	//!!!!!!why no "@Override"?
	public boolean onTouchEvent(final MotionEvent event) 
	{	
		int action = event.getAction();
		int currentPositionX = (int)event.getX();
		int currentPositionY = (int)event.getY();
		
		switch(action)
		{
		case MotionEvent.ACTION_DOWN:
			{
				if(lastMousePositionX != currentPositionX || lastMousePositionY != currentPositionY)
				{
					lastMousePositionX = currentPositionX;
					lastMousePositionY = currentPositionY;					
					PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_MouseMove, 
						lastMousePositionX, lastMousePositionY);
				}
				//!!!!left only				
				PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_MouseDown, 
					GameAppNativeBridge.MouseButtons_Left, 0);
			}
			break;
			
		case MotionEvent.ACTION_UP:
			{
				if(lastMousePositionX != currentPositionX || lastMousePositionY != currentPositionY)
				{
					lastMousePositionX = currentPositionX;
					lastMousePositionY = currentPositionY;
					PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_MouseMove, 
						lastMousePositionX, lastMousePositionY);
				}			
				//!!!!left only
				PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_MouseUp, 
					GameAppNativeBridge.MouseButtons_Left, 0);
			}
			break;
				
		case MotionEvent.ACTION_MOVE:
			{
				lastMousePositionX = currentPositionX;
				lastMousePositionY = currentPositionY;
				PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_MouseMove, 
					lastMousePositionX, lastMousePositionY);
			}
			break;
		}

		return true;
	}	

//!!!!!!?
//	@Override
//	public boolean onTrackballEvent(final MotionEvent event) 
//	{	
//		int action = event.getAction();
//		int currentXPosition = (int) event.getX();
//		int currentYPosition = (int) event.getY();
//		//!!!!!!
//		Log.d("onTrackballEvent", "x = " + currentXPosition + ", y = " + currentYPosition);
//
//		switch(action)
//		{
//			case MotionEvent.ACTION_DOWN:
//				SendMessageToRenderer(GameAppGLRenderer.Message_OnMouseDown, 
//					currentXPosition, currentYPosition);
//				break;
//			case MotionEvent.ACTION_UP:
//				SendMessageToRenderer(GameAppGLRenderer.Message_OnMouseUp, 
//					currentXPosition, currentYPosition);
//				break;
//			case MotionEvent.ACTION_MOVE:
//				SendMessageToRenderer(GameAppGLRenderer.Message_OnMouseMove, 
//					currentXPosition, currentYPosition);
//				break;
//		}
//
//		return true;
//	}
		
	@Override
	public boolean onKeyDown(int keyCode, KeyEvent event)
	{
		int eKey = GameAppKeyCodes.GetEKeyByKeyCode(keyCode);
		int characterCode = GameAppKeyCodes.GetCharacterCodeByKeyCode(keyCode, event);
		if(eKey != 0)
			PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_KeyDown, eKey, characterCode);
			
		//!!!! сворачивание окна по клавише НАЗАД
		//!!!!!!надо если обработали, то return true?
		if(keyCode == KeyEvent.KEYCODE_BACK)
			return true;

		return super.onKeyDown(keyCode, event);
	}

	@Override
	public boolean onKeyLongPress(int keyCode, KeyEvent event) 
	{
		//!!!!!?
		return true;
		//return super.onKeyLongPress(keyCode, event);
	}

	//@Override
	//public boolean onKeyMultiple(int keyCode, int repeatCount, KeyEvent event) 
	//{
	//	return true;
	//	//return super.onKeyMultiple(keyCode, event);
	//}

	//@Override
	//public boolean onKeyShortcut(int keyCode, KeyEvent event) 
	//{
	//	return true;
	//	//return super.onKeyShortcut(keyCode, event);
	//}

	@Override
	public boolean onKeyUp(int keyCode, KeyEvent event) 
	{
		int eKey = GameAppKeyCodes.GetEKeyByKeyCode(keyCode);
		//int characterCode = GameAppKeyCodes.GetCharacterCodeByKeyCode(keyCode, event);
		if(eKey != 0)
			PushMultithreadedMessage(GameAppNativeBridge.AndroidWindowMessages_KeyUp, eKey, 0);
	
		//!!!!!!? сворачивание окна по клавише НАЗАД
		//!!!!!!надо если обработали, то return true?
		if(keyCode == KeyEvent.KEYCODE_BACK)
			return true;

		return super.onKeyUp(keyCode, event);
	}  

	private void Init(boolean translucent, int depth, int stencil) 
	{
		/* By default, GLSurfaceView() creates a RGB_565 opaque surface.
		* If we want a translucent one, we should change the surface's
		* format here, using PixelFormat.TRANSLUCENT for GL Surfaces
		* is interpreted as any 32-bit surface with alpha by SurfaceFlinger.
		*/
		if (translucent)
		{
			//!!!!!!!?
			this.getHolder().setFormat(PixelFormat.TRANSLUCENT);
		}

		/* Setup the context factory for 2.0 rendering.
		* See ContextFactory class definition below
		*/
		setEGLContextFactory(new ContextFactory());

		/* We need to choose an EGLConfig that matches the format of
		* our surface exactly. This is going to be done in our
		* custom config chooser. See ConfigChooser class definition
		* below.
		*/
		setEGLConfigChooser( translucent ?
			new ConfigChooser(8, 8, 8, 8, depth, stencil) :
			new ConfigChooser(5, 6, 5, 0, depth, stencil) );

		/* Set the renderer responsible for frame rendering */
		mRenderer = new GameAppGLRenderer(this);
		setRenderer(mRenderer);
	}

	private static class ContextFactory implements GLSurfaceView.EGLContextFactory 
	{
		private static int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        
		public EGLContext createContext(EGL10 egl, EGLDisplay display, EGLConfig eglConfig) 
		{
			checkEglError("Before eglCreateContext", egl);
			int[] attrib_list = { EGL_CONTEXT_CLIENT_VERSION, 2, EGL10.EGL_NONE };
			EGLContext context = egl.eglCreateContext(display, eglConfig, EGL10.EGL_NO_CONTEXT, attrib_list);
			checkEglError("After eglCreateContext", egl);
			return context;
		}

		public void destroyContext(EGL10 egl, EGLDisplay display, EGLContext context) 
		{
			egl.eglDestroyContext(display, context);
		}
	}

	private static void checkEglError(String prompt, EGL10 egl) 
	{
		int error;
		//!!!!!
		while((error = egl.eglGetError()) != EGL10.EGL_SUCCESS) 
		{
			Log.e(TAG, String.format("%s: EGL error: 0x%x", prompt, error));
		}
	}

	static void PushMultithreadedMessage(int messageType, int parameter1, int parameter2)		
	{
		synchronized(multithreadedMessages)
		{
			multithreadedMessages.add(new MultithreadedMessage(messageType, parameter1, parameter2));
		}
	}
	
	static void SendMultithreadedMessages()
	{
		synchronized(multithreadedMessages)
		{
			do
			{
				MultithreadedMessage message = multithreadedMessages.poll();
				if(message == null)
					break;
				mGameAppNativeBridge.SendWindowMessage(message.messageType, message.parameter1, 
					message.parameter2);
			}while(true);
		}			
	}	

	/////////////////////////////////////////////

	private static class ConfigChooser implements GLSurfaceView.EGLConfigChooser 
	{
		protected int mRedSize;
		protected int mGreenSize;
		protected int mBlueSize;
		protected int mAlphaSize;
		protected int mDepthSize;
		protected int mStencilSize;
	
		public ConfigChooser(int r, int g, int b, int a, int depth, int stencil) 
		{
			mRedSize = r;
			mGreenSize = g;
			mBlueSize = b;
			mAlphaSize = a;
			mDepthSize = depth;
			mStencilSize = stencil;
		}

		//!!!!!!
		/* This EGL config specification is used to specify 2.0 rendering.
		* We use a minimum size of 4 bits for red/green/blue, but will
		* perform actual matching in chooseConfig() below.
		*/
		private static int EGL_OPENGL_ES2_BIT = 4;
		private static int[] s_configAttribs2 =
		{
			EGL10.EGL_RED_SIZE, 8,//4, //!!!!!!?
			EGL10.EGL_GREEN_SIZE, 8,//4,
			EGL10.EGL_BLUE_SIZE, 8,//4,

			//!!!!!
			//EGL10.EGL_DEPTH_SIZE, 16,
			//!!!!!what else?

			EGL10.EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
			EGL10.EGL_NONE
		};

		public EGLConfig chooseConfig(EGL10 egl, EGLDisplay display) 
		{
			/* Get the number of minimally matching EGL configurations
			*/
			int[] num_config = new int[1];
			egl.eglChooseConfig(display, s_configAttribs2, null, 0, num_config);

			int numConfigs = num_config[0];

			if (numConfigs <= 0) 
			{
				throw new IllegalArgumentException("No configs match configSpec");
			}

			/* Allocate then read the array of minimally matching EGL configs
			*/
			EGLConfig[] configs = new EGLConfig[numConfigs];
			egl.eglChooseConfig(display, s_configAttribs2, configs, numConfigs, num_config);

			if (DEBUG) 
			{
				printConfigs(egl, display, configs);
			}
			/* Now return the "best" one
			*/
			return chooseConfig(egl, display, configs);
		}

		public EGLConfig chooseConfig(EGL10 egl, EGLDisplay display, EGLConfig[] configs) 
		{
			for(EGLConfig config : configs) 
			{
				int d = findConfigAttrib(egl, display, config, EGL10.EGL_DEPTH_SIZE, 0);
				int s = findConfigAttrib(egl, display, config, EGL10.EGL_STENCIL_SIZE, 0);

				// We need at least mDepthSize and mStencilSize bits
				if (d >= mDepthSize && s >= mStencilSize)
				{
					// We want an *exact* match for red/green/blue/alpha
					int r = findConfigAttrib(egl, display, config, EGL10.EGL_RED_SIZE, 0);
					int g = findConfigAttrib(egl, display, config, EGL10.EGL_GREEN_SIZE, 0);
					int b = findConfigAttrib(egl, display, config, EGL10.EGL_BLUE_SIZE, 0);
					int a = findConfigAttrib(egl, display, config, EGL10.EGL_ALPHA_SIZE, 0);

					if (r == mRedSize && g == mGreenSize && b == mBlueSize && a == mAlphaSize)
						return config;
				}
			}
			return null;
		}

		private int findConfigAttrib(EGL10 egl, EGLDisplay display, EGLConfig config, int attribute, 
			int defaultValue) 
		{
			int[] value = new int[1];
			if (egl.eglGetConfigAttrib(display, config, attribute, value)) 
				return value[0];
			return defaultValue;
		}

		private void printConfigs(EGL10 egl, EGLDisplay display, EGLConfig[] configs) 
		{
			int numConfigs = configs.length;
			Log.w(TAG, String.format("%d configurations", numConfigs));
			for (int i = 0; i < numConfigs; i++) 
			{
				Log.w(TAG, String.format("Configuration %d:\n", i));
				printConfig(egl, display, configs[i]);
			}
		}

		private void printConfig(EGL10 egl, EGLDisplay display, EGLConfig config) 
		{
			int[] attributes = 
			{
				EGL10.EGL_BUFFER_SIZE,
				EGL10.EGL_ALPHA_SIZE,
				EGL10.EGL_BLUE_SIZE,
				EGL10.EGL_GREEN_SIZE,
				EGL10.EGL_RED_SIZE,
				EGL10.EGL_DEPTH_SIZE,
				EGL10.EGL_STENCIL_SIZE,
				EGL10.EGL_CONFIG_CAVEAT,
				EGL10.EGL_CONFIG_ID,
				EGL10.EGL_LEVEL,
				EGL10.EGL_MAX_PBUFFER_HEIGHT,
				EGL10.EGL_MAX_PBUFFER_PIXELS,
				EGL10.EGL_MAX_PBUFFER_WIDTH,
				EGL10.EGL_NATIVE_RENDERABLE,
				EGL10.EGL_NATIVE_VISUAL_ID,
				EGL10.EGL_NATIVE_VISUAL_TYPE,
				0x3030, // EGL10.EGL_PRESERVED_RESOURCES,
				EGL10.EGL_SAMPLES,
				EGL10.EGL_SAMPLE_BUFFERS,
				EGL10.EGL_SURFACE_TYPE,
				EGL10.EGL_TRANSPARENT_TYPE,
				EGL10.EGL_TRANSPARENT_RED_VALUE,
				EGL10.EGL_TRANSPARENT_GREEN_VALUE,
				EGL10.EGL_TRANSPARENT_BLUE_VALUE,
				0x3039, // EGL10.EGL_BIND_TO_TEXTURE_RGB,
				0x303A, // EGL10.EGL_BIND_TO_TEXTURE_RGBA,
				0x303B, // EGL10.EGL_MIN_SWAP_INTERVAL,
				0x303C, // EGL10.EGL_MAX_SWAP_INTERVAL,
				EGL10.EGL_LUMINANCE_SIZE,
				EGL10.EGL_ALPHA_MASK_SIZE,
				EGL10.EGL_COLOR_BUFFER_TYPE,
				EGL10.EGL_RENDERABLE_TYPE,
				0x3042 // EGL10.EGL_CONFORMANT
			};

			String[] names = 
			{
				"EGL_BUFFER_SIZE",
				"EGL_ALPHA_SIZE",
				"EGL_BLUE_SIZE",
				"EGL_GREEN_SIZE",
				"EGL_RED_SIZE",
				"EGL_DEPTH_SIZE",
				"EGL_STENCIL_SIZE",
				"EGL_CONFIG_CAVEAT",
				"EGL_CONFIG_ID",
				"EGL_LEVEL",
				"EGL_MAX_PBUFFER_HEIGHT",
				"EGL_MAX_PBUFFER_PIXELS",
				"EGL_MAX_PBUFFER_WIDTH",
				"EGL_NATIVE_RENDERABLE",
				"EGL_NATIVE_VISUAL_ID",
				"EGL_NATIVE_VISUAL_TYPE",
				"EGL_PRESERVED_RESOURCES",
				"EGL_SAMPLES",
				"EGL_SAMPLE_BUFFERS",
				"EGL_SURFACE_TYPE",
				"EGL_TRANSPARENT_TYPE",
				"EGL_TRANSPARENT_RED_VALUE",
				"EGL_TRANSPARENT_GREEN_VALUE",
				"EGL_TRANSPARENT_BLUE_VALUE",
				"EGL_BIND_TO_TEXTURE_RGB",
				"EGL_BIND_TO_TEXTURE_RGBA",
				"EGL_MIN_SWAP_INTERVAL",
				"EGL_MAX_SWAP_INTERVAL",
				"EGL_LUMINANCE_SIZE",
				"EGL_ALPHA_MASK_SIZE",
				"EGL_COLOR_BUFFER_TYPE",
				"EGL_RENDERABLE_TYPE",
				"EGL_CONFORMANT"
			};

			int[] value = new int[1];
			for(int i = 0; i < attributes.length; i++) 
			{
				int attribute = attributes[i];
				String name = names[i];
				if( egl.eglGetConfigAttrib(display, config, attribute, value)) 
				{
					Log.w(TAG, String.format("  %s: %d\n", name, value[0]));
				} 
				else
				{
					// Log.w(TAG, String.format("  %s: failed\n", name));
					while (egl.eglGetError() != EGL10.EGL_SUCCESS)
					{
					}
				}
			}
		}
    }

	/////////////////////////////////////////////

	public static class GameAppGLRenderer implements GLSurfaceView.Renderer 
	{
		GameAppGLSurfaceView mGameAppGLSurfaceView;

		//
	
		public GameAppGLRenderer(GameAppGLSurfaceView gameAppGLSurfaceView)
		{
			mGameAppGLSurfaceView = gameAppGLSurfaceView;
		}

		public void onDrawFrame(GL10 gl)
		{
			if(!mGameAppNativeBridge.IsEngineInitialized())
				mGameAppNativeBridge.Init();

			if(!mGameAppNativeBridge.IsNeedExit() && !mGameAppNativeBridge.IsFinished())
			{
				mGameAppGLSurfaceView.SendMultithreadedMessages();
				mGameAppNativeBridge.SendWindowMessage(GameAppNativeBridge.AndroidWindowMessages_Paint, 0, 0);
			}

			//!!!!!!xx xx;
			if(mGameAppNativeBridge.IsNeedExit())
			{
				if(mGameApp != null)
					mGameApp.finish();
			}
		}

		public void onSurfaceChanged(GL10 gl, int width, int height) 
		{
			mGameAppNativeBridge.SetClientAreaSize(width, height);
		}

		public void onSurfaceCreated(GL10 gl, EGLConfig config) 
		{		
			mGameAppNativeBridge.SetClientAreaSize(instance.getWidth(), instance.getHeight());
		}       		
	}
}
