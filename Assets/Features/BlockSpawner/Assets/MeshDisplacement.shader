Shader "Unlit/MeshDisplacement"
{
    Properties
    {
        _DisplacementX ("Displacement X", Range(-1,1)) = 0
        _DisplacementY ("Displacement Y", Range(-1,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float3 normal : NORMAL;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _DisplacementX;
            float _DisplacementY;

            Interpolator vert(MeshData v)
            {
                float2 displacementVec = float2(_DisplacementX, _DisplacementY);

                float4 calcDisplacementY = mul(transpose(unity_ObjectToWorld),
                                               float2(displacementVec.x, displacementVec.y));
                calcDisplacementY.y *= 10; // Because of the scale
                calcDisplacementY.x *= 10;

                v.vertex += calcDisplacementY;


                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(Interpolator i) : SV_Target
            {
                const float disYPercent = (-_DisplacementY) / 2;
                const float disXPercent = (_DisplacementX) / 2;

                float dis = 0;
                if (disYPercent != 0)
                {
                    dis = disYPercent;
                }
                else if(disXPercent != 0)
                {
                    dis = abs(disXPercent);
                }
                
                return float4(1, 1, 1, 1) * dis;
            }
            ENDCG
        }
    }
}