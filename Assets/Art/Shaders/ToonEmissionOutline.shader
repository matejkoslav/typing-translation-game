Shader "Custom/Toon Emission Outline"
{
    Properties
    {
        [Header(Farba)]
        _BaseMap ("Base Map (textura)", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)

        [Header(Toon tienovanie)]
        _ShadeColor ("Farba tiena", Color) = (0.55,0.55,0.7,1)
        _ShadeThreshold ("Hranica tiena", Range(-1,1)) = 0
        _ShadeSmoothness ("Makkost prechodu", Range(0.001,0.5)) = 0.02

        [Header(Emission)]
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color (cierna = vypnute)", Color) = (0,0,0,1)

        [Header(Outline)]
        _OutlineColor ("Farba obrysu", Color) = (0,0,0,1)
        _OutlineWidth ("Hrubka obrysu", Range(0,2)) = 0.3
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_BaseMap);     SAMPLER(sampler_BaseMap);
        TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4  _BaseColor;
            half4  _ShadeColor;
            half   _ShadeThreshold;
            half   _ShadeSmoothness;
            half4  _EmissionColor;
            half4  _OutlineColor;
            float  _OutlineWidth;
        CBUFFER_END
        ENDHLSL

        
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float  fogFactor  : TEXCOORD3;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS   = TransformObjectToWorldNormal(v.normalOS);
                o.uv         = TRANSFORM_TEX(v.uv, _BaseMap);
                o.fogFactor  = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                float3 n = normalize(i.normalWS);
                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));

                
                half NdotL = dot(n, light.direction);
                half lit = smoothstep(_ShadeThreshold - _ShadeSmoothness,
                                      _ShadeThreshold + _ShadeSmoothness, NdotL);
                lit *= step(0.5, light.shadowAttenuation); // ostre tiene od inych objektov

                half3 lighting = lerp(_ShadeColor.rgb, half3(1,1,1), lit) * light.color;
                half3 color = albedo.rgb * lighting;

                // Emission sa pricita navrch (nezavisi od svetla)
                color += SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, i.uv).rgb * _EmissionColor.rgb;

                color = MixFog(color, i.fogFactor);
                return half4(color, 1);
            }
            ENDHLSL
        }

        
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #pragma multi_compile_fog

            struct AttributesO
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct VaryingsO
            {
                float4 positionCS : SV_POSITION;
                float  fogFactor  : TEXCOORD0;
            };

            VaryingsO vertOutline (AttributesO v)
            {
                VaryingsO o;
                float4 posCS    = TransformObjectToHClip(v.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 normalCS = TransformWorldToHClipDir(normalWS);

                
                float2 offset = normalize(normalCS.xy + 1e-5);
                offset.x *= _ScreenParams.y / _ScreenParams.x;
                posCS.xy += offset * _OutlineWidth * 0.01 * posCS.w;

                o.positionCS = posCS;
                o.fogFactor  = ComputeFogFactor(posCS.z);
                return o;
            }

            half4 fragOutline (VaryingsO i) : SV_Target
            {
                return half4(MixFog(_OutlineColor.rgb, i.fogFactor), 1);
            }
            ENDHLSL
        }

        
        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
