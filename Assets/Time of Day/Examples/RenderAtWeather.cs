using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RenderAtWeather : MonoBehaviour
{
	public TOD_Sky sky;
	public TOD_WeatherType type;

	private Renderer rendererComponent;

	protected void OnEnable()
	{
		if (!sky) sky = TOD_Sky.Instance;

		rendererComponent = GetComponent<Renderer>();
	}

	protected void Update()
	{
		rendererComponent.enabled = sky.Components.Weather.Weather == type;
	}
}
