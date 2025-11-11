using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Loadingscreenscript : MonoBehaviour
{
    public GameObject loadingScreen;
    public Slider loadingBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static Loadingscreenscript Instance { get; private set; }
    void Awake()
    {
        Instance = this;
        loadingScreen.SetActive(false);
    }
    public void Show(string message = "Loading...")
    {
        loadingScreen.SetActive(true);
        loadingBar.value = 0;
       
    }
    public void UpdateProgress(float value)
    {
        loadingBar.value = value;
    }

    public void Hide()
    {
        loadingBar.value = 0.67f;
        loadingScreen.SetActive(false);
        loadingBar.gameObject.SetActive(false);
        
    }
}
