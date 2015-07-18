Shader "relaxed_cone_relief_mapping" {
	Properties {
		ambient_color 			("Ambient",  Color) 		= (0.2, 0.2, 0.2)
		diffuse_color 			("Diffuse",  Color) 		= (1, 1, 1)
		specular_color 			("Specular", Color)			= (0.75,0.75,0.75)

		shine 					("Shine", 		Float) 		= 32
		tile 					("Tile Factor",	Float) 		= 1
		depth 					("Depth Factor",Float) 		= 0.1

		color_map 				("color_map", 	2D) 		= "white" {}
		cone_relief_map 		("cone", 		2D) 		= "white" {}
		quadcone_relief_map 	("quadcone", 	2D) 		= "white" {}
		relaxedcone_relief_map 	("relaxedcone",	2D) 		= "white" {}

		lightpos 				("PointLight", 	Vector) 	= (-10, 10, -10)
	}

	SubShader {
		Pass {
		Cull Back
		AlphaTest GEqual 16

CGPROGRAM //-----------
#pragma target 3.0
#pragma vertex 		vertex_shader

//  enable one of them to see the difference
//	#pragma fragment pixel_shader_normal
	#pragma fragment pixel_shader_relief
//	#pragma fragment pixel_shader_quadcone
//	#pragma fragment pixel_shader_relaxedcone

#pragma profileoption MaxTexIndirections=64
#include "UnityCG.cginc"

#define DEPTH_BIAS
#define BORDER_CLAMP

float3 ambient_color;
float3 diffuse_color;
float3 specular_color;
float shine;

float tile;
float depth;
float relief_mode;
float depth_bias;
float border_clamp;

sampler2D color_map;
sampler2D relaxedcone_relief_map;
sampler2D cone_relief_map;
sampler2D quadcone_relief_map;
float3 lightpos;

struct a2v {
	float4 vertex 		: POSITION;
	float4 tangent		: TANGENT;
	float3 normal		: NORMAL;
	float2 texCoord		: TEXCOORD0; };

struct v2f {
	float4 hpos		: POSITION;
	float3 eye		: TEXCOORD0;
	float3 light	: TEXCOORD1;
	float2 texcoord : TEXCOORD2; };

v2f vertex_shader(a2v IN) {
	v2f OUT;
	float4 pos 				= float4(IN.vertex.xyz, 1.0); 		// vertex position IN object space
	OUT.hpos 				= mul(UNITY_MATRIX_MVP, pos); 		// vertex position IN clip space
	OUT.texcoord 			= IN.texCoord.xy*tile;				// copy color and texture coordINates
	float3x3 modelviewrot	= float3x3(UNITY_MATRIX_MV); 		// compute modelview rotation only part
	float3 IN_bINormal 		= cross(IN.normal, IN.tangent.xyz)*IN.tangent.w;
	float3 tangent			= mul(modelviewrot,IN.tangent.xyz); // tangent vectors IN view space ^
	float3 bINormal			= mul(modelviewrot,IN_bINormal.xyz);
	float3 normal			= mul(modelviewrot,IN.normal);
	float3x3 tangentspace	= float3x3(tangent,bINormal,normal);

	float3 vpos=mul(UNITY_MATRIX_MV,pos).xyz; 					// vertex position IN view space (with model transformations)
	OUT.eye=mul(tangentspace,vpos); 							// view and light IN tangent space
	OUT.light=mul(tangentspace,lightpos.xyz-vpos);				// light position IN tangent space
	return OUT;													// OUT.light=mul(tanspace,glstate.light[0].position.xyz-vpos);
}

void setup_ray(v2f IN, out float3 p, out float3 v) { 			// setup view ray vector and apply depth bias and factor
	p = float3(IN.texcoord,0);
	v = normalize(IN.eye);
	v.z = abs(v.z);
#ifdef DEPTH_BIAS
	float db = 1.0-v.z; db*=db; db*=db; db=1.0-db*db;
	v.xy *= db;
#endif
	v.xy *= depth;
}

float4 normal_mapping(sampler2D color_map, sampler2D normal_map, float2 texcoord, v2f IN) {
	float4 color = tex2D(color_map,texcoord);					// do normal mapping using given texture coordinate
	float4 normal = tex2D(normal_map,texcoord);					// tangent space phong lighting with optional border clamp
	normal.xy = 2*normal.xy - 1;								// normal X and Y stored in red and green channels
	normal.y = -normal.y;
	normal.z = sqrt(1.0 - dot(normal.xy,normal.xy));
	float3 l = normalize(IN.light);								// light and view in tangent space
	float3 v = normalize(IN.eye);
	float diff = saturate(dot(l,normal.xyz));					// compute diffuse and specular terms
	float spec = saturate(dot(normalize(l-v),normal.xyz));
	float att = 1.0 - max(0,l.z);								// attenuation factor
	att = 1.0 - att*att;
	float alpha=1;
#ifdef BORDER_CLAMP
	if (texcoord.x<0) 	 alpha=0;
	if (texcoord.y<0) 	 alpha=0;
	if (texcoord.x>tile) alpha=0;
	if (texcoord.y>tile) alpha=0;
#endif
	float4 finalcolor;
	finalcolor.xyz = ambient_color*color.xyz +
		att*(color.xyz*diffuse_color*diff +
		specular_color*pow(spec,shine));
	finalcolor.w = alpha;
	return finalcolor;
}

void ray_intersect_relaxedcone(sampler2D relaxedcone_relief_map, inout float3 p, inout float3 v) {
	const int cone_steps=15;									// ray intersect depth map using binary cone space leaping
	const int binary_steps=5;									// depth value stored in alpha channel (black is surfacial)
	float3 p0 = p;												// and cone ratio stored in blue channel
	v /= v.z;
	float dist = length(v.xy);
	for (int i=0;i<cone_steps;i++) {
		float4 tex = tex2D(relaxedcone_relief_map, p.xy);
		float height = saturate(tex.w - p.z);
		float cone_ratio = tex.z;
		p += v * (cone_ratio * height / (dist + cone_ratio));
	}

	v *= p.z*0.5;
	p = p0 + v;

	for (int i=0;i<binary_steps;i++) {
		float4 tex = tex2D(relaxedcone_relief_map, p.xy);
		v *= 0.5;
		if (p.z<tex.w) p+=v;
		else p-=v;
	}
}

void ray_intersect_quadcone(sampler2D cone_relief_map,sampler2D quadcone_relief_map,inout float3 p,inout float3 v) {
	const int num_steps=15;										// ray intersect depth map using quad cone space leaping
	float2 abs_view = abs(v.xy);								// depth value stored in alpha channel (black is surfacial)
	int index;													// and RGBA texture with pyramid ratios per cardinal direction
	if (abs_view.x>abs_view.y) index = v.x<0 ? 1 : 0;
	else index = v.y<0 ? 3 : 2;
	float dist = length(v.xy);
	for (int i=0;i<num_steps;i++) {
		float4 tex = tex2D(cone_relief_map, p.xy);
		float height = saturate(tex.w - p.z);
		//float cone_ratio = tex2D(quadcone_relief_map, p.xy)[index];	// not work in Unity3d ?!
		float cone_ratio;
		float4 c = tex2D(quadcone_relief_map, p.xy);
		if (index==0) { cone_ratio = c[0]; }
		if (index==1) { cone_ratio = c[1]; }
		if (index==2) { cone_ratio = c[2]; }
		if (index==3) { cone_ratio = c[3]; }
		p += v * (cone_ratio * height / (dist + cone_ratio));
	}
}

void ray_intersect_cone(sampler2D cone_relief_map,inout float3 p,inout float3 v) {
	const int num_steps = 15;									// ray intersect depth map using cone space leaping
	float dist = length(v.xy);									// depth value stored in alpha channel (black is surfacial)
	for (int i=0;i<num_steps;i++) {								// and cone ratio stored in blue channel
		float4 tex = tex2D(cone_relief_map, p.xy);
		float height = saturate(tex.w - p.z);
		float cone_ratio = tex.z;
		p += v * (cone_ratio * height / (dist + cone_ratio));
	}
}

void ray_intersect_relief(sampler2D relief_map,	inout float3 p,	inout float3 v) {
	const int num_steps_lin=15;									// ray intersect depth map using linear and binary searches
	const int num_steps_bin=5;									// depth value stored in alpha channel (black is surfacial)
	v /= v.z*num_steps_lin;
	int i;
	for (i=0;i<num_steps_lin;i++) {
		float4 tex = tex2D(relief_map, p.xy);
		if (p.z<tex.w) p+=v;
	}

	for (i=0;i<num_steps_bin;i++) {
		v *= 0.5;
		float4 tex = tex2D(relief_map, p.xy);
		if (p.z<tex.w) p+=v;
		else p-=v;
	}
}

float4 pixel_shader_relaxedcone(v2f IN):COLOR {
	float3 p,v;
	setup_ray(IN,p,v);
	ray_intersect_relaxedcone(relaxedcone_relief_map,p,v);
	return normal_mapping(color_map,relaxedcone_relief_map,p.xy,IN);
}

float4 pixel_shader_quadcone(v2f IN):COLOR {
	float3 p,v;
	setup_ray(IN,p,v);
	ray_intersect_quadcone(cone_relief_map,quadcone_relief_map,p,v);
	return normal_mapping(color_map,cone_relief_map,p.xy,IN);
}

float4 pixel_shader_cone(v2f IN):COLOR {
	float3 p,v;
	setup_ray(IN,p,v);
	ray_intersect_cone(cone_relief_map,p,v);
	return normal_mapping(color_map,cone_relief_map,p.xy,IN);
}

float4 pixel_shader_relief(v2f IN):COLOR {
	float3 p,v;
	setup_ray(IN,p,v);
	ray_intersect_relief(cone_relief_map,p.xyz,v);
	return normal_mapping(color_map,cone_relief_map,p.xy,IN);
}

float4 pixel_shader_normal(v2f IN):COLOR { return normal_mapping(color_map,cone_relief_map,IN.texcoord,IN);}

ENDCG //----------
		} // Pass
	} // SubShader
} // Shader
