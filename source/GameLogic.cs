using System;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer;

public class GameLogic(IPacketSender packetSender, IGameModeQuery gameModeQuery, IGameScoreQuery gameScoreQuery, IGameScoreCommand gameScoreCommand, IPlayerQuery playersQuery, IWorldVoxelQuery worldVoxelQuery) : IPlayerLogic, IEntityLogic, IPlayerDisconnector, IChatSender
{
	public void SpawnPlayer(
		Player player,
		Vector3? at = null,
		int forWhom = byte.MaxValue
	)
	{
		if (!player.IsSpawned)
		{
			player.IsSpawned = true;
			player.Health = Player.MaxHealth;
			player.Position = gameModeQuery.GetGameMode() == GameMode.Build ? new Vector3I(128, 63, 128) : worldVoxelQuery.FindSpawnPoint(player.Team);
		}

		if (at is not null) player.Position = at.Value;
		at ??= player.Position;

		Packet spawnPacket = Packet.Create(PacketName.Spawn);

        spawnPacket.Write<byte>((byte)player.Index);
		spawnPacket.WriteVectorByte((Vector3I)at);

        spawnPacket.Write<byte>(0); // Head Slot (Pumpkin/Santa Hat)
        spawnPacket.Write<byte>(0); // Mask Slot (Animal Masks)
        spawnPacket.Write<byte>(0); // Back Slot (Horns)

		if (forWhom == byte.MaxValue)
			packetSender.BroadcastPacket(spawnPacket, player.Index);
		else
			packetSender.SendPacket(forWhom, spawnPacket);

		if (forWhom != byte.MaxValue) return;

		Packet respawnPacket = Packet.Create(PacketName.SpawnEquip);

		respawnPacket.Write<byte>((byte)player.Health);
		respawnPacket.Write<byte>((byte)(player.HasHelmet ? 1 : 0));
		respawnPacket.Write<byte>((byte)player.Armor);

		respawnPacket.WriteVectorByte((Vector3I)at);

		respawnPacket.Write<int>((int)player.WeaponMelee);
		respawnPacket.Write<int>((int)player.WeaponPrimary);
		respawnPacket.Write<int>((int)player.WeaponSecondary);
		respawnPacket.Write<int>((int)player.WeaponUtility1);
		respawnPacket.Write<int>((int)player.WeaponUtility2);
		respawnPacket.Write<int>((int)player.WeaponUtility3);
		respawnPacket.Write<int>((int)player.WeaponGrenade1);
		respawnPacket.Write<int>((int)player.WeaponGrenade2);
		respawnPacket.Write<int>(ItemsDB.GetClipSize(player.WeaponPrimary));		// ClipAmmo (clip primary ammo) (actually sets the max clip ammo aswell)
		respawnPacket.Write<int>(ItemsDB.GetReserveSize(player.WeaponPrimary));		// Backpack (reserve primary ammo)
		respawnPacket.Write<int>(ItemsDB.GetClipSize(player.WeaponSecondary));		// ClipAmmo2 (clip secondary ammo) (^ same here)
		respawnPacket.Write<int>(ItemsDB.GetReserveSize(player.WeaponSecondary));	// Backpack2 (reserve secondary ammo)
		respawnPacket.Write<int>(gameModeQuery.GetGameMode() == GameMode.Build ? 0 : 200);	// BlockAmmo (amount of blocks available)
		respawnPacket.Write<int>(gameScoreQuery.GetRemainingTime());
		respawnPacket.Write<int>(player.WeaponUtility1 == ItemName.Mortar ? 6 : (player.WeaponUtility1 == ItemName.Shmel ? 1 : 4));
		respawnPacket.Write<int>(player.AmmoUtility2);
		respawnPacket.Write<int>(player.AmmoUtility3);
		respawnPacket.Write<int>(player.AmmoGrenade1);
		respawnPacket.Write<int>(player.AmmoGrenade2);

		respawnPacket.Write<byte>(0);	// Medkit_W small ?
		respawnPacket.Write<byte>(0);	// Medkit_G medium ?
		respawnPacket.Write<byte>(0);	// Medkit_O large ?

		// ZBK18M to FLASH
		for (int i = 0; i < 10; i++)
			respawnPacket.Write<byte>(byte.MaxValue); // TODO

		respawnPacket.Write<byte>(0); // SMOKE ?

		packetSender.SendPacket(player.Index, respawnPacket);
	}

	public async Task DisconnectPlayer(Player player, bool informVictim = true)
	{
		if (informVictim)
		{
			packetSender.SendPacket(player, PacketName.AppDisconnect);
			await Task.Delay(300);
		}

		player.Connected = false;
		Packet disconnectPacket = Packet.Create(PacketName.Disconnect);
		disconnectPacket.Write<byte>((byte)player.Index);
		packetSender.BroadcastPacket(disconnectPacket);
	}

	public async Task DisconnectEveryone()
	{
		packetSender.BroadcastPacket(PacketName.AppDisconnect);
		await Task.Delay(300);
		foreach (Player player in playersQuery.GetPlayers())
			player.Connected = false;
	}

	public async Task ReconnectEveryone()
	{
		packetSender.BroadcastPacket(PacketName.Reconnect2);
		await Task.Delay(300);
		foreach (Player player in playersQuery.GetPlayers())
			player.Connected = false;
	}

	public bool DamageZombie(int zombieEntityIndex, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine)
	{
		if (gameModeQuery.GetGameMode() != GameMode.Survival) return false;

		// TODO

		return true;
	}

	public bool DamagePlayer(Player victim, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine, bool allowRespawn = true)
	{
		if ((gameModeQuery.GetGameMode() == GameMode.Build) || (gameModeQuery.GetGameMode() == GameMode.Survival)) return false;
		if (!victim.IsAlive) return true;
		if (attacker == victim) attacker = null;
		if ((attacker is not null) && (attacker.Team == victim.Team)) return false;

		victim.Health = Math.Max(0, victim.Health - amount);

		if (!victim.IsAlive)
		{
			// the victim is now dead

			victim.IsSpawned = false;
			victim.Deaths++;

			if (attacker is not null)
			{
				attacker.Kills++;

				Packet statsPacket = Packet.Create(PacketName.Stats);
				statsPacket.Write<byte>((byte)attacker.Index);
				statsPacket.Write<int>(attacker.Kills);
				statsPacket.Write<int>(attacker.Deaths);
				statsPacket.Write<byte>((byte)victim.Index);
				statsPacket.Write<int>(victim.Kills);
				statsPacket.Write<int>(victim.Deaths);
				packetSender.BroadcastPacket(statsPacket);

				if (gameModeQuery.GetGameMode() == GameMode.Classic)
					gameScoreCommand.AddTeamScore(attacker.Team, 1);
			}

			if (allowRespawn)
			{
				_ = Task.Run(async () =>
				{
					// respawn them after 5 seconds
					await Task.Delay(5000);
					SpawnPlayer(victim);
				});
			}
		}

		Packet damagePacket = Packet.Create(PacketName.Damage);
		damagePacket.Write<byte>((byte)(attacker is not null ? attacker.Index : victim.Index));
		damagePacket.Write<byte>((byte)victim.Index);
		damagePacket.Write<int>(victim.Health);
		damagePacket.Write<int>((int)weaponId);
		damagePacket.Write<byte>((byte)hitbox);
		packetSender.BroadcastPacket(damagePacket);

		return !victim.IsAlive;
	}

	public void SendChatMessage(Player? toPlayer, Player? byPlayer, bool teamSay, string message)
	{
		if ((toPlayer is null) && (byPlayer is null))
			throw new ArgumentNullException(nameof(byPlayer));

		Packet chatPacket = Packet.Create(PacketName.Chat);
		chatPacket.Write<byte>((byte)(byPlayer is null ? toPlayer!.Index : byPlayer.Index));
		chatPacket.Write<byte>((byte)(byPlayer is null ? toPlayer!.Team : byPlayer.Team));
		chatPacket.Write<byte>((byte)(teamSay ? 1 : 0));
		chatPacket.WriteString(message);

		if (toPlayer is null)
			packetSender.BroadcastPacket(chatPacket);
		else
			packetSender.SendPacket(toPlayer, chatPacket);
	}
}
