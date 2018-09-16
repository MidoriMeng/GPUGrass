#ifndef TOD_SCATTERING_INCLUDED
#define TOD_SCATTERING_INCLUDED

inline float3 L(float3 viewdir, float3 sundir) {
	float3 res;

	// Angle between sun and viewdir
	float cosTheta = max(0, dot(viewdir, sundir));

	// Angular dependency
	// See [3] page 2 equation (2) and (4)
	float angular = (1 + cosTheta*cosTheta);

	// Rayleigh and mie scattering factors
	// See [3] page 2 equation (3) and (4)
	float3 betaTheta = TOD_BetaRayleighTheta
	+ TOD_BetaMieTheta / pow(TOD_BetaMiePhase.x - TOD_BetaMiePhase.y * cosTheta, 1.5);

	// Scattering solution
	// See [5] page 11
	res = angular * betaTheta * TOD_OneOverBeta;

	return res;
}

inline float3 L() {
	return TOD_BetaNight;
}

inline float3 T(float height) {
	float3 res;

	// Parameter value
	// See [7] page 70 equation (5.7)
	float h = clamp(abs(height + TOD_Horizon), 0.01, 1.0);
	float f = pow(h, TOD_Haziness);

	// Optical depth integral approximation
	// See [7] page 71 equation (5.8)
	// See [7] page 71 equation (5.10)
	// See [7] page 76 equation (6.1)
	float sh = (1 - f) * 190000;
	float sr = sh + f * (TOD_OpticalDepth.x - sh);
	float sm = sh + f * (TOD_OpticalDepth.y - sh);

	// Rayleigh and mie scattering factors
	// See [3] page 2 equation (1) and (2)
	float3 beta = TOD_BetaRayleigh * sr
	+ TOD_BetaMie * sm;

	// Scattering solution
	// See [5] page 11
	res = exp(-beta);

	return res;
}

inline float4 ScatteringColor(float3 dir) {
	float4 color = float4(0,0,0,1);

	#ifdef UNITY_PASS_FORWARDADD
	return color;
	#else
	// Scattering values
	float3 T_val  = T(dir.y);
	float3 E_sun  = TOD_SunColor;
	float3 E_moon = TOD_MoonColor;
	float3 L_sun  = L(-dir, TOD_LocalSunDirection);
	float3 L_moon = L();

	// Add scattering color
	color.rgb = (1-T_val) * (E_sun*L_sun + E_moon*L_moon);

	// Add simple moon halo
	color.rgb += TOD_MoonHaloColor * pow(max(0, dot(TOD_LocalMoonDirection, -dir)), TOD_MoonHaloPower);

	// Add additive color
	color.rgb += TOD_AdditiveColor;

	// Add fog color
	color.rgb = lerp(color.rgb, TOD_CloudColor, TOD_Fogginess);

	// Adjust output color according to gamma and contrast value
	color.rgb = pow(color.rgb, TOD_Contrast);

	return color;
	#endif
}

//
// Duplicates with the addition of a distance parameter
//

inline float3 T(float height, float dist) {
	float3 res;

	// Parameter value
	// See [7] page 70 equation (5.7)
	float h = clamp(abs(height + TOD_Horizon), 0.01, 1.0);
	float f = pow(h, TOD_Haziness);

	// Optical depth integral approximation
	// See [7] page 71 equation (5.8)
	// See [7] page 71 equation (5.10)
	// See [7] page 76 equation (6.1)
	float sh = (1 - f) * 190000;
	float sr = sh + f * (TOD_OpticalDepth.x - sh);
	float sm = sh + f * (TOD_OpticalDepth.y - sh);

	// Rayleigh and mie scattering factors
	// See [3] page 2 equation (1) and (2)
	float3 beta = TOD_BetaRayleigh * sr
	+ TOD_BetaMie * sm;

	// Multiply with distance
	beta *= dist;

	// Scattering solution
	// See [5] page 11
	res = exp(-beta);

	return res;
}

inline float4 ScatteringColor(float3 dir, float dist) {
	float4 color = float4(0,0,0,1);

	#ifdef UNITY_PASS_FORWARDADD
	return color;
	#else
	// Scattering values
	float3 T_val  = T(dir.y, dist);
	float3 E_sun  = TOD_SunColor;
	float3 E_moon = TOD_MoonColor;
	float3 L_sun  = L(-dir, TOD_LocalSunDirection);
	float3 L_moon = L();

	// Add scattering color
	color.rgb = (1-T_val) * (E_sun*L_sun + E_moon*L_moon);

	// Add simple moon halo
	color.rgb += TOD_MoonHaloColor * pow(max(0, dot(TOD_LocalMoonDirection, -dir)), TOD_MoonHaloPower);

	// Add additive color
	color.rgb += TOD_AdditiveColor;

	// Add fog color
	color.rgb = lerp(color.rgb, TOD_CloudColor, TOD_Fogginess);

	// Adjust output color according to gamma and contrast value
	color.rgb = pow(color.rgb, TOD_Contrast);

	return color;
	#endif
}

#endif
