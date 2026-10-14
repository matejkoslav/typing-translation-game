# NAVOD — ako si stiahnuť a rozbehnúť projekt

Krátky návod pre kolegov. Cieľ: **stiahnuť → otvoriť v Unity → funguje to.**

## Ako začať (rýchlo)

1. Pošli **Matejovi** svoj **GitHub username** → príde ti pozvánka do repa → **prijmi ju**.
2. `git clone …` (viď bod 2 nižšie). Ak práva nechceš, sprav namiesto toho **Fork** (bod 4, možnosť B).
3. Otvor v Unity, rob zmeny, potom **vetva → push → Pull Request** (bod 4).

Ako presne prispievať kódom a čo smie/nesmie do commitu je v bodoch **2, 4 a 5**. 👇

---

## 1. Čo potrebuješ (raz nainštalovať)

- **Unity Hub** — <https://unity.com/download>
- **Unity Editor `6000.6.5f1`** — presne táto verzia (v Unity Hub: *Installs → Install Editor → Archive*)
- **Git** — na Windows [Git for Windows](https://git-scm.com/download/win) (nainštaluj vrátane *Git Bash*), na Linux `sudo apt install git`.
- Odporúčaný modul v Hube: **Windows Build Support** (a/alebo **Linux Build Support**), ak chceš robiť buildy.

> Git LFS zatiaľ **netreba**. Keď pribudnú prvé FBX/audio, pridáme ho dodatočne.

## 2. Stiahnutie projektu

```bash
git clone https://github.com/matejkoslav/typing-translation-game.git
cd typing-translation-game
```

Na Windows to spustíš v **Git Bash** (odporúčané) alebo v **PowerShell** (príkazy sú rovnaké).

Alternatíva bez Gitu: na GitHube **Code → Download ZIP**. Pozor: so ZIPom nebudeš dostávať nové zmeny (`git pull`) a nebudeš vedieť posielať vlastné — preto odporúčam clone.

## 3. Otvorenie v Unity

1. **Unity Hub → Add → Add project from disk** → vyber priečinok `typing-translation-game`.
2. Otvor ho **verziou `6000.6.5f1`**.
3. **Prvé otvorenie trvá dlhšie** (Unity sťahuje balíky — vrátane URP a MCP pluginu). Nechaj ho dobehnúť.
4. Otvor scénu `Assets/Scenes/SampleScene.unity` (dvojklik v *Project* okne).
5. Stlač **Play** ▶.

> ⚠️ Projekt otváraj **vždy verziou `6000.6.5f1`**. Ak ťa Hub núti na inú verziu, klikni *Cancel* a doinštaluj `6000.6.5f1` — inak sa projekt „upgraduje“ a rozbijeme si zhodu v tíme.

## 4. Ako prispieť kódom (push)

Repo je **public**, takže klonovať si ho môže ktokoľvek — ale **pushovať priamo doň nemôžeš, kým nemáš práva**. Vyber si jednu možnosť:

**Možnosť A — Collaborator (odporúčané, môžeš pushovať priamo):**
1. Pošli mi svoj **GitHub username**.
2. Ja ťa pridám: *repo → Settings → Collaborators → Add people*.
3. Prijmi pozvánku (e-mail / GitHub) → máš push práva.

**Možnosť B — Fork + Pull Request (bez práv, funguje vždy):**
1. Na GitHube vpravo hore klikni **Fork** (vznikne tvoja kópia repa).
2. Klonuj si **svoj fork** a pracuješ v ňom.
3. Pushni do svojho forku → na GitHube **Contribute → Open pull request**.

**Bežný postup (platí pre obe možnosti):**

```bash
git pull                                    # VŽDY pred prácou
git checkout -b feature/kratky-nazov        # vlastná vetva
# ... zmeny, uloženie scény/skriptov v Unity ...
git add -A
git commit -m "Strucny popis zmeny"
git push -u origin feature/kratky-nazov     # pri B: origin = tvoj fork
```

Potom otvor **Pull Request** do `main` a napíš, čo si zmenil.

**Pravidlá:**

- `git pull` pred každou prácou, malé a časté commity s jasnou správou.
- **Dvaja naraz needitujú tú istú scénu ani prefab** (scény sa nedajú zlúčiť). Dohodnite sa vopred.
- Súbory **presúvajte a premenúvajte v Unity**, nie v prieskumníku — inak sa rozbijú `.meta` odkazy.
- **`.meta` súbory sa commitujú vždy** a nikdy sa nemažú ručne.
- Drž sa **presných názvov** (`Player.fbx` ≠ `player.fbx`) — v tíme sú aj Linux stroje, kde na veľkosti písmen záleží.
- `main` = stabilná verzia. Nerobiť do nej priame commity.

## 5. Čo sa commitovať MÁ / NEMÁ

- **Áno (pushuje sa):** `Assets/**` (aj všetky `.meta`), `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/**`, `.agents/**`, `AGENTS.md`, `NAVOD.md`, `.gitignore`
- **Nie (generuje si Unity / máš lokálne, je v `.gitignore`):** `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `Build/`, `Builds/`, `*.csproj`, `*.sln`, **`opencode.json`**

> `opencode.json` je **len lokálny** (MCP adresa pre tvoj stroj) — nikdy sa neposiela. Každý si ho vytvorí sám, viď bod 7.

## 6. Build (Windows / Linux)

- **File → Build Profiles** → vyber **Windows** alebo **Linux** → **Build**.
- Ak platforma chýba, doinštaluj modul cez **Unity Hub → Installs → ⚙ → Add modules**.

## 7. Voliteľne: AI v Unity (MCP for Unity)

Nie je potrebné na hru, je to len pomoc pre AI agenta pri skladaní scén/skriptov.

1. V Unity: **Window → MCP for Unity**.
2. Klikni **Start Server** a počkaj, kým je server „ready“.
3. Ak chceš OpenCode: vytvor v koreni projektu `opencode.json`:

   ```json
   {
     "$schema": "https://opencode.ai/config.json",
     "mcp": {
       "unityMCP": { "type": "remote", "url": "http://127.0.0.1:8080/mcp", "enabled": true }
     }
   }
   ```

4. **Spúšťaj v tomto poradí:** najprv Unity (server beží) → až potom OpenCode. Ak OpenCode štartuje skôr, spojenie zlyhá (`ConnectionRefused`) a treba reštart OpenCode alebo manuálny reconnect.

## 8. Čo je zatiaľ v projekte

- `Assets/Scenes/SampleScene.unity` — hlavná (zatiaľ ukážková) scéna
- `Assets/Scripts/Gameplay/TypingGameController.cs` — prototyp core loop (píšeš EN preklad SK slova → postava skočí → kamera stúpa)
- `Assets/Art/Materials/` — materiály (URP)
- `Assets/Settings/` — URP pipeline profily (PC + Mobile)
- `Assets/InputSystem_Actions.inputactions` — nastavenia nového Input Systemu
- `Assets/TextMesh Pro/`, `Assets/TutorialInfo/` — Unity default assety
- `AGENTS.md` — zadanie, tech stack, konvencie a pravidlá projektu (prečítať!)
- `Packages/manifest.json` — zoznam balíkov (URP, Input System, TextMesh Pro, MCP for Unity)

## 9. Keď niečo nefunguje

- **Unity pýta inú verziu** → nainštaluj `6000.6.5f1` v Unity Hube.
- **Chýbajú balíky / veľa errorov** → `Edit → Preferences → External Tools` OK, potom `Assets → Reimport All`.
- **`Library/` problémy** → zavri Unity, zmaž priečinok `Library/`, otvor znova (Unity si ho vygeneruje).
- **Konflikt v scéne** → nedá sa zlúčiť; dohodnite sa, kto scénu vlastní, a zlúčte ručne.
