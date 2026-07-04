# Red Moon RPG — script bug-fix log

Source: extracted from `scripts.vol` (packed 2015-10-17), the newer of the two
copies (the `Downloads/rmrpg` tree is an older 2002-2008 snapshot). This folder
is the git-tracked canonical source; deploy = loose scripts here with
`scripts.vol` made unreachable (loose files override the vol).

Baseline commit: `9e5adae` (as-extracted, pre-fix).

Status: `[x]` fixed · `[ ]` open · `[?]` needs investigation/decision

---

## FIXED

### 1. Chocobo.cs — leftover "TEMP" Cap()/round() clobbered rpgfunk's (HIGH impact)
- [x] Chocobo.cs had `////TEMP` `function Cap()` + `function round()` redefinitions.
  Chocobo loads AFTER rpgfunk (Server.cs: rpgfunk L114, Chocobo L163), so these
  won overrode the real ones. This Cap() dropped rpgfunk's `"inf"` (uncapped)
  branch, so `Cap(x, lb, "inf")` compared `x > "inf"` (→ x>0) and returned the
  STRING "inf". That silently broke, mod-wide:
    - playerdamage.cs `Cap(%value-25, 0, "inf")` — damage math
    - rpgstats.cs `pow(Cap(%i-20, 0, "inf"), 5)` — EXP table generation
    - blackSmith.cs `Cap(round(... *0.75), 1, "inf")` — sell pricing
  Removed the TEMP block; rpgfunk.cs now owns canonical Cap()/round() (round was
  identical anyway).

### 2. keys.cs — 11 malformed keybind lines (`0'` stray tagged-quote)
- [x] `bind break ... 0'` on 11 lines had a stray trailing `'` (opening an
  unterminated tagged string), vs the correct `bind break Down ... MoveBackward 0`
  on line 6. Removed the stray quotes. (Client keybind file; may be unused, but
  it was the only file failing the structural parse check.)

---

## OPEN / TO INVESTIGATE

### Same-file duplicate function definitions — INVESTIGATED
### 3. Mine.cs — mislabeled bomb onAdd callbacks (REAL BUG, FIXED)
- [x] Pattern in Mine.cs is `MineData BombN` then `function BombN::onAdd` (schedules
  Mine::Detonate). Four callbacks were mislabeled by copy-paste, so those bombs had
  NO onAdd and never detonated:
    - Bomb107 (had `Bomb4::onAdd`), Bomb108 (`Bomb4::onAdd`), Bomb200 (`Bomb4::onAdd`),
      bomb88888 (`bomb612::onAdd`). Renamed each to its own bomb. Now they detonate.
- [keep] rpgfunk.cs `IsInCommaList` 2x, `FixStuffString` 2x — identical bodies (the
  active copy just adds a dbecho). Harmless; optional cleanup, not a bug.
- [keep] Chocobo.cs `processMenuViewTradeInfo` 2x — divergent (builder vs handler), BUT
  the function has ZERO callers anywhere → dead/unfinished feature. Harmless.

### 4. [FLAG] AccessoryData.cs — `Botboulder35Image::onFire` defined 2x, DIVERGENT
- [?] 232 handles the "screech" weapon (fires screechbolt); 295 handles
  "dodgethis"/"dodgethisc" (fires dodgethisbolt). Both named the same, so 295 wins and
  SCREECH'S FIRE IS DEAD (screech won't launch its bolt, only melee). Both weapons use
  shape "boulder". Fix is one of:
    (a) MERGE — if screech & dodgethis truly share Botboulder35Image, combine both
        branches into one onFire (checks UsingWeapon). OR
    (b) RENAME — if it's a mislabel (like the bombs), 295 should be dodgethis's real
        image callback.
  NEEDS your call on how MakeItem maps shape "boulder" -> BotboulderNNImage (there's also
  a Botboulder1Image::onFire @241, so images are per-weapon-ish). Left untouched.

### 5. [FLAG] Spells2.cs — `GetBestSpell` defined 2x, DIVERGENT
- [?] 703 (dead): semi-random spell picker that USES the `%semiRandomSpell` param.
  802 (active): weighted "best value" selector that IGNORES `%semiRandomSpell`. The
  active one is more sophisticated (likely the intended newer version), so the
  semi-random code path is effectively gone. Confirm whether that's intended before
  removing the dead 703 (removing it changes nothing at runtime).

- [ ] Client.cs — `Game::endFrame` 5x — client-side file (not loaded by Server.cs);
  low priority, check later.

### Cross-file overrides resolved by load order (verify winner is the intended/correct one)
- [ ] `initSoundPoints` (rpgfunk vs nsound — nsound wins)
- [ ] `gatherBotInfo` (rpgfunk vs Ai — Ai wins)
- [ ] `calcRadiusDamage` (itemevents vs staticshape — staticshape wins)
- [ ] `StaticDoorForceField::onCollision` (Spells2 vs staticshape — staticshape wins)

### Not loaded by Server.cs (dead unless loaded elsewhere — likely harmless)
dm.cs, DeusClient.cs, Comchat2_.cs (in old snapshot), shopping.cs, Client.cs,
newstuff.cs, repack.cs, Carling1/2.cs — their duplicate defs don't conflict
server-side. Confirm none are exec'd from mission/client paths before assuming dead.

### 6. Chocobo.cs — MenuBreed() typo (REAL BUG, FIXED)
- [x] Chocobo.cs:808 (the "Breed" option handler) called `MenuBreed(%Client)`, which is
  defined nowhere — the real function is `MenuBreeding()` (Chocobo.cs:947). So choosing
  "Breed" silently did nothing (breeding menu never opened). Fixed the call name.

### Undefined-function-call scan (bare calls not defined in any script)
Method: strip comments/strings, collect all `function` defs, find bare calls not defined
and not obvious engine builtins. Findings:
- [x] FIXED MenuBreed -> MenuBreeding (above).
- [keep/dead] `fetchData` (shopping.cs) — shopping.cs isn't loaded (economy.cs is), and
  Client.cs:645 `rpgfetchdata(){return False}//RM doesn't support this` confirms RM has no
  fetchData. Dead-code calls. `useItem` (repack.cs) — repack.cs not loaded. Ignore both.
- [?] FLAG `Quest(%Client, %type, false)` — remote.cs:23 (LOADED) calls it, but Quests.cs
  defines NewQuest/FailedQuest/EndQuest, not a 3-arg `Quest`. Real undefined call in a live
  path (remoteQuestChat). Correct target unclear — needs the quest-flow intent.
- [?] FLAG Redplanet.cs — `add_to_fight()` (@1040-41) and `make_horse()` (@460) are called
  but defined nowhere. Redplanet is a special event map; may be an incomplete/unused feature
  or rely on a script that isn't loaded. Verify whether Redplanet is meant to be live.
- [assumed-engine] `RemotePlayAnim` (Ai.cs x4), `updateBuyingList` (Station.cs x3),
  `ClearEvents` (Comchat2.cs), `postAction`, `containerBoxFillSet`, etc. — called in loaded,
  hot code with no near-match; since the mod actually runs, these are almost certainly
  engine/plugin builtins, not bugs. Confirm only if they throw console "unknown command".

### Broader logic sweep — TODO (per-file reading for undefined vars, wrong logic, etc.)
