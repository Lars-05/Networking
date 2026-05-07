Shader "Unlit/ScrollingCheckerboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Size ("Size", Float) = 10.0
        _Color1 ("Color 1", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (0,0,0,1)
        _BorderColor ("Border Color", Color) = (0,0,0,1)
        _BorderThreshold ("Border Threshold", Float) = 1.0
        _ScrollingSpeed ("Scrolling Speed", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            float _ScrollingSpeed;
            float _Size;
            float  _BorderThreshold;
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4  _BorderColor;
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                   float2 uv = i.uv + float2(_Time.y *_ScrollingSpeed, 0);
                float2 Pos = floor(uv * _Size);
                float PatternMask = fmod(Pos.x + Pos.y, 2.0) * 0.5;
                

                
         

            if (i.uv.y < _BorderThreshold * 0.1 ||  i.uv.y > 1 -_BorderThreshold * 0.1)
            {
                return _BorderColor;
            }
            if(i.uv.x < _BorderThreshold * 0.1 || i.uv.x > 1 -_BorderThreshold * 0.1)
            {
                return _BorderColor;
            }
                    
                    
                    
                fixed4 col = lerp(_Color1, _Color2, PatternMask);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}