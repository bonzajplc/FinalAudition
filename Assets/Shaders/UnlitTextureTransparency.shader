Shader "Custom/UnlitTextureTransparency"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
		_Transparency( "Transparency", Range( 0, 1 ) ) = 1.0
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" }
		LOD 200
		ZTest Always
		//ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Transparency;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX( v.uv, _MainTex );
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				col.a *= _Transparency;
				return col;
			}
			ENDCG
		}
	}
}
