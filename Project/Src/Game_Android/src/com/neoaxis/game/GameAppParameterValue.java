// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
package com.neoaxis.game;

public class GameAppParameterValue
{
	public static final int ParameterTypes_Null = 0;
	public static final int ParameterTypes_Integer = 1;
	public static final int ParameterTypes_Float = 2;
	public static final int ParameterTypes_Double = 3;
	public static final int ParameterTypes_Boolean = 4;
	public static final int ParameterTypes_String = 5;

	public int type;
	public int intValue;
	public float floatValue;
	public double doubleValue;
	public boolean booleanValue;
	public String stringValue;
}