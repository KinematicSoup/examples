// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ShieldBlurLegacy" {

    Properties {
		_blurSizeXY ("BlurSizeXY", Range(0,5.0)) = 2.0
		_RimPower ("Rim Power", Range(0.1,3.0)) = 0.5
		_WaveAmplitude ("Wave Amplitude", Float) = 0.5
		_WaveFrequency ("Wave Frequency", Float) = 100.0
		_WaveThickness ("Wave Thickness", Float) = 100.0
		_MinBlur ("Minimun Blur", Range(0,2.0))= 0.5
	}
	
    SubShader {
        Tags { "Queue" = "Transparent+50" }
        
        //Cull Off
        
        GrabPass { }
        
        Pass {
			CGPROGRAM
			
			#pragma multi_compile IS_MOBILE IS_NOT_MOBILE
			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc"

            sampler2D _GrabTexture : register(s0);
            half _blurSizeXY;
			half _RimPower;
			half _WaveAmplitude;
			half _WaveFrequency;
			half _WaveThickness;
			half _MinBlur;
	
			struct vertInput {
    			float4 vertex : POSITION;
    			float3 normal : NORMAL;
			};

			struct vertOutput {
    			float4 position : POSITION;
    			float4 screenPos : TEXCOORD0;
    			half rim  : TEXCOORD1;
				float3 localPosition  : TEXCOORD2;
			};

			vertOutput vert(vertInput i) 
			{
   				vertOutput o;
   				
   				o.position = UnityObjectToClipPos(i.vertex);
    			o.screenPos = o.position;
    			
				float3 viewDir = normalize ( ObjSpaceViewDir(i.vertex) );
				half rim = 1 - saturate ( dot (i.normal, viewDir) );
				o.rim = pow(rim, _RimPower);
  				o.localPosition = i.vertex.xyz;
    			
    			return o;
			}
			
			half4 frag( vertOutput i ) : COLOR
			{	
				half positionGradient = i.localPosition.y / 15;
				half blurFactor = (_WaveAmplitude * sin(_Time.x * _WaveFrequency + positionGradient * positionGradient * _WaveThickness) + (1 - _WaveAmplitude)) * (1 - positionGradient);
    			float2 screenPos = i.screenPos.xy / i.screenPos.w;
				float depth = _blurSizeXY * 0.0005 * i.rim * blurFactor + _MinBlur * 0.0005;

    			screenPos.x = (screenPos.x + 1) * 0.5;
    			
    			#if IS_MOBILE
    			screenPos.y = 1-(1-(screenPos.y + 1) * 0.5);
    			#endif
    			
    			#if IS_NOT_MOBILE
    			screenPos.y = (1-(screenPos.y + 1) * 0.5);
    			#endif
    			
    			half4 sum = half4(0.0h,0.0h,0.0h,0.0h);
    			sum += tex2D( _GrabTexture, float2(screenPos.x-5.0 * depth, screenPos.y+5.0 * depth)) * 0.025;   
    			sum += tex2D( _GrabTexture, float2(screenPos.x+5.0 * depth, screenPos.y-5.0 * depth)) * 0.025;
    			sum += tex2D( _GrabTexture, float2(screenPos.x-4.0 * depth, screenPos.y+4.0 * depth)) * 0.05;
    			sum += tex2D( _GrabTexture, float2(screenPos.x+4.0 * depth, screenPos.y-4.0 * depth)) * 0.05;
		    	sum += tex2D( _GrabTexture, float2(screenPos.x-3.0 * depth, screenPos.y+3.0 * depth)) * 0.09;
    			sum += tex2D( _GrabTexture, float2(screenPos.x+3.0 * depth, screenPos.y-3.0 * depth)) * 0.09;
    			sum += tex2D( _GrabTexture, float2(screenPos.x-2.0 * depth, screenPos.y+2.0 * depth)) * 0.12;
    			sum += tex2D( _GrabTexture, float2(screenPos.x+2.0 * depth, screenPos.y-2.0 * depth)) * 0.12;
    			sum += tex2D( _GrabTexture, float2(screenPos.x-1.0 * depth, screenPos.y+1.0 * depth)) *  0.15;
    			sum += tex2D( _GrabTexture, float2(screenPos.x+1.0 * depth, screenPos.y-1.0 * depth)) *  0.15;

    			sum += tex2D( _GrabTexture, screenPos-5.0 * depth) * 0.025;
    			sum += tex2D( _GrabTexture, screenPos-4.0 * depth) * 0.05;
    			sum += tex2D( _GrabTexture, screenPos-3.0 * depth) * 0.09;
    			sum += tex2D( _GrabTexture, screenPos-2.0 * depth) * 0.12;
    			sum += tex2D( _GrabTexture, screenPos-1.0 * depth) * 0.15;
    			sum += tex2D( _GrabTexture, screenPos) * 0.16; 
    			sum += tex2D( _GrabTexture, screenPos+5.0 * depth) * 0.15;
    			sum += tex2D( _GrabTexture, screenPos+4.0 * depth) * 0.12;
    			sum += tex2D( _GrabTexture, screenPos+3.0 * depth) * 0.09;
    			sum += tex2D( _GrabTexture, screenPos+2.0 * depth) * 0.05;
    			sum += tex2D( _GrabTexture, screenPos+1.0 * depth) * 0.025;
    			
				return sum/2;
			}
			ENDCG
    	}
	}
	Fallback Off
} 