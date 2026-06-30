Shader "Custom/WatterEffect2"
{
    Properties
    {
        _BaseColor ("Base Water Color", Color) = (0.10, 0.18, 0.24, 0.78)
        _DeepColor ("Deep Shadow Color", Color) = (0.02, 0.05, 0.08, 0.90)
        _HighlightColor ("Facet Highlight Color", Color) = (0.45, 0.60, 0.72, 1.0)
        _GhostColor ("Ghost Fire Glow Color", Color) = (0.00, 0.85, 0.95, 1.0)

        _Opacity ("Opacity", Range(0, 1)) = 0.78
        _FacetScale ("Facet Size", Range(2, 80)) = 18
        _Contrast ("Facet Contrast", Range(0, 3)) = 1.25
        _FlowSpeed ("Flow Speed", Range(0, 2)) = 0.10
        _FlowDirection ("Flow Direction XY", Vector) = (1, 0.25, 0, 0)
        _GhostStrength ("Ghost Glow Strength", Range(0, 2)) = 0.35
        _GhostScale ("Ghost Glow Size", Range(1, 30)) = 7
        _Distortion ("Flow Distortion", Range(0, 1)) = 0.22
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

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor, _DeepColor, _HighlightColor, _GhostColor;
                half _Opacity, _FacetScale, _Contrast, _FlowSpeed;
                float4 _FlowDirection;
                half _GhostStrength, _GhostScale, _Distortion;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float facetNoise(float2 uv)
            {
                float2 grid = uv * _FacetScale;
                float2 cell = floor(grid);
                float2 f = frac(grid);

                float tri = step(f.y, f.x);
                float nA = hash21(cell);
                float nB = hash21(cell + float2(1, 0));
                float nC = hash21(cell + float2(0, 1));
                float nD = hash21(cell + float2(1, 1));

                float lowerTri = (nA + nB + nC) / 3.0;
                float upperTri = (nB + nC + nD) / 3.0;
                return lerp(upperTri, lowerTri, tri);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 dir = normalize(_FlowDirection.xy + float2(0.0001, 0.0001));
                float t = _Time.y * _FlowSpeed;

                float2 warp = float2(
                    valueNoise(input.uv * 3.0 + t * 0.35),
                    valueNoise(input.uv * 3.0 - t * 0.28)
                ) - 0.5;

                float2 uv1 = input.uv + dir * t + warp * _Distortion;
                float2 uv2 = input.uv * 1.7 - dir.yx * t * 0.55;

                float facets = facetNoise(uv1) * 0.72 + facetNoise(uv2) * 0.38;
                facets = saturate((facets - 0.42) * _Contrast + 0.5);

                half3 col = lerp(_DeepColor.rgb, _BaseColor.rgb, facets);
                half hi = smoothstep(0.62, 0.94, facets);
                col = lerp(col, _HighlightColor.rgb, hi * 0.42);

                float ghost = valueNoise(input.uv * _GhostScale - dir * t * 0.65);
                ghost *= valueNoise(input.uv * (_GhostScale * 0.45) + dir.yx * t * 0.25);
                col += _GhostColor.rgb * smoothstep(0.62, 0.92, ghost) * _GhostStrength;

                return half4(col, _Opacity);
            }
            ENDHLSL
        }
    }
}
