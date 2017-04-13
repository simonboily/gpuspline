// Copyright (C) 2017 Simon Boily

Shader "Custom/Spline Unlit"
{
	Properties
	{
		[NoScaleOffset] _MainTex ("Base (RGB), Alpha (A)", 2D) = "black" {}
		_Depth ("Spline Z Value", Float) = 0.0
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
		}

		Pass
		{
			Cull Off
			Lighting Off
			ZWrite On
			Fog { Mode Off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D	_MainTex;
			float4		_ControlPoints[1000];
			float		_Width;
			float		_Depth;
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				half4 color : COLOR;
			};

			struct v2f {
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				int index = v.vertex.z;
				float t = v.vertex.y;

				float2 cp0 = _ControlPoints[index].xy;
				float2 cp1 = _ControlPoints[index +1].xy;
				float2 cp2 = _ControlPoints[index +2].xy;
				float2 cp3 = _ControlPoints[index +3].xy;

				float2 base0 = -cp0 + cp3 + (cp1 - cp2) * 3;
				float2 base1 = 2*cp0 - 5*cp1 + 4*cp2 - cp3;

				float2 pos = 0.5 * (base0 * (t*t*t) + base1 * (t*t) + (-cp0+cp2)*t + 2 * cp1);
				float2 tang = base0 * (t * t * 1.5) + base1 * t + 0.5 * (cp2 - cp0);

				tang = normalize(tang) * _Width;

				pos = pos + lerp(float2(-tang.y, tang.x), float2(tang.y, -tang.x), v.color.r);

				v2f o;
				o.pos = UnityObjectToClipPos(float3(pos.xy, _Depth));
				o.texcoord = float2(v.color.r, v.vertex.x);
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{
				return tex2D(_MainTex, i.texcoord);
			}
			ENDCG
		}
	}
}