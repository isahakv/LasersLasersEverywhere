Shader "Unlit/TestUnlitShader"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		_WorldPosition("World Position", Vector) = (0, 0, 0, 0)
		_SphereRadius("Sphere Radius", Range(0, 20)) = 10.0
		_SphereSoftness("Sphere Softness", Range(0, 10)) = 1.0
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

			fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _WorldPosition;
			half _SphereRadius;
			half _SphereSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

				// Distance btw "_WorldPosition" property to pixel world position;
				half dist = distance(_WorldPosition, i.worldPos);
				half min = _SphereRadius - _SphereSoftness;
				half alpha = saturate((dist - min) / _SphereSoftness);

				fixed4 lerpedColor = lerp(col, fixed4(col.r, col.g, col.b, 0), alpha);

                return lerpedColor;
            }
            ENDCG
        }
    }
}
