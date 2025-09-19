using UnityEngine;

public class BookInput : MonoBehaviour
{
    [SerializeField]
    public GameObject bookUI;
    private bool isBookOpen = false;
    public GameObject StrokeInput;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            
            isBookOpen = !isBookOpen;
            bookUI.SetActive(!isBookOpen);
            StrokeInput.SetActive(!isBookOpen);
        }
        SetCursorVisibility();

    }

    private void PageTurn()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            // Code to turn to the next page
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            // Code to turn to the previous page
        }
    }
    private void SetCursorVisibility()
    {
        if (!isBookOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}