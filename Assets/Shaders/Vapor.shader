Shader "Unlit/Vapor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_VaporTex ("Vapor", 2D) = "white" {}
		_DisplacementTex ("Displacement", 2D) = "white" {}
		_Magnitude("Displacement Magnitude", Range(0,1)) = .5
		_Color ("Vapor Color", Color) = (1,1,1,1)
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
			};

			sampler2D _MainTex;
			sampler2D _VaporTex;
			sampler2D _DisplacementTex;
			float4 _Color;
			float _Magnitude;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float displacement = (tex2D(_DisplacementTex, i.uv + _Time.x).a * 2 - 1) * _Magnitude;

				// sample the textures
				fixed4 main = tex2D(_MainTex, i.uv + float2(displacement, 0));
				fixed4 vapor = tex2D(_VaporTex, i.uv + float2(displacement,_Time.x));

				float4 coloredVapor = vapor * _Color;
				fixed4 col =  coloredVapor * coloredVapor.a + main * (1-coloredVapor.a);

				return col;
			}
			ENDCG
		}
	}
}
