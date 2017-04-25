Shader "Custom/Stratus" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
 _Density ("Density", Range(0.0, 1.0)) = 0.8
 _CloudSize ("Cloud Size", Float) = 150000.0
}

SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
 LOD 200
 Cull Back

CGPROGRAM
#pragma surface surf PassThrough alpha vertex:vert
#include "UnityCG.cginc"

#define CONTRAST 0.25
#define BRIGHTNESS 0.4

sampler2D _MainTex;
float4 _Color;
float _Density;
float _CloudSize;

struct Input {
 float2 uv_MainTex;
 float fog;
};

void vert (inout appdata_full v, out Input data)
{
 data.fog = (length(v.vertex) / (_CloudSize * 0.5f));
 data.uv_MainTex = v.texcoord;
}

half4 LightingPassThrough (SurfaceOutput s, half3 lightDir, half atten) {
    half NdotL = dot (half3(0,1,0), lightDir);
    half4 c;
    c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
    c.a = s.Alpha;
    return c;
}

void surf (Input IN, inout SurfaceOutput o) {

 float src = tex2D(_MainTex, IN.uv_MainTex).x;

 float height = src * CONTRAST + (1.0 - CONTRAST);
 height *= BRIGHTNESS;
 float alpha = 1.0;

 if (src > _Density) {
   alpha = 0.0;
 } else {
   float dAlpha = (_Density - src) / _Density;
   dAlpha = pow(0.90, dAlpha * 255.0);
   alpha = 1.0 - dAlpha;
   clamp(alpha, 0.0, 1.0);
 }

 float4 c = float4(height, height, height, alpha) * _Color;
 o.Albedo = c.rgb;
 o.Alpha = lerp(c.a, 0.0, IN.fog);
}
ENDCG
}

Fallback "Transparent/Diffuse"
}

