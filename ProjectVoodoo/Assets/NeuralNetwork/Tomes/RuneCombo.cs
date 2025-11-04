using UnityEngine;
using System.Collections.Generic;
using NeuralNetwork_IHNMAIMS;

namespace NeuralNetwork_IHNMAIMS
{
    [CreateAssetMenu(fileName = "New Rune Combo", menuName = "Rune System/Rune Combo")]
    public class RuneCombo : ScriptableObject
    {
        public string comboName;
        public List<RuneType> runeSequence;
        public bool isUnlocked;
        public float bonusDamageMultiplier = 1.5f;
    }
}