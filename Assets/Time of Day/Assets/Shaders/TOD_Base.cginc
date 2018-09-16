// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef TOD_BASE_INCLUDED
#define TOD_BASE_INCLUDED

#include "UnityCG.cginc"

uniform float4x4 TOD_World2Sky;
uniform float4x4 TOD_Sky2World;

uniform float TOD_Gamma;
uniform float TOD_OneOverGamma;

uniform float3 TOD_SunColor;
uniform float3 TOD_MoonColor;
uniform float3 TOD_LightColor;
uniform float3 TOD_CloudColor;
uniform float3 TOD_AdditiveColor;
uniform float3 TOD_MoonHaloColor;
uniform float3 TOD_AmbientColor;

uniform float3 TOD_SunDirection;
uniform float3 TOD_MoonDirection;
uniform float3 TOD_LightDirection;

uniform float3 TOD_LocalSunDirection;
uniform float3 TOD_LocalMoonDirection;
uniform float3 TOD_LocalLightDirection;

uniform float TOD_Contrast;
uniform float TOD_Haziness;
uniform float TOD_Horizon;
uniform float TOD_Fogginess;
uniform float TOD_MoonHaloPower;

uniform float2 TOD_OpticalDepth;
uniform float3 TOD_OneOverBeta;
uniform float3 TOD_BetaRayleigh;
uniform float3 TOD_BetaRayleighTheta;
uniform float3 TOD_BetaMie;
uniform float3 TOD_BetaMieTheta;
uniform float2 TOD_BetaMiePhase;
uniform float3 TOD_BetaNight;

#define TOD_TRANSFORM_VERT(vert) UnityObjectToClipPos(vert)

#endif
