using UnityEngine;

public class BreakerInteract : MonoBehaviour, IInteractable
{
    public ScreenTransition screenTransition;

    void IInteractable.Interact()
    {
        Debug.Log("Breaker Interacted");
        if (screenTransition != null)
        {
            //screenTransition.FadeToBlack();
        }
    }
}