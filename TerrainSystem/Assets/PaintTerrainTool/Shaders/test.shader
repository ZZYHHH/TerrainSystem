Shader "test" {
    Properties {
        [Header(Debug)]
        _Debug ("", Vector) = (0, 0, 0, 0)
        _DebugSpec ("Debug Spec", Vector) = (1, 0, 0, 0)
        _DebugGray ("Debug Height", Vector) = (0, 0, 0, 0)
        _SpecFrom1 ("Spec1 From", Vector) = (1, 1, 1, 1)
        _SpecFrom2 ("Spec2 From", Vector) = (0, 0, 0, 0)
        _SpecFrom3 ("Spec3 From", Vector) = (0, 0, 0, 0)

        _SpecColor1 ("", Color) = (1, 1, 1, 1)
        _SpecColor2 ("", Color) = (1, 1, 1, 1)
        _SpecColor3 ("", Color) = (1, 1, 1, 1)
        _SpecColor4 ("", Color) = (1, 1, 1, 1)

        _AmbientColor1 ("", Color) = (0.5, 0.5, 0.6, 1)
        _AmbientColor2 ("", Color) = (0.5, 0.5, 0.6, 1)
        _AmbientColor3 ("", Color) = (0.5, 0.5, 0.6, 1)
        _AmbientColor4 ("", Color) = (0.5, 0.5, 0.6, 1)
        
        _GrayFrom1 ("Height1 From", Vector) = (1, 1, 1, 1)
        _GrayFrom2 ("Height2 From", Vector) = (0, 0, 0, 0)
        _GrayFrom3 ("Height3 From", Vector) = (0, 0, 0, 0)

        _SoliColor ("", Color) = (1, 1, 1, 1)
        _SandColor ("", Color) = (1, 1, 1, 1)

        [Space(50)]

        [HideInInspector] _Color ("Main Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MainTex ("Texture", 2D) = "white" { }
        
        [Header(AllLayerControl)]
        _Tilling ("Tile(RGBA)", Vector) = (1, 1, 1, 1)
        [Space(15)]

        _Gloss ("Gloss", Vector) = (1, 1, 1, 1)
        _SpecPower ("Spec Power", Vector) = (1, 1, 1, 1)
        _SpecLow ("Spec Map LowFactor", Vector) = (0.2, 0.0, 0.5, 0.2)
        _SpecHigh ("Spec Map HighFactor", Vector) = (0.3, 0.4, 0.7, 0.45)
        


        [Space(15)]
        _HeightLow ("Height Map LowFactor", Vector) = (0.2, 0.0, 0.5, 0.2)
        _HeightHigh ("Height Map HighFactor", Vector) = (0.3, 0.4, 0.7, 0.45)
        //_Weight ("Blend Weight", Range(0.001,1)) = 0.2
        
        [Space(20)]
        [Header(FirstMapInfo)]
        [NoScaleOffset]_Tex0 ("Layer 0(R)", 2D) = "white" { }
        [NoScaleOffset]_Normal0 ("Layer 0 Normal", 2D) = "bump" { }


        [Space(20)]
        [Header(SecondMapInfo)]
        [NoScaleOffset]_Tex1 ("Layer 1(G)", 2D) = "white" { }
        [NoScaleOffset]_Normal1 ("Layer 1 Normal", 2D) = "bump" { }

        [Space(20)]
        [Header(ThirdMapInfo)]
        [NoScaleOffset]_Tex2 ("Layer 2(B)", 2D) = "white" { }

        [Space(20)]
        [Header(ForthMapInfo)]
        [NoScaleOffset]_Tex3 ("Layer 3(Invert)", 2D) = "white" { }

        [Space(20)]
        [Header(OtherInfo)]
        _Mask ("Mask(RGB)", 2D) = "red" { }
        _LightMap ("LightMap", 2D) = "white" { }
        _SandRimTex ("Sand Rim Tex", 2D) = "" { }
        _BlendWeight ("Blend Weight", Range(0, 1)) = .8
        _WholeAmbientColor ("Whole Ambient Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_WholeDiffuse ("Whole Diffuse", 2D) = "" { }
        [NoScaleOffset]_WholeNormal ("Whole Normal", 2D) = "bump" { }
        _FadeNearFar ("Fade Near Far", Vector) = (210, 420, 0, 0)

        _ShadowBlob ("ShadowBlob", 2D) = "white" { }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////
    //BlinnPhong + HeightMap + 4TextureMap + 2NormalMap + 1LightMap + 1MaskMap
    SubShader {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry+1" }
        LOD 550                  //为了让LA能直接看到LOD500高配的效果，先把LOD写成550
        Offset 0.15, 0.15

        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d11

            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants LIGHTMAP_ON

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
                fixed4 tangent : TANGENT;
                fixed4 vertexColor : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                fixed4 normal : NORMAL;
                fixed3 viewDir : TEXCOORD1;
                fixed3 lightDir : TEXCOORD2;
                float4 worldPos : TEXCOORD3;
                fixed4 vertexColor : COLOR;

                SHADOW_COORDS(7)
                UNITY_FOG_COORDS(8)
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST;


            sampler2D _Tex0, _Tex1, _Tex2,_Tex3;
            sampler2D _Normal0, _Normal1;
            sampler2D _Mask;
            fixed4 _Mask_ST;

            sampler2D _LightMap;
            fixed4 _LightMap_ST;

            sampler2D _SandRimTex;
            //sampler2D _WholeDiffuse;
            //sampler2D _WholeNormal;

            fixed _BlendWeight;
            half4 _WholeTintColor;
            half4 _Tilling;
            half4 _Gloss;
            fixed4 _SpecPower;
            //fixed _Weight;
            fixed4 _HeightLow, _HeightHigh;
            fixed4 _SpecLow, _SpecHigh;
            //fixed4 _LightColor0 ;



            fixed4 _SpecColor1, _SpecColor2, _SpecColor3, _SpecColor4;
            fixed4 _AmbientColor1, _AmbientColor2, _AmbientColor3, _AmbientColor4;
            fixed4 _WholeAmbientColor;
            fixed4 _SoliColor, _SandColor;




            half2 _FadeNearFar;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv.zw = v.uv;
                o.uv.xy = o.worldPos.xz * 0.025 - floor(_WorldSpaceCameraPos.xz * 0.025);

                fixed dist = distance(o.worldPos.xyz, _WorldSpaceCameraPos.xyz);
                o.worldPos.w = smoothstep(_FadeNearFar.x, _FadeNearFar.y, dist);

                //fixed3 binormal = (cross(normalize(v.normal), normalize(v.tangent.xyz)) * v.tangent.w);
                //fixed3x3 TBN = fixed3x3(v.tangent.xyz,binormal,v.normal);

                o.lightDir.xyz = normalize(_WorldSpaceLightPos0);
                o.viewDir = normalize(o.worldPos.xyz - _WorldSpaceCameraPos.xyz);
                o.normal.xyz = normalize(UnityObjectToWorldNormal(v.normal));
                o.normal.w = UnityObjectToViewPos(v.vertex).z;

                o.vertexColor = v.vertexColor;


                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
            
            fixed4 blendFactor(fixed4 height, fixed4 control) {
                fixed4 blendValue = height * control;
                fixed maxValue = max(blendValue.r, max(blendValue.g, max(blendValue.b, blendValue.a)));
                
                blendValue = max(blendValue - maxValue + _BlendWeight, 0) * control;
                return blendValue / max((dot(blendValue, fixed4(1, 1, 1, 1)) + 1e-3f), 0.00001);
            }

            fixed4 _SpecFrom1, _SpecFrom2, _SpecFrom3;
            fixed4 _GrayFrom1, _GrayFrom2, _GrayFrom3;
            fixed4 frag(v2f i) : SV_Target {
                //Calculate uv
                fixed4 uv01 = i.uv.xyxy * _Tilling.xxyy;
                fixed4 uv23 = i.uv.xyxy * _Tilling.zzww;

                //Sample Texture
                fixed3 splat0 = tex2D(_Tex0, uv01.xy);
                fixed3 splat1 = tex2D(_Tex1, uv01.zw);
                fixed3 splat2 = tex2D(_Tex2, uv23.xy);
                fixed3 splat3 = tex2D(_Tex3, uv23.zw);


                fixed3 normal0 = UnpackNormal(tex2D(_Normal0, uv01.xy)).xzy;
                fixed3 normal1 = UnpackNormal(tex2D(_Normal1, uv01.zw)).xzy;
                fixed3 mask = tex2D(_Mask, TRANSFORM_TEX(i.uv.zw, _Mask));
                fixed a = 1 - saturate(dot(mask.rgb, fixed3(1, 1, 1)));

                fixed4 height;// = fixed4(splat0.g, splat1.g, splat2.r, splat3.b);
                
                /***********************Finally Convert To Hand Code***************************/
                fixed3 tmpgray = fixed3(_GrayFrom1.r, _GrayFrom2.r, _GrayFrom3.r);
                height.x = dot(tmpgray, splat0.rgb);

                tmpgray = fixed3(_GrayFrom1.g, _GrayFrom2.g, _GrayFrom3.g);
                height.y = dot(tmpgray, splat1.rgb);

                tmpgray = fixed3(_GrayFrom1.b, _GrayFrom2.b, _GrayFrom3.b);
                height.z = dot(tmpgray, splat2.rgb);

                tmpgray = fixed3(_GrayFrom1.a, _GrayFrom2.a, _GrayFrom3.a);
                height.w = dot(tmpgray, splat3.rgb);
                /***********************Finally Convert To Hand Code***************************/
                
                height = smoothstep(_HeightLow, _HeightHigh, height);
                fixed4 control = blendFactor(height, fixed4(mask, a));
                
                //return control.a;
                //Lighting model
                fixed3 normal = normal0 * control.r + normal1 * control.g + i.normal.xyz * saturate(1 - dot(control.rg, 1));
                fixed NdotL = saturate(dot(normal, i.lightDir.xyz));
                
                fixed3 halfVector = normalize(i.viewDir + i.lightDir.xyz);
                fixed NdotH = saturate(dot(normal, halfVector));
                
                _Gloss *= 128.0;
                half4 spec4 = half4(pow(NdotH, _Gloss.r), pow(NdotH, _Gloss.g), pow(NdotH, _Gloss.b), pow(NdotH, _Gloss.a)) * _SpecPower;
                fixed4 specMap;// = fixed4(splat0.g, splat1.g, splat2.r, splat3.b);
                /***********************Finally Convert To Hand Code***************************/
                
                fixed3 tmpspec = fixed3(_SpecFrom1.r, _SpecFrom2.r, _SpecFrom3.r);
                specMap.x = dot(tmpspec, splat0.rgb);

                tmpspec = fixed3(_SpecFrom1.g, _SpecFrom2.g, _SpecFrom3.g);
                specMap.y = dot(tmpspec, splat1.rgb);

                tmpspec = fixed3(_SpecFrom1.b, _SpecFrom2.b, _SpecFrom3.b);
                specMap.z = dot(tmpspec, splat2.rgb);

                tmpspec = fixed3(_SpecFrom1.a, _SpecFrom2.a, _SpecFrom3.a);
                specMap.w = dot(tmpspec, splat3.rgb);
                /***********************Finally Convert To Hand Code***************************/
                specMap = smoothstep(_SpecLow, _SpecHigh, specMap);
                half spec = dot(spec4 * specMap, control);

                fixed3 specCol1 = _SpecColor1.rgb * control.r;
                fixed3 specCol2 = _SpecColor2.rgb * control.g;
                fixed3 specCol3 = _SpecColor3.rgb * control.b;
                fixed3 specCol4 = _SpecColor4.rgb * control.a;

                fixed3 specCol = specCol1 + specCol2 + specCol3 + specCol4;
                //return fixed4(spec.rrr, 1);
                specCol *= spec;
                

                //Fresnel
                fixed fresnel = 1 - saturate(dot(normal, i.viewDir));
                fixed vertexColor = i.vertexColor;
                
                fixed4 col = fixed4(0, 0, 0, 1);
                //col.rgb = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (splat3.r * _SoliColor * (1-vertexColor) + splat3.g * _SandColor * vertexColor) * control.a;
                //Add Snow
                col.rgb = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * (1 - vertexColor) + splat3.g * _SandColor * vertexColor) * control.a;
                //Lightmap
                fixed3 lightmapCol = tex2D(_LightMap, TRANSFORM_TEX(i.uv.zw, _LightMap));
                lightmapCol *= 2;

                //Diffuse
                //fixed3 diffuse = col.rgb * NdotL;
                //Clamp Snow
                fixed snowMask = control.a * (1 - i.vertexColor.a);
                fixed3 diffuse = col.rgb * lerp(NdotL, clamp(NdotL, 0.0, 0.55), snowMask);
                
                //Sepcular
                fixed3 specular = specCol;

                fixed3 ambient1 = _AmbientColor1 * control.r;
                fixed3 ambient2 = _AmbientColor2 * control.g;
                fixed3 ambient3 = _AmbientColor3 * control.b;
                fixed3 ambient4 = _AmbientColor4 * control.a;

                fixed3 ambient = (ambient1 + ambient2 + ambient3 + ambient4) * col.rgb; //UNITY_LIGHTMODEL_AMBIENT.rgb * col.rgb;
                //fixed  atten = LIGHT_ATTENUATION(i);
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz)
                col.rgb = diffuse * atten * _LightColor0 + ambient;
                
                //col.rgb = lerp (col , col *(lightmapCol*2) , 0.8)+ specular*atten*NdotL* _LightColor0;
                //Clamp Snow Lightmap
                col.rgb = lerp(col, col * lerp(lightmapCol, clamp(lightmapCol, 0, 1), (1 - i.vertexColor.a) * control.a), 0.8) + specular * atten * NdotL * _LightColor0;

				/*
                fixed3 wholeNormal = UnpackNormal(tex2D(_WholeNormal, i.uv.zw));
                fixed3 wholeDiffuse = tex2D(_WholeDiffuse, i.uv.zw);

                fixed wholeNdotL = saturate(dot(wholeNormal, i.lightDir.xyz));
                
                fixed wholeNdotH = saturate(dot(wholeNdotL, halfVector));

                fixed3 wholeSpec = pow(wholeNdotH, 2) * wholeDiffuse;
                fixed3 wholeCol = wholeDiffuse;// wholeNdotL + _WholeAmbientColor.rgb*wholeDiffuse + wholeSpec;
                
                col.rgb = lerp(col.rgb * _WholeAmbientColor.rgb * 2, wholeCol, i.worldPos.w);
				*/

                return col ;
            }
            ENDCG

        }
        UsePass "Hidden/ShadowCasterForPC/SHADOWCASTER"
    }
}