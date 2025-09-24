using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpellCastingManager : MonoBehaviour
{
    public List<RuneData> allRunes;
    private Dictionary<RuneType, RuneData> runeDictionary;
    public StrokeInput StrokeInput;
    public SpellFXGenerator spellFXGenerator;
    public BookInput bookInput;
    private List<RuneType> currentRunes = new List<RuneType>();
    private bool spellReady = false;
    public SaySpellName saySpellName;

    public void Awake()
    {
        runeDictionary = new Dictionary<RuneType, RuneData>();
        foreach (var rune in allRunes)
        {
            if (!runeDictionary.ContainsKey(rune.Type))
            {
                runeDictionary.Add(rune.Type, rune);
            }
            else
            {
                Debug.LogWarning($"Duplicate rune type found: {rune.Type}");
            }
        }
    }

    private RuneData[] GetRuneDatas()
    {
        List<RuneData> runeDatas = new List<RuneData>();
        foreach (var runeType in currentRunes)
        {
            if (runeDictionary.TryGetValue(runeType, out RuneData runeData))
            {
                runeDatas.Add(runeData);
            }
            else
            {
                Debug.LogWarning($"Rune data not found for type: {runeType}");
            }
        }
        return runeDatas.ToArray();
    }

    public void RegisterRune(string runeName)
    {
        if (System.Enum.TryParse(runeName, out RuneType runeType))
        {
            currentRunes.Add(runeType);
            if (currentRunes.Count >= 3)
            {
                PrepareSpell();
            }
            Debug.Log($"Registered rune: {runeType}");
        }
        else
        {
            Debug.LogWarning($"Unrecognized rune name: {runeName}");
        }
    }

    public void ClearRunes()
    {
        currentRunes.Clear();
        Debug.Log("Cleared all registered runes.");
    }

    private void PrepareSpell()
    {
        spellReady = true;
        Debug.Log("Spell is prepared with runes:" + string.Join(", ", currentRunes));
        bookInput.isBookOpen = false;
    }

    private void Update()
    {
        if (!bookInput.isBookOpen && spellReady && Input.GetMouseButtonDown(0))
        {
            CastSpell();
        }
    }

    private void CastSpell()
    {
        ParticleSystem spellFX = spellFXGenerator.GenerateSpellFX(GetRuneDatas());
        if (spellFX != null)
        {
            spellFX.transform.position = transform.position + transform.forward * 2f;
            spellFX.transform.rotation = Quaternion.LookRotation(transform.forward);
            spellFX.Play();
        }
        saySpellName.PlayRuneVoices(GetRuneDatas());
        ClearRunes();
    }
}