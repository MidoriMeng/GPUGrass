using UnityEngine;

public partial class TOD_Sky : MonoBehaviour
{
	protected void OnEnable()
	{
		Components = GetComponent<TOD_Components>();
		Components.Initialize();

		LateUpdate();

		instances.Add(this);

		Initialized = true;
	}

	protected void OnDisable()
	{
		instances.Remove(this);
	}

	protected void LateUpdate()
	{
		#if UNITY_EDITOR
		Cycle.CheckRange();
		Atmosphere.CheckRange();
		Stars.CheckRange();
		Day.CheckRange();
		Night.CheckRange();
		Sun.CheckRange();
		Moon.CheckRange();
		Light.CheckRange();
		Clouds.CheckRange();
		World.CheckRange();
		Fog.CheckRange();
		Ambient.CheckRange();
		Reflection.CheckRange();
		#endif

		SetupQualitySettings();
		SetupSunAndMoon();
		SetupScattering();
		SetupRenderSettings();
		SetupShaderProperties();
	}
}
