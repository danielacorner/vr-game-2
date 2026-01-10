Shader "VRDungeonCrawler/PortalDistortion"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1.0
        _FresnelPower ("Fresnel Power", Range(0, 10)) = 3.0
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.8, 1, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "PortalDistortion"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _DistortionStrength;
                float _DistortionSpeed;
                float _FresnelPower;
                float4 _FresnelColor;
                float _EmissionIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Animated distortion
                float2 distortion = float2(
                    sin(_Time.y * _DistortionSpeed + input.uv.y * 10.0),
                    cos(_Time.y * _DistortionSpeed * 1.3 + input.uv.x * 10.0)
                ) * _DistortionStrength;

                float2 distortedUV = input.uv + distortion;

                // Sample texture with distortion
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);

                // Fresnel effect
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);

                // Combine color with fresnel
                half3 finalColor = col.rgb + _FresnelColor.rgb * fresnel * _EmissionIntensity;

                // Pulsing alpha
                float pulse = sin(_Time.y * 2.0) * 0.2 + 0.8;
                half alpha = col.a * pulse * (0.5 + fresnel * 0.5);

                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
