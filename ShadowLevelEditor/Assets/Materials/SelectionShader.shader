Shader "Custom/EditorSelectionShader" {
	Properties {
		_MainColor ("Some Color", Color) = (1,1,1,0.1)
		_Threshold ("No Light Threshold", Range(-1,0)) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
          #pragma surface surf SimpleLambert

          float _Threshold;

         // (value - min) / (max - min) = percentage inside a range

		half4 LightingSimpleLambert (SurfaceOutput s, half3 lightDir, half atten) {
		  half NdotL = dot (s.Normal, lightDir);
		  half diff = (NdotL - _Threshold) / (1 - _Threshold);
		  diff = max(0, diff); // Don't want a value less than 0
		  half4 c;
		  c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 2);
		  c.a = s.Alpha;
		  return c; // Color at this pixel if only this light is lighting it (run per light)
		}

		float4 _MainColor;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = _MainColor; //tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
