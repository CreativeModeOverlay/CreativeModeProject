Shader "Hidden/FadeInOutTransition"
{
    Properties
    {
        _FromTex ("From", 2D) = "white" {}
        _ToTex ("To", 2D) = "white" {}
        _Blend ("Blend", float) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _FromTex;
            sampler2D _ToTex;
            float _Blend;

            fixed4 frag (v2f i) : SV_Target
            {
                return lerp(tex2D(_FromTex, i.uv), tex2D(_ToTex, i.uv), _Blend);
            }
            ENDCG
        }
    }
}
