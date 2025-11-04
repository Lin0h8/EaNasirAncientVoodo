Shader "Universal Render Pipeline/Pixelate"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _PixelSize("Pixel Size", Float) = 128
        _Color("Tint Color", Color) = (1,1,1,1)
        _PlayerPos("Player Position", Vector) = (0,0,0,0)
        _Range("LightRange", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;
            float _PixelSize;
            float4 _Color;
            float3 _PlayerPos;
            float _Range;
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);
               N = N * sign(dot(N, viewDirWS));
                float2 pixelUV = floor(IN.uv * _PixelSize) / _PixelSize;
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, pixelUV) * _Color;

                Light mainLight = GetMainLight();
                float3 L = normalize(mainLight.direction);
                float NdotL = dot(N, L);
                float3 lighting = texColor.rgb * mainLight.color.rgb * NdotL;

                #if defined(_ADDITIONAL_LIGHTS)

                uint addLightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < addLightCount; i++)
                {
                    Light add = GetAdditionalLight(i, IN.positionWS);
                    float3 L2 = normalize(add.direction);
                    float NdotL2 = dot(N, L2);
                    float3 torchColor = float3(0.6, 0.25, 0.1);
                    float delta = length(_PlayerPos - floor(IN.positionWS * _PixelSize) / _PixelSize);
                    float nDist = _Range / delta;
                    float attenuation = 1.0 / (nDist * nDist + 1.0); 
                    
                    
                    
                        lighting += texColor.rgb *NdotL2  * add.color.rgb * torchColor *add.distanceAttenuation;
                    
                    
                  
                    
                    
                    
                }
                #endif
                lighting += texColor.rgb * 0.01;

                return half4(lighting, texColor.a);

                
            }
            ENDHLSL
        }
    }
}