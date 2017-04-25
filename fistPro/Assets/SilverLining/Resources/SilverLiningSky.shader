// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

Shader "Custom/SilverLining Sky" {
	
SubShader {

    Tags { "Queue"="Background" "RenderType"="Background" }
    Cull Off ZWrite Off Fog { Mode Off }
    
    Pass {
	
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

uniform float4 theColor;

uniform float4x4 XYZtoRGB;
uniform float3 xPerezABC;
uniform float3 xPerezDE;
uniform float3 yPerezABC;
uniform float3 yPerezDE;
uniform float3 YPerezABC;
uniform float3 YPerezDE;
uniform float3 sunPerez;
uniform float3 zenithPerez;
uniform float3 sunPos;
uniform float3 luminanceScales;
uniform float4 kAndLdmax;
uniform float4 overcast;
uniform float4 fog;

uniform float4 HosekRadiances;
uniform float3 XHosekABC;
uniform float3 XHosekDEF;
uniform float3 XHosekGHI;
uniform float3 YHosekABC;
uniform float3 YHosekDEF;
uniform float3 YHosekGHI;
uniform float3 ZHosekABC;
uniform float3 ZHosekDEF;
uniform float3 ZHosekGHI;

struct v2f {
    float4 pos : SV_POSITION;
    float3 color : COLOR0;
};

float Perez(float3 ABC, float3 DE, float costheta, float gamma, float cosGamma)
{
    float perez = (1.0 + ABC.x * exp(ABC.y / costheta)) *
                  (1.0 + ABC.z * exp(DE.x * gamma) + DE.y * cosGamma * cosGamma);

    return perez;
}

float HosekWilkie(float3 ABC, float3 DEF, float3 GHI, float costheta, float gamma, float cosGamma)
{
    float expM = exp(DEF.y * gamma);
    float rayM = cosGamma * cosGamma;
    float mieM = (1.0 + cosGamma * cosGamma) / pow((1.0 + GHI.z * GHI.z - 2.0 * GHI.z * cosGamma), 1.5);
    float zenith = sqrt(costheta);

    return (1.0 + ABC.x * exp(ABC.y / (costheta + 0.01))) *
            (ABC.z + DEF.x * expM + DEF.z * rayM + GHI.x * mieM + GHI.y * zenith);
}

float3 XYZtoxyY(float3 XYZ)
{
	float3 xyY;
	float den = (XYZ.x + XYZ.y + XYZ.z);
	xyY.x = XYZ.x / den;
	xyY.y = XYZ.y / den;
	xyY.z = XYZ.y;

	return xyY;
}

float3 toneMap(float3 xyY, float mC, float mR, float k, float Ldmax)
{
    // This is based on Durand's operator, which is based on Ferwerda
    // which is based on Ward...

    // Convert Kcd/m2 to cd/m2
    float Y = xyY.z * 1000.0;
    
    Y = max(Y, 0.0);

    float3 XYZ;
    float R;
    // Convert xyY to XYZ
    XYZ.x = xyY.x * (xyY.z / xyY.y);
    XYZ.y = xyY.z;
    XYZ.z = (1.0 - xyY.x - xyY.y) * (xyY.z / xyY.y);

    const float3 scotopic = float3(-0.702, 1.039, 0.433);
    R = dot(XYZ, scotopic);

    // Uncomment to apply perceptual blue-shift in scotopic conditions (Durand)
    //const float3 blue = float3(0.3, 0.3, 1);
    //xyY = lerp(xyY, blue, k);

    // Apply the photopic and scotopic scales that were precomputed in
    // the app per Ferwerda and Durand's operator
    float Ldp = Y * mC;
    float Lds = R * mR;

    Y = Ldp + k * Lds;
    // Normalize to display range
    Y = Y / Ldmax;

    xyY.z = Y;

    return xyY;
}

float4 xyYtoRGB(float3 xyY, float4x4 XYZtoRGB, float oneOverGamma)
{
    float4 XYZ, RGB;

    // Convert xyY to XYZ
    XYZ.x = xyY.x * (xyY.z / xyY.y);
    XYZ.y = xyY.z;
    XYZ.z = (1.0 - xyY.x - xyY.y) * (xyY.z / xyY.y);
    XYZ.w = 1.0;

    // Convert XYZ to RGB
    RGB = mul(XYZ, XYZtoRGB);

    // Scale down if necessary, preserving color
    float f = max(RGB.y, RGB.z);
    f = max(RGB.x, f); // f is now largest color component
    f = max(1.0, f); // if f is less than 1.0, set to 1.0 (to do nothing)

    RGB.xyz = RGB.xyz / f;

    RGB = pow(RGB, oneOverGamma); // apply gamma correction

    RGB.w = 1.0;

    RGB = saturate(RGB);

    return RGB;
}

v2f vert (appdata_base v)
{
    v2f o;
    float4x4 mv = UNITY_MATRIX_MV;
    mv[0][3] = mv[1][3] = mv[2][3] = 0.0;
    float4x4 mvp = mul(UNITY_MATRIX_P, mv); 
    o.pos = mul (mvp, v.vertex);
    
    float3 xyY;
    float3 xyYo = float3(0.310, 0.316, 0);

    // Find gamma angles
    float3 normalPos = normalize(v.vertex.xyz).xyz;
    float cosGammaS = dot(sunPos, normalPos);
    float gammaS = acos(cosGammaS);
    float costheta = dot(float3(0,1,0), normalPos);
    const float epsilon = 1E-3f;
    if (costheta <= 0) costheta = epsilon;

	// Evaluate Hosek-Wilkie functions for sun
	float3 XYZs;
	XYZs.x = HosekWilkie(XHosekABC, XHosekDEF, XHosekGHI, costheta, gammaS, cosGammaS) * HosekRadiances.x;
	XYZs.y = HosekWilkie(YHosekABC, YHosekDEF, YHosekGHI, costheta, gammaS, cosGammaS) * HosekRadiances.y;
	XYZs.z = HosekWilkie(ZHosekABC, ZHosekDEF, ZHosekGHI, costheta, gammaS, cosGammaS) * HosekRadiances.z;
	float3 xyYsHW = XYZtoxyY(XYZs);

	// Evaluate Perez functions for sun
	float3 xyYsP;
    xyYsP.x = Perez(xPerezABC, xPerezDE, costheta, gammaS, cosGammaS);
    xyYsP.y = Perez(yPerezABC, yPerezDE, costheta, gammaS, cosGammaS);
    xyYsP.z = Perez(YPerezABC, YPerezDE, costheta, gammaS, cosGammaS);
    xyYsP = zenithPerez.xyz * (xyYsP / sunPerez.xyz);

	// Blend the two
	float perezBlend = HosekRadiances.w;
	xyY = lerp(xyYsHW, xyYsP, perezBlend);

    float overcastBlend = overcast.y;
    float overcastFactor = (1.0 + 2.0 * costheta) / 3.0;
    xyYo.z = zenithPerez.z * overcastFactor;

    xyY = lerp(xyY, xyYo, overcastBlend);

    xyY = toneMap(xyY, luminanceScales.y, luminanceScales.x, kAndLdmax.x,
                  kAndLdmax.y);
  
    float4 skyColor = xyYtoRGB(xyY, XYZtoRGB, kAndLdmax.z);
        
    float fogDensity = fog.w;
    float volumeDistance = overcast.w;
    float fogDistance = volumeDistance / costheta;
    float f = exp(-(fogDensity * fogDistance));  
          
    o.color = lerp(fog.xyz, skyColor.xyz, f);
    return o;
}

float4 frag (v2f i) : COLOR
{
    return float4(i.color, 1.0);
}


ENDCG

    }
}
Fallback Off
} 