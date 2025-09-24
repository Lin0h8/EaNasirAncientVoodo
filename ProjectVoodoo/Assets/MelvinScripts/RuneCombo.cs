using UnityEngine;

[CreateAssetMenu(fileName = "RuneCombo", menuName = "Scriptable Objects/RuneCombo")]
public class RuneCombo : ScriptableObject
{
    public RuneType[] Sequence;

    public float SizeMultiplier = 1f;
    public float SpeedMultiplier = 1f;
    public float LifetimeMultiplier = 1f;
    public float EmissionRateMultiplier = 1f;
}