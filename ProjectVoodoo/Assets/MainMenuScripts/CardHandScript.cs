using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private Coroutine _scaleRoutine;
    private Graphic _cardGraphic;
    private int _originalSiblingIndex;
    private GraphicRaycaster _raycaster;

    private void Awake()
    {
        _originalScale = transform.localScale;
        _originalSiblingIndex = transform.GetSiblingIndex();

        _cardGraphic = GetComponent<RawImage>();
        if (_cardGraphic == null)
        {
            _cardGraphic = GetComponent<Image>();
        }

        if (_cardGraphic == null)
        {
            _cardGraphic = gameObject.AddComponent<Image>();
            ((Image)_cardGraphic).color = new Color(1, 1, 1, 0.01f);
        }

        _cardGraphic.raycastTarget = true;

        Canvas cardCanvas = GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            DestroyImmediate(cardCanvas);
        }

        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>();
        }

        if (parentCanvas != null)
        {
            _raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
            if (_raycaster == null)
            {
                _raycaster = parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        if (EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        var tmpComponents = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var tmp in tmpComponents)
        {
            if (tmp.raycastTarget)
            {
                tmp.raycastTarget = false;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoText != null)
        {
            infoText.text = $"<b>{developerName}</b>\n{developerDescription}";
        }

        transform.SetAsLastSibling();
        StartScaleTransition(_originalScale, _originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoText != null)
        {
            infoText.text = "";
        }

        transform.SetSiblingIndex(_originalSiblingIndex);
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
        Canvas cardCanvas = GetComponent<Canvas>();
        if (cardCanvas != null)
        {
            DestroyImmediate(cardCanvas);
        }
    }
}