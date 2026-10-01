Shader "Custom/Main" 
{
	Properties
	{
		_MainTex ("Diffuse", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_SpecularMap ("Specular Map", 2D) = "white" {}
		_Roughness ("Gloss", Range(0.01,1)) = 0.01
		_EmissionMap("Emission Map", 2D) = "black" {}
		_EmissionColor ("Emission Color", Color) = (0,0,0,1)
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
 
		CGPROGRAM
		#pragma surface surf StandardSpecular fullforwardshadows
		#pragma target 3.0
 
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _SpecularMap;
		sampler2D _EmissionMap;
		half _Roughness;
        fixed4 _EmissionColor;
 
		struct Input 
		{
			float2 uv_MainTex;
		};
 
		void surf (Input IN, inout SurfaceOutputStandardSpecular o) 
		{
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Specular = tex2D(_SpecularMap, IN.uv_MainTex);
			o.Smoothness = _Roughness;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
            o.Emission = _EmissionColor * tex2D(_EmissionMap, IN.uv_MainTex);
		}
		ENDCG
	}
	Fallback "Diffuse"
}