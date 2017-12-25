// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ArtSpaces/PearlsInstancer" {
	Properties{
			_Color( "Color", Color ) = ( 1, 1, 1, 1 )
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

	struct Pearl
	{
		float3 vPosition;
		float  fScale;
		float3 vColor;
	};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
	uniform StructuredBuffer<Pearl> positions;
#endif
	float4x4 _LocalToWorld;
	float4x4 _WorldToLocal;

	half _Metallic;
	half _Smoothness;
	half _Emission;
	fixed4 _Color;

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 color : COLOR;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct Input {
		float3 customColor;
	};

	void vert( inout appdata v, out Input data ) {
		UNITY_INITIALIZE_OUTPUT( Input, data );

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		v.vertex = mul( _LocalToWorld, float4( v.vertex.xyz * positions[UNITY_GET_INSTANCE_ID( v )].fScale + positions[UNITY_GET_INSTANCE_ID( v )].vPosition, 1 ) );
		v.normal = mul( _LocalToWorld, v.normal );
		data.customColor = positions[UNITY_GET_INSTANCE_ID( v )].vColor;
#endif
	}
	
	void setup()
	{
	}

	void surf( Input IN, inout SurfaceOutputStandard o ) 
	{

		// Metallic and smoothness come from slider variables
		o.Metallic = _Metallic;

		o.Smoothness = _Smoothness;

		o.Emission = _Emission;

		o.Albedo = _Color.xyz * IN.customColor;
	}
	ENDCG
	}
	FallBack "Diffuse"
}


