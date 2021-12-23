using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TerrainExportAtlas : EditorWindow
{
    enum RESOLUTIONS
    {
        OneK = 0,
        TwoK = 1,
        FourK = 2
    }

    int _maxTexNum = 0;
    int _secondTexNum = 0;
    int _thirdTexNum = 0;

    float _maxChannelWeight = 0f;
    float _secondChannelWeight = 0f;
    float _thirdChannelWeight = 0f;

    private Terrain _sourceTerrain;

    private int _baseMapResolution = 2048;
    private int BasemapResolution
    {
        get
        {
            switch (_resolution)
            {
                case RESOLUTIONS.OneK:
                    return 1024;
                case RESOLUTIONS.FourK:
                    return 4096;
            }
            return 2048;
        }
    }

    private RESOLUTIONS _resolution = RESOLUTIONS.TwoK;

    const int COLUMN_AND_ROW_COUNT = 4;
    private string path;
    private bool isPadding;
    private int paddingSpace=1;

    [MenuItem("Tools/ExportTerrainMaps", false, 2000)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TerrainExportAtlas));
    }

    void OnGUI()
    {
        GenericMenu menu = new GenericMenu();

        _sourceTerrain = (Terrain)EditorGUILayout.ObjectField("Select Terrain", _sourceTerrain, typeof(Terrain), true);
        GUILayout.Space(10);

        //提供导出按钮
        if (_sourceTerrain != null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Save To:", GUILayout.Width(150));
                path = EditorGUILayout.TextField(EditorPrefs.GetString("SAVETOPICTUREPATH"));
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    path = EditorUtility.OpenFolderPanel("Browse", path, Path.GetFileName(path));
                    EditorPrefs.SetString("SAVETOPICTUREPATH", path);
                }

            }

            //图片导出分辨率
            _resolution = (RESOLUTIONS)EditorGUILayout.EnumPopup("Base Map Resolution", _resolution);

            GUILayout.Space(10);
            using (new EditorGUILayout.HorizontalScope())
            {
                isPadding = EditorGUILayout.Toggle("isPadding:", isPadding);
                if (isPadding)
                    paddingSpace = EditorGUILayout.IntField("paddingSpace:", paddingSpace);
                else paddingSpace = 0;
            }
            
            //导出主贴图按钮
            if (GUILayout.Button("Export Base Map"))
            {
                ExportBasemap();
            }
            GUILayout.Space(10);

            //导出BlendMap & IndexMap按钮
            if (GUILayout.Button("Export Index And Blend Map"))
            {
                ExportIndexAndWeightMap();
                //ExportIndexAndBlendMap();
            }

            if (GUILayout.Button("Export Blend Atlas"))
            {
                ExportBlendAtlas();
            }
        }
        else
        {
            GUILayout.Label("Please select a terrain!");
        }
    }

    void ExportBasemap()
    {
        TerrainData _terrainData = _sourceTerrain.terrainData;
        SplatPrototype[] prototypeArray = _terrainData.splatPrototypes;

        //创建相应数目的RT
        RenderTexture[] rtArray = new RenderTexture[prototypeArray.Length];

        int texSize = BasemapResolution / COLUMN_AND_ROW_COUNT;
        Texture2D[] texArray = new Texture2D[prototypeArray.Length];
        Texture2D[] normalMapArray = new Texture2D[prototypeArray.Length];

        for (int i = 0; i < prototypeArray.Length; i++)
        {
            rtArray[i] = RenderTexture.GetTemporary(texSize, texSize, 24);
            texArray[i] = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);
            normalMapArray[i] = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);
        }
       
       
        //使用一个UnlitShader来将贴图绘制到具体的RenderTexture上
        Shader shader = Shader.Find("Unlit/TerrainAltasUnlitShader");
        Material material = new Material(shader);

        //将这些图读入相应数目的Tex2D中
        for (int i = 0; i < prototypeArray.Length; i++)
        {
            Graphics.Blit(prototypeArray[i].texture, rtArray[i], material, 0);
            RenderTexture.active = rtArray[i];
            texArray[i].ReadPixels(new Rect(0f, 0f, (float)texSize, (float)texSize), 0, 0);
           
            //如果有法线贴图，则将这些值读入
            if (prototypeArray[i].normalMap != null)
            {
                Graphics.Blit(prototypeArray[i].normalMap, rtArray[i], material, 0);
                RenderTexture.active = rtArray[i];
                normalMapArray[i].ReadPixels(new Rect(0f, 0f, (float)texSize, (float)texSize), 0, 0);
            }
        }


        //生成一个目标分辨率的图集
        Texture2D baseTex = new Texture2D(BasemapResolution+paddingSpace*8, BasemapResolution+ paddingSpace * 8, TextureFormat.RGB24, false);
        Texture2D normalTex = new Texture2D(BasemapResolution, BasemapResolution, TextureFormat.RGB24, false);

        for (int i = 0; i < prototypeArray.Length; i++)
        {
            //需要根据图片的序号算出当前贴图在图集中的起始位置
            int columnNum = i % COLUMN_AND_ROW_COUNT;
            int rowNum = (i % (COLUMN_AND_ROW_COUNT * COLUMN_AND_ROW_COUNT)) / COLUMN_AND_ROW_COUNT;
            int startWidth = columnNum * texSize;
            int startHeight = rowNum * texSize;
            
            if (isPadding) 
                AddPadding(texArray[i], baseTex, rowNum, columnNum,texSize, texSize, 1);
            else
                baseTex.SetPixels(startWidth, startHeight, texSize, texSize, texArray[i].GetPixels());
            /*for (int j = 0; j < texSize; j++)
            {
                for (int k = 0; k < texSize; k++)
                {
                   
                    Color color = GetPixelColor(j, k, texArray[i]);
                    baseTex.SetPixel(startWidth + j, startHeight + k, color);

                    Color normalColor = (prototypeArray[i].normalMap == null) ? new Color(0.5f, 0.5f, 1) : GetPixelColor(j, k, normalMapArray[i]);
                    normalTex.SetPixel(startWidth + j, startHeight + k, normalColor);
                }
            }*/
        }

        baseTex.Apply();
        SavePic(baseTex, "BaseMap");

        normalTex.Apply();
        SavePic(normalTex, "NormalMap");

        Debug.Log("BaseMap And NormalMap Exported");
    }

    private void AddPadding(Texture2D src, Texture2D texture, int i, int j, int wid, int hei, int edge)
    {
        texture.SetPixels(j * (wid + 2 * edge) + edge, i * (hei + 2 * edge) + edge, wid, hei, src.GetPixels());

        //加四条边
        var lineColor = src.GetPixels(wid - 1, 0, 1, hei);
        var fillColor = new Color[hei * edge];
        for (int k = 0; k < hei*edge; k++)
        {
            fillColor[k] = lineColor[k % hei];
        }
        texture.SetPixels(j * (wid + 2 * edge), i * (hei + 2 * edge) + edge, edge, hei, fillColor);

        lineColor = src.GetPixels(0,0,1,hei);
        for (int k = 0; k < hei * edge; k++)
        {
            fillColor[k] = lineColor[k % hei];
        }
        texture.SetPixels(j * (wid + 2 * edge)+wid+edge, i * (hei + 2 * edge) + edge, edge, hei, fillColor);

        lineColor = src.GetPixels(0,hei-1,wid,1);
        fillColor = new Color[wid * edge];
        for (int k = 0; k < wid * edge; k++)
        {
            fillColor[k] = lineColor[k % wid];
        }
        texture.SetPixels(j * (wid + 2 * edge)+edge, i * (hei + 2 * edge), wid, edge, fillColor);

        lineColor = src.GetPixels(0, 0, wid, 1);
        for (int k = 0; k < wid * edge; k++)
        {
            fillColor[k] = lineColor[k % wid];
        }
        texture.SetPixels(j * (wid + 2 * edge)  + edge, i * (hei + 2 * edge) + hei+edge, wid, edge, fillColor);

        //4 corner
        var cornerColor = src.GetPixel(0, hei - 1);
        fillColor = new Color[edge * edge];
        for (int k = 0; k < fillColor.Length; k++)
        {
            fillColor[k] = cornerColor;
        }
        texture.SetPixels(j * (wid + 2 * edge), i * (hei + 2 * edge), edge, edge, fillColor);

        cornerColor = src.GetPixel(0, 0);
        for (int k = 0; k < fillColor.Length; k++)
        {
            fillColor[k] = cornerColor;
        }
        texture.SetPixels(j * (wid + 2 * edge), i * (hei + 2 * edge)+hei+edge, edge, edge, fillColor);

        cornerColor = src.GetPixel(0, wid-1);
        for (int k = 0; k < fillColor.Length; k++)
        {
            fillColor[k] = cornerColor;
        }
        texture.SetPixels(j * (wid + 2 * edge) + wid + edge, i * (hei + 2 * edge) , edge, edge, fillColor);

        cornerColor = src.GetPixel(0, 0);
        for (int k = 0; k < fillColor.Length; k++)
        {
            fillColor[k] = cornerColor;
        }
        texture.SetPixels(j * (wid + 2 * edge) + wid + edge, i * (hei + 2 * edge) + hei + edge, edge, edge, fillColor);

    }
    void ExportIndexAndWeightMap()
    {
        TerrainData _terrainData = _sourceTerrain.terrainData;
        SplatPrototype[] prototypeArray = _terrainData.splatPrototypes;
        int _textureNum = prototypeArray.Length;

        //获取混合贴图
        Texture2D[] alphaMapArray = _terrainData.alphamapTextures;
        int witdh = alphaMapArray[0].width;
        int height = alphaMapArray[0].height;

        //新建和混合贴图一样大小的贴图
        Texture2D indexTex = new Texture2D(witdh, height, TextureFormat.RGB24, false, true);
        Color indexColor = new Color(0, 0, 0, 0);

        Texture2D blendTex = new Texture2D(witdh, height, TextureFormat.RGB24, false, true);
        Color blendColor = new Color(0, 0, 0, 0);

        //对每一个像素进行计算
        for (int j = 0; j < witdh; j++)
        {
            for (int k = 0; k < height; k++)
            {
                //默认都是第一个贴图
                //这里支持将三层索引的信息导出，可供后续的Shader使用
                ResetNumAndWeight();

                //遍历所有Control的所有通道，识别出最大的通道所在的贴图序号
                for (int i = 0; i < _textureNum; i++)
                {
                    //根据贴图的序号算出当前应该计算的是哪个值
                    int controlMapNumber = (i % 16) / 4;
                    int controlChannelNum = i % 4;
                    Color color = alphaMapArray[controlMapNumber].GetPixel(j, k);
                    switch (controlChannelNum)
                    {
                        case 0:
                            CalculateIndex(i, color.r);
                            break;
                        case 1:
                            CalculateIndex(i, color.g);
                            break;
                        case 2:
                            CalculateIndex(i, color.b);
                            break;
                        case 3:
                            CalculateIndex(i, color.a);
                            break;
                        default:
                            break;
                    }
                }

                //将识别出来的序号写入IndexMap的通道中
                //需将此值转换到(0, 1)的范围内，因为最多支持16张贴图，而序号是0到15，则除以15即可
                indexColor.r = _maxTexNum / 15f;
                indexColor.g = _secondTexNum / 15f;
                indexColor.b = _thirdTexNum / 15f;
                indexTex.SetPixel(j, k, indexColor);

                //计算Blend因子，将其填入到贴图通道中
                blendColor.r = _maxChannelWeight;
                blendColor.g = _secondChannelWeight;
                blendColor.b = _thirdChannelWeight;
                blendTex.SetPixel(j, k, blendColor);
            }
        }

        indexTex.Apply();
        SavePic(indexTex, "IndexTex");

        blendTex.Apply();
        SavePic(blendTex, "BlendTex");
        EditorUtility.DisplayDialog("Yes", "IndexTex and BlendTex Exported！", "Ok");
    }

    void ExportBlendAtlas()
    {
        TerrainData _terrainData = _sourceTerrain.terrainData;
        SplatPrototype[] prototypeArray = _terrainData.splatPrototypes;

        //获取混合贴图
        Texture2D[] alphaMapArray = _terrainData.alphamapTextures;

        //创建相应数目的RT
        RenderTexture[] rtArray = new RenderTexture[alphaMapArray.Length];

        int texSize = BasemapResolution / 2;
        Texture2D[] texArray = new Texture2D[prototypeArray.Length];

        for (int i = 0; i < alphaMapArray.Length; i++)
        {
            rtArray[i] = RenderTexture.GetTemporary(texSize, texSize, 32);
            texArray[i] = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);        
        }


        //使用一个UnlitShader来将贴图绘制到具体的RenderTexture上
        Shader shader = Shader.Find("Unlit/TerrainAltasUnlitShader");
        Material material = new Material(shader);

        //将这些图读入相应数目的Tex2D中
        for (int i = 0; i < alphaMapArray.Length; i++)
        {
            Graphics.Blit(alphaMapArray[i], rtArray[i], material, 0);
            RenderTexture.active = rtArray[i];
            texArray[i].ReadPixels(new Rect(0f, 0f, (float)texSize, (float)texSize), 0, 0);         
        }
    

        
        Texture2D blendAtlas = new Texture2D(BasemapResolution, BasemapResolution, alphaMapArray[0].format, false, true);
        //blendAtlas.SetPixels(0, 0, blendAtlas.width, blendAtlas.height,new Color[] { Color.black});
        for (int i = 0; i < alphaMapArray.Length; i++)
        {
            blendAtlas.SetPixels((i % 2) * texSize, (i/2) * texSize, texSize, texSize, texArray[i].GetPixels());
        }


        blendAtlas.Apply();
        SavePic(blendAtlas, "BlendAtlas");

        Debug.Log("BlendAtlas Exported");
    }
    void ResetNumAndWeight()
    {
        _maxTexNum = 0;
        _secondTexNum = 0;
        _thirdTexNum = 0;

        _maxChannelWeight = 0f;
        _secondChannelWeight = 0f;
        _thirdChannelWeight = 0f;
    }

    void CalculateIndex(int index, float curWeight)
    {
        //如果比最大的元素大，则取当前为最大，取之前第一为第二，取之前第二的为第三
        if (curWeight > _maxChannelWeight)
        {
            _thirdChannelWeight = _secondChannelWeight;
            _thirdTexNum = _secondTexNum;

            _secondChannelWeight = _maxChannelWeight;
            _secondTexNum = _maxTexNum;

            _maxChannelWeight = curWeight;
            _maxTexNum = index;
        }
        //如果仅是比第二的元素大，则取当前为第二，取之前的第二为第三
        else if (curWeight > _secondChannelWeight)
        {
            _thirdChannelWeight = _secondChannelWeight;
            _thirdTexNum = _secondTexNum;

            _secondChannelWeight = curWeight;
            _secondTexNum = index;
        }
        //如果仅是比第三的元素大，则取当前为第三
        else if (curWeight > _thirdChannelWeight)
        {
            _thirdChannelWeight = curWeight;
            _thirdTexNum = index;
        }
    }

    Color GetPixelColor(int rowNum, int columnNum, Texture2D oriTex)
    {
        Color oriColor;
        int minNum = BasemapResolution / 128 - 1;
        int maxNum = BasemapResolution / COLUMN_AND_ROW_COUNT - minNum + 1;

        //四个角
        if (rowNum <= minNum && columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(minNum, minNum);
        }
        else if (rowNum <= minNum && columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(minNum, maxNum);
        }
        else if (rowNum >= maxNum && columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(maxNum, minNum);
        }
        else if (rowNum >= maxNum && columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(maxNum, maxNum);
        }
        //四条边
        else if (rowNum <= minNum)
        {
            oriColor = oriTex.GetPixel(minNum, columnNum);
        }
        else if (rowNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(maxNum, columnNum);
        }
        else if (columnNum <= minNum)
        {
            oriColor = oriTex.GetPixel(rowNum, minNum);
        }
        else if (columnNum >= maxNum)
        {
            oriColor = oriTex.GetPixel(rowNum, maxNum);
        }

        //正常采样
        else
        {
            oriColor = oriTex.GetPixel(rowNum, columnNum);
        }
        return oriColor;
    }
    void SavePic(Texture2D tex, string fileName)
    {
        byte[] bytes = tex.EncodeToPNG();
        string finalPath = path + "/" + fileName + ".png";

        FileStream file = File.Open(finalPath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
    }
}
