Shader "Snake/White Overview After Post"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            ZWrite Off ZTest Always Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float depth = SampleSceneDepth(input.texcoord);
                #if UNITY_REVERSED_Z
                    if (depth > 0.000001) discard;
                #else
                    if (depth < 0.999999) discard;
                #endif
                return half4(1,1,1,1);
            }
            ENDHLSL
        }
    }
}
