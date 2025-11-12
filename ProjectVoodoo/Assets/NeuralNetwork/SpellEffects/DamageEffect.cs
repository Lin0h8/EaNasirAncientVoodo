using System.Linq;
using System.Reflection;
using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
    }

    [CreateAssetMenu(fileName = "New Damage Effect", menuName = "Rune System/Spell Effects/Damage")]
    public class DamageEffect : SpellEffect
    {
        public float baseDamage = 10f;

        public override void Apply(GameObject target, RuneData[] sourceRunes)
        {
            if (target == null)
            {
                Debug.LogWarning("DamageEffect.Apply called with null target.");
                return;
            }

            // Defensive: ensure sourceRunes isn't null
            if (sourceRunes == null || sourceRunes.Length == 0)
            {
                Debug.LogWarning("DamageEffect.Apply called with no sourceRunes; applying baseDamage.");
            }

            // Compute totalDamage safely; fall back to baseDamage when no collision-enabled runes exist
            var collisionDamages = (sourceRunes ?? Enumerable.Empty<RuneData>())
                                    .Where(r => r.collisionEnabled)
                                    .Select(r => r.damage);

            float totalDamage = collisionDamages.Any() ? collisionDamages.Average() : baseDamage;

            var tomeManager = FindFirstObjectByType<TomeManager>();
            if (tomeManager != null)
            {
                var completedCombo = tomeManager.GetCompletedCombo(sourceRunes);
                if (completedCombo != null)
                {
                    Debug.Log($"Damage boosted by combo: {completedCombo.comboName}!");
                    totalDamage *= completedCombo.bonusDamageMultiplier;
                }
            }

            if (target.TryGetComponent<EnemyResistances>(out var resistances))
            {
                foreach (var rune in sourceRunes ?? Enumerable.Empty<RuneData>())
                {
                    if (resistances.weaknesses.Contains(rune.runeType))
                    {
                        totalDamage *= resistances.weaknessMultiplier;
                        Debug.Log($"Damage multiplied by weakness to {rune.runeType}!");
                    }
                    if (resistances.resistances.Contains(rune.runeType))
                    {
                        totalDamage *= resistances.resistanceMultiplier;
                        Debug.Log($"Damage reduced by resistance to {rune.runeType}!");
                    }
                }
            }

            Debug.Log($"Applying {totalDamage} damage to {target.name}");

            bool handled = false;

            // Fast path: interface implementation (preferred)
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(totalDamage);
                Debug.Log($"Damage handled via IDamageable on {target.name}");
                handled = true;
            }
            else
            {
                // Fallback 1: reflection — find a MonoBehaviour with TakeDamage(float)
                var mono = target.GetComponents<MonoBehaviour>()
                                 .FirstOrDefault(m => m != null && m.GetType()
                                    .GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                               null, new[] { typeof(float) }, null) != null);

                if (mono != null)
                {
                    var method = mono.GetType().GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                          null, new[] { typeof(float) }, null);
                    method?.Invoke(mono, new object[] { totalDamage });
                    Debug.Log($"Damage handled via reflection on component {mono.GetType().Name} of {target.name}");
                    handled = true;
                }
            }

            // Fallback 2: SendMessage (string-based)
            if (!handled)
            {
                // SendMessage doesn't indicate success; we log intent and then warn if nothing likely handled it.
                target.SendMessage("TakeDamage", totalDamage, SendMessageOptions.DontRequireReceiver);
                Debug.Log($"SendMessage(\"TakeDamage\") sent to {target.name} (may or may not have been handled)");
            }

            if (!handled)
            {
                // Optionally look for a known concrete component type (example: Target)
                // var concrete = target.GetComponent<Target>();
                // if (concrete != null) { concrete.TakeDamage(totalDamage); handled = true; }

                // If still not handled, emit a warning so you can see it in Console.
                Debug.LogWarning($"No explicit damage handler found on {target.name}. Check that the target has a component with TakeDamage(float) or implements IDamageable.");
            }
        }
    }
}