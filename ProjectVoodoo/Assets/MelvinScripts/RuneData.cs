using UnityEngine;

[CreateAssetMenu(fileName = "Runes", menuName = "Scriptable Objects/Rune")]
public class RuneData : ScriptableObject
{
    public RuneType Type;
    public Color ThemeColor;
    public AudioClip RuneVoice;

    [Header("FX properties")]
    public Gradient ColorOverLife;

    public bool lightEmission = false;
    public bool NegativeGravity = false;
    public float SizeMultiplier = 1f;
    public float LifetimeMultiplier = 1f;
    public float EmissionRateMultiplier = 1f;
    public ParticleSystemShapeType ShapeType;
    public float NoiseStrength = 0f;
    public float SpeedMultiplier = 1f;
    public bool AddTrail = false;
    public Material OverrideMaterial;
}