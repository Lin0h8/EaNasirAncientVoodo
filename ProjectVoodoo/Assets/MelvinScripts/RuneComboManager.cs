using UnityEngine;

public class RuneComboManager : MonoBehaviour
{
    public RuneCombo[] runeCombos;

    public RuneCombo GetMatchingCombo(RuneType[] inputRunes)
    {
        foreach (var combo in runeCombos)
        {
            if (combo.Sequence.Length != inputRunes.Length) continue;
            bool isMatch = true;
            for (int i = 0; i < combo.Sequence.Length; i++)
            {
                if (combo.Sequence[i] != inputRunes[i])
                {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch)
            {
                return combo;
            }
        }
        return null;
    }
}