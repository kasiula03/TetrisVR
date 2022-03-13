Shader "Unlit/StoneBlock"
{
    Properties
    {
        _DissolveTex ("Dissolve Texture", 2D) = "black" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _Color("Main Color", Color) = (1,1,1,1)
        _BlendColor("Blend Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;

            float4 _Color;
            float4 _BlendColor;
            float _DissolveAmount;

            Interpolator vert(MeshData v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DissolveTex);

                return o;
            }


            float4 frag(Interpolator i) : SV_Target
            {
                float4 dissolve = tex2D(_DissolveTex, i.uv).r;
                dissolve = dissolve * 0.999;

                float dAmount = _DissolveAmount * 2;
                float4 dissolveBlend = saturate(dissolve - (1 - dAmount));
                
                return lerp(_Color, _BlendColor, dissolveBlend.x);
            }
            ENDCG
        }

    }
}