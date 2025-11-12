using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakerInteract : MonoBehaviour, IInteractable
{
    void IInteractable.Interact()
    {
        ScoreCounter.Instance.AddScore(1);
        SceneManager.LoadScene("Main");
    }
}