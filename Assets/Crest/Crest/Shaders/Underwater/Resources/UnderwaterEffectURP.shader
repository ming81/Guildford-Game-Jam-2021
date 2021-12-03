// Crest Ocean System

// Copyright 2021 Wave Harmonic Ltd

Shader "Hidden/Crest/Underwater/Underwater Effect URP"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			// #pragma enable_d3d11_debug_symbols

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT

			// Use multi_compile because these keywords are copied over from the ocean material. With shader_feature,
			// the keywords would be stripped from builds. Unused shader variants are stripped using a build processor.
			#pragma multi_compile_local __ _SUBSURFACESCATTERING_ON
			#pragma multi_compile_local __ _SUBSURFACESHALLOWCOLOUR_ON
			#pragma multi_compile_local __ _TRANSPARENCY_ON
			#pragma multi_compile_local __ _CAUSTICS_ON
			#pragma multi_compile_local __ _SHADOWS_ON
			#pragma multi_compile_local __ _PROJECTION_PERSPECTIVE _PROJECTION_ORTHOGRAPHIC

			#pragma multi_compile_local __ CREST_MENISCUS
			#pragma multi_compile_local __ _FULL_SCREEN_EFFECT
			#pragma multi_compile_local __ _DEBUG_VIEW_OCEAN_MASK

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#include "../../OceanGlobals.hlsl"
			#include "../../OceanInputsDriven.hlsl"
			#include "../../OceanShaderData.hlsl"
			#include "../../OceanHelpersNew.hlsl"
			#include "../../OceanShaderHelpers.hlsl"
			#include "../../OceanEmission.hlsl"

			TEXTURE2D_X(_CrestCameraColorTexture);
			TEXTURE2D_X(_CrestOceanMaskTexture);
			TEXTURE2D_X(_CrestOceanMaskDepthTexture);

			// For XR SPI as could not get correct value from unity_CameraToWorld;
			float3 _CameraForward;

			#include "../UnderwaterEffectShared.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings Vert (Attributes input)
			{
				Varyings output;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

#if CREST_HANDLE_XR
				// Already in HCS as we are using DrawProcedural as per Unity's recommendation.
				output.positionCS = input.positionOS;
#if UNITY_UV_STARTS_AT_TOP
				output.positionCS.y *= -1;
#endif // UNITY_UV_STARTS_AT_TOP
#else // CREST_HANDLE_XR
				// Already a fullscreen triangle.
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
#endif // CREST_HANDLE_XR

				output.uv = input.uv;

				return output;
			}

			real4 Frag (Varyings input) : SV_Target
			{
				// We need this when sampling a screenspace texture.
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				const uint2 uvScreenSpace = input.positionCS.xy;
				half3 sceneColour = LOAD_TEXTURE2D_X(_CrestCameraColorTexture, uvScreenSpace).rgb;
				float rawDepth = LOAD_TEXTURE2D_X(_CameraDepthTexture, uvScreenSpace).x;
				const float mask = LOAD_TEXTURE2D_X(_CrestOceanMaskTexture, uvScreenSpace).x;
				const float rawOceanDepth = LOAD_TEXTURE2D_X(_CrestOceanMaskDepthTexture, uvScreenSpace).x;

				bool isOceanSurface; bool isUnderwater; float sceneZ;
				GetOceanSurfaceAndUnderwaterData(uvScreenSpace, rawOceanDepth, mask, rawDepth, isOceanSurface, isUnderwater, sceneZ, 0.0);

				float wt = ComputeMeniscusWeight(uvScreenSpace, mask, _HorizonNormal, sceneZ);

#if _DEBUG_VIEW_OCEAN_MASK
				return DebugRenderOceanMask(isOceanSurface, isUnderwater, mask, sceneColour);
#endif // _DEBUG_VIEW_OCEAN_MASK

				if (isUnderwater)
				{
					// Position needs to be reconstructed in the fragment shader to avoid precision issues as per
					// Unity's lead. Fixes caustics stuttering when far from zero.
					const float3 positionWS = ComputeWorldSpacePosition(input.uv, rawDepth, UNITY_MATRIX_I_VP);
					const half3 view = normalize(_WorldSpaceCameraPos - positionWS);
					float3 scenePos = _WorldSpaceCameraPos - view * sceneZ / dot(_CameraForward, -view);
					const Light lightMain = GetMainLight();
					const real3 lightDir = lightMain.direction;
					const real3 lightCol = lightMain.color;
					sceneColour = ApplyUnderwaterEffect(uvScreenSpace, scenePos, sceneColour, lightCol, lightDir, rawDepth, sceneZ, view, isOceanSurface);
				}

				return half4(wt * sceneColour, 1.0);
			}
			ENDHLSL
		}
	}
}
