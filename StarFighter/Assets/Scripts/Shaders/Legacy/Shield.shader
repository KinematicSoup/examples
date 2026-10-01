Shader "Custom/SheildLegacy" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Reflections ("Cubemap", CUBE) = "" {}
		_ReflectColor ("Reflection Color", Color) = (1,1,1,1)
		_RimPower ("Rim Power", Range(0.1,3.0)) = 0.5
		_MinAlpha ("Min Alpha", Range(0.01,1.0)) = 0.5
		_WaveAmplitude ("Wave Amplitude", Float) = 0.25
		_WaveFrequency ("Wave Frequency", Float) = 100
		_WaveThickness ("Wave Thickness", Float) = 100.0
	}
	
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" }
		
		CGPROGRAM
		
		#pragma target 3.0
		#pragma glsl
		#pragma surface surf Lambert alpha vertex:vert

		struct Input {
			float2 uv_MainTex;
			float3 worldRefl;
			float3 viewDir;
			float3 position;
		};

		fixed4 _Color;
		samplerCUBE _Reflections;
		fixed4 _ReflectColor;
		fixed _RimPower;
		fixed _MinAlpha;
		half _WaveAmplitude;
		half _WaveFrequency;
		half _WaveThickness;
		
		void vert (inout appdata_full v, out Input o) {
  			UNITY_INITIALIZE_OUTPUT(Input,o);
  			o.position = v.vertex.xyz;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			half positionGradient = IN.position.y / 15;
			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			half transparencyAmount = pow(rim, _RimPower);
			fixed4 reflection = texCUBE (_Reflections, IN.worldRefl) * _ReflectColor;
			
			o.Alpha = ((1 - _MinAlpha) * (_Color.a * transparencyAmount * (_WaveAmplitude * sin(_Time * _WaveFrequency + positionGradient * positionGradient * _WaveThickness) + (1 - _WaveAmplitude)) * (1 - positionGradient)) + _MinAlpha);
			o.Emission = (reflection + _Color);
		}
		ENDCG
	} 
	FallBack Off
}
