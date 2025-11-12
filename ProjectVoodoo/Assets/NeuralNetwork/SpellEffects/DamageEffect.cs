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
        public bool validateTargetBeforeDamage = true;

        public override void Apply(GameObject target, RuneData[] sourceRunes)
        {
            if (target == null)
            {
                return;
            }

            if (sourceRunes == null || sourceRunes.Length == 0)
            {
            }

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
                    }
                    if (resistances.resistances.Contains(rune.runeType))
                    {
                        totalDamage *= resistances.resistanceMultiplier;
                    }
                }
            }

            bool handled = false;

            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(totalDamage);
                handled = true;
            }
            else
            {
                var mono = target.GetComponents<MonoBehaviour>()
                                 .FirstOrDefault(m => m != null && m.GetType()
                                    .GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                               null, new[] { typeof(float) }, null) != null);

                if (mono != null)
                {
                    if (validateTargetBeforeDamage && !ValidateTargetComponent(mono))
                    {
                        return;
                    }

                    try
                    {
                        var method = mono.GetType().GetMethod("TakeDamage", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                              null, new[] { typeof(float) }, null);
                        method?.Invoke(mono, new object[] { totalDamage });
                        handled = true;
                    }
                    catch (System.Reflection.TargetInvocationException ex)
                    {
                    }
                    catch (System.Exception ex)
                    {
                    }
                }

                if (!handled)
                {
                    target.SendMessage("TakeDamage", totalDamage, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

        private bool ValidateTargetComponent(MonoBehaviour component)
        {
            if (component == null) return false;

            var type = component.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(component) as UnityEngine.Object;

                    if (value == null && !IsFieldAllowedToBeNull(field))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsFieldAllowedToBeNull(FieldInfo field)
        {
            var attrs = field.GetCustomAttributes(typeof(System.Runtime.InteropServices.OptionalAttribute), false);
            if (attrs.Length > 0) return true;

            return false;
        }
    }
}