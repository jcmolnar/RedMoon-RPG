function remotefetchdata(){}

// NOTE (var names are case-insensitive: $adminpassword IS $AdminPassword):
// These "abc"@getrandom() lines ran AFTER config\rmrpgserv.cs and clobbered the
// real admin passwords with unguessable random values every boot - locking out
// all password admin (SAD("...")). Disabled so config\rmrpgserv.cs is the single
// source of truth for $AdminPassword[1..5]. (Original lockout preserved below.)
//$adminpassword[1]="abc"@getrandom();
//$adminpassword[2]="abc"@getrandom();
//$adminpassword[3]="abc"@getrandom();
//$adminpassword[4]="abc"@getrandom();
//$adminpassword[5]="abc"@getrandom();
//$adminpassword[6]="abc"@getrandom();

//$to_char_places=5;//for health read out
//$max_fights=20;
//$wait=6;//times persecond it checks ALL fights AND updates the waiting list AND has enemies attack
//$max_wait=30;
//$new_fighting_system=1;

$degradechance=1;

//$spawn_time=4;


$arenaOn=False;
$underwaterEffects=True;

//exec(redplanet);
//init_ttl();
//init_fights();


//exec(townbots);
//init_townbots();

//exec(myzones);
//init_myzones();

//$console::logmode = 0;