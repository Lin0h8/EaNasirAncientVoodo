using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHandScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card Info")]
    [SerializeField] private string developerName = "";
    [SerializeField, TextArea(3, 5)] private string developerDescription = "";

    [Header("References")]
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private Canvas parentCanvas;

    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverTransitionDuration = 0.2f;
    [SerializeField] private AnimationCurve hoverEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 _originalScale;
    private int _originalSortingOrder;
    private Canvas _cardCanvas;
    private Coroutine _scaleRoutine;

    private void Awake()
    {
        _originalScale = transform.localScale;

        _cardCanvas = GetComponent<Canvas>();
        if (_cardCanvas == null)
        {
            _cardCanvas = gameObject.AddComponent<Canvas>();
            _cardCanvas.overrideSorting = true;
        }

        _originalSortingOrder = _cardCanvas.sortingOrder;

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoText != null)
        {
            infoText.text = $"<b>{developerName}</b>\n{developerDescription}";
        }

        _cardCanvas.sortingOrder = 100;

        StartScaleTransition(_originalScale, _originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoText != null)
        {
            infoText.text = "";
        }

        _cardCanvas.sortingOrder = _originalSortingOrder;

        StartScaleTransition(transform.localScale, _originalScale);
    }

    private void StartScaleTransition(Vector3 from, Vector3 to)
    {
        if (_scaleRoutine != null)
            StopCoroutine(_scaleRoutine);

        if (hoverTransitionDuration <= 0f)
        {
            transform.localScale = to;
            return;
        }

        _scaleRoutine = StartCoroutine(ScaleTransitionCoroutine(from, to));
    }

    private System.Collections.IEnumerator ScaleTransitionCoroutine(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;
        while (elapsed < hoverTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / hoverTransitionDuration);
            float eased = hoverEasing != null ? hoverEasing.Evaluate(t) : t;
            transform.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        transform.localScale = to;
        _scaleRoutine = null;
    }

    private void OnValidate()
    {
        if (_cardCanvas == null)
        {
            _cardCanvas = GetComponent<Canvas>();
        }
    }
}