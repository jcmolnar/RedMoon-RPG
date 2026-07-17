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
	// bump the close token so any close scheduled by a previous conversation's
	// end (KronosNPC_RM_AfterChat) no-ops instead of shutting this new one.
	%client.knpcCloseTok = %now;

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
// shortcut path alive for HUD clients too). For HUD clients it ALSO mirrors the
// full (key,word) pair list to the window via KNPCKeys, which uses it to turn a
// clicked option back into the exact word to #say and as a fallback button list.
// The window's per-turn options themselves come from the spoken prompt's
// <f1>..<f0> markers (mirrored through KNPCLine), NOT from this full table.
function RM_KNPC_SetUpKeys(%Client, %trigs)
{
	remoteEval(%Client, "SetUpKeys", %trigs);
	if(!%Client.hasKronosHUD)
		return;
	remoteEval(%Client, "KNPCKeys", %trigs);
}

// --- End-of-turn close ----------------------------------------------------
// Called once at the very end of BotChatStuff. When the conversation ended this
// turn ($state cleared) close the window after a short beat so the final spoken
// line is readable and any shop/bank/smith hand-off GUI can take the screen. A
// token guards against a conversation restarted within the delay (its OpenRM /
// AfterChat resets knpcCloseTok, so this scheduled close no-ops).
function KronosNPC_RM_AfterChat(%Client, %closestId)
{
	if(!%Client.hasKronosHUD)
		return;
	if(%Client.knpcWinOpen == "")
		return;
	if($state[%Client, %closestId] == "")
		KronosNPC_EndRM(%Client);
	else
		// Conversation still waiting on input. If this turn offered no
		// clickable options it's a FREE-TEXT turn (bank amount) - the client
		// shows its in-window amount input (ignored when options are up).
		remoteEval(%Client, "KNPCFree");
}

// Schedule the window to close because the conversation ended. Shared by
// KronosNPC_RM_AfterChat (normal end) and comchat3.cs CheckChatState (timeout),
// which clears $state without going through BotChatStuff.
function KronosNPC_EndRM(%Client)
{
	if(!%Client.hasKronosHUD)
		return;
	if(%Client.knpcWinOpen == "")
		return;
	%Client.knpcCloseTok = getSimTime();
	schedule("KronosNPC_CloseRM("@%Client@", "@%Client.knpcCloseTok@");", 4);
}

// Close the window for a client (server-initiated: conversation ended). The
// token must still match - a newer conversation bumps knpcCloseTok and wins.
function KronosNPC_CloseRM(%Client, %tok)
{
	if(%tok != "" && %tok != %Client.knpcCloseTok)
		return;
	if(%Client.knpcWinOpen == "")
		return;
	%Client.knpcWinOpen = "";
	remoteEval(%Client, "KNPCClose");
}

// Immediate close - used at shop/bank/smith GUI hand-offs so the dialogue
// window doesn't linger behind the stock inventory screen until the normal
// 4s end-of-conversation close fires. Bumps the token so that pending
// scheduled close no-ops. Self-gates: no-op for vanilla/window-closed.
function KronosNPC_ForceCloseRM(%Client)
{
	if(!%Client.hasKronosHUD)
		return;
	if(%Client.knpcWinOpen == "")
		return;
	%Client.knpcCloseTok = getSimTime() @ "f";
	KronosNPC_CloseRM(%Client, %Client.knpcCloseTok);
}

echo("KronosNPC_Server (RMRPG): NPC dialogue window bridge loaded");
