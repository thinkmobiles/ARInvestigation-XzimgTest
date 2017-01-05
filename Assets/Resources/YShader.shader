/**
*
* Copyright (c) 2016 XZIMG , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of xzimg
*
* The XZIMG company is located at 903 Dannies House, 20 Luard Road, Wanchai, Hong Kong
* contact@xzimg.com, www.xzimg.com
*
*/
Shader "Custom/YShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_UVTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Pass {
		
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				uniform sampler2D _UVTex;

				fixed4 frag(v2f_img texCoord) : SV_Target {
					
					float y = tex2D(_MainTex, texCoord.uv).r;		// using GL_LUMINANCE
					float u = tex2D(_UVTex, texCoord.uv).a - 0.5;
					float v = tex2D(_UVTex, texCoord.uv).r - 0.5;

					float r = y + 1.370705*v;
					float g = y - 0.337633*u - 0.698001*v;
					float b = y + 1.732446*u;
					return float4(b,g,r,1.0);

					//float r = y + 1.13983*v;
					//float g = y - 0.39465*u - 0.58060*v;
					//float b = y + 2.03211*u;
				}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
