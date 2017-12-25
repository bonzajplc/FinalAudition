// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ArtSpaces/InstancedIndirect" {
	Properties{
			_Color( "Color", Color ) = ( 1, 1, 1, 1 )
			_WorldSpaceScale( "WorldSpaceScale", Vector ) = ( 1, 1, 1, 1 )
			_RoughnessTex( "Roughness (Gray)", 2D ) = "white" {}
			_Metallic( "Metallic", Range( 0, 1 ) ) = 0.0
			_Smoothness( "Smoothness", Range( 0, 1 ) ) = 1.0
			_Emission( "Emission", Range( 0, 100 ) ) = 0.0
	}
		SubShader{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }

		LOD 3000
		//AlphaToMask On
		//Cull Off		
		CGPROGRAM

		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard vertex:vert fullforwardshadows addshadow
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup

#pragma target 5.0

	struct Vertex
	{
		float3 vPosition;
		float3 vNormal;
		float3 vColor;
	};

	struct Triangle
	{
		Vertex v[3];
	};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	uniform StructuredBuffer<Triangle> triangles;
	uniform StructuredBuffer<uint> indexStructure;
#endif
	float4x4 _LocalToWorld;
	float4x4 _WorldToLocal;

	sampler2D _RoughnessTex;

	float4 _WorldSpaceScale;
	half _Metallic;
	half _Smoothness;
	half _Emission;
	fixed4 _Color;

	struct appdata {
		float4 vertex : POSITION;
		float4 color : COLOR;
		float3 normal : NORMAL;

		uint id : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Input {
		float3 worldPos;
		float3 worldNormal;
		float4 color : COLOR;
	};

	void vert( inout appdata v, out Input data ) {
		UNITY_INITIALIZE_OUTPUT( Input, data );

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		UNITY_SETUP_INSTANCE_ID( v );

		uint pid = ( v.id + indexStructure[0] * UNITY_GET_INSTANCE_ID( v ) ) / 3;
		uint vid = ( v.id + indexStructure[0] * UNITY_GET_INSTANCE_ID( v ) ) % 3;

		v.vertex = mul( _LocalToWorld, float4( triangles[pid].v[vid].vPosition, 1 ) );
		v.normal = mul( _LocalToWorld, triangles[pid].v[vid].vNormal );
		v.color = float4( triangles[pid].v[vid].vColor, 1.0 );
#endif
	}

	void setup()
	{
	}

#define max3(x, y, z) ( max(max(x, y), z) )
	float maxElem( float3 v ) { return max3( v.x, v.y, v.z ); }

	float2 uvMapPerpendicularToDominantNormalElement( float3 N, float3 uvw )
	{
		N = abs( N );
		float m = maxElem( N );
		float2 uv;

		if( m == N.x ) uv = uvw.yz;
		else if( m == N.y ) uv = uvw.xz;
		else uv = uvw.xy;
		return uv;
	}

	void surf( Input IN, inout SurfaceOutputStandard o ) {

		fixed4 c = _Color;

		float2 uvs = uvMapPerpendicularToDominantNormalElement( IN.worldNormal, IN.worldPos );


		// Metallic and smoothness come from slider variables
		o.Metallic = _Metallic;

		uvs = uvMapPerpendicularToDominantNormalElement( IN.worldNormal, IN.worldPos * _WorldSpaceScale );
		o.Smoothness = 1.0 - min( 1.0, tex2D( _RoughnessTex, uvs ).r * 2.0f - 0.0 ) * _Smoothness;

		o.Emission = _Emission;

		o.Albedo = _Color.xyz * IN.color;
	}
	ENDCG
	}
	FallBack "Diffuse"
}


