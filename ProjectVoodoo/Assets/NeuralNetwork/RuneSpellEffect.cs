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
        }

        private void OnParticleCollision(GameObject other)
        {
            if (Runes == null || !Runes.Any())
            {
                return;
            }

            var allEffects = Runes
                .Where(r => r.collisionEnabled && r.onHitEffects != null)
                .SelectMany(r => r.onHitEffects)
                .Where(e => e != null)
                .Distinct()
                .ToList();

            foreach (var effect in allEffects)
            {
                if (effect != null)
                {
                    effect.Apply(other, Runes);
                }
            }
        }
    }
}