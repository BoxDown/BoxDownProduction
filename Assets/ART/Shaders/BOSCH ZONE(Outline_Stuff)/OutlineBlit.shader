Shader "Hidden/OutlineBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Color ("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Pass
        {
            Blend One One
            ZTest Always
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_OutlineTarget);
            SAMPLER(sampler_OutlineTarget);
            float4 _OutlineTarget_TexelSize;

            float4 _Color;

            const static int outlineSize = 5;

            const static float2 vertices[] =
            {
                float2(-1.0, -1.0),
                float2(3.0, -1.0),
                float2(-1.0, 3.0),
            };

            Varyings vert(uint id : SV_VertexID)
            {
                Varyings output;
                output.vertex.xy = vertices[id];
                output.vertex.zw = float2(0.0, 1.0);
                
                output.uv = (output.vertex.xy + 1.0) / 2.0;
                output.uv.y = 1.0 - output.uv.y;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv); // Get Scene Color
                float outline = SAMPLE_TEXTURE2D(_OutlineTarget, sampler_OutlineTarget, input.uv).r; // Get Outline Color

                float expand = 0.0f;
                for (int i = 0; i < outlineSize * outlineSize; i++)
                {
                    int x = i % outlineSize; // Get X Offset from Index
                    int y = i / outlineSize; // Get Y Offset from Index

                    // Calculate uv, offset is multiplied by _OutlineTarget_TexelSize.xy to go from pixel to uv space.
                    float2 uv = input.uv + float2(x - outlineSize / 2, y - outlineSize / 2) * _OutlineTarget_TexelSize.xy;
                    // Add that to expand to get a simple box blur
                    expand += SAMPLE_TEXTURE2D(_OutlineTarget, sampler_OutlineTarget, uv).r / 9;
                }

                // Calculate the difference to get the outline, compare to low threshold for sharp edges, and multiply by user color.
                col += _Color * ((expand > 0.1) - (outline > 0.1));

                return col;
            }
            ENDHLSL
        }
    }
}