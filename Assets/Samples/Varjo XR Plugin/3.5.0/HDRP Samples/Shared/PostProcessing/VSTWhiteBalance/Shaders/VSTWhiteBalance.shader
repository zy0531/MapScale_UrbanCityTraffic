﻿Shader "Hidden/Shader/VSTWhiteBalance"
{
 //   HLSLINCLUDE

 //   #pragma target 4.5
 //   #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

 //   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
 //   #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
 //   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
 //   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/FXAA.hlsl"
 //   #include "Packages/com.unity.render-pipelines.high-definition/Runtime/PostProcessing/Shaders/RTUpscale.hlsl"

 //   struct Attributes
 //   {
 //       uint vertexID : SV_VertexID;
 //       UNITY_VERTEX_INPUT_INSTANCE_ID
 //   };

 //   struct Varyings
 //   {
 //       float4 positionCS : SV_POSITION;
 //       float2 texcoord   : TEXCOORD0;
 //       UNITY_VERTEX_OUTPUT_STEREO
 //   };

 //   Varyings Vert(Attributes input)
 //   {
 //       Varyings output;
 //       UNITY_SETUP_INSTANCE_ID(input);
 //       UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
 //       output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
 //       output.texcoord = GetFullScreenTriangleTexCoord(input.vertexID);
 //       return output;
 //   }

 //   // List of properties to control your post process effect
 //   float _Intensity;
 //   TEXTURE2D_X(_InputTexture);

	//float4 _CamWBGains;  //!< White balance gains to convert from 6500K to VST color temperature.
	//float4x4 _CamInvCCM; //!< Inverse CCM for 6500K.
	//float4x4 _CamCCM;    //!< CCM for VST color temperature.

	//float3 NormalizeWhiteBalance(float3 inRGB)
	//{
	//	float3 outRGB = mul(inRGB, (float3x3)_CamInvCCM);
	//	outRGB.rgb *= _CamWBGains.rgb;
	//	outRGB = saturate(outRGB);
	//	outRGB.rgb = mul(outRGB.rgb, (float3x3)_CamCCM);
	//	return outRGB;
	//}

 //   float4 CustomPostProcess(Varyings input) : SV_Target
 //   {
 //       UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

 //       uint2 positionSS = input.texcoord * _ScreenSize.xy;
 //       float3 outColor = LOAD_TEXTURE2D_X(_InputTexture, positionSS).xyz;
 //       return float4(lerp(outColor, NormalizeWhiteBalance(outColor), _Intensity), 1);
 //   }

 //   ENDHLSL

 //   SubShader
 //   {
 //       Pass
 //       {
 //           Name "VSTWhiteBalance"

 //           ZWrite Off
 //           ZTest Always
 //           Blend Off
 //           Cull Off

 //           HLSLPROGRAM
 //               #pragma fragment CustomPostProcess
 //               #pragma vertex Vert
 //           ENDHLSL
 //       }
 //   }
 //   Fallback Off
}
