Shader "Unlit/BlockEffect"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (0, 0.66, 0.73, 1)
        _Height ("Height", Float) = 0
        _BlendColor("Blend Color", Color) = (1,1,1,1)

        _DissolveTex ("Dissolve Texture", 2D) = "black" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _DissolveColor("Dissolve Color", Color) = (1,1,1,1)

        _WaveAmplitude("Wave Apmlitude", float) = 0.3
        _WaveShift("Wave Shift", float) = -1

        _Smoothness ("Smoothness", Float) = 0.5

        [Toggle(_ALPHATEST_ON)] _EnableAlphaTest("Enable Alpha Cutoff", Float) = 0.0
        _Cutoff ("Alpha Cutoff", Float) = 0.5

        [Toggle(_NORMALMAP)] _EnableBumpMap("Enable Normal/Bump Map", Float) = 0.0
        _BumpMap ("Normal/Bump Texture", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Float) = 1

        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
        _EmissionColor ("Emission Colour", Color) = (0, 0, 0, 0)

    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            #define TAU 6.28318530718

            #pragma shader_feature _EMISSION
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };


            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;

                DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

                // Note this macro is using TEXCOORD1
                #ifdef REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
                float3 positionWS : TEXCOORD2;
                #endif
                float3 normalWS : TEXCOORD3;
                #ifdef _NORMALMAP
                float4 tangentWS : TEXCOORD4;
                #endif
                float3 viewDirWS : TEXCOORD5;

                float2 uv_dissolve : TEXCOORDS6;

                half4 fogFactorAndVertexLight : TEXCOORD7;
                #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
    float4 shadowCoord              : TEXCOORD8;
                #endif
            };


            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST; // Texture tiling & offset inspector values
            float4 _Color;
            float _BumpScale;
            float4 _BlendColor;
            float _Height;

            float _Smoothness;
            float _Cutoff;

            float4 _EmissionColor;

            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _DissolveAmount;
            float4 _DissolveColor;

            float _WaveAmplitude;
            float _WaveShift;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Vertex Position
                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = positionInputs.positionCS;
                #ifdef REQUIRES_WORLD_SPACE_POS_INTERPOLATOR
                OUT.positionWS = positionInputs.positionWS;
                #endif
                // UVs & Vertex Colour
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.uv_dissolve = TRANSFORM_TEX(IN.uv, _DissolveTex);
                OUT.color = IN.color;

                // View Direction
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);

                // Normals & Tangents
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInputs.normalWS;

                half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
                half fogFactor = ComputeFogFactor(positionInputs.positionCS.z);
                OUT.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

                // Baked Lighting & SH (used for Ambient if there is no baked)
                OUTPUT_LIGHTMAP_UV(IN.lightmapUV, unity_LightmapST, OUT.lightmapUV);
                OUTPUT_SH(OUT.normalWS.xyz, OUT.vertexSH);

                // Shadow Coord
                #ifdef REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
                OUT.shadowCoord = GetShadowCoord(positionInputs);
                #endif
                return OUT;
            }

            InputData InitializeInputData(Varyings IN, half3 normalTS)
            {
                InputData inputData = (InputData)0;

                #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
                inputData.positionWS = IN.positionWS;
                #endif

                half3 viewDirWS = SafeNormalize(IN.viewDirWS);
                #ifdef _NORMALMAP
                float sgn = IN.tangentWS.w; // should be either +1 or -1
                float3 bitangent = sgn * cross(IN.normalWS.xyz, IN.tangentWS.xyz);
                inputData.normalWS = TransformTangentToWorld(
                    normalTS, half3x3(IN.tangentWS.xyz, bitangent.xyz, IN.normalWS.xyz));
                #else
                inputData.normalWS = IN.normalWS;
                #endif
                inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
                inputData.viewDirectionWS = viewDirWS;

                inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
                inputData.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, inputData.normalWS);
                return inputData;
            }

            SurfaceData InitializeSurfaceData(Varyings IN, float4 color)
            {
                SurfaceData surfaceData = (SurfaceData)0;
                half4 albedoAlpha = SampleAlbedoAlpha(IN.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
                surfaceData.alpha = Alpha(albedoAlpha.a, color, _Cutoff);
                surfaceData.albedo = albedoAlpha.rgb * color.rgb * IN.color.rgb;
                surfaceData.emission = SampleEmission(IN.uv, _EmissionColor.rgb,
                                                      TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = SampleNormal(IN.uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
                surfaceData.occlusion = 1;
                return surfaceData;
            }

            half4 CalculateColor(Varyings IN, float4 baseColor)
            {
                SurfaceData surfaceData = InitializeSurfaceData(IN, baseColor);
                InputData inputData = InitializeInputData(IN, surfaceData.normalTS);

                half4 color = UniversalFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic,
                                                   surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion,
                                                   surfaceData.emission, surfaceData.alpha);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = saturate(color.a);
                return color;
            }

            float InverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            float startValue = -1;
            float endValue = 2;

            float4 frag(Varyings IN) : SV_Target
            {
                 
                float2 uvsCentered = IN.uv * 2 - 1;
                float t = cos(IN.uv.x * TAU * 5.0f) * 0.5 + 0.5;
              
                float colorStep =  t * float4(uvsCentered.xxx, 1) + _WaveShift;
                
            
                float yNormal = abs(IN.normalWS.y);
                float xNormal = abs(IN.normalWS.x);
                float shineEffect = colorStep * (yNormal < 0.999) * (xNormal < 0.999);

                //return shineEffect;

                float4 shineColor = CalculateColor(IN, _DissolveColor * (shineEffect * 4.0f));


                half4 color = CalculateColor(IN, _Color);
                half4 blendColor = CalculateColor(IN, _BlendColor);
                half4 dissolveColor = CalculateColor(IN, _DissolveColor);

                float distance = saturate(_Height - IN.positionWS.y);
                float4 blendedByDistance = lerp(color, blendColor, distance);

                float4 dissolve = tex2D(_DissolveTex, IN.uv_dissolve).r;
                dissolve = dissolve * 0.999;

                float dAmount = _DissolveAmount * 2;
                float4 dissolveBlend = saturate(dissolve - (1 - dAmount));

                // if(shineColor.a > 0)
                // {
                //     return shineColor;
                // }
                return lerp(blendedByDistance, dissolveColor, dissolveBlend.x);
            }
            ENDHLSL

        }
    }
}