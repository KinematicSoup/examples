Shader "Custom/PlayerShip"
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_TeamColorMap ("Team Color Map", 2D) = "white" {}
		_TeamColor ("Team Color", Color) = (1,1,1,1)
		_SpecularMap ("Specular Map", 2D) = "white" {}
		_Roughness ("Gloss",Range(0.01,1)) = 0.01
		_ShieldStrength ("Shield Strength",Range(0,2)) = 0
		_ShieldColor ("Shield Color", 2D) = "white" {}
		_ShieldRim ("Shield Rim",Range(0.01,3)) = 0.5
		_ShieldMin ("Shield Min",Range(0,1)) = 0.5
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
 
		CGPROGRAM
		#pragma surface surf StandardSpecular fullforwardshadows
		#pragma target 3.0
 
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _EmissionMap;
		sampler2D _TeamColorMap;
		sampler2D _SpecularMap;
		sampler2D _ShieldColor;
		fixed3 _TeamColor;
		fixed3 _EmissionColor;
		half _ShieldStrength;
		half _Roughness;
		half _ShieldRim;
		half _ShieldMin;
 
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_ShieldColor;
			float3 viewDir;
		};
 
		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			fixed teamColor = tex2D(_TeamColorMap, IN.uv_MainTex);
			half shieldBlend = _ShieldStrength * ((1 - _ShieldMin) * pow((1.0 - saturate(dot (normalize(IN.viewDir), o.Normal))), _ShieldRim) + _ShieldMin);
			
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * (1 - Luminance(teamColor)) + (teamColor * _TeamColor);
			o.Specular = tex2D(_SpecularMap, IN.uv_MainTex);
			o.Smoothness = _Roughness;

			o.Emission = _EmissionColor * tex2D(_EmissionMap, IN.uv_MainTex) + tex2D(_ShieldColor, IN.uv_ShieldColor) * shieldBlend;
		}
		ENDCG
	}
	FallBack "Specular"
}