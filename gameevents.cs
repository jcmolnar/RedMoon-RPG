$hidelvls = true;
function Mission::init() {

	//if($displayPingAndPL)
	//	setClientScoreHeading("Name\t\x50Zone\t\xBFLVL\t\xDFPing\t\xFFPL");
	//else
	//	setClientScoreHeading("Name\t\x50Zone\t\xB2LVL\t\xD2Class\t\xFFPvP");

	//if($hidelvls)
	//	setClientScoreHeading("Warrior Name\t\x72Zone\t\xD0Class");
			setClientScoreHeading("Warrior Name"); //  \t\x79Zone");  //  \t\xD0Class");
	//else
	//	setClientScoreHeading("Warrior Name\t\x72Zone\t\xD0Level\t\xF6Class");

	if(!$NoSpawn)
		AI::setupAI();


	echo(".--==< RecursiveWorld STARTED >==--.");
      RecursiveWorld(5);
      RecursiveZone(2);
      RefreshStatus(5);
	exec("[RM]ServerStats.cs");
	if($OtherPrefs::BurnedClientId == "")
		$OtherPrefs::BurnedClientId = 1000;
	//exec("[RM]FireFloodLog.cs");
}

function Game::startMatch() {
	dbecho($dbechoMode, "Game::startMatch()");

	$matchStarted = true;
	$missionStartTime = getSimTime();
}

function Game::initialMissionDrop(%clientId) {

	Client::setGuiMode(%clientId, $GuiModePlay);
	%clientId.observerMode = "";

	//===================================================
	// Look for invalid characters in the player's name.
	// If none are found, LoadCharacter
	//===================================================

	%password = 0;
	%name = Client::getName(%clientId);
	%retval = FindInvalidChar(%name);
	%clientId.IsInvalid = False;

	for(%id = Client::getFirst(); %id != -1; %id = Client::getNext(%id)) {
		if(%clientId != %id && String::ICompare(%name, Client::getName(%id)) == 0) {
			%retval = "kick";
			break;
		}
	}

	if(%retval != "") {
		Observer::setFlyMode(%clientId, GameBase::getPosition(%observerMarker), GameBase::getRotation(%observerMarker), false, true);
		%kickMsg = "You are using invalid characters in your name.  Use a simpler name.  Suggested clan tag characters are dashes and underscores.";
		%clientId.IsInvalid = True;
	}
	%rw = CheckForReservedWords(%name);
	if(%rw != "") {
		Observer::setFlyMode(%clientId, GameBase::getPosition(%observerMarker), GameBase::getRotation(%observerMarker), false, true);
		%kickMsg = "You are using a reserved word in your name ("@%rw@").";
		%clientId.IsInvalid = True;
	}
	else {
		LoadCharacter(%clientId);
		
		//==================================================
		// Now that the profile is loaded, we can verify
		// the password.
		//==================================================

		%pkc = pk::bancheck(%clientid);	// Lets see if this character has pked to many people.

		if($password[%clientId] != $Client::info[%clientId, 5] && $password[%clientId] != "") {
			%clientId.IsInvalid = True;
			%password = 1;
		}
	}

	//==================================================
	// If there was invalid characters in the player's
	// name or the password was incorrect, then stick
	// the player in observer mode so he can be kicked
	// out soon after.
	//==================================================

	if(%clientId.IsInvalid) {
		if(%password == 1) {

			centerprint(%clientId, "This is a password protected character... You will have to enter a password by typing \"#logon mypassword\" or you will automatically be kicked within 20 seconds.  If this is not your character, please disconnect manually.", 0);
			schedule("check_password(" @ %clientId @ ",15);",5);
		}
		else {

			schedule("Net::kick(" @ %clientId @ ", \"" @ %kickMsg @ "\");", 20);
			centerprint(%clientId, %kickMsg @ "  You will automatically be kicked within 20 seconds.  If not, please disconnect manually.", 0);
		}
		RMSetObserver(%clientId);
	}
	else if(%pkc == "True")
	{
		schedule("Net::kick(" @ %clientId @ ", \"" @ %kickMsg @ "\");", 20);
		centerprint(%clientId, %kickMsg @ "  This character is banned for excessive PKing.", 0);
		RMSetObserver(%clientId);
	}
	else {
		//==================================================
		// Everything went fine, spawn the player (or make
		// him/her choose stats if creating a new char)
		//==================================================

		if(%clientId.choosingGroup) {
			StartStatSelection(%clientId);
			CheckClientFiles(%ClientId);
		}
		else {

			// Put the client into OBSERVER mode before the join menu. This
			// switches the client to PlayGui (where the ScriptGL KronosHUD
			// overlays + the modern menu panel render) AND positions the
			// observer camera at a real map point - so the "Join this world"
			// menu is actually VISIBLE instead of drawing invisibly over a
			// black void. (This replaces the old auto-join workaround, which
			// skipped the menu entirely because it couldn't be seen.)
			RMSetObserver(%clientId);

			// Probe for a HUD client (sets hasKronosHUD early; vanilla ignores it)
			remoteEval(%clientId, "KHudPing");

			// Kronos-style auto-join: returning characters spawn straight into the
			// world on login - no "Press 1 to join" menu. The 2s delay lets the
			// client finish switching to PlayGui (same timing the menu had). Only
			// NEW characters (choosingGroup branch above) see the observer cam,
			// for class selection. MenuJustJoined/ScheduleKick kept below in case
			// a manual join menu is ever needed again.
			$ClientWaiting[%clientId] = "True 1";
			Schedule("AutoJoinWorld("@%clientId@");", 2);
		}
	}
}

function AutoJoinWorld(%clientId) {

	if($ClientWaiting[%clientId] == "")	//disconnected during the 2s delay
		return;

	ClientJustJoinedTeam(%clientId);
}

function MenuJustJoined(%clientId, %t) {

	if($ClientWaiting[%clientId] == "")	//hehe oops
		return;

	// The client is in observer mode (PlayGui) by now - see the RMSetObserver
	// call in the join branch above - so this menu renders for both HUD
	// clients (modern KronosMenu panel) and vanilla clients (stock ChatMenu).
	if(%t == "")
		%msg = "Welcome!";
	else
		%msg = "Welcome! Join in "@%t@" seconds.";
	Client::buildMenu(%clientId, %msg, "justjoined", false);

	Client::addMenuItem(%clientId, "1Join this world.", "join");
	Client::addMenuItem(%clientId, "9Quit.", "quit");
}

function ScheduleKick(%clientId) {

	if(GetWord($ClientWaiting[%clientId], 0) != True)
		return;

	%w = GetWord($ClientWaiting[%clientId], 1);
	%cnt = 0;
	if(%w == 1) {
		for(%i = 60; %i > 0; %i--) {
			%cnt++;
			Schedule("MenuJustJoined("@%clientId@", "@%i@");", %cnt);
		}
		Schedule("ScheduleKick("@%clientId@");", 60);
		$ClientWaiting[%clientId] = "True 2";
	}
	else if(%w == 2) {
		Net::kick(%clientId, "You have been kicked because you waited to long on the login screen.\n\nYou have NOT been banned.");
		$ClientWaiting[%clientId] = "";
	 }
}

function processMenujustjoined(%clientId, %opt) {

	if(%opt == "join") {
		$ClientWaiting[%clientId] = "";
		ClientJustJoinedTeam(%ClientId);
	}
	else if(%opt == "quit") {
		$ClientWaiting[%clientId] = "";
		remoteEval(%ClientId, dropclient);
	}
	else
		MenuJustJoined(%ClientId);
}

function ClientJustJoinedTeam(%ClientId) {

	$ClientWaiting[%clientId] = "";
	Game::playerSpawn(%ClientId, false);
	RefreshAll(%ClientId);
	Schedule("remoteEval("@%ClientId@",\"ATKText\", \"<jc>Welcome to Red Moon RPG.\", wait);", 2);
	Schedule("CheckIsBlessed("@%clientId@");", 3);
	// join notice: #bugreport usage + repack download link (advertisements.cs)
	Schedule("RMJoinNotice("@%ClientId@");", 6);

	// Client-file check DELAYED 20s: with the KronosHUD auto-join this
	// function now runs ~2s after join-clear, while the client is still
	// loading the mission - the rmCheck probes couldn't answer inside
	// DeusScripts' 12s verdict window and every joiner got a false
	// "Not all of your needed files were loaded" warning. The old manual
	// "press 1 to join" step gave clients this grace implicitly.
	Schedule("CheckClientFiles("@%ClientId@");", 20);
}

function DoServerStatus(%clientId) {
	exec("[RM]ServerStats.cs");
	$BurnedClientTagId[%clientId] = $OtherPrefs::BurnedClientId;
	echo(">> New $BurnedClientTagId["@%clientId@"] = "@$OtherPrefs::BurnedClientId);
	$OtherPrefs::BurnedClientId++;
	export("$OtherPrefs::*", "Temp\\[RM]ServerStats.cs");

}

function CheckIsBlessed(%clientId) {

	if($isBlessed[%ClientId] == true)
		Client::sendmessage(%ClientId, 0, "Your Soul was blessed by the Gods of the Life Stream!");
	else {
		Client::sendmessage(%ClientId, 0, "Your Soul isn't blessed.");
		$isBlessed[%ClientId] = false;
	}
}
function RMSetObserver(%Client) {
	%group = nameToId("MissionGroup\\ObserverDropPoints");
	%observerMarker = Group::getObject(%group, 0);

	Client::setControlObject(%Client, Client::getObserverCamera(%Client));
	Observer::setFlyMode(%Client, GameBase::getPosition(%observerMarker), GameBase::getRotation(%observerMarker), false, true);
}

function Player::enterMissionArea(%player) { }	// Maybe we could use these somehow...
function Player::leaveMissionArea(%player) { }

function check_password(%clientId,%times_to_go)
{
	if($password[%clientId]!=$mypassword[%clientId])
	{
		if(%times_to_go>0)
		{
			%times_to_go--;
			schedule("check_password(" @ %clientId @ "," @ %times_to_go @ ");",1);
		}
		else
		{
			%kickMsg="You did not logon with the correct password. Change your password in \"Other info\" in your profile, or type \"#logon mypassword\".";
			Net::kick(%clientId,%kickMsg);
		}
	}
	else
	{
		%clientId.IsInvalid = false;
		centerprint(%clientId,"Password accepted.", 0);
		if(%clientId.choosingStats)
                  StartStatSelection(%clientId);
		else
			Game::playerSpawn(%clientId, false);
	}
}