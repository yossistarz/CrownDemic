// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "XRay Shaders/HeatVision"
{
	Properties
	{
		_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_Alpha("Alpha", Range(0,1)) = 1.0
	}

	SubShader
	{
		Stencil
		{
			Ref 0
			Comp NotEqual
		}

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"XRay" = "ColoredOutline"
		}

		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#define MAX_HEATPOINTS 10

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD1;
				float4 pixelPos: POSITION1;
				float4 viewPixelPos: POSITION2;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.pixelPos = v.vertex;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				return o;
			}

			float4 _EdgeColor;
			float _Alpha;
			float4 _HeatPoints[MAX_HEATPOINTS];
			float _Intensitys[MAX_HEATPOINTS];
			float _HeatSizes[MAX_HEATPOINTS];
			int _HeatPointsCount;

			fixed4 frag (v2f i) : SV_Target
			{
				float NdotV = 1 - dot(i.normal, i.viewDir) * 1.5;
				
				float minLength = 32000;
				float selectedIntensity = 0;
				float selectedHeatLength = 0;
				
				
				int indx;
				[loop]
				for (indx = 0; indx < _HeatPointsCount; indx++){
				
					float currLength = length(i.pixelPos - float4(_HeatPoints[indx].x,_HeatPoints[indx].y,_HeatPoints[indx].z,1));
					if (currLength <= minLength){
						minLength = currLength;
						selectedIntensity = _Intensitys[indx];
						selectedHeatLength = _HeatSizes[indx];
					}
				}

				float heatIntensity = minLength;//length(i.pixelPos - float4(0,0,0,1));
				float intensity = selectedIntensity * (1.0 - abs(sin(_Time.z)+cos(_Time.z)) * 0.1);

				// heatIntensity = 0 - means Hotest spot.
				fixed3 col = fixed3(
					lerp(0, 1, max(0, 1 - (min(selectedHeatLength, heatIntensity) + 0.1) / selectedHeatLength) * intensity),
					lerp(0, 1, max(0, 1 - (min(selectedHeatLength, heatIntensity) + 0.1 + 0.4 / intensity) / selectedHeatLength) * intensity),
					lerp(1, 0, 1 - min(selectedHeatLength, heatIntensity) / selectedHeatLength  + 0.2)
				);
				
				return (fixed4(col.r, col.g, col.b, 0) * (1 - NdotV) + _EdgeColor * NdotV) * fixed4(1,1,1,0) + fixed4(0,0,0,_Alpha);
			}

			ENDCG
		}
	}
}
