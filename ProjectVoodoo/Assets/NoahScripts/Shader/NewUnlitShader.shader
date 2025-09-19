Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Float) = 16
        _LightPixelationLevel ("Light Pixelation", Int) = 4
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "UniversalRenderPipeline" = "true" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
           
            float _PixelSize;
            int _LightPixelationLevel;
           
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdadd_fullshadows
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
           
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainTex);
            float4 _MainTex_ST;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            
            
            v2f vert (appdata v)
            {
                
                v2f o;
                float grid = 0.05;
                float3 pos = v.vertex.xyz;
                pos = round(pos / grid) * grid;
                //o.vertex = UnityObjectToClipPos(float4(pos,1.0));
                o.vertex = TransformObjectToHClip(float4(pos,1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.positionWS  = TransformObjectToWorld(v.vertex).xyz;
                return o;
            }
            /* float4 col = tex2D(_MainTex, uv);
                // RetroShading
                float3 normal = normalize(i.normal);
                int count = GetAdditionalLightsCount();
                for (int i = 0; i< count; i++){
                    AdditionalLight light = GetAdditionalLight(i);
                        // light properties
                    float3 lightColor = light.color;
                    float3 lightDir;

                    if (light.lightType == 0) // directional
                        lightDir = normalize(light.direction);
                    else // point/spot
                        lightDir = normalize(light.position - worldPos);

                    // Lambert diffuse
                    float NdotL = max(0, dot(normalWS, lightDir));
                    col.rgb += lightColor * NdotL; // additive
                }
                
                

                col.rgb *= NdotL;*/
            float4 frag (v2f i) : SV_Target
            {
                // Sample texture
                float2 uv = floor(i.uv * _PixelSize) / _PixelSize;
                float3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

                float3 normal = normalize(i.normal);
                // Light MainLight = GetMainLight();
                // float3 mainLightDir = normalize(MainLight.direction);
                // float NdotL = max(0, dot(normal, mainLightDir));
                // float3 totalLight = NdotL * _MainLightColor.rgb; // from main directional light
               Light mainLight = GetMainLight();
float3 L = normalize(mainLight.direction);
float NdotL = max(0, dot(normal, L));
float3 totalLight = mainLight.color * NdotL;

                int addCount = GetAdditionalLightsCount();
                for (int j = 0; j < addCount; j++)
                {
                    // Light add = GetAdditionalLight((uint)j, i.vertex);
                    // float3 addContrib = LightingLambert(add.color, add.direction, normal);
                    // addContrib *= add.distanceAttenuation * add.shadowAttenuation;
                    // totalLight += addContrib;
                    Light add = GetAdditionalLight(j, i.positionWS);
                    // float3 Ladd = normalize(-add.direction);
                    // float NdotLAdd = max(0, dot(normal, add.direction));
                    float3 L = normalize(add.direction);
                    float NdotL = max(0,dot(normal, L));
                    //float distance = length(add.position - i.positionWS);
                    //float att = saturate(1.0 - distance / (1 * 10));
                    float3 diffuse = add.color.rgb * NdotL * add.distanceAttenuation * add.shadowAttenuation;
                    //diffuse = floor(colo * _LightPixelationLevel) / _LightPixelationLevel;
                    //diffuse = floor(diffuse * _LightPixelationLevel) / _LightPixelationLevel;
                    //diffuse = clamp(diffuse, 0, );
                    totalLight += diffuse;
                }
                totalLight = saturate(totalLight);
                //totalLight = round(totalLight * _LightPixelationLevel) / _LightPixelationLevel;
                
                // apply fog
                //col.rgb = ApplyFog(col.rgb, fogFactor, _FogParams);
               float3 color = albedo  * totalLight;
               color = floor(color * _LightPixelationLevel) / _LightPixelationLevel;
              
               return float4(color, 1);

            }
            ENDHLSL
        }
    }
}
