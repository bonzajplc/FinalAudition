// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GS Billboard Black" 
{
	Properties 
	{
		_SpriteTex ("Base (RGB)", 2D) = "white" {}
		_Size ("Size", Range(0, 3)) = 0.5
		_Blend ("Blend", Range(0, 1)) = 1.0
		_NormalOffset( "Normal Offset", Range( 0, 3 ) ) = 0.0
		_Rotate( "Rotate", Float ) = 0.0
	}

	SubShader 
	{
		Pass
		{
			Tags{ "Queue" = "Transparent" }
			LOD 2000
			ZWrite Off
			//ZTest Always
			Blend Zero OneMinusSrcAlpha
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 
				
				#include "Compute/shaderQuat.cginc"

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct appdata 
				{
					float3 vertex : POSITION;
					float3 normal : NORMAL;
					float4 color : COLOR;
					float4 texcoord1 : TEXCOORD1;
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

				float _Size;
				float _Blend;
				float _NormalOffset;
				float _Rotate;
				float4x4 _VP;
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

					output.pos = mul( unity_ObjectToWorld, v.vertex + _NormalOffset * v.normal );// *lerp( 1.0f, v.texcoord1.x, _ArtSpaces_frame_fraction ) );
					output.normal = v.normal;
					output.tex0 = v.texcoord1;
					output.color = v.color;

					return output;
				}

				float hash( float h )
				{
					return frac( sin( h ) * 43758.5453123 );
				}

				float noise( float3 x )
				{
					float3 p = floor( x );
					float3 f = frac( x );
					f = f * f * ( 3.0 - 2.0 * f );

					float n = p.x + p.y * 157.0 + 113.0 * p.z;
					return lerp(
						lerp( lerp( hash( n + 0.0 ), hash( n + 1.0 ), f.x ),
							  lerp( hash( n + 157.0 ), hash( n + 158.0 ), f.x ), f.y ),
						lerp( lerp( hash( n + 113.0 ), hash( n + 114.0 ), f.x ),
							  lerp( hash( n + 270.0 ), hash( n + 271.0 ), f.x ), f.y ), f.z );
				}

				float fbm( float3 p )
				{
					float f = 0.0;
					f = 0.5000 * noise( p );
					p *= 2.01;
					f += 0.2500 * noise( p );
					p *= 2.02;
					f += 0.1250 * noise( p );

					return f;
				}

				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point GS_INPUT p[1], inout TriangleStream<FS_INPUT> triStream)
				{

					float3 p0Eye = UnityObjectToViewPos( p[0].pos.xyz );
							
					float4 v[4];
	
					float3 up = float3( 0, 1, 0 );
					float3 right = float3( -1, 0, 0 );

					float rotationOffset = fbm( p[0].pos.xyz * 100.0f );
					float noiseScale = fbm( p[0].normal.xyz * 100.0f );

					float halfS = 0.5f * _Size * ( noiseScale * 0.5 + 0.5 );

					float4 quat = picoQuatAxisAngleToQuat( float3(0,0,1), rotationOffset + ( noiseScale - 0.5f ) * _Rotate );
					right = picoQuatRotateVector( quat, right );
					up = picoQuatRotateVector( quat, up );

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
					return _SpriteTex.Sample(sampler_SpriteTex, input.tex0) * _Blend;
					//return input.color * 0.5;
				}

			ENDCG
		}
	} 
}
