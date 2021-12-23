Shader "Texture2DArray"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlendTex("Blend Texture", 2D) = "white" {}
		_BlockParams("Block Params",Vector) = (0.0078125, 0.234375, 0.25, 0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _BlendTex;
			float4 _BlendTex_ST;

			float4 _BlockParams;

			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _BlendTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				//记录观察空间的Z值
				o.worldPos.w = mul(UNITY_MATRIX_MV, v.vertex).z;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half lodLevel = min(-i.worldPos.w * 0.1, 3);

				//将贴图中被压缩到0,1之间的Index还原
				float3 blend=tex2D (_BlendTex, i.uv.zw);
				float indexLayer1 = floor((blend.r * 8));
				float indexLayer2 = floor((blend.g * 8));
				float blendMask =blend.b;

				//利用Index取得具体的贴图位置
				float4 colorLayer1 = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv.x, i.uv.y, indexLayer1));
				float4 colorLayer2 = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, float3(i.uv.x, i.uv.y, indexLayer2));

				//混合因子，其中r通道为第一层贴图所占权重，g通道为第二层贴图所占权重，b通道为第三层贴图所占权重
				half4 albedo = lerp(colorLayer1,colorLayer2,blendMask);

				//Lambert 光照模型
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				half NoL = saturate(dot(normalize(i.worldNormal), lightDir));
				half4 diffuseColor = _LightColor0 * NoL * albedo;
				return diffuseColor;
			}
			ENDCG
		}
	}
}
