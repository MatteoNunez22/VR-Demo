//
// GPGPU kernels for Stream
//
// Texture format:
// .xyz = particle position
// .w   = particle life
//
Shader "Hidden/Kvant/Stream/Kernel"
{
    Properties
    {
        _MainTex("-", 2D) = ""{}
        _FootprintTex("-", 2D) = ""{}
        _FootPos("-", Vector) = (10, 0, 0, 0)
        _CavePos("-", Vector) = (0, 0, 0, 0)
        _CavePosPrev("-", Vector) = (0, 0, 0, 0)
    }
    CGINCLUDE

    #pragma multi_compile NOISE_OFF NOISE_ON

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex, _FootprintTex;
    float4 _FootPos, _CavePos, _CavePosPrev;

    // Pass 0: Initialization
    float4 frag_init(v2f_img i) : SV_Target 
    {
        float4 p;
        // map uv to world position
        float2 worldPos = (i.uv - float2(0.5, 0.5)) * 2.4 - _CavePos.xz;
        //p.xyz = cnoise(float3(worldPos * 16, 0)) * 0.005f + cnoise(float3(worldPos * 4, 0)) * 0.005f;
        p.xyz = (float3)0;
        p.w = 1;
        return p;
    }

    // Pass 1: Update
    float4 frag_update(v2f_img i) : SV_Target 
    {
        // shift by cave position difference from last frame
        float2 cavePosDiff = _CavePos.xz - _CavePosPrev.xz;
        float2 prevSamplePos = i.uv - cavePosDiff / 2.4;
        float4 p;
        if (0 < prevSamplePos.x && prevSamplePos.x < 1 && 0 < prevSamplePos.y && prevSamplePos.y < 1) {
            p = tex2D(_MainTex, prevSamplePos);
        }
        else {
            p = frag_init(i);
        }
        float4 footPos;
        footPos = float4(_FootPos.xz, 0, 0);
        float4 footRect = float4(0.15, 0.32, 0, 0); // foot dimension in meters
        // map i.uv to normalized foot uv coordinate
        float4 worldCoord = (float4(float2(1, 1) - i.uv, 0, 0) - float4(0.5, 0.5, 0, 0)) * 2.4 + _CavePos.xzyw;
        float4 footCoord = ((worldCoord - footPos) / footRect) + float4(0.5, 0.5, 0, 0);
        if (0 <= footCoord.x && footCoord.x < 1 && 0 <= footCoord.y && footCoord.y < 1) {
            p += (tex2D(_FootprintTex, footCoord.xy) - float4(1, 1, 1, 0) * 127.0 / 255) * 0.005;
        }
        return p;
    }

    ENDCG

    SubShader
    {
        // Pass 0: Initialization
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_init
            ENDCG
        }
        // Pass 1: Update
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_update
            ENDCG
        }
    }
}
