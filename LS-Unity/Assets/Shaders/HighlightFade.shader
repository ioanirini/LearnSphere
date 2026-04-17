Shader "Custom/HighlightFade"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _FadeStart ("Fade Start (World Y)", Float) = 0
        _FadeEnd ("Fade End (World Y)", Float) = 2
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float worldY : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _Color;
            float _FadeStart;
            float _FadeEnd;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Convert to clip space
                o.pos = UnityObjectToClipPos(v.vertex);

                // Get WORLD SPACE position
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldY = worldPos.y;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Prevent divide-by-zero
                float denom = max(_FadeEnd - _FadeStart, 0.0001);

                // Normalize height into 0–1 range
                float t = saturate((i.worldY - _FadeStart) / denom);

                // Invert so it fades OUT as it goes UP
                float alpha = 1.0 - t;

                return float4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}