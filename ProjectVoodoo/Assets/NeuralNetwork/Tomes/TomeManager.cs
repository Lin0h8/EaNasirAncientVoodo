using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetwork_IHNMAIMS
{
    [CreateAssetMenu(fileName = "New Tome Manager", menuName = "Rune System/Tome Manager")]
    public class TomeManager : ScriptableObject
    {
        public List<RuneCombo> allCombos;

        private void OnEnable()
        {
            foreach (var combo in allCombos)
            {
                combo.isUnlocked = false;
            }
        }

        public void UnlockRandomTome()
        {
            var lockedCombos = allCombos.Where(c => !c.isUnlocked).ToList();
            if (lockedCombos.Any())
            {
                var comboToUnlock = lockedCombos[Random.Range(0, lockedCombos.Count)];
                comboToUnlock.isUnlocked = true;
                Debug.Log($"Unlocked Tome: {comboToUnlock.comboName}");
            }
        }

        public List<RuneType> GetNextRuneSuggestions(List<RuneData> currentSequence)
        {
            var suggestions = new List<RuneType>();
            if (currentSequence == null || currentSequence.Count == 0) return suggestions;

            var currentRuneTypes = currentSequence.Select(r => r.runeType).ToList();

            foreach (var combo in allCombos.Where(c => c.isUnlocked))
            {
                if (currentSequence.Count < combo.runeSequence.Count)
                {
                    var sequenceMatches = true;
                    for (int i = 0; i < currentSequence.Count; i++)
                    {
                        if (currentRuneTypes[i] != combo.runeSequence[i])
                        {
                            sequenceMatches = false;
                            break;
                        }
                    }

                    if (sequenceMatches)
                    {
                        suggestions.Add(combo.runeSequence[currentSequence.Count]);
                    }
                }
            }
            return suggestions.Distinct().ToList();
        }

        public RuneCombo GetCompletedCombo(RuneData[] sequence)
        {
            if (sequence == null || sequence.Length == 0) return null;
            var currentRuneTypes = sequence.Select(r => r.runeType).ToList();

            foreach (var combo in allCombos.Where(c => c.isUnlocked))
            {
                if (currentRuneTypes.SequenceEqual(combo.runeSequence))
                {
                    return combo;
                }
            }
            return null;
        }
    }
}