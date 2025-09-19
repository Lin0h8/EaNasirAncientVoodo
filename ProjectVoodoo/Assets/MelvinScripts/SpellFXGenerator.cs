using UnityEngine;

public class SpellFXGenerator : MonoBehaviour
{
    public ParticleSystem GenerateSpellFX(RuneData[] runes)
    {
        if (runes == null || runes.Length == 0)
        {
            Debug.LogWarning("No runes provided for spell FX generation.");
            return null;
        }

        GameObject spellObj = new GameObject("SpellFX");
        ParticleSystem ps = spellObj.AddComponent<ParticleSystem>();

        var main = ps.main;
        var emission = ps.emission;
        var shape = ps.shape;
        var noise = ps.noise;
        var trails = ps.trails;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        RuneData primaryRune = runes[0];
        var collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        emission.enabled = true;
        emission.rateOverTime = 50f;
        main.startSize = 0.5f;
        main.startLifetime = 1f;
        main.startSpeed = 1f;
        main.loop = false;
        main.duration = 2f * primaryRune.LifetimeMultiplier;
        main.startColor = primaryRune.ThemeColor;
        main.startSizeMultiplier = primaryRune.SizeMultiplier;
        main.startLifetimeMultiplier = primaryRune.LifetimeMultiplier;
        main.startSpeedMultiplier = primaryRune.SpeedMultiplier;
        shape.shapeType = primaryRune.ShapeType;
        noise.enabled = primaryRune.NoiseStrength > 0f;
        noise.strength = primaryRune.NoiseStrength;
        main.gravityModifier = primaryRune.NegativeGravity ? -1f : 1f;
        var lights = ps.lights;
        lights.enabled = primaryRune.lightEmission;
        lights.useParticleColor = true;
        lights.ratio = 0.5f;
        lights.intensityMultiplier = 2f;
        lights.rangeMultiplier = 5f;

        ps.Play();

        RuneData secondaryRune = runes.Length > 1 ? runes[1] : null;
        if (secondaryRune != null)
        {
            emission.rateOverTimeMultiplier *= secondaryRune.EmissionRateMultiplier;
            main.startSizeMultiplier *= secondaryRune.SizeMultiplier;
            main.startSpeedMultiplier *= secondaryRune.SpeedMultiplier;
            if (secondaryRune.OverrideMaterial != null)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.material = secondaryRune.OverrideMaterial;
            }
        }

        RuneData tertiaryRune = runes.Length > 2 ? runes[2] : null;
        if (tertiaryRune != null && tertiaryRune.AddTrail)
        {
            trails.enabled = true;
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.trailMaterial = tertiaryRune.OverrideMaterial != null ? tertiaryRune.OverrideMaterial : renderer.material;
            var currentStrength = noise.strength.constant;
            noise.strength = new ParticleSystem.MinMaxCurve(currentStrength + tertiaryRune.NoiseStrength);
        }

        RuneComboManager comboManager = GetComponent<RuneComboManager>();
        RuneType[] runeTypes = new RuneType[runes.Length];
        for (int i = 0; i < runes.Length; i++)
        {
            runeTypes[i] = runes[i].Type;
        }
        RuneCombo combo = comboManager.GetMatchingCombo(runeTypes);
        float comboSize = combo != null ? combo.SizeMultiplier : 1f;
        float comboSpeed = combo != null ? combo.SpeedMultiplier : 1f;
        float comboLifetime = combo != null ? combo.LifetimeMultiplier : 1f;
        float comboEmission = combo != null ? combo.EmissionRateMultiplier : 1f;

        main.startSizeMultiplier *= comboSize;
        main.startSpeedMultiplier *= comboSpeed;
        main.startLifetimeMultiplier *= comboLifetime;
        emission.rateOverTimeMultiplier *= comboEmission;

        Destroy(ps.gameObject, main.duration + main.startLifetime.constantMax);
        return ps;
    }
}