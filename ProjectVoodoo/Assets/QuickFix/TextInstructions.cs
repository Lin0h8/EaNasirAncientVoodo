using UnityEngine;
using TMPro;
using System.Collections;

public class TextInstructions : MonoBehaviour
{
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private float letterDelay = 0.05f;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void ShowInstructions(string text)
    {
        StopAllCoroutines();
        StartCoroutine(TypeAndClearText(text));
    }

    private IEnumerator TypeAndClearText(string text)
    {
        instructionText.text = "";

        foreach (char letter in text)
        {
            instructionText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }

        yield return new WaitForSeconds(displayDuration);
        instructionText.text = "";
    }
}