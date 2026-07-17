// ============================================================================
// CHOCOBO COLOR SKINS - generated datablock set
// One FlierData per color; each chocobo<X>.dts is chocobo.dts with the texture
// name baked into the material slot (chocoY.bmp etc, in RMRPG\skins\).
// The engine's "base." skin substitution keys off owner/team skins, which
// can't express per-object colors - baked copies are the robust route.
// Selected in Chocobo::Spawn by $ChocoboColor. Gold is slightly faster ;)
// exec'd from Chocobo.cs (datablocks exist before preloadServerDataBlocks).
// ============================================================================
FlierData ChocoboVehicleY
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboY";
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
	description = "Yellow Chocobo";
};
FlierData ChocoboVehicleR
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboR";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 47;
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
	description = "Red Chocobo";
};
FlierData ChocoboVehicleB
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboB";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 47;
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
	description = "Blue Chocobo";
};
FlierData ChocoboVehicleG
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboG";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 47;
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
	description = "Green Chocobo";
};
FlierData ChocoboVehicleK
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboK";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 50;
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
	description = "Black Chocobo";
};
FlierData ChocoboVehicleW
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboW";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 47;
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
	description = "White Chocobo";
};
FlierData ChocoboVehicleAu
{
	explosionId = flashExpLarge;
	debrisId = flashDebrisLarge;
	className = "Vehicle";
	shapeFile = "chocoboAu";
	shieldShapeName = "shield_medium";
	mass = 9.0;
	drag = 1.0;
	density = 1.2;
	maxBank = 0.05;             // ground mount: no banking lean in turns
	maxPitch = 0.6;             // slope handling; flight blocked engine-side (maxAlt<0)
	maxSpeed = 55;
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
	description = "Gold Chocobo";
};
