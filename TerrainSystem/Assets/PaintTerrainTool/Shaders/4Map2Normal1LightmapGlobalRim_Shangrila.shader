Shader "BRMobile/Terrain/4Map2Normal1LightmapGlobalRim_Shanglila" {
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
        [NoScaleOffset]_Splat0 ("Layer 0(R)", 2D) = "white" { }
        [NoScaleOffset]_Normal0 ("Layer 0 Normal", 2D) = "bump" { }


        [Space(20)]
        [Header(SecondMapInfo)]
        [NoScaleOffset]_Splat1 ("Layer 1(G)", 2D) = "white" { }
        [NoScaleOffset]_Normal1 ("Layer 1 Normal", 2D) = "bump" { }

        [Space(20)]
        [Header(ThirdMapInfo)]
        [NoScaleOffset]_Splat2 ("Layer 2(B)", 2D) = "white" { }

        [Space(20)]
        [Header(ForthMapInfo)]
        [NoScaleOffset]_Splat3 ("Layer 3(Invert)", 2D) = "white" { }

        [Space(20)]
        [Header(OtherInfo)]
        _Control ("Mask(RGB)", 2D) = "red" { }
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


            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            sampler2D _Normal0, _Normal1;
            sampler2D _Control;
            fixed4 _Control_ST;

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
                fixed3 splat0 = tex2D(_Splat0, uv01.xy);
                fixed3 splat1 = tex2D(_Splat1, uv01.zw);
                fixed3 splat2 = tex2D(_Splat2, uv23.xy);
                fixed3 splat3 = tex2D(_Splat3, uv23.zw);


                fixed3 normal0 = UnpackNormal(tex2D(_Normal0, uv01.xy)).xzy;
                fixed3 normal1 = UnpackNormal(tex2D(_Normal1, uv01.zw)).xzy;
                fixed3 mask = tex2D(_Control, TRANSFORM_TEX(i.uv.zw, _Control));
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
    //	/////////////////////////////////////////////////////////////////////////////////////////////////////
    SubShader {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry+1" }
        LOD 500
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
            #pragma multi_compile _ ENABLE_DUMMY_SHADOW

            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants LIGHTMAP_ON

            #pragma target 2.0

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
                fixed4 vertexColor : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
                fixed4 vertexColor : COLOR;
                float4 worldPos : TEXCOORD1;
                fixed3 lightDir : TEXCOORD2;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };


            #define _HeightLow  fixed4(0.206f, 0.37f, 0.404f, 0.765f) //use different values in each dimension of low and high to avoid smoothstep inf error
            #define _HeightHigh fixed4(0.962f, 0.57f, 0.827f, 0.942f)

            #define _AmbientColor fixed4(0.5f, 0.5f, 0.5f, 0.5f)
            #define _FadeNearFar fixed4(88.0f, 351.0f, 1.0f, 1.0f)
            #define _SoliColor fixed4(0.91f, 0.94f, 1.0f, 1.0f)
            #define _SandColor fixed4(0.95f, 0.82f, 0.67f, 1.0f)

            //tiling开放(第四张图有两个材质,tiling数值不一样无法写死)
            half4 _Tilling;
            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            sampler2D _Normal0, _Normal1;
            sampler2D _Control;
            sampler2D _LightMap;
            //sampler2D _WholeDiffuse;


            v2f vert(appdata v) {
                v2f o;
                o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(o.worldPos.xyz);
                o.uv.zw = v.uv;
                o.uv.xy = o.worldPos.xz * 0.025 - floor(_WorldSpaceCameraPos.xz * 0.025);

                float dist = distance(o.worldPos.xyz, _WorldSpaceCameraPos.xyz);
                o.worldPos.w = smoothstep(_FadeNearFar.x, _FadeNearFar.y, dist);

                o.normal.xyz = normalize(UnityObjectToWorldNormal(v.normal));
                o.vertexColor = v.vertexColor;
                o.lightDir = normalize(_WorldSpaceLightPos0.xyz);
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 blendFactor(fixed4 height, fixed4 control) {
                fixed4 blendValue = height * control;
                fixed maxValue = max(blendValue.r, max(blendValue.g, max(blendValue.b, blendValue.a)));

                blendValue = max(blendValue - maxValue + 0.2, 0.0f) * control;
                return blendValue / (dot(blendValue, fixed4(1.0f, 1.0f, 1.0f, 1.0f)) + 0.001f);
            }

            fixed4 frag(v2f i) : SV_Target {
                //Calculate uv
                fixed4 uv01 = i.uv.xyxy * _Tilling.xxyy;
                fixed4 uv23 = i.uv.xyxy * _Tilling.zzww;

                //Albedo
                fixed3 splat0 = tex2D(_Splat0, uv01.xy);
                fixed3 splat1 = tex2D(_Splat1, uv01.zw);
                fixed3 splat2 = tex2D(_Splat2, uv23.xy);
                fixed3 splat3 = tex2D(_Splat3, uv23.zw);

                //Normal
                fixed3 normal0 = UnpackNormal(tex2D(_Normal0, uv01.xy)).xzy;
                fixed3 normal1 = UnpackNormal(tex2D(_Normal1, uv01.zw)).xzy;

                //Mask
                fixed3 mask = tex2D(_Control, i.uv.zw);
                fixed a = 1 - saturate(dot(mask.rgb, fixed3(1, 1, 1)));

                fixed4 height = fixed4(splat0.r, splat1.r, splat2.r, splat3.b);
                height = smoothstep(_HeightLow, _HeightHigh, height);
                fixed4 control = blendFactor(height, fixed4(mask, a));

                //Lighting model
                fixed3 normal = normal0 * control.r + normal1 * control.g + i.normal.xyz * saturate(1.0f - dot(control.rg, 1.0f));
                fixed NdotL = saturate(dot(normal, i.lightDir.xyz));
                

                //Specular  (Spec Off)
                //fixed4 spec4 = fixed4(pow(fixed4(NdotH, NdotH, NdotH, NdotH), _Gloss)) * _SpecPower;
                //fixed4 specMap = fixed4(splat0.r, splat1.b, splat2.r, splat3.g);
                //specMap = smoothstep(_SpecLow,_SpecHigh,specMap);
                //fixed spec = dot(spec4 * specMap , control);

                //fixed3 specCol = _SpecColor1.rgb * control.r + _SpecColor2.rgb * control.g + _SpecColor3.rgb * control.b + _SpecColor4.rgb * control.a;
                //specCol *= spec;

                //Diffuse
                //fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + splat3 * control.a;
                // = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (splat3.r * _SoliColor.rgb * (1 - i.vertexColor.rgb) + splat3.g * _SandColor.rgb * i.vertexColor.rgb) * control.a;
                fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * (1 - i.vertexColor.rgb) + splat3.g * _SandColor * i.vertexColor.rgb) * control.a;

                //Ambient
                //Ignore the difference by the texture compression
                //fixed controlLen = dot(control, fixed4(1.0f, 1.0f, 1.0f, 1.0f));
                fixed3 ambientCol = _AmbientColor.rgb;// * controlLen;

                //Lightmap
                fixed3 lightmapCol = tex2D(_LightMap, i.uv.zw);
                lightmapCol *= 2;

                fixed snowMask = control.a * (1 - i.vertexColor.a);
                fixed3 diffuse = lerp(NdotL, clamp(NdotL, 0.0, 0.55), snowMask);

                fixed3 col = albedoCol * (diffuse * _LightColor0 + ambientCol);
                //col = lerp(col, col * (lightmapCol * 2), 0.8f) + specCol * NdotL * _LightColor0;
                col.rgb = lerp(col, col * lerp(lightmapCol, clamp(lightmapCol, 0, 1), (1 - i.vertexColor.a) * control.a), 0.8);

                #if defined(ENABLE_DUMMY_SHADOW)
                    fixed lmAtten = lightmapCol.r;
                    float2 shadowCoord = float2(i.worldPos.x - _LocalPlayerPos.x, i.worldPos.z - _LocalPlayerPos.z);
                    if (dot(shadowCoord, shadowCoord) < 1.5) {
                        shadowCoord = float2(shadowCoord.x + 1.0, shadowCoord.y + 1.0) * 0.5;
                        col.rgb *= lerp(1.0, lerp(tex2D(_ShadowBlob, shadowCoord).rgb, 1.0, saturate(_LocalPlayerPos.y - i.worldPos.y)), saturate(lmAtten * 5.0 - 1.4));
                    }
                #endif

                #if defined(SHADOWS_SCREEN)
                    BR_SHADOW_ATTENUATION(brShadow)
                    BR_LIGHT_ATTENUATION(atten, i)
                    BR_SHADOW_FINAL(atten)

                    BR_SHADOW_COLOR(col, atten)
                #endif

                //Blend with distant
				/*
                fixed3 wholeDiffuse = tex2D(_WholeDiffuse, i.uv.zw);
                fixed3 wholeCol = wholeDiffuse;
                col.rgb = lerp(col.rgb, wholeCol, i.worldPos.w);
				*/
                return fixed4(col, 1.0f);
            }
            ENDCG

        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    SubShader {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry+1" }
        LOD 300
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
            #pragma multi_compile _ ENABLE_DUMMY_SHADOW

            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants LIGHTMAP_ON

            #pragma target 2.0

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
                fixed4 vertexColor : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                fixed4 vertexColor : COLOR;
                fixed diffuse : TEXCOORD2;
                SHADOW_COORDS(4)
                UNITY_FOG_COORDS(5)
            };
            #define _HeightDiff fixed4(1.322f, 5f, 2.364f, 5.649f)  //  1.0f /(_HeightHigh - _HeightLow) = 1.0f /(fixed4(0.962f,0.57f,0.827,0.942f) -  fixed4(0.206f,0.37f,0.404f,0.765f))
            #define _HeightLow  fixed4(0.206f, 0.37f, 0.404f, 0.765f) //use different values in each dimension of low and high to avoid smoothstep inf error
            #define _HeightHigh fixed4(0.962f, 0.57f, 0.827f, 0.942f)

            #define _AmbientColor fixed4(0.5f, 0.5f, 0.5f, 0.5f)
            #define _FadeNearFar fixed4(88.0f, 351.0f, 1.0f, 1.0f)
            #define _SoliColor fixed4(0.91f, 0.94f, 1.0f, 1.0f)
            #define _SandColor fixed4(0.95f, 0.82f, 0.67f, 1.0f)


            half4 _Tilling;

            sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
            sampler2D _Control;
            sampler2D _LightMap;

            v2f vert(appdata v) {
                v2f o;
                o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityWorldToClipPos(o.worldPos.xyz);
                o.uv.zw = v.uv;
                o.uv.xy = o.worldPos.xz * 0.025 - floor(_WorldSpaceCameraPos.xz * 0.025);

                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.diffuse = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.worldPos.w = 1.0f;
                o.vertexColor = v.vertexColor;
                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 blendFactor(fixed4 height, fixed4 control) {
                fixed4 blendValue = height * control;
                fixed maxValue = max(blendValue.r, max(blendValue.g, max(blendValue.b, blendValue.a)));

                blendValue = max(blendValue - maxValue + 0.2f, 0.0f) * control;
                return blendValue / (dot(blendValue, fixed4(1.0f, 1.0f, 1.0f, 1.0f)) + 0.001f);
            }

            fixed4 frag(v2f i) : SV_Target {
                //Calculate uv
                fixed4 uv01 = i.uv.xyxy * _Tilling.xxyy;
                fixed4 uv23 = i.uv.xyxy * _Tilling.zzww;

                //Albedo
                fixed3 splat0 = tex2D(_Splat0, uv01.xy);
                fixed3 splat1 = tex2D(_Splat1, uv01.zw);
                fixed3 splat2 = tex2D(_Splat2, uv23.xy);
                fixed3 splat3 = tex2D(_Splat3, uv23.zw);

                //Mask
                fixed3 mask = tex2D(_Control, i.uv.zw);
                fixed a = 1 - saturate(dot(mask.rgb, fixed3(1, 1, 1)));

                fixed4 height = fixed4(splat0.r, splat1.r, splat2.r, splat3.b);
                height = smoothstep(_HeightLow, _HeightHigh, height);
                //height = saturate((height - _HeightLow) * _HeightDiff);
                fixed4 control = blendFactor(height, fixed4(mask, a));

                //Lighting model
                fixed NdotL = i.diffuse;

                //Diffuse
                //fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + splat3.r * _SoliColor.rgb  * control.a;
                //fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * control.a;
                fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * (1 - i.vertexColor.rgb) + splat3.g * _SandColor * i.vertexColor.rgb) * control.a;
                fixed snowMask = control.a * (1 - i.vertexColor.a);
                fixed3 diffuse = lerp(NdotL, clamp(NdotL, 0.0, 0.55), snowMask);
                //Ambient
                //Ignore the difference by the texture compression
                //fixed controlLen = dot(control, fixed4(1.0f, 1.0f, 1.0f, 1.0f));
                fixed3 ambientCol = _AmbientColor.rgb;// * controlLen;

                //Lightmap
                fixed3 lightmapCol = tex2D(_LightMap, i.uv.zw);
                lightmapCol *= 2;

                fixed3 col = albedoCol * (diffuse * _LightColor0 + ambientCol);
                //col = lerp(col, col * (lightmapCol * 2), 0.8f);
                col = lerp(col, col * lerp(lightmapCol, clamp(lightmapCol, 0, 1), (1 - i.vertexColor.a) * control.a), 0.8) ;

                #if defined(ENABLE_DUMMY_SHADOW)
                    fixed lmAtten = lightmapCol.r;
                    float2 shadowCoord = float2(i.worldPos.x - _LocalPlayerPos.x, i.worldPos.z - _LocalPlayerPos.z);
                    if (dot(shadowCoord, shadowCoord) < 1.5) {
                        shadowCoord = float2(shadowCoord.x + 1.0, shadowCoord.y + 1.0) * 0.5;
                        col.rgb *= lerp(1.0, lerp(tex2D(_ShadowBlob, shadowCoord).rgb, 1.0, saturate(_LocalPlayerPos.y - i.worldPos.y)), saturate(lmAtten * 5.0 - 1.4));
                    }
                #endif
                return fixed4(col, 1.0f);
            }
            ENDCG

        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    SubShader {
        Tags { "RenderType" = "Opaque" "LightMode" = "ForwardBase" "Queue" = "Geometry+1" }
        LOD 200
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

            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH
            #pragma skip_variants LIGHTMAP_ON

            #pragma target 2.0

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed3 normal : NORMAL;
                fixed4 vertexColor : COLOR;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                fixed diffuse : TEXCOORD2;
                fixed4 vertexColor : COLOR;
            };


            #define _AmbientColor fixed4(0.5f, 0.5f, 0.5f, 0.5f)
            #define _FadeNearFar fixed4(88.0f, 351.0f, 1.0f, 1.0f)
            #define _SoliColor fixed4(0.91f, 0.94f, 1.0f, 1.0f)
            #define _SandColor fixed4(0.95f, 0.82f, 0.67f, 1.0f)

            half4 _Tilling;
            sampler2D _Splat1, _Splat2, _Splat3;
            sampler2D _Control;
            sampler2D _LightMap;
            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv.zw = v.uv;
                o.uv.xy = o.worldPos.xz * 0.025 - floor(_WorldSpaceCameraPos.xz * 0.025);

                fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
                o.diffuse = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.worldPos.w = 1.0f;
                o.vertexColor = v.vertexColor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                //Calculate uv
                fixed4 uv01 = i.uv.xyxy * _Tilling.xxyy;
                fixed4 uv23 = i.uv.xyxy * _Tilling.zzww;

                //Albedo
                fixed3 splat1 = tex2D(_Splat1, uv01.zw);
                fixed3 splat2 = tex2D(_Splat2, uv23.xy);
                fixed3 splat3 = tex2D(_Splat3, uv23.zw);
                fixed3 splat0 = splat1;

                //Mask
                fixed3 mask = tex2D(_Control, i.uv.zw);
                fixed a = 1 - saturate(dot(mask.rgb, fixed3(1.0f, 1.0f, 1.0f)));

                fixed4 control = fixed4(mask, a);
                fixed controlLen = dot(control, fixed4(1.0f, 1.0f, 1.0f, 1.0f));
                control = control / controlLen;

                //Lighting model
                fixed NdotL = i.diffuse;

                //Diffuse
                //fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + splat3.r * _SoliColor.rgb * control.a;
                //fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * control.a;
                fixed3 albedoCol = splat0.rgb * control.r + splat1.rgb * control.g + splat2 * control.b + (lerp(splat3.b, splat3.r * _SoliColor, i.vertexColor.a) * (1 - i.vertexColor.rgb) + splat3.g * _SandColor * i.vertexColor.rgb) * control.a;
                //Ambient
                //Ignore the difference by the texture compression
                fixed3 ambientCol = _AmbientColor.rgb;// * controlLen;

                //Lightmap
                fixed3 lightmapCol = tex2D(_LightMap, i.uv.zw);
                lightmapCol *= 2;

                fixed3 col = albedoCol * (NdotL * _LightColor0 + ambientCol);
                col = lerp(col, col * lerp(lightmapCol, clamp(lightmapCol, 0, 1), (1 - i.vertexColor.a) * control.a), 0.8);

                return fixed4(col, 1.0f);
            }
            ENDCG

        }
    }

    CustomEditor "Terrain_4Map2Normal1LightmapGlobalRimGUI"
}