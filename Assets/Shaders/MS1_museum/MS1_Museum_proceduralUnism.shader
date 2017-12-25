// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Custom/MS1_Museum_proceduralUnism" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_UnismTex ("UnismTex", 2D) = "white" {}
		_WorldSpaceScale ("WorldSpaceScale", Vector) = (1,1,1,1)
		_RoughnessTex ("Roughness Tex (Gray)", 2D) = "black" {}
		_Smoothness ("Smoothness", Range(0,1)) = 1.0
		_Metallic ("Metallic", Range(0,1)) = 0.0
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

		sampler2D _RoughnessTex;
		sampler2D _UnismTex;

		struct Input {
			//float2 uv_RoughnessTex;
			float3 worldPos;
			float3 worldNormal;
		};

		fixed4 _Color;
		uniform float _UNISM_LERP = 0.0f; //setting a default value
		uniform float _UNISM_WHITE_LERP = 0.0f; //setting a default value

		float4 _WorldSpaceScale;
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
			// Albedo comes from a texture tinted by color

			fixed4 c = _Color;
			
			float2 uvs = uvMapPerpendicularToDominantNormalElement(IN.worldNormal, IN.worldPos );
			fixed4 unism = tex2D(_UnismTex, uvs );

			unism = lerp( unism, 1.0, _UNISM_WHITE_LERP );

			o.Albedo = lerp( c.rgb, unism,  _UNISM_LERP );

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;

			uvs = uvMapPerpendicularToDominantNormalElement(IN.worldNormal, IN.worldPos * _WorldSpaceScale);
			o.Smoothness = _Smoothness * (1.0 - tex2D(_RoughnessTex, uvs ).r - 0.1);// * _UNISM_LERP;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
