using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class PaintTerrainTool : EditorWindow
{
    [MenuItem("Tools/PaintTerrainTool", false, -10000)]

    private static void Init()
    {
        win = GetWindow<PaintTerrainTool>("PaintTerrainTool");
        win.minSize = new Vector2(500, 700);
        win.GenerateStyles();
    }

    #region 常量

    private const float MINBRUSHSIZE = 0.001f;
    private const float MAXBRUSHSIZE = 30.0f;

    private const string DEFAULTLAYER = "Layer_StaticObject";
    private const string BRUSHESPATH = "Assets/TA_Systems/PaintTerrainTool/Brushes/";
    private const string BURSHPREVIEWSHADERPATH = "Hidden/BrushPreview";
    private const string BLENDMODESHADERPATH = "Hidden/Photoshop_BlendModes_Function";
    #endregion

    #region 变量
    static PaintTerrainTool win;
    GUIStyle infoStyle;
    GUIStyle boxStyle;
    GUIStyle richStyle;

    Vector2 scrollPos = Vector2.zero;
    Vector2 scrollPosJ = Vector2.zero;

    Color PaintColor = new Color(1f, 0f, 0f, 1f);
    Color PaintMaskColor = new Color(1f, 0f, 0f, 0.0f);
    //Color PaintIndex = new Color(0.5f / 16.0f, 1.5f / 16.0f, 2.5f / 16.0f, 3.5f / 16.0f);
    Color FWColor = Color.white;
    Color BGColor = Color.black;

    Color[] ChannelColorArray = new Color[4];

    bool allowPainting = false;
    bool allowPaintingLightmap = false;

    bool isPainting = false;
    Vector2 mousePos = Vector2.zero;
    RaycastHit curHit;
    bool changingBrushValue = false;
    float brushSize = 5f, oldBrushSize = 0.0f;
    //float brushOpacity = 0.0234375f;
    float brushOpacity = 0.1f;
    float burshPreviewOpacity = 1.0f;
    //已经弃用
    float brushFalloff = 0.5f;

    Tool curTool;

    private GameObject terrainObj;
    private int layerMask = 0;
    private string tag = "Terrain";
    private int matIndex = 0;
    private string[] matOptions = new string[] { " " };

    Renderer renderer;
    MeshCollider meshCollider;
    Vector2 pixelUV;
    Texture2D maskTexture;
    Texture2D IDTexture;
    Material terrainMat;

    public Texture tileTexture;
    private Sprite[] tileTextureSprites;
    private Texture[] tileTextureSpritesPreview;

    int selbrush = 0, oldSelBrush = -1;
    Texture[] brushTexes;
    Texture[] splats = new Texture2D[4];
    private Projector MaskPreviewProjector;

    string MaskTexName = "_Control";
    string LightMapName = "_LightMap";
    string[] SplatName = new string[4] { "_Splat0", "_Splat1", "_Splat2", "_Splat3" };
    string inputPath, outputPath, fileName;
    string[] toJsonMaps = new string[6];
    bool IsLoadJson, DrawJsonWindow = false;
    Material replaceMat;

    int blendModeIndex = 11;

    private int tileSpritePreviewWidth = 100;
    private int tileSpritePreviewHeight = 100;
    private int tileSpriteSelectID = 0;

    private bool bOverwriteMask = false;
    private bool bShowAliasing = false;

    private bool RegistUndoOnMouseDownOnly = true;

    Texture2D lightMap;
    Texture2D lightMapPainting;
    RenderTexture lightMapFinal;
    Material blendMat;
    #endregion

    #region 结构
    enum BlendMode
    {
        Darken,
        Multiply,
        ColorBurn,
        LinearBurn,
        DarkerColor,
        Lighten,
        Screen,
        ColorDodge,
        LinearDodge,
        LighterColor,
        Overlay,
        SoftLight,
        HardLight,
        VividLight,
        LinearLight,
        PinLight,
        Hardlerp,
        Difference,
        Exclusion,
        Subtract,
        Divide,
        Hue,
        Saturation,
        Color,
        Luminosity,
    };
    string[] BlendModeNames = new string[] {
                "Darken【变暗】",
                "Multiply【正片叠底】",
                "ColorBurn【颜色加深】",
                "LinearBurn【线性加深】",
                "DarkerColor【深色】",
                "Lighten【变亮】",
                "Screen【滤色】",
                "ColorDodge【颜色减淡】",
                "LinearDodge【线性减】(添加)】",
                "LighterColor【浅色】",
                "Overlay【叠加】",
                "SoftLight【柔光】",
                "HardLight【强光】",
                "VividLight【亮光】",
                "LinearLight【线性光】",
                "PinLight【点光】",
                "Hardlerp【实色混合】",
                "Difference【差值】",
                "Exclusion【排除】",
                "Subtract【减去】",
                "Divide【划分】",
                "Hue【色相】",
                "Saturation【饱和度】",
                "Color【颜色】",
                "Luminosity【明度】"
            };
    BlendMode blendMode = BlendMode.SoftLight;

    Color[] BlendModeBaseColor = new Color[]
    {
                new Color(1,1,1,0),//Darken,
                new Color(1,1,1,0),//Multiply,
                new Color(1,1,1,0),//ColorBurn,
                new Color(1,1,1,0),//LinearBurn,
                new Color(1,1,1,0),//DarkerColor,
                new Color(0,0,0,0),//Lighten,
                new Color(0,0,0,0),//Screen,
                new Color(),//ColorDodge,
                new Color(0,0,0,0),//LinearDodge,
                new Color(0,0,0,0),//LighterColor,
                new Color(0.5f,0.5f,0.5f,0),//Overlay,
                new Color(0.5f,0.5f,0.5f,0),//SoftLight,
                new Color(0.5f,0.5f,0.5f,0),//HardLight,
                new Color(),//VividLight,
                new Color(),//LinearLight,
                new Color(),//PinLight,
                new Color(),//Hardlerp,
                new Color(),//Difference,
                new Color(),//Exclusion,
                new Color(0,0,0,0),//Subtract,
                new Color(1,1,1,0),//Divide,
                new Color(),//Hue,
                new Color(),//Saturation,
                new Color(),//Color,
                new Color(),//Luminosity,
    };
    #endregion

    #region Editor方法
    private void OnEnable()
    {
        layerMask = 1 << LayerMask.NameToLayer(DEFAULTLAYER);

        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        brushTexes = InitBrush();


        ChannelColorArray[0] = new Color(1f, 0f, 0f, 0f);
        ChannelColorArray[1] = new Color(0f, 1f, 0f, 0f);
        ChannelColorArray[2] = new Color(0f, 0f, 1f, 0f);
        ChannelColorArray[3] = new Color(0f, 0f, 0f, 1f);
    }

    private void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        if (MaskPreviewProjector != null)
        {
            DestroyImmediate(MaskPreviewProjector.gameObject);
            MaskPreviewProjector = null;
        }
        if (lightMapFinal != null)
        {
            lightMapFinal.Release();
            DestroyImmediate(lightMapFinal);
        }
        if (renderer != null)
        {

            SetTexutreByPropertyName(renderer, MaskTexName, maskTexture);
            SetTexutreByPropertyName(renderer, LightMapName, lightMap);
        }
    }
    #endregion

    #region GUI

    private void OnGUI()
    {
        using (new EditorGUI.DisabledGroupScope(DrawJsonWindow))
        {
            //Draw Header Text
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Box("Terrain Mask Painter(CPU) V2.0", boxStyle, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            ///------------------------------------


            //layerMask = EditorGUILayout.LayerField("Terrain Layer:", layerMask);
            //layerMask = EditorGUILayout.MaskField("Terrain Layer:",EditorStyles.lay)
            EditorGUILayout.BeginVertical("Box");
            EditorGUI.BeginDisabledGroup(!allowPainting);
            maskTexture = EditorGUILayout.ObjectField(maskTexture, typeof(Texture2D), false) as Texture2D;
            MaskTexName = EditorGUILayout.TextField("Mask Tex Name:", MaskTexName); GUILayout.Space(10);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            EditorGUI.BeginDisabledGroup(!allowPaintingLightmap);
            GUILayout.Space(4);
            lightMap = EditorGUILayout.ObjectField(lightMap, typeof(Texture2D), false) as Texture2D;
            lightMapPainting = EditorGUILayout.ObjectField(lightMapPainting, typeof(Texture2D), false) as Texture2D;
            lightMapFinal = EditorGUILayout.ObjectField(lightMapFinal, typeof(RenderTexture), false) as RenderTexture;
            blendMat = EditorGUILayout.ObjectField(blendMat, typeof(Material), false) as Material;
            LightMapName = EditorGUILayout.TextField("Light Map Name:", LightMapName);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Blend Mode(混合模式)");
            GUILayout.Label("<color=purple>" + BlendModeNames[blendModeIndex] + "</color>", richStyle);
            blendMode = (BlendMode)EditorGUILayout.EnumPopup(blendMode);
            blendModeIndex = (int)blendMode;
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            layerMask = MaskField("Terrain Layer:", layerMask);
            tag = EditorGUILayout.TagField("Terrain Tag:", tag);

            EditorGUILayout.BeginHorizontal();
            terrainObj = EditorGUILayout.ObjectField("Terrain Object:", terrainObj, typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("Check Terrain"))
            {
                CheckTerrain();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            matIndex = EditorGUILayout.Popup("Materials:", matIndex, matOptions);
            if (EditorGUI.EndChangeCheck())
            {
                maskTexture = renderer.sharedMaterials[matIndex].GetTexture(MaskTexName) as Texture2D;
                terrainMat = renderer.sharedMaterials[matIndex];
            }
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Red(1)", GUILayout.Height(24)))
            {
                PaintMaskColor = new Color(1f, 0f, 0f, 0.1f);
                MaskPreviewProjector.material.SetTexture("_MainTex", splats[0]);
            }
            if (GUILayout.Button("Green(2)", GUILayout.Height(24)))
            {

                PaintMaskColor = new Color(0f, 1f, 0f, 0.1f);
                MaskPreviewProjector.material.SetTexture("_MainTex", splats[1]);
            }
            if (GUILayout.Button("Blue(3)", GUILayout.Height(24)))
            {

                PaintMaskColor = new Color(0f, 0f, 1f, 0.1f);
                MaskPreviewProjector.material.SetTexture("_MainTex", splats[2]);
            }
            if (GUILayout.Button("Black(4)", GUILayout.Height(24)))
            {

                PaintMaskColor = new Color(0f, 0f, 0f, 0.1f);
                MaskPreviewProjector.material.SetTexture("_MainTex", splats[3]);
            }

            EditorGUI.BeginChangeCheck();
            PaintMaskColor = EditorGUILayout.ColorField("", PaintMaskColor, GUILayout.Width(100), GUILayout.Height(24));
            if (EditorGUI.EndChangeCheck())
            {
                brushOpacity = PaintMaskColor.a;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
            int len = brushTexes.Length;
            int colCount = 10;
            int rowCount = len / colCount + (len % colCount == 0 ? 0 : 1);
            //Debug.Log(rowCount);
            int ui_height = rowCount * 32;
            int ui_width = 32 * colCount;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("box", GUILayout.Width(ui_width));
            //selbrush = GUILayout.SelectionGrid(selbrush, brushTexes, (int)win.position.width / 64, "gridlist", GUILayout.Height(128));
            selbrush = GUILayout.SelectionGrid(selbrush, brushTexes, colCount, "gridlist", GUILayout.Height(ui_height), GUILayout.Width(ui_width));
            if (selbrush != oldSelBrush && MaskPreviewProjector != null)
            {
                MaskPreviewProjector.material.SetTexture("_MaskTex", brushTexes[selbrush]);
                oldSelBrush = selbrush;
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            brushSize = EditorGUILayout.Slider("Brush Size", brushSize, MINBRUSHSIZE, MAXBRUSHSIZE);
            brushOpacity = EditorGUILayout.Slider("Brush Opacity", brushOpacity, 0, 1);

            if (brushSize != oldBrushSize && MaskPreviewProjector != null)
            {
                if (curHit.transform != null)
                {
                    Vector2 MeshSize;
                    if (curHit.transform.GetComponent<MeshFilter>() == null) MeshSize = new Vector2(0, 0);
                    else MeshSize = new Vector2(curHit.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.x, curHit.transform.GetComponent<MeshFilter>().sharedMesh.bounds.size.z);
                    MaskPreviewProjector.orthographicSize = (brushSize * curHit.transform.localScale.x) * (MeshSize.x / 200f);// * 4.5f;
                }

                oldBrushSize = brushSize;
            }

            EditorGUI.BeginChangeCheck();
            burshPreviewOpacity = EditorGUILayout.Slider("Brush Preview Opacity", burshPreviewOpacity, 0, 1);
            if (EditorGUI.EndChangeCheck())
            {
                if (MaskPreviewProjector != null) MaskPreviewProjector.material.SetFloat("_Transp", burshPreviewOpacity);
            }
            //brushFalloff = EditorGUILayout.Slider("Brush Falloff", brushFalloff, MINBRUSHSIZE, brushSize);

            //-----------------------LIGHTMAP PAINTING-----------------------
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            allowPainting = GUILayout.Toggle(allowPainting, "Start Painting Mask(P)", GUI.skin.button, GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck())
            {
                allowPaintingLightmap = false;
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save PNG", GUILayout.Height(34)))
            {
                if (maskTexture != null)
                {
                    allowPainting = false;
                    SavePicture(maskTexture, maskTexture, "png");
                }
            }
            if (GUILayout.Button("Save TGA", GUILayout.Height(34)))
            {
                if (maskTexture != null)
                {
                    allowPainting = false;
                    SavePicture(maskTexture, maskTexture, "tga");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            allowPaintingLightmap = GUILayout.Toggle(allowPaintingLightmap, "Start Painting Lightmap(Shift+P)", GUI.skin.button, GUILayout.Height(40));
            if (EditorGUI.EndChangeCheck())
            {
                allowPainting = false;
                if (MaskPreviewProjector != null)
                {
                    MaskPreviewProjector.material.SetTexture("_MainTex", null);
                }
            }

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            if (GUILayout.Button("Save PNG(paint)", GUILayout.Height(16)))
            {
                if (lightMapPainting != null)
                {
                    allowPaintingLightmap = false;
                    SavePicture(lightMapPainting, lightMap, "png");
                }
            }
            if (GUILayout.Button("Save TGA(paint)", GUILayout.Height(16)))
            {
                if (lightMapPainting != null)
                {
                    allowPaintingLightmap = false;
                    SavePicture(lightMapPainting, lightMap, "tga");
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (GUILayout.Button("Save PNG(final)", GUILayout.Height(16)))
            {
                if (lightMapFinal != null)
                {
                    allowPaintingLightmap = false;
                    SavePicture(RtToTexture2D(lightMapFinal), lightMap, "png");
                }
            }
            if (GUILayout.Button("Save TGA(final)", GUILayout.Height(16)))
            {
                if (lightMapFinal != null)
                {
                    allowPaintingLightmap = false;
                    SavePicture(RtToTexture2D(lightMapFinal), lightMap, "tga");
                }
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
         
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.BeginVertical("groupbox");
                GUILayout.Box("Help Info", boxStyle, GUILayout.Height(30), GUILayout.ExpandWidth(true));
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
                infoStyle.richText = true;
                GUILayout.Label("<color=green>打开此窗    </color>快捷键 <color=blue>Ctrl+T</color>", infoStyle);
                GUILayout.Label("<color=green>Painting       </color>快捷键 <color=blue>P</color>", infoStyle);
                GUILayout.Label("<color=green>笔刷大小       </color>快捷键 <color=blue>Ctrl+鼠标左键 左右拖动</color>", infoStyle);
                //GUILayout.Label("<color=green>笔刷渐变       </color>快捷 <color=blue>Shift+鼠标左键 左右拖动</color>", infoStyle);
                GUILayout.Label("<color=green>笔刷透明度    </color>快捷键 <color=blue>Ctrl+Shift+鼠标左键 左右拖动</color>", infoStyle);
                GUILayout.Label("<color=green>红色笔刷       </color>快捷键 <color=blue>1</color>", infoStyle);
                GUILayout.Label("<color=green>绿色笔刷       </color>快捷键 <color=blue>2(由于快捷键冲突，需按两�?)</color>", infoStyle);
                GUILayout.Label("<color=green>蓝色笔刷       </color>快捷键 <color=blue>3</color>", infoStyle);
                GUILayout.Label("<color=green>黑色笔刷       </color>快捷键 <color=blue>4</color>", infoStyle);
                EditorGUILayout.EndScrollView();
                GUILayout.EndVertical();

                GUILayout.BeginVertical("groupbox", GUILayout.Width(200));
                GUILayout.Box("Configure Shader Properties", boxStyle, GUILayout.Height(30));
                if (GUILayout.Button("Shader Properties To Json", GUILayout.Height(20)))
                {
                    DrawJsonWindow = true;
                }
                if (DrawJsonWindow)
                {
                    BeginWindows();
                    Rect windowRect = new Rect(position.width / 2.5f, position.height / 8f, (int)position.width / 1.7f, (int)position.height / 2.0f);
                    GUILayout.Window(354888562, windowRect, SecondWindow, "配置属性名称", GUI.skin.window);
                    EndWindows();
                }
                
                if (GUILayout.Button("Load Json to Properties", GUILayout.Height(20)))
                {
                    inputPath = EditorUtility.OpenFilePanel("Browse", inputPath, Path.GetFileName(inputPath));
                    EditorPrefs.SetString("TA_JSONTOSHADER", inputPath);
                    string[] maps = Read(inputPath);
                    LoadProperties(maps);
                }
                if (IsLoadJson)
                {
                    EditorGUILayout.LabelField("Loaded New Shader " + Path.GetFileName(inputPath));
                    if (GUILayout.Button("Delete"))
                    {
                        MaskTexName = "_Control";
                        LightMapName = "_LightMap";
                        SplatName = new string[4] { "_Splat0", "_Splat1", "_Splat2", "_Splat3" };
                        IsLoadJson = false;
                    }
                }

                GUILayout.EndVertical();
            }
        }
    }
    void OnSceneGUI(SceneView sceneView)
    {
        SceneView.RepaintAll();
        if (win == null)
        {
            win = EditorWindow.GetWindow<PaintTerrainTool>(/*false, "Painting Terrain Mask", true*/);
            win.GenerateStyles();
        }

        if (allowPaintingLightmap)
        {
            PaintColor = ChangeColorA(FWColor, brushOpacity);
            FWColor = PaintColor;
        }
        else
        if (allowPainting)
        {

            PaintColor = ChangeColorA(PaintMaskColor, brushOpacity);

            PaintMaskColor = PaintColor;
        }
        ProcessInputs(curHit);
        if (allowPainting || allowPaintingLightmap)
        {
            Selection.activeObject = null;
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(10, 10, 160, 30));
            GUILayout.BeginHorizontal("box");

            EditorGUI.BeginChangeCheck();
            PaintMaskColor = EditorGUILayout.ColorField(PaintMaskColor, GUILayout.Height(24));
            if (EditorGUI.EndChangeCheck())
            {
                brushOpacity = PaintMaskColor.a;
            }
            if (GUILayout.Button("R", GUILayout.Height(24))) { PaintMaskColor = Color.red; MaskPreviewProjector.material.SetTexture("_MainTex", splats[0]); }
            if (GUILayout.Button("G", GUILayout.Height(24))) { PaintMaskColor = Color.green; MaskPreviewProjector.material.SetTexture("_MainTex", splats[1]); }
            if (GUILayout.Button("B", GUILayout.Height(24))) { PaintMaskColor = Color.blue; MaskPreviewProjector.material.SetTexture("_MainTex", splats[2]); }
            if (GUILayout.Button("K", GUILayout.Height(24))) { PaintMaskColor = Color.black; MaskPreviewProjector.material.SetTexture("_MainTex", splats[3]); }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();


            GUILayout.BeginArea(new Rect(10, 40, 160, 30));
            GUILayout.BeginHorizontal("box");
            FWColor = EditorGUILayout.ColorField(FWColor, GUILayout.Height(24));
            if (GUILayout.Button("X", GUILayout.Height(24)))
            {
                Color tmp = FWColor;
                FWColor = BGColor;
                BGColor = tmp;
            }

            BGColor = EditorGUILayout.ColorField(BGColor, GUILayout.Height(24));
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
            Handles.EndGUI();
            win.Repaint();
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            Event current = Event.current;
            Ray worldRay = HandleUtility.GUIPointToWorldRay(mousePos);
            if (!changingBrushValue)
            {
                if (Physics.Raycast(worldRay, out curHit, 2000f, layerMask))
                {
                    //if (curHit.transform.tag != tag) return;//取消tag 避免鼠标移向物体产生卡顿

                    // SceneView.RepaintAll();
                    if (MaskPreviewProjector == null && curHit.transform.tag == tag) InitPreview(curHit);
                    //if (!isInitTest) InitPreview(curHit);
                    if (MaskPreviewProjector != null)
                        MaskPreviewProjector.transform.position = curHit.point + Vector3.up * 0.5f;

                    //Handles.DrawWireDisc(curHit.point + Vector3.up * 1.0f, curHit.normal, 20.0f);
                    if (renderer != null)
                    {
                        if (terrainMat.HasProperty(SplatName[0]))
                        {
                            splats[0] = terrainMat.GetTexture(SplatName[0]);
                        }
                        if (terrainMat.HasProperty(SplatName[1]))
                        {
                            splats[1] = terrainMat.GetTexture(SplatName[1]);
                        }
                        if (terrainMat.HasProperty(SplatName[2]))
                        {
                            splats[2] = terrainMat.GetTexture(SplatName[2]);
                        }
                        if (terrainMat.HasProperty(SplatName[3]))
                        {
                            splats[3] = terrainMat.GetTexture(SplatName[3]);
                        }

                    }
                    //Debug.Log(LayerMask.LayerToName(curHit.transform.gameObject.layer));
                    //Debug.Log(LayerMask.LayerToName(layerMask));
                    int controlID = GUIUtility.GetControlID(sceneView.GetHashCode(), FocusType.Passive);
                    var eventType = current.GetTypeForControl(controlID);
                    switch (eventType)
                    {
                        case EventType.Layout:
                            {
                                if (!isPainting) return;
                                break;
                            }
                        case EventType.MouseDown:
                        case EventType.MouseDrag:
                            {
                                if (!current.alt && isPainting)
                                {
                                    if (curHit.transform.tag == tag)
                                    {

                                        var registUndo = !RegistUndoOnMouseDownOnly || eventType == EventType.MouseDown;
                                        if (registUndo)
                                        {
                                            if (allowPainting)
                                            {
                                                if (curHit.transform.GetComponent<MeshRenderer>() == null || curHit.transform.GetComponent<MeshFilter>().sharedMesh == null || !curHit.transform.GetComponent<MeshRenderer>().enabled)
                                                    EditorUtility.DisplayDialog("提示", "所画物体缺少mesh,请检查MeshRenderer或者MeshFilter", "确认");
                                                else
                                                {
                                                    try
                                                    {
                                                        if (maskTexture != null)
                                                            Undo.RegisterCompleteObjectUndo(maskTexture, "Painting...");

                                                        if (IDTexture != null)
                                                            Undo.RegisterCompleteObjectUndo(IDTexture, "Painting...");
                                                    }
                                                    catch (System.Exception e)
                                                    {
                                                        Debug.LogException(e);
                                                    }
                                                }
                                            }
                                        }
                                        Paint(curHit, PaintColor, brushSize, brushFalloff);
                                        //Debug.Log("Paintting...");
                                    }

                                }
                                break;
                            }
                    }
                }
            }
        }


        sceneView.Repaint();
    }
    #endregion

    #region Custom 方法  
    public void SaveTexture2DToPIC(Texture2D tex, string save_file_name, string type)
    {
        byte[] bytes;
        if (type == "tga")
            bytes = tex.EncodeToTGA();
        else
            bytes = tex.EncodeToPNG();

        string directory = Path.GetDirectoryName(save_file_name);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        File.WriteAllBytes(save_file_name, bytes);
    }
    void SavePicture(Texture2D ortTexture, Texture2D destTextureForGetPath, string formatname)
    {
        string orifilefullpath = AssetDatabase.GetAssetPath(destTextureForGetPath);
        string orifilefullpathwithoutname = Path.GetDirectoryName(orifilefullpath);
        string save_file_name = EditorUtility.SaveFilePanel("Save Texture...", orifilefullpathwithoutname, ortTexture.name, formatname);
        //string save_file_name = EditorUtility.SaveFilePanelInProject("Save Texture...", maskTexture.name, "png", "png");
        SaveTexture2DToPIC(ortTexture, save_file_name, formatname.ToLower());
        //AssetDatabase.ImportAsset(save_file_name);
        AssetDatabase.Refresh();
    }
    Texture2D RtToTexture2D(RenderTexture rt)
    {
        int width = rt.width;
        int height = rt.height;
        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        return texture;
    }

    Texture[] InitBrush()
    {
        ArrayList brushArrayList = new ArrayList();
        Texture brushTex;
        int brushesCount = 0;
        do
        {
            brushTex = AssetDatabase.LoadAssetAtPath(BRUSHESPATH + "Brush" + brushesCount + ".png", typeof(Texture)) as Texture;
            if (brushTex) brushArrayList.Add(brushTex);
            brushesCount++;
        } while (brushTex);
        return brushArrayList.ToArray(typeof(Texture)) as Texture[];
    }
    static int MaskField(string label, int layermask)
    {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();
        for (int i = 0; i < 32; i++)
        {
            string layername = LayerMask.LayerToName(i);
            if (layername != null)
            {
                layers.Add(layername);
                layerNumbers.Add(i);
            }
        }
        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if (((1 << layerNumbers[i]) & layermask) > 0)
            {
                maskWithoutEmpty |= (1 << i);
            }
        }
        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
            {
                mask |= (1 << layerNumbers[i]);
            }
        }
        return mask;
    }

    void Paint(RaycastHit curHit, Color color, float brushsize, float brushfalloff)
    {
        renderer = curHit.transform.GetComponent<Renderer>();
        if (renderer == null) return;
        meshCollider = curHit.collider as MeshCollider;

        //MeshFilter mf = curHit.transform.GetComponent<MeshFilter>();

        if (maskTexture == null)
        {
            maskTexture = renderer.sharedMaterials[0].GetTexture(MaskTexName) as Texture2D;
            terrainMat = renderer.sharedMaterials[0];
            matOptions = new string[renderer.sharedMaterials.Length];
            int i = 0;
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat.HasProperty(MaskTexName))
                {
                    matOptions[i] = mat.name;
                    i++;
                }
            }

        }
        else
        {
            maskTexture = renderer.sharedMaterials[0].GetTexture(MaskTexName) as Texture2D;//每次更新贴图 以检测贴图名称
        }

        if (lightMap == null)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat.HasProperty(LightMapName))
                {
                    lightMap = mat.GetTexture(LightMapName) as Texture2D;
                    if (lightMap == null)
                    {
                        EditorUtility.DisplayDialog("提示", "缺少lightMap贴图", "确认"); return;
                    }
                    if (lightMapPainting == null)
                    {
                        Color[] cols = new Color[lightMap.width * lightMap.height];
                        for (int i = 0; i < lightMap.height; i++)
                        {
                            for (int j = 0; j < lightMap.width; j++)
                            {
                                int index = (i * lightMap.width) + j;
                                cols[index] = BlendModeBaseColor[blendModeIndex];
                            }
                        }

                        lightMapPainting = new Texture2D(lightMap.width, lightMap.height);
                        lightMapPainting.SetPixels(cols);
                        lightMapPainting.name = "lightMapPainting";
                        InitBlendMat();
                    }
                    if (lightMapFinal == null)
                    {
                        lightMapFinal = new RenderTexture(lightMap.width, lightMap.height, 24, RenderTextureFormat.ARGB32);
                        lightMapFinal.enableRandomWrite = true;
                        lightMapFinal.Create();
                        lightMapFinal.name = "lightMapFinal";
                        Graphics.Blit(lightMapPainting, lightMapFinal, blendMat);
                    }

                    //break;
                }
            }
        }

        if (renderer == null || renderer.sharedMaterial == null || /*maskTexture == null ||*/ meshCollider == null)
            return;

        if (maskTexture == null)
            return;

        /*if (!maskTexture.name.Contains("_RW"))
        {
            EditorUtility.DisplayDialog("提示", "该mask贴图不可读", "确认");
            return;
        }
        if (!maskTexture.name.Contains("_UC"))
        {
            EditorUtility.DisplayDialog("提示", "该mask贴图未压缩", "确认");
            return;
        }*/

        if (allowPainting)
            CalculatePixels(curHit, maskTexture, IDTexture);

        else if (allowPaintingLightmap)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat.mainTexture != lightMapFinal)
                    mat.SetTexture(LightMapName, lightMapFinal);
            }
            CalculatePixels(curHit, lightMapPainting);
            Graphics.Blit(lightMapPainting, lightMapFinal, blendMat);

        }
        //Debug.Log("hit point:--> " + curHit.point);

        //float sizeX = mf.sharedMesh.bounds.size.x;
        //float sizeZ = mf.sharedMesh.bounds.size.z;

        //Debug.Log("painting");

    }
    void InitBlendMat()
    {
        if (blendMat == null)
        {
            blendMat = new Material(Shader.Find(BLENDMODESHADERPATH));
            blendMat.SetTexture("_destination", lightMap);
            blendMat.SetInt("number", blendModeIndex);
            blendMat.SetTexture("_MainTex", lightMapPainting);
        }
    }

    public static Color ChangeColorA(Color c, float a) { return new Color(c.r, c.g, c.b, a); }
    void CalculatePixels(RaycastHit curHit, Texture2D lightmapTex)
    {
        Vector2 pixelUV = curHit.textureCoord;
        int brushsizePercent = (int)(Mathf.Round(brushSize * lightmapTex.width) / 100.0f);

        //计算笔刷所覆盖的区域
        int PuX = Mathf.FloorToInt(pixelUV.x * lightmapTex.width);
        int PuY = Mathf.FloorToInt(pixelUV.y * lightmapTex.height);
        int x = Mathf.Clamp(PuX - brushsizePercent / 2, 0, lightmapTex.width - 1);
        int y = Mathf.Clamp(PuY - brushsizePercent / 2, 0, lightmapTex.height - 1);
        int width = Mathf.Clamp((PuX + brushsizePercent / 2), 0, lightmapTex.width) - x;
        int height = Mathf.Clamp((PuY + brushsizePercent / 2), 0, lightmapTex.height) - y;

        Color[] terrainBay = lightmapTex.GetPixels(x, y, width, height, 0);//获取Control贴图被笔刷所覆盖的区域的颜色

        Texture2D TBrush = brushTexes[selbrush] as Texture2D;//获取笔刷性状贴图
        float[] brushAlpha = new float[brushsizePercent * brushsizePercent];//笔刷透明度

        //根据笔刷贴图计算笔刷的透明度
        for (int i = 0; i < brushsizePercent; i++)
        {
            for (int j = 0; j < brushsizePercent; j++)
            {
                brushAlpha[j * brushsizePercent + i] = TBrush.GetPixelBilinear(((float)i) / brushsizePercent, ((float)j) / brushsizePercent).a;
            }
        }

        //计算绘制后的颜色
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int index = (i * width) + j;
                float opacity = brushAlpha[Mathf.Clamp((y + i) - (PuY - brushsizePercent / 2), 0, brushsizePercent - 1) * brushsizePercent + Mathf.Clamp((x + j) - (PuX - brushsizePercent / 2), 0, brushsizePercent - 1)] * brushOpacity;

                terrainBay[index] = Color.Lerp(terrainBay[index], PaintColor, opacity);
            }
        }
        Undo.RegisterCompleteObjectUndo(lightmapTex, "meshPaint");//保存历史记录以便撤销

        lightmapTex.SetPixels(x, y, width, height, terrainBay, 0);//把绘制后的Control贴图保存起来
        lightmapTex.Apply();
    }

    void CalculatePixels(RaycastHit curHit, Texture2D maskTex, Texture2D idTex)
    {
        //Vector3 scale = curHit.transform.lossyScale;
        //float aspect = scale.z / scale.x;

        pixelUV = curHit.textureCoord;
        //Debug.Log("hit pixelUV--> : " + pixelUV);
        int brushsizePercent = (int)(Mathf.Round(brushSize * maskTex.width) / 100.0f);

        //Painting Main...........
        int overlayX = Mathf.FloorToInt(pixelUV.x * maskTex.width);
        int overlayY = Mathf.FloorToInt(pixelUV.y * maskTex.height);

        int x = Mathf.Clamp(overlayX - brushsizePercent / 2, 0, maskTex.width - 1);
        int y = Mathf.Clamp(overlayY - brushsizePercent / 2, 0, maskTex.height - 1);

        var checkBlockStep = 0;
        int paddingX = Mathf.Min(checkBlockStep, x);
        int paddingY = Mathf.Min(checkBlockStep, y);

        int width = Mathf.Clamp(overlayX + brushsizePercent / 2, 0, maskTex.width) - x;
        int height = Mathf.Clamp(overlayY + brushsizePercent / 2, 0, maskTex.height) - y;

        int paddingW = Mathf.Min(checkBlockStep, maskTex.width - x - width);
        int paddingH = Mathf.Min(checkBlockStep, maskTex.height - y - height);

        var grabX = x - paddingX;
        var grabY = y - paddingY;
        var grabW = width + paddingW;
        var grabH = height + paddingH;
        Color[] maskBlock = maskTex.GetPixels(grabX, grabY, grabW, grabH, 0);
        Texture2D TBrush = (Texture2D)brushTexes[selbrush];

        float[] brushAlpha = new float[brushsizePercent * brushsizePercent];



        //Debug.Log("@@ 2.3 CalculatePixels GetPixels=" + sw.ElapsedMilliseconds);
        //sw.Reset();
        //sw.Start();

        //计算笔刷透明�?
        for (int i = 0; i < brushsizePercent; i++)
        {
            for (int j = 0; j < brushsizePercent; j++)
            {
                brushAlpha[j * brushsizePercent + i] = TBrush.GetPixelBilinear(((float)i) / brushsizePercent, ((float)j) / brushsizePercent).a;
            }
        }

        //计算绘制后的颜色
        float opacityMin = /*0.01f*/0.001f;
        int idLayer = tileSpriteSelectID / 4;
        float idColor = (tileSpriteSelectID + 0.5f) / 16.0f;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int index = ((i + paddingY) * grabW) + j + paddingX;
                float opacity = brushAlpha[Mathf.Clamp((y + i) - (overlayY - brushsizePercent / 2), 0, brushsizePercent - 1) * brushsizePercent
                    + Mathf.Clamp((x + j) - (overlayX - brushsizePercent / 2), 0, brushsizePercent - 1)] * brushOpacity;

                /////
                float tmpOpacity = 1.0f;
                bool isValid = true;
                bool isEdge = false;


                if (opacity >= opacityMin)
                {
                    maskBlock[index] = Color.Lerp(maskBlock[index], PaintColor, opacity);

                    //if (idLayer == 0)
                    //{
                    //    float tmpMin = Mathf.Min(maskBlock[index].r, tmpOpacity);
                    //    float tmpGap = maskBlock[index].r - tmpMin;
                    //    float tmpSum = maskBlock[index].g + maskBlock[index].b + maskBlock[index].a;
                    //    maskBlock[index].r = tmpMin;
                    //    maskBlock[index].g = maskBlock[index].g + maskBlock[index].g / tmpSum * tmpGap;
                    //    maskBlock[index].b = maskBlock[index].b + maskBlock[index].b / tmpSum * tmpGap;
                    //    maskBlock[index].a = maskBlock[index].a + maskBlock[index].a / tmpSum * tmpGap;
                    //}
                }

            }
        }
        maskTex.SetPixels(grabX, grabY, grabW, grabH, maskBlock, 0);
        maskTex.Apply();
    }
    void SetTexutreByPropertyName(Renderer renderer, string PropertyName, Texture tex)
    {
        foreach (Material mat in renderer.sharedMaterials)
        {
            if (mat.HasProperty(PropertyName))
            {
                mat.SetTexture(PropertyName, tex);
                //break;
            }
        }
    }

    void InitPreview(RaycastHit curHit)
    {
        var ProjectorMask = new GameObject("Preview Mask");
        //ProjectorMask.AddComponent<NotBuildInVersionObject>();
        MaskPreviewProjector = ProjectorMask.AddComponent<Projector>();
        //ProjectorMask.hideFlags = HideFlags.HideInHierarchy;

        MeshFilter mf = curHit.transform.GetComponent<MeshFilter>();

        Vector2 MeshSize;
        if (mf == null) MeshSize = new Vector2(0, 0);
        else MeshSize = new Vector2(mf.sharedMesh.bounds.size.x, mf.sharedMesh.bounds.size.z);
        MaskPreviewProjector.nearClipPlane = 0.001f;
        MaskPreviewProjector.farClipPlane = 1000f;
        MaskPreviewProjector.orthographic = true;
        MaskPreviewProjector.orthographicSize = (brushSize * curHit.transform.localScale.x) * (MeshSize.x / 200f);
        MaskPreviewProjector.ignoreLayers = ~layerMask;
        MaskPreviewProjector.transform.Rotate(90, 0, 180);
        Material previewMat = new Material(Shader.Find(BURSHPREVIEWSHADERPATH));
        MaskPreviewProjector.material = previewMat;
        MaskPreviewProjector.material.SetTexture("_MaskTex", brushTexes[selbrush]);

        //isInitTest = true;
    }

    void ProcessInputs(RaycastHit curHit)
    {
        Event e = Event.current;
        mousePos = e.mousePosition;
        //KeyDown
        if (e.type == EventType.KeyDown)
        {
            if (e.isKey)
            {
                if (e.keyCode == KeyCode.P && !e.shift)
                {
                    allowPainting = !allowPainting;
                    if (!allowPainting)
                    {
                        Tools.current = curTool;
                    }
                    else
                    {
                        curTool = Tools.current;
                        Tools.current = Tool.None;
                    }
                    win.Repaint();
                }
                else if (e.shift && e.keyCode == KeyCode.P)
                {
                    allowPaintingLightmap = !allowPaintingLightmap;
                    if (!allowPaintingLightmap)
                    {
                        Tools.current = curTool;
                    }
                    else
                    {
                        curTool = Tools.current;
                        Tools.current = Tool.None;
                    }
                    win.Repaint();
                }
                else if (e.keyCode == KeyCode.X)
                {
                    Color tmp = FWColor;
                    FWColor = BGColor;
                    BGColor = tmp;
                }
            }

        }

        if (allowPainting || allowPaintingLightmap)
        {
            if (e.type == EventType.MouseDrag && e.control && e.button == 0 && !e.shift)
            {
                brushSize += e.delta.x * 0.005f;
                brushSize = Mathf.Clamp(brushSize, MINBRUSHSIZE, MAXBRUSHSIZE);
                //brushFalloff = Mathf.Min(brushFalloff, brushSize);
                changingBrushValue = true;
            }
            else if (e.type == EventType.MouseDrag && !e.control && e.button == 0 && e.shift)
            {

                changingBrushValue = true;
            }
            else if (e.type == EventType.MouseDrag && e.control && e.button == 0 && e.shift)
            {
                brushOpacity += e.delta.x * 0.005f;
                brushOpacity = Mathf.Clamp01(brushOpacity);
                //PaintColor = PaintColor.ChangeColorA(brushOpacity);
                changingBrushValue = true;
            }
            else if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && !e.control && !e.shift && !e.alt)
            {
                isPainting = true;
            }
            else if (e.type == EventType.MouseUp)
            {
                isPainting = false;
                changingBrushValue = false;
            }

            //Two ways to changing brush
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Equals || e.keyCode == KeyCode.KeypadPlus)
                {
                    if (brushSize > 10f) brushSize += 1f;
                    else if (brushSize > 1f) brushSize += 0.1f;
                    else if (brushSize > 0.1f) brushSize += 0.01f;
                    else if (brushSize > 0 && brushSize <= 0.1f) brushSize += 0.005f;
                    brushSize = Mathf.Min(brushSize, MAXBRUSHSIZE);
                    brushSize = Mathf.Max(brushSize, MINBRUSHSIZE);
                    changingBrushValue = true;
                }
                else if (e.keyCode == KeyCode.Minus || e.keyCode == KeyCode.KeypadMinus)
                {
                    if (brushSize > 10f) brushSize -= 1f;
                    else if (brushSize > 1f) brushSize -= 0.1f;
                    else if (brushSize > 0.1f) brushSize -= 0.01f;
                    else if (brushSize > 0 && brushSize <= 0.1f) brushSize -= 0.005f;
                    brushSize = Mathf.Min(brushSize, MAXBRUSHSIZE);
                    brushSize = Mathf.Max(MINBRUSHSIZE, brushSize);
                    changingBrushValue = true;
                }
                else if (e.keyCode == KeyCode.RightBracket)
                {

                    changingBrushValue = true;
                }
                else if (e.keyCode == KeyCode.LeftBracket)
                {

                    changingBrushValue = true;
                }

                else if (e.keyCode == KeyCode.Comma)
                {
                    brushOpacity += 0.05f;
                    brushOpacity = Mathf.Clamp01(brushOpacity);
                    //PaintColor = PaintColor.ChangeColorA(brushOpacity);
                    changingBrushValue = true;
                }
                else if (e.keyCode == KeyCode.Period)
                {
                    brushOpacity -= 0.05f;
                    brushOpacity = Mathf.Clamp01(brushOpacity);
                    //PaintColor = PaintColor.ChangeColorA(brushOpacity);
                    changingBrushValue = true;
                }
                else if (e.keyCode == KeyCode.Alpha1 || e.keyCode == KeyCode.Keypad1)
                {
                    if (allowPainting)
                    {
                        PaintMaskColor = new Color(1f, 0f, 0f, 0.1f);

                        MaskPreviewProjector.material.SetTexture("_MainTex", splats[0]);
                        //brushOpacity = 0.0234375f;
                    }
                }
                else if (e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.Keypad2)
                {
                    if (allowPainting)
                    {
                        PaintMaskColor = new Color(0f, 1f, 0f, 0.1f);
                        MaskPreviewProjector.material.SetTexture("_MainTex", splats[1]);
                        //brushOpacity = 0.0234375f;
                        Event.current.Use();
                    }
                }
                else if (e.keyCode == KeyCode.Alpha3 || e.keyCode == KeyCode.Keypad3)
                {
                    if (allowPainting)
                    {
                        PaintMaskColor = new Color(0f, 0f, 1f, 0.1f);
                        MaskPreviewProjector.material.SetTexture("_MainTex", splats[2]);
                        //brushOpacity = 0.0234375f;
                    }
                }
                else if (e.keyCode == KeyCode.Alpha4 || e.keyCode == KeyCode.Keypad4)
                {
                    if (allowPainting)
                    {
                        PaintMaskColor = new Color(0f, 0f, 0f, 0.1f);
                        MaskPreviewProjector.material.SetTexture("_MainTex", splats[3]);
                        //brushOpacity = 0.0234375f;
                    }
                }
            }
            else if (e.type == EventType.KeyUp)
            {
                changingBrushValue = false;

                //SceneView.lastActiveSceneView.in2DMode = false;
            }
        }
    }
    void GenerateStyles()
    {
        infoStyle = new GUIStyle();

        boxStyle = new GUIStyle();
        boxStyle.margin = new RectOffset(0, 0, 0, 0);
        boxStyle.normal.textColor = Color.black;
        boxStyle.fontSize = 16;
        boxStyle.fontStyle = FontStyle.Bold;
        boxStyle.alignment = TextAnchor.MiddleCenter;

        richStyle = new GUIStyle();
        richStyle.fontStyle = FontStyle.Bold;
        richStyle.richText = true;
    }
    private void SecondWindow(int windowID)
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            replaceMat = EditorGUILayout.ObjectField("要替换的材质：", replaceMat, typeof(Material), false) as Material;
            if (replaceMat != null)
            {
                SerializedObject so = new SerializedObject(replaceMat);
                var properties = so.FindProperty("m_SavedProperties.m_TexEnvs");
                if (properties != null && properties.isArray)
                {
                    EditorGUILayout.LabelField("贴图列表：");
                    scrollPosJ = EditorGUILayout.BeginScrollView(scrollPosJ, GUILayout.Height(150));
                    for (int i = 0; i < properties.arraySize; i++)
                    {
                        var propName = properties.GetArrayElementAtIndex(i).FindPropertyRelative("first").stringValue;
                        using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField(propName, GUILayout.Width(100));
                            EditorGUILayout.ObjectField(replaceMat.GetTexture(propName), typeof(Texture2D), false);
                            if (GUILayout.Button("Copy", GUILayout.Width(50)))
                            {
                                GUIUtility.systemCopyBuffer = propName;
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
        }
        GUILayout.Space(8);
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Shader中对应的贴图属性名");
            string[] labelName = { "Mask:", "LightMap:", "Splat0:", "Splat1:", "Splat2:", "Splat3:" };
            for (int i = 0; i < 6; i++)
            {
                using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    toJsonMaps[i] = EditorGUILayout.TextField(labelName[i], toJsonMaps[i]);
                    if (GUILayout.Button("Paste", GUILayout.Width(50)))
                    {
                        toJsonMaps[i] = GUIUtility.systemCopyBuffer;
                    }
                }
            }
        }
        if (GUILayout.Button("Load", GUILayout.Height(20)))
        {
            LoadProperties(toJsonMaps);
        }
        GUILayout.Space(8);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("保存为Json文件");
            using (new EditorGUILayout.HorizontalScope())
            {              
                GUILayout.Label("Save Path:");
                outputPath = EditorGUILayout.TextField(EditorPrefs.GetString("SAVETOPICTUREPATH"));
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    outputPath = EditorUtility.OpenFolderPanel("Browse", outputPath, Path.GetFileName(outputPath));
                    EditorPrefs.SetString("SAVETOPICTUREPATH", outputPath);
                }

            }
            fileName = EditorGUILayout.TextField("FileName:", fileName);
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save to Json", GUILayout.Height(20)))
            {
                if (outputPath == "" || fileName == "") ShowNotification(new GUIContent("请填写完整"));
                else
                {
                    string finalPath = outputPath + "/" + fileName + ".json";
                    Write(toJsonMaps, finalPath);
                }
            }
            if (GUILayout.Button("Open Folder", GUILayout.Height(20)))
                System.Diagnostics.Process.Start("explorer.exe", outputPath.Replace("/", "\\"));
        }
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Close", GUILayout.Height(30))) { DrawJsonWindow = false; }
        GUI.DragWindow();

    }

    #region json方法
    [System.Serializable]
    public class Item
    {
        public string maskMap;
        public string lightMap;
        public string splat0;
        public string splat1;
        public string splat2;
        public string splat3;
    }
    string[] Read(string finalPath)
    {
        if (!File.Exists(finalPath)) return null;
        StreamReader streamReader = new StreamReader(finalPath);
        string str = streamReader.ReadToEnd();
        Item jsonItem = JsonUtility.FromJson<Item>(str);
        string[] maps = {
                    jsonItem.maskMap,
                    jsonItem.lightMap,
                    jsonItem.splat0,
                    jsonItem.splat1,
                    jsonItem.splat2,
                    jsonItem.splat3
                };
        return maps;

    }
    void Write(string[] jsonMaps, string finalPath)
    {

        if (Directory.Exists(Path.GetDirectoryName(finalPath)) == false)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath));
        }

        FileStream fs = null;
        using (fs = new FileStream(finalPath, FileMode.Create))
        {
            StreamWriter sw = new StreamWriter(fs);
            Item jsonItem = new Item();
            jsonItem.maskMap = jsonMaps[0];
            jsonItem.lightMap = jsonMaps[1];
            jsonItem.splat0 = jsonMaps[2];
            jsonItem.splat1 = jsonMaps[3];
            jsonItem.splat2 = jsonMaps[4];
            jsonItem.splat3 = jsonMaps[5];

            string jsonStr = JsonUtility.ToJson(jsonItem);
            sw.Write(jsonStr);
            ShowNotification(new GUIContent("导出成功！"));
            sw.Flush();
            sw.Close();
            fs.Close();
        }

    }
    void LoadProperties(string[] maps)
    {
        MaskTexName = maps[0];
        LightMapName = maps[1];
        SplatName[0] = maps[2];
        SplatName[1] = maps[3];
        SplatName[2] = maps[4];
        SplatName[3] = maps[5];
        maskTexture = null;

        IsLoadJson = true;
        ShowNotification(new GUIContent("导入成功！"));
    }
    #endregion

    #region Terrain一键检测方法
    bool isHasTag(string tag)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i].Equals(tag))
                return true;
        }
        return false;
    }
    static bool isHasLayer(string layer)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.layers.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.layers[i].Contains(layer))
                return true;
        }
        return false;
    }
    void CheckTerrain()
    {
        if (isHasTag(tag))
            terrainObj.transform.tag = tag;
        else
        {
            EditorUtility.DisplayDialog("提示", "请添加一个名为“" + tag + "”的tag标签", "确认");
            return;
        }

        if (isHasLayer(DEFAULTLAYER))
            terrainObj.layer = LayerMask.NameToLayer(DEFAULTLAYER);
        else
        {
            EditorUtility.DisplayDialog("提示", "请添加一个名为“" + DEFAULTLAYER + "”的layer层", "确认");
            return;
        }
        if (terrainObj.GetComponent<MeshCollider>() == null)
        {
            terrainObj.AddComponent<MeshCollider>();
        }
        Material mat = terrainObj.transform.GetComponent<Renderer>().sharedMaterials[0];
        Texture2D maskTex = mat.GetTexture(MaskTexName) as Texture2D;
        TextureImporter imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(maskTex)) as TextureImporter;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.isReadable = true;
        /*if (!maskTex.name.Contains("_RW") || maskTex.isReadable == false)
        {
            EditorUtility.DisplayDialog("提示", "该物体的mask贴图不可读，请手动修改", "确认");
            return;
        }
        if (!maskTex.name.Contains("_UC"))
        {
            EditorUtility.DisplayDialog("提示", "该物体的mask贴图未压缩，请手动修改", "确认");
            return;
        }*/
        Texture2D lightMap = mat.GetTexture(LightMapName) as Texture2D;
        if (lightMap == null)
        {
            EditorUtility.DisplayDialog("提示", "该物体缺少lightMap贴图，请手动添加", "确认"); return;
        }
        EditorUtility.DisplayDialog("提示", "该地形完成检查和设置！可以开始作画~", "确认");
    }
    #endregion

    #endregion
}
