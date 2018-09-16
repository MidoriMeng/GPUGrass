using UnityEngine;

/// Component manager class.
///
/// Component of the main camera of the scene.

[ExecuteInEditMode]
public class TOD_Components : MonoBehaviour
{
	/// Sun child game object reference.
	public GameObject Sun = null;

	/// Moon child game object reference.
	public GameObject Moon = null;

	/// Atmosphere child game object reference.
	public GameObject Atmosphere = null;

	/// Clear child game object reference.
	public GameObject Clear = null;

	/// Clouds child game object reference.
	public GameObject Clouds = null;

	/// Space child game object reference.
	public GameObject Space = null;

	/// Light child game object reference.
	public GameObject Light = null;

	/// Projector child game object reference.
	public GameObject Projector = null;

	/// Transform component of the sky dome game object.
	internal Transform DomeTransform;

	/// Transform component of the sun game object.
	internal Transform SunTransform;

	/// Transform component of the moon game object.
	internal Transform MoonTransform;

	/// Transform component of the light source game object.
	internal Transform LightTransform;

	/// Transform component of the space game object.
	internal Transform SpaceTransform;

	/// Renderer component of the space game object.
	internal Renderer SpaceRenderer;

	/// Renderer component of the atmosphere game object.
	internal Renderer AtmosphereRenderer;

	/// Renderer component of the clear game object.
	internal Renderer ClearRenderer;

	/// Renderer component of the cloud game object.
	internal Renderer CloudRenderer;

	/// Renderer component of the sun game object.
	internal Renderer SunRenderer;

	/// Renderer component of the moon game object.
	internal Renderer MoonRenderer;

	/// MeshFilter component of the space game object.
	internal MeshFilter SpaceMeshFilter;

	/// MeshFilter component of the atmosphere game object.
	internal MeshFilter AtmosphereMeshFilter;

	/// MeshFilter component of the clear game object.
	internal MeshFilter ClearMeshFilter;

	/// MeshFilter component of the cloud game object.
	internal MeshFilter CloudMeshFilter;

	/// MeshFilter component of the sun game object.
	internal MeshFilter SunMeshFilter;

	/// MeshFilter component of the moon game object.
	internal MeshFilter MoonMeshFilter;

	/// Main material of the space game object.
	internal Material SpaceMaterial;

	/// Main material of the atmosphere game object.
	internal Material AtmosphereMaterial;

	/// Main material of the clear game object.
	internal Material ClearMaterial;

	/// Main material of the cloud game object.
	internal Material CloudMaterial;

	/// Main material of the sun game object.
	internal Material SunMaterial;

	/// Main material of the moon game object.
	internal Material MoonMaterial;

	/// Main material of the projector game object.
	internal Material ShadowMaterial;

	/// Light component of the light source game object.
	internal Light LightSource;

	/// Projector component of the shadow projector game object.
	internal Projector ShadowProjector;

	/// Sky component of the sky dome game object.
	internal TOD_Sky Sky;

	/// Animation component of the sky dome game object.
	internal TOD_Animation Animation;

	/// Time component of the sky dome game object.
	internal TOD_Time Time;

	/// Weather component of the sky dome game object.
	internal TOD_Weather Weather;

	/// Resource container component of the sky dome game object.
	internal TOD_Resources Resources;

	/// God ray component of the camera game object if available.
	internal TOD_Rays Rays;

	/// Initializes all component references
	public void Initialize()
	{
		DomeTransform = GetComponent<Transform>();

		Sky       = GetComponent<TOD_Sky>();
		Animation = GetComponent<TOD_Animation>();
		Time      = GetComponent<TOD_Time>();
		Weather   = GetComponent<TOD_Weather>();
		Resources = GetComponent<TOD_Resources>();

		if (Space)
		{
			SpaceTransform  = Space.GetComponent<Transform>();
			SpaceRenderer   = Space.GetComponent<Renderer>();
			SpaceMaterial   = SpaceRenderer.sharedMaterial;
			SpaceMeshFilter = Space.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Space reference not set.");
		}

		if (Atmosphere)
		{
			AtmosphereRenderer   = Atmosphere.GetComponent<Renderer>();
			AtmosphereMaterial   = AtmosphereRenderer.sharedMaterial;
			AtmosphereMeshFilter = Atmosphere.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Atmosphere reference not set.");
		}

		if (Clear)
		{
			ClearRenderer   = Clear.GetComponent<Renderer>();
			ClearMaterial   = ClearRenderer.sharedMaterial;
			ClearMeshFilter = Clear.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Clear reference not set.");
		}

		if (Clouds)
		{
			CloudRenderer   = Clouds.GetComponent<Renderer>();
			CloudMaterial   = CloudRenderer.sharedMaterial;
			CloudMeshFilter = Clouds.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Clouds reference not set.");
		}

		if (Projector)
		{
			ShadowProjector = Projector.GetComponent<Projector>();
			ShadowMaterial  = ShadowProjector.material;
		}
		else
		{
			Debug.LogError("Projector reference not set.");
		}

		if (Light)
		{
			LightTransform = Light.GetComponent<Transform>();
			LightSource    = Light.GetComponent<Light>();
		}
		else
		{
			Debug.LogError("Light reference not set.");
		}

		if (Sun)
		{
			SunTransform  = Sun.GetComponent<Transform>();
			SunRenderer   = Sun.GetComponent<Renderer>();
			SunMaterial   = SunRenderer.sharedMaterial;
			SunMeshFilter = Sun.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Sun reference not set.");
		}

		if (Moon)
		{
			MoonTransform  = Moon.GetComponent<Transform>();
			MoonRenderer   = Moon.GetComponent<Renderer>();
			MoonMaterial   = MoonRenderer.sharedMaterial;
			MoonMeshFilter = Moon.GetComponent<MeshFilter>();
		}
		else
		{
			Debug.LogError("Moon reference not set.");
		}
	}
}
