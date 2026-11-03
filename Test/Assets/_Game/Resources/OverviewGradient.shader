Shader "Snake/Overview Gradient"
{
    Properties
    {
        _Top ("Top", Color) = (0.7,0.76,0.82,1)
        _Bottom ("Bottom", Color) = (0.96,0.93,0.88,1)
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
            #include "UnityCG.cginc"
            half4 _Top, _Bottom;
            struct v2f { float4 vertex:SV_POSITION; float4 screen:TEXCOORD0; };
            v2f vert(appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screen = ComputeScreenPos(o.vertex);
                return o;
            }
            half4 frag(v2f i):SV_Target
            {
                float height = saturate(i.screen.y / i.screen.w);
                return half4(lerp(_Bottom.rgb, _Top.rgb, smoothstep(0,1,height)),1);
            }
            ENDCG
        }
    }
    Fallback Off
}
