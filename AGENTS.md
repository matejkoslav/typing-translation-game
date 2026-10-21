# AGENTS.md — 3D typing/translation hra (Unity)

> Zdieľaný dokument: čítajú ho AI agenti (opencode) AJ členovia tímu.
> Drž ho krátky a presný. Ak sa niečo zmení, dopíš to sem.

## 1. Prehľad projektu

- Žáner: 3D typing/translation game (školský projekt).
- Pracovný názov: `My project` (finálny názov TBD).
- Core loop: hráč píše slová; za správne slovo postavička skočí hore, kamera pomaly stúpa. Kto píše pomaly, umiera. Skóre rastie, obtiažnosť sa zvyšuje s výškou.
- Slová: prekladanie **EN ↔ SK** (prepínateľné). Nápoveda: dashes podľa počtu písmen, niektoré písmená vopred doplnené.
- Word banky: A2 → B1 (SK + EN), malé a pamäťovo úsporné.
- Neskôr (v2): custom skins postavičky + power-upy (viac času, dvojité skóre), backgrounds/platformy, zvuk/hudba.
- Cieľ: čo najrýchlejšie hrateľný build (1–2 dni práce), potom polish a bugfixy. **Deadline: 10. 12.**

## 2. Tech stack a verzie

- Unity **6000.6.5f1** — presne táto verzia u všetkých v tíme. Nezvyšovať bez dohody.
- URP (Universal Render Pipeline). Projekt je osekaný na základ (AI/collab balíky odstránené kvôli výkonu a RAM).
- **Nový Input System** (nie starý `Input.*`); existuje `Assets/InputSystem_Actions.inputactions`.
- UI: uGUI + TextMeshPro.
- Build target: **Windows + Linux**, bez platform-špecifického kódu.
- AI integrácia: MCP for Unity (`com.coplaydev.unity-mcp`) v `Packages/manifest.json` — kolegom sa doinštaluje automaticky po otvorení projektu.
- Import Worker Count = 2 (v `UserSettings`, kvôli RAM na Matejovom notebooku).

## 3. Tím a roly

- **Matej** — kód, integrácia, build, repo.
- **Kolega A** — docs, podpora, testovanie.
- **Kolega B** — design, 3D modely, art.
- Nikto neprepisuje cudzie assety/scény bez dohody.

## 4. Repo a collaboration rules

- Repo: GitHub, root = tento priečinok (`My project/`), binárky cez **Git LFS**.
- Pushuje sa: `Assets/**` (aj všetky `.meta`), `.agents/**`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/**`, `.gitignore`, `AGENTS.md`.
- Nepushuje sa (pokryté `.gitignore`): `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `obj/`, `Build/`, `Builds/`, `.vs/`, `*.csproj`, `*.sln`.
- LFS track: `*.fbx *.png *.jpg *.jpeg *.tga *.psd *.wav *.mp3 *.ogg`
- Pravidlá:
  - `git pull` pred prácou; malé a časté commity s jasnou správou.
  - `.meta` súbory sa commitujú vždy; nikdy ich nemažte ručne.
  - **Dvaja naraz needitujú ten istý Scene/Prefab** (scény sa nedajú mergeovať).
  - Súbory presúvajte/premenúvajte **v Unity**, nie v prieskumníkovi.
  - Linux je case-sensitive: presné názvy (`Player.fbx` ≠ `player.fbx`).
  - Branch flow: `main` (stabilné) + krátke feature branche. **AI agent necommituje ani nepushuje bez výslovného pokynu.**
- Assets od kolegov: spôsob doručenia TBD (dohodnúť); preferované cez repo/LFS alebo cloud share — Matej integruje.

## 5. Art a audio pipeline

- 3D modely: **výhradne FBX** (`.blend` neimportovať — na Linuxe nie je Blender). Export z Blenderu: Y-up, apply transforms.
- Mierka: **1 Unity unit = 1 m**; postava má origin pri nohách (y = 0), rotácia (0, 0, 0).
- Textúry: PNG/JPG. Audio: WAV/OGG.
- Pomenovanie: `Character_Fox.fbx`, `Platform_Grass_01.fbx`, `UI_Button.png` (PascalCase, varianty `_01`).
- Priečinky: `Assets/Art/Models/`, `Assets/Art/Materials/`, `Assets/Art/Textures/`, `Assets/Audio/SFX/`, `Assets/Audio/Music/`, `Assets/WordBanks/`, `Assets/Prefabs/`, `Assets/Scenes/`, `Assets/Scripts/`.

## 6. Konvencie kódu

- Anglické názvy, krátke anglické komentáre.
- C#: `PascalCase` triedy/metódy, `camelCase` privátne polia, `[SerializeField] private` namiesto public.
- Vstup len cez nový Input System (`InputAction` asset).
- Herné dáta (word banks, skiny, power-upy) cez **ScriptableObjects** (`Assets/Scripts/Data/`).
- Štruktúra skriptov: `Assets/Scripts/Core`, `Gameplay`, `UI`, `Data`.
- Performance: žiadne per-frame `Find`/`GetComponent`, minimum alokácií v `Update`, opakované objekty (platformy) poolovať; word banky načítať raz.
- Hra musí bežať plynulo aj na Intel iGPU (Mint notebook, 2.8K panel) — jednoduché URP shadery.

## 7. MVP vs neskôr

- **MVP (v1):** písanie EN↔SK, nápoveda (dashes + čiastočne doplnené písmená), skok postavičky, stúpajúca kamera, smrť pri pomalom písaní, skóre, obtiažnosť podľa výšky, základné SK+EN A2 banky.
- **v2:** skiny + power-upy (viac času / dvojité skóre), backgrounds, platformy, zvuk/hudba, polish.

## 8. Build a spustenie

- Projekt otvárať cez **Unity Hub** (správna verzia), nie raw editor.
- `File → Build Profiles`: pridať Windows (x86_64) a Linux (x86_64). Chýbajúce moduly doinštalovať cez Unity Hub.
- Bez `#if UNITY_*` vetiev, pokiaľ to nie je nevyhnutné.
- Pri veľkých importoch na Matejovom notebooku použiť `../tools/unity-safe-launch.sh` (E-jadrá + watchdog).

## 9. AI pokyny

- Pred väčšími zmenami krátky plán (3 body) a potvrdenie.
- Drž sa konvencií v tomto dokumente; needituj cudzie assety/scény.
- Stav projektu a next kroky: `PROGRESS.md` — prečítaj pred prácou a aktualizuj po väčšej zmene.
- Necommituj ani nepushuj bez výslovného pokynu.
- MCP (Unity) slúži na skladanie scén/prefabov a skriptov; pred použitím musí byť otvorené Unity s bežiacim bridge (Window → MCP for Unity).
- Ak `unityMCP` tools v session chýbajú (OpenCode sa pripojil skôr, než Unity bridge nabehol), agent spraví reconnect namiesto čakania:
  `opencode api post "/api/experimental/mcp/unityMCP/connect?location%5Bdirectory%5D=$PWD"`
  Tools sa objavia aj uprostred session; netreba reštart OpenCode ani editovať config. Unity okno môže nesprávne hlásiť "Not Configured" (kontroluje len globálny config) — ignorovať, nestláčať "Configure".
- Unity CLI agent skill je v `.agents/skills/unity-cli/` — spravuje ju `unity skill refresh`, needitujte ju ručne (commituje sa s repom).
- Komunikácia SK/EN prirodzene; kód a identifikátory po anglicky.
