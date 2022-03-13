Shader "Unlit/StoneBlockByHeight"
{
    Properties
    {
        _Height ("Height", Float) = 0
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
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };


            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;

            float4 _Color;
            float4 _BlendColor;
            float _Height;

            Interpolator vert(MeshData v)
            {
                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }


            float4 frag(Interpolator i) : SV_Target
            {
                float distance = saturate(_Height - i.worldPos.y);
                return lerp(_Color, _BlendColor, distance);
            }
            ENDCG
        }
    }
}