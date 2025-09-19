using UnityEngine;

[CreateAssetMenu(fileName = "Runes", menuName = "Scriptable Objects/Rune")]
public class RuneData : ScriptableObject
{
    public RuneType Type;
    public string DisplayName;
    public Color ThemeColor;
    public GameObject ParticlePrefab;
    public AudioClip RuneVoice;
}
