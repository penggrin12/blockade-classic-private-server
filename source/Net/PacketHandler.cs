using System;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;
using BlockadeClassicPrivateServer.Voxel;
using Godot;
using Ionic.Zlib;

namespace BlockadeClassicPrivateServer.Net;

public static class PacketHandler
{
	public static event Action<Player, string[]>? CommandReceived;

	public static async Task RecvEndOfSnap(IPlayerDisconnector playerDisconnector, Player player, Packet recvPacket)
	{
		await playerDisconnector.DisconnectPlayer(player, false);
	}

	public static void RecvAuth(IPacketSender packetSender, IGameModeQuery gameModeQuery, IMapIdQuery mapIdQuery, Player player, Packet recvPacket)
	{
		player.Country = (Flag)recvPacket.Read<byte>();
		player.Network = (Network)recvPacket.Read<byte>();
		// the rest is:
		//  (int)     server password
		//  (string)  PlayerProfile.id
		//  (string)  PlayerProfile.authKey
		//  (string)  PlayerProfile.session
		//  (string)  PlayerProfile.gameSession

		Packet authPacket = Packet.Create(PacketName.Auth);
		authPacket.Write<byte>((byte)gameModeQuery.GetGameMode()); // game mode
		authPacket.WriteString(mapIdQuery.GetMapId()); // was originally: "1" (citadel)
		packetSender.SendPacket(player.Index, authPacket);

		// MapData (radar data im assuming)
		// Packet mapDataPacket = Packet.Create(PacketName.MapData);
		// for (var i = 0; i < 64; i++)
		// {
		// 	mapDataPacket.WriteVectorByte(new Vector3I(15, 50, 15));
		// }
		// server.SendPacket(player.Index, mapDataPacket);
	}

	public static async Task RecvBlockInfo(IPacketSender packetSender, IWorldVoxelQuery worldVoxelQuery, Player player, Packet recvPacket)
	{
		foreach (ChunkData chunkData in worldVoxelQuery.FindChangedChunks())
		{
			// ! FIXME: DotNetZip is VERY outdated.. but the client uses it... so..
			byte[] compressedChunkData = ZlibStream.CompressBuffer(chunkData.BlockTypes);

			Packet chunkDataPacket = Packet.Create(PacketName.ChunkData);
			chunkDataPacket.WriteVectorByte(chunkData.Position);
			chunkDataPacket.WriteBytes(compressedChunkData);
			packetSender.SendPacket(player.Index, chunkDataPacket);
		}

		await Task.Delay(300);

		packetSender.SendPacket(player.Index, Packet.Create(PacketName.ChunkFinish));
	}

	public static async Task RecvMyInfo(IPacketSender packetSender, IPlayerLogic playerLogic, IPlayerQuery playerQuery, IGameModeQuery gameModeQuery, Player player, Packet recvPacket)
	{
		Packet myInfoPacket = Packet.Create(PacketName.MyInfo);

		// player index
		myInfoPacket.Write<byte>((byte)player.Index);

		// radar team spawn points
		for (var i = 0; i < 4; i++)
			myInfoPacket.WriteVectorByte(Vector3I.Zero);

		myInfoPacket.WriteVectorByte(Vector3I.Zero); // radar base_pos ?

		// map.mlx & map.mly & map.mlz ?
		// Map Limits maybe?
		myInfoPacket.WriteVectorByte(new Vector2I(0, 255));
		myInfoPacket.WriteVectorByte(new Vector2I(0, 63));
		myInfoPacket.WriteVectorByte(new Vector2I(0, 255));

		packetSender.SendPacket(player.Index, myInfoPacket);

		await Task.Delay(200);

		Packet myDataPacket = Packet.Create(PacketName.MyData);
		// 1. Player Index (byte)
		myDataPacket.Write<byte>((byte)player.Index);

		// 2. Player Name (length-prefixed string)
		myDataPacket.WriteString(player.Name);

		// 3. Clan Name (length-prefixed string)
		myDataPacket.WriteString("");
		// The client code for recv_my_data has a quirk: it reads the clan name and then
		// advances the read position by (length + 1). This implies it's expecting a
		// null terminator after the string, even though it's length-prefixed.
		// We must add this byte to keep the client's parser in sync.
		myDataPacket.Write<byte>(0);

		// 4. Country ID (byte)
		myDataPacket.Write<byte>((byte)player.Country);

		// 5. Helmet (byte)
		myDataPacket.Write<byte>(1);

		// 6. Item[54] - Armor (byte)
		myDataPacket.Write<byte>(1);

		// 7. Item[146] - Helmet 2 (byte)
		myDataPacket.Write<byte>(0);

		// 8. Item[147] - Armor 2 (byte)
		myDataPacket.Write<byte>(0);

		// 9. Skin ID (int)
		myDataPacket.Write<int>((int)ItemName.Skin_Default); // Default skin

		// 10. Znak (Badge) ID (int)
		myDataPacket.Write<int>((int)ItemName.None);

		// --- The Great Item Spam ---
		// The rest of the packet is a massive list of every single item the player owns.

		myDataPacket.Write<byte>(1); // Item[6]
		myDataPacket.Write<byte>(1); // Item[2]
		myDataPacket.Write<byte>(1); // Item[3]
		myDataPacket.Write<byte>(1); // Item[9]
		myDataPacket.Write<byte>(1); // Item[12]
		myDataPacket.Write<byte>(1); // Item[13]
		myDataPacket.Write<byte>(1); // Item[14]
		myDataPacket.Write<byte>(1); // Item[15]
		myDataPacket.Write<byte>(1); // Item[16]
		myDataPacket.Write<byte>(1); // Item[17]
		myDataPacket.Write<byte>(1); // Item[18]
		myDataPacket.Write<byte>(1); // Item[19]
		myDataPacket.Write<byte>(1); // Item[40]
		myDataPacket.Write<byte>(1); // Item[47]
		myDataPacket.Write<byte>(1); // Item[48]
		myDataPacket.Write<byte>(1); // Item[49]
		myDataPacket.Write<byte>(1); // Item[50]
		myDataPacket.Write<byte>(1); // Item[51]
		myDataPacket.Write<byte>(1); // Item[52]
		myDataPacket.Write<byte>(1); // Item[53]
		myDataPacket.Write<byte>(1); // Item[60]
		myDataPacket.Write<byte>(1); // Item[61]
		myDataPacket.Write<byte>(1); // Item[68]
		myDataPacket.Write<byte>(1); // Item[69]
		myDataPacket.Write<byte>(1); // Item[70]
		myDataPacket.Write<byte>(1); // Item[71]
		myDataPacket.Write<byte>(1); // Item[72]
		myDataPacket.Write<byte>(1); // Item[73]
		myDataPacket.Write<byte>(1); // Item[74]
		myDataPacket.Write<int>(1); // Item[77]
		myDataPacket.Write<byte>(1); // Item[78]
		myDataPacket.Write<byte>(1); // Item[79]
		myDataPacket.Write<byte>(1); // Item[80]
		myDataPacket.Write<byte>(1); // Item[81]
		myDataPacket.Write<byte>(1); // Item[82]
		myDataPacket.Write<byte>(1); // Item[89]
		myDataPacket.Write<byte>(1); // Item[90]
		myDataPacket.Write<byte>(1); // Item[91]
		myDataPacket.Write<byte>(1); // Item[34]
		myDataPacket.Write<byte>(1); // Item[93]
		myDataPacket.Write<byte>(1); // Item[94]
		myDataPacket.Write<byte>(1); // Item[95]
		myDataPacket.Write<byte>(1); // Item[96]
		myDataPacket.Write<byte>(1); // Item[101]
		myDataPacket.Write<byte>(1); // Item[102]
		myDataPacket.Write<byte>(1); // Item[103]
		myDataPacket.Write<byte>(1); // Item[104]
		myDataPacket.Write<byte>(1); // Item[105]
		myDataPacket.Write<byte>(1); // Item[106]
		myDataPacket.Write<byte>(1); // Item[107]
		myDataPacket.Write<byte>(1); // Item[108]
		myDataPacket.Write<byte>(1); // Item[109]
		myDataPacket.Write<byte>(1); // Item[110]
		myDataPacket.Write<byte>(1); // Item[111]
		myDataPacket.Write<byte>(1); // Item[112]
		myDataPacket.Write<int>(1); // Item[62]
		myDataPacket.Write<byte>(1); // Item[137]
		myDataPacket.Write<int>(1); // Item[138]
		myDataPacket.Write<int>(1); // Item[10]
		myDataPacket.Write<int>(1); // Item[100]
		myDataPacket.Write<int>(1); // Item[55]
		myDataPacket.Write<int>(1); // Item[7]
		myDataPacket.Write<byte>(1); // Item[139]
		myDataPacket.Write<byte>(1); // Item[140]
		myDataPacket.Write<byte>(1); // Item[141]
		myDataPacket.Write<byte>(1); // Item[142]
		myDataPacket.Write<byte>(1); // Item[143]
		myDataPacket.Write<byte>(1); // Item[144]
		myDataPacket.Write<byte>(1); // Item[145]
		myDataPacket.Write<int>(1); // Item[169]
		myDataPacket.Write<int>(1); // Item[168]
		myDataPacket.Write<int>(1); // Item[170]
		myDataPacket.Write<byte>(1); // Item[161]
		myDataPacket.Write<byte>(1); // Item[162]
		myDataPacket.Write<byte>(1); // Item[160]
		myDataPacket.Write<byte>(1); // Item[159]
		myDataPacket.Write<byte>(1); // Item[157]
		myDataPacket.Write<byte>(1); // Item[158]
		myDataPacket.Write<int>(1); // Item[171]
		myDataPacket.Write<int>(1); // Item[172]
		myDataPacket.Write<byte>(1); // Item[173]
		myDataPacket.Write<byte>(1); // Item[174]
		myDataPacket.Write<byte>(1); // Item[175]
		myDataPacket.Write<byte>(1); // Item[176]
		myDataPacket.Write<byte>(1); // Item[177]
		myDataPacket.Write<int>(1); // Item[183]
		myDataPacket.Write<int>(1); // Item[184]
		myDataPacket.Write<int>(1); // Item[185]
		myDataPacket.Write<int>(1); // Item[186]
		myDataPacket.Write<byte>(1); // Item[188]
		myDataPacket.Write<byte>(1); // Item[189]
		myDataPacket.Write<byte>(1); // Item[190]
		myDataPacket.Write<byte>(1); // Item[191]
		myDataPacket.Write<byte>(1); // Item[192]
		myDataPacket.Write<byte>(1); // Item[193]
		myDataPacket.Write<byte>(1); // Item[201]
		myDataPacket.Write<byte>(1); // Item[202]
		myDataPacket.Write<byte>(1); // Item[203]
		myDataPacket.Write<byte>(1); // Item[204]
		myDataPacket.Write<byte>(1); // Item[136]
		myDataPacket.Write<int>(1); // Item[135]
		myDataPacket.Write<int>(1); // Item[205]
		myDataPacket.Write<int>(1); // Item[206]
		myDataPacket.Write<int>(1); // Item[198]
		myDataPacket.Write<byte>(1); // Item[207]
		myDataPacket.Write<byte>(1); // Item[208]
		myDataPacket.Write<byte>(1); // Item[209]
		myDataPacket.Write<byte>(1); // Item[210]
		myDataPacket.Write<int>(1); // Item[211]
		myDataPacket.Write<byte>(1); // Item[218]
		myDataPacket.Write<byte>(1); // Item[219]
		myDataPacket.Write<byte>(1); // Item[220]
		myDataPacket.Write<byte>(1); // Item[221]
		myDataPacket.Write<byte>(1); // Item[222]
		myDataPacket.Write<byte>(1); // Item[223]
		myDataPacket.Write<byte>(1); // Item[224]
		myDataPacket.Write<byte>(1); // Item[225]
		myDataPacket.Write<byte>(1); // Item[226]
		myDataPacket.Write<byte>(1); // Item[301]
		myDataPacket.Write<byte>(1); // Item[302]
		myDataPacket.Write<byte>(1); // Item[303]
		myDataPacket.Write<byte>(1); // Item[304]
		myDataPacket.Write<byte>(1); // Item[305]
		myDataPacket.Write<byte>(1); // Item[308]
		myDataPacket.Write<byte>(1); // Item[309]
		myDataPacket.Write<byte>(1); // Item[313]
		myDataPacket.Write<byte>(1); // Item[314]
		myDataPacket.Write<byte>(1); // Item[315]
		myDataPacket.Write<byte>(1); // Item[329]
		myDataPacket.Write<byte>(1); // Item[330]
		myDataPacket.Write<byte>(1); // Item[331]
		myDataPacket.Write<byte>(1); // Item[332]
		myDataPacket.Write<byte>(1); // Item[333]
		myDataPacket.Write<byte>(1); // Item[39]
		myDataPacket.Write<byte>(1); // Item[154]
		myDataPacket.Write<byte>(1); // Item[155]
		myDataPacket.Write<byte>(1); // Item[92]
		myDataPacket.Write<int>(1); // Item[156]
		packetSender.SendPacket(player.Index, myDataPacket);

		await Task.Delay(200);
		// server.EmitSignal(Server.SignalName.PlayerLoggedIn, player.Index);

		static Packet PlayerInfoGenerator(Player player)
		{
			Packet playerInfoPacket = Packet.Create(PacketName.PlayerInfo);

			// unused
			playerInfoPacket.Write<byte>(0);

			playerInfoPacket.Write<byte>((byte)player.Index);
			playerInfoPacket.WriteString(player.Name);
			playerInfoPacket.WriteString(player.Clan);
			// clan expects length AND null terminator
			playerInfoPacket.Write<byte>(0);

			// is dead? (0 = alive, 1 = dead)
			playerInfoPacket.Write<byte>(player.IsAlive ? (byte)0 : (byte)1);

			playerInfoPacket.Write<byte>((byte)player.Team);

			playerInfoPacket.Write<int>(player.Kills);
			playerInfoPacket.Write<int>(player.Deaths);

			playerInfoPacket.Write<byte>((byte)player.Country);
			playerInfoPacket.Write<byte>(1); // Helmet?
			playerInfoPacket.Write<byte>(1); // Vest?
			playerInfoPacket.Write<byte>(0); // HelmetPlus?
			playerInfoPacket.Write<byte>(0); // VestPlus?

			playerInfoPacket.Write<int>((int)ItemName.Skin_Default);
			playerInfoPacket.Write<int>((int)ItemName.None); // Znak

			playerInfoPacket.Write<byte>(0); // Item.Premium?
			playerInfoPacket.Write<byte>(0); // Item.Tank_Mg?
			playerInfoPacket.Write<int>(0);	// Item.Armored_Block?
			playerInfoPacket.Write<int>(0);  // Item.Tykva?
			playerInfoPacket.Write<byte>(0); // Item.Kolpak?
			playerInfoPacket.Write<byte>(0); // Item.Roga
			playerInfoPacket.Write<byte>(0); // Item.Mask_Bear
			playerInfoPacket.Write<byte>(0); // Item.Mask_Fox
			playerInfoPacket.Write<byte>(0); // Item.Mask_Rabbit

			return playerInfoPacket;
		}

		foreach (Player otherPlayer in playerQuery.GetPlayers())
		{
			if (otherPlayer.Index == player.Index) continue;

			packetSender.SendPacket(player, PlayerInfoGenerator(otherPlayer));

			if (otherPlayer.IsSpawned)
				playerLogic.SpawnPlayer(otherPlayer, forWhom: player.Index);
		}

		packetSender.BroadcastPacket(PlayerInfoGenerator(player), player.Index);

		if (gameModeQuery.GetGameMode() == GameMode.Build)
			playerLogic.SpawnPlayer(player);
	}

	public static void RecvPosition(IPacketSender packetSender, Player player, Packet recvPacket)
	{
		if ((!player.IsSpawned) || (!player.IsAlive)) return;

		player.Position = new Vector3(recvPacket.Read<float>(), recvPacket.Read<float>(), recvPacket.Read<float>());
		player.Angle = new Vector2(recvPacket.Read<float>(), recvPacket.Read<float>());
		player.MovementState = (PlayerMovementState)recvPacket.Read<byte>();

		// loh
		// TODO: find out what it does
		recvPacket.Read<byte>();

		Packet positionPacket = Packet.Create(PacketName.Position);
		positionPacket.Write<byte>((byte)player.Index);

		positionPacket.WriteVectorUShort(player.Position);

		positionPacket.Write<byte>((byte)(player.Angle.X * 256f / 360f));
		positionPacket.Write<byte>((byte)(player.Angle.Y * 256f / 360f));
		positionPacket.Write<byte>((byte)player.MovementState);

		packetSender.BroadcastPacket(positionPacket);
	}

	public static void RecvJoinTeamClass(IPacketSender packetSender, IPlayerLogic playerLogic, Player player, Packet recvPacket)
	{
		player.Team = (Team)recvPacket.Read<byte>();

		// TODO
		if (player.IsSpawned)
			playerLogic.DamagePlayer(player, null, Player.MaxHealth, ItemName.Default_Death, allowRespawn: false);

		Packet joinTeamClassPacket = Packet.Create(PacketName.JoinTeamClass);
		joinTeamClassPacket.Write<byte>((byte)player.Index);
		joinTeamClassPacket.Write<byte>((byte)player.Team);
		packetSender.BroadcastPacket(joinTeamClassPacket);
	}

	public static void RecvNewConfig(IPacketSender packetSender, IPlayerQuery playerQuery, IPlayerLogic gameLogic, Player player, Packet recvPacket)
	{
		player.WeaponMelee = (ItemName)recvPacket.Read<int>();
		player.WeaponPrimary = (ItemName)recvPacket.Read<int>();
		player.WeaponSecondary = (ItemName)recvPacket.Read<int>();

		player.WeaponUtility1 = (ItemName)recvPacket.Read<int>();
		player.WeaponUtility2 = (ItemName)recvPacket.Read<int>();
		player.WeaponUtility3 = (ItemName)recvPacket.Read<int>();

		player.WeaponGrenade1 = (ItemName)recvPacket.Read<int>();
		player.WeaponGrenade2 = (ItemName)recvPacket.Read<int>();

		gameLogic.SpawnPlayer(player);

		// make sure the player knows about everyone else's gun
		foreach (Player otherPlayer in playerQuery.GetPlayers())
		{
			Packet currentWeaponPacket = Packet.Create(PacketName.CurrentWeapon);
			currentWeaponPacket.Write<byte>((byte)otherPlayer.Index);
			currentWeaponPacket.Write<int>((int)otherPlayer.CurrentWeapon);
			packetSender.SendPacket(player, currentWeaponPacket);
		}
	}

	public static void RecvBlockAttack(IPacketSender packetSender, IWorldVoxelCommand worldVoxelCommand, Player player, Packet recvPacket)
	{
		Vector3I at = (Vector3I)recvPacket.ReadVector3Int();
		ItemName weaponId = (ItemName)recvPacket.Read<int>();

		float fValue = recvPacket.Read<float>();
		Vector3 x = recvPacket.ReadVector3Float();
		Vector3 y = recvPacket.ReadVector3Float();
		byte loh = recvPacket.Read<byte>();

		// Packet explodePacket = Packet.Create(PacketName.Explode);
		// explodePacket.WriteVectorUShort(at);
		// server.BroadcastPacket(explodePacket);

		bool shouldInstantlyDestroy = ItemsDB.Items[(int)weaponId].Category == ItemCategory.Melee;
		int blockHp = worldVoxelCommand.DamageBlock(at, shouldInstantlyDestroy ? Block.MaxHealth : ItemsDB.Items[(int)weaponId].Upgrades[1][0].Value);

		Packet blockAttackPacket = Packet.Create(PacketName.BlockAttack);
		blockAttackPacket.WriteVectorByte(at);
		blockAttackPacket.Write<byte>((byte)blockHp);
		packetSender.BroadcastPacket(blockAttackPacket);

		// this is not how ur supposed to destroy singular blocks... but it looks cool

		// Packet blockDestroyPacket = Packet.Create(PacketName.BlockDestroy);
		// blockDestroyPacket.WriteVectorByte(at);
		// server.BroadcastPacket(blockDestroyPacket);

		// Packet destroyStatusPacket = Packet.Create(PacketName.DestroyStatus);
		// destroyStatusPacket.Write<byte>(0); // 0 = apply, 1 = clear
		// server.BroadcastPacket(destroyStatusPacket);

		// destroyStatusPacket = Packet.Create(PacketName.DestroyStatus);
		// destroyStatusPacket.Write<byte>(1); // 0 = apply, 1 = clear
		// server.BroadcastPacket(destroyStatusPacket);
	}

	public static void RecvSetBlock(IPacketSender packetSender, IWorldVoxelCommand worldVoxelCommand, Player player, Packet recvPacket)
	{
		Vector3I at = recvPacket.ReadVector3Byte();

		// Packet explodePacket = Packet.Create(PacketName.Explode);
		// explodePacket.WriteVector(at);
		// server.BroadcastPacket(explodePacket);

		worldVoxelCommand.SetBlock(at, (BlockType)((byte)BlockType.Brick_Blue + (byte)player.Team));

		// broadcast setblock back
		Packet setBlockPacket = Packet.Create(PacketName.SetBlock);
		setBlockPacket.WriteVectorByte(at);
		setBlockPacket.Write<byte>((byte)player.Team); // team
		packetSender.BroadcastPacket(setBlockPacket);
	}

	public static void RecvSelectBlock(IPacketSender packetSender, Player player, Packet recvPacket)
	{
		byte blockId = recvPacket.Read<byte>();

		Packet selectedBlockPacket = Packet.Create(PacketName.SelectedBlock);
		selectedBlockPacket.Write<byte>((byte)player.Index); // player index
		selectedBlockPacket.Write<byte>((byte)blockId); // block id
		packetSender.BroadcastPacket(selectedBlockPacket);
	}

	public static void RecvCurrentWeapon(IPacketSender packetSender, Player player, Packet recvPacket)
	{
		ItemName weaponId = (ItemName)recvPacket.Read<int>();

		player.CurrentWeapon = weaponId;

		Packet currentWeaponPacket = Packet.Create(PacketName.CurrentWeapon);
		currentWeaponPacket.Write<byte>((byte)player.Index); // player index
		currentWeaponPacket.Write<int>((int)weaponId); // weapon id
		packetSender.BroadcastPacket(currentWeaponPacket);
	}

	public static void RecvDamage(IGameModeQuery gameModeQuery, IPlayerLogic playerLogic, IEntityLogic entityLogic, IPlayerQuery playerQuery, Player player, Packet recvPacket)
	{
		ItemName weaponId = (ItemName)recvPacket.Read<int>();
		int victimIndex = recvPacket.Read<int>();
		BodyPart hitbox = (BodyPart)recvPacket.Read<byte>();

		float fValue = recvPacket.Read<float>();
		Vector3 a = recvPacket.ReadVector3Float(); // attacker's weapon system pos
		Vector3 b = recvPacket.ReadVector3Float(); // raycast hit's pos
		Vector3 c = recvPacket.ReadVector3Float(); // attacker's camera pos
		Vector3 d = recvPacket.ReadVector3Float(); // actual raycast hit pos ?
		byte loh = recvPacket.Read<byte>();

		int damage = ItemsDB.GetDamage(weaponId);
		bool hitShield = hitbox == BodyPart.Shield;

		if (hitbox != BodyPart.Head)
			damage = (int)(damage * 0.5f);

		if (hitShield)
			damage = 0;

		if (gameModeQuery.GetGameMode() == GameMode.Survival)
		{
			entityLogic.DamageZombie(victimIndex, player, damage, weaponId, hitbox);
		}
		else
		{
			Player? victim = playerQuery.GetPlayer(recvPacket.Read<int>());
			if (victim is null)
			{
				GD.Print($"[ NET ] {player} tried damaging an invalid victim");
				return;
			}

			playerLogic.DamagePlayer(victim, player, damage, weaponId, hitbox);
		}
	}

	public static void RecvChat(IChatSender chatSender, Player player, Packet recvPacket)
	{
		bool teamSay = recvPacket.Read<byte>() == 1;
		string text = recvPacket.ReadString();

		if (text[0] == '/')
		{
			// await ServerCommandHandling.HandleCommand(player, text[1..].Split(' '));
			CommandReceived?.Invoke(player, text[1..].Split(' '));
			return;
		}

		GD.Print($"[CHAT{(teamSay ? $" TEAM: {player.Team}" : "")}] <{player.Name}>: {text}");
		chatSender.SendChatMessage(null, player, teamSay, text);
	}

	public static void RecvCreateEntity(IEntityCommand entityCommand, Player player, Packet recvPacket)
	{
		// TODO: players that joined after this wont see them

		entityCommand.CreateEntity(
			(EntityName)recvPacket.Read<byte>(),
			player.Index,

			recvPacket.ReadVector3Float(),
			recvPacket.ReadVector3Float(),
			recvPacket.ReadVector3Float(),
			recvPacket.ReadVector3Float()
		);
	}

	public static void RecvDetonateEntity(IEntityQuery entityQuery, Player player, Packet recvPacket)
	{
		int entityIndex = recvPacket.Read<int>();
		Vector3 at = recvPacket.ReadVector3Float();

		Entity? entity = entityQuery.GetEntity(entityIndex);

		if (entity is null) return;
		if (player.Index != entity.OwnerPlayerIndex) return; // TODO: flag player

		entity.Position = at;
		entity.OnExplode();
	}

	public static void RecvNewEntityPosition(IPacketSender packetSender, IEntityQuery entityQuery, Player player, Packet recvPacket)
	{
		// TODO: this can sometimes cause a zombie to spawn

		int entityIndex = recvPacket.Read<int>();
		Vector3 at = recvPacket.ReadVector3Float();

		Entity? entity = entityQuery.GetEntity(entityIndex);

		if (entity is null) return;
		if (player.Index != entity.OwnerPlayerIndex) return; // TODO: flag player

		entity.Position = at;

		Packet entityPositionPacket = Packet.Create(PacketName.EntityPosition);
		entityPositionPacket.Write<byte>((byte)entity.OwnerPlayerIndex);
		entityPositionPacket.Write<int>(entity.Index);
		entityPositionPacket.Write<byte>((byte)entity.Name);
		entityPositionPacket.WriteVectorUShort(at);
		packetSender.BroadcastPacket(entityPositionPacket);
	}

	public static void RecvDetonateMyC4(IEntityQuery entityQuery, Player player, Packet recvPacket)
	{
		foreach (Entity entity in entityQuery.GetEntities())
		{
			if (entity.OwnerPlayerIndex != player.Index) continue;
			if (entity.Name != EntityName.C4) continue;

			entity.OnExplode();
		}
	}

	public static async Task HandlePacket(
		Player player,
		Packet recvPacket,
		IPlayerDisconnector playerDisconnector,
		IPlayerLogic playerLogic,
		IEntityLogic entityLogic,
		IChatSender chatSender,
		IGameModeQuery gameModeQuery,
		IMapIdQuery mapIdQuery,
		IPacketSender packetSender,
		IEntityQuery entityQuery,
		IEntityCommand entityCommand,
		IPlayerQuery playerQuery,
		IWorldVoxelQuery worldVoxelQuery,
		IWorldVoxelCommand worldVoxelCommand
	)
	{
		if (recvPacket.Name != PacketName.Disconnect)
			recvPacket.Read<int>(); // time

		// TODO
		// PacketName.AttackMilk: player shot but hit nothing
		// "Physics"

		switch (recvPacket.Name)
		{
			case PacketName.EndOfSnap: // player disconnects
				await RecvEndOfSnap(playerDisconnector, player, recvPacket);
				break;
			case PacketName.Auth:          // #0 - step 1
				RecvAuth(packetSender, gameModeQuery, mapIdQuery, player, recvPacket);
				break;
			case PacketName.BlockInfo:     // #5 - step 2
										   // send changes
				await RecvBlockInfo(packetSender, worldVoxelQuery, player, recvPacket);
				break;
			case PacketName.MyInfo:        // #3 - step 3
										   // send their profile (MyData)
				await RecvMyInfo(packetSender, playerLogic, playerQuery, gameModeQuery, player, recvPacket);
				break;
			case PacketName.JoinTeamClass: // #9 - optional step 4
				RecvJoinTeamClass(packetSender, playerLogic, player, recvPacket);
				break;
			case PacketName.Position:
				RecvPosition(packetSender, player, recvPacket);
				break;
			case PacketName.NewConfig:
				RecvNewConfig(packetSender, playerQuery, playerLogic, player, recvPacket);
				break;
			case PacketName.BlockAttack:
				RecvBlockAttack(packetSender, worldVoxelCommand, player, recvPacket);
				break;
			case PacketName.SetBlock:
				RecvSetBlock(packetSender, worldVoxelCommand, player, recvPacket);
				break;
			case PacketName.SelectBlock:
				RecvSelectBlock(packetSender, player, recvPacket);
				break;
			case PacketName.CurrentWeapon:
				RecvCurrentWeapon(packetSender, player, recvPacket);
				break;
			case PacketName.Damage:
				RecvDamage(gameModeQuery, playerLogic, entityLogic, playerQuery, player, recvPacket);
				break;
			case PacketName.Chat:
				RecvChat(chatSender, player, recvPacket);
				break;
			case PacketName.CreateEntity:
				RecvCreateEntity(entityCommand, player, recvPacket);
				break;
			case PacketName.DetonateEntity:
				RecvDetonateEntity(entityQuery, player, recvPacket);
				break;
			case PacketName.NewEntityPosition:
				RecvNewEntityPosition(packetSender, entityQuery, player, recvPacket);
				break;
			case PacketName.DetonateMyC4:
				RecvDetonateMyC4(entityQuery, player, recvPacket);
				break;
			default:
				GD.PrintRich($"[ NET ] [color=#eeee11]Unhandled packet: {recvPacket}[/color]");
				break;
		}
	}
}
