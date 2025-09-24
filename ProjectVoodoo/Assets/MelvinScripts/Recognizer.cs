using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

public class Recognizer
{
    //NAMES ARE WEIRD BECAUSE IT IS IMPORTED FROM A C# CONSOLE APPLICATION
    //AND I DIDN'T WANT TO CHANGE IT
    //DEAL WITH IT ONG. (-_-)
    //VECTOR2 WAS POINT BEFORE...

    //SETTINGS
    private const int NumPoints = 128;

    private const float SquareSize = 250.0f;
    //-------------------

    private readonly List<(string Name, List<Vector2> Points)> templates = new List<(string Name, List<Vector2> Points)>();

    public (string Name, float Score) Recognize(List<Vector2> points)
    {
        List<Vector2> normPoint = Normalize(points);
        List<Vector2> mirroredPoint = MirrorHorizontally(normPoint);
        float minDistance = float.MaxValue;
        string bestTemplate = "No match";
        foreach (var template in templates)
        {
            float dOriginal = PathDistance(normPoint, template.Points);
            float dMirrored = PathDistance(mirroredPoint, template.Points);

            float d = Mathf.Min(dOriginal, dMirrored);
            if (d < minDistance)
            {
                minDistance = d;
                bestTemplate = template.Name;
            }
        }
        float score = 1.0f - (minDistance / (0.5f * (float)Math.Sqrt(SquareSize * SquareSize + SquareSize * SquareSize)));
        return (bestTemplate, score);
    }

    private List<Vector2> MirrorHorizontally(List<Vector2> points)
    {
        List<Vector2> mirrored = new List<Vector2>();
        foreach (var p in points)
        {
            mirrored.Add(new Vector2(-p.x, p.y));
        }
        return mirrored;
    }

    public void AddTemplate(string name, List<Vector2> points)
    {
        List<Vector2> normPoint = Normalize(points);
        templates.Add((name, normPoint));
    }

    public List<Vector2> Normalize(List<Vector2> points)
    {
        points = Resample(points, NumPoints);
        //points = RotateToZero(points); // Borttagen för att urskilja rotation
        points = ScaleToSquare(points, SquareSize);
        points = TranslateToOrigin(points);

        return points;
    }

    private List<Vector2> Resample(List<Vector2> points, int n)
    {
        float I = PathLength(points) / (n - 1);
        float D = 0.0f;
        List<Vector2> newPoints = new List<Vector2> { points[0] };
        for (int i = 1; i < points.Count; i++)
        {
            float d = Distance(points[i - 1], points[i]);
            if ((D + d) >= I)
            {
                float qx = points[i - 1].x + ((I - D) / d) * (points[i].x - points[i - 1].x);
                float qy = points[i - 1].y + ((I - D) / d) * (points[i].y - points[i - 1].y);
                Vector2 q = new Vector2(qx, qy);
                newPoints.Add(q);
                points.Insert(i, q);
                D = 0.0f;
            }
            else
            {
                D += d;
            }
        }
        while (newPoints.Count < n)
        {
            newPoints.Add(points[^1]);
        }
        return newPoints;
    }

    private List<Vector2> RotateToZero(List<Vector2> points)
    {
        Vector2 c = Centroid(points);
        float theta = (float)Math.Atan2(points[0].y - c.y, points[0].x - c.x);
        return RotateBy(points, -theta);
    }

    private List<Vector2> ScaleToSquare(List<Vector2> points, float size)
    {
        RectangleF B = BoundingBox(points);
        List<Vector2> newPoints = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            float scale = Math.Max(B.Width, B.Height) == 0 ? 1 : size / Math.Max(B.Width, B.Height);
            float qx = p.x * scale;
            float qy = p.y * scale;
            newPoints.Add(new Vector2(qx, qy));
        }
        return newPoints;
    }

    private List<Vector2> TranslateToOrigin(List<Vector2> points)
    {
        Vector2 c = Centroid(points);
        List<Vector2> newPoints = new List<Vector2>();
        foreach (Vector2 p in points)
        {
            float qx = p.x - c.x;
            float qy = p.y - c.y;
            newPoints.Add(new Vector2(qx, qy));
        }
        return newPoints;
    }

    private float PathLength(List<Vector2> points)
    {
        float d = 0.0f;
        for (int i = 1; i < points.Count; i++)
        {
            d += Distance(points[i - 1], points[i]);
        }
        return d;
    }

    private Vector2 Centroid(List<Vector2> points)
    {
        float x = 0.0f, y = 0.0f;
        foreach (Vector2 p in points)
        {
            x += p.x;
            y += p.y;
        }
        x /= points.Count;
        y /= points.Count;
        return new Vector2(x, y);
    }

    private RectangleF BoundingBox(List<Vector2> points)
    {
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (Vector2 p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }
        return new RectangleF(minX, minY, maxX - minX, maxY - minY);
    }

    private List<Vector2> RotateBy(List<Vector2> points, float theta)
    {
        Vector2 c = Centroid(points);
        List<Vector2> newPoints = new List<Vector2>();
        float cos = (float)Math.Cos(theta);
        float sin = (float)Math.Sin(theta);
        foreach (Vector2 p in points)
        {
            float qx = (p.x - c.x) * cos - (p.y - c.y) * sin + c.x;
            float qy = (p.x - c.x) * sin + (p.y - c.y) * cos + c.y;
            newPoints.Add(new Vector2(qx, qy));
        }
        return newPoints;
    }

    private float Distance(Vector2 p1, Vector2 p2)
    {
        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    private float PathDistance(List<Vector2> pts1, List<Vector2> pts2)
    {
        float d = 0.0f;
        for (int i = 0; i < pts1.Count; i++)
        {
            d += Distance(pts1[i], pts2[i]);

            if (i > 0)
            {
                Vector2 v1 = pts1[i] - pts1[i - 1];
                Vector2 v2 = pts2[i] - pts2[i - 1];
                float angle = Vector2.Angle(v1, v2) / 180f;
                d += angle * 20f;
            }
        }
        return d / pts1.Count;
    }
}