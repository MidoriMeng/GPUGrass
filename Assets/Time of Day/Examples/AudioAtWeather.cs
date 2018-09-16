using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAtWeather : MonoBehaviour
{
	public TOD_Sky sky;
	public TOD_WeatherType type;

	public  float fadeTime = 1;
	private float lerpTime = 0;

	private AudioSource audioComponent;
	private float audioVolume;

	protected void OnEnable()
	{
		if (!sky) sky = TOD_Sky.Instance;

		audioComponent = GetComponent<AudioSource>();
		audioVolume    = audioComponent.volume;

		if (sky.Components.Weather.Weather != type) audioComponent.volume = 0;
	}

	protected void Update()
	{
		int sign = (sky.Components.Weather.Weather == type) ? +1 : -1;
		lerpTime = Mathf.Clamp01(lerpTime + sign * Time.deltaTime / fadeTime);

		audioComponent.volume = Mathf.Lerp(0, audioVolume, lerpTime);
	}
}
