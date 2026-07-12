Shader "TheTwoWorld/Character Sprite Fill Light"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HDR] _FillColor ("Fill Light Color", Color) = (1.0,0.55,0.28,1)
        _FillStrength ("Fill Light Strength", Range(0,1)) = 0.12
        _Brightness ("Brightness", Range(0.5,2)) = 1.0
        _EnvironmentLightStrength ("Environment Light Strength", Range(0,2)) = 1.0
        _AmbientStrength ("Ambient Light Strength", Range(0,1)) = 0.25
        _LightWrap ("Point Light Wrap", Range(0,1)) = 0.35
        [Toggle] _ReceiveShadows ("Receive Shadows", Float) = 1
        [Toggle] _PixelSnap ("Pixel Snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Lit"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
        }

        Cull Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "SpriteForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5

            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile _ _FORWARD_PLUS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _FillColor;
                half _FillStrength;
                half _Brightness;
                half _EnvironmentLightStrength;
                half _AmbientStrength;
                half _LightWrap;
                half _ReceiveShadows;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }

            half LightFacing(half3 normalWS, half3 lightDirection)
            {
                // Wrapped diffuse keeps a useful response when a 2D sprite and
                // a point light are close to the same XY plane.
                return lerp(saturate(dot(normalWS, lightDirection)), 1.0h, _LightWrap);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 sprite = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half alpha = sprite.a * input.color.a;
                half3 albedo = sprite.rgb * input.color.rgb * _Brightness;

                // Treat the flat sprite as camera-facing. This works regardless
                // of which way the SpriteRenderer object is flipped or rotated.
                half3 normalWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half3 realtimeLighting = 0;

                half4 shadowMask = half4(1, 1, 1, 1);
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(input.positionWS));
                half mainShadow = lerp(1.0h, mainLight.shadowAttenuation, _ReceiveShadows);
                realtimeLighting += mainLight.color
                    * mainLight.distanceAttenuation
                    * mainShadow
                    * LightFacing(normalWS, mainLight.direction);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = normalWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                uint pixelLightCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light light = GetAdditionalLight(lightIndex, input.positionWS, shadowMask);
                    half lightShadow = lerp(1.0h, light.shadowAttenuation, _ReceiveShadows);
                    realtimeLighting += light.color
                        * light.distanceAttenuation
                        * lightShadow
                        * LightFacing(normalWS, light.direction);
                LIGHT_LOOP_END

                half3 ambientLighting = SampleSH(normalWS) * _AmbientStrength;
                half3 sceneLighting = ambientLighting + realtimeLighting * _EnvironmentLightStrength;

                // Keep the original artistic fill light, then let real scene
                // lights illuminate and tint the sprite.
                half3 litColor = albedo * (1.0h + sceneLighting)
                    + _FillColor.rgb * _FillStrength;

                return half4(litColor * alpha, alpha);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
