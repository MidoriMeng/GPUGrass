#if UNITY_3_0||UNITY_3_1||UNITY_3_2||UNITY_3_3||UNITY_3_4||UNITY_3_5||UNITY_3_6||UNITY_3_7||UNITY_3_8||UNITY_3_9
#define UNITY_3
#endif

#if UNITY_4_0||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
#define UNITY_4
#endif

using UnityEngine;

/// Moon position types.
public enum TOD_MoonPositionType
{
	OppositeToSun,
	Realistic
}

/// Stars position types.
public enum TOD_StarsPositionType
{
	Static,
	Rotating
}

/// Horizon type.
public enum TOD_HorizonType
{
	Static,
	ZeroLevel
}

/// Fog adjustment types.
public enum TOD_FogType
{
	None,
	Color,
	Directional
}

/// Ambient light types.
public enum TOD_AmbientType
{
#if UNITY_3 || UNITY_4
	None,
	Color
#else
	None,
	Color,
	Gradient,
	Spherical
#endif
}

/// Reflection cubemap types.
public enum TOD_ReflectionType
{
#if UNITY_3 || UNITY_4
	None
#else
	None,
	Cubemap
#endif
}

/// Unity color space detection.
public enum TOD_ColorSpaceDetection
{
	Auto,
	Linear,
	Gamma
}

/// Cloud rendering qualities.
public enum TOD_CloudQualityType
{
	Fastest,
	Density,
	Bumped
}

/// Mesh vertex count levels.
public enum TOD_MeshQualityType
{
	Low,
	Medium,
	High
}

/// Cloud coverage types.
public enum TOD_CloudType
{
	Custom,
	None,
	Few,
	Scattered,
	Broken,
	Overcast
}

/// Weather types.
public enum TOD_WeatherType
{
	Custom,
	Clear,
	Storm,
	Dust,
	Fog
}
