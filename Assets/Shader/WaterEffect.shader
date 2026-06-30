Shader "Custom/WaterEffect"
{
    Properties
    {
        _WaterTex ("Water Texture", 2D) = "white" {}
        _TintColor ("Water Tint", Color) = (0.15, 0.55, 0.65, 0.75)
        _HighlightColor ("Highlight Color", Color) = (0.75, 0.95, 1.0, 1)
        _Opacity ("Opacity", Range(0,1)) = 0.75
        _FlowDirection ("Flow Direction XY", Vector) = (1, 0, 0, 0)
        _FlowSpeed ("Flow Speed", Range(0,2)) = 0.08
        _Tiling ("Tiling", Range(0.1,10)) = 1.5
        _HighlightStrength ("Highlight Strength", Range(0,2)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_WaterTex);
            SAMPLER(sampler_WaterTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterTex_ST;
                half4 _TintColor;
                half4 _HighlightColor;
                half _Opacity;
                float4 _FlowDirection;
                half _FlowSpeed;
                half _Tiling;
                half _HighlightStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 dir = normalize(_FlowDirection.xy + 0.0001);
                float t = _Time.y * _FlowSpeed;

                float2 uv1 = input.uv * _Tiling + dir * t;
                float2 uv2 = input.uv * (_Tiling * 1.15) - dir * t * 0.45;

                half3 tex1 = SAMPLE_TEXTURE2D(_WaterTex, sampler_WaterTex, uv1).rgb;
                half3 tex2 = SAMPLE_TEXTURE2D(_WaterTex, sampler_WaterTex, uv2).rgb;

                half v = (tex1.r + tex2.r) * 0.5;

                half3 color = _TintColor.rgb * (0.55 + v * 0.45);
                color = lerp(color, _HighlightColor.rgb, saturate(v - 0.62) * _HighlightStrength);

                return half4(color, _Opacity);
            }
            ENDHLSL
        }
    }
}
