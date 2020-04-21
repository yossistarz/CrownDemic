// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/HeatVision"
{
	Properties
	{
		_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_LightLengthMax("HeatVision Ray Far Range", Float) = 40
		_LightLengthMin("HeatVision Ray Close Range", Float) = 15
		_LightIntensity("HeatVision Ray Intensity", Range(0,10)) = 1
		_LightAngle("HeatVision Ray Angle", Float) = 30
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"XRay" = "ColoredOutline"
		}

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
				float4 worldPixelPos: POSITION2;
				float4 viewPixelPos: POSITION3;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.pixelPos = v.vertex;
				o.worldPixelPos =  mul(unity_ObjectToWorld, v.vertex);
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz);
				return o;
			}

			float4 _EdgeColor;
			
			float _LightLengthMax = 40;
			float _LightLengthMin = 15;
			float _LightIntensity = 1;
			float _LightAngle = 30.0;

			float4 _HeatPoints[MAX_HEATPOINTS];
			float _Intensitys[MAX_HEATPOINTS];
			float _HeatSizes[MAX_HEATPOINTS];
			int _HeatPointsCount;

			float4 _LightPos;
			float4 _LightDirection;

			float getAngle3(float3 v1, float3 v2){
				float dt = dot(v1, v2);
				float angle = acos(dt / (length(v1)*length(v2)));
				return degrees(angle);
			}

			float getAngle4(float4 v1, float4 v2){
				float dt = dot(v1, v2);
				float angle = acos(dt / (length(v1)*length(v2)));
				return degrees(angle);
			}			

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 realPixelColor = fixed4(1,1,1,1);

				// Heat Calculation
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
				
				fixed4 heatColor = (fixed4(max(0,col.r), max(0,col.g), max(0,col.b), 1) * (1 - NdotV) + _EdgeColor * NdotV);

				// Light Calculation
				float4 pointN = float4(i.normal.x, i.normal.y, i.normal.z, 1);
				float4 pointPos = i.worldPixelPos;
				float4 lightPos = _LightPos;
				float4 lightN = _LightDirection;

				float4 lightDirection = _LightDirection;
				float  lightLengthMax = _LightLengthMax;
				float  lightLengthMin = _LightLengthMin;
				float  lightIntensity = _LightIntensity;
				float  lightAngle =_LightAngle;
				
				float4 distanceToLight = i.worldPixelPos - lightPos;
				
				float lightToPixelLength = length(distanceToLight);
				
				
				float4 diff = pointPos - lightPos;
				float4 diffN = normalize(diff);
				float4 refdiff = normalize(reflect(diff, pointN));
				float4 refLightN = reflect(lightN, pointN);
				
				float positionIntensity =  dot(refdiff, pointN) * (1 - min(1, max(0, length(diff) - lightLengthMin) / max(1, lightLengthMax - lightLengthMin)) ); // Need to make the diff length relevant.
				float directionIntensity = dot(lightN, diffN) * (1 - min(1, abs(getAngle4(lightN, diffN) / lightAngle))); // Need to make the light angle relevant
				float totalLightIntensity = positionIntensity * directionIntensity;

				return heatColor * lightIntensity * totalLightIntensity + (1 - totalLightIntensity) * realPixelColor;
			}

			ENDCG
		}
	}
}
