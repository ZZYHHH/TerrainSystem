using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Terrain_4Map2Normal1LightmapGlobalRimGUI : ShaderGUI
{
    private enum ChannelMode
    {
        R = 0,
        G = 1,
        B = 2,
    }

    //MaterialProperty _Debug_prop = null;
    MaterialProperty _DebugSpec_prop = null;
    MaterialProperty _DebugGray_prop = null;
    MaterialProperty _SpecFrom1_prop = null;
    MaterialProperty _SpecFrom2_prop = null;
    MaterialProperty _SpecFrom3_prop = null;
    MaterialProperty _GrayFrom1_prop = null;
    MaterialProperty _GrayFrom2_prop = null;
    MaterialProperty _GrayFrom3_prop = null;

    MaterialProperty _SpecColor1_prop = null;
    MaterialProperty _SpecColor2_prop = null;
    MaterialProperty _SpecColor3_prop = null;
    MaterialProperty _SpecColor4_prop = null;

    MaterialProperty _AmbientColor1_prop = null;
    MaterialProperty _AmbientColor2_prop = null;
    MaterialProperty _AmbientColor3_prop = null;
    MaterialProperty _AmbientColor4_prop = null;

    MaterialProperty _Tilling_prop = null;

    MaterialProperty _SoliColor_prop = null;
    MaterialProperty _SandColor_prop = null;

    MaterialProperty _Gloss_prop = null;
    MaterialProperty _SpecPower_prop = null;

    MaterialProperty _SpecLow_prop = null;
    MaterialProperty _SpecHigh_prop = null;
    MaterialProperty _HeightLow_prop = null;
    MaterialProperty _HeightHigh_prop = null;
    MaterialProperty _Splat0_prop = null;
    MaterialProperty _Splat1_prop = null;
    MaterialProperty _Splat2_prop = null;
    MaterialProperty _Splat3_prop = null;
    MaterialProperty _Normal0_prop = null;
    MaterialProperty _Normal1_prop = null;
    MaterialProperty _Control_prop = null;
    MaterialProperty _LightMap_prop = null;
    MaterialProperty _BlendWeight_prop = null;
    MaterialProperty _SandRimTex_prop = null;
    MaterialProperty _WholeDiffuse_prop = null;
    MaterialProperty _WholeNormal_prop = null;
    MaterialProperty _FadeNearFar_prop = null;
    MaterialProperty _WholeAmbientColor_prop = null;
    MaterialProperty _ShadowBlob_prop = null;

    //MaterialProperty _TopView_prop = null;
    //MaterialProperty _TopViewNormal_prop = null;

    //MaterialProperty _SixInOneDiffuse = null;
    //MaterialProperty _SixInOneID = null;
    //MaterialProperty _SixInOneMask = null;

    ChannelMode[] specMode = new ChannelMode[] { ChannelMode.R, ChannelMode.R, ChannelMode.R, ChannelMode.R };

    ChannelMode[] grayMode = new ChannelMode[] { ChannelMode.R, ChannelMode.R, ChannelMode.R, ChannelMode.R };
    MaterialEditor _MaterialEditor;
    //Vector4[] rgba = new Vector4[4] { new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1) };

    public void FindProperties(MaterialProperty[] props)
    {
        _DebugSpec_prop = FindProperty("_DebugSpec", props);
        _DebugGray_prop = FindProperty("_DebugGray", props);
        _SpecFrom1_prop = FindProperty("_SpecFrom1", props);
        _SpecFrom2_prop = FindProperty("_SpecFrom2", props);
        _SpecFrom3_prop = FindProperty("_SpecFrom3", props);
        _GrayFrom1_prop = FindProperty("_GrayFrom1", props);
        _GrayFrom2_prop = FindProperty("_GrayFrom2", props);
        _GrayFrom3_prop = FindProperty("_GrayFrom3", props);

        if (_SpecFrom1_prop.vectorValue.x != 0) specMode[0] = ChannelMode.R;
        if (_SpecFrom1_prop.vectorValue.y != 0) specMode[1] = ChannelMode.R;
        if (_SpecFrom1_prop.vectorValue.z != 0) specMode[2] = ChannelMode.R;
        if (_SpecFrom1_prop.vectorValue.w != 0) specMode[3] = ChannelMode.R;

        if (_SpecFrom2_prop.vectorValue.x != 0) specMode[0] = ChannelMode.G;
        if (_SpecFrom2_prop.vectorValue.y != 0) specMode[1] = ChannelMode.G;
        if (_SpecFrom2_prop.vectorValue.z != 0) specMode[2] = ChannelMode.G;
        if (_SpecFrom2_prop.vectorValue.w != 0) specMode[3] = ChannelMode.G;

        if (_SpecFrom3_prop.vectorValue.x != 0) specMode[0] = ChannelMode.B;
        if (_SpecFrom3_prop.vectorValue.y != 0) specMode[1] = ChannelMode.B;
        if (_SpecFrom3_prop.vectorValue.z != 0) specMode[2] = ChannelMode.B;
        if (_SpecFrom3_prop.vectorValue.w != 0) specMode[3] = ChannelMode.B;

        if (_GrayFrom1_prop.vectorValue.x != 0) grayMode[0] = ChannelMode.R;
        if (_GrayFrom1_prop.vectorValue.y != 0) grayMode[1] = ChannelMode.R;
        if (_GrayFrom1_prop.vectorValue.z != 0) grayMode[2] = ChannelMode.R;
        if (_GrayFrom1_prop.vectorValue.w != 0) grayMode[3] = ChannelMode.R;

        if (_GrayFrom2_prop.vectorValue.x != 0) grayMode[0] = ChannelMode.G;
        if (_GrayFrom2_prop.vectorValue.y != 0) grayMode[1] = ChannelMode.G;
        if (_GrayFrom2_prop.vectorValue.z != 0) grayMode[2] = ChannelMode.G;
        if (_GrayFrom2_prop.vectorValue.w != 0) grayMode[3] = ChannelMode.G;

        if (_GrayFrom3_prop.vectorValue.x != 0) grayMode[0] = ChannelMode.B;
        if (_GrayFrom3_prop.vectorValue.y != 0) grayMode[1] = ChannelMode.B;
        if (_GrayFrom3_prop.vectorValue.z != 0) grayMode[2] = ChannelMode.B;
        if (_GrayFrom3_prop.vectorValue.w != 0) grayMode[3] = ChannelMode.B;


        _SpecColor1_prop = FindProperty("_SpecColor1", props);
        _SpecColor2_prop = FindProperty("_SpecColor2", props);
        _SpecColor3_prop = FindProperty("_SpecColor3", props);
        _SpecColor4_prop = FindProperty("_SpecColor4", props);


        _AmbientColor1_prop = FindProperty("_AmbientColor1", props);
        _AmbientColor2_prop = FindProperty("_AmbientColor2", props);
        _AmbientColor3_prop = FindProperty("_AmbientColor3", props);
        _AmbientColor4_prop = FindProperty("_AmbientColor4", props);

        _Tilling_prop = FindProperty("_Tilling", props);

        _SoliColor_prop = FindProperty("_SoliColor", props);
        _SandColor_prop = FindProperty("_SandColor", props);

        _Gloss_prop = FindProperty("_Gloss", props);
        _SpecPower_prop = FindProperty("_SpecPower", props);

        _SpecLow_prop = FindProperty("_SpecLow", props);
        _SpecHigh_prop = FindProperty("_SpecHigh", props);
        _HeightLow_prop = FindProperty("_HeightLow", props);
        _HeightHigh_prop = FindProperty("_HeightHigh", props);
        _Splat0_prop = FindProperty("_Splat0", props);
        _Splat1_prop = FindProperty("_Splat1", props);
        _Splat2_prop = FindProperty("_Splat2", props);
        _Splat3_prop = FindProperty("_Splat3", props);
        _Normal0_prop = FindProperty("_Normal0", props);
        _Normal1_prop = FindProperty("_Normal1", props);
        _Control_prop = FindProperty("_Control", props);
        _LightMap_prop = FindProperty("_LightMap", props);
        _BlendWeight_prop = FindProperty("_BlendWeight", props);
        _SandRimTex_prop = FindProperty("_SandRimTex", props);
        _WholeDiffuse_prop = FindProperty("_WholeDiffuse", props);
        _WholeNormal_prop = FindProperty("_WholeNormal", props);
        _FadeNearFar_prop = FindProperty("_FadeNearFar", props);
        _WholeAmbientColor_prop = FindProperty("_WholeAmbientColor", props);

        _ShadowBlob_prop = FindProperty("_ShadowBlob", props);

        //_SixInOneDiffuse = FindProperty("_SixInOneDiffuse", props);
        //_SixInOneID = FindProperty("_SixInOneID", props);
        //_SixInOneMask = FindProperty("_SixInOneMask", props);
    }
    private static class Styles
    {
        public static GUIContent NoneText = new GUIContent("");
        public static GUIContent DebugSpecText = new GUIContent("S", "显示高光图？");
        public static GUIContent DebugHeightText = new GUIContent("H", "显示高度图？");
        public static GUIContent SplatMapText = new GUIContent("map");
        public static GUIContent SplatNormalText = new GUIContent("normal");
        public static GUIContent TillingText = new GUIContent("Tile");
        public static GUIContent FirstMapsText = new GUIContent("第一张图");
        public static GUIContent SecontMapsText = new GUIContent("第二张图");
        public static GUIContent ThirdMapsText = new GUIContent("第三张图");
        public static GUIContent ForthMapsText = new GUIContent("第四张图");
        public static GUIContent MaskmapText = new GUIContent("Mask Map");
        public static GUIContent LightmapText = new GUIContent("Light Map");
        public static GUIContent TopViewText = new GUIContent("Top View Diffuse");
        public static GUIContent TopViewNormalText = new GUIContent("Top View Normal");
        public static GUIContent ShadowBlobText = new GUIContent("Shadow Blob");
    }


    //bool useSixInOne = false;
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {


        FindProperties(properties);
        _MaterialEditor = materialEditor;
        //base.OnGUI(materialEditor, properties);
        Material mat = materialEditor.target as Material;

        EditorGUI.BeginChangeCheck();
        {
            MaterialProperty[] proptmp = new MaterialProperty[] { _Splat0_prop, _Normal0_prop, _Tilling_prop, _DebugSpec_prop, _DebugGray_prop, _SpecLow_prop, _SpecHigh_prop, _HeightLow_prop, _HeightHigh_prop, _Gloss_prop, _SpecPower_prop, _SpecColor1_prop, _AmbientColor1_prop };
            int index = 0;
            DrawLayer(Styles.FirstMapsText, Styles.SplatMapText, Styles.TillingText, proptmp, index);

            proptmp = new MaterialProperty[] { _Splat1_prop, _Normal1_prop, _Tilling_prop, _DebugSpec_prop, _DebugGray_prop, _SpecLow_prop, _SpecHigh_prop, _HeightLow_prop, _HeightHigh_prop, _Gloss_prop, _SpecPower_prop, _SpecColor2_prop, _AmbientColor2_prop };
            index = 1;
            DrawLayer(Styles.SecontMapsText, Styles.SplatMapText, Styles.TillingText, proptmp, index);


            proptmp = new MaterialProperty[] { _Splat2_prop, null, _Tilling_prop, _DebugSpec_prop, _DebugGray_prop, _SpecLow_prop, _SpecHigh_prop, _HeightLow_prop, _HeightHigh_prop, _Gloss_prop, _SpecPower_prop, _SpecColor3_prop, _AmbientColor3_prop };
            index = 2;
            DrawLayer(Styles.ThirdMapsText, Styles.SplatMapText, Styles.TillingText, proptmp, index);


            proptmp = new MaterialProperty[] { _Splat3_prop, null, _Tilling_prop, _DebugSpec_prop, _DebugGray_prop, _SpecLow_prop, _SpecHigh_prop, _HeightLow_prop, _HeightHigh_prop, _Gloss_prop, _SpecPower_prop, _SpecColor4_prop, _AmbientColor4_prop};
            index = 3;
            DrawLayer(Styles.ForthMapsText, Styles.SplatMapText, Styles.TillingText, proptmp, index);



            //Draw Preview
            /*
            GUILayout.Space(30);
            GUILayout.BeginVertical("groupbox");
            GUILayout.Label(new GUIContent("鸟视图"), EditorStyles.boldLabel);
            
            Rect topViewRect = _MaterialEditor.TexturePropertySingleLine(Styles.TopViewText, _TopView_prop);
            Texture topViewDiffuse = _TopView_prop.textureValue;
            mat.DisableKeyword("TOP_VIEW_BLEND");
            if (topViewDiffuse != null)
            {
                Rect topNormRect = new Rect(topViewRect);
                topNormRect.x += 200;
                _MaterialEditor.TexturePropertyMiniThumbnail(topNormRect, _TopViewNormal_prop, Styles.TopViewText.text, "");
                mat.EnableKeyword("TOP_VIEW_BLEND");
            }
           
            GUILayout.EndVertical();
             */
            //Draw Others
            GUILayout.Space(30);
            GUILayout.BeginVertical("groupbox");
            GUILayout.Label(new GUIContent("其他设置"), EditorStyles.boldLabel);

            _SoliColor_prop.colorValue = EditorGUILayout.ColorField("SoliColor", _SoliColor_prop.colorValue);
            _SandColor_prop.colorValue = EditorGUILayout.ColorField("SandColor", _SandColor_prop.colorValue);

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Blend");
            _BlendWeight_prop.floatValue = EditorGUILayout.Slider(_BlendWeight_prop.floatValue, 0, 1);
            GUILayout.EndHorizontal();

            _WholeAmbientColor_prop.colorValue = EditorGUILayout.ColorField("Whole Ambient Color", _WholeAmbientColor_prop.colorValue);


            Rect maskMapRect = _MaterialEditor.TexturePropertySingleLine(Styles.MaskmapText, _Control_prop);
            Rect lightMapRect = new Rect(maskMapRect);
            lightMapRect.x += 100;
            _MaterialEditor.TexturePropertyMiniThumbnail(lightMapRect, _LightMap_prop, "Light Map", "");

            lightMapRect.x += 100;
            _MaterialEditor.TexturePropertyMiniThumbnail(lightMapRect, _SandRimTex_prop, "Sand Rim Texture", "");


            lightMapRect.x += 150;
            _MaterialEditor.TexturePropertyMiniThumbnail(lightMapRect, _WholeDiffuse_prop, "WholeDiff", "");

            lightMapRect.x += 100;
            _MaterialEditor.TexturePropertyMiniThumbnail(lightMapRect, _WholeNormal_prop, "WholeNorm", "");

            Vector4 fadenearfar = _FadeNearFar_prop.vectorValue;
            fadenearfar.x = EditorGUILayout.Slider(fadenearfar.x, 0, 2000);
            fadenearfar.y = EditorGUILayout.Slider(fadenearfar.y, 0, 2000);
            _FadeNearFar_prop.vectorValue = fadenearfar;

            _MaterialEditor.TexturePropertySingleLine(Styles.ShadowBlobText, _ShadowBlob_prop);

            GUILayout.EndVertical();

            ////Draw Others
            //GUILayout.Space(30);
            //GUILayout.BeginVertical("groupbox");

            //GUILayout.BeginHorizontal();
            //GUIContent sixInOneLable = new GUIContent("SixInOne Switch");
            //GUILayout.Label(sixInOneLable, EditorStyles.boldLabel);
            //Rect tileRect = GUILayoutUtility.GetRect(sixInOneLable, GUIStyle.none);
            ////Rect sixInOneLableRect = _MaterialEditor.TexturePropertySingleLine(sixInOneLable, properties[0]);
            ////Rect tileRect = new Rect(sixInOneLable.pos);
            //tileRect.x -= 60;  tileRect.y += 5;
            //GUILayout.EndHorizontal();
            //GUILayout.BeginHorizontal();

            //if (mat.IsKeywordEnabled("SIX_IN_ONE"))
            //    useSixInOne = true;
            //else
            //    useSixInOne = false;

            //useSixInOne = EditorGUI.Toggle(tileRect, useSixInOne, "radio");

            //if (useSixInOne)
            //    mat.EnableKeyword("SIX_IN_ONE");
            //else
            //    mat.DisableKeyword("SIX_IN_ONE");
            
            //GUILayout.EndHorizontal();

            //GUILayout.Label(new GUIContent("Diffuse"), EditorStyles.boldLabel);
            //Rect diffuseRect = _MaterialEditor.TexturePropertySingleLine(new GUIContent("SixInOne Diffuse"), _SixInOneDiffuse);
            //Texture sixInOneDiffuse = _SixInOneDiffuse.textureValue;
          
            //GUILayout.Label(new GUIContent("ID"), EditorStyles.boldLabel);
            //Rect idRect = _MaterialEditor.TexturePropertySingleLine(new GUIContent("SixInOne ID"), _SixInOneID);
            //Texture sixInOneID = _SixInOneID.textureValue;

            //GUILayout.Label(new GUIContent("Mask"), EditorStyles.boldLabel);
            //Rect maskRect = _MaterialEditor.TexturePropertySingleLine(new GUIContent("SixInOne Mask"), _SixInOneMask);
            //Texture sixInOneMask = _SixInOneMask.textureValue;

            //GUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(mat);
        }
    }

    public void DrawRim(MaterialProperty RimColor_prop, MaterialProperty RimWeight_prop)
    {
        //Draw Rim
        GUILayout.BeginHorizontal();
        GUILayout.Label("Rim", GUILayout.MaxWidth(28));
        RimColor_prop.colorValue = EditorGUILayout.ColorField(RimColor_prop.colorValue);
        Vector4 rimweight = RimWeight_prop.vectorValue;
        //rimweight.x = EditorGUILayout.IntSlider("",(int)rimweight.x, 0, 16, GUILayout.MaxWidth(200));
        rimweight.x = EditorGUILayout.Slider("", rimweight.x, 0, 16, GUILayout.MaxWidth(200));
        rimweight.y = EditorGUILayout.Slider("", rimweight.y, 0, 10.0f, GUILayout.MaxWidth(170));
        RimWeight_prop.vectorValue = rimweight;

        GUILayout.EndHorizontal();
    }


    public void DrawLayer(GUIContent layerLabel, GUIContent splatLabel, GUIContent tileLabel, MaterialProperty[] properties, int index)
    {
        GUILayout.Space(5);
        GUILayout.BeginVertical("groupbox");
        GUILayout.Label(layerLabel, EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();

        Rect splatMapRect = _MaterialEditor.TexturePropertySingleLine(splatLabel, properties[0]);
        Rect splatNormalRect = new Rect(splatMapRect);
        splatNormalRect.x += 65;
        if (properties[1] != null)
            _MaterialEditor.TexturePropertyMiniThumbnail(splatNormalRect, properties[1], "normal", "");

        Vector4 tilling = properties[2].vectorValue;
        Rect tileLabelRect = new Rect(splatNormalRect);
        tileLabelRect.x += 75;
        EditorGUI.PrefixLabel(tileLabelRect, tileLabel);
        Rect tileRect = new Rect(tileLabelRect);
        tileRect.x += 25;
        tileRect.width = 30;
        tilling[index] = EditorGUI.FloatField(tileRect, properties[2].vectorValue[index]);
        properties[2].vectorValue = tilling;

        Vector4 debugspec = properties[3].vectorValue;
        bool debugspecFlag = debugspec[index] > 0;
        Rect debutspec1Rect = new Rect(tileRect);
        debutspec1Rect.x += 35;
        debugspecFlag = EditorGUI.Toggle(debutspec1Rect, debugspecFlag, "button");
        debugspec[index] = debugspecFlag ? 1 : 0;
        properties[3].vectorValue = debugspec;

        Vector4 debuggray = properties[4].vectorValue;
        bool debuggrayFlag = debuggray[index] > 0;
        Rect debutgray1Rect = new Rect(debutspec1Rect);
        debutgray1Rect.x += 30;
        debuggrayFlag = EditorGUI.Toggle(debutgray1Rect, debuggrayFlag, "button");
        debuggray[index] = debuggrayFlag ? 1 : 0;
        properties[4].vectorValue = debuggray;

        GUILayout.EndHorizontal();


        specMode[index] = (ChannelMode)EditorGUILayout.EnumPopup("spec from", specMode[index]);

        switch (specMode[index])
        {
            case ChannelMode.R:
                {
                    Vector4 specfrom1 = _SpecFrom1_prop.vectorValue;
                    Vector4 specfrom2 = _SpecFrom2_prop.vectorValue;
                    Vector4 specfrom3 = _SpecFrom3_prop.vectorValue;
                    specfrom1[index] = 1;
                    specfrom2[index] = 0;
                    specfrom3[index] = 0;
                    _SpecFrom1_prop.vectorValue = specfrom1;
                    _SpecFrom2_prop.vectorValue = specfrom2;
                    _SpecFrom3_prop.vectorValue = specfrom3;
                }
                break;
            case ChannelMode.G:
                {
                    Vector4 specfrom1 = _SpecFrom1_prop.vectorValue;
                    Vector4 specfrom2 = _SpecFrom2_prop.vectorValue;
                    Vector4 specfrom3 = _SpecFrom3_prop.vectorValue;
                    specfrom1[index] = 0;
                    specfrom2[index] = 1;
                    specfrom3[index] = 0;
                    _SpecFrom1_prop.vectorValue = specfrom1;
                    _SpecFrom2_prop.vectorValue = specfrom2;
                    _SpecFrom3_prop.vectorValue = specfrom3;
                }
                break;
            case ChannelMode.B:
                {
                    Vector4 specfrom1 = _SpecFrom1_prop.vectorValue;
                    Vector4 specfrom2 = _SpecFrom2_prop.vectorValue;
                    Vector4 specfrom3 = _SpecFrom3_prop.vectorValue;
                    specfrom1[index] = 0;
                    specfrom2[index] = 0;
                    specfrom3[index] = 1;
                    _SpecFrom1_prop.vectorValue = specfrom1;
                    _SpecFrom2_prop.vectorValue = specfrom2;
                    _SpecFrom3_prop.vectorValue = specfrom3;
                }
                break;
        }
        Vector4 specLow = properties[5].vectorValue;
        //specLow.x = GUILayout.HorizontalSlider(specLow.x, -2f, 2f, GUILayout.Width(260), GUILayout.ExpandWidth(false));
        specLow[index] = EditorGUILayout.Slider("", properties[5].vectorValue[index], -1f, 1f);
        properties[5].vectorValue = specLow;


        Vector4 specHigh = properties[6].vectorValue;
        //specHigh.x = GUILayout.HorizontalSlider(specHigh.x, -2f, 2f, GUILayout.MinWidth(260), GUILayout.ExpandWidth(false));
        specHigh[index] = EditorGUILayout.Slider("", properties[6].vectorValue[index], -1f, 1f);
        properties[6].vectorValue = specHigh;


        grayMode[index] = (ChannelMode)EditorGUILayout.EnumPopup("height from", grayMode[index]);

        switch (grayMode[index])
        {
            case ChannelMode.R:
                {
                    Vector4 grayfrom1 = _GrayFrom1_prop.vectorValue;
                    Vector4 grayfrom2 = _GrayFrom2_prop.vectorValue;
                    Vector4 grayfrom3 = _GrayFrom3_prop.vectorValue;
                    grayfrom1[index] = 1;
                    grayfrom2[index] = 0;
                    grayfrom3[index] = 0;
                    _GrayFrom1_prop.vectorValue = grayfrom1;
                    _GrayFrom2_prop.vectorValue = grayfrom2;
                    _GrayFrom3_prop.vectorValue = grayfrom3;
                }
                break;
            case ChannelMode.G:
                {
                    Vector4 grayfrom1 = _GrayFrom1_prop.vectorValue;
                    Vector4 grayfrom2 = _GrayFrom2_prop.vectorValue;
                    Vector4 grayfrom3 = _GrayFrom3_prop.vectorValue;
                    grayfrom1[index] = 0;
                    grayfrom2[index] = 1;
                    grayfrom3[index] = 0;
                    _GrayFrom1_prop.vectorValue = grayfrom1;
                    _GrayFrom2_prop.vectorValue = grayfrom2;
                    _GrayFrom3_prop.vectorValue = grayfrom3;
                }
                break;
            case ChannelMode.B:
                {
                    Vector4 grayfrom1 = _GrayFrom1_prop.vectorValue;
                    Vector4 grayfrom2 = _GrayFrom2_prop.vectorValue;
                    Vector4 grayfrom3 = _GrayFrom3_prop.vectorValue;
                    grayfrom1[index] = 0;
                    grayfrom2[index] = 0;
                    grayfrom3[index] = 1;
                    _GrayFrom1_prop.vectorValue = grayfrom1;
                    _GrayFrom2_prop.vectorValue = grayfrom2;
                    _GrayFrom3_prop.vectorValue = grayfrom3;
                }
                break;
        }

        Vector4 grayLow = properties[7].vectorValue;
        //specLow.x = GUILayout.HorizontalSlider(specLow.x, -2f, 2f, GUILayout.Width(260), GUILayout.ExpandWidth(false));
        grayLow[index] = EditorGUILayout.Slider("", properties[7].vectorValue[index], -1f, 1f);
        properties[7].vectorValue = grayLow;


        Vector4 grayHigh = properties[8].vectorValue;
        //specHigh.x = GUILayout.HorizontalSlider(specHigh.x, -2f, 2f, GUILayout.MinWidth(260), GUILayout.ExpandWidth(false));
        grayHigh[index] = EditorGUILayout.Slider("", properties[8].vectorValue[index], -1f, 1f);
        properties[8].vectorValue = grayHigh;

        //Spec Color
        properties[11].colorValue = EditorGUILayout.ColorField("Specular Color", properties[11].colorValue);

        //GUILayout.BeginHorizontal();
        Vector4 gloss = properties[9].vectorValue;
        gloss[index] = EditorGUILayout.Slider("Gloss", properties[9].vectorValue[index], 0f, 2f);
        properties[9].vectorValue = gloss;

        Vector4 specPower = properties[10].vectorValue;
        specPower[index] = EditorGUILayout.Slider("SpecPower", properties[10].vectorValue[index], 0f, 2f);
        properties[10].vectorValue = specPower;
        // GUILayout.EndHorizontal();

        //Ambient Color
        properties[12].colorValue = EditorGUILayout.ColorField("Ambient Color", properties[12].colorValue);
        GUILayout.EndVertical();
    }
}
