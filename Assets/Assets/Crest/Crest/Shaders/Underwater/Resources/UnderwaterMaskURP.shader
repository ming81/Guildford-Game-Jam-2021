// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

Shader "Hidden/Crest/Underwater/Ocean Mask URP"
{
	SubShader
	{
		Pass
		{
			Name "Ocean Surface Mask"
			// We always disable culling when rendering ocean mask, as we only
			// use it for underwater rendering features.
			Cull Off

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			// for VFACE
			#pragma target 3.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#include "../UnderwaterMaskShared.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "Ocean Horizon Mask"
			Cull Off
			ZWrite Off
			// Horizon must be rendered first or it will overwrite the mask with incorrect values. ZTest not needed.
			ZTest Always

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			#include "../UnderwaterMaskHorizonShared.hlsl"
			ENDHLSL
		}
	}
}
