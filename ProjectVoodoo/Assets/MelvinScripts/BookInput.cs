using UnityEngine;

public class BookInput : MonoBehaviour
{
    [SerializeField]
    public GameObject bookUI;

    public bool isBookOpen = false;
    public GameObject StrokeInput;
    public SpellCastingManager spellCastingManager;

    private void Update()
    {
        bookUI.SetActive(isBookOpen);
        StrokeInput.SetActive(isBookOpen);
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isBookOpen = !isBookOpen;
            spellCastingManager.ClearRunes();
        }
        SetCursorVisibility();
    }

    private void PageTurn()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Code to turn to the next page
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Code to turn to the previous page
        }
    }

    private void SetCursorVisibility()
    {
        if (isBookOpen)
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