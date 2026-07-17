// ============================================================================
// ADVERTISEMENTS SYSTEM - Order-Based (ported from Kingdom of Kronos)
// Ads play in the order they are defined below (top to bottom)
// To add a new ad: Just call AddAd("line1", "line2", ...) anywhere
// To insert between existing ads: Just add a new AddAd() call where you want it
//
// Wiring: exec'd at createServer (defines functions early) and RE-exec'd a
// beat after mission load (Server.cs finishMissionLoad) because Tribes
// flushes pending schedules during mission load - the re-exec restarts the
// rotation via the hot-reload branch at the bottom.
// ============================================================================

// Reset ads when file loads (for hot-reload support)
$AdList = "";
$AdLineCount = "";
$AdTotalCount = 0;

// Helper function to register an ad (supports up to 5 lines per ad)
function AddAd(%line0, %line1, %line2, %line3, %line4)
{
	$AdTotalCount++;
	%idx = $AdTotalCount;

	// Store lines
	$AdLine[%idx, 0] = %line0;
	$AdLine[%idx, 1] = %line1;
	$AdLine[%idx, 2] = %line2;
	$AdLine[%idx, 3] = %line3;
	$AdLine[%idx, 4] = %line4;

	// Default: show to everyone. Set $AdSkipHud[%idx]=true after an AddAd() to
	// hide that ad from players already running the KronosHUD repack.
	$AdSkipHud[%idx] = false;
}

// ============================================================================
// DEFINE YOUR ADS HERE - Just add/remove/reorder AddAd() calls as needed
// Ads will play in the order they appear (top to bottom)
// ============================================================================

AddAd("Welcome to Red Moon RPG - Hosted by Jobo",
      "       Active development by Jobo");

AddAd("Found a bug? Report it in-game!",
      "Use: #bugreport your description here",
      "Example: #bugreport My weapon disappeared after I died");

AddAd("Download the Red Moon RPG repack:",
      "KingdomofKronos.com/downloads",
      "Includes the modern KronosHUD - bars, target frames, shop and bank GUI!");
// Players already running the KronosHUD repack don't need the download pitch.
$AdSkipHud[$AdTotalCount] = true;

AddAd("Want to suggest a feature or idea?",
      "Use #featurerequest or #requestfeature",
      "Your ideas could be added to the server!");

// ============================================================================
// JOIN NOTICE - sent to each player shortly after they enter the world
// (called from gameevents.cs ClientJustJoinedTeam on a short delay)
// ============================================================================
function RMJoinNotice(%clientId)
{
	if(Client::getName(%clientId) == "" || Client::getName(%clientId) == -1)
		return;
	Client::sendMessage(%clientId, 2, "Found a bug? Report it in-game with: #bugreport your description here~adv");
	// Skip the download pitch for players already on the KronosHUD repack.
	if(!%clientId.hasKronosHUD)
		Client::sendMessage(%clientId, 2, "Get the Red Moon RPG repack (modern HUD!) at KingdomofKronos.com/downloads~adv");
}

// ============================================================================
// ADVERTISEMENT ENGINE - Don't modify below unless you know what you're doing
// ============================================================================
// Rotation uses a generation/epoch guard instead of a stop-flag. Every (re)exec
// of this file bumps $AdEpoch; any Advertize() call carrying an older epoch is a
// stale chain from a previous load and terminates itself on its next tick.
//
// This replaces the old $AdvertizeStopped flag, which was broken: it set stop=1
// then reset it 1ms later, but a live chain's next call is 300s out, so it never
// saw the flag. Every mission-load re-exec (Server.cs) stacked another chain and
// ads posted twice (or more) in a row. The epoch has no timing window to miss.

function Advertize(%stage, %epoch)
{
	// Stale chain from a previous exec of this file -> die.
	if(%epoch != $AdEpoch)
		return;

	// Wrap around
	if(%stage < 1) %stage = 1;
	if(%stage > $AdTotalCount) %stage = 1;

	echo("Advertize stage " @ %stage @ " of " @ $AdTotalCount @ " (epoch " @ %epoch @ ")");

	// Send per-client so $AdSkipHud ads (the repack download pitch) can skip
	// players already running the KronosHUD repack. ~adv tag lets ChatFilter.cs
	// mute ads; the engine strips ~tags before display so vanilla clients see
	// no difference.
	for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
	{
		if($AdSkipHud[%stage] && %cl.hasKronosHUD)
			continue;

		for(%line = 0; %line < 5; %line++)
		{
			%text = $AdLine[%stage, %line];
			if(%text != "")
				Client::sendMessage(%cl, 2, %text @ "~adv");
		}
	}

	schedule("Advertize(" @ (%stage + 1) @ ", " @ %epoch @ ");", 300);
}

// (Re)start the rotation. Bump the epoch first so any chain still in flight from
// a previous exec sees a mismatched epoch on its next tick and stops -- exactly
// one chain is ever live. First ad fires after one interval (not instantly) so
// re-execs on mission load don't burst an ad the moment the map changes.
$AdEpoch++;
schedule("Advertize(1, " @ $AdEpoch @ ");", 300);
