using UnityEngine;

public class LoadSkyFromFile : MonoBehaviour
{
	public TOD_Sky sky;

	public TextAsset textAsset = null;

	protected void OnEnable()
	{
		if (!sky) sky = TOD_Sky.Instance;

		if (textAsset) sky.LoadParameters(textAsset.text);
	}
}
