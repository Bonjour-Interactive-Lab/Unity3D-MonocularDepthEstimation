Shader "Hidden/DepthViewer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MinDepth("Min Depth", Float) = 0.25
        _MaxDepth("Max Depth", Float) = 4.0
        _Ramp("Ramp", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
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

            sampler2D _MainTex;
            sampler2D _Ramp;
            float _MinDepth;
            float _MaxDepth;
           
            float4 frag (v2f i) : SV_Target
            {
                float depth     = tex2D(_MainTex, i.uv).r;
                // depth           = clamp(depth, _MinDepth, _MaxDepth);
                float normDepth = (depth - _MinDepth) / (_MaxDepth - _MinDepth);
                normDepth       = saturate(normDepth);

                float3 color    = tex2D(_Ramp, float2(normDepth, 0.5)).rgb;
                return float4(color, 1);
            }
            ENDCG
        }
    }
}
