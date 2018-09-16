using UnityEngine;
using System;

/// Time iteration class.
///
/// Component of the sky dome parent game object.

public class TOD_Time : MonoBehaviour
{
	/// Day length inspector variable.
	/// Length of one day in minutes.
	public float DayLengthInMinutes = 30;

	/// Adjust the time progress according to the time curve.
	public bool UseTimeCurve = false;

	/// Time of day progression curve.
	/// Can be used to make days longer and nights shorter.
	public AnimationCurve TimeCurve = AnimationCurve.Linear(0, 0, 24, 24);

	private TOD_Sky sky;

	/// Apply the time curve to a time span.
	/// \param deltaTime The time span to adjust.
	/// \return The adjusted time span.
	public float ApplyTimeCurve(float deltaTime)
	{
		float hour = sky.Cycle.Hour;
		float curveFactor = TimeCurve.Evaluate(hour + 0.5f) - TimeCurve.Evaluate(hour - 0.5f);
		return deltaTime * curveFactor;
	}

	/// Add hours and fractions of hours to the current time.
	/// \param hours The hours to add.
	/// \param adjust Whether or not to apply the time curve.
	public void AddHours(float hours, bool adjust = true)
	{
		if (UseTimeCurve && adjust) hours = ApplyTimeCurve(hours);

		sky.Cycle.DateTime = sky.Cycle.DateTime.AddHours(hours);
	}

	/// Add seconds and fractions of seconds to the current time.
	/// \param seconds The seconds to add.
	/// \param adjust Whether or not to apply the time curve.
	public void AddSeconds(float seconds, bool adjust = true)
	{
		if (UseTimeCurve && adjust) seconds = ApplyTimeCurve(seconds);

		sky.Cycle.DateTime = sky.Cycle.DateTime.AddSeconds(seconds);
	}

	protected void Awake()
	{
		sky = GetComponent<TOD_Sky>();
	}

	protected void FixedUpdate()
	{
		const float oneDayInMinutes = 60 * 24;

		float timeFactor = oneDayInMinutes / DayLengthInMinutes;

		AddSeconds(Time.deltaTime * timeFactor);
	}
}
