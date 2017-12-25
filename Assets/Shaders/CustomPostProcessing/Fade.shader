Shader "Hidden/Custom/Fade"
{
	HLSLINCLUDE

		#include "../../PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		float	_Blend;
		float4	_Color;

		float4 Frag(VaryingsDefault i) : SV_TARGET
		{
			float4	color		=	SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
			color.rgb			=	lerp(color.rgb, _Color.rgb, _Color.aaa);
			return	color;
		}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM

				#pragma	vertex		VertDefault
				#pragma	fragment	Frag

			ENDHLSL
		}
	}
}
