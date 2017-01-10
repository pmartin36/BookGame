///////// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Black/Circle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Center ("Center", Vector) = (0,0,0,0)
		_TimeDiff("Time Difference", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 worldpos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			fixed4 _Center;
			float _TimeDiff;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				o.worldpos.x *= _MainTex_TexelSize.z;
				o.worldpos.y = abs(o.worldpos.y * _MainTex_TexelSize.w);

				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				

				//_SinTime.x + 1 returns 0->2
				float d = max(5, distance(i.worldpos, _Center)/2);

				if( 5 / d > _TimeDiff){
					fixed4 col = tex2D(_MainTex, i.uv);
					return col;
				}
				else{
					return fixed4(0,0,0,1);
				}
			}
			ENDCG
		}
	}
}
