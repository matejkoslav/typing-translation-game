# PROGRESS — 3D typing/translation hra

> Stav pre ďalšiu session (aj pre AI agenta). Po väčšom kroku ho aktualizuj.
> Kontext, pravidlá a konvencie: `AGENTS.md`. Deadline: 10. 12.

## Hotové ✅

- [x] Core loop prototyp — `Assets/Scripts/Gameplay/TypingGameController.cs`:
      písanie SK↔EN, skok postavičky, stúpajúca kamera, smrť pod kamerou, skóre, game over.
- [x] Word banky A1–C2 — `Assets/WordBanks/WordBank_A1.asset` … `_C2.asset` (**420 párov**)
      + SO trieda `WordBank` v `Assets/Scripts/Data/WordBank.cs` (`CefrLevel` enum, `Words`).
- [x] Banky napojené do controllera — `WordBank[]` v inspektore (A1→C2), výber banky podľa
      postupu (2 platformy na level), random bez opakovania v rámci banky.
- [x] UI polish — level badge + pulse, high score + NEW BEST, restart R (aj počas pauzy),
      zelený/červený flash, screen shake, ESC pauza, death animácia.
- [x] Hint nápoveda — len od B2 vyššie: jedno náhodné písmeno (nie vždy prvé) sa ukáže
      na 1,5 s a potom zmizne; vypína sa v Inspectori cez `hintsEnabled`.
- [x] Endless mode — plošiny sa recyklujú, hra nekončí po 12 slovách.
- [x] EN↔SK prepínanie — voľba na štarte `1`/`2`, počas hry sa nedá prepnúť
      (TAB by prezradil preklad). HUD badge `SK → EN` / `EN → SK`.
      Porovnávanie je tolerantné: lowercase + bez diakritiky (`batozina` = `batožina`).
      Voľba sa pamätá v `PlayerPrefs` (`Direction`).
- [x] 3D modely — `character.fbx` (Player) + `platforma.fbx` (12× v `Platforms`), všetky
      materiály na `Custom/Toon Emission Outline`. Postava 2,0 jednotky, plošiny
      3,0 × 0,557 × 2,08; collider top sedí s vizuálom. Overené v Play mode (skóre + 2 skoky).
      `DeathRoutine` používa `GetComponentInChildren<Renderer>()` kvôli modelu v child objekte.
- [x] Visual tuning — postava otočená čelom ku kamere (`CharacterModel` rotácia 180° okolo Y).
      Plošiny rozostup **3,5** (bolo 2,0) → medzi hlavou a plošinou nad ním je ~0,94 jednotky,
      žiadne prekrývanie pri státí. X-cikcak zosilnený na `0 / ±0,75`.
      Kamera: `cameraRiseSpeed` 0,25 → **0,4**, `cameraFollowSpeed` 6 → **8**.
      Skok: `jumpDuration` 0,35 → **0,55**, `jumpArcHeight` 0,9 → **1,95** (v apex skoku sú
      nohy ~0,2 nad plošinou, takže dopadá zhora a neprepadáva cez ňu).
      `PlatformSpacing` v skripte = 3,5 (musí sedieť so scénou kvôli recyklácii).
      Overené v Play mode: 8 slov / 8 skokov, skóre 8, bez smrti.

| Banka | Párov |
|---|---|
| A1 | 60 |
| A2 | 80 |
| B1 | 100 |
| B2 | 100 |
| C1 | 50 |
| C2 | 30 |

## Next — v tomto poradí 🔜

1. **Build Windows + Linux** a playtest na Intel iGPU (build zatiaľ neoverený).
2. **Polish**: zvuk skoku/level-up, particles, pozadie.
3. **v2**: skins postavičky + power-upy (hinty sa presunú pod konkrétne skiny).

## Poznámky 📌

- **Per-word timer sme vynechali** — tlak robí stúpajúca kamera (MVP „smrť pri pomalom písaní“).
  Samostatný countdown je možný doplnok, nie nutnosť.
- Banky sú len `sk` (prompt) + `en` (odpoveď). EN→SK mód = swap polí, netreba druhú sadu.
- Žiadne viacslovné frázy, žiadne duplicitné SK/EN naprieč bankami (overené skriptom).
- C1/C2 sú malé (50/30) a slová sú dlhé → "bonus" pri vysokom leveli, nie hlavná hracia plocha.
- Obtiažnosť = **banka + hint (od B2) + rýchlosť kamery**, banka sama ju nerieši.
- Dátový súbor sa generoval dočasným Editor skriptom, ktorý je už zmazaný; banky sa už needitujú skriptom,
  len v Inspectori alebo podľa potreby novým jednorazovým builderom.
