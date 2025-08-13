Shader "Custom/Invisible"
{
    Properties
    {
        [Header(Invisible Settings)]
        [Toggle] _CompletelyInvisible ("Completely Invisible", Float) = 1
        [Range(0,1)] _Alpha ("Alpha Override", Range(0,1)) = 0
        
        [Header(Debug)]
        [Toggle] _ShowInSceneView ("Show in Scene View", Float) = 0
        _DebugColor ("Debug Color", Color) = (1,0,1,0.3)
        
        [Header(Depth)]
        [Toggle] _WriteDepth ("Write to Depth Buffer", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("Z Test", Float) = 4 // LEqual
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        
        Pass
        {
            Name "InvisiblePass"
            Tags { "LightMode" = "UniversalForward" }
            
            // Blend settings for transparency
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite [_WriteDepth]
            ZTest [_ZTest]
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // Properties
            CBUFFER_START(UnityPerMaterial)
                float _CompletelyInvisible;
                float _Alpha;
                float _ShowInSceneView;
                float4 _DebugColor;
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
                float3 positionWS : TEXCOORD1;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = input.uv;
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                // Completely invisible - discard all pixels
                if (_CompletelyInvisible > 0.5)
                {
                    discard;
                }
                
                float4 color = float4(0, 0, 0, _Alpha);
                
                // Debug mode - show colored overlay in scene view
                #if UNITY_EDITOR
                if (_ShowInSceneView > 0.5)
                {
                    // Check if we're in scene view (approximate)
                    float3 viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                    color = _DebugColor;
                    color.a = _DebugColor.a;
                }
                #endif
                
                return color;
            }
            ENDHLSL
        }
        
        // Shadow caster pass (optional - for casting shadows while invisible)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        
        // Depth only pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
            ZWrite [_WriteDepth]
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
    
    // Fallback for older Unity versions
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}