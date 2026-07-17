//==============================================
// KronosShop_Server.cs (RMRPG) - drive the Kronos shop/bank overlay
// (config\Presto\KronosShop.cs) from RMRPG's native economy.
//==============================================
// The client GUI is protocol-driven: KShopOpen(mode,name), KShopStock rows,
// KShopInv rows, KBankCoins, KShopClose, KShopTip*. Its transaction buttons
// send the STOCK remotes (buyItem/sellItem/useItem/dropItem) plus Kronos bank
// ops (KBankDeposit/Withdraw, KBankCoinsDeposit/Withdraw) and KShopSync.
//
// Design: HUD clients get the overlay; ALL transactions run through RMRPG's
// own remoteBuyItem/remoteSellItem/buyItem/sellItem (distance checks, price
// quote-then-confirm, bank-storage branches, merchant counters - untouched).
// SetupShop/SetupBank route here for HUD clients (economy.cs), INCLUDING the
// post-transaction refresh calls those functions make - so after every
// buy/sell the overlay repopulates instead of the stock GUI popping up.
// Vanilla clients keep the stock GuiMode-4 screen exactly as before.
//
// NOT covered (stays on the stock GUI even for HUD clients): the blacksmith
// flow (SetupBlackSmith - its Sell-to-add-material staging is bespoke) and
// lootbag shops (SetUpLootShop).
//==============================================

$KronosShopRM::MaxRows = 120;   // row cap per pane push

// --- shared helpers --------------------------------------------------------

// Heading strings for the client are "<sortkey><Label>" - the FIRST CHARACTER
// is a category sort key the client strips for display ("aArmor" -> "Armor").
// Sending the raw label ate its first letter ("Shields" showed as "hields").
// The key comes from the $Headers:: letter (A..Z) - RMRPG's own category
// order - via a reverse map built once at exec.
function KronosShopRM_BuildHeadMap()
{
	for(%i = 0; %i < 26; %i++)
	{
		%ch = String::getSubStr("ABCDEFGHIJKLMNOPQRSTUVWXYZ", %i, 1);
		eval("$KronosShopRM::tmpHead = $Headers::" @ %ch @ ";");
		if($KronosShopRM::tmpHead != "" && $KronosShopRM::tmpHead != -1)
			if($KronosShopRM::HeadKey[$KronosShopRM::tmpHead] == "")
				$KronosShopRM::HeadKey[$KronosShopRM::tmpHead] = %ch;
	}
	$KronosShopRM::tmpHead = "";
}
KronosShopRM_BuildHeadMap();

function KronosShopRM_Heading(%item)
{
	%h = $ItemData[%item, header];
	if(%h == "" || %h == -1)
		%h = "Items";
	%k = $KronosShopRM::HeadKey[%h];
	if(%k == "")
		%k = "Z";
	return %k @ %h;
}

function KronosShopRM_Name(%item)
{
	%n = $ItemData[%item, Name];
	if(%n == "" || %n == -1)
		%n = %item;
	return %n;
}

// The item REF pushed to the client comes back verbatim on every action
// (buyItem/sellItem/useItem/dropItem/KBank*/KShopTip), and those remotes all
// resolve items via $ItemData[<ref>, DataName] - a table keyed by the SPACED
// item name ("Light Potion"), exactly what the native client sends after its
// AddSpaces pass. Internal lists ($TownBot SHOP / ItemList / $BankStorage)
// hold the UNDERSCORED datanames, so convert before pushing. Safe inversion:
// MakeItem rejects datanames containing literal underscores.
function KronosShopRM_SpacedRef(%item)
{
	%guard = 0;
	while(String::findSubStr(%item, "_") != -1 && %guard < 16)
	{
		%guard++;
		%item = String::replace(%item, "_", " ");
	}
	return %item;
}

// Remove <f0>/<f1>/<jc>/... engine markup (item info strings carry it; the
// overlay draws text raw via ScriptGL).
function KronosShopRM_Strip(%s)
{
	%guard = 0;
	while(%guard < 32)
	{
		%guard++;
		%p = String::findSubStr(%s, "<");
		if(%p == -1)
			return %s;
		%rest = String::getSubStr(%s, %p, 99999);
		%e = String::findSubStr(%rest, ">");
		if(%e == -1)
			return %s;
		%s = String::getSubStr(%s, 0, %p) @ String::getSubStr(%s, %p + %e + 1, 99999);
	}
	return %s;
}

// Overlay-open + proximity gate for everything the client can send.
function KronosShopRM_Gate(%Client)
{
	if(!%Client.hasKronosHUD)
		return false;
	if(%Client.kshopOpen == "")
		return false;
	// plain inventory mode ("I" key) has no bot to stand near
	if(%Client.kshopOpen == "inv")
		return true;
	// same distance rule as the native GUI (10m, drops to play mode if far)
	return Client::isShoppingOn(%Client, %Client.currentShop, %Client.currentBank, %Client.currentLoot, %Client.currentSmith);
}

// --- open / refresh ---------------------------------------------------------
// Called from economy.cs SetupShop/SetupBank for HUD clients - both on the
// initial hand-off and on every post-transaction refresh.

function KronosShopRM_OpenShop(%Client, %id, %i)
{
	%Client.kshopOpen = "shop";
	%Client.kshopShopIdx = %i;

	remoteEval(%Client, "KShopOpen", "shop", $TownBot[%id, NAME]);
	KronosShopRM_PushStock(%Client);
	KronosShopRM_PushInv(%Client);
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
	KronosShopRM_Cursor(%Client);
}

function KronosShopRM_OpenBank(%Client, %id)
{
	%Client.kshopOpen = "bank";
	%Client.kshopShopIdx = "";

	remoteEval(%Client, "KShopOpen", "bank", $TownBot[%id, NAME]);
	KronosShopRM_PushStock(%Client);
	KronosShopRM_PushInv(%Client);
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
	KronosShopRM_Cursor(%Client);
}

// Plain inventory screen ("I" key) - inventory pane only, no stock. Wired
// from remote.cs remoteToggleInventoryMode/remoteInventoryMode HUD gates.
function KronosShopRM_OpenInv(%Client)
{
	%Client.kshopOpen = "inv";
	%Client.kshopShopIdx = "";

	remoteEval(%Client, "KShopOpen", "inv", "");
	remoteEval(%Client, "KShopStockCount", 0);
	KronosShopRM_PushInv(%Client);
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
	KronosShopRM_Cursor(%Client);
}

// Mouse cursor for the overlay - the Kronos mechanism: open the score dialog
// (its stock controls are binary-patched off-screen on HUD clients, so it's
// invisible) purely to raise the engine GUI cursor. Scheduled a beat later,
// exactly like the Kronos original, so it lands after any menu-close
// setMenuScoreVis(false) from the same tick. Closing the score screen (TAB)
// closes the shop - see the remoteScoresOff hook in remote.cs.
function KronosShopRM_Cursor(%Client)
{
	schedule("if(" @ %Client @ ".kshopOpen != \"\") Client::setMenuScoreVis(" @ %Client @ ", true);", 0.15);
}

// Shared server-side close: clear state, drop the cursor (score dialog),
// tell the client. The KShopClose echo is ALWAYS sent - the client's own
// Esc/X path doesn't hide the panel locally, it waits for this echo
// (mirrors the Kronos original).
function KronosShopRM_Close(%Client)
{
	if(%Client.kshopOpen == "")
		return;
	%Client.kshopOpen = "";
	%Client.kshopShopIdx = "";
	remoteEval(%Client, "KShopClose");
	Client::setMenuScoreVis(%Client, false);
}

// Stock pane. Shop mode: the merchant's SHOP list with buy prices.
// Bank mode: the player's stored items - the "price" column carries the
// stored COUNT (the client's withdraw button sends it back as the amount).
function KronosShopRM_PushStock(%Client)
{
	%sent = 0;
	if(%Client.kshopOpen == "bank")
	{
		%list = $BankStorage[%Client];
		for(%i = 0; (%item = GetWord(%list, %i)) != -1; %i += 2)
		{
			%cnt = floor(GetWord(%list, %i + 1));
			if(%cnt <= 0)
				continue;
			if(%sent >= $KronosShopRM::MaxRows)
				break;
			remoteEval(%Client, "KShopStock", %sent, "d", KronosShopRM_SpacedRef(%item), %cnt, KronosShopRM_Heading(%item), KronosShopRM_Name(%item));
			%sent++;
		}
	}
	else
	{
		%aiName = $TownBot[%Client.currentShop, NAME];
		if(%Client.kshopShopIdx != "")
			%list = $TownBot[%Client.currentShop, SHOP, %Client.kshopShopIdx];
		else
			%list = $TownBot[%Client.currentShop, SHOP];
		for(%i = 0; (%item = GetWord(%list, %i)) != -1; %i++)
		{
			if(%sent >= $KronosShopRM::MaxRows)
				break;
			remoteEval(%Client, "KShopStock", %sent, "d", KronosShopRM_SpacedRef(%item), getBuyCost(%aiName, %item), KronosShopRM_Heading(%item), KronosShopRM_Name(%item));
			%sent++;
		}
	}
	remoteEval(%Client, "KShopStockCount", %sent);
}

// Inventory pane: the player's carried ItemList ("item count item count ...").
function KronosShopRM_PushInv(%Client)
{
	%sent = 0;
	%list = $ClientData[%Client, ItemList];
	for(%i = 0; (%item = GetWord(%list, %i)) != -1; %i += 2)
	{
		%cnt = floor(GetWord(%list, %i + 1));
		if(%cnt <= 0)
			continue;
		if(%sent >= $KronosShopRM::MaxRows)
			break;
		remoteEval(%Client, "KShopInv", %sent, "d", KronosShopRM_SpacedRef(%item), %cnt, KronosShopRM_Heading(%item), KronosShopRM_Name(%item));
		%sent++;
	}
	remoteEval(%Client, "KShopInvCount", %sent);
}

// --- client requests --------------------------------------------------------

// Full refresh (the client schedules this 0.5s after each action)
function remoteKShopSync(%Client)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	KronosShopRM_PushStock(%Client);
	KronosShopRM_PushInv(%Client);
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
}

// Player closed the overlay (Esc / X) - mirror the native close (clears
// currentShop/currentBank via remotePlayMode, which also runs the close).
function remoteKShopClose(%Client)
{
	if(!%Client.hasKronosHUD)
		return;
	remotePlayMode(%Client);   // closes the overlay (KShopClose echo) + play mode
}

// Item tooltip (hover). Composed from $ItemData directly - RMRPG's WhatIs
// can't be reused here because it pushes the SetPrint popup to the client
// as a side effect (would pop the stock info box on every hover).
function remoteKShopTip(%Client, %kind, %ref)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	// refs arrive SPACED (see KronosShopRM_SpacedRef); resolve the dataname
	// all the $ItemData detail fields are keyed by
	%item = $ItemData[%ref, DataName];
	if(%item == "" || %item == -1)
		return;

	%msg = KronosShopRM_Name(%item);
	%info = KronosShopRM_Strip($ItemData[%item, info]);
	if(%info != "" && %info != -1)
		%msg = %msg @ "    " @ %info;
	%bonus = WhatSpecialVars(%item);
	if(%bonus != "" && %bonus != "None" && %bonus != -1)
		%msg = %msg @ "    Bonus: " @ %bonus;
	%w = $ItemData[%item, weight];
	if(%w != "" && %w != -1)
		%msg = %msg @ "    Weight: " @ %w;
	if(%Client.kshopOpen == "shop")
		%msg = %msg @ "    Sells back: " @ FixM(getSellCost($TownBot[%Client.currentShop, NAME], %item)) @ " gil";

	remoteEval(%Client, "KShopTipBegin");
	%len = String::len(%msg);
	for(%p = 0; %p < %len; %p += 150)
		remoteEval(%Client, "KShopTipPart", String::getSubStr(%msg, %p, 150));
	remoteEval(%Client, "KShopTipDone");
}

// --- bank operations --------------------------------------------------------
// Item deposit/withdraw reuse RMRPG's own handlers: at a bank, sellItem
// DEPOSITS into $BankStorage and buyItem WITHDRAWS from it (economy.cs) -
// same validation, caps and refresh as the native GUI.

function remoteKBankDeposit(%Client, %ref, %cnt)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	if(%Client.kshopOpen != "bank")
		return;
	remoteSellItem(%Client, %ref, %cnt);
}

function remoteKBankWithdraw(%Client, %ref, %cnt)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	if(%Client.kshopOpen != "bank")
		return;
	remoteBuyItem(%Client, %ref, %cnt);
}

// Coin deposit/withdraw: same rules as the banker conversation (comchat3.cs)
// including the $MaxCOINS carry cap.
function remoteKBankCoinsDeposit(%Client, %amt)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	if(%Client.kshopOpen != "bank")
		return;
	if(%amt == "all")
		%amt = $COINS[%Client];
	%c = floor(%amt);
	if(%c <= 0)
		return;
	if(%c > $COINS[%Client])
		%c = $COINS[%Client];
	$BANK[%Client] += %c;
	$COINS[%Client] -= %c;
	playSound(SoundMoney1, GameBase::getPosition(%Client));
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
	Game::refreshClientScore(%Client);
}

function remoteKBankCoinsWithdraw(%Client, %amt)
{
	if(!KronosShopRM_Gate(%Client))
		return;
	if(%Client.kshopOpen != "bank")
		return;
	if(%amt == "all")
		%amt = $BANK[%Client];
	%c = floor(%amt);
	if(%c <= 0)
		return;
	if(%c > $BANK[%Client])
		%c = $BANK[%Client];
	if(%c + $COINS[%Client] > $MaxCOINS[%Client])
		%c = $MaxCOINS[%Client] - $COINS[%Client];
	if(%c <= 0)
	{
		Client::sendMessage(%Client, 0, "You cannot carry over "@$MaxCOINS[%Client]@" gil.");
		return;
	}
	$COINS[%Client] += %c;
	$BANK[%Client] -= %c;
	playSound(SoundMoney1, GameBase::getPosition(%Client));
	remoteEval(%Client, "KBankCoins", $COINS[%Client], $BANK[%Client]);
	Game::refreshClientScore(%Client);
}

// Belt ops don't exist on RMRPG; the client only sends them for "b" rows,
// which this server never pushes. Stubs guard against crafted packets.
function remoteKShopBeltBuy(%Client, %item) {}
function remoteKShopBeltSell(%Client, %item) {}
function remoteKShopBeltUse(%Client, %item) {}
function remoteKShopBeltDrop(%Client, %item) {}
function remoteKBankBeltDeposit(%Client, %item, %cnt) {}
function remoteKBankBeltWithdraw(%Client, %item, %cnt) {}

echo("KronosShop_Server (RMRPG): shop/bank overlay adapter loaded");
