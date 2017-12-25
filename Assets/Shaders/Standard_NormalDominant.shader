// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "ArtSpaces/Standard_NormalDominant" {
	Properties {
		_WorldSpaceScale( "WorldSpaceScale", Vector ) = ( 1, 1, 1, 1 )
		_Albedo ("Albedo", Color) = (1,1,1,1)
		_AlbedoTex( "Albedo Tex", 2D ) = "white" {}
		_SmoothnessTex ("Smoothness Tex (Gray)", 2D) = "white" {}
		_Smoothness ("Smoothness", Range(0,1)) = 1.0
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MetallicTex( "Metallic Tex (Gray)", 2D ) = "white" {}
	}
	SubShader {
		Tags {"RenderType"="Opaque"}
		//Cull Off

        LOD 3000
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _AlbedoTex;
		sampler2D _SmoothnessTex;
		sampler2D _MetallicTex;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		float4 _WorldSpaceScale;
		fixed4 _Albedo;
		float _Smoothness;
		half _Metallic;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		#define max3(x, y, z) ( max(max(x, y), z) )
		float maxElem(float3 v) { return max3(v.x, v.y, v.z); }

		float2 uvMapPerpendicularToDominantNormalElement( float3 N, float3 uvw )
		{
			N = abs(N);
			float m = maxElem(N);
			float2 uv;

			if (m == N.x) uv = uvw.yz;
			else if (m == N.y) uv = uvw.xz;
			else			   uv = uvw.xy;
			return uv;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {

			float2 uvs = uvMapPerpendicularToDominantNormalElement( IN.worldNormal, IN.worldPos * _WorldSpaceScale );

			o.Albedo = _Albedo * tex2D( _AlbedoTex, uvs );

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic * tex2D( _MetallicTex, uvs );

			o.Smoothness = _Smoothness * tex2D( _SmoothnessTex, uvs );
		}

		ENDCG
	}
	FallBack "Diffuse"
}
