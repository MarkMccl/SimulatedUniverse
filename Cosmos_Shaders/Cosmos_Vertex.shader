Shader "Cosmos_Vertex" {
		Pass{
			CGPROGRAM

			#pragma vertex vert 
			#pragma fragment frag

			float4 vert(float4 vertexPos : POSITION) : SV_POSITION{
				return mul(UNITY_MATRIX_MVP, vertexPos); 
			}

			float4 frag(void) : COLOR {
				return float4(1.0, 0.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}