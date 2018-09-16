using UnityEngine;
using System;

public class DeviceTime : MonoBehaviour
{
	public TOD_Sky sky;

	protected void OnEnable()
	{
		if (!sky) sky = TOD_Sky.Instance;

		sky.Cycle.DateTime = DateTime.Now;
	}
}
