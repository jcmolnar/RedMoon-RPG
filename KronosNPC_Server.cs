//==============================================
// KronosNPC_Server.cs (RMRPG) - drive the KronosNPC dialogue window from
// RMRPG's existing NPC conversation system (comchat3.cs BotChatStuff).
//==============================================
// Adapted from the Kingdom of Kronos KronosNPC_Server.cs. RMRPG differs:
//  - Option keywords come from comchat3.cs SetUpKeys pairs, NOT from
//    bracket/ALL-CAPS markers in the spoken text (so no ExtractOpts here).
//  - The window opens when the player GREETS a bot (comchat.cs), not on bump.
//
// VANILLA-SAFE: nothing here does anything unless %client.hasKronosHUD is true,
// which only becomes true after the client's KHudOn handshake. Vanilla clients
// never send KHudOn, so every path below early-returns for them.
//
// Wiring (added in stages):
//   Server.cs      - exec(KronosNPC_Server);                      [handshake stage]
//   playerspawn.cs - Game::playerSpawn -> remoteEval(%Client,"KHudPing")
//   comchat.cs     - greeting branch -> KronosNPC_OpenRM(...)
//   AI.cs          - AI::sayLater -> mirror spoken line to KNPCLine
//   comchat3.cs    - SetUpKeys -> RM_KNPC_SetUpKeys; end of BotChatStuff ->
//                    KronosNPC_RM_AfterChat
//==============================================

// --- Handshake ------------------------------------------------------------
// The client (config\Presto\KronosHUD.cs) announces itself with KHudOn. That
// is the ONLY thing that flips hasKronosHUD true; it gates everything else.
function remoteKHudOn(%clientId)
{
	%clientId.hasKronosHUD = true;
}

// --- Open the window ------------------------------------------------------
// Called from comchat.cs when a HUD client greets a bot. Opens the window
// only; the spoken lines + options are fed by the AI::sayLater / SetUpKeys
// hooks from the BotChatStuff call that the same greeting is already running.
function KronosNPC_OpenRM(%client, %botId, %display)
{
	if(!%client.hasKronosHUD)
		return;
	if(Player::isAiControlled(%client))
		return;

	%now = getSimTime();
	// don't reopen mid-conversation; 60s recovers from a stale flag
	if(%client.knpcWinOpen != "" && (%now - %client.knpcTime) < 60)
		return;

	if(%display == "" || %display == -1 || %display == "0")
		%display = $TownBot[%botId, NAME];
	if(%display == "" || %display == -1 || %display == "0")
		%display = "Stranger";

	%client.knpcWinOpen = true;
	%client.knpcBot = %botId;
	%client.knpcTime = %now;
	%client.knpcPendingOpts = "";

	// KNPCBeginQuiet opens the window WITHOUT the client auto-sending "#say hi"
	// (RMRPG's greeting is already in flight from the player's own text, so the
	// stock KNPCBegin would double-greet the bot).
	remoteEval(%client, "KNPCBeginQuiet", %display);
}

// --- Close ----------------------------------------------------------------
// Client closed the window (Goodbye / Esc / Tab) -> remoteKNPCClose.
function remoteKNPCClose(%client)
{
	if(!%client.hasKronosHUD)
		return;
	if(%client.knpcWinOpen == "")
		return;
	%client.knpcWinOpen = "";
	// Reset the conversation state so the next greeting starts fresh. RMRPG
	// indexes $state as [Client, botId] (see comchat3.cs BotChatStuff).
	if(%client.knpcBot != "" && %client.knpcBot != -1)
		$state[%client, %client.knpcBot] = "";
	%client.knpcPendingOpts = "";
}

// --- Options --------------------------------------------------------------
// Drop-in for comchat3.cs's raw remoteEval(%Client,"SetUpKeys",%trigs). ALWAYS
// forwards to the real SetUpKeys (keeps vanilla behavior and the keyboard
// shortcut path alive for HUD clients too). For HUD clients it caches the
// keyword half of each (key,word) pair; the KNPCOpts push is deferred to
// KronosNPC_RM_AfterChat so an end-of-conversation turn that still re-sends
// SetUpKeys doesn't leave stale buttons on screen.
function RM_KNPC_SetUpKeys(%Client, %trigs)
{
	remoteEval(%Client, "SetUpKeys", %trigs);
	if(!%Client.hasKronosHUD)
		return;
	// %trigs = "b buy n no y yes" -> keywords are words 1,3,5,...
	%opts = "";
	for(%i = 1; (%kw = GetWord(%trigs, %i)) != -1; %i += 2)
	{
		if(%opts == "")
			%opts = %kw;
		else
			%opts = %opts @ " " @ %kw;
	}
	%Client.knpcPendingOpts = %opts;
}

// --- End-of-turn options push --------------------------------------------
// Called once at the very end of BotChatStuff. If the conversation continues
// ($state still set) push the cached options; if it ended this turn push an
// empty list (only the client's auto "Goodbye" row shows).
function KronosNPC_RM_AfterChat(%Client, %closestId)
{
	if(!%Client.hasKronosHUD)
		return;
	if(%Client.knpcWinOpen == "")
		return;
	if($state[%Client, %closestId] != "")
		remoteEval(%Client, "KNPCOpts", %Client.knpcPendingOpts);
	else
		remoteEval(%Client, "KNPCOpts", "");
	%Client.knpcPendingOpts = "";
}

echo("KronosNPC_Server (RMRPG): NPC dialogue window bridge loaded");
