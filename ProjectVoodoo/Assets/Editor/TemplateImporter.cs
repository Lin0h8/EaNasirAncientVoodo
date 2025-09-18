using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class TemplateImporter : EditorWindow
{
    private Texture2D inputTexture;
    private string templateName = "NewTemplate";

    [MenuItem("Tools/Template Importer")]
    public static void ShowWindow()
    {
        GetWindow<TemplateImporter>("Template Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Stroke Template", EditorStyles.boldLabel);
        inputTexture = (Texture2D)EditorGUILayout.ObjectField("Input Texture", inputTexture, typeof(Texture2D), false);
        templateName = EditorGUILayout.TextField("Template Name", templateName);
        if (GUILayout.Button("Import Template"))
        {
            if (inputTexture != null && !string.IsNullOrEmpty(templateName))
            {
                ImportTemplate();
            }
            else
            {
                Debug.LogWarning("Please assign a texture and a template name.");
            }
        }
    }

    private void ImportTemplate()
    {
        Color[] pixels = inputTexture.GetPixels();
        int width = inputTexture.width;
        int height = inputTexture.height;
        List<Vector2> points = new List<Vector2>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = pixels[y * width + x];
                if (pixel.r < 0.1f && pixel.g < 0.1f && pixel.b < 0.1f)
                {
                    points.Add(new Vector2(x, y));
                }
            }
        }
        if (points.Count == 0)
        {
            Debug.LogWarning("No stroke points found in the texture.");
            return;
        }
        StrokeTemplate newTemplate = ScriptableObject.CreateInstance<StrokeTemplate>();
        newTemplate.name = templateName;
        newTemplate.points = points;
        string path = EditorUtility.SaveFilePanelInProject("Save Stroke Template", templateName, "asset", "Please enter a file name to save the stroke template to");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newTemplate, path);
            AssetDatabase.SaveAssets();
            Debug.Log($"Stroke template '{templateName}' imported with {points.Count} points.");
        }
    }
}