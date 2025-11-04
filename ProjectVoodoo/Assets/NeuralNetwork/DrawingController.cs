using UnityEngine;
using System.Collections.Generic;
using System.IO;
using NeuralNetwork_IHNMAIMS;
using UnityEngine.UI;
using System.Linq;

public class DrawingController : MonoBehaviour
{
    public RectTransform drawingArea;
    public Color lineColor = Color.black;
    public Material lineMaterial;
    public float lineThickness = 1.5f;
    public string networkModelPath = "trained_network.json";
    public Text recognitionResultText;
    public List<RuneData> runeDatabase;

    [Header("Rune System")]
    public RuneMagicController runeMagicController;

    public Text suggestionResultText;
    public TomeManager tomeManager;
    private Canvas _canvas;
    private LineRenderer _currentLineRenderer;
    private bool _isDrawingMode = false;
    private Camera _mainCamera;
    private NetworkProcessor _networkProcessor;
    private List<RuneData> _runeSequence = new List<RuneData>();
    private List<GameObject> _strokes = new List<GameObject>();

    public static Material GetMaterialForRune(RuneData runeData, Material defaultMaterial)
    {
        if (runeData.useRandomFromMaterialList && runeData.overrideMaterials.Any())
        {
            int randomIndex = Random.Range(0, runeData.overrideMaterials.Count);
            Material selectedMaterial = runeData.overrideMaterials[randomIndex];
            if (selectedMaterial != null)
            {
                return selectedMaterial;
            }
        }

        if (runeData.overrideMaterial != null)
        {
            return runeData.overrideMaterial;
        }

        return defaultMaterial;
    }

    private float[] CaptureAndProcessDrawing()
    {
        var pixels = new float[28 * 28];
        if (_currentLineRenderer.positionCount < 2) return pixels;

        var points = new Vector3[_currentLineRenderer.positionCount];
        _currentLineRenderer.GetPositions(points);

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        Vector2 massCenter = Vector2.zero;
        foreach (var p in points)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
            massCenter += new Vector2(p.x, p.y);
        }
        massCenter /= points.Length;

        float drawingWidth = maxX - minX;
        float drawingHeight = maxY - minY;
        if (drawingWidth == 0 || drawingHeight == 0) return pixels;
        float scale = (drawingWidth > drawingHeight) ? 20.0f / drawingWidth : 20.0f / drawingHeight;

        Vector2 offset = new Vector2(14, 14) - new Vector2((massCenter.x - minX) * scale, (massCenter.y - minY) * scale);

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 start = new Vector2((points[i].x - minX) * scale + offset.x, (points[i].y - minY) * scale + offset.y);
            Vector2 end = new Vector2((points[i + 1].x - minX) * scale + offset.x, (points[i + 1].y - minY) * scale + offset.y);
            DrawLineOnArray(pixels, start, end, 28, 28, lineThickness);
        }

        return pixels;
    }

    private void ClearDrawing()
    {
        foreach (var stroke in _strokes)
        {
            Destroy(stroke);
        }
        _strokes.Clear();
        _currentLineRenderer = null;
    }

    private void Draw()
    {
        var cam = GetUICamera();

        if (!RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition, cam))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, Input.mousePosition, cam, out Vector2 localPoint);

        if (_currentLineRenderer == null)
        {
            GameObject lineGO = new GameObject("DrawingStroke");
            lineGO.transform.SetParent(drawingArea, false);
            _strokes.Add(lineGO);

            _currentLineRenderer = lineGO.AddComponent<LineRenderer>();
            var mat = lineMaterial;
            if (mat == null)
            {
                var fallbackShader = Shader.Find("Sprites/Default");
                if (fallbackShader == null)
                {
                    fallbackShader = Shader.Find("Universal Render Pipeline/Unlit");
                }
                mat = new Material(fallbackShader);
            }
            _currentLineRenderer.material = mat;
            _currentLineRenderer.startColor = lineColor;
            _currentLineRenderer.endColor = lineColor;
            _currentLineRenderer.startWidth = 5f;
            _currentLineRenderer.endWidth = 5f;
            _currentLineRenderer.positionCount = 0;
            _currentLineRenderer.useWorldSpace = false;
            _currentLineRenderer.alignment = LineAlignment.View;

            if (_canvas != null)
            {
                _currentLineRenderer.sortingLayerID = _canvas.sortingLayerID;
                _currentLineRenderer.sortingOrder = _canvas.sortingOrder + 100;
            }
            else
            {
                _currentLineRenderer.sortingLayerName = "UI";
                _currentLineRenderer.sortingOrder = short.MaxValue;
            }

            _currentLineRenderer.material.renderQueue = 4000;
        }

        if (_currentLineRenderer.positionCount == 0 || Vector2.Distance(_currentLineRenderer.GetPosition(_currentLineRenderer.positionCount - 1), localPoint) > 1f)
        {
            _currentLineRenderer.positionCount++;
            _currentLineRenderer.SetPosition(_currentLineRenderer.positionCount - 1, new Vector3(localPoint.x, localPoint.y, 0f));
        }
    }

    private void DrawLineOnArray(float[] pixels, Vector2 from, Vector2 to, int width, int height, float thickness)
    {
        int x0 = (int)from.x;
        int y0 = (int)from.y;
        int x1 = (int)to.x;
        int y1 = (int)to.y;

        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            int drawRadius = Mathf.CeilToInt(thickness);
            for (int tx = -drawRadius; tx <= drawRadius; tx++)
            {
                for (int ty = -drawRadius; ty <= drawRadius; ty++)
                {
                    int px = x0 + tx;
                    int py = y0 + ty;

                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        float distance = Mathf.Sqrt(tx * tx + ty * ty);
                        if (distance < thickness)
                        {
                            float value = 1.0f - (distance / thickness);
                            int index = (height - 1 - py) * width + px;
                            pixels[index] = Mathf.Max(pixels[index], value);
                        }
                    }
                }
            }

            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    private Camera GetUICamera()
    {
        if (_canvas == null)
            return null;

        if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return _canvas.worldCamera != null ? _canvas.worldCamera : (_mainCamera != null ? _mainCamera : Camera.main);
    }

    private void RecognizeDrawing()
    {
        if (_currentLineRenderer == null) return;

        float[] pixels = CaptureAndProcessDrawing();
        int digit = _networkProcessor.RecognizeDigit(pixels);
        Debug.Log($"Recognized Digit: {digit}");

        if (digit >= 0 && digit < runeDatabase.Count)
        {
            _runeSequence.Add(runeDatabase[digit]);
            if (_runeSequence.Count > 3)
            {
                _runeSequence.RemoveAt(0);
            }
        }

        UpdateRuneText();
        UpdateSuggestions();

        if (_runeSequence.Count == 3)
        {
            var runes = _runeSequence.ToArray();
            if (runes.Any(r => r.isProjectile))
            {
                var origin = transform.position;
                var direction = (_mainCamera != null ? _mainCamera.transform.forward : transform.forward);
                runeMagicController.ThrowSpellProjectile(runes, origin, direction);
            }
            else
            {
                runeMagicController.GenerateSpell(runes, transform.position);
            }

            _runeSequence.Clear();
            UpdateRuneText();
            UpdateSuggestions();
        }

        ClearDrawing();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _canvas = drawingArea != null ? drawingArea.GetComponentInParent<Canvas>() : null;
        string fullPath = Path.Combine(Application.streamingAssetsPath, networkModelPath);
        _networkProcessor = new NetworkProcessor(fullPath);

        if (drawingArea == null || runeMagicController == null)
        {
            Debug.LogError("Drawing Area and Rune Magic Controller must be assigned in the inspector.");
            enabled = false;
        }
        UpdateRuneText();
    }

    private void StartDrawing()
    {
        ClearDrawing();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _isDrawingMode = !_isDrawingMode;
            if (_isDrawingMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _currentLineRenderer = null;
                ClearDrawing();
            }
        }

        if (_isDrawingMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing();
            }
            else if (Input.GetMouseButton(0))
            {
                Draw();
            }
            else if (Input.GetMouseButtonUp(0) && _currentLineRenderer != null)
            {
                RecognizeDrawing();
                _currentLineRenderer = null;
            }
        }
    }

    private void UpdateRuneText()
    {
        if (recognitionResultText != null)
        {
            string text = "Runes: ";
            foreach (var runeData in _runeSequence)
            {
                text += runeData.runeType.ToString() + " ";
            }
            recognitionResultText.text = text;
        }
    }

    private void UpdateSuggestions()
    {
        if (tomeManager == null || suggestionResultText == null) return;

        var suggestions = tomeManager.GetNextRuneSuggestions(_runeSequence);
        string text = "Suggestions: ";
        if (suggestions.Any())
        {
            text += string.Join(", ", suggestions);
        }
        else
        {
            text += "None";
        }
        suggestionResultText.text = text;
    }
}