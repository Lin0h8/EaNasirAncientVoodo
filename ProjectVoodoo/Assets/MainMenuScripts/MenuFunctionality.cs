using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class MenuFunctionality : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex < 0)
        {
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Reset()
    {
        ScoreCounter.Instance.ResetScore();
    }
}