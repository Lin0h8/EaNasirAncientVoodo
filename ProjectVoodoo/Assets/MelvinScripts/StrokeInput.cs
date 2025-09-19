using System.Collections.Generic;
using UnityEngine;

public class StrokeInput : MonoBehaviour
{
    public Recognizer recognizer;
    private List<Vector2> currentStroke = new List<Vector2>();
    public StrokeTemplate[] templates;
    public SpellCastingManager spellCastingManager;

    private void Awake()
    {
        Debug.Log($"Templates array size: {templates?.Length ?? -1}");
        if (recognizer == null)
        {
            recognizer = new Recognizer();
        }

        foreach (var template in templates)
        {
            recognizer.AddTemplate(template.name, new List<Vector2>(template.points));
            Debug.Log($"Added template: {template.name} with {template.points.Count} points");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            currentStroke.Clear();
        }
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            currentStroke.Add(mousePos);
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (currentStroke.Count > 0)
            {
                (string name, float score) = recognizer.Recognize(currentStroke);
                Debug.Log($"Recognized: {name} with score {score}");
                if (spellCastingManager != null)
                {
                    spellCastingManager.RegisterRune(name);
                }
            }
        }
    }

    private void OnGUI()
    {
        if (currentStroke.Count < 2) return;
        for (int i = 0; i < currentStroke.Count - 1; i++)
        {
            Vector2 start = new Vector2(currentStroke[i].x, Screen.height - currentStroke[i].y);
            Vector2 end = new Vector2(currentStroke[i + 1].x, Screen.height - currentStroke[i + 1].y);
            DrawLine(start, end, Color.red, 2f);
        }
    }

    private void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        Matrix4x4 matrix = GUI.matrix;
        Color savedColor = GUI.color;
        GUI.color = color;
        float angle = Vector3.Angle(pointB - pointA, Vector2.right);
        if (pointA.y > pointB.y) angle = -angle;
        GUIUtility.RotateAroundPivot(angle, pointA);
        GUI.DrawTexture(new Rect(pointA.x, pointA.y - (width / 2), (pointB - pointA).magnitude, width), Texture2D.whiteTexture);
        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
}