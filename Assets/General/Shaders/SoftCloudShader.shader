//
// SoftCloudShader.shader: Default shader for clouds with soft particle support.
//
// Author:
//   Based on the Unity3D built-in shaders
//   Andreas Suter (andy@edelweissinteractive.com)
//
// Copyright (C) 2011-2012 Edelweiss Interactive (http://edelweissinteractive.com)
//

Shader "Cloud/Soft Cloud" {

	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range (0.01, 3.0)) = 1.0
	}
	
	Category {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Lighting Off
		ColorMask RGB
		Cull Off
		ZWrite Off
		Fog {Mode Off}
		
		Blend SrcAlpha OneMinusSrcAlpha
		
		BindChannels {
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
		
			// Fragment program cards
		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_particles
	
				#include "UnityCG.cginc"
	
				sampler2D _MainTex;
				
				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};
	
				struct v2f {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD1;
					#endif
				};
				
				float4 _MainTex_ST;
	
				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul (UNITY_MATRIX_MVP, v.vertex);
					#ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH (o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX (v.texcoord,_MainTex);
					return o;
				}
	
				sampler2D _CameraDepthTexture;
				float _InvFade;
				
				half4 frag (v2f i) : COLOR
				{
					#ifdef SOFTPARTICLES_ON
					float sceneZ = LinearEyeDepth (tex2Dproj (_CameraDepthTexture, UNITY_PROJ_COORD (i.projPos)).r);
					float partZ = i.projPos.z;
					float fade = saturate (_InvFade * (sceneZ-partZ));
					i.color.a *= fade;
					#endif
					
					return i.color * tex2D(_MainTex, i.texcoord);
				}
				ENDCG 
			}
		} 	
	}
	
	Fallback "Cloud/Cloud"
}