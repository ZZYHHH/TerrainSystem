using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateTexture2DArray : EditorWindow
{

    [MenuItem("Tools/CreateTexture2DArray", false, 2000)]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<CreateTexture2DArray>(true);
    }


    public string saveName;
    public string savePath;
    public Texture2D[] ordinaryTextures;
    SerializedObject so;
    SerializedProperty prop_baseTexs;
    Material mat;

    private void OnEnable()
    {
        so = new SerializedObject(this);
        prop_baseTexs = so.FindProperty("ordinaryTextures");
    }
    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label("Save To:", GUILayout.Width(150));
            savePath = EditorGUILayout.TextField(EditorPrefs.GetString("SAVETOPICTUREPATH"));
            if (GUILayout.Button("Browse", GUILayout.Width(100)))
            {
                savePath = EditorUtility.OpenFolderPanel("Browse", savePath, Path.GetFileName(savePath));
                EditorPrefs.SetString("SAVETOPICTUREPATH", savePath);
            }

        }
        saveName= EditorGUILayout.TextField("Name:",saveName);
        so.Update();
        EditorGUILayout.PropertyField(prop_baseTexs, new GUIContent("Texures:"), true);
        so.ApplyModifiedProperties();
        mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material),true);
        if (GUILayout.Button("Save to Texture2D Array"))
        {
            SaveTextureArray();
        }    
    }
    private void SaveTextureArray()
    {
        var output = new Texture2DArray(ordinaryTextures[0].width, ordinaryTextures[0].height, ordinaryTextures.Length, TextureFormat.RGB24, true, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat
        };

        for (var i = 0; i < ordinaryTextures.Length; i++)
        {
            output.SetPixels(ordinaryTextures[i].GetPixels(0), i, 0);
        }

        output.Apply();

        AssetDatabase.CreateAsset(output, "Assets/Textures/"  + saveName + ".asset");
        mat.SetTexture("_TextureArray", output);
    }
}
