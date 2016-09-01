//make sure that you have the sprite as the letter, the maintex as the same letter, and the effect to whatever you want
Shader "UI/ButtonEffect"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_EffectTex ("Effect Texture", 2D) = "white" {}
		_EffectColor ("Effect Color", Color) = (1,1,1,1)
		_EffectOffset ("Effect Offset", Float) = 1
	}
	SubShader
	{

		Tags {  
			"Queue" = "Transparent"
		}

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
			sampler2D _EffectTex;
			fixed4 _EffectColor;
			float _EffectOffset;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				float4 eff = tex2D(_EffectTex, float2(i.uv.x - _EffectOffset, i.uv.y - _EffectOffset));
				//				
				float4 show = (col.r * col) + ((1-col.r) * (col.a) * eff) * _EffectColor;
				return show;
			}
			ENDCG
		}
	}
}