// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/DifferentialSurfaceShader_Metalness" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_NormalTex( "Normal", 2D ) = "white" {}
		_MetallicMap( "MetallicMap", 2D ) = "white" {}
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard  vertex:vert fullforwardshadows addshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma shader_feature DIFF_SURF
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalTex;
		sampler2D _MetallicMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NormalTex;
			float3 vertexColor; // Vertex color stored here by vert() method
			float3 viewDir;
			INTERNAL_DATA
		};

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

		void vert( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.vertexColor = v.color; // Save the Vertex Color in the Input for the surf() method
			v.vertex.xyz = generateDifferentialSurface( v.texcoord.xy );
			float3 tangent = generateDifferentialSurface( v.texcoord.xy + float2( 0.01f, 0.0f ) ) - v.vertex.xyz;
			float tLength = length( tangent );
			if( tLength > 0.001f )
				v.tangent = float4( tangent / tLength, 0.0f );
			else
				v.tangent = float4( 1.0f, 0.0f, 0.0f, 0.0f );

			float3 binormal = generateDifferentialSurface( v.texcoord.xy + float2( 0.0f, 0.01f ) ) - v.vertex.xyz;
			float tBinormal = length( binormal );
			if( tBinormal > 0.001f )
				v.normal = normalize( cross( binormal / tBinormal, v.tangent.xyz ) );
			else
				v.normal = float4( 0.0f, 1.0f, 0.0f, 0.0f );
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Normal;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D( _MainTex, IN.uv_MainTex ) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal( tex2D( _NormalTex, IN.uv_NormalTex ) );
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic * tex2D( _MetallicMap, IN.uv_MainTex ).r;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
