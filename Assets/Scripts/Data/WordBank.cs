using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CEFR word bank: Slovak prompt + English translation pairs.
/// One asset per level lives in Assets/WordBanks/.
/// </summary>
[CreateAssetMenu(fileName = "WordBank_A1", menuName = "TypingGame/Word Bank")]
public class WordBank : ScriptableObject
{
    public enum CefrLevel
    {
        A1 = 0,
        A2 = 1,
        B1 = 2,
        B2 = 3,
        C1 = 4,
        C2 = 5,
    }

    [Serializable]
    public struct WordEntry
    {
        public string sk;
        public string en;
    }

    [SerializeField] private CefrLevel level = CefrLevel.A1;
    [SerializeField] private List<WordEntry> words = new List<WordEntry>();

    public CefrLevel Level => level;
    public IReadOnlyList<WordEntry> Words => words;
    public int Count => words.Count;
}