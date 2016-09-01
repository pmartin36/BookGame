Shader "Sprites/BeamMeUp"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EffectMap("Effect Map", 2D) = "white" {}
		_Seed("Seed",int) = 1
	}
	SubShader
	{
		Tags{
			"Queue" = "Transparent"
		}

		// No culling or depth
		Blend SrcAlpha OneMinusSrcAlpha

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
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _EffectMap;
			float4 _MainTex_TexelSize;
			int _Seed;

			float rand(float2 co){
    			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//				fixed4 col = tex2D(_MainTex, i.uv);
//					if(col.g == 1 && col.r + col.b == 0){
//					float ran = rand(float2(round(i.uv.x*100),_Seed)) - i.uv.y;
//					col = float4(0.9,0.9,0.5,ran);
//				}
//				return col;

				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 eff = tex2D(_EffectMap, i.uv + fixed2(0,_MainTex_TexelSize.y)); //we shift this down one to remove the green outline
				float4 ran = float4(0.9,0.9,0.5,rand(float2(round(i.uv.x*100),_Seed)) - i.uv.y);
				float4 show = col * eff.r + ran * (1-eff.r);
				return show;
				
			}
			ENDCG
		}
	}
}
