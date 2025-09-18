using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StrokeTemplate", menuName = "Scriptable Objects/StrokeTemplate")]
public class StrokeTemplate : ScriptableObject
{
    public string templateName;
    public List<Vector2> points = new List<Vector2>();
}