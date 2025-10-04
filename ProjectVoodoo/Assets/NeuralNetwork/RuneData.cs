using NeuralNetwork_IHNMAIMS;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rune Data", menuName = "Rune System/Rune Data")]
public class RuneData : ScriptableObject
{
    [Header("General")]
    public NeuralNetwork_IHNMAIMS.RuneType runeType;

    [Header("Main Module")]
    public Color startColor = Color.white;

    public ParticleSystem.MinMaxCurve startLifetime = new ParticleSystem.MinMaxCurve(1f, 2f);
    public ParticleSystem.MinMaxCurve startSpeed = new ParticleSystem.MinMaxCurve(5f);
    public ParticleSystem.MinMaxCurve startSize = new ParticleSystem.MinMaxCurve(1f);
    public ParticleSystem.MinMaxCurve startRotation = new ParticleSystem.MinMaxCurve(0f);
    public float gravityModifier = 0f;

    [Header("Emission Module")]
    public float emissionRate = 50f;

    public ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[0];

    [Header("Shape Module")]
    public ParticleSystemShapeType shapeType = ParticleSystemShapeType.Sphere;

    public float shapeRadius = 1f;
    public float shapeAngle = 25f;
    public bool randomizeDirection = false;
    public float randomizeDirectionAmount = 15f;

    [Header("Lifetime Modules")]
    public ParticleSystem.MinMaxGradient colorOverLifetime = new ParticleSystem.MinMaxGradient(Color.white);

    public AnimationCurve sizeOverLifetime = AnimationCurve.Linear(0, 1, 1, 0);
    public ParticleSystem.MinMaxCurve rotationOverLifetime = new ParticleSystem.MinMaxCurve(0f);
    public ParticleSystem.MinMaxCurve velocityX = new ParticleSystem.MinMaxCurve(0f);
    public ParticleSystem.MinMaxCurve velocityY = new ParticleSystem.MinMaxCurve(0f);
    public ParticleSystem.MinMaxCurve velocityZ = new ParticleSystem.MinMaxCurve(0f);

    [Header("Noise Module")]
    public bool noiseEnabled = false;

    public float noiseStrength = 1f;
    public float noiseFrequency = 0.5f;
    public float noiseScrollSpeed = 1f;

    [Header("Trails Module")]
    public bool trailsEnabled = false;

    public ParticleSystem.MinMaxGradient trailColor = new ParticleSystem.MinMaxGradient(Color.white);
    public float trailLifetime = 0.2f;
    public float trailWidth = 0.1f;

    [Header("Collision Module")]
    public bool collisionEnabled = false;

    public ParticleSystem.MinMaxCurve bounce = new ParticleSystem.MinMaxCurve(0.5f);
    public ParticleSystem.MinMaxCurve lifetimeLoss = new ParticleSystem.MinMaxCurve(0f);
    public float dampen = 0.1f;
    public float damage = 10f;

    [Header("Effects")]
    public SpellEffect[] onHitEffects;

    [Header("Texture Sheet Animation")]
    public bool textureSheetEnabled = false;

    public int numTilesX = 1;
    public int numTilesY = 1;
    public ParticleSystemAnimationType animationType = ParticleSystemAnimationType.WholeSheet;
    public ParticleSystem.MinMaxCurve frameOverTime = new ParticleSystem.MinMaxCurve(0f);

    [Header("Lights Module")]
    public bool lightsEnabled = false;

    public Light lightPrefab;
    public float lightRatio = 0.1f;
    public float lightRange = 5f;
    public float lightIntensity = 1f;

    [Header("Sub-Emitters Module")]
    public bool subEmittersEnabled = false;

    public GameObject subEmitterPrefab;
    public ParticleSystemSubEmitterType subEmitterType = ParticleSystemSubEmitterType.Birth;

    [Header("Projectile Module")]
    public bool isProjectile = false;

    public GameObject projectilePrefab;
    public float projectileSpeed = 20f;
    public float projectileLifetime = 10f;
    public bool projectileUseGravity = true;
    public float projectileArc = 0.2f;
}