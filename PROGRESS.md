# PROGRESS — 3D typing/translation hra

> Stav pre ďalšiu session (aj pre AI agenta). Po väčšom kroku ho aktualizuj.
> Kontext, pravidlá a konvencie: `AGENTS.md`. Deadline: 10. 12.

## Hotové ✅

- [x] Core loop prototyp — `Assets/Scripts/Gameplay/TypingGameController.cs`:
      písanie SK→EN, skok postavičky, stúpajúca kamera, smrť pod kamerou, skóre, game over.
- [x] Word banky A1–C2 — `Assets/WordBanks/WordBank_A1.asset` … `_C2.asset` (**420 párov**)
      + SO trieda `WordBank` v `Assets/Scripts/Data/WordBank.cs` (`CefrLevel` enum, `Words`).

| Banka | Párov |
|---|---|
| A1 | 60 |
| A2 | 80 |
| B1 | 100 |
| B2 | 100 |
| C1 | 50 |
| C2 | 30 |

## Next — v tomto poradí 🔜

1. **Napojiť banky do controllera**: nahradiť hardcoded `Words[]` poľom `WordBank[]` v inspektore
   (alebo jedným poľom + level), výber banky podľa výšky, random bez opakovania v rámci kola.
2. **Nápovedy podľa levelu**: dashes + `hintCount` (A1 ~2 písmená → C2 0) v `wordLabel`.
3. **Časový limit na slovo** podľa levelu (A1 ~8 s → C2 ~4 s), smrť pri vypršaní.
4. **EN→SK mód** (prepínač) + tolerantné porovnávanie (lowercase, bez diakritiky).
5. **Polish**: zvuk skoku/level-up, particles, pozadie.

## Poznámky 📌

- Banky sú len `sk` (prompt) + `en` (odpoveď). EN→SK mód = swap polí, netreba druhú sadu.
- Žiadne viacslovné frázy, žiadne duplicitné SK/EN naprieč bankami (overené skriptom).
- C1/C2 sú malé (50/30) a slová sú dlhé → "bonus" pri vysokom leveli, nie hlavná hracia plocha.
- Obtiažnosť = **banka + hintCount + timeLimit**, banka sama ju nerieši.
- Dátový súbor sa generoval dočasným Editor skriptom, ktorý je už zmazaný; banky sa už needitujú skriptom,
  len v Inspectori alebo podľa potreby novým jednorazovým builderom.
