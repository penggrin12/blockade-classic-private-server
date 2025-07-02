using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer;

public static class CommandHandler
{
	private static void TeleportCommand(IPlayerLogic playerLogic, ref string? responseText, Player player, string[] parameters)
	{
		playerLogic.SpawnPlayer(player, new Vector3(byte.Parse(parameters[0]), byte.Parse(parameters[1]), byte.Parse(parameters[2])));
	}

	private static void RespawnCommand(IPlayerLogic playerLogic, ref string? responseText, Player player, string[] parameters)
	{
		playerLogic.SpawnPlayer(player, new Vector3(0, 63, 0));
	}

	private static void KillCommand(IPlayerQuery playerQuery, IPlayerLogic playerLogic, ref string? responseText, Player player, string[] parameters)
	{
		int victimIndex = player.Index;
		if (parameters.Length > 0) _ = int.TryParse(parameters[0], out victimIndex);
		Player? victim = playerQuery.GetPlayer(victimIndex);
		if (victim is null)
		{
			responseText = "Unknown player.";
			return;
		}
		playerLogic.DamagePlayer(victim, null, Player.MaxHealth, ItemName.Default_Death);
	}

	private static void ItemInfoCommand(ref string? responseText, Player player, string[] parameters)
	{
		responseText = ItemsDB.Items[int.Parse(parameters[0])].ToString();
	}

	private static void ItemUpgradesCommand(ref string? responseText, Player player, string[] parameters)
	{
		ItemName item = parameters.Length > 0 ? (ItemName)int.Parse(parameters[0]) : player.CurrentWeapon;

		responseText += "\n\n\n\n\n : ";
		responseText += $"[DMG     ] {ItemsDB.GetDamage(item)}\n : ";
		responseText += $"[FIRERATE] {ItemsDB.GetFireRate(item)}\n : ";
		responseText += $"[CLIP    ] {ItemsDB.GetClipSize(item)}\n : ";
		responseText += $"[RESERVE ] {ItemsDB.GetReserveSize(item)}\n : ";
		responseText += $"[RELOAD  ] {ItemsDB.GetReloadSpeed(item)}";

		GD.Print(responseText);
	}

	private static void ZombieCommand(IEntityCommand entityCommand, ref string? responseText, Player player, string[] parameters)
	{
		entityCommand.CreateEntity(
			EntityName.Zombie,
			255,

			player.Position + (Vector3.Up * 15),
			Vector3.Zero,
			Vector3.Zero,
			Vector3.Zero
		);
	}

	public static async Task HandleCommand(Player player, string[] cmd, IPacketSender packetSender, IPlayerQuery playerQuery, IPlayerLogic playerLogic, IEntityCommand entityCommand)
	{
		string? responseText = null;
		string[] parameters = cmd[1..];

		switch (cmd[0].ToLowerInvariant())
		{
			case "tp":
				TeleportCommand(playerLogic, ref responseText, player, parameters);
				break;
			case "respawn":
				RespawnCommand(playerLogic, ref responseText, player, parameters);
				break;
			case "kill":
				KillCommand(playerQuery, playerLogic, ref responseText, player, parameters);
				break;
			case "ii": // item info
				ItemInfoCommand(ref responseText, player, parameters);
				break;
			case "iu": // item upgrades
				ItemUpgradesCommand(ref responseText, player, parameters);
				break;
			case "zombie": // spawn a zombie
				ZombieCommand(entityCommand, ref responseText, player, parameters);
				break;
			default:
				responseText = "Unknown command.";
				break;
		}

		if (responseText is not null)
		{
			Packet chatResponsePacket = Packet.Create(PacketName.Chat);
			chatResponsePacket.Write<byte>((byte)player.Index);
			chatResponsePacket.Write<byte>((byte)player.Team);
			chatResponsePacket.Write<byte>(0);
			chatResponsePacket.WriteString($"[ONLY VISIBLE TO YOU]:");
			packetSender.SendPacket(player, chatResponsePacket);

			chatResponsePacket = Packet.Create(PacketName.Chat);
			chatResponsePacket.Write<byte>((byte)player.Index);
			chatResponsePacket.Write<byte>((byte)player.Team);
			chatResponsePacket.Write<byte>(0);
			chatResponsePacket.WriteString(responseText);
			packetSender.SendPacket(player, chatResponsePacket);
		}
	}
}
