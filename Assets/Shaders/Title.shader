Shader "Unlit/Title"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EffectTex ("Effect", 2D) = "white" {}
		_HideRed("Hide Red Value Amount", Range(0,1)) = .5
	}
	SubShader
	{
		Tags { 
			"Queue" = "Transparent"
		}

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

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

			sampler2D _MainTex;
			sampler2D _EffectTex;
			float _HideRed;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the textures
				fixed4 effect = tex2D(_EffectTex, i.uv);
				if(effect.r >= _HideRed)
					return float4(0,0,0,0);

				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
