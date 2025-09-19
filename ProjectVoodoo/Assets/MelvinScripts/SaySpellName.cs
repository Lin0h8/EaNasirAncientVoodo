using System.Collections;
using UnityEngine;

public class SaySpellName : MonoBehaviour
{
    public void PlayRuneVoices(RuneData[] runes, float delayBetween = 0.75f)
    {
        StartCoroutine(PlayRuneVoicesCoroutine(runes, delayBetween));
    }

    private IEnumerator PlayRuneVoicesCoroutine(RuneData[] runes, float delayBetween)
    {
        foreach (var rune in runes)
        {
            if (rune.RuneVoice != null)
            {
                GameObject tempAudio = new GameObject("TempAudio");
                tempAudio.transform.position = Camera.main.transform.position;
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = rune.RuneVoice;
                audioSource.spatialBlend = 1.0f;
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.Play();
                Destroy(tempAudio, rune.RuneVoice.length + 0.1f);
                yield return new WaitForSeconds(delayBetween);
            }
        }
    }
}