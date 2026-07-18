//==============================================
// KronosHUD_Server.cs (RMRPG) - server side of the KronosHUD TAB roster.
//==============================================
// Answers the modern TAB menu's player-list request. The client sends
// remoteEval(2048, KMGetPlayers) whenever a server menu opens (KronosMenu.cs);
// we reply with one KMPlayer row per real player plus a KMPlayerCount total.
// Without this handler the list is empty and the client log fills with
// "remoteKMGetPlayers: Unknown command."
//
// Adapted from the Kingdom of Kronos KronosHUD_Server.cs. RMRPG field mappings:
//   level  -> getFinalLVL()             class  -> $CLASS[] (fallback $GROUP[])
//   zone   -> Zone::getDesc($zone[])     remort -> 0 (RMRPG has no remort)
//
// VANILLA-SAFE: vanilla clients never send KMGetPlayers, so this never runs for
// them; it only answers HUD clients that ask.
//==============================================

$KronosMenu::MaxListRows = 16;   // must match $KM::MaxPRows client-side

// True if a client id is one of the town NPCs. Townbots share the player id
// pool (2049-2175) with real players and enemy bots, so we filter them out of
// the roster explicitly (belt-and-suspenders alongside Player::isAiControlled).
function isRMTownBot(%id)
{
	if($TownBotList == "")
		return false;
	return String::findSubStr(" "@$TownBotList@" ", " "@%id@" ") != -1;
}

// Client -> server request from KronosMenu.cs (remoteNewMenu). Send the roster.
function remoteKMGetPlayers(%clientId)
{
	if(Player::isAiControlled(%clientId))
		return;
	if(isRMTownBot(%clientId))
		return;
	if(Client::getName(%clientId) == "" || Client::getName(%clientId) == -1)
		return;

	%sent = 0;
	%total = 0;
	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		if(Player::isAiControlled(%cl))
			continue;
		if(isRMTownBot(%cl))
			continue;
		if(Client::getName(%cl) == "" || Client::getName(%cl) == -1)
			continue;

		%total++;
		if(%sent >= $KronosMenu::MaxListRows)
			continue;

		%class = $CLASS[%cl];
		if(%class == "")
			%class = $GROUP[%cl];

		%zone = Zone::getDesc($zone[%cl]);
		if(%zone == "" || %zone == -1)
			%zone = "Unknown";

		// row layout must match remoteKMPlayer(%server,%idx,%id,%lvl,%remort,%class,%name,%zone)
		remoteEval(%clientId, "KMPlayer", %sent, %cl, getFinalLVL(%cl), 0, %class, Client::getName(%cl), %zone);
		%sent++;
	}
	remoteEval(%clientId, "KMPlayerCount", %sent, %total);
}

// ============================================
// Main stat push - drives the client's HP/MP/EXP bars ($KH::hasData)
// ============================================
// Client contract (config\Presto\KronosHUD.cs):
//   remoteKronosHUD(%server, %hp,%maxHp, %mana,%maxMana, %exp,%xpCur,%xpNext,
//                   %gold, %lvl, %remort)   - all numeric
//   remoteKronosHUD2(%server, %class, %zone) - strings (may contain spaces)
// RMRPG mappings: hp=getHP/$MaxHP, mana=getMANA/getMaxMANA, exp=getFixedExp
// (RMRPG EXP is a multi-word big number; getFixedExp scalarizes it) with the
// level curve from $EXPFixedtable. remort=0 (RMRPG has no remort).
// The vitals push stays UNGATED on the handshake - it's the discovery channel
// (first push -> client sets $KH::hasData -> answers KHudPing).

function KronosHUD_Push(%clientId)
{
	// Watchdog: revive the LOS scan loop if it isn't running (mission
	// load flushes schedules, killing any loop started before/during it)
	if(getSimTime() - $KronosHUD::LOSLastTick > 5)
		KronosHUD_StartLOSScan();

	if(Player::isAiControlled(%clientId))
		return;
	if(isRMTownBot(%clientId))
		return;
	if(Client::getName(%clientId) == "" || Client::getName(%clientId) == -1)
		return;
	if(!$HasLoadedAndSpawned[%clientId])
		return;

	// Vitals. getHP/getMANA deref the player object - guard the dead case.
	if(IsDead(%clientId))
	{
		%hp = 0;
		%mana = 0;
	}
	else
	{
		%hp = getHP(%clientId);
		%mana = getMANA(%clientId);
	}
	%maxHp = $MaxHP[%clientId];
	%maxMana = getMaxMANA(%clientId);

	%lvl = getFinalLVL(%clientId);
	%exp = getFixedExp(%clientId);
	%xpCur = $EXPFixedtable[%lvl];
	if(%xpCur == "")
		%xpCur = 0;
	%xpNext = $EXPFixedtable[%lvl + 1];
	if(%xpNext == "" || %xpNext <= %xpCur)   // level cap: pin the bar full
		%xpNext = %exp;

	%gold = $COINS[%clientId];

	remoteEval(%clientId, "KronosHUD", %hp, %maxHp, %mana, %maxMana, %exp, %xpCur, %xpNext, %gold, %lvl, 0);

	// Metadata (spaces allowed) - handshake-gated like the Kronos original
	if(%clientId.hasKronosHUD)
	{
		%class = $CLASS[%clientId];
		if(%class == "" || %class == -1)
			%class = $GROUP[%clientId];
		if(%class == "" || %class == -1)
			%class = "Adventurer";
		%zone = Zone::getDesc($zone[%clientId]);
		if(%zone == "" || %zone == -1)
			%zone = "Red Moon";
		remoteEval(%clientId, "KronosHUD2", %class, %zone);
	}
}

// ============================================
// Target frame push - enemy name/HP% (+hit number) to the attacker
// ============================================
function KronosHUD_PushTarget(%shooterClient, %damagedClient, %damage)
{
	if(Player::isAiControlled(%shooterClient))
		return;
	if(Client::getName(%shooterClient) == "" || Client::getName(%shooterClient) == -1)
		return;
	if(!%shooterClient.hasKronosHUD)
		return;

	%targetName = Client::getName(%damagedClient);
	if(%targetName == "" || %targetName == -1)
		%targetName = "Unknown";

	%maxHp = $MaxHP[%damagedClient];
	%targetHpPct = 0;
	if(%maxHp > 0 && !IsDead(%damagedClient))
		%targetHpPct = floor((getHP(%damagedClient) * 100) / %maxHp);
	if(%targetHpPct < 0)
		%targetHpPct = 0;
	if(%targetHpPct > 100)
		%targetHpPct = 100;

	// RM 2026-07-18: the damage arg lights KronosHUD's target-plate -DMG/LCK
	// slot - send it ONLY to shooters in nameplate mode (TAB menu "Set damage
	// numbers"; $DmgStyle, SaveData slot 49). Floating-mode shooters get the
	// number as an ATKText float (RM::sendATKText, playerdamage.cs), so
	// passing %damage to them too showed the same hit TWICE. The name +
	// live-HP plate push goes to everyone either way.
	%plateDmg = "";
	if($DmgStyle[%shooterClient] == "nameplate")
		%plateDmg = %damage;
	remoteEval(%shooterClient, "KronosTarget", %targetName, %targetHpPct, %plateDmg);
}

// ============================================
// LOS scan - target frame when a player looks at a player/enemy bot
// ============================================
// Periodic raycast down each HUD player's view (stock GameBase::getLOSInfo).
// Town bots get a nameplate-only push ("NPC" in the HP slot - they're
// damage-immune, so an HP bar would mislead). Also piggybacks a change-gated
// vitals push so damage/heal/mana-drain updates the bars between refreshes.

$KronosHUD::LOSScanPeriod = 0.5; // seconds between scans
$KronosHUD::LOSRange = 120;      // meters
$KronosHUD::LOSDebug = false;

// Resolve a player OBJECT back to its client id. Player::getClient covers
// real players; RMRPG enemy bots are AI clients, so fall back to matching
// Client::getOwnedObject across the client list.
function KronosHUD_ClientFromPlayer(%obj)
{
	%cl = Player::getClient(%obj);
	if(%cl != "" && %cl != -1 && %cl != 0)
		return %cl;
	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
		if(Client::getOwnedObject(%cl) == %obj)
			return %cl;
	return -1;
}

function KronosHUD_LOSScan(%gen)
{
	// Generation guard: re-exec'ing this file bumps the generation,
	// which kills any previously scheduled scan loop
	if(%gen != $KronosHUD::LOSGen)
		return;
	schedule("KronosHUD_LOSScan(" @ %gen @ ");", $KronosHUD::LOSScanPeriod);
	$KronosHUD::LOSLastTick = getSimTime();

	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		// HUD clients only (handshake) - also skips the raycast work
		if(!%cl.hasKronosHUD)
			continue;
		if(Player::isAiControlled(%cl))
			continue;
		if(!$HasLoadedAndSpawned[%cl])
			continue;
		if(IsDead(%cl))
			continue;
		%pobj = Client::getOwnedObject(%cl);
		if(%pobj == "" || %pobj == -1 || %pobj == 0)
			continue;

		// Shop/bank overlay range check: walking away (>10m) closes it, same
		// as the native GUI's isShoppingOn rule (which also drops play mode).
		// Plain inventory mode ("inv") has no bot to stand near - exempt.
		if(%cl.kshopOpen != "" && %cl.kshopOpen != "inv")
		{
			if(!Client::isShoppingOn(%cl, %cl.currentShop, %cl.currentBank, %cl.currentLoot, %cl.currentSmith))
				KronosShopRM_Close(%cl);
		}

		// Vitals freshness: numbers are only pushed on stat-refresh events,
		// so plain damage/heals/mana drain would leave the bars stale.
		// Change-gated: idle players cost nothing.
		%vhp = getHP(%cl);
		%vmana = getMANA(%cl);
		if(%vhp != %cl.khudLastHP || %vmana != %cl.khudLastMana)
		{
			%cl.khudLastHP = %vhp;
			%cl.khudLastMana = %vmana;
			KronosHUD_Push(%cl);
		}

		// Raycast down the player's view
		if(!GameBase::getLOSInfo(%pobj, $KronosHUD::LOSRange))
			continue;
		if(getObjectType($los::object) != "Player")
			continue;

		%targetId = KronosHUD_ClientFromPlayer($los::object);
		if(%targetId == "" || %targetId == -1 || %targetId == %cl)
			continue;
		if(IsDead(%targetId))
			continue;
		if(isRMTownBot(%targetId))
		{
			// Damage-immune town NPC: nameplate-only frame ("NPC" HP slot)
			%npcName = $TownBot[%targetId, NAME];
			if(%npcName == "" || %npcName == -1)
				%npcName = Client::getName(%targetId);
			if(%npcName != "" && %npcName != -1)
				remoteEval(%cl, "KronosTarget", %npcName, "NPC", "");
			continue;
		}

		KronosHUD_PushTarget(%cl, %targetId);
	}
}

// Start (or restart) the scan loop. Generation counter kills any
// previously scheduled loop, so calling this twice is safe.
function KronosHUD_StartLOSScan()
{
	$KronosHUD::LOSGen++;
	$KronosHUD::LOSLastTick = getSimTime();
	echo("KronosHUD: LOS scan starting (gen " @ $KronosHUD::LOSGen @ ")");
	KronosHUD_LOSScan($KronosHUD::LOSGen);
}
// NOTE: the loop is NOT scheduled at exec time - mission load flushes
// schedules. The watchdog in KronosHUD_Push starts/revives it instead.

// ============================================
// TAB menu info box (6 setInfoLine rows)
// ============================================
// Fills the stock InfoCtrlBox - an engine control every client has, so this
// is vanilla-safe too. Own stats when the menu opens; clicking a roster row
// (remoteSelectClient) shows that player's public info instead.

function KronosMenu_SendOwnInfo(%clientId)
{
	if(Player::isAiControlled(%clientId))
		return;
	if(Client::getName(%clientId) == "" || Client::getName(%clientId) == -1)
		return;
	if(!$HasLoadedAndSpawned[%clientId])
		return;

	%class = $CLASS[%clientId];
	if(%class == "" || %class == -1)
		%class = $GROUP[%clientId];

	if(IsDead(%clientId))
		%hp = 0;
	else
		%hp = getHP(%clientId);

	%coins = $COINS[%clientId];
	%bank = $BANK[%clientId];

	remoteEval(%clientId, "setInfoLine", 1, Client::getName(%clientId) @ " - Lv " @ getFinalLVL(%clientId) @ " " @ %class);
	remoteEval(%clientId, "setInfoLine", 2, "STR " @ getFinalSTR(%clientId) @ "   DEF " @ getFinalDEF(%clientId) @ "   MDEF " @ getFinalMDEF(%clientId) @ "   LCK " @ getFinalLCK(%clientId));
	remoteEval(%clientId, "setInfoLine", 3, "HP " @ %hp @ "/" @ $MaxHP[%clientId] @ "   MP " @ getMANA(%clientId) @ "/" @ getMaxMANA(%clientId) @ "   STA " @ getSTA(%clientId));
	remoteEval(%clientId, "setInfoLine", 4, "EXP " @ FixM(getFixedExp(%clientId)) @ "   (Need " @ FixM(getTNL(%clientId, Strip)) @ ")");
	remoteEval(%clientId, "setInfoLine", 5, "Gil " @ FixM(%coins) @ "   Bank " @ FixM(%bank) @ "   Total " @ FixM(%coins + %bank));
	remoteEval(%clientId, "setInfoLine", 6, "Weight " @ $Weight[%clientId] @ " / " @ GetMaxWeight(%clientId));
}

// Public info for a roster-clicked player (no coins/bank - that's private)
function KronosMenu_SendPlayerInfo(%clientId, %selId)
{
	if(%selId == "" || %selId == -1 || %selId == %clientId)
	{
		KronosMenu_SendOwnInfo(%clientId);
		return;
	}
	if(Client::getName(%selId) == "" || Client::getName(%selId) == -1)
		return;

	%class = $CLASS[%selId];
	if(%class == "" || %class == -1)
		%class = $GROUP[%selId];

	%zone = Zone::getDesc($zone[%selId]);
	if(%zone == "" || %zone == -1)
		%zone = "Unknown";

	remoteEval(%clientId, "setInfoLine", 1, Client::getName(%selId) @ " - Lv " @ getFinalLVL(%selId) @ " " @ %class);
	remoteEval(%clientId, "setInfoLine", 2, "Location: " @ %zone);
	remoteEval(%clientId, "setInfoLine", 3, "Real Name: " @ $Client::info[%selId, 1]);
	remoteEval(%clientId, "setInfoLine", 4, "");
	remoteEval(%clientId, "setInfoLine", 5, "");
	remoteEval(%clientId, "setInfoLine", 6, "");
}

// ============================================
// Presto client compat stubs
// ============================================
// remotePlayAnim: the Presto chat menu's emote entries (Kneel/Celebration)
// send it, and the stock vol's own remotePlayAnimWav calls it - yet NO
// Tribes mod in this lineage defines it (verified: stock vol, Kronos, RMRPG
// all lack it; emote ANIMATIONS have never played, only the wavs). Stub it
// to kill the per-emote "Unknown command" console error without inventing
// animation behavior that never existed.
function remotePlayAnim(%Client, %anim) {}

// remoteBuyFavorites: Presto's Favorites screen (a Tribes loadout feature)
// sends this; RMRPG has no favorites-loadout server support and its GUI.CS
// stubs the favorites UI out client-side. Silence the console error.
function remoteBuyFavorites(%Client, %a, %b, %c, %d, %e, %f, %g, %h)
{
	Client::sendMessage(%Client, 0, "Favorites are not supported on Red Moon RPG.");
}

echo("KronosHUD_Server (RMRPG): roster + stat push + LOS target scan loaded");
