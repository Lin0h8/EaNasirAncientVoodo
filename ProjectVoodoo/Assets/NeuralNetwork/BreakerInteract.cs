using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakerInteract : MonoBehaviour, IInteractable
{
    void IInteractable.Interact()
    {
        SceneManager.LoadScene("Main");
    }
}