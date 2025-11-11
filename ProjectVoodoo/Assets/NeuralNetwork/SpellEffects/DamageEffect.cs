using UnityEngine;
using System.Linq;

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
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float totalDamage = sourceRunes.Where(r => r.collisionEnabled).Average(r => r.damage);

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
                    foreach (var rune in sourceRunes)
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
                damageable.TakeDamage(totalDamage);
            }
            else
            {
                Debug.LogWarning($"Target {target.name} does not have an IDamageable component.");
            }
        }
    }
}