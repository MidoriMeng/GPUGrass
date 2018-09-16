using UnityEngine;

/// Utility method class.
///
/// Those utility methods should be part of Unity, but they're not.

public static class TOD_Util
{
	/// Apply inverse gamma curve to color if required.
	/// \param color The color in gamma space.
	/// \returns The input color in the active color space.
	public static Color Linear(Color color)
	{
		return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
	}

	/// Multiply the RGB components of a color.
	/// \param color The color in gamma space.
	/// \param exposure The exposure time of color.
	/// \returns The input color with its RGB components multiplied with exposure.
	public static Color ExpRGB(Color color, float exposure)
	{
		if (exposure == 1) return color;
		return new Color(color.r * exposure, color.g * exposure, color.b * exposure, color.a);
	}

	/// Multiply the RGBA components of a color.
	/// \param color The color in gamma space.
	/// \param exposure The exposure time of color.
	/// \returns The input color with its RGB components multiplied with exposure.
	public static Color ExpRGBA(Color color, float exposure)
	{
		if (exposure == 1) return color;
		return new Color(color.r * exposure, color.g * exposure, color.b * exposure, color.a * exposure);
	}

	/// Power of the RGB components of a color.
	/// \param color The color.
	/// \param power The power.
	/// \returns The input color with its RGB components pow'd by power.
	public static Color PowRGB(Color color, float power)
	{
		if (power == 1) return color;
		return new Color(Mathf.Pow(color.r, power), Mathf.Pow(color.g, power), Mathf.Pow(color.b, power), color.a);
	}

	/// Power of the RGBA components of a color.
	/// \param color The color.
	/// \param power The power.
	/// \returns The input color with its RGBA components pow'd by power.
	public static Color PowRGBA(Color color, float power)
	{
		if (power == 1) return color;
		return new Color(Mathf.Pow(color.r, power), Mathf.Pow(color.g, power), Mathf.Pow(color.b, power), Mathf.Pow(color.a, power));
	}

	/// Inverse of a vector.
	/// \param vector The vector.
	/// \returns The inverse of the input vector.
	public static Vector3 Inverse(Vector3 vector)
	{
		return new Vector3(1f / vector.x, 1f / vector.y, 1f / vector.z);
	}
}
