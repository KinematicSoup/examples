// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Per pixel bumped refraction.
// Uses a normal map to distort the image behind, and
// an additional texture to tint the color.

Shader "Custom/SheildBur" 
{
	Properties
	{
		_BumpAmt  ("Distortion", range (0,64)) = 10
		_MinBumpAmt ("Minimum Distortion", Range(0,1.0)) = 0.5
		_RimPower ("Rim Power", Range(0.1,3.0)) = 0.5
		_WaveAmplitude ("Wave Amplitude", Float) = 0.5
		_WaveFrequency ("Wave Frequency", Float) = 100.0
		_WaveThickness ("Wave Thickness", Float) = 100.0
		_Color ("Tint Color", Color) = (1,1,1,1)
		_Reflections ("Cubemap", CUBE) = "" {}
		_ReflectColor ("Reflection Color", Color) = (1,1,1,1)
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}

	Category 
	{
		Tags { "Queue"="Transparent+50" "RenderType"="Opaque" }

		SubShader 
		{
			// This pass grabs the screen behind the object into a texture.
			// We can access the result in the next pass as _GrabTexture
			GrabPass 
			{
				Name "BASE"
				Tags { "LightMode" = "Always" }
			}
		
			// Main pass: Take the texture grabbed above and use the bumpmap to perturb it on to the screen
			Pass 
			{
				Name "BASE"
				Tags { "LightMode" = "Always" }
			
				CGPROGRAM
				#pragma target es3.0
				#pragma glsl
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 texcoord: TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					half4 uvgrab : TEXCOORD0;
					half2 uvbump : TEXCOORD1;
					half3 normalDir : TEXCOORD2;
					half3 viewDir : TEXCOORD3;
					float rim  : TEXCOORD4;
					float3 localPosition  : TEXCOORD5;
				};

				samplerCUBE _Reflections;
				fixed4 _Color;
				fixed4 _ReflectColor;
				half4 _BumpMap_ST;
				half _BumpAmt;
				half _MinBumpAmt;
				half _RimPower;
				half _WaveAmplitude;
				half _WaveFrequency;
				half _WaveThickness;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#if UNITY_UV_STARTS_AT_TOP
					half scale = -1.0;
					#else
					half scale = 1.0;
					#endif
					o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
					o.uvgrab.zw = o.vertex.zw;
					o.uvbump = TRANSFORM_TEX( v.texcoord, _BumpMap );

					float3 viewDir = normalize ( ObjSpaceViewDir(v.vertex) );
					o.viewDir = viewDir;
					o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
					o.rim = pow(1 - saturate ( dot (v.normal, viewDir) ), _RimPower);
  					o.localPosition = v.vertex.xyz;

					return o;
				}

				sampler2D _GrabTexture;
				float4 _GrabTexture_TexelSize;
				sampler2D _BumpMap;

				half4 frag (v2f i) : SV_Target
				{
					half positionGradient = i.localPosition.y / 15;
					half refractStrength = 1 - (sin(_Time.x * _WaveFrequency + positionGradient * positionGradient * _WaveThickness) * (1 - positionGradient));

					// calculate perturbed coordinates
					half2 bump = UnpackNormal(tex2D( _BumpMap, i.uvbump )).rg;
					half2 offset = (refractStrength * bump * _BumpAmt * i.rim) * _GrabTexture_TexelSize.xy;
					i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
					fixed4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
	
					float3 reflectedDir = reflect(i.viewDir, normalize(i.normalDir));
					fixed4 reflection = texCUBE (_Reflections, reflectedDir) * _ReflectColor;
					col += _Color * ((1 - _MinBumpAmt) * (0.4 * refractStrength + 0.6) * i.rim + _MinBumpAmt) + reflection;
					return col;
				}
			ENDCG
			}
		}
	}
}
