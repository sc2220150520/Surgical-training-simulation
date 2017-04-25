// Copyright (c) 2011 Sundog Software LLC. All rights reserved worldwide.

Shader "Particles/SilverLiningPuff" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend One OneMinusSrcAlpha
	ColorMask RGBA
	Cull Off Lighting Off ZWrite Off Fog { Mode Off }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	// ---- Fragment program cards
	SubShader {
		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_particles

			#include "UnityCG.cginc"

//            #define RAYLEIGH
            #define HENYEYGREENSTEIN
            
			uniform float ambientBoost;
			uniform float3 ambient;
			uniform float3 lightColor;
			uniform float3 lightDir;
			uniform float extinction;
			uniform float4 fog;
			uniform float fade;
            uniform float3 cloudPos;
            uniform float minPhase;
            uniform float maxPhase;

			sampler2D _MainTex;
			float4 _TintColor;
			
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				#endif
			};

			float4 _MainTex_ST;

            float ComputePhaseFunction(float3 worldPos)
            {
                float3 P = worldPos - _WorldSpaceCameraPos;
                P = normalize(P);
                float cosTheta = dot(P, lightDir);
                float phase = 1.0;
#ifdef RAYLEIGH
                phase = 0.75 * (1.0 + cosTheta * cosTheta);
#endif

#ifdef HENYEYGREENSTEIN
                const float hg1 = 0.85;
                const float hg2 = -0.1;
                float f = 0.85;
                float g1sq = hg1 * hg1;
                float g1 = (1.0 - g1sq) / pow((1.0 + g1sq - 2.0 * hg1 * cosTheta), 1.5);
                float g2sq = hg2 * hg2;
                float g2 = (1.0 - g2sq) / pow((1.0 + g2sq - 2.0 * hg2 * cosTheta), 1.5);
                phase = g1 * f + g2 * (1.0 - f);
#endif
                phase = clamp(phase, minPhase, maxPhase);
                return phase;
            }
			
			float4 SetColor(float4 v, float4 color)
			{
				float3 c;

                float3 eyeCoords = mul(UNITY_MATRIX_MV, v).xyz;

		    	float fogDistance = length(eyeCoords.xyz);
			    float fogExponent = clamp(fogDistance * fog.w, 0.0, 1.0);
			    float fogFactor = clamp(exp(-abs(fogExponent)), 0.0, 1.0);

                float phase = ComputePhaseFunction(cloudPos);

//                return float4(phase, phase, phase, 1.0f);

		        c = color.xyz * phase + ambient * ambientBoost;
		        c *= color.w;

				//float maxC = max(c.x, c.y);
				//maxC = max(maxC, c.z);
				//if (maxC > 1.0) c /= maxC;
                c = saturate(c);
				
				float4 preFadedColor = float4(c.r, c.g, c.b, extinction);
		        preFadedColor.xyz = lerp(fog.xyz, preFadedColor.xyz, fogFactor);
				
				float4 outColor = lerp(float4(0, 0, 0, 0), preFadedColor, fade * (1 - fogExponent * fogExponent));

				return float4(outColor.x, outColor.y, outColor.z, outColor.w);
		    }
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				o.color = SetColor(v.vertex, v.color);
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			sampler2D _CameraDepthTexture;
			float _InvFade;
			
			float4 frag (v2f i) : COLOR
			{
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				float partZ = i.projPos.z;
				float softFade = saturate (_InvFade * (sceneZ-partZ));
				i.color *= softFade;
				#endif
				
				float4 tex = tex2D(_MainTex, i.texcoord);
				float4 col;
				col = tex.xxxx * i.color;
				return col;
			}
			ENDCG 
		}
	}
}

Fallback Off
}
