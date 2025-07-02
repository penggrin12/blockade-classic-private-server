using System;
using System.IO;
using Godot;

namespace BlockadeClassicPrivateServer.Net.Shared;

public static class ItemsDB
{
	public static ItemData[] Items { get; } = new ItemData[(int)ItemName.Vehicles_Max];

	public static int GetDamage(ItemName itemName) => Items[(int)itemName].Upgrades[1][0].Value;

	public static int GetClipSize(ItemName itemName) => Items[(int)itemName].Upgrades[2][0].Value;

	public static int GetReserveSize(ItemName itemName) => Items[(int)itemName].Upgrades[3][0].Value;

	public static int GetFireRate(ItemName itemName) => Items[(int)itemName].Upgrades[4][0].Value;

	public static int GetReloadSpeed(ItemName itemName) => Items[(int)itemName].Upgrades[5][0].Value;

	static ItemsDB()
	{
		GD.Print("[ITEMS] Gonna populate ItemsDB");

		using var stream = File.OpenRead(ProjectSettings.GlobalizePath("res://items.bin"));
		using var reader = new BinaryReader(stream);

		int itemCount = reader.ReadInt32();

		int totalAdded = 0;

		for (int i = 0; i < itemCount; i++)
		{
			int itemId = reader.ReadInt32();

			if (itemId >= 999 || !Enum.IsDefined(typeof(ItemName), itemId))
			{
				ItemType itemType = (ItemType)reader.ReadByte();
				stream.Seek(4 + (sizeof(int) * 3), SeekOrigin.Current); // skip 4 bytes and 3 ints

				if (itemType < ItemType.Customization)
				{
					while (reader.ReadByte() != 255)
					{
						stream.Seek(2 + (sizeof(int) * 2), SeekOrigin.Current); // skip 2 bytes and 2 ints
					}
				}

				continue;
			}

			Items[itemId] = new ItemData(reader, (ItemName)itemId);
			totalAdded++;
		}

		GD.Print($"[ITEMS] Done! Added {totalAdded} items.");
	}
}

public readonly record struct WeaponUpgrade(int Cost, int Value)
{
	public const int MaxTypes = 7;
	public const int MaxLevels = 6;
}

public readonly record struct ItemData
{
	public readonly ItemName ItemName { get; init; }
	public readonly ItemType Type { get; init; }
	public readonly ItemCategory Category { get; init; }
	public readonly int ShowStatus { get; init; }
	public readonly ItemTheme Theme { get; init; }
	public readonly int Lvl { get; init; }
	public readonly int CostGold { get; init; }
	public readonly float CostSocial { get; init; }
	public readonly int Count { get; init; }
	public readonly WeaponUpgrade[][] Upgrades { get; init; }

	private static WeaponUpgrade[][] CreateDefaultUpgradesArray()
	{
		var upgrades = new WeaponUpgrade[WeaponUpgrade.MaxTypes][];
		for (int i = 0; i < WeaponUpgrade.MaxTypes; i++)
		{
			upgrades[i] = new WeaponUpgrade[WeaponUpgrade.MaxLevels];
		}
		return upgrades;
	}

	public ItemData(BinaryReader reader, ItemName itemName)
	{
		ItemName = itemName;
		Type = (ItemType)reader.ReadByte();
		Category = (ItemCategory)reader.ReadByte();
		Lvl = reader.ReadByte();
		ShowStatus = reader.ReadByte();
		Theme = (ItemTheme)reader.ReadByte();
		CostGold = reader.ReadInt32();
		CostSocial = (float)reader.ReadInt32();
		Count = reader.ReadInt32();

		Upgrades = CreateDefaultUpgradesArray();

		if (Type < ItemType.Customization)
		{
			while (reader.ReadByte() != 255)
			{
				int upgradeType = reader.ReadByte();
				int upgradeLevel = reader.ReadByte();
				int upgradeValue = reader.ReadInt32();
				int upgradeCost = reader.ReadInt32();

				Upgrades[upgradeType][upgradeLevel] = new WeaponUpgrade(upgradeCost, upgradeValue);
			}
		}
	}
}

public class ChunkData(Vector3I position, int size, byte[]? blocks = null)
{
	public Vector3I Position { get; } = position;
	public int Size { get; } = size;
	public byte[] BlockTypes { get; } = blocks ?? (new byte[size * size * size]);
}

// BotsController.CreatePlayer
public enum BodyPart : byte
{
    Spine = 0,
    Head = 1,
    R_UpperArm = 2,
    R_Forearm = 3,
    R_Hand = 4,
    L_UpperArm = 5,
    L_Forearm = 6,
    L_Hand = 7,
    R_Thigh = 8,
    R_Calf = 9,
    R_Foot = 10,
    L_Thigh = 11,
    L_Calf = 12,
    L_Foot = 13,

    Shield = 77
}

// NETWORK
public enum Network : byte
{
	VK = 1, // vkontakte (vk.ru)
	ST, // steam
	OK, // odnoklassniki (ok.ru)
	FB, // facebook
	MM, // ?
	KG, // ?
	NL, // NovaLink (their launcher)
	ID, // ?
	KR, // ?
}

// GUIManager.tex_flags
public enum Flag : byte
{
	Xx = 0, // blank
	Ru,
	Ua,
	By,
	Kz,
	Md,
	Ee,
	Lv,
	De,
	Am,
	Us,
	Ad,
	Ae,
	Af,
	Ag,
	Ai,
	Al,
	An,
	Ao,
	Ar,
	As,
	At,
	Au,
	Aw,
	Ax,
	Az,
	Ba,
	Bb,
	Bd,
	Be,
	Bf,
	Bg,
	Bh,
	Bi,
	Bj,
	Bm,
	Bn,
	Bo,
	Br,
	Bs,
	Bt,
	Bv,
	Bw,
	Bz,
	Ca,
	Cc,
	Cd,
	Cf,
	Cg,
	Ch,
	Ci,
	Ck,
	Cl,
	Cm,
	Cn,
	Co,
	Cr,
	Cs,
	Cu,
	Cv,
	Cx,
	Cy,
	Cz,
	Dj,
	Dk,
	Dm,
	Do,
	Dz,
	Ec,
	Eg,
	Eh,
	Er,
	Es,
	Et,
	Fi,
	Fj,
	Fk,
	Fm,
	Fo,
	Fr,
	Ga,
	Gb,
	Gd,
	Ge,
	Gf,
	Gh,
	Gi,
	Gl,
	Gm,
	Gn,
	Gp,
	Gq,
	Gr,
	Gs,
	Gt,
	Gu,
	Gw,
	Gy,
	Hk,
	Hm,
	Hn,
	Hr,
	Ht,
	Hu,
	Id,
	Ie,
	Il,
	In,
	Io,
	Iq,
	Ir,
	Is,
	It,
	Jm,
	Jo,
	Jp,
	Ke,
	Kg,
	Kh,
	Ki,
	Km,
	Kn,
	Kp,
	Kr,
	Kw,
	Ky,
	La,
	Lb,
	Lc,
	Li,
	Lk,
	Lr,
	Ls,
	Lt,
	Lu,
	Ly,
	Ma,
	Mc,
	Me,
	Mg,
	Mh,
	Mk,
	Ml,
	Mm,
	Mn,
	Mo,
	Mp,
	Mq,
	Mr,
	Ms,
	Mt,
	Mu,
	Mv,
	Mw,
	Mx,
	My,
	Mz,
	Na,
	Nc,
	Ne,
	Nf,
	Ng,
	Ni,
	Nl,
	No,
	Np,
	Nr,
	Nu,
	Nz,
	Om,
	Pa,
	Pe,
	Pf,
	Pg,
	Ph,
	Pk,
	Pl,
	Pm,
	Pn,
	Pr,
	Ps,
	Pt,
	Pw,
	Py,
	Qa,
	Re,
	Ro,
	Rs,
	Rw,
	Sa,
	Sb,
	Sc,
	Sd,
	Se,
	Sg,
	Sh,
	Si,
	Sj,
	Sk,
	Sl,
	Sm,
	Sn,
	So,
	Sr,
	St,
	Sv,
	Sy,
	Sz,
	Tc,
	Td,
	Tf,
	Tg,
	Th,
	Tj,
	Tk,
	Tl,
	Tm,
	Tn,
	To,
	Tr,
	Tt,
	Tv,
	Tw,
	Tz,
	Ug,
	Um,
	Uy,
	Uz,
	Va,
	Vc,
	Ve,
	Vg,
	Vi,
	Vn,
	Vu,
	Wf,
	Ws,
	Ye,
	Yt,
	Za,
	Zm,
	Zw,
}

// ZipLoader.GetBlock
public enum BlockType : byte
{
	None = 0,
	Stoneend = 1,
	Dirt = 2,
	Grass = 3,
	Snow = 4,
	Sand = 5,
	Stone = 6,
	Water = 7,
	Wood = 8,
	Wood2 = 9,
	Leaf = 10,
	Brick = 11,
	Brick_Blue = 12,
	Brick_Red = 13,
	Brick_Green = 14,
	Brick_Yellow = 15,
	Window = 16,
	Box = 17,
	Brick2 = 18,
	Stone2 = 19,
	Stone3 = 20,
	Stone4 = 21,
	Tile = 22,
	Stone5 = 23,
	Sand2 = 24,
	Stone6 = 25,
	Metall1 = 26,
	Metall2 = 27,
	Stone7 = 28,
	Stone8 = 29,
	R_B_Blue = 30,
	R_B_Red = 31,
	R_B_Green = 32,
	R_B_Yellow = 33,
	R_Z = 34,
	R_C_Blue = 35,
	R_C_Red = 36,
	R_Center = 37,
	Color1 = 38,
	Color2 = 39,
	Color3 = 40,
	Color4 = 41,
	Color5 = 42,
	Color6 = 43,
	Color7 = 44,
	Color8 = 45,
	Color9 = 46,
	Color10 = 47,
	Color11 = 48,
	Color12 = 49,
	TNT = 50,
	Danger = 51,
	Barrel1 = 52,
	Barrel2 = 53,
	Barrel3 = 54,
	Barrel4 = 55,
	Barrel5 = 56,
	Block1 = 57,
	Box2 = 58,
	Block2 = 59,
	Block3 = 60,
	Block4 = 61,
	Block5 = 62,
	Block6 = 63,
	Block7 = 64,
	Block8 = 65,
	Block9 = 66,
	Block10 = 67,
	Block11 = 68,
	Block12 = 69,
	Block13 = 70,
	Block14 = 71,
	Block15 = 72,
	Block16 = 73,
	Armored_Brick_Blue = 74,
	Armored_Brick_Red = 75,
	Armored_Brick_Green = 76,
	Armored_Brick_Yellow = 77
}

// MODE
public enum GameMode : sbyte
{
	Null = -1,
	Battle = 0,
	Classic,
	Build,
	Zombie,
	Capture,
	Contra,
	Melee,
	Survival,
	M1945,
	Proriv,
	Clear,
	Tank,
	Snowballs,
}

// CONST.VEHICLES
public enum VehicleName : int
{
	None = 0,
	Position_Jeep_Driver = 0,
	Tanks = 1,
	Position_Jeep_Gunner = 1,
	Jeep = 2,
	Position_Jeep_Pass = 2,
	Position_None = 200,
	Vehicle_Tank_Light = 200,
	Vehicle_Tank_Medium = 201,
	Vehicle_Tank_Heavy = 202,
	Vehicle_Jeep = 203,
	Vehicle_Modul_Tank_Mg = 220,
	Vehicle_Modul_Repair_Kit = 221,
	Vehicle_Modul_Anti_Missle = 222,
	Vehicle_Modul_Smoke = 223,
}

// CONST.ENTS
public enum EntityName : int
{
	Grenade = 1,
	Shmel = 2,
	Zombie = 3,
	Gp = 4,
	Boat = 5,
	Shturm_Minen = 6,
	Turrets = 7,
	Tnt_Place = 8,
	Fence = 9,
	Zombie2 = 10,
	Zombie_Boss = 11,
	Ej = 12,
	Tank = 13,
	Tank_Snaryad = 14, // tank shell
	Rpg = 15,
	Tank_Light = 16,
	Tank_Medium = 17,
	Tank_Heavy = 18,
	Zbk18m = 19,
	Zof26 = 20,
	Minefly = 21,
	Javelin = 22,
	Arrow = 23,
	Smoke_Grenade = 24,
	He_Grenade = 25,
	Rkg3 = 26,
	Mine = 27,
	C4 = 28,
	Jeep = 29,
	Anti_Missle = 30, // anti (tank?) missile
	Smoke = 31,
	At_Mine = 32,
	Molotov = 33,
	M202 = 34,
	Gaz_Grenade = 35,
	Snowball = 36,
	Ghost = 37,
	Ghost_Boss = 38,
	Stielhandgranate = 39,
	Max_Ents = 512, // yet they are casted to a byte on the client...
}

// CONST.TEAMS
public enum Team : int
{
	Blue = 0,
	Red = 1,
	Green = 2,
	Yellow = 3,

	Bot = 255, // SpawnManager.SetRandomFollow
}

// ITEM_CATEGORY
public enum ItemCategory : int
{
	Pistols = 1,
	Pp, // Submachines
	Automats, // Automatic Rifles
	Machineguns,
	Snipers,
	Shotguns,
	Melee,
	Rest, // Unused...?
	Tanks,
	Cars,
	Planes,
	Hely, // Helicopters?
	Boats,
	Player_Skins,
	Vehicle_Skins,
	Badges,
	Caps,
	Armor,
	Meds, // Medkits
	Grens, // Grenades
	Launchers,
	Explosives,
	Barricades,
	Gifts,
	Premium,
	Modes,
	Maps,
	Clan,
	Megapacks, // All the weapons..?
	Other,
	Vehicles_Ammo,
}

// ITEM_THEME
public enum ItemTheme : byte
{
	Standart = 1,
	Lady,
	Zombie,
	Wwii,
	Ny, // New year
	Halloween,
}

// ITEM_TYPE
public enum ItemType : byte
{
	Weapons = 1,
	Vehicles,
	Customization,
	Ammunition,
	Other,
}

// ITEM
public enum ItemName : int
{
	Block,
	Helmet,
	Helmetplus = 146,
	Vest = 54,
	Vestplus = 147,
	Shovel = 33,
	Glock = 46,
	M3 = 43,
	M14,
	Mp5,
	Ak47 = 2,
	Svd,
	M61 = 7,
	Zombie = 35,
	Deagle = 9,
	Shmel,
	Asval = 12,
	G36c,
	Kriss,
	M4a1,
	Vsk94 = 19,
	M249 = 16,
	Spas12,
	Vintorez,
	Medkit_S = 36,
	Medkit_M,
	Medkit_L,
	Kar98k,
	Usp,
	Barrett = 47,
	Tmp,
	Axe = 50,
	Bat,
	Crowbar,
	Knife = 49,
	Caramel = 53,
	Tnt = 55,
	Mortar = 62,
	Auga3 = 60,
	Sg552,
	M14ebr = 68,
	L96a1,
	Nova,
	Kord,
	Anaconda,
	Scar_H,
	P90,
	Rpk = 78,
	Hk416,
	Ak102,
	Sr25,
	Mglmk1,
	Gp = 77,
	Mosin = 89,
	Ppsh,
	Mp40,
	L96a1mod = 34,
	Tt = 92,
	Kacpdw,
	Famas,
	Beretta,
	Machete,
	Rpg = 100,
	Wrench,
	Aa12,
	Fn57,
	Fs2000,
	L85,
	Mac10,
	Pkp,
	Pm,
	Tar21,
	Ump45,
	Ntw20,
	Vintorez_Desert,
	Minigun = 137,
	Javelin,
	Zaa12,
	Zasval,
	Zfn57,
	Zkord,
	Zm249,
	Zminigun,
	Zspas12,
	Mg13 = 154,
	Rpd,
	Stielhandgranate,
	M18 = 169,
	Mk3 = 168,
	Rkg3 = 170,
	Tube = 157,
	Bulava,
	Katana,
	Mauzer,
	Crossbow,
	Qbz95,
	Mine = 171,
	C4,
	Chopper,
	Shield,
	Aksu,
	M700,
	Stechkin,
	At_Mine = 183,
	Molotov,
	M202,
	M7a2,
	Armored_Block = 198,
	Dpm = 188,
	M1924,
	Mg42,
	Sten_Mk2,
	M1a1,
	Type99,
	Tank_Mg = 136,
	Bizon = 207,
	Groza,
	Jackhammer,
	Chainsaw,
	Psg_1 = 218,
	Krytac,
	Mp5sd,
	Colts,
	Jackhammer_Lady = 301,
	M700_Lady,
	Mg42_Lady,
	Shield_Lady,
	Magnum_Lady,
	Scorpion = 308,
	G36c_Veteran,
	Fmg9 = 313,
	Saiga,
	Flamethrower,
	Ak47_Snow = 329,
	P90_Snow,
	Saiga_Snow,
	Sr25_Snow,
	Usp_Snow,
	Vehicle_Repair_Kit = 135,
	Tykva = 211, // pumpkin head
	Tank_Medium = 202,
	Tank_Light = 201,
	Tank_Heavy = 203,
	Jeep,
	Module_Anti_Missle,
	Module_Smoke,
	Zbk18m = 133,
	Zof26,
	Skin_Default = 32,
	Skin_1337 = 42,
	Skin_Bronxy = 227,
	Skin_Franky,
	Skin_Mozzy,
	Skin_Sas = 8,
	Skin_Sf1000 = 97,
	Skin_Sf1122 = 99,
	Skin_Sf1207 = 98,
	Skin_Usmc = 27,
	Skin_Alice = 216,
	Skin_Anarchist = 163,
	Skin_Belsnickel = 148,
	Skin_Arctic = 41,
	Skin_Archer = 324,
	Skin_Assassin = 316,
	Skin_Bandit = 66,
	Skin_Prisoner = 75,
	Skin_Blokadovec = 86,
	Skin_Bomber = 64,
	Skin_Britanec = 194,
	Skin_Rebel = 22,
	Skin_Survivor = 65,
	Skin_German = 88,
	Skin_Grinch = 312,
	Skin_Ubivashka = 180,
	Skin_Desant = 28,
	Skin_Jason = 114,
	Skin_Jack = 30,
	Skin_Joker = 319,
	Skin_Dracula = 213,
	Skin_Duke = 325,
	Skin_April = 178,
	Skin_Zgirl = 117, // zombie girl
	Skin_Zboy = 116, // zombie boy
	Skin_Engineer = 326,
	Skin_Casper = 29,
	Skin_Kestrel = 327,
	Skin_Rabbit_Girl = 150,
	Skin_Rabbit_Boy = 149,
	Skin_Mulatka = 179,
	Skin_Mummy = 214,
	Skin_Rebelterror = 164,
	Skin_Mercenary = 67,
	Skin_Mercgirl = 76,
	Skin_Werewolf = 215,
	Skin_Security = 328,
	Skin_It = 318,
	Skin_Marine = 165,
	Skin_Saw = 320,
	Skin_Pilot = 21,
	Skin_Insurgent = 20,
	Skin_Cop = 85,
	Skin_Ghost = 5,
	Skin_Scarecrow = 321,
	Skin_Rider = 23,
	Skin_Santa = 59,
	Skin_Sapper = 306,
	Skin_Skeleton = 323,
	Skin_Slender = 31,
	Skin_Sniper = 24,
	Skin_Snowman = 57,
	Skin_Santagirl,
	Skin_Soviet = 87,
	Skin_Vvs = 167,
	Skin_Vmf = 166,
	Skin_Us = 197,
	Skin_Soldier = 4,
	Skin_Spec = 25,
	Skin_Stalker,
	Skin_Terror = 11,
	Skin_Terroristka = 181, // female terror
	Skin_Silentassassin = 311,
	Skin_Toxik = 182,
	Skin_Killer = 84,
	Skin_Scientist = 322,
	Skin_French = 195,
	Skin_Freddy = 113,
	Skin_Harley = 317,
	Skin_Rorschach = 115,
	Skin_Elf = 56,
	Skin_Japonia = 196, // japan(ese?)
	Skin_Lt_Desert = 118,
	Skin_Lt_Hexagon,
	Skin_Lt_Multicam,
	Skin_Lt_Tiger,
	Skin_Lt_Wood,
	Skin_Lt_Winter = 151,
	Skin_Mt_Desert = 123,
	Skin_Mt_Hexagon,
	Skin_Mt_Multicam,
	Skin_Mt_Tiger,
	Skin_Mt_Wood,
	Skin_Mt_Winter = 152,
	Skin_Ht_Desert = 128,
	Skin_Ht_Hexagon,
	Skin_Ht_Multicam,
	Skin_Ht_Tiger,
	Skin_Ht_Wood,
	Skin_Ht_Winter = 153,
	Badge_70years_Wwii = 240,
	Badge_Glenta, // Ribbon of Saint George
	Badge_B3d_3years = 251,
	Badge_Belorussia = 250, // belarus
	Badge_Britan = 242, // united kingdom (britain)
	Badge_Germany = 244,
	Badge_Russia = 246,
	Badge_Ussr = 248,
	Badge_Usa = 247,
	Badge_Ukraina = 249, // ukraine
	Badge_French = 243,
	Badge_Japonia = 245, // japan
	Roga = 223, // horns
	Kolpak = 222, // santa hat
	Mask_Fox = 225,
	Mask_Bear = 224,
	Mask_Rabbit = 226,
	Premium = 6,
	Mode_Contra = 83,
	Map_Slot = 521,
	Clan8 = 520,
	Weaponsmegapack = 187,
	Gift_6_Years = 307,
	Gift_9_May = 310,
	Default_Death = 255,
	None = 999,
	Weapons_Max,
	Vehicles_Max,
}

public enum PacketName : byte
{
	/// <summary>[Both] C2S: Client sends auth credentials. S2C: Server responds with auth status and map info.</summary>
	Auth = 0,
	/// <summary>[Both] C2S: Client sends its position and state. S2C: Server sends other players' positions.</summary>
	Position = 1,
	/// <summary>[C2S] Client sends a bonus/loh value.</summary>
	SendBonus = 2,
	/// <summary>[S2C] Server sends full data for another player.</summary>
	PlayerInfo = 2,
	/// <summary>[Both] C2S: Client requests its data. S2C: Server sends the client its own index and game data.</summary>
	MyInfo = 3,
	/// <summary>[Both] C2S: Client reports attacking a block. S2C: Server broadcasts the block attack effect.</summary>
	BlockAttack = 4,
	/// <summary>[Both] C2S: Client requests block info. S2C: Server sends info about a specific block.</summary>
	BlockInfo = 5,
	/// <summary>[S2C] Server sends a list of blocks that were destroyed and should become physics objects.</summary>
	BlockDestroy = 6,
	/// <summary>[Both] C2S: Client reports dealing damage. S2C: Server broadcasts a damage event to relevant clients.</summary>
	Damage = 7,
	/// <summary>[S2C] Server sends updated team scores and game timer.</summary>
	Scores = 8,
	/// <summary>[Both] C2S: Client sends its team/class choice. S2C: Server broadcasts the choice to other clients.</summary>
	JoinTeamClass = 9,
	/// <summary>[Both] C2S: Client requests to spawn with its last used weapon configuration. S2C: Server confirms a player has spawned.</summary>
	Spawn = 10,
	/// <summary>[Both] C2S: Client reports a shot that doesn't hit any block. S2C: Server broadcasts the attack event.</summary>
	AttackMilk = 11,
	/// <summary>[Both] C2S: Client requests to place a block. S2C: Server confirms a block has been placed.</summary>
	SetBlock = 12,
	/// <summary>[Both] C2S: Client sends a chat message. S2C: Server relays the chat message to other clients.</summary>
	Chat = 13,
	/// <summary>[S2C] Server sends player kill/death statistics updates.</summary>
	Stats = 14,
	/// <summary>[Both] C2S: Client informs the server of its newly selected weapon. S2C: Server informs clients of a player's weapon change.</summary>
	CurrentWeapon = 15,
	/// <summary>[Both] C2S: Client sends an error disconnect message. S2C: Server informs clients that a player has disconnected.</summary>
	Disconnect = 16,
	/// <summary>[S2C] Server instructs the client to reconnect to the server (e.g., map change).</summary>
	Reconnect = 17,
	/// <summary>[Both] C2S: Client sends a graceful disconnect message. S2C: Server sends an end-of-snapshot marker (handler is empty).</summary>
	EndOfSnap = 18,
	/// <summary>[C2S] Client informs server of the type of block it has selected for building.</summary>
	SelectBlock = 20,
	/// <summary>[S2C] Server tells the client to build a block at a specific location.</summary>
	BuildBlock = 21,
	/// <summary>[C2S] Client requests to save the map.</summary>
	SaveMap = 22,
	/// <summary>[S2C] Server sends detailed data about the local player.</summary>
	MyData = 22,
	/// <summary>[S2C] Server informs clients that a player's helmet was damaged or destroyed.</summary>
	DamageHelmet = 23,
	/// <summary>[Both] C2S: Client requests to create an entity (e.g., throw grenade). S2C: Server broadcasts the entity creation.</summary>
	CreateEntity = 24,
	/// <summary>[C2S] Client requests to detonate a placed entity (like C4).</summary>
	DetonateEntity = 25,
	/// <summary>[S2C] Server sends the status of a physics-based block destruction event.</summary>
	DestroyStatus = 26,
	/// <summary>[S2C] Server broadcasts an explosion effect at a specific location.</summary>
	Explode = 27,
	/// <summary>[S2C] Server provides information about the private room settings.</summary>
	PrivateInfo = 28,
	/// <summary>[C2S] Client sends new settings for its private room.</summary>
	PrivateSettings = 29,
	/// <summary>[S2C] Server instructs the client to reconnect after a short wait.</summary>
	Reconnect2 = 30,
	/// <summary>[Both] C2S: Client requests to spawn. S2C: Server notifies the client it is waiting for a respawn wave.</summary>
	ReadyForSpawn = 31,
	/// <summary>[S2C] Server sends the full equipment and state for the client to spawn with.</summary>
	SpawnEquip = 32,
	/// <summary>[C2S] Client sends a reload/prereload action.</summary>
	Reload = 33,
	/// <summary>[S2C] Server sends the countdown timer for Zombie Mode.</summary>
	ZombieCountdown = 33,
	/// <summary>[S2C] Server informs a client they have been infected in Zombie Mode.</summary>
	ZombieInfect = 34,
	/// <summary>[S2C] Server sends a message specific to Zombie Mode.</summary>
	ZombieMessage = 35,
	/// <summary>[S2C] Server sends the client's current health value.</summary>
	SetHealth = 36,
	/// <summary>[C2S] Client sends a command as the private room owner (e.g., kick player).</summary>
	PrivateCommand = 39,
	/// <summary>[S2C] Server sends a formatted system message (e.g., "Player A killed Player B").</summary>
	Message = 40,
	/// <summary>[C2S] Client reports using a medkit.</summary>
	WeaponMedkit = 41,
	/// <summary>[C2S] Client uses a TNT weapon.</summary>
	WeaponTNT = 42,
	/// <summary>[S2C] Server sends Capture the Flag radar data.</summary>
	CTRadar = 42,
	/// <summary>[S2C] Server informs the client its armor was destroyed.</summary>
	DamageArmor = 43,
	/// <summary>[S2C] Server commands a specific sound effect to be played for a player.</summary>
	SoundFx = 44,
	/// <summary>[S2C] Server forces the client's position to a new location.</summary>
	Reposition = 45,
	/// <summary>[S2C] Server sends an update for a moving entity's position.</summary>
	MoveEntity = 50,
	/// <summary>[S2C] Server commands the client to destroy an entity.</summary>
	DestroyEntity = 51,
	/// <summary>[S2C] Server sends a generic game message to be displayed (e.g., "Wave starting").</summary>
	GameMessage = 52,
	/// <summary>[S2C] Server sends the amount of blocks the player has for building.</summary>
	Equip = 53,
	/// <summary>[S2C] Server sends the position of a non-player entity.</summary>
	EntityPosition = 54,
	/// <summary>[S2C] Server commands a boss NPC to play a specific animation.</summary>
	MoveBoss = 55,
	/// <summary>[S2C] Server-side event, sent when an elevator the player is in moves up.</summary>
	LiftUp = 56,
	/// <summary>[S2C] Server sends additional player information, such as a Unique ID (UID).</summary>
	PlayerInfo2 = 57,
	/// <summary>[S2C] Server sends an update for a player's info, triggering a UI refresh.</summary>
	PlayerUpdate = 58,
	/// <summary>[C2S] Client requests to enter a vehicle/entity.</summary>
	EnterEntityRequest = 60,
	/// <summary>[S2C] Server confirms a player has entered an entity and broadcasts it.</summary>
	EnterEntity = 61,
	/// <summary>[S2C] Server confirms a player has exited an entity and broadcasts it.</summary>
	ExitEntity = 63,
	/// <summary>[C2S] Client requests to exit a vehicle/entity.</summary>
	ExitEntityRequest = 64,
	/// <summary>[C2S] Client sends its vehicle's turret and gun rotation to the server.</summary>
	VehicleTurretUpdate = 65,
	/// <summary>[C2S] Client requests to use a vehicle's special module.</summary>
	UseVehicleModule = 66,
	/// <summary>[S2C] Server sends vehicle turret rotation updates for other players.</summary>
	VehicleTurret = 66,
	/// <summary>[C2S] Client sends a targeting packet (e.g., for lock-on missiles).</summary>
	VehicleTargeting = 67,
	/// <summary>[S2C] Server sends vehicle health and armor updates.</summary>
	VehicleHealth = 67,
	/// <summary>[S2C] Server broadcasts that a vehicle has exploded.</summary>
	VehicleExplode = 68,
	/// <summary>[S2C] Server broadcasts that a vehicle has been hit.</summary>
	VehicleHit = 69,
	/// <summary>[S2C] Server informs the client they are being targeted by a lock-on weapon.</summary>
	VehicleTargetingInfo = 70,
	/// <summary>[S2C] Server sends an update on a non-player entity's health.</summary>
	EntityHealth = 71,
	/// <summary>[C2S] Client reports attacking a non-player entity.</summary>
	AttackEntity = 91,
	/// <summary>[C2S] Client requests to spawn their personal vehicle.</summary>
	SpawnMyVehicle = 92,
	/// <summary>[C2S] Client requests to detonate all of their placed C4 explosives.</summary>
	DetonateMyC4 = 93,
	/// <summary>[C2S] Client sends a new position for an entity it controls.</summary>
	NewEntityPosition = 94,
	/// <summary>[C2S] Client sends its new weapon configuration.</summary>
	NewConfig = 101,
	/// <summary>[S2C] Server sends compressed player position data.</summary>
	ZippedPlayerPosition = 101,
	/// <summary>[C2S] Client requests to auto-join a team with its last saved config.</summary>
	AutoJoinTeamClass = 102,
	/// <summary>[S2C] Server sends compressed entity position data.</summary>
	ZippedEntityPosition = 102,
	/// <summary>[S2C] Server sends a compressed chunk of map block data.</summary>
	ChunkData = 103,
	/// <summary>[S2C] Server signals that all map chunk data has been sent.</summary>
	ChunkFinish = 104,
	/// <summary>[S2C] Server sends special map data, like team spawn locations.</summary>
	MapData = 105,
	/// <summary>[S2C] Server sends the initial state and position of a capture flag.</summary>
	FlagSet = 107,
	/// <summary>[S2C] Server sends an update on a capture flag's state (e.g., who is carrying it).</summary>
	FlagUpdate = 108,
	/// <summary>[S2C] Server confirms that the client's weapon selection has been accepted.</summary>
	AcceptWeapons = 109,
	/// <summary>[S2C] Server informs clients of another player's selected building block.</summary>
	SelectedBlock = 110,
	/// <summary>[C2S] Client reports its mission status to the server.</summary>
	MissionStatus = 151,
	/// <summary>[C2S] Client sends a dummy/ping packet.</summary>
	Dummy = 200,
	/// <summary>[S2C] Server confirms it's ready for authentication.</summary>
	AuthReady = 200,
	/// <summary>[S2C] Server forces the client to disconnect and return to the main menu.</summary>
	AppDisconnect = 255,
}
