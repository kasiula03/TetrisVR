Shader "Unlit/GridDisplay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [ShowAsVector2] _Position("Position", Vector) = (0,0,0,0)
        _Width ("Wdith", float) = 1
        _MinY("MinY", float) = 0

        _BlockHighlightIntensive("Block Highlight Intensive", float) = 1.0
        _BlockColor("Block Color", Color) = (1,1,1,1)
        [ShowAsVector2] _FirstSegmentPosition("First Segment Position", Vector) = (0,0,0,0)
        [ShowAsVector2] _SecondSegmentPosition("Second Segment Position", Vector) = (0,0,0,0)
        [ShowAsVector2] _ThirdSegmentPosition("Third Segment Position", Vector) = (0,0,0,0)
        [ShowAsVector2] _FourthSegmentPosition("Fourth Segment Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        LOD 100


        Pass
        {
            ZWrite Off

            Blend One One

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _Position;
            float _Width;
            float _MinY;

            float4 _BlockColor;
            float _BlockHighlightIntensive;
            float2 _FirstSegmentPosition;
            float2 _SecondSegmentPosition;
            float2 _ThirdSegmentPosition;
            float2 _FourthSegmentPosition;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float CheckUvForPosition(float2 uv, float2 position)
            {
                int blockSize = 1;
                return uv.x > position.x && uv.x < position.x + blockSize && uv.y > position.y && uv.y < position.y +
                    blockSize;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);
                float uvByTileX = i.uv.x;
                float uvByTileY = i.uv.y;
                float visible = uvByTileX > _Position.x && uvByTileX < _Position.x + _Width && uvByTileY < _Position.y
                    && uvByTileY > _MinY;

                float blockVisible = CheckUvForPosition(i.uv, _FirstSegmentPosition) ||
                    CheckUvForPosition(i.uv, _SecondSegmentPosition) || CheckUvForPosition(i.uv, _ThirdSegmentPosition)
                    || CheckUvForPosition(i.uv, _FourthSegmentPosition);

                if(blockVisible)
                {
                    return col * _BlockColor * blockVisible * _BlockHighlightIntensive;
                }

                return col * visible;
            }
            ENDCG
        }
    }
}