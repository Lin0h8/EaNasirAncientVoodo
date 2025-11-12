using UnityEngine;
using System.Linq;

namespace NeuralNetwork_IHNMAIMS
{
    [CreateAssetMenu(fileName = "New Heal Effect", menuName = "Rune System/Spell Effects/Heal")]
    public class HealEffect : SpellEffect
    {
        public float baseHealAmount = 15f;

        public override void Apply(GameObject target, RuneData[] sourceRunes)
        {
            var healable = target.GetComponent<IHealable>();
            if (healable != null)
            {
                healable.Heal(baseHealAmount);
            }
        }
    }

    public interface IHealable
    {
        void Heal(float amount);
    }
}
