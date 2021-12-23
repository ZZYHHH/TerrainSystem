using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;


public class Mesh2Height : EditorWindow
{

    [MenuItem("Tools/Mesh To HeightMap", false, 2000)]
    static void OpenWindow()
    {

        EditorWindow.GetWindow<Mesh2Height>(true);
    }

    private int resolution = 512;
    private Vector3 addTerrain;
    int bottomTopRadioSelected = 0;
    static string[] bottomTopRadio = new string[] { "Bottom Up", "Top Down" };
    private float shiftHeight = 0f;
    string path;
    Texture2D tex;

    void OnGUI()
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
        resolution = EditorGUILayout.IntField("HeightMap Size：", resolution);
        shiftHeight = EditorGUILayout.Slider("Shift height", shiftHeight, -1f, 1f);
        bottomTopRadioSelected = GUILayout.SelectionGrid(bottomTopRadioSelected, bottomTopRadio,
                    bottomTopRadio.Length, EditorStyles.radioButton);
        if (GUILayout.Button("Convert Mesh To HeightMap"))
        {
            if (Selection.activeGameObject == null)
            {

                EditorUtility.DisplayDialog("No object selected", "Please select an object.", "Ok");
                return;
            }

            else
            {

                CreateHeightMap();
            }
        }

    }

    delegate void CleanUp();

    void CreateHeightMap()
    {
        Collider collider;
        collider = Selection.activeGameObject.GetComponent<MeshCollider>();

        CleanUp cleanUp = null;

        if (!collider)
        {
            collider = Selection.activeGameObject.AddComponent<MeshCollider>();
            cleanUp = () => DestroyImmediate(collider);
        }

        Bounds bounds = collider.bounds;

        float sizeFactor = collider.bounds.size.y / (collider.bounds.size.y + addTerrain.y);

        float[,] heights = new float[resolution, resolution];
        Ray ray = new Ray(new Vector3(bounds.min.x, bounds.max.y + bounds.size.y, bounds.min.z), -Vector3.up);
        RaycastHit hit;
        float meshHeightInverse = 1 / bounds.size.y; //to keep height range from 0 to 1
        Vector3 rayOrigin = ray.origin;

        int maxHeight = heights.GetLength(0);
        int maxLength = heights.GetLength(1);

        Vector2 stepXZ = new Vector2(bounds.size.x / maxLength, bounds.size.z / maxHeight);

        for (int zCount = 0; zCount < maxHeight; zCount++)
        {
            for (int xCount = 0; xCount < maxLength; xCount++)
            {

                float height = 0.0f;

                if (collider.Raycast(ray, out hit, bounds.size.y * 3))
                {

                    height = (hit.point.y - bounds.min.y) * meshHeightInverse;
                    height += shiftHeight;

                    //bottom up
                    if (bottomTopRadioSelected == 0)
                    {
                        height *= sizeFactor;
                    }

                    //clamp
                    if (height < 0)
                    {

                        height = 0;
                    }
                }

                heights[zCount, xCount] = height;
                rayOrigin.x += stepXZ[0];
                ray.origin = rayOrigin;
            }

            rayOrigin.z += stepXZ[1];
            rayOrigin.x = bounds.min.x;
            ray.origin = rayOrigin;
        }

        SavePic(HeightToPixel(heights));

        EditorUtility.ClearProgressBar();

        if (cleanUp != null)
        {

            cleanUp();
        }
    }

    Texture2D HeightToPixel(float[,] heightMap)
    {
        tex = new Texture2D(heightMap.GetLength(0), heightMap.GetLength(1));
        for (int i = 0; i < heightMap.GetLength(0); i++)
            for (int j = 0; j < heightMap.GetLength(1); j++)
            {
                Color col;
                col.r = heightMap[i, j];
                col.g = heightMap[i, j];
                col.b = heightMap[i, j];
                col.a = 1;
                tex.SetPixel(i, j, col);
            }
        tex.Apply();
        return tex;
    }
    void SavePic(Texture2D heightMap)
    {
        byte[] bytes = heightMap.EncodeToPNG();
        string fileName = Selection.activeGameObject.name;
        string finalPath = path + "/" + fileName + "-heightMap.png";

        FileStream file = File.Open(finalPath, FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
    }

}
