# PROGRESS — 3D typing/translation hra

> Stav pre ďalšiu session (aj pre AI agenta). Po väčšom kroku ho aktualizuj.
> Kontext, pravidlá a konvencie: `AGENTS.md`. Deadline: 10. 12.

## Hotové ✅

- [x] Core loop prototyp — `Assets/Scripts/Gameplay/TypingGameController.cs`:
      písanie SK→EN, skok postavičky, stúpajúca kamera, smrť pod kamerou, skóre, game over.
- [x] Word banky A1–C2 — `Assets/WordBanks/WordBank_A1.asset` … `_C2.asset` (**420 párov**)
      + SO trieda `WordBank` v `Assets/Scripts/Data/WordBank.cs` (`CefrLevel` enum, `Words`).
- [x] Banky napojené do controllera — `WordBank[]` v inspektore (A1→C2), výber banky podľa
      postupu (2 platformy na level), random bez opakovania v rámci banky.
- [x] UI polish — level badge + pulse, high score (PlayerPrefs), restart R, červený flash,
      screen shake pri zlom slove, ESC pauza.

| Banka | Párov |
|---|---|
| A1 | 60 |
| A2 | 80 |
| B1 | 100 |
| B2 | 100 |
| C1 | 50 |
| C2 | 30 |

## Next — v tomto poradí 🔜

1. **Nápovedy podľa levelu**: dashes + `hintCount` (A1 ~2 písmená → C2 0) v `wordLabel`.
2. **Časový limit na slovo** podľa levelu (A1 ~8 s → C2 ~4 s), smrť pri vypršaní.
3. **EN→SK mód** (prepínač) + tolerantné porovnávanie (lowercase, bez diakritiky).
4. **Polish**: zvuk skoku/level-up, particles, pozadie.

## Poznámky 📌

- Banky sú len `sk` (prompt) + `en` (odpoveď). EN→SK mód = swap polí, netreba druhú sadu.
- Žiadne viacslovné frázy, žiadne duplicitné SK/EN naprieč bankami (overené skriptom).
- C1/C2 sú malé (50/30) a slová sú dlhé → "bonus" pri vysokom leveli, nie hlavná hracia plocha.
- Obtiažnosť = **banka + hintCount + timeLimit**, banka sama ju nerieši.
- Dátový súbor sa generoval dočasným Editor skriptom, ktorý je už zmazaný; banky sa už needitujú skriptom,
  len v Inspectori alebo podľa potreby novým jednorazovým builderom.
