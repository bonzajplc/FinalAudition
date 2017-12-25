// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ArtSpaces/InstancedIndirectAdditive" {
	Properties{
		_Color( "Color", Color ) = ( 1, 1, 1, 1 )
		_ColorScale( "ColorScale", Range( 0, 10 ) ) = 1.0
		_Emission( "Emission", Range( 0, 10 ) ) = 0.0
	}
		SubShader{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }

		LOD 3000
		//AlphaToMask On
		//Cull Off		
		//Blend One One
		Lighting Off

		CGPROGRAM

		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard vertex:vert addshadow
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup

#pragma target 5.0

sampler2D _AlphaTex;

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
	float4 _CameraFront;

	half _Emission;
	half _ColorScale;
	fixed4 _Color;

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;

		uint id : SV_VertexID;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Input {
		float4 color : COLOR;
	};

	void vert( inout appdata v, out Input data ) {

		UNITY_INITIALIZE_OUTPUT( Input, data );
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		UNITY_SETUP_INSTANCE_ID( v );

		uint pid = ( v.id + indexStructure[0] * UNITY_GET_INSTANCE_ID( v ) ) / 3;
		uint vid = ( v.id + indexStructure[0] * UNITY_GET_INSTANCE_ID( v ) ) % 3;

		float4 positionA = mul( _LocalToWorld, float4( triangles[pid].v[vid].vPosition, 1.0f ) );
		float3 edge = mul( _LocalToWorld, float4( triangles[pid].v[vid].vNormal, 0.0f ) ).xyz;
		float3 side = cross( normalize( edge ), _CameraFront.xyz );

		positionA.xyz -= triangles[pid].v[vid].vColor.r * side;
		
		v.vertex = positionA;
		v.normal = float3( 0.0f, 1.0f, 0.0f );
#endif
		//v.color = float4( triangles[pid].v[vid].vColor, 1.0 );
		//data.worldPos = v.vertex;
		//data.worldNormal = v.normal;
	}

	void setup()
	{
	}

	void surf( Input IN, inout SurfaceOutputStandard o ) {

		o.Emission = _Emission;

		o.Albedo = _Color.xyz *_ColorScale;
	}
	ENDCG
	}
	FallBack "Diffuse"
}

