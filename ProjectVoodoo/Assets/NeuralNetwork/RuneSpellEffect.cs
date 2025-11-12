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
            var ps = GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var col = ps.collision;
                Debug.Log($"[RuneSpellEffect] OnParticleCollision called. PS collision: enabled={col.enabled}, sendCollisionMessages={col.sendCollisionMessages}, type={col.type}");
            }
            Debug.Log($"[RuneSpellEffect] OnParticleCollision hit: other={other?.name ?? "null"}; RunesNull={Runes == null}; RunesCount={(Runes?.Length ?? 0)}");

            if (Runes == null || !Runes.Any())
            {
                Debug.Log("[RuneSpellEffect] No runes set or empty runes — skipping effects.");
                return;
            }

            var allEffects = Runes
                .Where(r => r.collisionEnabled && r.onHitEffects != null)
                .SelectMany(r => r.onHitEffects)
                .Where(e => e != null)
                .Distinct()
                .ToList();

            Debug.Log($"[RuneSpellEffect] Computed on-hit effects count = {allEffects.Count}");

            if (allEffects.Count == 0)
            {
                Debug.Log("[RuneSpellEffect] No onHitEffects found on collision-enabled runes.");
            }

            foreach (var effect in allEffects)
            {
                if (effect != null)
                {
                    Debug.Log($"[RuneSpellEffect] Applying effect '{effect.name ?? effect.GetType().Name}' to {other.name}");
                    effect.Apply(other, Runes);
                }
            }
        }
    }
}