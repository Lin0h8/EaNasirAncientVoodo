using UnityEngine;

namespace NeuralNetwork_IHNMAIMS
{
    public abstract class SpellEffect : ScriptableObject
    {
        public string effectName;

        public abstract void Apply(GameObject target, RuneData[] sourceRunes);
    }
}
