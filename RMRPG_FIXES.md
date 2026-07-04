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

### Same-file duplicate function definitions (later def silently wins; earlier is dead)
Need to check each pair: identical (harmless cleanup) vs divergent (real bug —
the "dead" one may have been the intended behavior).
- [ ] Client.cs — `Game::endFrame` defined 5x  (Client.cs is client-side)
- [ ] Mine.cs — `Bomb4::onAdd` 4x, `Bomb612::onAdd` 2x
- [ ] rpgfunk.cs — `IsInCommaList` 2x, `FixStuffString` 2x
- [ ] Spells2.cs — `GetBestSpell` 2x
- [ ] Chocobo.cs — `processMenuViewTradeInfo` 2x
- [ ] AccessoryData.cs — `BotBoulder35Image::onFire` 2x

### Cross-file overrides resolved by load order (verify winner is the intended/correct one)
- [ ] `initSoundPoints` (rpgfunk vs nsound — nsound wins)
- [ ] `gatherBotInfo` (rpgfunk vs Ai — Ai wins)
- [ ] `calcRadiusDamage` (itemevents vs staticshape — staticshape wins)
- [ ] `StaticDoorForceField::onCollision` (Spells2 vs staticshape — staticshape wins)

### Not loaded by Server.cs (dead unless loaded elsewhere — likely harmless)
dm.cs, DeusClient.cs, Comchat2_.cs (in old snapshot), shopping.cs, Client.cs,
newstuff.cs, repack.cs, Carling1/2.cs — their duplicate defs don't conflict
server-side. Confirm none are exec'd from mission/client paths before assuming dead.

### Broader logic sweep — TODO (per-file reading for undefined vars, wrong logic, etc.)
