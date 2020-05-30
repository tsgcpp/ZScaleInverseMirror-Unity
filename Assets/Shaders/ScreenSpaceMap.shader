Shader "ScreenSpaceMap"
{
    Properties
	{
		_MainTex ("Texture", any) = "" {}
	}
    
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // texture arrays are not available everywhere,
            // only compile shader on platforms where they are
            #pragma require 2darray
            
            #include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			//v2f output struct

			struct v2f
			{

				float4 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = ComputeScreenPos(o.vertex);

				return o;
			}

			sampler2D _MainTex;
			half4 _MainTex_ST;

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2Dproj(_MainTex, i.uv);
			}

            ENDCG
        }
    }
}