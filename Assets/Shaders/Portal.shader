Shader "Custom/Portal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _InnerRadius ("Inner Radius", Range(0, 1)) = 0.3
        _OuterRadius ("Outer Radius", Range(0, 1)) = 0.8
        _Falloff ("Edge Falloff", Range(0.01, 0.5)) = 0.1
        _Glow ("Glow Intensity", Range(0, 5)) = 2
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2
        _NoiseSpeed ("Noise Speed", Range(0, 5)) = 1
        _DistortionStrength ("Distortion Strength", Range(0, 0.5)) = 0.1
    }
    
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _InnerRadius;
            float _OuterRadius;
            float _Falloff;
            float _Glow;
            float _NoiseScale;
            float _NoiseSpeed;
            float _DistortionStrength;

            // Simple noise function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f); // Smooth interpolation
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Convert UV to centered coordinates (-0.5 to 0.5)
                float2 centeredUV = i.uv - 0.5;
                
                // Add animated noise distortion
                float time = _Time.y * _NoiseSpeed;
                float2 noiseUV = centeredUV * _NoiseScale + time * 0.1;
                float noiseValue = smoothNoise(noiseUV) * 2.0 - 1.0;
                centeredUV += noiseValue * _DistortionStrength;
                
                // Calculate distance from center
                float distanceFromCenter = length(centeredUV);
                
                // Create donut mask
                float donutMask = 1.0 - smoothstep(_InnerRadius - _Falloff, _InnerRadius, distanceFromCenter);
                donutMask *= smoothstep(_OuterRadius + _Falloff, _OuterRadius, distanceFromCenter);
                
                // Add swirling effect
                float angle = atan2(centeredUV.y, centeredUV.x);
                float swirl = sin(angle * 6.0 + time * 2.0 + distanceFromCenter * 10.0) * 0.5 + 0.5;
                
                // Add radial energy lines
                float radialLines = sin(angle * 20.0 + time * 3.0) * 0.3 + 0.7;
                
                // Combine effects
                float finalMask = donutMask * swirl * radialLines;
                
                // Add pulsing glow
                float pulse = sin(time * 4.0) * 0.3 + 0.7;
                finalMask *= pulse;
                
                // Sample texture and apply color
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = _Color * tex;
                
                // Apply glow and transparency
                col.rgb *= _Glow * finalMask;
                col.a = finalMask * _Color.a;
                
                // Add extra brightness to edges for rim lighting
                float edgeBrightness = 1.0 - smoothstep(0.0, 0.2, abs(distanceFromCenter - (_InnerRadius + _OuterRadius) * 0.5));
                col.rgb += edgeBrightness * _Color.rgb * 0.5;
                
                return col;
            }
            ENDCG
        }
    }
}