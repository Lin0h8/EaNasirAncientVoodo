using UnityEngine;

public class BreakerInteract : MonoBehaviour, IInteractable
{
    public ScreenTransition screenTransition;
    public MazeGenerator mazeGenerator;
    public float delayBeforeRegenerate = 0.5f;
    public float delayBeforeFadeIn = 0.3f;

    void IInteractable.Interact()
    {
        Debug.Log("Breaker Interacted");

        if (screenTransition == null)
        {
            Debug.LogWarning("ScreenTransition reference is missing!");
            return;
        }

        if (mazeGenerator == null)
        {
            Debug.LogWarning("MazeGenerator reference is missing!");
            return;
        }

        screenTransition.FadeToBlack(() =>
        {
            StartCoroutine(RegenerateMaze());
        });
    }

    private System.Collections.IEnumerator RegenerateMaze()
    {
        yield return new WaitForSeconds(delayBeforeRegenerate);

        mazeGenerator.Generate();

        yield return new WaitForSeconds(delayBeforeFadeIn);

        screenTransition.FadeFromBlack();
    }
}