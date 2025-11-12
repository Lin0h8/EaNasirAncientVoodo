using System.Linq;
using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    [RequireComponent(typeof(ParticleSystem))]
    public class RuneSpellEffect : MonoBehaviour
    {
        public RuneData[] Runes { get; private set; }

        public void Initialize(RuneData[] runes)
        {
            Runes = runes;
            Debug.Log($"RuneSpellEffect initialized with {runes?.Length ?? 0} runes");

            if (runes != null)
            {
                foreach (var rune in runes)
                {
                    Debug.Log($"Rune: {rune.runeType}, CollisionEnabled: {rune.collisionEnabled}, " +
                             $"OnHitEffects: {rune.onHitEffects?.Length ?? 0}, Damage: {rune.damage}");
                }
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            Debug.Log($"Particle collision detected with: {other.name}");

            if (Runes == null || !Runes.Any())
            {
                Debug.LogWarning("No runes assigned to RuneSpellEffect!");
                return;
            }

            var allEffects = Runes
                .Where(r => r.collisionEnabled && r.onHitEffects != null)
                .SelectMany(r => r.onHitEffects)
                .Where(e => e != null)
                .Distinct()
                .ToList();

            Debug.Log($"Found {allEffects.Count} effects to apply to {other.name}");

            foreach (var effect in allEffects)
            {
                if (effect != null)
                {
                    Debug.Log($"Applying effect: {effect.effectName} to {other.name}");
                    effect.Apply(other, Runes);
                }
            }
        }
    }
}