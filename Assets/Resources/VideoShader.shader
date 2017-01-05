Shader "Custom/VideoShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue"="Geometry-10" }
		LOD 200
		
		Pass {
			Color (1,1,1,0) Material { Diffuse (1,1,1,0) Ambient (1,1,1,0) } 
			Lighting Off
			SetTexture [_MainTex]
		}
	} 
	FallBack "Diffuse"
}

