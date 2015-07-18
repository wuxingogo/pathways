// Upgrade NOTE: replaced 'glstate.matrix.modelview[0]' with 'UNITY_MATRIX_MV'
// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'

Shader "quad_depth_relief_mapping" 
{
	Properties 
	{
		depth 	("Depth Factor", Float) = 0.1
		tile 	("Tile Factor", Float) = 1
		
		ambient_color 	("Ambient",  Color) = (0.2, 0.2, 0.2)
		diffuse_color 	("Diffuse",  Color) = (1, 1, 1)
		specular_color 	("Specular", Color) = (0.75,0.75,0.75)
		
		shine 	("Shine", Float) = 128
		
		lightpos ("Light Position (view space)", Vector) = (100.0, -50.0, 50.0)
		
		color_map 		("Color Map"	 ,2D) = "white" {}
		quad_depth_map 	("Quad Depth Map",2D) = "white" {}
		normal_map_x	("Normal Map X"	 ,2D) = "white" {}
		normal_map_y 	("Normal Map Y"	 ,2D) = "white" {}

	}
	SubShader 
	{
		Pass 
		{		
		Cull off

CGPROGRAM //-----------

#pragma target 3.0	
#pragma vertex 	 view_space
#pragma fragment relief_map_quad_depth
#pragma profileoption MaxTexIndirections=64

//----- uniform
float 	depth;
float 	tile;
float3 	ambient_color;
float3 	diffuse_color;
float3 	specular_color;
float 	shine;
float3 	lightpos : POSITION;

//----- application data		

struct a2v 
{
    float4 vertex       : POSITION;		// float4 pos
    float3 normal    	: NORMAL;
    float2 texcoord  	: TEXCOORD0;
    float4 tangent   	: TANGENT0;		// float3 tangent
    //float3 binormal   : BINORMAL0;	// Not found in Unity3d
};

struct v2f
{
	float4 hpos 	: POSITION;
	float3 eye 		: TEXCOORD0;
	float3 light 	: TEXCOORD1;
	float2 texcoord : TEXCOORD2;
};

//----- vetrex shader
v2f view_space(a2v IN) 
{ 

	v2f OUT;

	// vertex position in object space
	float4 pos=float4(IN.vertex.x,IN.vertex.y,IN.vertex.z,1.0);

	// vertex position IN clip space
	OUT.hpos 		= mul(UNITY_MATRIX_MVP, pos);

	// copy color and texture coordinates
	OUT.texcoord=IN.texcoord.xy*tile;

	// compute modelview rotation only part
	float3x3 modelviewrot	=float3x3(UNITY_MATRIX_MV);

	// tangent vectors IN view space	
	float3 IN_bINormal 		= cross( IN.normal, IN.tangent.xyz )*IN.tangent.w;	 
	float3 tangent			= mul(modelviewrot,	IN.tangent.xyz);
	float3 bINormal			= mul(modelviewrot,	IN_bINormal.xyz);	// IN.biNormal
	float3 normal			= mul(modelviewrot,	IN.normal);
	float3x3 tangentspace	= float3x3(tangent,	bINormal,normal);

	// vertex position IN view space (with model transformations)
	float3 vpos=mul(UNITY_MATRIX_MV,pos).xyz;

	// view in tangent space
	float3 eye=mul(tangentspace,vpos);
	eye.z=-eye.z;
	OUT.eye=eye;
	
	// light position in tangent space
	OUT.light=mul(tangentspace,lightpos -vpos);
	//OUT.light=mul(tangentspace,glstate.light[0].position.xyz -vpos);

	return OUT;
}
		
		
// ray intersect quad depth map with linear search
void ray_intersect_rmqd_linear(
      in sampler2D quad_depth_map,
      inout float3 s, 
      inout float3 ds)
{
   const int linear_search_steps=30;  //15
   
   ds/=linear_search_steps;
   
   // search front to back for first point inside object
   for( int i=0;i<linear_search_steps-1;i++ )
   {
		float4 t=tex2D(quad_depth_map,s.xy);
		
		float4 d=s.z-t;									// compute distances to each layer
		d.xy*=d.zw;	d.x*=d.y;							// x=(x*y)*(z*w)
		
		if (d.x>0)										// if ouside object move forward
			s+=ds;
   }
}

// ray intersect quad depth map with binary search
void ray_intersect_rmqd_binary(
      in sampler2D quad_depth_map,
      inout float3 s, 
      inout float3 ds)
{
   const int binary_search_steps=5;
   
   float3 ss=sign(ds.z);

   // recurse around first point for closest match
   for( int i=0;i<binary_search_steps;i++ )
   {
		ds*=0.5;										// half size at each step
		
		float4 t=tex2D(quad_depth_map,s.xy);
		
		float4 d=s.z-t;									// compute distances to each layer
		d.xy*=d.zw;	d.x*=d.y;							// x=(x*y)*(z*w)
		
		if (d.x<0)										// if inside
		{
			ss=s;										// store good return position
			s-=2*ds;									// move backward
		}
		s+=ds;											// else move forward
   }
   
   s=ss;
}

float4 relief_map_quad_depth(
								v2f IN,
								uniform sampler2D quad_depth_map,
								uniform sampler2D color_map,
								uniform sampler2D normal_map_x,
								uniform sampler2D normal_map_y	) : COLOR
{
	// view vector in tangent space
	float3 v=normalize(IN.eye);

	// serach start position
	float3 s=float3(IN.texcoord,0);
	
	// separate direction (front or back face)
	float dir=v.z;
	v.z=abs(v.z);
	
	// depth bias (1-(1-d)*(1-d))
	float d=depth*(2*v.z-v.z*v.z);

	// compute serach vector
	v/=v.z;
	v.xy*=d;
	s.xy-=v.xy*0.5;
	
	// if viewing from backface
	if (dir<0)
	{	
		s.z=0.996;	// search from back to front
		v.z=-v.z;
	}
	
	// ray intersect quad depth map
	ray_intersect_rmqd_linear(quad_depth_map,s,v);
	ray_intersect_rmqd_binary(quad_depth_map,s,v);
	
	// discard if no intersection is found
	if (s.z>0.997) discard;
	if (s.z<0.003) discard;

	// DEBUG: return depth
	//return float4(s.zzz,1);

	// get quad depth and color at intersection
	float4 t=tex2D(quad_depth_map,s.xy);
	float4 c=tex2D(color_map,s.xy);

	// get normal components X and Y
	float4 nx=tex2D(normal_map_x,s.xy);
	float4 ny=tex2D(normal_map_y,s.xy);
	
	// compute normal
	float4 z=abs(s.z-t);
	int m=0;											// find min component
	float zm = z.x;
	if (z.y<zm) { m=1; zm = z.y; }
	if (z.z<zm) { m=2; zm = z.z; }
	if (z.w<zm) { m=3; 			 }
	
	float3 n; 	 										// get normal at min component layer	
	if ( m == 0) { n.x=nx[0]; n.y=1-ny[0]; }			//n.x=nx[m]; n.y=1-ny[m];	
	if ( m == 1) { n.x=nx[1]; n.y=1-ny[1]; }
	if ( m == 2) { n.x=nx[2]; n.y=1-ny[2]; }
	if ( m == 3) { n.x=nx[3]; n.y=1-ny[3]; }
	 
	n.xy=n.xy*2-1; 										// expand to [-1,1] range
	n.z=sqrt(max(0,1.0-dot(n.xy,n.xy))); 				// recompute z
	if (m==1||m==3) 									// invert normal z if in backface
		n.z=-n.z;

	// DEBUG: return normal
	// return float4(n*0.5+0.5,1);

	// compute light vector in view space
	float3 l=normalize(IN.light);

	// restore view direction z component
	v=normalize(IN.eye);
	v.z=-v.z;
	
	// compute diffuse and specular terms
	float ldotn=saturate(dot(l,n));
	float ndoth=saturate(dot(n,normalize(l-v)));
	
	// attenuation factor
	float att=1.0-max(0,l.z); att=1.0-att*att;

	// return final color with lighting
	float4 finalcolor;
	finalcolor.xyz = 	c.xyz*ambient_color + 
						att*(c.xyz*diffuse_color*ldotn +
						c.w*specular_color.xyz*pow(ndoth,shine));
	finalcolor.w=1;
	
	return finalcolor;
}
		
ENDCG //----------		
		} // Pass
	} // SubShader
} // Shader
