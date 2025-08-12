Shader "Custom/VoidPortal"
{
    Properties
    {
        _MainTex ("Background Texture", 2D) = "black" {}
        _DistortionTex ("Distortion Noise", 2D) = "bump" {}
        _VoidColor ("Void Color", Color) = (0, 0, 0.1, 0.8)
        _EdgeGlow ("Edge Glow Color", Color) = (0.2, 0.4, 1, 1)
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.1
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1
        _EdgeWidth ("Edge Glow Width", Range(0.01, 0.3)) = 0.1
        _EdgePower ("Edge Glow Power", Range(1, 10)) = 3
        _DepthFade ("Depth Fade Distance", Range(0.1, 10)) = 2
        _Transparency ("Transparency", Range(0, 1)) = 0.7
        _VortexStrength ("Vortex Strength", Range(0, 2)) = 0.5
        _VortexSpeed ("Vortex Speed", Range(0, 5)) = 1
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent-100"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Front  // Render inside faces
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                UNITY_FOG_COORDS(5)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _DistortionTex;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float4 _MainTex_ST;
            float4 _DistortionTex_ST;
            
            fixed4 _VoidColor;
            fixed4 _EdgeGlow;
            float _DistortionStrength;
            float _DistortionSpeed;
            float _EdgeWidth;
            float _EdgePower;
            float _DepthFade;
            float _Transparency;
            float _VortexStrength;
            float _VortexSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert to centered UV coordinates
                float2 centeredUV = i.uv - 0.5;
                float distanceFromCenter = length(centeredUV);
                
                // Calculate angle for vortex effect
                float angle = atan2(centeredUV.y, centeredUV.x);
                
                // Create vortex distortion
                float time = _Time.y * _VortexSpeed;
                float vortexOffset = _VortexStrength * distanceFromCenter * sin(time + distanceFromCenter * 10.0);
                float2 vortexUV = float2(
                    cos(angle + vortexOffset) * distanceFromCenter,
                    sin(angle + vortexOffset) * distanceFromCenter
                ) + 0.5;
                
                // Sample distortion texture with animation
                float2 distortionUV = vortexUV * _DistortionTex_ST.xy + _Time.y * _DistortionSpeed * float2(0.1, 0.05);
                float3 distortion = tex2D(_DistortionTex, distortionUV).rgb - 0.5;
                
                // Apply distortion to UV
                float2 distortedUV = vortexUV + distortion.xy * _DistortionStrength;
                
                // Sample the main texture (could be starfield or noise)
                fixed4 tex = tex2D(_MainTex, distortedUV);
                
                // Calculate depth fade for soft intersection
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV));
                float partZ = i.screenPos.z;
                float fade = saturate(_DepthFade * (sceneZ - partZ));
                
                // Calculate fresnel for edge glow
                float fresnel = 1.0 - saturate(dot(i.worldNormal, i.viewDir));
                float edgeGlow = pow(fresnel, _EdgePower);
                
                // Create radial gradient from center
                float radialMask = 1.0 - smoothstep(0.0, 1.0, distanceFromCenter);
                
                // Combine void color with edge glow
                fixed4 voidColor = _VoidColor;
                voidColor.rgb += _EdgeGlow.rgb * edgeGlow * _EdgeWidth;
                
                // Add subtle animated patterns
                float pattern1 = sin(angle * 8.0 + time * 2.0) * 0.1 + 0.9;
                float pattern2 = sin(distanceFromCenter * 20.0 - time * 3.0) * 0.1 + 0.9;
                
                // Combine all effects
                fixed4 finalColor = voidColor;
                finalColor.rgb *= tex.rgb * pattern1 * pattern2;
                finalColor.rgb += _EdgeGlow.rgb * edgeGlow * 0.5;
                
                // Apply transparency and depth fade
                finalColor.a *= _Transparency * fade * radialMask;
                
                // Add some "depth" illusion
                float depthIllusion = pow(1.0 - distanceFromCenter, 2.0);
                finalColor.rgb *= depthIllusion;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
    }
}