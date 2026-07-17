function Game::pickObserverSpawn(%Client) {
	dbecho($dbechoMode2, "Game::pickObserverSpawn(" @ %Client @ ")");

	%group = nameToID("MissionGroup\\ObserverDropPoints");
	%count = Group::objectCount(%group);

	if(%group == -1 || !%count)
		%group = nameToID("MissionGroup\\Teams\\team0\\DropPoints");
	%count = Group::objectCount(%group);
	if(%group == -1 || !%count)
		%group = nameToID("MissionGroup\\Teams\\team0\\DropPoints");
	%count = Group::objectCount(%group);
	if(%group == -1 || !%count)
		return -1;
	%spawnIdx = %Client.lastObserverSpawn + 1;
	if(%spawnIdx >= %count)
		%spawnIdx = 0;
	%Client.lastObserverSpawn = %spawnIdx;

	return Group::getObject(%group, %spawnIdx);
}

function Game::pickPlayerSpawn(%Client, %respawn, %opt) {
	dbecho($dbechoMode2, "Game::pickPlayerSpawn(" @ %Client @ ", " @ %respawn @ ")");

	if($lastzone[%Client] == "" || %opt == True)
		%group = nameToID("MissionGroup/Teams/team0/DropPoints");
	else
		%group = nameToID("MissionGroup/Zones/" @ Object::getName($lastzone[%Client]) @ "/DropPoints");

	%count = Group::objectCount(%group);
	if(!%count)
		return -1;
	%spawnIdx = floor(getRandom() * (%count - 0.1));
	%value = %count;

	for(%i = %spawnIdx; %i < %value; %i++)
	{
		%set = newObject("set",SimSet);
		%obj = Group::getObject(%group, %i);
		if(containerBoxFillSet(%set,$SimPlayerObjectType|$VehicleObjectType,GameBase::getPosition(%obj),2,2,4,0) == 0)
		{
			deleteObject(%set);
			return %obj;
		}
		if(%i == %count - 1)
		{
			%i = -1;
			%value = %spawnIdx;
		}
		deleteObject(%set);
	}
	return false;
}

function Game::playerSpawn(%Client, %respawn) {
	dbecho($dbechoMode2, "Game::playerSpawn(" @ %Client @ ", " @ %respawn @ ")");

	if(!$ghosting)
		return false;

	Client::clearItemShopping(%Client);
	Client::clearItemBuying(%Client);

	%spawnMarker = Game::pickPlayerSpawn(%Client, %respawn);

	if(%spawnMarker)
	{
		%Client.guiLock = "";
		%Client.dead = "";
		if(%spawnMarker == -1)
		{
			%spawnPos = "0 0 600";
			%spawnRot = "0 0 0";
		}
		else if($campPos[%Client] != "" && !%respawn)
		{
			//if the player HAS a $campPos and it is his FIRST TIME SPAWNING, then spawn him at this campPos
			%spawnPos = $campPos[%Client];
			%spawnRot = "0 0 0";
			remoteEval(%Client, "DoCmd", "OnJoinedTeam");
		}
		else
		{
			%spawnPos = GameBase::getPosition(%spawnMarker);
			%spawnRot = GameBase::getRotation(%spawnMarker);
		}

		%armor = $RaceToArmorType[$RACE[%Client]];

		if(isObject(Client::getOwnedObject(%Client)))
			deleteObject(Client::getOwnedObject(%Client));

		%pl = spawnPlayer(%armor, %spawnPos, %spawnRot);
		PlaySound(SoundSpawn2, %spawnPos);
		GameBase::startFadeIn(Client::getOwnedObject(%Client));

		echo("SPAWN: [CLIENT:"@%Client@" | PLAYER:"@%pl@" | MARKER:"@%spawnMarker@" | POS:"@%spawnPos@" | ARMOR:"@%armor@"]");

		if(%pl != -1) {
			UpdateTeam(%Client);
			GameBase::setTeam(%pl, Client::getTeam(%Client));
			Client::setOwnedObject(%Client, %pl);
			Client::setControlObject(%Client, %pl);
			Game::playerSpawned(%pl, %Client, %armor, %respawn);

			Client::setControlObject(%Client, %pl);
			$dumbAIflag[%Client.possessId] = "";

			SetOnGround(%Client, 1);

			// KronosHUD handshake trigger. HUD clients answer with KHudOn
			// (-> remoteKHudOn -> hasKronosHUD). Vanilla clients don't define
			// remoteKHudPing and harmlessly ignore it. Throttled client-side.
			remoteEval(%Client, "KHudPing");
		}
		return true;
	}
	else
	{
		Client::sendMessage(%Client, 0, "Sorry No Respawn Positions Are Empty - Trying again in 3 seconds...");
		Schedule("Game::playerSpawn("@%Client@", "@%respawn@");", 3);
		return false;
	}
}

function Game::playerSpawned(%pl, %Client, %armor) {

	dbecho($dbechoMode2, "Game::playerSpawned(" @ %pl @ ", " @ %Client @ ", " @ %armor @ ")");

	$HasLoadedAndSpawned[%Client] = True;

	$SpellCastStep[%Client] = "";
	RefreshAll(%Client);
	UpdateSkin(%Client);
	remotetoggleShield(%Client); //if they had an shield on it'll whip it out for them =p
	Game::refreshClientScore(%Client);

	if($ClientData[%Client, JustJoined]) {
		$ClientData[%Client, JustJoined] = "";
		refreshHP(%Client, -$Client::tmp[%Client, hp]);
		refreshMANA(%Client, -$Client::tmp[%Client, mp]);
		refreshSTAMINA(%Client, -$Client::tmp[%Client, sta]);
	}
	else {
		setHP(%Client, $MaxHP[%Client]);
		setSTAMINA(%Client, $MaxSTA[%Client]);
	}

	GiveThisStuff(%Client, $Client::tmp[%Client, "ItemList"], false);
	GiveThisStuff(%Client, $Client::tmp[%Client, "EquipList"], false);
	GiveThisStuff(%Client, $Client::tmp[%Client, "QuestList"], false);

	deleteVariables("Client::tmp"@%Client@"*");

	RefreshAll(%Client);

	//re-equip the last used weapon (Kronos-style weapon persistence).
	//$ClientData[SavedWeapon] carries it across login (save field 48);
	//UsingWeapon carries it across a respawn within the session.
	if(!Player::isAiControlled(%Client)) {
		%lastWeap = $ClientData[%Client, SavedWeapon];
		if(%lastWeap == "" || %lastWeap == "-1")
			%lastWeap = $ClientData[%Client, UsingWeapon];
		$ClientData[%Client, SavedWeapon] = "";

		if(%lastWeap != "" && %lastWeap != "-1") {
			//only if the weapon is actually in their inventory
			%wlist = Client::getItemListByClass(%Client, "Weapon", "ItemList");
			for(%wi = 0; (%wit = GetWord(%wlist, %wi)) != -1; %wi++) {
				if(%wit == %lastWeap) {
					RPGmountItem(%pl, %lastWeap, $WeaponSlot);
					break;
				}
			}
		}
	}
}

function Game::autoRespawn(%Client)
{
	dbecho($dbechoMode2, "Game::autoRespawn(" @ %Client @ ")");

	if(%Client.dead == 1)
		Game::playerSpawn(%Client, True);
}