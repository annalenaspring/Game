Shader "Snake/Soft Panorama"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Panorama", 2D) = "grey" {}
        _Tint ("Tint", Color) = (0.5,0.5,0.5,1)
        _Exposure ("Exposure", Float) = 1
        _Rotation ("Rotation", Float) = 0
        _Blur ("Blur", Range(0,1)) = 0.85
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            half4 _MainTex_HDR;
            half4 _Tint;
            float _Exposure, _Rotation, _Blur;
            struct v2f { float4 vertex : SV_POSITION; float3 direction : TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                float angle = radians(_Rotation);
                float s = sin(angle), c = cos(angle);
                float3 rotated = float3(c*v.vertex.x-s*v.vertex.z, v.vertex.y, s*v.vertex.x+c*v.vertex.z);
                o.vertex = UnityObjectToClipPos(float4(rotated, 1));
                o.direction = v.vertex.xyz;
                return o;
            }
            half3 samplePanorama(float2 uv, float lod)
            {
                uv = float2(frac(uv.x), clamp(uv.y, 0.001, 0.999));
                return DecodeHDR(tex2Dlod(_MainTex, float4(uv, 0, lod)), _MainTex_HDR);
            }
            half4 frag(v2f i) : SV_Target
            {
                float3 d = normalize(i.direction);
                float2 uv = float2(0.5-atan2(d.z,d.x)/(2*UNITY_PI), 1-acos(clamp(d.y,-1,1))/UNITY_PI);
                // Resolution-independent blur: coarse mip levels plus a broad 3x3 kernel.
                float lod = max(0, log2(max(1, _MainTex_TexelSize.z)/8)) * _Blur;
                float radius = 0.12 * _Blur;
                half3 col = samplePanorama(uv,lod)*4;
                col += 2*samplePanorama(uv+float2(radius,0),lod);
                col += 2*samplePanorama(uv-float2(radius,0),lod);
                col += 2*samplePanorama(uv+float2(0,radius),lod);
                col += 2*samplePanorama(uv-float2(0,radius),lod);
                col += samplePanorama(uv+float2(radius,radius),lod);
                col += samplePanorama(uv+float2(-radius,radius),lod);
                col += samplePanorama(uv+float2(radius,-radius),lod);
                col += samplePanorama(uv-float2(radius,radius),lod);
                return half4(col/16 * _Tint.rgb * unity_ColorSpaceDouble.rgb * _Exposure, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
