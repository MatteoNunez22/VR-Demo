// http://polycount.com/discussion/117185/creating-normals-from-alpha-heightmap-inside-a-shader

Shader "Height/Normal Map From Height" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _FootprintTex("Footprint Texture", 2D) = "grey" {}
        _MainTex("Diffuse (RGB) Alpha (A)", 2D) = "white" {}
        _BumpMap("Normal (Normal)", 2D) = "bump" {}
        _HeightMap("Heightmap (R)", 2D) = "grey" {}
        _HeightmapStrength("Heightmap Strength", Float) = 1.0
        _HeightmapDimX("Heightmap Width", Float) = 2048
        _HeightmapDimY("Heightmap Height", Float) = 2048
        _FootPosition("Foot Position", Vector) = (0, 0, 0, 0)
    }

        SubShader{
            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }

        CGPROGRAM

#pragma surface surf NormalsHeight alpha
#pragma target 3.0

    struct Input
    {
        float2 uv_MainTex;
    };

    sampler2D _MainTex, _BumpMap, _HeightMap, _FootprintText;
    float _HeightmapStrength, _HeightmapDimX, _HeightmapDimY;
    float4 _Color, _FootPosition;

    void surf(Input IN, inout SurfaceOutput o)
    {
        o.Albedo = tex2D(_MainTex, IN.uv_MainTex);

        float3 normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

        float me = tex2D(_HeightMap, IN.uv_MainTex).x;
        float n = tex2D(_HeightMap, float2(IN.uv_MainTex.x, IN.uv_MainTex.y + 1.0 / _HeightmapDimY)).x;
        float s = tex2D(_HeightMap, float2(IN.uv_MainTex.x, IN.uv_MainTex.y - 1.0 / _HeightmapDimY)).x;
        float e = tex2D(_HeightMap, float2(IN.uv_MainTex.x - 1.0 / _HeightmapDimX, IN.uv_MainTex.y)).x;
        float w = tex2D(_HeightMap, float2(IN.uv_MainTex.x + 1.0 / _HeightmapDimX, IN.uv_MainTex.y)).x;

        float3 norm = normal;
        float3 temp = norm; //a temporary vector that is not parallel to norm
        if (norm.x == 1)
            temp.y += 0.5;
        else
            temp.x += 0.5;

        //form a basis with norm being one of the axes:
        float3 perp1 = normalize(cross(norm,temp));
        float3 perp2 = normalize(cross(norm,perp1));

        //use the basis to move the normal in its own space by the offset
        float3 normalOffset = -_HeightmapStrength * (((n - me) - (s - me)) * perp1 + ((e - me) - (w - me)) * perp2);
        norm += normalOffset;
        norm = normalize(norm);

        o.Normal = norm;
    }

    inline fixed4 LightingNormalsHeight(SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
    {
        viewDir = normalize(viewDir);
        lightDir = normalize(lightDir);
        s.Normal = normalize(s.Normal);
        float NdotL = dot(s.Normal, lightDir);
        //_LightColor0.rgb = _LightColor0.rgb;
        _LightColor0.rgb = _Color;

        fixed4 c;
        c.rgb = float3(0.5, 0.5, 0.5) * saturate(NdotL) * _LightColor0.rgb * atten;
        c.a = 0.5;
        return c;
    }

    ENDCG
    }
        FallBack "VertexLit"
}