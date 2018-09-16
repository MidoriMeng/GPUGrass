using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RenderAtDay : MonoBehaviour
{
	public TOD_Sky sky;

	private Renderer rendererComponent;

	protected void OnEnable()
	{
		if (!sky) sky = TOD_Sky.Instance;

		rendererComponent = GetComponent<Renderer>();
	}

	protected void Update()
	{
		rendererComponent.enabled = sky.IsDay;
	}
}
