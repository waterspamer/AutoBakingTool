Shader "Unlit/Lightmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (.8, .8, .8, 1)
        _DirectLightMap ("Lightmap Direct", 2D) = "white" {}
        _IndirectLightMap ("Lightmap Indirect", 2D) = "white" {}
        
        _IndirectMultiplier ("Multiplier", float) = 1

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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _IndirectLightMap;
            sampler2D _DirectLightMap;
            float _IndirectMultiplier;
            
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
                fixed4 lmCol = tex2D(_IndirectLightMap, i.uv1) * _IndirectMultiplier + tex2D(_DirectLightMap, i.uv1);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col * lmCol;
            }
            ENDCG
        }
    }
}
