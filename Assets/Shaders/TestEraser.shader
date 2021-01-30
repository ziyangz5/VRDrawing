// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/TestEraser"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OrgTex ("Org Texture", 2D) = "white" {}
		_BrushTex("Brush Texture",2D)= "white" {}
		_BrushBaseTex("Brush Base Texture",2D)= "white" {}
		_Color("Color",Color)=(1,1,1,1)
		_UV("UV",Vector)=(0,0,0,0)
		_Size("Size",Range(1,1000))=1
		_EraserCoeff("Eraser Coefficent",float)=1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
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

			sampler2D _MainTex;
			sampler2D _OrgTex;
			float4 _MainTex_ST;
			sampler2D _BrushTex;
			sampler2D _BrushBaseTex;
			fixed4 _UV;
			float _Size;
			fixed4 _Color;
			float _EraserCoeff;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float size = _Size;
				float2 uv = i.uv + (0.5f/size);
				float2 base_uv = i.uv*5;
				uv = uv - _UV.xy;
				uv *= size;
				float4 col = tex2D(_BrushTex,uv);
				if (col.a < 0.001)
				{
					return fixed4(0,0,0,0);
				}
				col.rgb = 1;
				col *= _Color;
				col.a = 0.01;
				return col;
			}
			ENDCG
		}
	}
}
