# RMRPG Enemy Balance Audit — 2026-07-17

Full-mob balance sweep (Opus agent) after the "GnollPup 3-hit-kills a L79" report.
Damage model verified in playerdamage.cs: per-hit ~= (1.5*weaponChar6 + attackerSTR/2
- defenderDEX/20 + ATKbonus) * roll(0.85-1.6), crit x2. Damage does NOT scale with
defender MaxHP (the /100 at playerdamage.cs:638 is cancelled by *100 in hp.cs:59);
mitigation is only DEX/20 + the DEF roll term, so armor/DEF gear is essential by design.

Bot stat build (AI.cs HardCodedSkills ~:243-283):
- skill = LVL (levels 3-89), 2*LVL at 90+  -> POWER CLIFF at level 90
- CON multiplier 2 below level 120, 3 at 120+ -> HP CLIFF at level 120
- Keep each mob's LVL band on ONE side of 90/120 or it feels spiky within one type.

## FIXED (2026-07-17)
- [x] scourge weapon typo `zombiesworb` -> `zombieswordb` (spawned weaponless). EnemyArmors.cs:282
- [x] ofCopper inverted band `LVL 300-225` -> `200-225` (golem ladder: Cu 200-225,
      Ag 225-250, Steel 250-275, Fe 275-300). EnemyArmors.cs:301

## NOT A BUG (confirmed by Joe)
- Toxic-race mobs (Fish/Blob) missing $JumpVelForRace: intentional — they don't jump.

## OUTLIERS — TO RETUNE LATER (ranked worst first)

1. **batm** (cyborg ship, LVL 10-20) — worst in game. esmissile char6 ~219 (3d12+200)
   + ranged bolt on a 92-HP level-15 mob -> ~410/hit, one-shots level-appropriate
   players. Fix: dedicated weapon char6 15-25, OR raise band to 120-160.
   EnemyArmors.cs:311, AccessoryData.cs:212.
2. **cko / Rocko** (Big Blue Area, LVL 400-450) — dodgethisc char6=1999 -> ~4,100/hit
   + 15,000 bonus HP, on a maxForwardSpeed=2 stationary mob. Fix: char6 -> ~900, drop
   the +15k HP, or move to the 700+ tier. EnemyArmors.cs:343, AccessoryData.cs:287.
3. **Pup** (Catacombs, LVL 60-80) — gnollspeara char6=150 char1=75 -> ~360/hit,
   3-hit-kills a 1,337-HP L79. Fix: gnollspeara 150/75 -> 60/30 (~150/hit, ~8-hit TTK).
   NOTE gnollspeara is shared with warrior — retune together. AccessoryData.cs:324.
4. **ob / Blob** (Rin Sewers, LVL 100-150) — blobweapon char6=500 -> ~1,050/hit AND
   char2=+10,000 DEX self-buff = nearly unhittable, in the newbie-hub sewers.
   Fix: char2 -> ~+200, char6 500 -> ~180; consider re-homing or band ~30-60.
   EnemyArmors.cs:353, AccessoryData.cs:320.
5. **Spirit** (Temple Halls, LVL 100-150) — ghostweapon also has char2=+10,000
   (unhittable "ghost" mechanic). Same fix as Blob if unintended. AccessoryData.cs:319.
6. **warrior** (Catacombs, LVL 85-110) — band straddles the L90 skill-doubling cliff:
   effSTR jumps to ~290, ~435/hit, 2,158 HP right next to the Pup. Fix: band 80-89
   (below cliff) OR 90-100 with its own ~char6 90 weapon. Progression target:
   Pup(60) -> warrior(90) -> Hunter(150) weapon char6 60 -> 90 -> 150.
7. **Wrath (LVL 750) & Underdog (LVL 850)** — spawn from the SAME RMR.mis marker as
   Nightmare(300)/Shadow(400) ("Small Blue Area", marker `.4 0 0 10 40 50 51 52 53`).
   Underdog: ~234k HP, ~3,740/hit. Fix: split marker -> `50 51` + new marker `52 53`.
8. **Gloom** (Burial Tree MASTER bot, LVL 1-2) — 11 HP, weapon char6=1: non-functional
   boss. Fix: band ~40-60, weapon char6 ~40. EnemyArmors.cs:279.
9. **Berserker** (Burial Tree, LVL 50-75) — zombiesworda char6=1 -> ~45/hit, trivially
   weak. Likely meant zombieswordb (300; heavy) or wants its own char6 ~40-60.
   EnemyArmors.cs:281.
10. **Fish** (Rin Sewers, LVL 100-150) — level band vs newbie-hub area mismatch
    (fall-off-map respawn returns players to Rin Vale). Fix: band ~30-60 or re-home.
11. **Nightmare** (LVL 300-400) — godeyesword char15=+10,000 MDEF = fully spell-immune;
    hard-counters casters with no warning. Keep only if intentional.

## DATA BUGS — TO CLEAN LATER
- Undefined weapons referenced by $BotEquipment (mob spawns weaponless): RDagger,
  RShortsword, RLongsword, RBroadsword, RClaymore, RPickAxe, CastingBlade, bare
  Shortsword/Longsword, claymore, amazonsword, zombiesword (no suffix). Used by
  Bard/Monk/Miner/Commoner/Civilian/Gladiator/Mercenary/Militia/Skeleton/Necromancer/
  Brigand/Marauder/Bladeir/RedWood/Machine (EnemyArmors.cs:258-345). Most of these
  types never spawn in RMR.mis, but the town-NPC set (indices 11-14) may be placed
  elsewhere — verify before deleting.
- ~25 $BotEquipment types never referenced by any RMR.mis spawn marker (dead data or
  event/other-mission only): Shaman, scourge, Skeleton, Necromancer, 4x Golems,
  Brigand, Marauder, Civilian, Gladiator, Mercenary, Militia, RedWood, Bladeir, Eye,
  Machine (LVL 920-999 super-boss), 4x Keepers, Archer, Knight, Person, sPerson,
  arrior, Bard, Monk, Miner, Commoner.
- Overwritten map keys (last-writer-wins): $ArmorTypeToRace[GodeyeArmor] set twice
  (:123,:131); $RaceToArmorType Keeper/Godeye collide; AmazonArmor -> Amazon then Town
  (:124,:134). Keeper/Town skinning currently rides on Godeye/Amazon.
- $Masterbot[Shadow] lowercase-b vs $MasterBot convention (:331) — engine arrays are
  case-insensitive so it likely works; just inconsistent.

## RELATED (found during the damage trace, separate from mob data)
- Bots spawn with $ClientData[bot, UsingWeapon]="-1" and AI::mountItem never adds the
  weapon to EquipList, so some bot swings land with weaponRoll ~0 (near-unarmed hits).
  Makes bot damage feel random (40 one hit, 400 the next).
- AddPoints (Accessory.cs:173) counts a weapon TWICE if it's both in EquipList and set
  as UsingWeapon — inflates player-side damage.
