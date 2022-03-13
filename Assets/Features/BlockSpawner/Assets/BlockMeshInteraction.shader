Shader "Unlit/BlockMeshInteraction"
{
    Properties
    {
        _DisplacementX ("Displacement X", Range(-1,1)) = 0
        _DisplacementY ("Displacement Y", Range(-1,0)) = 0
        _Rotation ("Rotation", Range(-90, 90) ) = 0
        _WorldScaleInversed ("WorldScaleInversed", Vector) = (1,1,1)
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

            static const float3 objectInverseScale = float3(
                1 / length(unity_ObjectToWorld._m00_m10_m20),
                1 / length(unity_ObjectToWorld._m01_m11_m21),
                1 / length(unity_ObjectToWorld._m02_m12_m22)
            );

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 tangent : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            float _DisplacementX;
            float _DisplacementY;
            float _Rotation;
            float3 _WorldScaleInversed;

            float4 RotateAroundZInDegrees(float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xy), vertex.zw).xyzw;
            }

            Interpolator vert(MeshData v)
            {
                float2 displacementVec = float2(_DisplacementX, _DisplacementY);

                float4 calcDisplacement = mul(transpose(unity_ObjectToWorld),
                                              float2(displacementVec.x, displacementVec.y));

                calcDisplacement.y *= _WorldScaleInversed.x;
                calcDisplacement.x *= _WorldScaleInversed.y;
                calcDisplacement.z *= _WorldScaleInversed.z;

                v.vertex = RotateAroundZInDegrees(v.vertex, _Rotation);

                Interpolator o;
                o.vertex = UnityObjectToClipPos(v.vertex + calcDisplacement);
                o.uv = v.uv;
                o.normal = v.normal;
                o.tangent = v.tangent;
                return o;
            }

            float4 frag(Interpolator i) : SV_Target
            {
                const float disYPercent = (-_DisplacementY) / 2;
                const float disXPercent = (_DisplacementX) / 2;
                const float rotationPercent = _Rotation / 90;

                float dis = 0;
                if (disYPercent != 0)
                {
                    dis = disYPercent;
                }
                else if (disXPercent != 0)
                {
                    dis = abs(disXPercent);
                }
                else if (rotationPercent != 0)
                {
                    dis = abs(rotationPercent);
                }


                return float4(1, 1, 1, 1) * dis;
            }
            ENDCG
        }
    }
}