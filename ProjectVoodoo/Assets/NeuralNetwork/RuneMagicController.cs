using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
            var go = CreateParticleSystemObject("ProceduralSpell", position, dominantRune);
            var ps = go.GetComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = false;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.startLifetime = PreserveCurveOrAverage(runes, r => r.startLifetime, dominantRune);
            main.startSpeed = PreserveCurveOrAverage(runes, r => r.startSpeed, dominantRune);
            main.startSize = PreserveCurveOrAverage(runes, r => r.startSize, dominantRune);
            main.startRotation = PreserveCurveOrAverage(runes, r => r.startRotation, dominantRune);
            main.gravityModifier = runes.Average(r => r.gravityModifier);
            main.duration = runes.Max(r => r.systemDuration);

            var completedCombo = tomeManager?.GetCompletedCombo(runes);
            var emission = ps.emission;
            emission.rateOverTime = runes.Average(r => r.emissionRate) * (completedCombo != null ? completedCombo.bonusDamageMultiplier : 1f);

            if (runes.Any(r => r.bursts != null && r.bursts.Length > 0))
            {
                ps.emission.SetBursts(runes.SelectMany(r => r.bursts).ToArray());
            }

            var shape = ps.shape;
            shape.shapeType = dominantRune.shapeType;
            shape.radius = dominantRune.shapeRadius;
            shape.angle = dominantRune.shapeAngle;
            shape.randomDirectionAmount = dominantRune.randomizeDirection ? dominantRune.randomizeDirectionAmount : 0f;

            var col = ps.colorOverLifetime;
            col.enabled = true;
            col.color = CreateBlendedGradient(runes);

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, dominantRune.sizeOverLifetime);

            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.z = PreserveCurveOrAverage(runes, r => r.rotationOverLifetime, dominantRune);

            var vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.x = PreserveCurveOrAverage(runes, r => r.velocityX, dominantRune);
            vol.y = PreserveCurveOrAverage(runes, r => r.velocityY, dominantRune);
            vol.z = PreserveCurveOrAverage(runes, r => r.velocityZ, dominantRune);

            var noise = ps.noise;
            noise.enabled = runes.Any(r => r.noiseEnabled);
            if (noise.enabled)
            {
                noise.strength = runes.Where(r => r.noiseEnabled).Average(r => r.noiseStrength);
                noise.frequency = runes.Where(r => r.noiseEnabled).Average(r => r.noiseFrequency);
                noise.scrollSpeed = runes.Where(r => r.noiseEnabled).Average(r => r.noiseScrollSpeed);
            }

            var trails = ps.trails;
            trails.enabled = runes.Any(r => r.trailsEnabled);
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
                trails.lifetime = new ParticleSystem.MinMaxCurve(runes.Where(r => r.trailsEnabled).Average(r => r.trailLifetime));
                trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, runes.Where(r => r.trailsEnabled).Average(r => r.trailWidth));
                trails.colorOverTrail = CreateBlendedGradient(runes.Where(r => r.trailsEnabled));
            }

            var collision = ps.collision;
            collision.enabled = runes.Any(r => r.collisionEnabled);
            if (collision.enabled)
            {
                collision.type = ParticleSystemCollisionType.World;
                collision.mode = ParticleSystemCollisionMode.Collision3D;
                collision.bounce = PreserveCurveOrAverage(runes.Where(r => r.collisionEnabled).Select(r => r.bounce), dominantRune.bounce);
                collision.lifetimeLoss = PreserveCurveOrAverage(runes.Where(r => r.collisionEnabled).Select(r => r.lifetimeLoss), dominantRune.lifetimeLoss);
                collision.dampen = runes.Where(r => r.collisionEnabled).Average(r => r.dampen);
                collision.sendCollisionMessages = true;
            }

            var texSheet = ps.textureSheetAnimation;
            texSheet.enabled = runes.Any(r => r.textureSheetEnabled);
            if (texSheet.enabled)
            {
                texSheet.numTilesX = dominantRune.numTilesX;
                texSheet.numTilesY = dominantRune.numTilesY;
                texSheet.animation = dominantRune.animationType;
                texSheet.frameOverTime = PreserveCurveOrAverage(runes.Where(r => r.textureSheetEnabled).Select(r => r.frameOverTime), dominantRune.frameOverTime);
            }

            var lights = ps.lights;
            lights.enabled = runes.Any(r => r.lightsEnabled);
            if (lights.enabled)
            {
                lights.light = dominantRune.lightPrefab;
                lights.ratio = runes.Where(r => r.lightsEnabled).Average(r => r.lightRatio);
                lights.range = runes.Where(r => r.lightsEnabled).Average(r => r.lightRange);
                lights.intensity = runes.Where(r => r.lightsEnabled).Average(r => r.lightIntensity);
            }

            var subEmitters = ps.subEmitters;
            subEmitters.enabled = runes.Any(r => r.subEmittersEnabled);
            if (subEmitters.enabled)
            {
                if (dominantRune.subEmitterPrefab != null)
                {
                    var subEmitterSystem = dominantRune.subEmitterPrefab.GetComponent<ParticleSystem>();
                    if (subEmitterSystem != null)
                    {
                        subEmitters.AddSubEmitter(subEmitterSystem, dominantRune.subEmitterType, ParticleSystemSubEmitterProperties.InheritNothing);
                    }
                }
            }

            var spellEffect = go.AddComponent<RuneSpellEffect>();
            spellEffect.Initialize(runes);

            go.AddComponent<DestroyAfterParticles>();
            go.SetActive(true);
            ps.Play();
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
                Material particleMaterial = DrawingController.GetMaterialForRune(dominantRune, defaultParticleMaterial);
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

        private GameObject CreateParticleSystemObject(string name, Vector3 position, RuneData dominantRune)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.playOnAwake = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material particleMaterial = DrawingController.GetMaterialForRune(dominantRune, defaultParticleMaterial);
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
    }
}