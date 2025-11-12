using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    public class RuneMagicController : MonoBehaviour
    {
        public Material defaultParticleMaterial;
        public Material defaultTrailMaterial;
        public TomeManager tomeManager;

        public void GenerateSpell(RuneData[] runes, Vector3 position)
        {
            if (runes == null || runes.Length == 0) return;

            var dominantRune = runes.Last();

            var materialsForRune = ResolveMaterialsForRune(dominantRune);
            if (materialsForRune == null || materialsForRune.Count == 0)
            {
                materialsForRune = new List<Material> { ResolveFallbackMaterial(null) };
            }

            var duration = runes.Max(r => r.systemDuration);
            var startLifetime = PreserveCurveOrAverage(runes, r => r.startLifetime, dominantRune);
            var startSpeed = PreserveCurveOrAverage(runes, r => r.startSpeed, dominantRune);
            var startSize = PreserveCurveOrAverage(runes, r => r.startSize, dominantRune);
            var startRotation = PreserveCurveOrAverage(runes, r => r.startRotation, dominantRune);
            var gravity = runes.Average(r => r.gravityModifier);

            var completedCombo = tomeManager?.GetCompletedCombo(runes);
            var emissionRate = runes.Average(r => r.emissionRate) * (completedCombo != null ? completedCombo.bonusDamageMultiplier : 1f);
            var burstsCombined = runes.Any(r => r.bursts != null && r.bursts.Length > 0)
                ? runes.SelectMany(r => r.bursts).ToArray()
                : null;

            var gradient = CreateBlendedGradient(runes);

            var sizeOverLifetimeCurve = new ParticleSystem.MinMaxCurve(1f, dominantRune.sizeOverLifetime);
            var rotationOverLifetimeCurve = PreserveCurveOrAverage(runes, r => r.rotationOverLifetime, dominantRune);
            var velocityX = PreserveCurveOrAverage(runes, r => r.velocityX, dominantRune);
            var velocityY = PreserveCurveOrAverage(runes, r => r.velocityY, dominantRune);
            var velocityZ = PreserveCurveOrAverage(runes, r => r.velocityZ, dominantRune);

            bool noiseEnabled = runes.Any(r => r.noiseEnabled);
            float noiseStrength = noiseEnabled ? runes.Where(r => r.noiseEnabled).Average(r => r.noiseStrength) : 0f;
            float noiseFrequency = noiseEnabled ? runes.Where(r => r.noiseEnabled).Average(r => r.noiseFrequency) : 0f;
            float noiseScrollSpeed = noiseEnabled ? runes.Where(r => r.noiseEnabled).Average(r => r.noiseScrollSpeed) : 0f;

            bool trailsEnabled = runes.Any(r => r.trailsEnabled);
            float trailLifetime = trailsEnabled ? runes.Where(r => r.trailsEnabled).Average(r => r.trailLifetime) : 0f;
            float trailWidth = trailsEnabled ? runes.Where(r => r.trailsEnabled).Average(r => r.trailWidth) : 0f;
            var trailGradient = trailsEnabled ? CreateBlendedGradient(runes.Where(r => r.trailsEnabled)) : new ParticleSystem.MinMaxGradient(Color.white);

            bool collisionEnabled = runes.Any(r => r.collisionEnabled);
            var collisionBounce = collisionEnabled ? PreserveCurveOrAverage(runes.Where(r => r.collisionEnabled).Select(r => r.bounce), dominantRune.bounce) : new ParticleSystem.MinMaxCurve(0f);
            var collisionLifetimeLoss = collisionEnabled ? PreserveCurveOrAverage(runes.Where(r => r.collisionEnabled).Select(r => r.lifetimeLoss), dominantRune.lifetimeLoss) : new ParticleSystem.MinMaxCurve(0f);
            float collisionDampen = collisionEnabled ? runes.Where(r => r.collisionEnabled).Average(r => r.dampen) : 0.1f;

            bool texSheetEnabled = runes.Any(r => r.textureSheetEnabled);
            int numTilesX = dominantRune.numTilesX;
            int numTilesY = dominantRune.numTilesY;
            var animationType = dominantRune.animationType;
            var frameOverTime = texSheetEnabled ? PreserveCurveOrAverage(runes.Where(r => r.textureSheetEnabled).Select(r => r.frameOverTime), dominantRune.frameOverTime) : new ParticleSystem.MinMaxCurve(0f);

            bool lightsEnabled = runes.Any(r => r.lightsEnabled);
            var lightPrefab = dominantRune.lightPrefab;
            float lightRatio = lightsEnabled ? runes.Where(r => r.lightsEnabled).Average(r => r.lightRatio) : 0.1f;
            float lightRange = lightsEnabled ? runes.Where(r => r.lightsEnabled).Average(r => r.lightRange) : 5f;
            float lightIntensity = lightsEnabled ? runes.Where(r => r.lightsEnabled).Average(r => r.lightIntensity) : 1f;

            bool subEmittersEnabled = runes.Any(r => r.subEmittersEnabled);
            var subEmitterPrefab = dominantRune.subEmitterPrefab;
            var subEmitterType = dominantRune.subEmitterType;

            var shapeType = dominantRune.shapeType;
            float shapeRadius = dominantRune.shapeRadius;
            float shapeAngle = dominantRune.shapeAngle;
            float randomDirAmount = dominantRune.randomizeDirection ? dominantRune.randomizeDirectionAmount : 0f;

            if (materialsForRune.Count == 1)
            {
                var go = CreateParticleSystemObject("ProceduralSpell", position, materialsForRune[0]);
                var ps = go.GetComponent<ParticleSystem>();

                var main = ps.main;
                main.loop = false;
                main.stopAction = ParticleSystemStopAction.Destroy;
                main.startLifetime = startLifetime;
                main.startSpeed = startSpeed;
                main.startSize = startSize;
                main.startRotation = startRotation;
                main.gravityModifier = gravity;
                main.duration = duration;

                var emission = ps.emission;
                emission.rateOverTime = emissionRate;
                if (burstsCombined != null)
                {
                    ps.emission.SetBursts(burstsCombined);
                }

                var shape = ps.shape;
                shape.shapeType = shapeType;
                shape.radius = shapeRadius;
                shape.angle = shapeAngle;
                shape.randomDirectionAmount = randomDirAmount;

                var col = ps.colorOverLifetime;
                col.enabled = true;
                col.color = gradient;

                var sol = ps.sizeOverLifetime;
                sol.enabled = true;
                sol.size = sizeOverLifetimeCurve;

                var rol = ps.rotationOverLifetime;
                rol.enabled = true;
                rol.z = rotationOverLifetimeCurve;

                var vol = ps.velocityOverLifetime;
                vol.enabled = true;
                vol.x = velocityX;
                vol.y = velocityY;
                vol.z = velocityZ;

                var noise = ps.noise;
                noise.enabled = noiseEnabled;
                if (noise.enabled)
                {
                    noise.strength = noiseStrength;
                    noise.frequency = noiseFrequency;
                    noise.scrollSpeed = noiseScrollSpeed;
                }

                var trails = ps.trails;
                trails.enabled = trailsEnabled;
                if (trails.enabled)
                {
                    var renderer = ps.GetComponent<ParticleSystemRenderer>();
                    if (defaultTrailMaterial != null)
                    {
                        renderer.trailMaterial = defaultTrailMaterial;
                    }
                    else
                    {
                        var trailShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                        if (trailShader == null) trailShader = Shader.Find("Particles/Standard Unlit");
                        if (trailShader == null) trailShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                        renderer.trailMaterial = new Material(trailShader);
                    }
                    trails.mode = ParticleSystemTrailMode.PerParticle;
                    trails.lifetime = new ParticleSystem.MinMaxCurve(trailLifetime);
                    trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, trailWidth);
                    trails.colorOverTrail = trailGradient;
                }

                var collision = ps.collision;
                collision.enabled = collisionEnabled;
                if (collision.enabled)
                {
                    collision.type = ParticleSystemCollisionType.World;
                    collision.mode = ParticleSystemCollisionMode.Collision3D;
                    collision.bounce = collisionBounce;
                    collision.lifetimeLoss = collisionLifetimeLoss;
                    collision.dampen = collisionDampen;
                    collision.sendCollisionMessages = true;
                }

                var texSheet = ps.textureSheetAnimation;
                texSheet.enabled = texSheetEnabled;
                if (texSheet.enabled)
                {
                    texSheet.numTilesX = numTilesX;
                    texSheet.numTilesY = numTilesY;
                    texSheet.animation = animationType;
                    texSheet.frameOverTime = frameOverTime;
                }

                var lights = ps.lights;
                lights.enabled = lightsEnabled;
                if (lights.enabled)
                {
                    lights.light = lightPrefab;
                    lights.ratio = lightRatio;
                    lights.range = lightRange;
                    lights.intensity = lightIntensity;
                }

                var subEmitters = ps.subEmitters;
                subEmitters.enabled = subEmittersEnabled;
                if (subEmitters.enabled)
                {
                    if (subEmitterPrefab != null)
                    {
                        var subEmitterSystem = subEmitterPrefab.GetComponent<ParticleSystem>();
                        if (subEmitterSystem != null)
                        {
                            subEmitters.AddSubEmitter(subEmitterSystem, subEmitterType, ParticleSystemSubEmitterProperties.InheritNothing);
                        }
                    }
                }

                var spellEffect = go.AddComponent<RuneSpellEffect>();
                spellEffect.Initialize(runes);

                go.AddComponent<DestroyAfterParticles>();
                go.SetActive(true);
                ps.Play();
            }
            else
            {
                var parent = new GameObject("ProceduralSpell");
                parent.transform.position = position;
                parent.AddComponent<DestroyWhenNoChildren>();
                parent.SetActive(true);

                float perSystemRate = emissionRate / materialsForRune.Count;
                var scaledBursts = burstsCombined != null ? ScaleBursts(burstsCombined, 1f / materialsForRune.Count) : null;

                foreach (var mat in materialsForRune)
                {
                    var child = CreateParticleSystemObject("ProceduralSpell_Part", position, mat);
                    child.transform.SetParent(parent.transform, worldPositionStays: true);
                    child.SetActive(true);
                    var ps = child.GetComponent<ParticleSystem>();

                    var main = ps.main;
                    main.loop = false;
                    main.stopAction = ParticleSystemStopAction.Destroy;
                    main.startLifetime = startLifetime;
                    main.startSpeed = startSpeed;
                    main.startSize = startSize;
                    main.startRotation = startRotation;
                    main.gravityModifier = gravity;
                    main.duration = duration;

                    var emission = ps.emission;
                    emission.rateOverTime = perSystemRate;
                    if (scaledBursts != null)
                    {
                        ps.emission.SetBursts(scaledBursts);
                    }

                    var shape = ps.shape;
                    shape.shapeType = shapeType;
                    shape.radius = shapeRadius;
                    shape.angle = shapeAngle;
                    shape.randomDirectionAmount = randomDirAmount;

                    var col = ps.colorOverLifetime;
                    col.enabled = true;
                    col.color = gradient;

                    var sol = ps.sizeOverLifetime;
                    sol.enabled = true;
                    sol.size = sizeOverLifetimeCurve;

                    var rol = ps.rotationOverLifetime;
                    rol.enabled = true;
                    rol.z = rotationOverLifetimeCurve;

                    var vol = ps.velocityOverLifetime;
                    vol.enabled = true;
                    vol.x = velocityX;
                    vol.y = velocityY;
                    vol.z = velocityZ;

                    var noise = ps.noise;
                    noise.enabled = noiseEnabled;
                    if (noise.enabled)
                    {
                        noise.strength = noiseStrength;
                        noise.frequency = noiseFrequency;
                        noise.scrollSpeed = noiseScrollSpeed;
                    }

                    var trails = ps.trails;
                    trails.enabled = trailsEnabled;
                    if (trails.enabled)
                    {
                        var renderer = ps.GetComponent<ParticleSystemRenderer>();
                        if (defaultTrailMaterial != null)
                        {
                            renderer.trailMaterial = defaultTrailMaterial;
                        }
                        else
                        {
                            var trailShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                            if (trailShader == null) trailShader = Shader.Find("Particles/Standard Unlit");
                            if (trailShader == null) trailShader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
                            renderer.trailMaterial = new Material(trailShader);
                        }
                        trails.mode = ParticleSystemTrailMode.PerParticle;
                        trails.lifetime = new ParticleSystem.MinMaxCurve(trailLifetime);
                        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, trailWidth);
                        trails.colorOverTrail = trailGradient;
                    }

                    var collision = ps.collision;
                    collision.enabled = collisionEnabled;
                    if (collision.enabled)
                    {
                        collision.type = ParticleSystemCollisionType.World;
                        collision.mode = ParticleSystemCollisionMode.Collision3D;
                        collision.bounce = collisionBounce;
                        collision.lifetimeLoss = collisionLifetimeLoss;
                        collision.dampen = collisionDampen;
                        collision.sendCollisionMessages = true;
                    }

                    var texSheet = ps.textureSheetAnimation;
                    texSheet.enabled = texSheetEnabled;
                    if (texSheet.enabled)
                    {
                        texSheet.numTilesX = numTilesX;
                        texSheet.numTilesY = numTilesY;
                        texSheet.animation = animationType;
                        texSheet.frameOverTime = frameOverTime;
                    }

                    var lights = ps.lights;
                    lights.enabled = lightsEnabled;
                    if (lights.enabled)
                    {
                        lights.light = lightPrefab;
                        lights.ratio = lightRatio;
                        lights.range = lightRange;
                        lights.intensity = lightIntensity;
                    }

                    var subEmitters = ps.subEmitters;
                    subEmitters.enabled = subEmittersEnabled;
                    if (subEmitters.enabled)
                    {
                        if (subEmitterPrefab != null)
                        {
                            var subEmitterSystem = subEmitterPrefab.GetComponent<ParticleSystem>();
                            if (subEmitterSystem != null)
                            {
                                subEmitters.AddSubEmitter(subEmitterSystem, subEmitterType, ParticleSystemSubEmitterProperties.InheritNothing);
                            }
                        }
                    }

                    var spellEffect = child.AddComponent<RuneSpellEffect>();
                    spellEffect.Initialize(runes);

                    child.AddComponent<DestroyAfterParticles>();
                    ps.Play();
                }
            }
        }

        public void ThrowSpellProjectile(RuneData[] runes, Vector3 origin, Vector3 direction)
        {
            if (runes == null || !runes.Any(r => r.isProjectile)) return;

            var projectileRunes = runes.Where(r => r.isProjectile).ToArray();
            var dominantRune = projectileRunes.Last();

            if (direction.sqrMagnitude < 0.0001f) direction = transform.forward;
            direction.Normalize();

            GameObject projGO;
            RuneProjectile projectile;
            var prefab = dominantRune.projectilePrefab;

            if (prefab != null)
            {
                projGO = Instantiate(prefab, origin, Quaternion.LookRotation(direction));
                projectile = projGO.GetComponent<RuneProjectile>();
                if (projectile == null)
                {
                    projectile = projGO.AddComponent<RuneProjectile>();
                }
            }
            else
            {
                projGO = new GameObject("RuneProjectile");
                projGO.transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
                projectile = projGO.AddComponent<RuneProjectile>();
            }

            var renderer = projGO.GetComponentInChildren<ParticleSystemRenderer>();
            if (renderer != null)
            {
                var mats = ResolveMaterialsForRune(dominantRune);
                var particleMaterial = (mats != null && mats.Count > 0) ? mats[Random.Range(0, mats.Count)] : ResolveFallbackMaterial(null);
                renderer.material = particleMaterial;
            }

            float speed = projectileRunes.Average(r => r.projectileSpeed);
            float lifetime = projectileRunes.Average(r => r.projectileLifetime);
            bool useGravity = projectileRunes.Any(r => r.projectileUseGravity);
            float arc = projectileRunes.Average(r => r.projectileArc);

            var completedCombo = tomeManager?.GetCompletedCombo(runes);
            if (completedCombo != null)
            {
                speed *= completedCombo.bonusDamageMultiplier;
            }

            Vector3 velocity = direction * speed + Vector3.up * (speed * arc);
            projectile.Init(this, runes, velocity, speed, lifetime, useGravity);
        }

        private ParticleSystem.MinMaxCurve AverageMinMaxCurve(IEnumerable<ParticleSystem.MinMaxCurve> curves)
        {
            if (!curves.Any()) return new ParticleSystem.MinMaxCurve(0);
            float avgConstantMin = curves.Average(c => c.constantMin);
            float avgConstantMax = curves.Average(c => c.constantMax);
            return new ParticleSystem.MinMaxCurve(avgConstantMin, avgConstantMax);
        }

        private ParticleSystem.MinMaxGradient CreateBlendedGradient(IEnumerable<RuneData> runes)
        {
            var runeList = runes.ToList();
            if (!runeList.Any()) return new ParticleSystem.MinMaxGradient(Color.white);

            Color start = new Color(0, 0, 0, 0);
            Color end = new Color(0, 0, 0, 0);

            foreach (var rune in runeList)
            {
                start += rune.startColor;
                end += rune.colorOverLifetime.color;
            }

            start /= runeList.Count;
            end /= runeList.Count;

            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(start, 0.0f), new GradientColorKey(end, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(start.a, 0.0f), new GradientAlphaKey(end.a, 1.0f) }
            );

            return new ParticleSystem.MinMaxGradient(gradient);
        }

        private GameObject CreateParticleSystemObject(string name, Vector3 position, Material explicitMaterial)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.playOnAwake = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material particleMaterial = ResolveFallbackMaterial(explicitMaterial);
            renderer.material = particleMaterial;
            if (renderer.material.HasProperty("_Color")) renderer.material.color = Color.white;
            return go;
        }

        private ParticleSystem.MinMaxCurve PreserveCurveOrAverage(IEnumerable<RuneData> runes, System.Func<RuneData, ParticleSystem.MinMaxCurve> selector, RuneData dominantRune)
        {
            var list = runes.Select(selector).ToList();
            bool anyCurve = list.Any(c => c.mode == ParticleSystemCurveMode.Curve || c.mode == ParticleSystemCurveMode.TwoCurves);
            if (anyCurve) return selector(dominantRune);
            return AverageMinMaxCurve(list);
        }

        private ParticleSystem.MinMaxCurve PreserveCurveOrAverage(IEnumerable<ParticleSystem.MinMaxCurve> curves, ParticleSystem.MinMaxCurve fallbackFromDominant)
        {
            var list = curves.ToList();
            bool anyCurve = list.Any(c => c.mode == ParticleSystemCurveMode.Curve || c.mode == ParticleSystemCurveMode.TwoCurves);
            if (anyCurve) return fallbackFromDominant;
            return AverageMinMaxCurve(list);
        }

        private Material ResolveFallbackMaterial(Material candidate)
        {
            if (candidate != null) return candidate;
            if (defaultParticleMaterial != null) return defaultParticleMaterial;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            return new Material(shader);
        }

        private List<Material> ResolveMaterialsForRune(RuneData rune)
        {
            var result = new List<Material>();
            if (rune == null) return result;

            if (rune.useRandomFromMaterialList && rune.overrideMaterials != null && rune.overrideMaterials.Count > 0)
            {
                foreach (var m in rune.overrideMaterials)
                {
                    if (m != null) result.Add(m);
                }
            }
            else if (rune.overrideMaterial != null)
            {
                result.Add(rune.overrideMaterial);
            }
            else if (defaultParticleMaterial != null)
            {
                result.Add(defaultParticleMaterial);
            }

            return result;
        }

        private ParticleSystem.Burst[] ScaleBursts(ParticleSystem.Burst[] bursts, float scale)
        {
            if (bursts == null) return null;
            var scaled = new ParticleSystem.Burst[bursts.Length];
            for (int i = 0; i < bursts.Length; i++)
            {
                var b = bursts[i];
                var count = b.count;
                var scaledCount = new ParticleSystem.MinMaxCurve(count.constantMin * scale, count.constantMax * scale);
                var nb = new ParticleSystem.Burst(b.time, scaledCount)
                {
                    cycleCount = b.cycleCount,
                    repeatInterval = b.repeatInterval,
                    probability = b.probability
                };
                scaled[i] = nb;
            }
            return scaled;
        }
    }
}