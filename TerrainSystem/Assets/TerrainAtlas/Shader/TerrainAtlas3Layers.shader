Shader "TerrainAtlas3Layers"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_IndexTex("Index Texture", 2D) = "white" {}
		_BlendTex("Blend Texture", 2D) = "white" {}
		_NormalTex("Normal Texture",2D)="bump"{}
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
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD2;
				float4 TtoW0 : TEXCOORD3;  
				float4 TtoW1 : TEXCOORD4;  
				float4 TtoW2 : TEXCOORD5;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			sampler2D _IndexTex;
			float4 _IndexTex_ST;
			
			sampler2D _BlendTex;
			sampler2D _NormalTex;
			float4 _BlockParams;	

			half4 GetUVByIndex(float index, float lodLevel, float2 worldPos)
			{
				float2 columnAndRow;
				//先取列再取行，范围都是0到3
				columnAndRow.x = (index % 4.0);
				columnAndRow.y = floor((float((index % 16.0))) / 4.0);

				float4 curUV;
				//由于是4x4的图集，所以具体的行列需要乘以0.25 
				//如1就是（0.25, 0），刚好对应第二张贴图的起始位置
				curUV.xy = ((columnAndRow * 0.25) + ((frac((worldPos * _BlockParams.z)) 
					* _BlockParams.yy) + _BlockParams.xx));
				curUV.w = lodLevel;
				return curUV;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _IndexTex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);  
				fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);  
				fixed3 worldBinormal = cross(worldNormal, worldTangent) * v.tangent.w; 
				
				o.TtoW0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, o.worldPos.x);
				o.TtoW1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, o.worldPos.y);
				o.TtoW2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, o.worldPos.z);  
				//记录观察空间的Z值
				o.worldPos.w = mul(UNITY_MATRIX_MV, v.vertex).z;

				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half lodLevel = min(-i.worldPos.w * 0.1, 3);

				//将贴图中被压缩到0,1之间的Index还原
				float4 index=tex2D (_IndexTex, i.uv.zw);
				float indexLayer1 = floor((index.r * 15));
				float indexLayer2 = floor((index.g * 15));
				float indexLayer3 = floor((index.b * 15));

				float4 uv1=GetUVByIndex(indexLayer1, lodLevel, i.worldPos.xz);
				float4 uv2=GetUVByIndex(indexLayer2, lodLevel, i.worldPos.xz);
				float4 uv3=GetUVByIndex(indexLayer3, lodLevel, i.worldPos.xz);

				//利用Index取得具体的贴图位置
				float4 colorLayer1 = tex2Dlod(_MainTex,uv1);
				float4 colorLayer2 =  tex2Dlod(_MainTex,uv2);
				float4 colorLayer3 =  tex2Dlod(_MainTex,uv3);

				float3 normal1=UnpackNormal(tex2D(_NormalTex,uv1.xy));
				float3 normal2=UnpackNormal(tex2D(_NormalTex,uv2.xy));
				float3 normal3=UnpackNormal(tex2D(_NormalTex,uv3.xy));
				normal1=half3(dot(i.TtoW0.xyz, normal1), dot(i.TtoW1.xyz, normal1), dot(i.TtoW2.xyz, normal1));
				normal2=half3(dot(i.TtoW0.xyz, normal2), dot(i.TtoW1.xyz, normal2), dot(i.TtoW2.xyz, normal2));
				normal3=half3(dot(i.TtoW0.xyz, normal3), dot(i.TtoW1.xyz, normal3), dot(i.TtoW2.xyz, normal3));

				//混合因子，其中r通道为第一层贴图所占权重，g通道为第二层贴图所占权重，b通道为第三层贴图所占权重
				float4 blend = tex2D (_BlendTex, i.uv.xy);
				half4 albedo = (colorLayer1 * blend.r + colorLayer2 * blend.g + colorLayer3 * blend.b)/(blend.r+blend.g+blend.b);
				half3 normal=(normal1 * blend.r + normal2 * blend.g + normal3 * blend.b)/(blend.r+blend.g+blend.b);
				//Lambert 光照模型
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				half NoL = saturate(dot(normalize(normal), lightDir));
				half4 diffuseColor = _LightColor0 * NoL * albedo;
				return diffuseColor;
			}
			ENDCG
		}
	}
}
