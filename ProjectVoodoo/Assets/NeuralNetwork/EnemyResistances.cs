using UnityEngine;
using System.Collections.Generic;

namespace NeuralNetwork_IHNMAIMS
{
    public class EnemyResistances : MonoBehaviour
    {
        [Tooltip("Rune types this enemy is resistant to. Damage will be reduced.")]
        public List<RuneType> resistances;

        [Tooltip("Rune types this enemy is weak to. Damage will be increased.")]
        public List<RuneType> weaknesses;

        [Tooltip("How much to reduce damage by for resistances. E.g., 0.5 for 50% damage.")]
        [Range(0f, 1f)]
        public float resistanceMultiplier = 0.5f;

        [Tooltip("How much to increase damage by for weaknesses. E.g., 1.5 for 150% damage.")]
        [Min(1f)]
        public float weaknessMultiplier = 1.5f;
    }
}