//By Deus_ex_Machina
//
//
	$Chocobover = "0.49 Alpha";

$MaxChocobo = "6"; //8 is max for now...(Menu shows up to 8 lines per page)
		//Careing of one Chocobo is hard and costs $$
		//But you can make good money if you have a good Chocobo!

//New Save vars
//$Chocobo[%Client] //Num of Chocobo Client has
//$Chocobo[%Client, %i] //Keep track of the Chocobos...
//$ChocoboTakeCare[%Client, %i] //You can pay a trainer to care for your Chocobo(s).
//$ChocoboName[%Client, $Chocobo[%Client, %i]] // Name
//$ChocoboColor[%Client, $Chocobo[%Client, %i]] // Color
//$ChocoboSex[%Client, $Chocobo[%Client, %i]] // Sex (Male or Female)
//$ChocoboYAge[%Client, $Chocobo[%Client, %i]] // Age Years = 40 days
//$ChocoboDAge[%Client, $Chocobo[%Client, %i]] // Age Days = 15 mins(real time)
//$ChocoboTempAge[%Client, $Chocobo[%Client, %i]] // Temp Age (Tracks mins)
//$ChocoboSTR[%Client, $Chocobo[%Client, %i]] //STR
//$ChocoboDEX[%Client, $Chocobo[%Client, %i]] //DEX
//$ChocoboCON[%Client, $Chocobo[%Client, %i]] //CON
//$ChocoboINT[%Client, $Chocobo[%Client, %i]] //INT
//$ChocoboWIS[%Client, $Chocobo[%Client, %i]] //WIS
//$ChocoboEXP[%Client, $Chocobo[%Client, %i]] //EXP
//$ChocoboHealth[%Client, $Chocobo[%Client, %i]] //Health (in %. 100% being max...) 100% = super happy 0% = he WILL die... random chance of dieing/running away at 10% maybe higher?
//$ChocoboHungry[%Client, $Chocobo[%Client, %i]] //Full (in %. 100% means his full! If 0% he WILL die)
//$ChocoboWorth[%Client, $Chocobo[%Client, %i]] // $$$ =p
//$ChocoboMeats[%Client, $Chocobo[%Client, %i]] //Meat = STR+
//$ChocoboFruits[%Client, $Chocobo[%Client, %i]] //Fruits etc = INT+
//$ChocoboVits[%Client, $Chocobo[%Client, %i]] //Vit = CON+
//$ChocoboSeeds[%Client, $Chocobo[%Client, %i]] //Seeds = WIS+
//$ChocoboCandies[%Client, $Chocobo[%Client, %i]] //Candies = DEX+

%i = 0;
$ChocoboNames[%i++] = "Choco";	//Need more names!
$ChocoboNames[%i++] = "Coco";
$ChocoboNames[%i++] = "Ruby";
$ChocoboNames[%i++] = "Luke";
$ChocoboNames[%i++] = "Lucky";
%i = "";

//       Chocobo::Get(2049,  found, "Choco", "Red" , "Male", 1,    13,     14,     50,   46,   20,   75,   71,  83128,   76,       79,     412,    0,       0,     0,      0,     0);
function Chocobo::Get(%Client, %opt, %name, %color, %sex, %YAge, %DAge, %TempAge, %STR, %DEX, %CON, %INT, %WIS, %EXP, %Health, %Hungry, %Worth, %Meats, %Fruits, %Vits, %Seeds, %Candies) { //Buy, Quest or Found
	if($Chocobo[%Client] < $MaxChocobo) {																																														//Found Chocobo always have a name. =\
		if($Chocobo[%Client] == "")
			$Chocobo[%Client] = 0;
		$Chocobo[%Client]++;
		%SaveSlot = Chocobo::GetSaveSlot(%Client);
	}
	else {
		if(%opt == found)
			%opt = find;
		if(%opt == quest)
			Client::sendMessage(%Client, 1, "The Chocobo trainer tells you, Sorry. You have too many Chocobos! ("@$Chocobo[%Client]@") Come back if you release one.");
		else
			Client::sendMessage(%Client, 0, "You have to many Chocobos! ("@$Chocobo[%Client]@") Release one if you want to "@%opt@" new ones!");
		return;
	}
	if(%opt == buy) {
		Client::sendMessage(%Client, 0, "You bought a "@%color@" Chocobo!");
	}
	else if(%opt == found) {
		Client::sendMessage(%Client, 0, "You found a "@%color@" Chocobo!");
		Client::sendMessage(%Client, 2, %name@" tells you, Kweh! Now that you caught me you better take good care of me!");
	}
	else if(%opt == quest)
		Client::sendMessage(%Client, 0, "The Chocobo trainer gave you a "@%color@" Chocobo!");
	else if(%opt == bred)
		Client::sendMessage(%Client, 0, "Kweh kweh! A baby "@%color@" Chocobo hatched into your stable!");

	Chocobo::Add(%Client, %opt, %SaveSlot, %name, %color, %sex, %YAge, %DAge, %TempAge, %STR, %DEX, %CON, %INT, %WIS, %EXP, %Health, %Hungry, %Worth, %Meats, %Fruits, %Vits, %Seeds, %Candies);
}

function Chocobo::Trade(%ClientHost, %ClientBuyer, %opt) { //Trade or Sell
	%Buyername = Client::getname(%ClientBuyer);
	%Hostname = Client::getname(%ClientHost);

	//$MenuTradeBuyerId[%Client]
	$MenuTradeHostId[%ClientBuyer] = %ClientHost;	//Save host Id on buyer
		//$MenuTradeChocoboId::Host[%Client] = "";
		//$MenuTradeChocoboId::Buyer[%Client] = "";

	if($Chocobo[%ClientHost] <= "0") {	//The "host" always has to have one Chocobo
		Client::sendMessage(%ClientHost, 0, "You don't have a Chocobo to "@%opt@"!");
		return false;
	}
	else if(%opt == JustChecking)
		return true;
	if(%opt == trade) {
		if($Chocobo[%ClientBuyer] <= "0") {	//If the host wants to trade the other guy has to have one
			Client::sendMessage(%ClientHost, 0, %Buyername@" doesn't have a Chocobo to trade with!");
			$MenuTradeChocoboId::Host[%ClientHost] = "";
			$MenuTradeChocoboId::Buyer[%ClientHost] = "";
			return false;
		}
		else {
			$IsTradeing[%ClientHost] = true;
			$IsTradeing[%ClientBuyer] = true;
			Client::sendMessage(%ClientBuyer, 0, %Hostname@" wants to trade his "@$ChocoboName[%ClientHost, $MenuTradeChocoboId::Host[%ClientHost]]@" for your "@$ChocoboName[%ClientBuyer, $MenuTradeChocoboId::Buyer[%ClientBuyer]]);
			Client::sendMessage(%ClientBuyer, 0, "Type '#trade yes' if you want to make this trade! Or '#trade no' if you don't.");
			$CanTrade[%ClientBuyer] = false; //if true(for both) then trade goes thru..
			$CanTrade[%ClientHost] = true;  //Host can always type #trade no if he changes his mind before buyer trades.
		}
	}
	if(%opt == Sell) {
		$IsTradeing[%ClientHost] = true;
		$IsTradeing[%ClientBuyer] = true;
		Client::sendMessage(%ClientBuyer, 0, %Hostname@" wants to sell his "@$ChocoboName[%ClientHost, $MenuTradeChocoboId::Host[%ClientHost]]@" for "@FixM($ChocoboWorth[%ClientHost, $MenuTradeChocoboId::Host[%ClientHost]]));
		Client::sendMessage(%ClientBuyer, 0, "Type '#buy yes' if you want to buy this Chocobo! Or '#buy no' if you don't.");
		$CanBuyChocobo[%ClientBuyer] = $MenuTradeChocoboId::Host[%ClientHost];
	//
	}
	if(%opt == failedbuyer) {
		Client::sendMessage(%ClientBuyer, 1, "You canceled the trade!");
		Client::sendMessage(%ClientHost, 1, %Buyername@" canceled the trade!");
		$CanTrade[%ClientBuyer] = "";
		$CanTrade[%ClientHost] = "";
		$MenuTradeBuyerId[%ClientHost] = "";
		$MenuTradeHostId[%ClientBuyer] = "";
		$MenuTradeChocoboId::Host[%ClientHost] = "";
		$MenuTradeChocoboId::Buyer[%ClientHost] = "";
		$IsTradeing[%ClientHost] = "";
		$IsTradeing[%ClientBuyer] = "";
	}
	if(%opt == failedhost) {
		Client::sendMessage(%ClientHost, 1, "You canceled the trade!");
		Client::sendMessage(%ClientBuyer, 1, %Hostname@" canceled the trade!");
		$CanTrade[%ClientBuyer] = "";
		$CanTrade[%ClientHost] = "";
		$MenuTradeBuyerId[%ClientHost] = "";
		$MenuTradeHostId[%ClientBuyer] = "";
		$MenuTradeChocoboId::Host[%ClientHost] = "";
		$MenuTradeChocoboId::Buyer[%ClientHost] = "";
		$IsTradeing[%ClientHost] = "";
		$IsTradeing[%ClientBuyer] = "";
	}
	if(%opt == traded) {
		%HostChoco = $MenuTradeChocoboId::Host[%ClientHost];
		%BuyerChoco = $MenuTradeChocoboId::Buyer[%ClientHost];
		Chocobo::Switch(%ClientHost, %ClientBuyer, %HostChoco, %BuyerChoco, None);
		$CanTrade[%ClientBuyer] = "";
		$CanTrade[%ClientHost] = "";
		$MenuTradeBuyerId[%ClientHost] = "";
		$MenuTradeHostId[%ClientBuyer] = "";
		$MenuTradeChocoboId::Host[%ClientHost] = "";
		$MenuTradeChocoboId::Buyer[%ClientHost] = "";
		$IsTradeing[%ClientHost] = "";
		$IsTradeing[%ClientBuyer] = "";
	}

}

function Chocobo::Breed(%ClientHost, %ClientBuyer, %opt) {

	%Buyername = Client::getname(%ClientBuyer);
	%Hostname = Client::getname(%ClientHost);

	$MenuBreedHostId[%ClientBuyer] = %ClientHost;

	if(%opt == "Breed") {
		// pre-validate before bothering the partner (ChocoboTrainer.cs)
		%why = Chocobo::BreedCheck(%ClientHost, $MenuBreedChocoboId::Host[%ClientHost], %ClientBuyer, $MenuBreedChocoboId::Buyer[%ClientHost]);
		if(%why != "") {
			Client::sendMessage(%ClientHost, 1, %why);
			return;
		}
		$IsTradeing[%ClientHost] = true;
		$IsTradeing[%ClientBuyer] = true;
		$CanBreed[%ClientBuyer] = false;  // buyer must confirm
		$CanBreed[%ClientHost] = true;    // host may cancel
		Client::sendMessage(%ClientBuyer, 0, %Hostname@" wants to breed his "@$ChocoboName[%ClientHost, $MenuBreedChocoboId::Host[%ClientHost]]@" with your "@$ChocoboName[%ClientBuyer, $MenuBreedChocoboId::Buyer[%ClientHost]]@"! (The chick goes to "@%Hostname@".)");
		Client::sendMessage(%ClientBuyer, 0, "Type '#breed yes' to accept! Or '#breed no' to refuse.");
		Client::sendMessage(%ClientHost, 0, "Waiting for "@%Buyername@" to accept... (they type #breed yes)");
	}
	else if(%opt == "Breeding") {
		Chocobo::BreedBirth(%ClientHost, %ClientBuyer);
		Chocobo::BreedClearState(%ClientHost, %ClientBuyer);
	}
	else if(%opt == "failedbuyer") {
		Client::sendMessage(%ClientBuyer, 1, "You canceled!");
		Client::sendMessage(%ClientHost, 1, %Buyername@" canceled!");
		Chocobo::BreedClearState(%ClientHost, %ClientBuyer);
	}
	else if(%opt == "failedhost") {
		Client::sendMessage(%ClientHost, 1, "You canceled!");
		Client::sendMessage(%ClientBuyer, 1, %Hostname@" canceled!");
		Chocobo::BreedClearState(%ClientHost, %ClientBuyer);
	}
}

function Chocobo::BreedClearState(%ClientHost, %ClientBuyer) {
	$IsTradeing[%ClientHost] = "";
	$IsTradeing[%ClientBuyer] = "";
	$CanBreed[%ClientHost] = "";
	$CanBreed[%ClientBuyer] = "";
	$MenuBreedBuyerId[%ClientHost] = "";
	$MenuBreedHostId[%ClientBuyer] = "";
	$MenuBreedChocoboId::Host[%ClientHost] = "";
	$MenuBreedChocoboId::Buyer[%ClientHost] = "";
}

function Chocobo::Switch(%Client, %ClientBuyer, %Choco, %BuyerChoco, %opt) {
	// Rewritten 2026-07-17: a TRADE is a swap - neither side's bird count
	// changes, so no free slot is needed. The old version freed-then-refilled
	// slots and, when both stables were full, fell into a corrupt overflow
	// path (hardcoded slot 20 outside $MaxChocobo, stale indices). Explicit
	// in-place swap of all 21 per-bird fields.
	%t = $ChocoboName[%Client, %Choco]; $ChocoboName[%Client, %Choco] = $ChocoboName[%ClientBuyer, %BuyerChoco]; $ChocoboName[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboColor[%Client, %Choco]; $ChocoboColor[%Client, %Choco] = $ChocoboColor[%ClientBuyer, %BuyerChoco]; $ChocoboColor[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboSex[%Client, %Choco]; $ChocoboSex[%Client, %Choco] = $ChocoboSex[%ClientBuyer, %BuyerChoco]; $ChocoboSex[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboYAge[%Client, %Choco]; $ChocoboYAge[%Client, %Choco] = $ChocoboYAge[%ClientBuyer, %BuyerChoco]; $ChocoboYAge[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboDAge[%Client, %Choco]; $ChocoboDAge[%Client, %Choco] = $ChocoboDAge[%ClientBuyer, %BuyerChoco]; $ChocoboDAge[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboTempAge[%Client, %Choco]; $ChocoboTempAge[%Client, %Choco] = $ChocoboTempAge[%ClientBuyer, %BuyerChoco]; $ChocoboTempAge[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboSTR[%Client, %Choco]; $ChocoboSTR[%Client, %Choco] = $ChocoboSTR[%ClientBuyer, %BuyerChoco]; $ChocoboSTR[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboDEX[%Client, %Choco]; $ChocoboDEX[%Client, %Choco] = $ChocoboDEX[%ClientBuyer, %BuyerChoco]; $ChocoboDEX[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboCON[%Client, %Choco]; $ChocoboCON[%Client, %Choco] = $ChocoboCON[%ClientBuyer, %BuyerChoco]; $ChocoboCON[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboINT[%Client, %Choco]; $ChocoboINT[%Client, %Choco] = $ChocoboINT[%ClientBuyer, %BuyerChoco]; $ChocoboINT[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboWIS[%Client, %Choco]; $ChocoboWIS[%Client, %Choco] = $ChocoboWIS[%ClientBuyer, %BuyerChoco]; $ChocoboWIS[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboEXP[%Client, %Choco]; $ChocoboEXP[%Client, %Choco] = $ChocoboEXP[%ClientBuyer, %BuyerChoco]; $ChocoboEXP[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboHealth[%Client, %Choco]; $ChocoboHealth[%Client, %Choco] = $ChocoboHealth[%ClientBuyer, %BuyerChoco]; $ChocoboHealth[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboHungry[%Client, %Choco]; $ChocoboHungry[%Client, %Choco] = $ChocoboHungry[%ClientBuyer, %BuyerChoco]; $ChocoboHungry[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboWorth[%Client, %Choco]; $ChocoboWorth[%Client, %Choco] = $ChocoboWorth[%ClientBuyer, %BuyerChoco]; $ChocoboWorth[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboMeats[%Client, %Choco]; $ChocoboMeats[%Client, %Choco] = $ChocoboMeats[%ClientBuyer, %BuyerChoco]; $ChocoboMeats[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboFruits[%Client, %Choco]; $ChocoboFruits[%Client, %Choco] = $ChocoboFruits[%ClientBuyer, %BuyerChoco]; $ChocoboFruits[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboVits[%Client, %Choco]; $ChocoboVits[%Client, %Choco] = $ChocoboVits[%ClientBuyer, %BuyerChoco]; $ChocoboVits[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboSeeds[%Client, %Choco]; $ChocoboSeeds[%Client, %Choco] = $ChocoboSeeds[%ClientBuyer, %BuyerChoco]; $ChocoboSeeds[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboCandies[%Client, %Choco]; $ChocoboCandies[%Client, %Choco] = $ChocoboCandies[%ClientBuyer, %BuyerChoco]; $ChocoboCandies[%ClientBuyer, %BuyerChoco] = %t;
	%t = $ChocoboTakeCare[%Client, %Choco]; $ChocoboTakeCare[%Client, %Choco] = $ChocoboTakeCare[%ClientBuyer, %BuyerChoco]; $ChocoboTakeCare[%ClientBuyer, %BuyerChoco] = %t;
}

// One-way transfer (player-to-player SALE): the bird moves %from -> %to.
// Returns false (with messages) if the buyer has no free stable slot.
function Chocobo::GiveBird(%from, %to, %choco) {
	if($Chocobo[%to] >= $MaxChocobo) {
		Client::sendMessage(%to, 1, "Your stable is full! ("@$Chocobo[%to]@"/"@$MaxChocobo@")");
		Client::sendMessage(%from, 1, Client::getName(%to)@"'s stable is full - sale canceled.");
		return false;
	}
	if($Chocobo[%to] == "")
		$Chocobo[%to] = 0;
	$Chocobo[%to]++;
	%i = Chocobo::GetSaveSlot(%to);
	$Chocobo[%to, %i] = true;
	$ChocoboTakeCare[%to, %i] = "false";
	$ChocoboName[%to, %i] = $ChocoboName[%from, %choco];
	$ChocoboColor[%to, %i] = $ChocoboColor[%from, %choco];
	$ChocoboSex[%to, %i] = $ChocoboSex[%from, %choco];
	$ChocoboYAge[%to, %i] = $ChocoboYAge[%from, %choco];
	$ChocoboDAge[%to, %i] = $ChocoboDAge[%from, %choco];
	$ChocoboTempAge[%to, %i] = $ChocoboTempAge[%from, %choco];
	$ChocoboSTR[%to, %i] = $ChocoboSTR[%from, %choco];
	$ChocoboDEX[%to, %i] = $ChocoboDEX[%from, %choco];
	$ChocoboCON[%to, %i] = $ChocoboCON[%from, %choco];
	$ChocoboINT[%to, %i] = $ChocoboINT[%from, %choco];
	$ChocoboWIS[%to, %i] = $ChocoboWIS[%from, %choco];
	$ChocoboEXP[%to, %i] = $ChocoboEXP[%from, %choco];
	$ChocoboHealth[%to, %i] = $ChocoboHealth[%from, %choco];
	$ChocoboHungry[%to, %i] = $ChocoboHungry[%from, %choco];
	$ChocoboWorth[%to, %i] = $ChocoboWorth[%from, %choco];
	$ChocoboMeats[%to, %i] = $ChocoboMeats[%from, %choco];
	$ChocoboFruits[%to, %i] = $ChocoboFruits[%from, %choco];
	$ChocoboVits[%to, %i] = $ChocoboVits[%from, %choco];
	$ChocoboSeeds[%to, %i] = $ChocoboSeeds[%from, %choco];
	$ChocoboCandies[%to, %i] = $ChocoboCandies[%from, %choco];
	Chocobo::Clear(%from, %choco);
	return true;
}

function Chocobo::NewName(%Client, %opt, %Choco) {
	$ChocoboTempName[%Client] = %Choco;
	if(%opt == NewName) {
		Client::sendMessage(%Client, 0, "Enter a name for your new "@$ChocoboColor[%Client, %Choco]@" Chocobo! (Example: #name "@$ChocoboColor[%Client, %Choco]@"_Choco).");
		$CanName[%Client] = true;
	}
	else if(%opt == OldName) {
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@". What new name would you like? (Example: #name Red_Chocobo Or type just #name  to cancel.).");
		$CanName[%Client] = true;
		$OldName[%Client] = true;
	}
	else if(%opt == TryAgain) {
		Client::sendMessage(%Client, 0, "You can't have spaces in your name! Try again. (Example: #name Red_Chocobo).");
		$CanName[%Client] = true;
	}
}

function Chocobo::Theme(%Client, %object) {

	if($ChocoboSpawn[%Client] == "") {
		%object.driver = "";
		%object.vehicle = "";
	}
	if(%object.driver == 1) {
		remoteEval(%Client, SFXPLAY, 1, "ChocoboTheme.wav", 62/2);
	}
}

$Chocobo::invalidChars = " ><?\\\"{}[]+=:;/.,!@#$%&()|`~";

function Chocobo::InvalidChar(%name) {

	for(%a = 1; %a <= String::len($Chocobo::invalidChars); %a++) {
		%b = String::getSubStr($Chocobo::invalidChars, %a-1, 1);
		if(String::findSubStr(%name, %b) != -1) {
			return true;
		}
	}
	return false;
}

function Chocobo::Getlen(%string) {

	for(%len = 0; String::getSubStr(%string, %len, 1) != ""; %len++) {}
	return %len;
}

function Chocobo::GetSaveSlot(%Client) {
	for(%i = 1; %i <= $MaxChocobo; %i++) {
		if($ChocoboName[%Client, %i] == "")
			$ChocoboName[%Client, %i] = "Free save slot";
		if($Chocobo[%Client, %i] == "")
			$Chocobo[%Client, %i] = "false";
	}
	for(%i = 1; %i <= $MaxChocobo; %i++) {
		if($Chocobo[%Client, %i] == "false")
			return %i;
	}
}

function Chocobo::Add(%Client, %opt, %SaveSlot, %name, %color, %sex, %YAge, %DAge, %TempAge, %STR, %DEX, %CON, %INT, %WIS, %EXP, %Health, %Hungry, %Worth, %Meats, %Fruits, %Vits, %Seeds, %Candies) {

		%i = %SaveSlot;

		$Chocobo[%Client, %i]		= true;
		$ChocoboTakeCare[%Client, %i]= "false";
		$ChocoboName[%Client, %i]	= %name;
		$ChocoboColor[%Client, %i]	= %color;
		$ChocoboSex[%Client, %i]	= %sex;
		$ChocoboYAge[%Client, %i]	= %YAge;
		$ChocoboDAge[%Client, %i]	= %DAge;
		$ChocoboTempAge[%Client, %i]= %TempAge;
		$ChocoboSTR[%Client, %i]	= %STR;
		$ChocoboDEX[%Client, %i]	= %DEX;
		$ChocoboCON[%Client, %i]	= %CON;
		$ChocoboINT[%Client, %i]	= %INT;
		$ChocoboWIS[%Client, %i]	= %WIS;
		$ChocoboEXP[%Client, %i]	= %EXP;
		$ChocoboHealth[%Client, %i]	= %Health;
		$ChocoboHungry[%Client, %i]	= %Hungry;
		$ChocoboWorth[%Client, %i]	= %Worth;
		$ChocoboMeats[%Client, %i]	= %Meats;
		$ChocoboFruits[%Client, %i]	= %Fruits;
		$ChocoboVits[%Client, %i]	= %Vits;
		$ChocoboSeeds[%Client, %i]	= %Seeds;
		$ChocoboCandies[%Client, %i]= %Candies;
}

function Chocobo::Clear(%Client, %Choco) {

	if($Chocobo[%Client, %Choco] == false || $Chocobo[%Client, %Choco] == "" || !$HasLoadedAndSpawned[%Client])
		return;

	%name = Client::getName(%Client);

	if($RMDebug)
		Client::sendMessage(%Client, 1, "Clearing "@$ChocoboName[%Client, %Choco]@" Num: "@%Choco);

	$Chocobo[%Client]--;
	if($Chocobo[%Client] < 0)
		$Chocobo[%Client] = 0;
	$Chocobo[%Client, %Choco] = "false";
	$ChocoboTakeCare[%Client, %Choco] = "";
	$ChocoboName[%Client, %Choco] = "Free save slot";
	$ChocoboColor[%Client, %Choco] = "";
	$ChocoboSex[%Client, %Choco]  = "";
	$ChocoboYAge[%Client, %Choco]  = "";
	$ChocoboDAge[%Client, %Choco]  = "";
	$ChocoboTempAge[%Client, %Choco]  = "";
	$ChocoboSTR[%Client, %Choco]  = "";
	$ChocoboDEX[%Client, %Choco] = "";
	$ChocoboCON[%Client, %Choco]  = "";
	$ChocoboINT[%Client, %Choco]  = "";
	$ChocoboWIS[%Client, %Choco]  = "";
	$ChocoboEXP[%Client, %Choco]  = "";
	$ChocoboHealth[%Client, %Choco] = "";
	$ChocoboHungry[%Client, %Choco]  = "";
	$ChocoboWorth[%Client, %Choco]  = "";
	$ChocoboMeats[%Client, %Choco] = "";
	$ChocoboFruits[%Client, %Choco] = "";
	$ChocoboVits[%Client, %Choco] = "";
	$ChocoboSeeds[%Client, %Choco] = "";
	$ChocoboCandies[%Client, %Choco] = "";

	if($Chocobo[%Client] < 0)
		Client::sendMessage(%Client, 0, "What happened!? Your Chocobo total count is "@$Chocobo[%Client]@". This isn't a good thing. Contact your Server-host and ask him/her to look over your char file to see if you have any Chocobos left...");
}

function Chocobo::Food(%Client, %name, %lvltype, %STR, %DEX, %CON, %INT, %WIS, %M, %C, %V, %F, %S, %Choco) {

	$ChocoboMeats[%Client, %Choco] += %M;
	$ChocoboCandies[%Client, %Choco] += %C;
	$ChocoboVits[%Client, %Choco] += %V;
	$ChocoboFruits[%Client, %Choco] += %F;
	$ChocoboSeeds[%Client, %Choco] += %S;

	Chocobo::Sim(%Client, %Choco, %name, %lvltype);

	$ChocoboSTR[%Client, %Choco] += %STR;
	$ChocoboDEX[%Client, %Choco] += %DEX;
	$ChocoboCON[%Client, %Choco] += %CON;
	$ChocoboINT[%Client, %Choco] += %INT;
	$ChocoboWIS[%Client, %Choco] += %WIS;

}

function Chocobo::Sim(%Client, %Choco, %opt, %lvltype) { //Called every 5 mins or when you feed your Chocobo

	%name = Client::getname(%Client);
	if($RMDebug)
		echo("Sim called ["@%Client@"]: %choco: "@%Choco@" | %opt = "@%opt@" | %lvltype = "@%lvltype);
	if($Chocobo[%Client, %Choco] == false || $Chocobo[%Client, %Choco] != true)
		return;

	if(%opt == Take) {

		%t = $ChocoboCandies[%Client, %Choco] + $ChocoboVits[%Client, %Choco] + $ChocoboFruits[%Client, %Choco] + $ChocoboSeeds[%Client, %Choco] - $ChocoboMeats[%Client, %Choco];
		if($ChocoboMeats[%Client, %Choco] > %t)
			$ChocoboHealth[%Client, %Choco] -= 4;
		%t = $ChocoboVits[%Client, %Choco] + $ChocoboFruits[%Client, %Choco] + $ChocoboSeeds[%Client, %Choco] + $ChocoboMeats[%Client, %Choco] - $ChocoboCandies[%Client, %Choco];
		if($ChocoboCandies[%Client, %Choco] > %t)
			$ChocoboHealth[%Client, %Choco] -= 4;
		%t = $ChocoboFruits[%Client, %Choco] + $ChocoboSeeds[%Client, %Choco] + $ChocoboMeats[%Client, %Choco] + $ChocoboCandies[%Client, %Choco] - $ChocoboVits[%Client, %Choco];
		if($ChocoboVits[%Client, %Choco] > %t)
			$ChocoboHealth[%Client, %Choco] -= 4;
		%t = $ChocoboSeeds[%Client, %Choco] + $ChocoboMeats[%Client, %Choco] + $ChocoboCandies[%Client, %Choco] + $ChocoboVits[%Client, %Choco] - $ChocoboFruits[%Client, %Choco];
		if($ChocoboFruits[%Client, %Choco] > %t)
			$ChocoboHealth[%Client, %Choco] -= 4;
		%t = $ChocoboMeats[%Client, %Choco] + $ChocoboCandies[%Client, %Choco] + $ChocoboVits[%Client, %Choco] + $ChocoboFruits[%Client, %Choco] - $ChocoboSeeds[%Client, %Choco];
		if($ChocoboSeeds[%Client, %Choco] > %t)
			$ChocoboHealth[%Client, %Choco] -= 4;

		$ChocoboHealth[%Client, %Choco] -= 1;
		$ChocoboHungry[%Client, %Choco] -= 3;

		%Stats = $ChocoboFruits[%Client, %Choco] + $ChocoboSeeds[%Client, %Choco] + $ChocoboMeats[%Client, %Choco] + $ChocoboCandies[%Client, %Choco] + $ChocoboVits[%Client, %Choco];
		$ChocoboWorth[%Client, %Choco] += round(%Stats / 5) + $ChocoboHealth[%Client, %Choco] + $ChocoboHungry[%Client, %Choco];

		if($ChocoboHungry[%Client, %Choco] <= 50) {
			$ChocoboHealth[%Client, %Choco] -= 2;
			$ChocoboWorth[%Client, %Choco] -= 50;
			%txt = "Chocobo "@$ChocoboName[%Client, %Choco]@" is hungry ";
		}
		if($ChocoboHungry[%Client, %Choco] <= 25) {
			$ChocoboHealth[%Client, %Choco] -= 3; //Your Chocobo can lose up to 10 health every 5 mins
			$ChocoboWorth[%Client, %Choco] -= 250;
			%txt = "Chocobo "@$ChocoboName[%Client, %Choco]@" is very hungry ";
		}
		if($ChocoboHungry[%Client, %Choco] >= 51 && $ChocoboHungry[%Client, %Choco] <= 74) {
			$ChocoboWorth[%Client, %Choco] += 50;
			%txt = "Chocobo "@$ChocoboName[%Client, %Choco]@" is getting a little hungry ";
		}
		if($ChocoboHungry[%Client, %Choco] >= 75 && $ChocoboHungry[%Client, %Choco] <= 89) {
			$ChocoboWorth[%Client, %Choco] += 75;
			%txt = "Chocobo "@$ChocoboName[%Client, %Choco]@" isn't very hungry ";
		}
		if($ChocoboHungry[%Client, %Choco] >= 90 && $ChocoboHungry[%Client, %Choco] <= 100) {
			$ChocoboWorth[%Client, %Choco] += 100;
			%txt = "Chocobo "@$ChocoboName[%Client, %Choco]@" is full ";
		}
		//Client::sendMessage(%Client, 0, %txt);

		if($ChocoboHealth[%Client, %Choco] <= 50) {
			$ChocoboWorth[%Client, %Choco] -= 50;
			%txt2 = "and is getting sick.";
		}
		if($ChocoboHealth[%Client, %Choco] <= 15) {
			$ChocoboWorth[%Client, %Choco] -= 250;
			%txt2 = "and has fallen ill.";
			if(OddsAre(2)) {
				MessageAll(0, %name@"'s "@$ChocoboColor[%Client, %Choco]@" Chocobo "@$ChocoboName[%Client, %Choco]@" has died of poor health.");
				Chocobo::Clear(%Client, %Choco);
				return;
			}
		}
		if($ChocoboHealth[%Client, %Choco] >= 51 && $ChocoboHealth[%Client, %Choco] <= 74) {
			$ChocoboWorth[%Client, %Choco] += 50;
			%txt2 = "and is a little happy.";
		}
		if($ChocoboHealth[%Client, %Choco] >= 75 && $ChocoboHealth[%Client, %Choco] <= 89) {
			$ChocoboWorth[%Client, %Choco] += 75;
			%txt2 = "and is happy.";
		}
		if($ChocoboHealth[%Client, %Choco] >= 90 && $ChocoboHealth[%Client, %Choco] <= 100) {
			$ChocoboWorth[%Client, %Choco] += 100;
			%txt2 = "and is very happy!";
		}

		Client::sendMessage(%Client, 0, %txt @ %txt2);
	}

	if($ChocoboHealth[%Client, %Choco] <= 0) {
		MessageAll(0, %name@"'s "@$ChocoboColor[%Client, %Choco]@" Chocobo "@$ChocoboName[%Client, %Choco]@" has died of poor health.");
		Chocobo::Clear(%Client, %Choco);
		return;
	}
	if($ChocoboHungry[%Client, %Choco] <= 0) {
		MessageAll(0, %name@"'s "@$ChocoboColor[%Client, %Choco]@" Chocobo "@$ChocoboName[%Client, %Choco]@" has died of hunger.");
		Chocobo::Clear(%Client, %Choco);
		return;
	}
	if($ChocoboHealth[%Client, %Choco] >= 100)
		$ChocoboHealth[%Client, %Choco] = 100;
	if($ChocoboHungry[%Client, %Choco] >= 100) {
		$ChocoboHungry[%Client, %Choco] = 100;
		Client::sendMessage(%Client, 0, "Your Chocobo is full.");
	}
	if($ChocoboWorth[%Client, %Choco] < 0)
		$ChocoboWorth[%Client, %Choco] = 0;
	// Worth cap (2026-07-17): the Take tick adds worth every 5 min forever, so
	// a parked well-fed bird was an unbounded money printer via the sell menu.
	%wcap = ($ChocoboSTR[%Client, %Choco] + $ChocoboDEX[%Client, %Choco] + $ChocoboCON[%Client, %Choco] + $ChocoboINT[%Client, %Choco] + $ChocoboWIS[%Client, %Choco]) * 75 + 5000;
	if($ChocoboWorth[%Client, %Choco] > %wcap)
		$ChocoboWorth[%Client, %Choco] = %wcap;
}

// (Removed leftover "TEMP" Cap()/round() redefinitions. They loaded AFTER
// rpgfunk.cs and overrode its versions - and this Cap() dropped rpgfunk's
// "inf" (uncapped) handling, so Cap(x,lb,"inf") returned the string "inf".
// That silently broke damage (playerdamage Cap(v-25,0,"inf")), the EXP table
// (rpgstats pow(Cap(i-20,0,"inf"),5)) and blacksmith pricing. rpgfunk.cs owns
// the canonical Cap()/round().)

function Chocobo::FoodType(%Client, %type, %lvltype, %Choco, %coins, %name) {

	if($ChocoboHungry[%Client, %Choco] >= 100) {
		Client::sendMessage(%Client, 0, "Your Chocobo isn't hungry.");
		return;
	}
	Client::sendMessage(%Client, 0, "You bought "@%type@".~wSoundMoney1.wav");

	%STR = %DEX = %CON = %INT = %WIS = %M = %C = %V = %F = %S = 0;

	$COINS[%Client] += %coins;

	if(%lvltype == 1) //6 lvls-- -1-$1,000  -2-$10,000  -3-$20,000  -4-$40,000  -5-$100,000 -6-$1,000,000
		%d = Between(1, 3); //1-3
	else if(%lvltype == 2)
		%d = Between(5, 15); //5-15
	else if(%lvltype == 3)
		%d = Between(15, 30); //15-30
	else if(%lvltype == 4)
		%d = Between(30, 40); //30-40
	else if(%lvltype == 5)
		%d = Between(50, 70); //50-70
	else if(%lvltype == 6)
		%d = Between(100, 150); //100-150
	$ChocoboWorth[%Client, %Choco] += ((%lvltype * 2) * %d) + (-%coins / 10);
	if(%name == "Meats") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 16 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %d, %DEX, %CON, %INT, %WIS, 1, %C, %V, %F, %S, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s STR increased by "@%d@".");
	}
	else if(%name == "Candies") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 13 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %STR, %d, %CON, %INT, %WIS, %M, 1, %V, %F, %S, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s DEX increased by "@%d@".");
	}
	else if(%name == "Vits") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 10 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %STR, %DEX, %d, %INT, %WIS, %M, %C, 1, %F, %S, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s CON increased by "@%d@".");
	}
	else if(%name == "Fruits") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 12 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %STR, %DEX, %CON, %d, %WIS, %M, %C, %V, 1, %S, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s INT increased by "@%d@".");
	}
	else if(%name == "Seeds") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 11 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %STR, %DEX, %CON, %INT, %d, %M, %C, %V, %F, 1, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s WIS increased by "@%d@".");
	}
	else if(%name == "Cracker") {
		$ChocoboHealth[%Client, %Choco] += %lvltype;
		$ChocoboHungry[%Client, %Choco] += 16 - %lvltype;
		Chocobo::Food(%Client, %name, %lvltype, %d, %d, %d, %d, %d, 1, 1, 1, 1, 1, %Choco);
		Client::sendMessage(%Client, 0, $ChocoboName[%Client, %Choco]@"'s all stats increased by "@%d@"!");
	}
	if($ChocoboHealth[%Client, %Choco] >= 100)
		$ChocoboHealth[%Client, %Choco] = 100;
	if($ChocoboHungry[%Client, %Choco] >= 100) {
		$ChocoboHungry[%Client, %Choco] = 100;
		Client::sendMessage(%Client, 0, "Your Chocobo is full.");
	}
	//RefreshAll(%Client);
	$CanFeed[%Client] = true;
	processMenuChocoboViewInfo(%Client, Feed);
}


// Trainer-proximity gate. $CanFeed is set by Chocobo::Talk (trainer NPC) and
// wiped on restart; admins (>=3) bypass so the system is testable before a
// trainer bot exists.
function Chocobo::CanUse(%Client) {
	if($CanFeed[%Client] == true)
		return true;
	if(%Client.adminLevel >= 3)
		return true;
	return false;
}

function Chocobo::Timer(%Client, %Choco) { // Gets called in RecursiveWorld
	$ChocoboTempAge[%Client, %Choco]++;
	if($ChocoboTempAge[%Client, %Choco] == 5 || $ChocoboTempAge[%Client, %Choco] == 10 || $ChocoboTempAge[%Client, %Choco] == 15)
		Chocobo::Sim(%Client, %Choco, "Take", None);
	if($ChocoboTempAge[%Client, %Choco] >= 15) {
		$ChocoboTempAge[%Client, %Choco] = 0;
		$ChocoboDAge[%Client, %Choco]++;
	}
	if($ChocoboDAge[%Client, %Choco] >= 40) {
		$ChocoboDAge[%Client, %Choco] = 0;
		$ChocoboYAge[%Client, %Choco]++;
	}
}

//
function MenuChocobo(%Client) {	//================================================== MENUS

	Client::buildMenu(%Client, "Select a Chocobo\n"@$Chocobover, "PickedChocobo", true);

	%curItem = 1;
	for(%i = 1; $Chocobo[%Client, %i] != ""; %i++) {
		Client::addMenuItem(%Client, %curItem++ @ $ChocoboName[%Client, %i], %i);
		if(%curItem == 8)
			return;
	}
}

//Chocobo opts
function processMenuPickedChocobo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuPickedChocobo(" @ %Client @ ", " @ %opt @ ")");

	if(%opt != "" && %opt != Page1 && %opt != Page2)
		$MenuChoco[%Client] = %opt; //Save the # of the Chocobo your looking at
	if($ChocoboName[%Client, $MenuChoco[%Client]] != "Free save slot") {
		if(%opt != "Page2") {       //The %opt is the Chocobo num not the page =]
			Client::buildMenu(%Client, $ChocoboName[%Client, $MenuChoco[%Client]], "ChocoboViewInfo", true);

			Client::addMenuItem(%Client, "1View Stats", "Stats");
			Client::addMenuItem(%Client, "2Feed", "Feed");
			if($ChocoboSpawn[%Client])
				Client::addMenuItem(%Client, "3Return Chocobo", "Return");
			else
				Client::addMenuItem(%Client, "3Ride Chocobo", "Play");
			Client::addMenuItem(%Client, "4Breed", "Breed");
			Client::addMenuItem(%Client, "5Sell", "Sell");
			Client::addMenuItem(%Client, "6Trade/Sell with other players", "Trade");
			Client::addMenuItem(%Client, "nNext>>", "Page2");
			Client::addMenuItem(%Client, "x<<Back", "Back");
		}
		if(%opt == "Page2") {
			Client::buildMenu(%Client, $ChocoboName[%Client, $MenuChoco[%Client]], "ChocoboViewInfo", true);

			Client::addMenuItem(%Client, "1Release Chocobo", "Release");
			Client::addMenuItem(%Client, "2Change Chocobo's name", "NameChange");
			Client::addMenuItem(%Client, "x<<Back", "Page1");
		}
	}
	else {
			Client::buildMenu(%Client, "Free save slot\n", "ChocoboViewInfo", true);
			Client::addMenuItem(%Client, "1No data saved", "Back");
			Client::addMenuItem(%Client, "x<<Back", "Back");
	}
}

function processMenuChocoboViewInfo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuChocoboViewInfo(" @ %Client @ ", " @ %opt @ ")");

	%name = Client::getName(%Client);

	if(%opt == "Stats") {
		MenuChocoboStats(%Client, %opt);
		return;
	}
	else if(%opt == "Feed") {
		if(Chocobo::CanUse(%Client)) {
			Client::buildMenu(%Client, "What will you feed your Chocobo?", "Feeding", true);

			if($Feeding[%Client] == Shop1) { //Changes these if you wanna Chee
				$Chocobo::tmpFoodCost[%Client] = 1000;
				$Chocobo::tmpFoodlvl[%Client] = 1;
				Client::addMenuItem(%Client, "1Seeds - 1000 gil", "Seeds|Seeds"); //These add stats to your Chocobo!
				Client::addMenuItem(%Client, "2Fruits - 1000 gil", "Fruits|Fruits");
				Client::addMenuItem(%Client, "3Candy - 1000 gil", "Candy|Candies");
				Client::addMenuItem(%Client, "4Hot dog - 1000 gil", "Hot dog|Meats");
				Client::addMenuItem(%Client, "5Vit A - 1000 gil", "Vit A|Vits");
				Client::addMenuItem(%Client, "6These cost a lot...", "cost");
				Client::addMenuItem(%Client, "x<<Back", "Back");
			}
			else if($Feeding[%Client] == Shop2) {
				$Chocobo::tmpFoodCost[%Client] = 10000;
				$Chocobo::tmpFoodlvl[%Client] = 2;
				Client::addMenuItem(%Client, "1Berrys - 10000 gil", "Berrys|Fruits");
				Client::addMenuItem(%Client, "2Pizza - 10000 gil", "Pizza|Meats");
				Client::addMenuItem(%Client, "3Vit B - 10000 gil", "Vit B|Vits");
				Client::addMenuItem(%Client, "4Blue Seeds - 10000 gil", "Blue Seeds|Seeds");
				Client::addMenuItem(%Client, "5Jam - 10000 gil", "Jam|Candies");
				Client::addMenuItem(%Client, "x<<Back", "Back");
			}
			else if($Feeding[%Client] == Shop3) {
				$Chocobo::tmpFoodCost[%Client] = 20000;
				$Chocobo::tmpFoodlvl[%Client] = 3;
				Client::addMenuItem(%Client, "1Banana  - 20000 gil", "Banana|Fruits");
				Client::addMenuItem(%Client, "2Green Seeds - 20000 gil", "Green Seeds|Seeds");
				Client::addMenuItem(%Client, "3Steak - 20000 gil", "Steak|Meats");
				Client::addMenuItem(%Client, "4Vit B+ - 20000 gil", "Vit B+|Vits");
				Client::addMenuItem(%Client, "5Sugar cube - 20000 gil", "Sugar cube|Candies");
				Client::addMenuItem(%Client, "x<<Back", "Back");
			}
			else if($Feeding[%Client] == Shop4) {
				$Chocobo::tmpFoodCost[%Client] = 40000;
				$Chocobo::tmpFoodlvl[%Client] = 4;
				Client::addMenuItem(%Client, "1Blue Berrys  - 40000 gil", "Blue Berrys|Fruits");
				Client::addMenuItem(%Client, "2Yellow Seeds - 40000 gil", "Yellow Seeds|Seeds");
				Client::addMenuItem(%Client, "3Meat Balls - 40000 gil", "Meat Balls|Meats");
				Client::addMenuItem(%Client, "4Vit C - 40000 gil", "Vit C|Vits");
				Client::addMenuItem(%Client, "5Sugar Cookie - 40000 gil", "Sugar Cookie|Candies");
				Client::addMenuItem(%Client, "x<<Back", "Back");
			}
			else if($Feeding[%Client] == Shop5) {
				$Chocobo::tmpFoodCost[%Client] = 100000;
				$Chocobo::tmpFoodlvl[%Client] = 5;
				Client::addMenuItem(%Client, "1Black Berrys  - 100000 gil", "Black Berrys|Fruits"); //INT
				Client::addMenuItem(%Client, "2Black Seeds - 100000 gil", "Black Seeds|Seeds"); //WIS
				Client::addMenuItem(%Client, "3T-Bone Steak - 100000 gil", "T-Bone Steak|Meats"); //STR
				Client::addMenuItem(%Client, "4Vit C+ - 100000 gil", "Vit C+|Vits"); //CON
				Client::addMenuItem(%Client, "5Chocolate - 100000 gil", "Chocolate|Candies"); //DEX
				Client::addMenuItem(%Client, "x<<Back", "Back");
			}
			else if($Feeding[%Client] == Shop6) {
				$Chocobo::tmpFoodCost[%Client] = 1000000;
				$Chocobo::tmpFoodlvl[%Client] = 6;
				Client::addMenuItem(%Client, "1Cracker - 1000000 gil", "Cracker|Cracker"); //ALL STATS+
				Client::addMenuItem(%Client, "x<<Back", "Back");	    //We will have to find a gooood spot for him >=]
			}
		}
		else
			Client::sendMessage(%Client, 0, "Talk to a Chocobo trainer to feed your Chocobo!");

		return;
	}
	else if(%opt == "Play") {
		if(Chocobo::CanUse(%Client)) {
			Client::sendMessage(%Client, 0, "You hop on your "@$ChocoboColor[%Client, $MenuChoco[%Client]]@" Chocobo "@$ChocoboName[%Client, $MenuChoco[%Client]]@".");
			%pos = GameBase::getposition(%Client);
			Chocobo::Spawn(%Client, $MenuChoco[%Client], %pos);
		}
		else {
			Client::sendMessage(%Client, 0, "Talk to a Chocobo trainer if you want to ride your Chocobo!");
			MenuChocobo(%Client);
		}
		return;
	}
	else if(%opt == "Return") {
		if(Chocobo::CanUse(%Client)) {
			Client::setControlObject(%Client, %Client);
			Chocobo::Delete(%Client);
			MenuChocobo(%Client);
		}
		else {
			Client::sendMessage(%Client, 0, "Talk to a Chocobo trainer if you want to return your Chocobo!");
			MenuChocobo(%Client);
		}
		return;
	}
	else if(%opt == "Breed") {
		if(Chocobo::CanUse(%Client)) {
			MenuBreeding(%Client);
			return;	//===
		}
		else {
			Client::sendMessage(%Client, 0, "Talk to a Chocobo trainer if you want to breed your Chocobo!");
			MenuChocobo(%Client);
			return;
		}
		return;
	}
	else if(%opt == "Sell") {
		if(Chocobo::CanUse(%Client)) {
			Client::buildMenu(%Client, "Worth: $"@FixM($ChocoboWorth[%Client, $MenuChoco[%Client]]), "Selling", true);
			Client::addMenuItem(%Client, "1Sell", 1);
			Client::addMenuItem(%Client, "9Don't sell", 9);
		}
		else {
			Client::sendMessage(%Client, 0, "Talk to a Chocobo trainer to sell your Chocobo!");
			Client::buildMenu(%Client, "Your Chocobo is worth: $"@FixM($ChocoboWorth[%Client, $MenuChoco[%Client]]), "Selling", true);
			Client::addMenuItem(%Client, "x<<Back", 9);
		}
		return;
	}
	else if(%opt == "Trade") {
		Client::buildMenu(%Client, "Trade or Sell?", "Tradeing", true);

		Client::addMenuItem(%Client, "1Trade", "Trade");
		Client::addMenuItem(%Client, "2Sell", "Sell");

		Client::addMenuItem(%Client, "x<<Back", Back);
		return;
	}
	else if(%opt == "Release") {
		Client::buildMenu(%Client, "Release "@$ChocoboName[%Client, $MenuChoco[%Client]]@"?", "Releaseing", true);
		Client::addMenuItem(%Client, "1Yes", 1);
		Client::addMenuItem(%Client, "9No", 9);
		if($ChocoboSex[%Client, $MenuChoco[%Client]] == "Male")
			%char = "he";
		else	%char = "she";														//IF we make them able to attack =p
		Client::sendMessage(%Client, 1, "You are about to relase your Chocobo!"); //If you do, "@%char@"'ll fight you or anyone around!")
		return;
	}
	else if(%opt == "TakeCare") {
	//$ChocoboTakeCare[%Client, $MenuChoco[%Client]
	}
	else if(%opt == "NameChange")
		Chocobo::NewName(%Client, OldName, $MenuChoco[%Client]);
	else if(%opt == Page2)
		processMenuPickedChocobo(%Client, Page2);
	else if(%opt == "Back")
		MenuChocobo(%Client);
	else if(%opt == Page1)
		processMenuPickedChocobo(%Client, Page1);
}

function MenuChocoboStats(%Client, %opt) {

	Client::buildMenu(%Client, $ChocoboName[%Client, $MenuChoco[%Client]]@"'s stats:", "StatsInfo", true);

	if(%opt == Stats) {
		Client::addMenuItem(%Client, "1Years old: "@$ChocoboYAge[%Client, $MenuChoco[%Client]]@"", 1);
		Client::addMenuItem(%Client, "2Days old: "@$ChocoboDAge[%Client, $MenuChoco[%Client]]@"", 1);
		Client::addMenuItem(%Client, "3EXP: "@$ChocoboEXP[%Client, $MenuChoco[%Client]]@"", 4);
		Client::addMenuItem(%Client, "4Color: "@$ChocoboColor[%Client, $MenuChoco[%Client]]@"", 5);
		Client::addMenuItem(%Client, "5Health: "@$ChocoboHealth[%Client, $MenuChoco[%Client]]@"%", 7);
		Client::addMenuItem(%Client, "6Full: "@$ChocoboHungry[%Client, $MenuChoco[%Client]]@"%", 8);
		Client::addMenuItem(%Client, "7Worth: $"@FixM($ChocoboWorth[%Client, $MenuChoco[%Client]])@"", 0);

		Client::addMenuItem(%Client, "nNext>> ", "Next");
		return;
	}
	else if(%opt == Stats2) {

		Client::buildMenu(%Client, $ChocoboName[%Client, $MenuChoco[%Client]]@"'s stats:", "StatsInfo", true);

		Client::addMenuItem(%Client, "1STR [ "@$ChocoboSTR[%Client, $MenuChoco[%Client]]@" ]", 1);
		Client::addMenuItem(%Client, "2DEX [ "@$ChocoboDEX[%Client, $MenuChoco[%Client]]@" ]", 2);
		Client::addMenuItem(%Client, "3CON [ "@$ChocoboCON[%Client, $MenuChoco[%Client]]@" ]", 3);
		Client::addMenuItem(%Client, "4INT [ "@$ChocoboINT[%Client, $MenuChoco[%Client]]@" ]", 4);
		Client::addMenuItem(%Client, "5WIS [ "@$ChocoboWIS[%Client, $MenuChoco[%Client]]@" ]", 5);

		Client::addMenuItem(%Client, "b<<Back ", "Back");
		return;
	}
}

function processMenuStatsInfo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuStatsInfo(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == "Next")
		MenuChocoboStats(%Client, Stats2);
	else if(%opt == "Back")
		//MenuChocoboStats(%Client);
		processMenuPickedChocobo(%Client);
}

function processMenuTradeing(%Client, %opt) {

	dbecho($dbechoMode, "processMenuTradeing(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == back) {
		processMenuPickedChocobo(%Client);
		return;
	}

	Client::buildMenu(%Client, %opt@" with?", "TradeOrSell", true);

	if($IsTradeing[%Client] == true) {
		if(%opt == trade)
			%opt = "start a new trade";
		Client::sendmessage(%Client, 0, "You can't "@%opt@" right now.");
		return;
	}
	$MenuTrade[%Client] = %opt; //Save the option the Client picked

	%index = 0;
	%char = "1234567890"; //Array

	%list = GetPlayerIdList();
	for(%i = 0; GetWord(%list, %i) != -1; %i++) {

		%cl = GetWord(%list, %i);
		if(%cl != %Client) {
			if($Chocobo[%cl] >= 1) {
				%name = Client::getName(%cl);
				//Client::getTeam(%cl);

				%choice = String::GetSubStr(%char, %index, 1);

				Client::addMenuItem(%Client, %choice @ %name, %cl);
				%index++;
				if(%index >= 10) //key list "1234567890" only has 10 slots
					return;
			}
		}
	}
}

function MenuBreeding(%Client) {

	Client::buildMenu(%Client, "Pick player", "PickPlayer::Breed", true);

	if(%opt == "Back") {
		processMenuPickedChocobo(%Client);
		return;
	}

	%index = 0;
	%char = "1234567890"; //Array

	%list = GetPlayerIdList();
	for(%i = 0; GetWord(%list, %i) != -1; %i++) {

		%cl = GetWord(%list, %i);
		if(%cl != %Client) {
			if($Chocobo[%cl] >= 1) {
				%name = Client::getName(%cl);

				%choice = String::GetSubStr(%char, %index, 1);

				Client::addMenuItem(%Client, %choice @ %name, %cl);
				%index++;
				if(%index >= 10) //key list "1234567890" only has 10 slots
					return;
			}
		}
	}
}

function processMenuPickPlayer::Breed(%Client, %opt) {

	dbecho($dbechoMode, "processMenuPickPlayer::Breed(" @ %Client @ ", " @ %opt @ ")");

	$MenuBreedBuyerId[%Client] = %opt; //Save buyers ID


	Client::buildMenu(%Client, "Pick Chocobo", "PickChoco::Breed", true);

 	%curItem = 0;
	for(%i = 1; $Chocobo[%opt, %i] != ""; %i++) {
		Client::addMenuItem(%Client, %curItem++ @ $ChocoboName[%opt, %i], %i);
		if(%curItem == 8)
			return;
	}
}

function processMenuPickChoco::Breed(%Client, %opt) {

	dbecho($dbechoMode, "processMenuPickChoco::Breed(" @ %Client @ ", " @ %opt @ ")");

	Client::buildMenu(%Client, "Breed?", "BreedChocobo", true);

	%Id = $MenuBreedBuyerId[%Client];
	$MenuBreedChocoboId::Buyer[%Client] = %opt;
	$MenuBreedChocoboId::Host[%Client] = $MenuChoco[%Client];

	Client::addMenuItem(%Client, "1Color: "@$ChocoboColor[%Id, %opt]@"", 1);
	Client::addMenuItem(%Client, "2Health: "@$ChocoboHealth[%Id, %opt]@"%", 2);
	Client::addMenuItem(%Client, "3Full: "@$ChocoboHungry[%Id, %opt]@"%", 3);
	Client::addMenuItem(%Client, "4Worth: $"@FixM($ChocoboWorth[%Id, %opt])@"", 4);

	Client::addMenuItem(%Client, "5Breed!", "Breed");
	Client::addMenuItem(%Client, "6Don't Breed", "Back");
}

function processMenuBreedChocobo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuBreedkChocobo(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == "Breed")
		Chocobo::Breed(%Client, $MenuBreedBuyerId[%Client], "Breed");
		//
	else if(%opt == "Back") {
		$MenuBreedBuyerId[%Client] = "";
		$MenuBreedChocoboId::Buyer[%Client] = "";
		$MenuBreedChocoboId::Host[%Client] = "";
		processMenuPickedChocobo(%Client);
	}
}
function processMenuTradeOrSell(%Client, %opt) {

	dbecho($dbechoMode, "processMenuTradeOrSell(" @ %Client @ ", " @ %opt @ ")");

	$MenuTradeBuyerId[%Client] = %opt; //Save buyers ID

	Client::buildMenu(%Client, "What Chocobo will you "@$MenuTrade[%Client]@"?", "PickedChocoboToTradeOrSell", true);

 	%curItem = 0;
	for(%i = 1; $Chocobo[%Client, %i] != ""; %i++) {
		Client::addMenuItem(%Client, %curItem++ @ $ChocoboName[%Client, %i], %i);
		if(%curItem == 8)
			return;
	}
}

function processMenuPickedChocoboToTradeOrSell(%Client, %opt) {

	dbecho($dbechoMode, "processMenuPickedChocoboToTradeOrSell(" @ %Client @ ", " @ %opt @ ")");

	$MenuTradeChocoboId::Host[%Client] = %opt;
	%id = $MenuTradeBuyerId[%Client];

	if($MenuTrade[%Client] == "Sell") {

		Client::buildMenu(%Client, "Sell?", "Sell", true);

		Client::addMenuItem(%Client, "1Color: "@$ChocoboColor[%id, %opt], 1);
		Client::addMenuItem(%Client, "2Health: "@$ChocoboHealth[%id, %opt]@"%", 2);
		Client::addMenuItem(%Client, "3Full: "@$ChocoboHungry[%id, %opt]@"%", 3);
		Client::addMenuItem(%Client, "4Worth: $"@FixM($ChocoboWorth[%id, %opt]), 4);

		Client::addMenuItem(%Client, "5Sell!", "Sell");
		Client::addMenuItem(%Client, "6Don't sell", "Quit");
	}
	else {
		Client::buildMenu(%Client, "Trade for?", "ViewTradeInfo", true);
		%BuyerId = $MenuTradeBuyerId[%Client];
 		%curItem = 0;
		for(%i = 1; $Chocobo[%BuyerId, %i] != ""; %i++) {
			Client::addMenuItem(%Client, %curItem++ @ $ChocoboName[%BuyerId, %i], %i);
			if(%curItem == 8)
				return;
		}
	}
}

function processMenuViewTradeInfo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuViewTradeInfo(" @ %Client @ ", " @ %opt @ ")");

	Client::buildMenu(%Client, "Trade?", "TradeChocobo", true);

	%Id = $MenuTradeBuyerId[%Client];
	$MenuTradeChocoboId::Buyer[%Client] = %opt;

	Client::addMenuItem(%Client, "1Color: "@$ChocoboColor[%Id, %opt], 1);
	Client::addMenuItem(%Client, "2Health: "@$ChocoboHealth[%Id, %opt]@"%", 2);
	Client::addMenuItem(%Client, "3Full: "@$ChocoboHungry[%Id, %opt]@"%", 3);
	Client::addMenuItem(%Client, "4Worth: $"@FixM($ChocoboWorth[%Id, %opt]), 4);

	Client::addMenuItem(%Client, "5Trade!", "Trade");
	Client::addMenuItem(%Client, "6Don't Trade", "Back");
}

function processMenuTradeChocobo(%Client, %opt) {

	dbecho($dbechoMode, "processMenuTradeChocobo(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == "Trade") {
		Chocobo::Trade(%Client, $MenuTradeBuyerId[%Client], Trade);
		return; //=====================================================================================
	}

	else if(%opt == "Back") {
		$MenuTradeChocoboId::Host[%Client] = "";
		$MenuTradeChocoboId::Buyer[%Client] = "";
		$MenuTradeBuyerId[%Client] = "";
		$MenuTrade[%Client] = "";
		processMenuPickedChocobo(%Client);
	}
}

function processMenuFeeding(%Client, %opt) {

	%Choco = $MenuChoco[%Client];

	%cost = $Chocobo::tmpFoodCost[%Client];
	%foodlvl = $Chocobo::tmpFoodlvl[%Client];
	$Chocobo::tmpFoodCost[%Client] = "";
	$Chocobo::tmpFoodlvl[%Client] = "";

	if(%opt == "cost") {
		Client::sendMessage(%Client, 2, "Chocobo trainer tells you, All our food is specially made just for Chocobos.");
		processMenuPickedChocobo(%Client);
		return;
	}
	else if(%opt == "Back") {
		processMenuPickedChocobo(%Client, %Choco);
		return;
	}

	if($COINS[%Client] >= %cost) {
		//Black Berrys|Fruits
		%pos = String::findSubStr(%opt, "|");
		%food = String::getSubStr(%opt, 0, %pos);
		%name = String::getSubStr(%opt, %pos+1, 9999);
		Chocobo::FoodType(%Client, %food, %foodlvl, %Choco, -%cost, %name);
	}
	else
		Client::sendMessage(%Client, 1, "You don't have enough coins!");




}

function Chocobo::Talk(%Client, %choco, %opt) {

	// Fixed 2026-07-17: the old schedule baked the literal "%Client" into the
	// string (empty at fire time), so the 30s expiry cleared $CanFeed[""] and
	// the flag lasted until restart; it also re-set the flag on the Clear call.
	if(%opt == Clear) {
		$CanFeed[%Client] = "";
		return;
	}
	$CanFeed[%Client] = true;
	Schedule::add("Chocobo::Talk("@%Client@", 0, Clear);", 60, "ChocoTalk"@%Client);
}
function processMenuSelling(%Client, %opt) {

	dbecho($dbechoMode, "processMenuSelling(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == 1) {
		$COINS[%Client] += $ChocoboWorth[%Client, $MenuChoco[%Client]];
		RefreshAll(%Client);
		Client::sendMessage(%Client, 1, "You sold "@$ChocoboName[%Client, $MenuChoco[%Client]]@" for "@FixM($ChocoboWorth[%Client, $MenuChoco[%Client]])@" coins.~wSoundMoney1.wav");
		Chocobo::Clear(%Client, $MenuChoco[%Client]);
	}
	else if(%opt == 9)
		  processMenuPickedChocobo(%Client);
}

function processMenuReleaseing(%Client, %opt) {

	dbecho($dbechoMode, "processMenuReleaseing(" @ %Client @ ", " @ %opt @ ")");

	if(%opt == 1) {
		Client::sendMessage(%Client, 1, "You released "@$ChocoboName[%Client, $MenuChoco[%Client]]@".");
		Chocobo::Clear(%Client, $MenuChoco[%Client]);
	}
	else if(%opt == 9)
		  processMenuPickedChocobo(%Client);
}

$MaxWorldChocobo = 12;

// ============================================================
// VEHICLE-MODE CHOCOBO - DEFAULT ON (verified in-game 2026-07-16)
// ============================================================
// The real chocobo model is rigged as a VEHICLE shape ("dummy pilot128" seat
// node) - using it as PlayerData crashes on mount. This follows the route Deus
// prototyped (his commented className="Vehicle"/shapeFile="flyer" fossil in
// ChocoboArmor.cs). chocobo.dts is forcefield.DTS with binary fixes: details
// table (dummies were LOD entries -> crash), dummy nodes reparented under
// root128, bounds origin de-offset (mount pos + whole-bird culling), root
// +90/eye -90/pilot -90 yaw set (facing + camera), walk seq renamed "thrust"
// (Flier.cpp plays it unguarded -> was SetSequence(-1) crash; now the legs
// animate while moving). Rider sits via the NATIVE-PORT getMountPose engine
// fix (driverPose was never applied in stock).
// Opt-outs (per server boot): $Choco::VehicleMode = 0 -> old on-foot Player
// chocobo; $Choco::BirdShape = 0 -> flyer shell instead of the bird.
$Choco::VehicleMode = 1;
$Choco::BirdShape = 1;
exec(ChocoboSkins);   // per-color bird datablocks (ChocoboVehicleY/R/B/G/K/W/Au)
exec(ChocoboTrainer); // rarity/price tables, buy menus, breeding (trainer townbot)
// Datablock adapted from Kronos RPG Vehicle.cs FlierData Scout; all
// referenced ids (flashExpLarge etc.) verified present in RMRPG.
FlierData ChocoboVehicle
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "flyer";        // STAGE 2: "forcefield" (the actual chocobo)
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 45;              // chocobo sprint, not scout-flier 140
	minSpeed = -10;
	lift = 0.9;
	maxAlt = -1;                // <0 = GROUND VEHICLE (engine NATIVE-PORT: no hover pad, no altitude gain)                // ground bird: hug the terrain, no cruising
	maxVertical = 1;            // flightless: tiny climb thrust; MUST be >0 - the engine divides by maxVertical/2 when steering (Flier.cpp:344), 0 = crash; also sets descent rate (-1.75x)
	maxDamage = 1.5;
	damageLevel = {1.0, 1.0};
	maxEnergy = 100;
	accel = 1.4;
	groundDamageScale = 0.5;
	repairRate = 0.5;
	damageSound = SoundFlierCrash;
	ramDamage = 0;              // a chocobo is not a battering ram
	ramDamageType = -1;
	mapFilter = 2;
	mapIcon = "M_vehicle";
	visibleToSensor = true;
	shadowDetailMask = 2;
	mountSound = SoundFlyerMount;
	dismountSound = SoundFlyerDismount;
	visibleDriver = true;
	driverPose = 22;            // the pose the ChocoboArmor fossils intended
	description = "Chocobo";
};

// STAGE 2: the REAL bird. forcefield.DTS carries the full vehicle node set
// (dummy pilot128 seat, dummy exit, dummy eye - verified against flyer.DTS),
// one idle sequence (missing thrust/jet is non-fatal for vehicles), single
// 128 LOD, and a skinnable base.larmor.BMP material (future colored birds).
// $Choco::BirdShape = 1 selects this; 0/unset = flyer shell (known-good).
FlierData ChocoboVehicleBird
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocobo";      // forcefield.DTS with its details(LOD) table FIXED:
	                            // the original registered dummy pilot128/exit/eye
	                            // as DETAIL LEVELS (pilot tied with root128 at size
	                            // 128) - ambiguous LOD rendered an empty subtree
	                            // (invisible at angles) and broke mount resolution
	                            // (client crash). chocobo.dts repoints details 1-3
	                            // at root128 size 0. forcefield.DTS left untouched
	                            // (door forcefields still reference it).
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 45;
	minSpeed = -10;
	lift = 0.9;
	maxAlt = -1;                // <0 = GROUND VEHICLE (engine NATIVE-PORT: no hover pad, no altitude gain)
	maxVertical = 1;            // flightless: tiny climb thrust; MUST be >0 - the engine divides by maxVertical/2 when steering (Flier.cpp:344), 0 = crash; also sets descent rate (-1.75x)
	maxDamage = 1.5;
	damageLevel = {1.0, 1.0};
	maxEnergy = 100;
	accel = 1.4;
	groundDamageScale = 0.5;
	repairRate = 0.5;
	damageSound = SoundFlierCrash;
	ramDamage = 0;
	ramDamageType = -1;
	mapFilter = 2;
	mapIcon = "M_vehicle";
	visibleToSensor = true;
	shadowDetailMask = 2;
	mountSound = SoundFlyerMount;
	dismountSound = SoundFlyerDismount;
	visibleDriver = true;
	driverPose = 22;
	description = "Chocobo";
};

// ------------------------------------------------------------
// Vehicle boarding/dismount handlers. RMRPG's Vehicle.cs was gutted, so the
// engine's className="Vehicle" callbacks had NO script side - colliding with
// a vehicle did nothing and the pilot never received control (spawned fine,
// couldn't move). Ported from Kronos RPG Vehicle.cs (mount = seat 1 +
// setControlObject; dismount = jump). ChocoboVehicle is the only Vehicle-class
// object in RMRPG, but handlers guard on $isChocobo anyway.
// ------------------------------------------------------------
function Vehicle::onAdd(%this)
{
	// Fliers thrust/steer on ENERGY; without a recharge rate the vehicle
	// spawns dead-stick (mounts fine, ignores input). Kronos Vehicle.cs
	// does exactly this in its onAdd.
	%this.shieldStrength = 0.0;
	GameBase::setRechargeRate(%this, 10);
	GameBase::setMapName(%this, "Chocobo");
}

function Vehicle::onCollision(%this, %object)
{
	if(!$isChocobo[%this])
		return;
	if(GameBase::getDamageLevel(%this) >= (GameBase::getDataName(%this)).maxDamage)
		return;
	if(getObjectType(%object) != "Player")
		return;
	// remount cooldown (set on dismount) so you don't instantly re-board
	if(getSimTime() <= %object.newMountTime && %object.lastMount == %this)
		return;
	%clientId = Player::getClient(%object);
	if(%clientId == "" || %clientId == -1 || %clientId == 0)
		return;   // bots/NPCs don't ride
	if(%this.chocoOwner != "" && %clientId != %this.chocoOwner) {
		Client::sendMessage(%clientId, 0, "That's not your Chocobo!");
		return;
	}
	if(!Vehicle::canMount(%this, %object))
		return;   // pilot seat taken

	// stow the weapon like Kronos does (restored on dismount)
	%weapon = Player::getMountedItem(%object, $WeaponSlot);
	if(%weapon != -1) {
		%object.lastWeapon = %weapon;
		Player::unMountItem(%object, $WeaponSlot);
	}
	Player::setMountObject(%object, %this, 1);
	Client::setControlObject(%clientId, %this);   // THE missing piece: hand over the controls
	playSound(GameBase::getDataName(%this).mountSound, GameBase::getPosition(%this));
	%object.driver = 1;
	%object.vehicle = %this;
	%this.clLastMount = %clientId;
}

function Vehicle::jump(%this, %mom)
{
	Vehicle::dismount(%this, %mom);
}

function Vehicle::dismount(%this, %mom)
{
	%cl = GameBase::getControlClient(%this);
	if(%cl == -1)
		return;
	%pl = Client::getOwnedObject(%cl);
	if(getObjectType(%pl) != "Player")
		return;
	if(GameBase::testPosition(%pl, Vehicle::getMountPoint(%this, 0))) {
		%pl.lastMount = %this;
		%pl.newMountTime = getSimTime() + 3.0;
		Player::setMountObject(%pl, %this, 0);
		Player::setMountObject(%pl, -1, 0);
		%rot = GameBase::getRotation(%this);
		GameBase::setRotation(%pl, "0 0 " @ getWord(%rot, 2));
		Player::applyImpulse(%pl, %mom);
		Client::setControlObject(%cl, %pl);
		playSound(GameBase::getDataName(%this).dismountSound, GameBase::getPosition(%this));
		if(%pl.lastWeapon != "") {
			Player::useItem(%pl, %pl.lastWeapon);
			%pl.lastWeapon = "";
		}
		%pl.driver = "";
		%pl.vehicle = "";
	}
	else
		Client::sendMessage(%cl, 0, "Can not dismount - Obstacle in the way.");
}

function Chocobo::Spawn(%Client, %Choco, %pos) {
	if($Choco::VehicleMode) {
		if($ChocoboSpawn[%Client]) {
			Client::sendMessage(%Client, 0, "You already have a Chocobo out!");
			return;
		}
		if($Choco::BirdShape) {
			// color -> skinned datablock (ChocoboSkins.cs); unknown colors ride Yellow
			%c = $ChocoboColor[%Client, %Choco];
			if(%c == "Red")         %vdata = ChocoboVehicleR;
			else if(%c == "Blue")   %vdata = ChocoboVehicleB;
			else if(%c == "Green")  %vdata = ChocoboVehicleG;
			else if(%c == "Black")  %vdata = ChocoboVehicleK;
			else if(%c == "White")  %vdata = ChocoboVehicleW;
			else if(%c == "Gold")   %vdata = ChocoboVehicleAu;
			else                    %vdata = ChocoboVehicleY;
		}
		else
			%vdata = ChocoboVehicle;       // flyer shell (known-good fallback)
		// spawn a few meters ahead so it doesn't auto-mount the moment it appears
		%fwd = Vector::getFromRot(GameBase::getRotation(%Client), 4);
		%pos = Vector::add(%pos, %fwd);
		$ChocoboSpawn[%Client] = newObject("Choco_"@%Client, "flier", %vdata, true);
		if($ChocoboSpawn[%Client]) {
			addToSet("MissionCleanup", $ChocoboSpawn[%Client]);
			GameBase::setPosition($ChocoboSpawn[%Client], %pos);
			$isChocobo[$ChocoboSpawn[%Client]] = true;
			$ChocoboSpawn[%Client].chocoOwner = %Client;  // only the owner may ride
			Client::sendMessage(%Client, 0, "Your Chocobo scratches at the ground. Walk into it to ride! (vehicle-mode test)");
		}
		else
			echo("Error in Chocobo::Spawn (vehicle mode).");
		return;
	}
	if($ChocoboSpawn[%Client]) {
		Client::sendMessage(%Client, 0, "You already have a Chocobo out!");
		return;
	}
	else {
		%armor = Chocobo::SetArmor(%Client, %Choco);
		if(%armor == None)
			return;
		echo("Spawning Chocobo #"@%Choco@" in %armor: "@%armor);
		$ChocoboSpawn[%Client] = newObject("Choco_"@%Client,"Player",%armor,true);
		if($ChocoboSpawn[%Client]) {
			addToSet("MissionCleanup",$ChocoboSpawn[%Client]);
			//GameBase::startFadeOut($ChocoboSpawn[%Client]);
			GameBase::setPosition($ChocoboSpawn[%Client],%pos);
			//%this = Client::getOwnedObject($ChocoboSpawn[%Client]); echo("Spawned %this: "@%this);
			$isChocobo[$ChocoboSpawn[%Client]] = true;
		}
		else
			echo("Error in Chocobo::Spawn.");
	}
}

function Chocobo::SetArmor(%Client, %Choco) {

	%Color = $ChocoboColor[%Client, %Choco];

	%STR = $ChocoboSTR[%Client, %Choco];
	%DEX = $ChocoboDEX[%Client, %Choco];
	%CON = $ChocoboCON[%Client, %Choco];
	%INT = $ChocoboINT[%Client, %Choco];
	%WIS = $ChocoboWIS[%Client, %Choco];

	%a = ChocoboArmor1;

	%Stats = %STR + %DEX + %CON + %INT + %WIS;
	if(%Color != Gold) {

		if(%Stats >= 0 && %Stats <= 250)
			%a = ChocoboArmor4;
		if(%Stats >= 551 && %Stats <= 1099)
			%a = ChocoboArmor5;
		if(%Stats >= 1100) // && %Stats <= 2099)
			%a = ChocoboArmor6;
		if(%DEX > %INT + %WIS) {
				%a = ChocoboArmorDex1;
			if(%Stats >= 500 && %DEX >= 100)
				%a = ChocoboArmorDex1;
			if(%Stats >= 2000 && %DEX >= 350)
				%a = ChocoboArmorDex2;
			if(%Stats >= 4000 && %DEX >= 750)
				%a = ChocoboArmorDex3;
			if(%Stats >= 5000 && %DEX >= 1500)
				%a = ChocoboArmorDex4;
			if(%Stats >= 9000 && %DEX >= 3000)
				%a = ChocoboArmorDex5;
		}
	}
	else if(%Color == Gold) {
		if(%Stats >= 0 && %Stats <= 250)
			%a = ChocoboArmor4;
		if(%Stats >= 551 && %Stats <= 1099)
			%a = ChocoboArmor5;
		if(%Stats >= 1100) //&& %Stats <= 2099)
			%a = ChocoboArmor6;
		if(%DEX > %INT + %WIS) {
				%a = ChocoboArmorDex1;
			if(%Stats >= 500 && %DEX >= 100)
				%a = ChocoboArmorDex1;
			if(%Stats >= 2000 && %DEX >= 350)
				%a = ChocoboArmorDex2;
			if(%Stats >= 4000 && %DEX >= 750)
				%a = ChocoboArmorDex3;
			if(%Stats >= 5000 && %DEX >= 1500)
				%a = ChocoboArmorDex4;
			if(%Stats >= 9000 && %DEX >= 3000)
				%a = ChocoboArmorDex5;
		}
		if(%INT + %WIS > %DEX) {
				%a = ChocoboArmorfly1;
			if(%Stats >= 400 && %INT >= 200 && %WIS >= 200)
				%a = ChocoboArmorfly1;
			if(%Stats >= 1000 && %INT >= 300 && %WIS >= 200)
				%a = ChocoboArmorfly2;
			if(%Stats >= 2000 && %INT >= 350 && %WIS >= 350)
				%a = ChocoboArmorfly3;
			if(%Stats >= 4000 && %INT >= 500 && %WIS >= 400)
				%a = ChocoboArmorfly4;
			if(%Stats >= 5000 && %INT >= 850 && %WIS >= 850)
				%a = ChocoboArmorfly5;
		}
		if(%Stats >= 10000)
			%a = ChocoboArmorSuper;
	}
	if($ChocoboHealth[%Client, %Choco] <= 50 || $ChocoboHungry[%Client, %Choco] <= 50)
		%a = ChocoboArmor3;
	if($ChocoboHealth[%Client, %Choco] <= 40 || $ChocoboHungry[%Client, %Choco] <= 40)
		%a = ChocoboArmor2;
	if($ChocoboHealth[%Client, %Choco] <= 30 || $ChocoboHungry[%Client, %Choco] <= 30)
		%a = ChocoboArmor1;
	if($ChocoboHealth[%Client, %Choco] <= 20 || $ChocoboHungry[%Client, %Choco] <= 20) {
		Client::sendMessage(%Client, 1, "Chocobo "@$ChocoboName[%Client, %Choco]@" won't let you on!");
		%a = None;
	}
	return %a;
}

function Chocobo::Delete(%Client) {

	$isChocobo[$ChocoboSpawn[%Client]] = "";
	deleteObject($ChocoboSpawn[%Client]);
	$ChocoboSpawn[%Client] = "";
}

function Armor::onCollision(%this, %object) {

	if($isChocobo[%this] == "isUsed")
		return;

	%armor = Player::getArmor(%this);

//	if(String::findSubStr(%armor, "ChocoboArmor") != -1)
//		$isChocobo[%this] = true;

//echo("Is it a Choco? "@$isChocobo[%this]);
	if(!$isChocobo[%this]) {
		Client::Bumped(%this, %object);
		return;
	}

	%Client = Player::getClient(%object);
//	%ChocoId = Player::getClient(%this); //Id -1

	if(%object.driver != 1) {

echo("Can mount PASS %this "@%this);

		%weapon = Player::getMountedItem(%object,$WeaponSlot);
		if(%weapon != -1) {
			%object.lastWeapon = %weapon;
			Player::unMountItem(%object,$WeaponSlot);
		}
		Player::setMountObject(%object, %this, 1);
		Client::setControlObject(%Client, %this);
		//playSound(GameBase::getDataName(%this).mountSound, GameBase::getPosition(%this));

		%object.driver = 1;
		Chocobo::Theme(%Client, %object);
		$isChocobo[%this] = "isUsed";
		$OwnersChocoboThis[%Client] = %this;

		//%a = Client::getGender(%Client);
		//Player::setArmor(%Client, %a);

		%object.vehicle = %this;
		%this.clLastMount = %Client;
	}
//else
//echo("Can mount FAILED %this "@%this);
}

function Armor::jump(%this, %mom)
{
	// The jump callback may fire with %this = the controlled chocobo OR the mounted
	// RIDER (players and chocobos share className "Armor"). Handle both; anyone else
	// falls through to the stock vehicle-dismount handling in Player::jump.
	if($isChocobo[%this] == "isUsed") {
		Armor::dismount(%this, %mom);	//%this IS the ridden chocobo
		return;
	}
	%mnt = Player::getMountObject(%this);
	if($isChocobo[%mnt] == "isUsed") {
		Armor::dismount(%mnt, %mom);	//%this is the RIDER; dismount via its chocobo
		return;
	}
	Player::jump(%this, %mom);
}

function Armor::dismount(%this, %mom) {
	%cl = %this.clLastMount;	//the rider's client, stored on mount (Armor::onCollision)
   if(%cl != -1 && %cl != "")
   {
      %pl = Client::getOwnedObject(%cl);
      if(getObjectType(%pl) == "Player")
      {
		   // dismount the player
			if(GameBase::testPosition(%pl, Vehicle::getMountPoint(%this,0))) {
				%pl.lastMount = %this;
				%pl.newMountTime = getSimTime() + 3.0;
				Player::setMountObject(%pl, %this, 0);
        	 	Player::setMountObject(%pl, -1, 0);
				%rot = GameBase::getRotation(%this);
				%rotZ = getWord(%rot,2);
				GameBase::setRotation(%pl, "0 0 " @ %rotZ);
				Player::applyImpulse(%pl,%mom);
        	 	Client::setControlObject(%cl, %pl);
				playSound (GameBase::getDataName(%this).dismountSound, GameBase::getPosition(%this));
				if(%pl.lastWeapon != "") {
					Player::useItem(%pl,%pl.lastWeapon);
					%pl.lastWeapon = "";
      		}
				%pl.driver = "";
				%pl.vehicle = "";
				if($ChocoboSpawn[%cl] == %this)
					Chocobo::Delete(%cl);	//owner hopped off: stable the bird (menu "Return" cleanup)
				else {
					$isChocobo[%this] = true;	//not the rider's bird: leave it standing, free to mount again
					%this.clLastMount = "";
				}
			}
			else
				Client::sendMessage(%cl,0,"Can not dismount - Obstacle in the way.~wError_Message.wav");
		}
   }
}
