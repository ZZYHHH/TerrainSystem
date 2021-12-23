Shader "TerrainAtlas-BlendAtlas"
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
			
			sampler2D _IndexTex;
			float4 _IndexTex_ST;
			
			sampler2D _BlendTex;
			sampler2D _NormalTex;
			float4 _BlockParams;	

			half4 GetColorByIndex(float index, float lodLevel, float2 worldPos)
			{
				float2 columnAndRow;
				//先取列再取行，范围都是0到3
				columnAndRow.x = (index % 4.0);
				columnAndRow.y = floor((float((index % 16.0))) / 4.0);

				float4 curUV;
				float2 dx=clamp(0.234375*ddx(worldPos * _BlockParams.z),-0.0078125,0.0078125);
				float2 dy=clamp(0.234375*ddy(worldPos * _BlockParams.z),-0.0078125,0.0078125);

				curUV.xy = ((columnAndRow * 0.25) + ((frac((worldPos * _BlockParams.z)) 
					* _BlockParams.yy) + _BlockParams.xx));
				curUV.w = lodLevel;
		
				return tex2D(_MainTex,curUV.xy,dx,dy);
			}
			
			half getChannelValue(float4 col,float index)
			{
				if(index==0)return col.r;
				else if(index==1) return col.g;
				else if(index==2) return col.b;
				else return col.a;
			}
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.uv, _IndexTex);
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
				float4 index=tex2D (_IndexTex, i.uv.zw);
				int indexLayer1 = floor((index.r * 15));
				int indexLayer2 = floor((index.g * 15));
				int indexLayer3 = floor((index.b * 15));

				//利用Index取得具体的贴图位置
				float4 colorLayer1 = GetColorByIndex(indexLayer1, lodLevel, i.worldPos.xz);
				float4 colorLayer2 = GetColorByIndex(indexLayer2, lodLevel, i.worldPos.xz);
				float4 colorLayer3 = GetColorByIndex(indexLayer3, lodLevel, i.worldPos.xz);
				
				float2 blend_uv1=i.uv.xy*0.5+float2((indexLayer1/4)%2,indexLayer1/8)*0.5;
				float2 blend_uv2=i.uv.xy*0.5+float2((indexLayer2/4)%2,indexLayer2/8)*0.5;		
				
				float blendValue2=getChannelValue(tex2D(_BlendTex,blend_uv2),indexLayer2%4);	
				float blendValue1=getChannelValue(tex2D(_BlendTex,blend_uv1),indexLayer1%4);
				float blendValue3=1-blendValue2-blendValue1;

				half4 albedo = (colorLayer1 * blendValue1 + colorLayer2 *blendValue2 + colorLayer3 * blendValue3);
				
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
