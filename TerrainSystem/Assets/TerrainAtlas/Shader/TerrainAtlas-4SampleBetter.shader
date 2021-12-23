Shader "TerrainAtlas-4SampleBetter"
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

			half4 GetColorByIndex(float index,float2 worldPos)
			{
				float2 columnAndRow;
				//先取列再取行，范围都是0到3
				columnAndRow.x = (index % 4.0);
				columnAndRow.y = floor((float((index % 16.0))) / 4.0);

				float2 curUV;
				float2 dx=clamp(0.234375*ddx(worldPos * _BlockParams.z),-0.0078125,0.0078125);
				float2 dy=clamp(0.234375*ddy(worldPos * _BlockParams.z),-0.0078125,0.0078125);

				curUV.xy = ((columnAndRow * 0.25) + ((frac((worldPos * _BlockParams.z)) 
				* _BlockParams.yy) + _BlockParams.xx));
				
				return tex2D(_MainTex,curUV.xy,dx,dy);
			}
			void pushToList(inout int keyList[12],inout float valueList[12],inout int index,int id,float value)
			{
				int existIndex=index;
				UNITY_UNROLL
				for(int i=0;i<index;i++)
				{
					if(keyList[i]==id)
					{
						existIndex=i;
						break;
					}
				}
				valueList[existIndex]+=value;
				keyList[existIndex]=id;
				if(existIndex==index)index++;
			}

			void SplatmapMix(v2f i, out half weight, out fixed4 mixedDiffuse)
			{
				
				float4  splat_control = tex2D(_IndexTex, i.uv.zw);
				float4  splat_control_1_0 = tex2D(_IndexTex, i.uv.zw +half2(1,0)/1024.0);
				float4  splat_control_0_1 = tex2D(_IndexTex, i.uv.zw +half2(0,1)/1024.0);
				float4  splat_control_1_1 = tex2D(_IndexTex, i.uv.zw +half2(1,1)/1024.0);

				
				weight = 1;

				//混合因子，其中r通道为第一层贴图所占权重，g通道为第二层贴图所占权重，b通道为第三层贴图所占权重
				float4 blend = tex2D (_BlendTex, i.uv.xy);	
				float4  blend_1_0 = tex2D(_BlendTex, i.uv.xy +half2(1,0)/1024.0);
				float4  blend_0_1 = tex2D(_BlendTex, i.uv.xy +half2(0,1)/1024.0);
				float4  blend_1_1 = tex2D(_BlendTex, i.uv.xy +half2(1,1)/1024.0);
				half2 uv_frac =frac((i.uv.xy) * 1024);
				int listIndex=0;
				int keyList[12]={-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1};
				float valueList[12]={0,0,0,0,0,0,0,0,0,0,0,0};
				pushToList(keyList,valueList,listIndex,floor(splat_control.r*15),blend.r*(1-uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_0_1.r*15),blend_0_1.r*(1-uv_frac.x)*(uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_0.r*15),blend_1_0.r*(uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_1.r*15),blend_1_1.r*(uv_frac.x)*(uv_frac.y));

				pushToList(keyList,valueList,listIndex,floor(splat_control.g*15),blend.g*(1-uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_0_1.g*15),blend_0_1.g*(1-uv_frac.x)*(uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_0.g*15),blend_1_0.g*(uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_1.g*15),blend_1_1.g*(uv_frac.x)*(uv_frac.y));
				
				pushToList(keyList,valueList,listIndex,floor(splat_control.b*15),blend.b*(1-uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_0_1.b*15),blend_0_1.b*(1-uv_frac.x)*(uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_0.b*15),blend_1_0.b*(uv_frac.x)*(1-uv_frac.y));
				pushToList(keyList,valueList,listIndex,floor(splat_control_1_1.b*15),blend_1_1.b*(uv_frac.x)*(uv_frac.y));				
				
				mixedDiffuse=0;
				for(int j=0;j<12;j++)
				{
					int id= keyList[j];
					float value=valueList[j];
					if(id>-1){
						half4 col=GetColorByIndex(id, i.worldPos.xz);
						mixedDiffuse+=value* col;
					}
				}

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
				half weight;
				fixed4 mixedDiffuse;
				SplatmapMix(i,weight,mixedDiffuse);

				//Lambert 光照模型
				float3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				half NoL = saturate(dot(normalize(i.worldNormal), lightDir));
				half4 diffuseColor = _LightColor0 * NoL * mixedDiffuse;

				return diffuseColor;
			}
			ENDCG
		}
	}
}
