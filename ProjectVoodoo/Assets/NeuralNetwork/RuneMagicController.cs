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

            float speed = projectileRunes.Average(r => r.projectileSpeed);
            float lifetime = projectileRunes.Average(r => r.projectileLifetime);
            bool useGravity = projectileRunes.Any(r => r.projectileUseGravity);
            float arc = projectileRunes.Average(r => r.projectileArc);

            var completedCombo = tomeManager?.GetCompletedCombo(runes);
            if (completedCombo != null)
            {
                Debug.Log($"Casting with unlocked combo: {completedCombo.comboName}! Applying bonus.");
                speed *= completedCombo.bonusDamageMultiplier;
            }

            Vector3 velocity = direction * speed + Vector3.up * (speed * arc);
            projectile.Init(this, runes, velocity, speed, lifetime, useGravity);
        }

        public void GenerateSpell(RuneData[] runes, Vector3 position)
        {
            if (runes == null || runes.Length == 0) return;

            var go = CreateParticleSystemObject("ProceduralSpell", position);
            var ps = go.GetComponent<ParticleSystem>();
            var dominantRune = runes.Last();

            var main = ps.main;
            main.startLifetime = AverageMinMaxCurve(runes.Select(r => r.startLifetime));
            main.startSpeed = AverageMinMaxCurve(runes.Select(r => r.startSpeed));
            main.startSize = AverageMinMaxCurve(runes.Select(r => r.startSize));
            main.startRotation = AverageMinMaxCurve(runes.Select(r => r.startRotation));
            main.gravityModifier = runes.Average(r => r.gravityModifier);
            main.startColor = new ParticleSystem.MinMaxGradient(AverageColor(runes.Select(r => r.startColor)));
            main.duration = 2.0f;

            var completedCombo = tomeManager?.GetCompletedCombo(runes);
            if (completedCombo != null)
            {
                Debug.Log($"Casting with unlocked combo: {completedCombo.comboName}! Applying bonus.");
                var emission = ps.emission;
                emission.rateOverTime = runes.Average(r => r.emissionRate) * completedCombo.bonusDamageMultiplier;
            }
            else
            {
                var emission = ps.emission;
                emission.rateOverTime = runes.Average(r => r.emissionRate);
            }

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
            col.color = AverageMinMaxGradient(runes.Select(r => r.colorOverLifetime));

            var sol = ps.sizeOverLifetime;
            sol.enabled = true;
            sol.size = new ParticleSystem.MinMaxCurve(1f, dominantRune.sizeOverLifetime);

            var rol = ps.rotationOverLifetime;
            rol.enabled = true;
            rol.z = AverageMinMaxCurve(runes.Select(r => r.rotationOverLifetime));

            var vol = ps.velocityOverLifetime;
            vol.enabled = true;
            vol.x = AverageMinMaxCurve(runes.Select(r => r.velocityX));
            vol.y = AverageMinMaxCurve(runes.Select(r => r.velocityY));
            vol.z = AverageMinMaxCurve(runes.Select(r => r.velocityZ));

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
                renderer.trailMaterial = defaultTrailMaterial;
                trails.mode = ParticleSystemTrailMode.PerParticle;
                trails.lifetime = new ParticleSystem.MinMaxCurve(runes.Where(r => r.trailsEnabled).Average(r => r.trailLifetime));
                trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, runes.Where(r => r.trailsEnabled).Average(r => r.trailWidth));
                trails.colorOverTrail = AverageMinMaxGradient(runes.Where(r => r.trailsEnabled).Select(r => r.trailColor));
            }

            var collision = ps.collision;
            collision.enabled = runes.Any(r => r.collisionEnabled);
            if (collision.enabled)
            {
                collision.type = ParticleSystemCollisionType.World;
                collision.mode = ParticleSystemCollisionMode.Collision3D;
                collision.bounce = AverageMinMaxCurve(runes.Where(r => r.collisionEnabled).Select(r => r.bounce));
                collision.lifetimeLoss = AverageMinMaxCurve(runes.Where(r => r.collisionEnabled).Select(r => r.lifetimeLoss));
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
                texSheet.frameOverTime = AverageMinMaxCurve(runes.Where(r => r.textureSheetEnabled).Select(r => r.frameOverTime));
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

        private GameObject CreateParticleSystemObject(string name, Vector3 position)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.position = position;
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.playOnAwake = false;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = defaultParticleMaterial;
            return go;
        }

        private ParticleSystem.MinMaxCurve AverageMinMaxCurve(IEnumerable<ParticleSystem.MinMaxCurve> curves)
        {
            if (!curves.Any()) return new ParticleSystem.MinMaxCurve(0);
            float avgConstantMin = curves.Average(c => c.constantMin);
            float avgConstantMax = curves.Average(c => c.constantMax);
            return new ParticleSystem.MinMaxCurve(avgConstantMin, avgConstantMax) { mode = curves.First().mode };
        }

        private Color AverageColor(IEnumerable<Color> colors)
        {
            if (!colors.Any()) return Color.white;
            float r = 0, g = 0, b = 0, a = 0;
            foreach (var c in colors)
            {
                r += c.r;
                g += c.g;
                b += c.b;
                a += c.a;
            }
            int count = colors.Count();
            return new Color(r / count, g / count, b / count, a / count);
        }

        private ParticleSystem.MinMaxGradient AverageMinMaxGradient(IEnumerable<ParticleSystem.MinMaxGradient> gradients)
        {
            if (!gradients.Any()) return new ParticleSystem.MinMaxGradient(Color.white);
            Color avgColorMin = Color.black;
            Color avgColorMax = Color.black;
            foreach (var grad in gradients)
            {
                avgColorMin += grad.colorMin;
                avgColorMax += grad.colorMax;
            }
            avgColorMin /= gradients.Count();
            avgColorMax /= gradients.Count();
            return new ParticleSystem.MinMaxGradient(avgColorMin, avgColorMax) { mode = gradients.First().mode };
        }
    }
}