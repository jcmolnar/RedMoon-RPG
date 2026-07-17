// ============================================================================
// CHOCOBO TRAINER + RARITY / BREEDING (2026 completion of Deus' 0.49 Alpha)
// ============================================================================
// Adds the missing acquisition path: a "chocotrainer" townbot (townbots.cs)
// whose dialogue (comchat3.cs) opens the buy menu below, plus the color
// rarity/price tiers and the breeding implementation Chocobo.cs stubs call.
//
// RARITY MODEL (classic FF lineage, mapped to the 7 vehicle skins):
//   Tier 1  Yellow            - common, cheap, modest stats
//   Tier 2  Red, Blue, Green  - uncommon
//   Tier 3  Black, White      - rare, expensive, strong stats
//   Tier 4  Gold              - NOT SOLD; bred only (Black x White chance),
//                               fastest mount (ChocoboSkins.cs maxSpeed 55)
// exec'd from Chocobo.cs.
// ============================================================================

// ---- price / stat-roll tables -------------------------------------------
$Choco::Price["Yellow"] = 25000;
$Choco::Price["Red"]    = 100000;
$Choco::Price["Blue"]   = 100000;
$Choco::Price["Green"]  = 100000;
$Choco::Price["Black"]  = 400000;
$Choco::Price["White"]  = 600000;
// Gold: no price on purpose - breeding only.

$Choco::Tier["Yellow"] = 1;
$Choco::Tier["Red"]   = 2; $Choco::Tier["Blue"] = 2; $Choco::Tier["Green"] = 2;
$Choco::Tier["Black"] = 3; $Choco::Tier["White"] = 3;
$Choco::Tier["Gold"]  = 4;

// base stat roll range per tier (each of STR/DEX/CON/INT/WIS)
$Choco::StatMin[1] = 10;  $Choco::StatMax[1] = 25;
$Choco::StatMin[2] = 20;  $Choco::StatMax[2] = 40;
$Choco::StatMin[3] = 35;  $Choco::StatMax[3] = 60;
$Choco::StatMin[4] = 55;  $Choco::StatMax[4] = 85;

function Chocobo::TierOf(%color) {
	%t = $Choco::Tier[%color];
	if(%t == "")
		%t = 1;
	return %t;
}

function Chocobo::RandColorAtTier(%tier) {
	if(%tier <= 1) return "Yellow";
	if(%tier == 2) {
		%r = Between(1, 3);
		if(%r == 1) return "Red";
		if(%r == 2) return "Blue";
		return "Green";
	}
	if(%tier == 3) {
		if(Between(1, 2) == 1) return "Black";
		return "White";
	}
	return "Gold";
}

// ---- buy a new bird -------------------------------------------------------
function Chocobo::RollNew(%Client, %color) {

	%price = $Choco::Price[%color];
	if(%price == "") {
		Client::sendMessage(%Client, 0, "Gold Chocobos can't be bought - only bred from the finest Black and White stock!");
		return;
	}
	if($Chocobo[%Client] >= $MaxChocobo) {
		Client::sendMessage(%Client, 0, "Your stable is full! ("@$Chocobo[%Client]@"/"@$MaxChocobo@") Release or sell one first.");
		return;
	}
	if($COINS[%Client] < %price) {
		Client::sendMessage(%Client, 1, "You can't afford a "@%color@" Chocobo! ("@FixM(%price)@" gil)");
		return;
	}

	$COINS[%Client] -= %price;
	RefreshAll(%Client);

	%tier = Chocobo::TierOf(%color);
	%STR = Between($Choco::StatMin[%tier], $Choco::StatMax[%tier]);
	%DEX = Between($Choco::StatMin[%tier], $Choco::StatMax[%tier]);
	%CON = Between($Choco::StatMin[%tier], $Choco::StatMax[%tier]);
	%INT = Between($Choco::StatMin[%tier], $Choco::StatMax[%tier]);
	%WIS = Between($Choco::StatMin[%tier], $Choco::StatMax[%tier]);
	if(Between(1, 2) == 1)
		%sex = "Male";
	else
		%sex = "Female";
	%name = $ChocoboNames[Between(1, 5)] @ Between(2, 99);

	Chocobo::Get(%Client, buy, %name, %color, %sex, 0, 0, 0,
		%STR, %DEX, %CON, %INT, %WIS, 0, 100, 100, floor(%price / 2), 0, 0, 0, 0, 0);

	// find the slot it landed in so the rename prompt targets it
	for(%i = 1; %i <= $MaxChocobo; %i++)
		if($ChocoboName[%Client, %i] == %name)
			Chocobo::NewName(%Client, NewName, %i);
}

// ---- trainer menus ---------------------------------------------------------
function MenuChocoboTrainer(%Client) {
	Client::buildMenu(%Client, "Chocobo Trainer", "ChocoTrainer", true);
	Client::addMenuItem(%Client, "1Buy a Chocobo", "BuyBird");
	if($Chocobo[%Client] >= 1)
		Client::addMenuItem(%Client, "2My Chocobos (feed/ride/sell...)", "Stable");
	Client::addMenuItem(%Client, "xNevermind", "Bye");
}

function processMenuChocoTrainer(%Client, %opt) {
	if(%opt == "BuyBird")
		MenuChocoboBuy(%Client);
	else if(%opt == "Stable")
		MenuChocobo(%Client);
}

function MenuChocoboBuy(%Client) {
	Client::buildMenu(%Client, "Buy which Chocobo?\n(stats improve with rarity)", "ChocoBuy", true);
	Client::addMenuItem(%Client, "1Yellow - "@FixM($Choco::Price["Yellow"])@" gil", "Yellow");
	Client::addMenuItem(%Client, "2Red - "@FixM($Choco::Price["Red"])@" gil", "Red");
	Client::addMenuItem(%Client, "3Blue - "@FixM($Choco::Price["Blue"])@" gil", "Blue");
	Client::addMenuItem(%Client, "4Green - "@FixM($Choco::Price["Green"])@" gil", "Green");
	Client::addMenuItem(%Client, "5Black - "@FixM($Choco::Price["Black"])@" gil", "Black");
	Client::addMenuItem(%Client, "6White - "@FixM($Choco::Price["White"])@" gil", "White");
	Client::addMenuItem(%Client, "7Gold - breeding only!", "Gold");
	Client::addMenuItem(%Client, "x<<Back", "Back");
}

function processMenuChocoBuy(%Client, %opt) {
	if(%opt == "Back") {
		MenuChocoboTrainer(%Client);
		return;
	}
	Chocobo::RollNew(%Client, %opt);
}

// ---- breeding --------------------------------------------------------------
// Offspring color: same tier bias toward the parents, with mutation chances.
//   Black x White        -> 15% GOLD, else 50/50 Black or White
//   same color           -> 70% that color, 25% random tier-2, 5% random tier-3
//   different colors     -> 45/45 either parent, 10% mutate one tier up
function Chocobo::BreedColor(%c1, %c2) {

	if((%c1 == "Black" && %c2 == "White") || (%c1 == "White" && %c2 == "Black")) {
		if(Between(1, 100) <= 15)
			return "Gold";
		if(Between(1, 2) == 1) return "Black";
		return "White";
	}
	if(%c1 == %c2) {
		%r = Between(1, 100);
		if(%r <= 70) return %c1;
		if(%r <= 95) return Chocobo::RandColorAtTier(2);
		return Chocobo::RandColorAtTier(3);
	}
	%r = Between(1, 100);
	if(%r <= 45) return %c1;
	if(%r <= 90) return %c2;
	%up = Chocobo::TierOf(%c1);
	if(Chocobo::TierOf(%c2) > %up)
		%up = Chocobo::TierOf(%c2);
	%up++;
	if(%up > 3) %up = 3;   // tier-up mutation caps at Black/White; Gold is BlackxWhite only
	return Chocobo::RandColorAtTier(%up);
}

// Validate a breeding pair. Returns "" if OK, else the reason (sent to both).
function Chocobo::BreedCheck(%Host, %HostChoco, %Buyer, %BuyerChoco) {
	if($Chocobo[%Host, %HostChoco] != true || $Chocobo[%Buyer, %BuyerChoco] != true)
		return "That Chocobo doesn't exist anymore!";
	if($ChocoboSex[%Host, %HostChoco] == $ChocoboSex[%Buyer, %BuyerChoco])
		return "You need a Male and a Female to breed! ("@$ChocoboSex[%Host, %HostChoco]@" + "@$ChocoboSex[%Buyer, %BuyerChoco]@")";
	if($ChocoboYAge[%Host, %HostChoco] < 1 || $ChocoboYAge[%Buyer, %BuyerChoco] < 1)
		return "Both Chocobos must be at least 1 year old to breed!";
	if($ChocoboHungry[%Host, %HostChoco] < 50 || $ChocoboHungry[%Buyer, %BuyerChoco] < 50)
		return "Both Chocobos must be well fed (50%+) to breed!";
	if($Chocobo[%Host] >= $MaxChocobo)
		return "The initiating player needs a free stable slot for the chick!";
	return "";
}

// The actual offspring creation (called from Chocobo::Breed "Breeding" branch
// after the partner confirms). Chick goes to the INITIATOR (%Host).
function Chocobo::BreedBirth(%Host, %Buyer) {

	%HostChoco = $MenuBreedChocoboId::Host[%Host];
	%BuyerChoco = $MenuBreedChocoboId::Buyer[%Host];

	%why = Chocobo::BreedCheck(%Host, %HostChoco, %Buyer, %BuyerChoco);
	if(%why != "") {
		Client::sendMessage(%Host, 1, %why);
		Client::sendMessage(%Buyer, 1, %why);
		return;
	}

	%color = Chocobo::BreedColor($ChocoboColor[%Host, %HostChoco], $ChocoboColor[%Buyer, %BuyerChoco]);
	%tier = Chocobo::TierOf(%color);

	// per-stat: average of parents + variance, floored at the tier minimum
	%STR = floor(($ChocoboSTR[%Host, %HostChoco] + $ChocoboSTR[%Buyer, %BuyerChoco]) / 2) + Between(-5, 10);
	%DEX = floor(($ChocoboDEX[%Host, %HostChoco] + $ChocoboDEX[%Buyer, %BuyerChoco]) / 2) + Between(-5, 10);
	%CON = floor(($ChocoboCON[%Host, %HostChoco] + $ChocoboCON[%Buyer, %BuyerChoco]) / 2) + Between(-5, 10);
	%INT = floor(($ChocoboINT[%Host, %HostChoco] + $ChocoboINT[%Buyer, %BuyerChoco]) / 2) + Between(-5, 10);
	%WIS = floor(($ChocoboWIS[%Host, %HostChoco] + $ChocoboWIS[%Buyer, %BuyerChoco]) / 2) + Between(-5, 10);
	if(%STR < $Choco::StatMin[%tier]) %STR = $Choco::StatMin[%tier];
	if(%DEX < $Choco::StatMin[%tier]) %DEX = $Choco::StatMin[%tier];
	if(%CON < $Choco::StatMin[%tier]) %CON = $Choco::StatMin[%tier];
	if(%INT < $Choco::StatMin[%tier]) %INT = $Choco::StatMin[%tier];
	if(%WIS < $Choco::StatMin[%tier]) %WIS = $Choco::StatMin[%tier];

	if(Between(1, 2) == 1)
		%sex = "Male";
	else
		%sex = "Female";
	%name = $ChocoboNames[Between(1, 5)] @ Between(2, 99);

	// breeding is tiring - both parents lose 30 hunger
	$ChocoboHungry[%Host, %HostChoco] -= 30;
	$ChocoboHungry[%Buyer, %BuyerChoco] -= 30;

	Chocobo::Get(%Host, bred, %name, %color, %sex, 0, 0, 0,
		%STR, %DEX, %CON, %INT, %WIS, 0, 100, 100, floor((%STR+%DEX+%CON+%INT+%WIS) * 30), 0, 0, 0, 0, 0);

	Client::sendMessage(%Buyer, 0, "Kweh! A baby "@%color@" Chocobo was born to "@Client::getName(%Host)@"'s stable!");
	if(%color == "Gold") {
		MessageAll(2, Client::getName(%Host)@" and "@Client::getName(%Buyer)@" bred a legendary GOLD Chocobo!~wSoundHitBell1.wav");
	}

	// let the initiator name the chick
	for(%i = 1; %i <= $MaxChocobo; %i++)
		if($ChocoboName[%Host, %i] == %name)
			Chocobo::NewName(%Host, NewName, %i);
}
