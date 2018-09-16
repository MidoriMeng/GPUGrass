using UnityEngine;

/// Weather management class.
///
/// Component of the sky dome parent game object.

public class TOD_Weather : MonoBehaviour
{
	/// Fade time inspector variable.
	/// Time to fade from one weather type to the other.
	public float FadeTime = 10f;

	/// Currently selected cloud type.
	public TOD_CloudType Clouds = TOD_CloudType.Custom;

	/// Currently selected weather type.
	public TOD_WeatherType Weather = TOD_WeatherType.Custom;

	private float cloudBrightnessDefault;
	private float cloudDensityDefault;
	private float atmosphereFogDefault;

	private float cloudBrightness;
	private float cloudDensity;
	private float atmosphereFog;
	private float cloudSharpness;

	private TOD_Sky sky;

	protected void Start()
	{
		sky = GetComponent<TOD_Sky>();

		cloudBrightness = cloudBrightnessDefault = sky.Clouds.Brightness;
		cloudDensity    = cloudDensityDefault    = sky.Clouds.Density;
		atmosphereFog   = atmosphereFogDefault   = sky.Atmosphere.Fogginess;
		cloudSharpness  = sky.Clouds.Sharpness;
	}

	protected void Update()
	{
		if (Clouds == TOD_CloudType.Custom && Weather == TOD_WeatherType.Custom) return;

		switch (Clouds)
		{
			case TOD_CloudType.Custom:
				cloudDensity   = sky.Clouds.Density;
				cloudSharpness = sky.Clouds.Sharpness;
				break;

			case TOD_CloudType.None:
				cloudDensity   = 0.0f;
				cloudSharpness = 1.0f;
				break;

			case TOD_CloudType.Few:
				cloudDensity   = cloudDensityDefault;
				cloudSharpness = 5.0f;
				break;

			case TOD_CloudType.Scattered:
				cloudDensity   = cloudDensityDefault;
				cloudSharpness = 1.0f;
				break;

			case TOD_CloudType.Broken:
				cloudDensity   = cloudDensityDefault;
				cloudSharpness = 0.5f;
				break;

			case TOD_CloudType.Overcast:
				cloudDensity   = cloudDensityDefault;
				cloudSharpness = 0.1f;
				break;
		}

		switch (Weather)
		{
			case TOD_WeatherType.Custom:
				cloudBrightness = sky.Clouds.Brightness;
				atmosphereFog   = sky.Atmosphere.Fogginess;
				break;

			case TOD_WeatherType.Clear:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog   = atmosphereFogDefault;
				break;

			case TOD_WeatherType.Storm:
				cloudBrightness = 0.3f;
				atmosphereFog   = 1.0f;
				break;

			case TOD_WeatherType.Dust:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog   = 0.5f;
				break;

			case TOD_WeatherType.Fog:
				cloudBrightness = cloudBrightnessDefault;
				atmosphereFog   = 1.0f;
				break;
		}

		// FadeTime is not exact as the fade smoothens a little towards the end
		float t = Time.deltaTime / FadeTime;

		sky.Clouds.Brightness    = Mathf.Lerp(sky.Clouds.Brightness,    cloudBrightness, t);
		sky.Clouds.Density       = Mathf.Lerp(sky.Clouds.Density,       cloudDensity,    t);
		sky.Clouds.Sharpness     = Mathf.Lerp(sky.Clouds.Sharpness,     cloudSharpness,  t);
		sky.Atmosphere.Fogginess = Mathf.Lerp(sky.Atmosphere.Fogginess, atmosphereFog,   t);
	}
}
