using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NeuralNetwork_IHNMAIMS;
using UnityEngine.UI;
using System.Linq;

public class DrawingController : MonoBehaviour
{
    public RectTransform bookContentRect;
    public RectTransform drawingArea;
    public Color lineColor = Color.black;
    public Material lineMaterial;
    public float lineThickness = 5f;
    public string networkModelPath = "trained_network.json";
    public float pullDownDuration = 0.3f;

    [Header("Canvas Animation")]
    public float pullUpDistance = 50f;

    public float pullUpDuration = 0.15f;
    public Text recognitionResultText;
    public List<RuneData> runeDatabase;

    [Header("Rune System")]
    public RuneMagicController runeMagicController;

    [Header("Spell Book UI")]
    public GameObject spellBookCanvas;

    public CanvasGroup spellBookCanvasGroup;
    public Text suggestionResultText;
    public TomeManager tomeManager;
    private List<GameObject> _allStrokes = new List<GameObject>();
    private Canvas _canvas;
    private List<Vector2> _currentStrokePoints = new List<Vector2>();
    private List<GameObject> _currentStrokeSegments = new List<GameObject>();
    private bool _isAnimatingClose = false;
    private bool _isDrawingMode = false;
    private Vector2 _lastDrawnPoint;
    private Camera _mainCamera;
    private NetworkProcessor _networkProcessor;

    private float _previousTimeScale = 1f;

    private RuneData[] _readySpell = null;
    private List<RuneData> _runeSequence = new List<RuneData>();
    private bool _spellReady = false;

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

    private IEnumerator AnimateCanvasClose()
    {
        ApplyPauseState(false);

        if (spellBookCanvas == null) yield break;

        _isAnimatingClose = true;

        RectTransform targetRect = bookContentRect;
        if (targetRect == null)
        {
            targetRect = spellBookCanvas.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                spellBookCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _isAnimatingClose = false;
                yield break;
            }
        }

        Vector2 originalPosition = targetRect.anchoredPosition;
        Vector2 pullUpPosition = originalPosition + new Vector2(0, pullUpDistance);

        float elapsed = 0f;
        while (elapsed < pullUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pullUpDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            targetRect.anchoredPosition = Vector2.Lerp(originalPosition, pullUpPosition, easedT);
            yield return null;
        }
        targetRect.anchoredPosition = pullUpPosition;

        elapsed = 0f;
        Vector2 pullDownPosition = originalPosition - new Vector2(0, Screen.height + 100);

        while (elapsed < pullDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pullDownDuration;
            float easedT = Mathf.Pow(t, 2f);
            targetRect.anchoredPosition = Vector2.Lerp(pullUpPosition, pullDownPosition, easedT);

            if (spellBookCanvasGroup != null)
            {
                spellBookCanvasGroup.alpha = 1f - easedT;
            }

            yield return null;
        }

        spellBookCanvas.SetActive(false);

        targetRect.anchoredPosition = originalPosition;
        if (spellBookCanvasGroup != null)
        {
            spellBookCanvasGroup.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _isAnimatingClose = false;
    }

    private IEnumerator AnimateCanvasCloseOnTab()
    {
        ApplyPauseState(false);

        if (spellBookCanvas == null) yield break;

        _isAnimatingClose = true;

        RectTransform targetRect = bookContentRect;
        if (targetRect == null)
        {
            targetRect = spellBookCanvas.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                spellBookCanvas.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _isAnimatingClose = false;
                yield break;
            }
        }

        Vector2 originalPosition = targetRect.anchoredPosition;
        Vector2 pullUpPosition = originalPosition + new Vector2(0, pullUpDistance);

        float elapsed = 0f;
        while (elapsed < pullUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pullUpDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            targetRect.anchoredPosition = Vector2.Lerp(originalPosition, pullUpPosition, easedT);
            yield return null;
        }
        targetRect.anchoredPosition = pullUpPosition;

        elapsed = 0f;
        Vector2 pullDownPosition = originalPosition - new Vector2(0, Screen.height + 100);

        while (elapsed < pullDownDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / pullDownDuration;
            float easedT = Mathf.Pow(t, 2f);
            targetRect.anchoredPosition = Vector2.Lerp(pullUpPosition, pullDownPosition, easedT);

            if (spellBookCanvasGroup != null)
            {
                spellBookCanvasGroup.alpha = 1f - easedT;
            }

            yield return null;
        }

        ClearDrawing();

        spellBookCanvas.SetActive(false);

        targetRect.anchoredPosition = originalPosition;
        if (spellBookCanvasGroup != null)
        {
            spellBookCanvasGroup.alpha = 1f;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _isAnimatingClose = false;
    }

    private void ApplyPauseState(bool paused)
    {
        if (paused)
        {
            if (Time.timeScale != 0f)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
        }
        else
        {
            Time.timeScale = _previousTimeScale;
        }
    }

    private float[] CaptureAndProcessDrawing()
    {
        var pixels = new float[28 * 28];
        if (_currentStrokePoints.Count < 2) return pixels;

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        Vector2 massCenter = Vector2.zero;

        foreach (var p in _currentStrokePoints)
        {
            minX = Mathf.Min(minX, p.x);
            maxX = Mathf.Max(maxX, p.x);
            minY = Mathf.Min(minY, p.y);
            maxY = Mathf.Max(maxY, p.y);
            massCenter += p;
        }
        massCenter /= _currentStrokePoints.Count;

        float drawingWidth = maxX - minX;
        float drawingHeight = maxY - minY;
        if (drawingWidth == 0 || drawingHeight == 0) return pixels;
        float scale = (drawingWidth > drawingHeight) ? 20.0f / drawingWidth : 20.0f / drawingHeight;

        Vector2 offset = new Vector2(14, 14) - new Vector2((massCenter.x - minX) * scale, (massCenter.y - minY) * scale);

        for (int i = 0; i < _currentStrokePoints.Count - 1; i++)
        {
            Vector2 start = new Vector2((_currentStrokePoints[i].x - minX) * scale + offset.x,
                                       (_currentStrokePoints[i].y - minY) * scale + offset.y);
            Vector2 end = new Vector2((_currentStrokePoints[i + 1].x - minX) * scale + offset.x,
                                     (_currentStrokePoints[i + 1].y - minY) * scale + offset.y);
            DrawLineOnArray(pixels, start, end, 28, 28, lineThickness);
        }

        return pixels;
    }

    private void ClearDrawing()
    {
        foreach (var stroke in _allStrokes)
        {
            Destroy(stroke);
        }
        _allStrokes.Clear();
        _currentStrokeSegments.Clear();
        _currentStrokePoints.Clear();
    }

    private void CreateLineSegment(Vector2 start, Vector2 end)
    {
        GameObject lineObj = new GameObject("LineSegment");
        lineObj.transform.SetParent(drawingArea, false);

        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = lineColor;
        lineImage.raycastTarget = false;

        RectTransform rectTransform = lineImage.rectTransform;

        Vector2 dir = end - start;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Vector2 midpoint = (start + end) / 2f;
        rectTransform.anchoredPosition = midpoint;
        rectTransform.sizeDelta = new Vector2(distance, lineThickness);
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        _currentStrokeSegments.Add(lineObj);
        _allStrokes.Add(lineObj);
    }

    private void Draw()
    {
        var cam = GetUICamera();

        if (!RectTransformUtility.RectangleContainsScreenPoint(drawingArea, Input.mousePosition, cam))
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(drawingArea, Input.mousePosition, cam, out Vector2 localPoint);

        if (_currentStrokePoints.Count == 0 || Vector2.Distance(_lastDrawnPoint, localPoint) > 2f)
        {
            _currentStrokePoints.Add(localPoint);
            _lastDrawnPoint = localPoint;

            if (_currentStrokePoints.Count >= 2)
            {
                Vector2 startPoint = _currentStrokePoints[_currentStrokePoints.Count - 2];
                Vector2 endPoint = _currentStrokePoints[_currentStrokePoints.Count - 1];
                CreateLineSegment(startPoint, endPoint);
            }
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

    private void FireReadySpell()
    {
        if (!_spellReady || _readySpell == null) return;

        var runes = _readySpell;
        if (runes.Any(r => r.isProjectile))
        {
            var origin = _mainCamera != null ? _mainCamera.transform.position : transform.position;
            var direction = _mainCamera != null ? _mainCamera.transform.forward : transform.forward;
            runeMagicController.ThrowSpellProjectile(runes, origin, direction);
        }
        else
        {
            var targetPosition = _mainCamera != null ? _mainCamera.transform.position + _mainCamera.transform.forward * 5f : transform.position;
            runeMagicController.GenerateSpell(runes, targetPosition);
        }

        _spellReady = false;
        _readySpell = null;
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
        if (_currentStrokePoints.Count < 2) return;

        float[] pixels = CaptureAndProcessDrawing();
        int digit = _networkProcessor.RecognizeDigit(pixels);

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
            _readySpell = _runeSequence.ToArray();
            _spellReady = true;
            _isDrawingMode = false;

            StartCoroutine(AnimateCanvasClose());

            _runeSequence.Clear();
            UpdateRuneText();
            UpdateSuggestions();
        }

        foreach (var segment in _currentStrokeSegments)
        {
            Destroy(segment);
        }

        _currentStrokePoints.Clear();
        _currentStrokeSegments.Clear();
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

        if (spellBookCanvas != null)
        {
            spellBookCanvas.SetActive(false);
        }

        if (spellBookCanvasGroup == null && spellBookCanvas != null)
        {
            spellBookCanvasGroup = spellBookCanvas.GetComponent<CanvasGroup>();
        }

        if (bookContentRect == null && spellBookCanvas != null)
        {
            RectTransform canvasRect = spellBookCanvas.GetComponent<RectTransform>();
            if (canvasRect != null && canvasRect.childCount > 0)
            {
                bookContentRect = canvasRect.GetChild(0).GetComponent<RectTransform>();
            }
        }

        UpdateRuneText();
    }

    private void StartDrawing()
    {
        _currentStrokePoints.Clear();
        _currentStrokeSegments.Clear();
    }

    private void Update()
    {
        if (_isAnimatingClose) return;

        if (_spellReady && Input.GetMouseButtonDown(0))
        {
            FireReadySpell();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (_isDrawingMode)
            {
                _isDrawingMode = false;
                StartCoroutine(AnimateCanvasCloseOnTab());
            }
            else
            {
                _isDrawingMode = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (spellBookCanvas != null)
                {
                    spellBookCanvas.SetActive(true);

                    RectTransform targetRect = bookContentRect != null ? bookContentRect : spellBookCanvas.GetComponent<RectTransform>();
                    if (targetRect != null)
                    {
                        targetRect.anchoredPosition = Vector2.zero;
                    }

                    if (spellBookCanvasGroup != null)
                    {
                        spellBookCanvasGroup.alpha = 1f;
                    }
                }

                ApplyPauseState(true);
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
            else if (Input.GetMouseButtonUp(0) && _currentStrokePoints.Count > 0)
            {
                RecognizeDrawing();
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