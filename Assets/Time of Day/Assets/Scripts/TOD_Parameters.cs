#if UNITY_3_0||UNITY_3_1||UNITY_3_2||UNITY_3_3||UNITY_3_4||UNITY_3_5||UNITY_3_6||UNITY_3_7||UNITY_3_8||UNITY_3_9
#define UNITY_3
#endif

#if UNITY_4_0||UNITY_4_1||UNITY_4_2||UNITY_4_3||UNITY_4_4||UNITY_4_5||UNITY_4_6||UNITY_4_7||UNITY_4_8||UNITY_4_9
#define UNITY_4
#endif

using UnityEngine;
using System;
using UnityEngine.Rendering;

/// All parameters of the sky dome.
[Serializable] public class TOD_Parameters
{
	public TOD_CycleParameters      Cycle;
	public TOD_WorldParameters      World;
	public TOD_AtmosphereParameters Atmosphere;
	public TOD_DayParameters        Day;
	public TOD_NightParameters      Night;
	public TOD_SunParameters        Sun;
	public TOD_MoonParameters       Moon;
	public TOD_LightParameters      Light;
	public TOD_StarParameters       Stars;
	public TOD_CloudParameters      Clouds;
	public TOD_FogParameters        Fog;
	public TOD_AmbientParameters    Ambient;
	public TOD_ReflectionParameters Reflection;

	public TOD_Parameters()
	{
	}

	public TOD_Parameters(TOD_Sky sky)
	{
		Cycle      = sky.Cycle;
		World      = sky.World;
		Atmosphere = sky.Atmosphere;
		Day        = sky.Day;
		Night      = sky.Night;
		Sun        = sky.Sun;
		Moon       = sky.Moon;
		Light      = sky.Light;
		Stars      = sky.Stars;
		Clouds     = sky.Clouds;
		Fog        = sky.Fog;
		Ambient    = sky.Ambient;
		Reflection = sky.Reflection;
	}

	public void ToSky(TOD_Sky sky)
	{
		sky.Cycle      = Cycle;
		sky.World      = World;
		sky.Atmosphere = Atmosphere;
		sky.Day        = Day;
		sky.Night      = Night;
		sky.Sun        = Sun;
		sky.Moon       = Moon;
		sky.Light      = Light;
		sky.Stars      = Stars;
		sky.Clouds     = Clouds;
		sky.Fog        = Fog;
		sky.Ambient    = Ambient;
		sky.Reflection = Reflection;
	}
}

/// Parameters of the day and night cycle.
[Serializable] public class TOD_CycleParameters
{
	/// [0, 24]
	/// Time of the day in hours.
	/// \n = 0 at the start of the day.
	/// \n = 12 at noon.
	/// \n = 24 at the end of the day.
	public float Hour = 12;

	/// [1, 28-31]
	/// Current day of the month.
	public int Day = 15;

	/// [1, 12]
	/// Current month of the year.
	public int Month = 6;

	/// [1, 9999]
	/// Current year.
	public int Year = 2000;

	/// All time information as a System.DateTime instance.
	public System.DateTime DateTime
	{
		get
		{
			var res = new DateTime(0, DateTimeKind.Utc);
			return res.AddYears(Year-1).AddMonths(Month-1).AddDays(Day-1).AddHours(Hour);
		}
		set
		{
			Year  = value.Year;
			Month = value.Month;
			Day   = value.Day;
			Hour  = value.Hour + value.Minute / 60f + value.Second / 3600f + value.Millisecond / 3600000f;
		}
	}

	/// All time information as a single long.
	/// Value corresponds to the System.DateTime.Ticks property.
	public long Ticks
	{
		get
		{
			return DateTime.Ticks;
		}
		set
		{
			DateTime = new System.DateTime(value, DateTimeKind.Utc);
		}
	}

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		DateTime = DateTime;
	}
}

/// Parameters of the world.
[Serializable] public class TOD_WorldParameters
{
	/// The zero / water level of the scene in world space.
	/// The horizon offset is automatically adjusted whenever the sky dome is above this level.
	public float ZeroLevel = 0;

	/// [-90, 90]
	/// Latitude of your position in degrees.
	/// \n = -90 at the south pole.
	/// \n = 0 at the equator.
	/// \n = 90 at the north pole.
	public float Latitude = 0;

	/// [-180, 180]
	/// Longitude of your position in degrees.
	/// \n = -180 at 180 degrees in the west of Greenwich, England.
	/// \n = 0 at Greenwich, England.
	/// \n = 180 at 180 degrees in the east of Greenwich, England.
	public float Longitude = 0;

	/// UTC/GMT time zone of the current location.
	/// \n = 0 for Greenwich, England.
	public float UTC = 0;

	/// Type of the horizon offset.
	public TOD_HorizonType Horizon = TOD_HorizonType.Static;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		Longitude = Mathf.Clamp(Longitude, -180, 180);
		Latitude  = Mathf.Clamp(Latitude, -90, 90);
	}
}

/// Parameters of the atmosphere.
[Serializable] public class TOD_AtmosphereParameters
{
	/// Artistic value to shift the scattering color of the atmosphere.
	/// Can be used to easily simulate alien worlds.
	public Color ScatteringColor = Color.white;

	/// [0, &infin;]
	/// Intensity of the atmospheric Rayleigh scattering.
	/// Generally speaking this resembles the static scattering.
	public float RayleighMultiplier = 1.0f;

	/// [0, &infin;]
	/// Intensity of the atmospheric Mie scattering.
	/// Generally speaking this resembles the angular scattering.
	public float MieMultiplier = 1.0f;

	/// [0, &infin;]
	/// Brightness of the atmosphere.
	/// This is being applied as a simple multiplier to the output color.
	public float Brightness = 1.0f;

	/// [0, &infin;]
	/// Contrast of the atmosphere.
	/// This is being applied as a power of the output color.
	public float Contrast = 1.0f;

	/// [0, 1]
	/// Directionality factor that determines the size and sharpness of the glow around the light source.
	public float Directionality = 0.65f;

	/// [0, 1]
	/// Intensity of the haziness of the sky at the horizon.
	public float Haziness = 0.5f;

	/// [0, 1]
	/// Density of the fog covering the sky.
	/// This does not affect the RenderSettings fog that is being applied to other objects in the scene.
	public float Fogginess = 0.0f;

	/// [0, 1]
	/// Amount of fake HDR at sunset.
	public float FakeHDR = 0.5f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		MieMultiplier      = Mathf.Max(0, MieMultiplier);
		RayleighMultiplier = Mathf.Max(0, RayleighMultiplier);
		Brightness         = Mathf.Max(0, Brightness);
		Contrast           = Mathf.Max(0, Contrast);
		Directionality     = Mathf.Clamp01(Directionality);
		Haziness           = Mathf.Clamp01(Haziness);
		Fogginess          = Mathf.Clamp01(Fogginess);
		FakeHDR            = Mathf.Clamp01(FakeHDR);
	}
}

/// Parameters that are unique to the day.
[Serializable] public class TOD_DayParameters
{
	/// Artistic value for an additive color at day.
	public Color AdditiveColor = new Color32(0, 0, 0, 255);

	/// Color of the ambient light at day.
	public Color AmbientColor = new Color32(20, 25, 30, 255);

	/// Color of the clouds at night.
	public Color CloudColor = new Color32(255, 255, 255, 255);

	/// [0, 1]
	/// Sky opacity multiplier at day.
	public float SkyMultiplier = 1.0f;

	/// [0, 1]
	/// Cloud tone multiplier at day.
	public float CloudMultiplier = 1.0f;

	/// [0, &infin;]
	/// Brightness of ambient light at day.
	public float AmbientMultiplier = 1.0f;

	/// [0, &infin;]
	/// Brightness of reflected light at day.
	public float ReflectionMultiplier = 1.0f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		SkyMultiplier        = Mathf.Clamp01(SkyMultiplier);
		CloudMultiplier      = Mathf.Clamp01(CloudMultiplier);
		AmbientMultiplier    = Mathf.Max(0, AmbientMultiplier);
		ReflectionMultiplier = Mathf.Max(0, ReflectionMultiplier);
	}
}

/// Parameters that are unique to the night.
[Serializable] public class TOD_NightParameters
{
	/// Artistic value for an additive color at night.
	public Color AdditiveColor = new Color32(0, 0, 0, 255);

	/// Color of the ambient light at night.
	public Color AmbientColor = new Color32(0, 0, 0, 255);

	/// Color of the clouds at night.
	public Color CloudColor = new Color32(47, 73, 137, 255);

	/// [0, 1]
	/// Sky opacity multiplier at night.
	public float SkyMultiplier = 0.01f;

	/// [0, 1]
	/// Cloud tone multiplier at night.
	public float CloudMultiplier = 0.01f;

	/// [0, &infin;]
	/// Brightness of ambient light at night.
	public float AmbientMultiplier = 1.0f;

	/// [0, &infin;]
	/// Brightness of reflected light at night.
	public float ReflectionMultiplier = 1.0f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		SkyMultiplier        = Mathf.Clamp01(SkyMultiplier);
		CloudMultiplier      = Mathf.Clamp01(CloudMultiplier);
		AmbientMultiplier    = Mathf.Max(0, AmbientMultiplier);
		ReflectionMultiplier = Mathf.Max(0, ReflectionMultiplier);
	}
}

/// Parameters that are unique to the sun.
[Serializable] public class TOD_SunParameters
{
	/// Color of the light emitted by the sun.
	public Color LightColor = new Color32(255, 243, 234, 255);

	/// Color of the sun material.
	public Color MeshColor = new Color32(255, 160, 25, 255);

	/// Color of the god rays cast by the sun.
	public Color RayColor = new Color32(255, 243, 234, 255);

	/// [0, &infin;]
	/// Size of the sun mesh in degrees.
	public float MeshSize = 1.0f;

	/// [0, &infin;]
	/// Brightness of the sun mesh.
	public float MeshBrightness = 1.0f;

	/// [0, &infin;]
	/// Contrast of the sun mesh.
	public float MeshContrast = 1.0f;

	/// [0, &infin;]
	/// Intensity of the sun light source.
	public float LightIntensity = 1.0f;

	/// [0, 1]
	/// Opacity of the object shadows dropped by the sun light source.
	public float ShadowStrength = 1.0f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		MeshSize       = Mathf.Max(0, MeshSize);
		MeshBrightness = Mathf.Max(0, MeshBrightness);
		MeshContrast   = Mathf.Max(0, MeshContrast);
		LightIntensity = Mathf.Max(0, LightIntensity);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
	}
}

/// Parameters that are unique to the moon.
[Serializable] public class TOD_MoonParameters
{
	/// Color of the light emitted by the moon.
	public Color LightColor = new Color32(181, 204, 255, 255);

	/// Color of the moon material.
	public Color MeshColor = new Color32(255, 233, 200, 255);

	/// Color of the god rays cast by the moon.
	public Color RayColor = new Color32(81, 104, 155, 50);

	/// Color of the moon halo.
	public Color HaloColor = new Color32(15, 20, 30, 25);

	/// [0, &infin;]
	/// Size of the moon halo.
	public float HaloSize = 0.1f;

	/// [0, &infin;]
	/// Size of the moon mesh in degrees.
	public float MeshSize = 1.0f;

	/// [0, &infin;]
	/// Brightness of the moon mesh.
	public float MeshBrightness = 1.0f;

	/// [0, &infin;]
	/// Contrast of the moon mesh.
	public float MeshContrast = 1.0f;

	/// [0, &infin;]
	/// Intensity of the moon light source.
	public float LightIntensity = 0.1f;

	/// [0, 1]
	/// Opacity of the object shadows dropped by the moon light source.
	public float ShadowStrength = 1.0f;

	/// Type of the moon position calculation.
	public TOD_MoonPositionType Position = TOD_MoonPositionType.Realistic;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		HaloSize       = Mathf.Max(0, HaloSize);
		MeshSize       = Mathf.Max(0, MeshSize);
		MeshBrightness = Mathf.Max(0, MeshBrightness);
		MeshContrast   = Mathf.Max(0, MeshContrast);
		LightIntensity = Mathf.Max(0, LightIntensity);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
	}
}

/// Parameters of the light source.
[Serializable] public class TOD_LightParameters
{
	/// Light source position update interval in seconds.
	/// Zero means every frame.
	public float UpdateInterval = 0.0f;

	/// [0, 1]
	/// Controls how low the light source is allowed to go.
	/// \n = -1 light source can go as low as it wants.
	/// \n = 0 light source will never go below the horizon.
	/// \n = +1 light source will never leave zenith.
	public float MinimumHeight = 0.0f;

	/// [0, 1]
	/// Controls how fast the sun color falls off.
	/// This is especially visible during sunset and sunrise.
	public float Falloff = 0.75f;

	/// [0, 1]
	/// Controls how strongly the light color is being affected by sunset and sunrise.
	public float Coloring = 0.75f;

	/// [0, 1]
	/// Controls how strongly the sun color affects the atmosphere color.
	/// This is especially visible during sunset and sunrise.
	public float SkyColoring = 0.5f;

	/// [0, 1]
	/// Controls how strongly the sun color affects the cloud color.
	/// This is especially visible during sunset and sunrise.
	public float CloudColoring = 0.75f;

	/// [0, 1]
	/// Controls how strongly the god ray color is being affected by sunset and sunrise.
	public float RayColoring = 0.75f;

	/// [0, 1]
	/// Controls how strongly the ambient color is being affected by sunset and sunrise.
	public float AmbientColoring = 0.5f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		UpdateInterval  = Mathf.Max(0, UpdateInterval);
		MinimumHeight   = Mathf.Clamp(MinimumHeight, -1, 1);
		Falloff         = Mathf.Clamp01(Falloff);
		Coloring        = Mathf.Clamp01(Coloring);
		SkyColoring     = Mathf.Clamp01(SkyColoring);
		CloudColoring   = Mathf.Clamp01(CloudColoring);
		RayColoring     = Mathf.Clamp01(RayColoring);
		AmbientColoring = Mathf.Clamp01(AmbientColoring);
	}
}

/// Parameters of the stars.
[Serializable] public class TOD_StarParameters
{
	/// [0, &infin;]
	/// Texture tiling of the stars texture.
	/// Determines how often the texture is tiled accross the sky and therefore the size of the stars.
	public float Tiling = 6.0f;

	/// [0, &infin;]
	/// Brightness of the stars.
	public float Brightness = 3.0f;

	/// Type of the stars position calculation.
	public TOD_StarsPositionType Position = TOD_StarsPositionType.Rotating;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		Tiling     = Mathf.Max(0, Tiling);
		Brightness = Mathf.Max(0, Brightness);
	}
}

/// Parameters of the clouds.
[Serializable] public class TOD_CloudParameters
{
	/// [0, &infin;]
	/// Density multiplier of the clouds.
	/// \n = 0 no clouds.
	/// \n > 0 thicker clouds that are less transparent.
	public float Density = 1.0f;

	/// [0, &infin;]
	/// Sharpness multiplier of the clouds.
	/// \n = 0 one giant cloud.
	/// \n > 0 several smaller clouds.
	public float Sharpness = 3.0f;

	/// [0, &infin;]
	/// Brightness multiplier of the clouds.
	/// \n = 0 black clouds.
	/// \n > 0 brighter clouds.
	public float Brightness = 2.0f;

	/// [0, &infin;]
	/// Glow multiplier of the clouds.
	public float Glow = 1.0f;

	/// [0, 1]
	/// Opacity of the cloud shadows.
	public float ShadowStrength = 0.0f;

	/// [1, &infin;]
	/// Scale of the first clouds.
	public Vector2 Scale1 = new Vector2(3, 3);

	/// [1, &infin;]
	/// Scale of the second clouds.
	public Vector2 Scale2 = new Vector2(7, 7);

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		Scale1         = new Vector2(Mathf.Max(1, Scale1.x), Mathf.Max(1, Scale1.y));
		Scale2         = new Vector2(Mathf.Max(1, Scale2.x), Mathf.Max(1, Scale2.y));
		Density        = Mathf.Max(0, Density);
		Sharpness      = Mathf.Max(0, Sharpness);
		Brightness     = Mathf.Max(0, Brightness);
		Glow           = Mathf.Max(0, Glow);
		ShadowStrength = Mathf.Clamp01(ShadowStrength);
	}
}

/// Parameters of the fog mode.
[Serializable] public class TOD_FogParameters
{
	/// Fog color mode.
	public TOD_FogType Mode = TOD_FogType.Color;

	/// [0, 1]
	/// Fog color sampling height.
	/// \n = 0 fog is atmosphere color at horizon.
	/// \n = 1 fog is atmosphere color at zenith.
	public float HeightBias = 0.1f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		HeightBias = Mathf.Clamp01(HeightBias);
	}
}

/// Parameters of the ambient mode.
[Serializable] public class TOD_AmbientParameters
{
	/// Ambient light mode.
	public TOD_AmbientType Mode = TOD_AmbientType.Color;

	/// Refresh interval of the ambient light probe in seconds.
	public float UpdateInterval = 1.0f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		UpdateInterval = Mathf.Max(0, UpdateInterval);
	}
}

/// Parameters of the reflection mode.
[Serializable] public class TOD_ReflectionParameters
{
	/// Reflection cubemap mode.
	public TOD_ReflectionType Mode = TOD_ReflectionType.None;

	#if !UNITY_3 && !UNITY_4

	/// Clear flags to use for the reflection.
	public ReflectionProbeClearFlags ClearFlags = ReflectionProbeClearFlags.Skybox;

	/// Layers to include in the reflection.
	public LayerMask CullingMask = 0;

	#endif

	/// Refresh interval of the reflection cubemap in seconds.
	public float UpdateInterval = 1.0f;

	/// Assures that all parameters are within a reasonable range.
	public void CheckRange()
	{
		UpdateInterval = Mathf.Max(0, UpdateInterval);
	}
}
