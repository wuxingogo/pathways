Shader "Example/Normal Extrusion" {
	Properties {
		_cm ("Texture", 2D) = "white" {}
		_dm ("Displacement Map", 2D) = "white" {}
		_h ("Height", Range(-1,1)) = 0.5
	}

	SubShader {
		Tags { "RenderType" = "Opaque" }
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		struct Input { float2 uv_cm; };
		float _h;
		sampler2D _dm;
		void vert(inout appdata_full v) { v.vertex.xyz += v.normal * v.texcoord1; }
		sampler2D _cm;
		void surf(Input IN, inout SurfaceOutput o) { o.Albedo = tex2D(_cm, IN.uv_cm).rgb; }
		ENDCG
	} Fallback "Diffuse"
}
