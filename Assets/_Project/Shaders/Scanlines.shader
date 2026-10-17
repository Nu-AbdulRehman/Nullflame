// Time-power scanlines for the URP Full Screen Pass Renderer Feature (AD-013).
// Strength comes from the global _TimeFxIntensity set by ScreenFx; at 0 the image passes through unchanged.
Shader "Nullflame/Scanlines"
{
    Properties
    {
        _Lines ("Lines across the screen", Float) = 240
        _Darkness ("Line darkness at full intensity", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "Scanlines"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Lines;
            float _Darkness;
            float _TimeFxIntensity;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                float wave = 0.5 + 0.5 * sin(input.texcoord.y * _Lines * 6.2831853);
                color.rgb *= lerp(1.0, 1.0 - _Darkness * wave, _TimeFxIntensity);
                return color;
            }
            ENDHLSL
        }
    }
}
