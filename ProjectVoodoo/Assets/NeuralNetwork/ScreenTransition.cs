using UnityEngine;
using UnityEngine.UI;

public class ScreenTransition : MonoBehaviour
{
    public float fadeDuration = 1f;
    public Image fadeImage;
    public bool useUnscaledTime = false;
    public AnimationCurve fadeEasing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private Coroutine _currentFadeCoroutine;

    private void Awake()
    {
        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
    }

    public void FadeToBlack(System.Action onComplete = null)
    {
        StartFade(0f, 1f, onComplete);
    }

    public void FadeFromBlack(System.Action onComplete = null)
    {
        StartFade(1f, 0f, onComplete);
    }

    public void StartFade(float startAlpha, float endAlpha, System.Action onComplete = null)
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }

        _currentFadeCoroutine = StartCoroutine(Fade(startAlpha, endAlpha, onComplete));
    }

    public void StopFade()
    {
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
            _currentFadeCoroutine = null;
        }
    }

    public void SetAlphaImmediate(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = Mathf.Clamp01(alpha);
            fadeImage.color = color;
        }
    }

    private System.Collections.IEnumerator Fade(float startAlpha, float endAlpha, System.Action onComplete)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = startAlpha;
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / fadeDuration);
            float easedTime = fadeEasing != null ? fadeEasing.Evaluate(normalizedTime) : normalizedTime;

            color.a = Mathf.Lerp(startAlpha, endAlpha, easedTime);
            fadeImage.color = color;

            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
        _currentFadeCoroutine = null;
        onComplete?.Invoke();
    }
}