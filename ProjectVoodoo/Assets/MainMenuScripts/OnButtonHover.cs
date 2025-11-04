using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Text")]
    public TextMeshProUGUI targetText;

    public Color hoverColor = new Color(1f, 0.85f, 0.3f, 1f);
    public bool revertOnExit = true;

    [Header("Transition")]
    public float colorTransitionDuration = 0.15f;

    public AnimationCurve colorEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool useUnscaledTime = true;

    private Color _originalColor;
    private Coroutine _colorRoutine;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TextMeshProUGUI>();

        if (targetText != null)
            _originalColor = targetText.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText == null) return;
        StartColorTransition(targetText.color, hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText == null) return;
        if (revertOnExit)
            StartColorTransition(targetText.color, _originalColor);
    }

    private void StartColorTransition(Color from, Color to)
    {
        if (_colorRoutine != null)
            StopCoroutine(_colorRoutine);

        if (colorTransitionDuration <= 0f)
        {
            targetText.color = to;
            _colorRoutine = null;
            return;
        }

        _colorRoutine = StartCoroutine(ColorTransitionCoroutine(from, to, colorTransitionDuration));
    }

    private System.Collections.IEnumerator ColorTransitionCoroutine(Color from, Color to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = colorEasing != null ? colorEasing.Evaluate(t) : t;
            targetText.color = Color.LerpUnclamped(from, to, eased);
            yield return null;
        }
        targetText.color = to;
        _colorRoutine = null;
    }
}