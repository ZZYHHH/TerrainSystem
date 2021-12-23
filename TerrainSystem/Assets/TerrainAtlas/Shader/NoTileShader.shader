Shader "Unlit/NoTileShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlendRatio("Blend Ratio",Range(0,1))=0.5
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 srcPos:TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _BlendRatio;
			const float DITHER_THRESHOLDS[16] =
             {
                   1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                   13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                   4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                   16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
              };
            float4 hash4(float2 p)
            {
                float t1 = 1.0 + dot(p, float2(37.0, 17.0));
                float t2 = 2.0 + dot(p, float2(11.0, 47.0));
                float t3 = 3.0 + dot(p, float2(41.0, 29.0));
                float t4 = 4.0 + dot(p, float2(23.0, 31.0));
                return frac(sin(float4(t1, t2, t3, t4)) * 103.0);
            }
            fixed4 texNoTileTech1(sampler2D tex, float2 uv) {
				float2 iuv = floor(uv);
				float2 fuv = frac(uv);

				// Generate per-tile transformation
				float4 ofa = hash4(iuv + float2(0, 0));
				float4 ofb = hash4(iuv + float2(1, 0));
				float4 ofc = hash4(iuv + float2(0, 1));
				float4 ofd = hash4(iuv + float2(1, 1));

				// Compute the correct derivatives
				float2 dx = ddx(uv);
				float2 dy = ddy(uv);

				// Mirror per-tile uvs
				ofa.zw = sign(ofa.zw - 0.5);
				ofb.zw = sign(ofb.zw - 0.5);
				ofc.zw = sign(ofc.zw - 0.5);
				ofd.zw = sign(ofd.zw - 0.5);

				float2 uva = uv * ofa.zw + ofa.xy, dxa = dx * ofa.zw, dya = dy * ofa.zw;
				float2 uvb = uv * ofb.zw + ofb.xy, dxb = dx * ofb.zw, dyb = dy * ofb.zw;
				float2 uvc = uv * ofc.zw + ofc.xy, dxc = dx * ofc.zw, dyc = dy * ofc.zw;
				float2 uvd = uv * ofd.zw + ofd.xy, dxd = dx * ofd.zw, dyd = dy * ofd.zw;

				// Fetch and blend
				float2 b = smoothstep(_BlendRatio, 1.0 - _BlendRatio, fuv);

				return lerp(lerp(tex2D(tex, uva, dxa, dya), tex2D(tex, uvb, dxb, dyb), b.x),
							lerp(tex2D(tex, uvc, dxc, dyc), tex2D(tex, uvd, dxd, dyd), b.x), b.y);
			}

			fixed4 texNoTileTech2(sampler2D tex, float2 uv) {
				float2 iuv = floor(uv);  //cell(n)
				float2 fuv = frac(uv);   //当前坐标

				// Compute the correct derivatives for mipmapping
				float2 dx = ddx(uv);
				float2 dy = ddy(uv);

				// Voronoi contribution
				float4 va = 0.0;
				float wt = 0.0;
				float blur = -(_BlendRatio + 0.5) * 30.0;
				for (int j = -1; j <= 1; j++) { //临近的9个Voronoi点
					for (int i = -1; i <= 1; i++) {
						float2 g = float2((float)i, (float)j);
						float4 o = hash4(iuv + g); //随机点
						
						// Compute the blending weight proportional to a gaussian fallof
						float2 r = g - fuv + o.xy; //Voronoi点-像素当前坐标= 距离
						float d = dot(r, r);
						float w = exp(blur * d);
						float4 c = tex2D(tex, uv + o.zw, dx, dy);
						va += w * c;
						wt += w;
					}
				}

				// Normalization
				return va/wt;
			}

            fixed4 texNoTileTech3(sampler2D tex,float2 uv,float4 spos) {
				float2 iuv = floor(uv);
				float2 fuv = frac(uv);

				// Generate per-tile transformation
				float4 ofa = hash4(iuv + float2(0, 0));
				float4 ofb = hash4(iuv + float2(1, 0));
				float4 ofc = hash4(iuv + float2(0, 1));
				float4 ofd = hash4(iuv + float2(1, 1));
				
                // Mirror per-tile uvs  if zw<0.5 then zw=0
				ofa.zw = sign(ofa.zw - 0.5);
				ofb.zw = sign(ofb.zw - 0.5);
				ofc.zw = sign(ofc.zw - 0.5);
				ofd.zw = sign(ofd.zw - 0.5);
				// Compute the correct derivatives
				float2 dx = ddx(uv);
				float2 dy = ddy(uv);

				float2 b= smoothstep(0,1,fuv);
                spos.xy= (spos.xy/ spos.w)*_ScreenParams.xy;
                int index=(int(spos.x)%4)*4+int(spos.y)%4;
                b=saturate(sign(b-DITHER_THRESHOLDS[index]));
                half4 dither=lerp(lerp(ofa,ofb,b.x),lerp(ofc,ofd,b.x),b.y);
                dx*=dither.zw;
                dy*=dither.zw;
                half2 finalUV=half2(uv.xy*dither.zw+dither.xy);
                return tex2D(_MainTex,finalUV,dx,dy);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.srcPos=ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = texNoTileTech1(_MainTex, i.uv)*1.5;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
