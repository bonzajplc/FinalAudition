// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GS Billboard Julia Fractal" 
{
	Properties 
	{
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
		_SpriteTex ("Base (RGB)", 2D) = "white" {}
		_Size( "Size", Range( 0, 3 ) ) = 0.001
	}

	SubShader 
	{
		Pass
		{
			Tags{ "Queue" = "Transparent" }
			LOD 200
			ZWrite Off
			Blend One One // Additive
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct appdata 
				{
					float3 vertex : POSITION;
					float3 normal : NORMAL;
					float4 color : COLOR;
					float4 texcoord1 : TEXCOORD1;
					uint id : SV_VertexID;
				};

				struct GS_INPUT
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float4  color	: COLOR;
					float2  tex0	: TEXCOORD0;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					float2  tex0	: TEXCOORD0;
					float4  color	: COLOR;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************
				
				StructuredBuffer<float3>	points;
				float _Size;
				float4x4 _VP;
				fixed4 _Color;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;

				uniform float _ArtSpaces_frame_fraction = 0.0f; //setting a default value

				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main( appdata v )
				{
					GS_INPUT output = (GS_INPUT)0;

					output.pos = mul( unity_ObjectToWorld, points[v.id] );// *lerp( 1.0f, v.texcoord1.x, _ArtSpaces_frame_fraction ) );
					output.normal = v.normal;
					output.tex0 = v.texcoord1;
					output.color = v.color;

					return output;
				}



				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{
					float3 up = float3(0, 1, 0);
					float3 look = _WorldSpaceCameraPos - p[0].pos;
					look.y = 0;
					look = normalize(look);
					float3 right = cross(up, look);
					up = cross( look, right );

					float halfS = 0.5f * _Size;

					float3 p0Eye = UnityObjectToViewPos( p[0].pos.xyz );
							
					float4 v[4];
	
					up = float3( 0, 1, 0 );
					right = float3( -1, 0, 0 );

					v[0] = float4( p0Eye + halfS * right - halfS * up, 1.0f);
					v[1] = float4( p0Eye + halfS * right + halfS * up, 1.0f);
					v[2] = float4( p0Eye - halfS * right - halfS * up, 1.0f);
					v[3] = float4( p0Eye - halfS * right + halfS * up, 1.0f);

//					float4x4 vp = UnityObjectToClipPos(unity_WorldToObject);
					FS_INPUT pIn;
					pIn.color = p[0].color;
					pIn.pos = mul( UNITY_MATRIX_P, v[0] );
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul( UNITY_MATRIX_P, v[1] );
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos = mul( UNITY_MATRIX_P, v[2] );
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = mul( UNITY_MATRIX_P, v[3] );
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
				}



				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
					return _SpriteTex.Sample(sampler_SpriteTex, input.tex0) * _Color;
				}

			ENDCG
		}
	} 
}
