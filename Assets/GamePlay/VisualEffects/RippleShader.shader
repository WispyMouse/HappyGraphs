// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/RippleShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Scale ("Scale", float) = 1
		_Speed ("Speed", float) = 1
		_Frequency ("Frequency", float) = 1
	}

	SubShader {
		Tags {"RenderType" = "Opaque"}
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
		float _Scale, _Speed, _Frequency;
		half4 _Color;

		float _WaveAmplitude[8], _Distance[8], _xImpact[8], _zImpact[8], _xOffset[8], _zOffset[8];

		float _WaveAmplitude1, _WaveAmplitude2, _WaveAmplitude3, _WaveAmplitude4, _WaveAmplitude5, _WaveAmplitude6, _WaveAmplitude7, _WaveAmplitude8;
		float _OffsetX1, _OffsetZ1, _OffsetX2, _OffsetZ2, _OffsetX3, _OffsetZ3, _OffsetX4, _OffsetZ4, _OffsetX5, _OffsetZ5, _OffsetX6, _OffsetZ6, _OffsetX7, _OffsetZ7, _OffsetX8, _OffsetZ8;
		float _Distance1, _Distance2, _Distance3, _Distance4, _Distance5, _Distance6, _Distance7, _Distance8;
		float _xImpact1, _zImpact1, _xImpact2, _zImpact2, _xImpact3, _zImpact3, _xImpact4, _zImpact4, _xImpact5, _zImpact5, _xImpact6, _zImpact6, _xImpact7, _zImpact7, _xImpact8, _zImpact8;
		
		struct Input {
			float2 uv_MainTex;
			float3 customValue;
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);

			half offsetvert = (v.vertex.x * v.vertex.x) + (v.vertex.z * v.vertex.z);

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			for (int ii = 0; ii < 8; ii++)
			{				
				half value = _Scale * sin(_Time.w * _Speed + _Frequency * offsetvert + (v.vertex.x * _xOffset[ii]) + (v.vertex.z * _zOffset[ii]));

				v.vertex.y += value * _WaveAmplitude[ii];
				v.normal.y += value * _WaveAmplitude[ii];

				o.customValue += value * _WaveAmplitude[ii];
			}
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Normal.y += IN.customValue;
			o.Albedo = _Color.rgb;
		}

		ENDCG
	}
	FallBack "Diffuse"
}