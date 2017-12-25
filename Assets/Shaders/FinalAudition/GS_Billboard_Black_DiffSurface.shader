// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GS Billboard Black Diff Surface" 
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
				
				#include "../Compute/shaderQuat.cginc"

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


				float minU;
				float minV;
				float maxU;
				float maxV;

				float param0;
				float param1;

				float surfType;

				float3 generateDifferentialSurface( float2 uv )
				{
					float3 outVertex;

					float u = minU + uv.x * ( maxU - minU );
					float v = minV + uv.y * ( maxV - minV );

					if( surfType < 1 ) //Tunnel_1_1
					{
						outVertex.x = -4 * ( 1 - v / 6.283 )*cos( 2 * v )*( 1 + cos( u ) ) + 1 * cos( 2 * v );
						outVertex.y = 4 * ( 1 - v / 6.283 )*sin( 2 * v )*( 1 + 0.1*sin( v * 3 ) )*( 1 + cos( u ) ) + 1 * sin( 2 * v );
						outVertex.z = 80 * v / 6.283 + 4 * ( 1 - v / 6.283 )*( 1 + 0.2*sin( v * 8 ) )*sin( u );
					}
					else if( surfType < 2 ) //Tunnel_1_2
					{
						outVertex.x = ( 2.0 + 0.3*cos( u )*sin( 6 * v ) )*cos( u )*sin( v );
						outVertex.y = ( 2.0 + 0.2*cos( u )*sin( 3 * v ) )*sin( u )*sin( v );
						outVertex.z = ( 2.0 + 0.2*cos( u )*sin( 2 * v ) )*cos( v );
					}
					else if( surfType < 3 ) //Tunnel_2_1
					{
						outVertex.x = ( 4.0 + 0.5*cos( param0 + 3 * u )*sin( 5 * v ) )*cos( v ) + 3 * cos( u );
						outVertex.y = ( 4.0 + 0.5*cos( param1 + 3 * u )*sin( 5 * v ) )*sin( v ) + 3 * sin( u );
						outVertex.z = u * 4;
					}
					else if( surfType < 4 || surfType < 5 ) //Surface_1_1 Surface_2_1
					{
						outVertex.x = ( 2.0f + 0.5f * cos( 5 * u ) * sin( 5 * v ) ) * cos( u ) * sin( v );
						outVertex.y = ( 2.0f + 0.5f * cos( 5 * u ) * sin( 5 * v ) ) * sin( u ) * sin( v );
						outVertex.z = ( 2.0f + 0.5f * cos( 5 * u ) * sin( 5 * v ) ) * cos( v );
					}
					else if( surfType < 6 ) //Surface_2_2
					{
						outVertex.x = ( 2.0 + 0.5*cos( 2 * u )*sin( param0*v ) )*cos( u )*sin( v );
						outVertex.y = ( 2.0 + 0.5*cos( param1*u )*sin( 5 * v ) )*sin( u )*sin( v );
						outVertex.z = ( 2.0 + 0.5*cos( 4 * u )*sin( 5 * v ) )*cos( v );
					}

					return outVertex;
				}

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main( appdata v )
				{
					GS_INPUT output = (GS_INPUT)0;

					float3 vertex = generateDifferentialSurface( v.texcoord1.xy );
					float3 tangent = generateDifferentialSurface( v.texcoord1.xy + float2( 0.01f, 0.0f ) ) - v.vertex.xyz;
					float tLength = length( tangent );
					if( tLength > 0.001f )
						tangent = tangent / tLength;
					else
						tangent = float3( 1.0f, 0.0f, 0.0f );

					float3 binormal = generateDifferentialSurface( v.texcoord1.xy + float2( 0.0f, 0.01f ) ) - v.vertex.xyz;
					float tBinormal = length( binormal );
					float3 normal = float3( 0.0f, 1.0f, 0.0f );

					if( tBinormal > 0.001f )
						normal = normalize( cross( binormal / tBinormal, tangent ) );
						
					output.pos = mul( unity_ObjectToWorld, vertex + _NormalOffset * normal );// *lerp( 1.0f, v.texcoord1.x, _ArtSpaces_frame_fraction ) );
					output.normal = normal;
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

					float rotationOffset = fbm( float3( p[0].tex0.xy, 0.0f ) * 100.0f );
					float noiseScale = fbm( p[0].normal.xyz * 100.0f );
					float headCollision = smoothstep( 0.0f, 1.0f, length( p0Eye) * 1.0f );


					float halfS = 0.5f * headCollision * _Size * ( noiseScale * 0.5 + 0.5 );

					float4 quat = picoQuatAxisAngleToQuat( float3( 0, 0, 1 ), rotationOffset + ( noiseScale - 0.5f ) * 3.0f *_Rotate );
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
