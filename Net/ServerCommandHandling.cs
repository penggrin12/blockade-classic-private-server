using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Net;

public static class ServerCommandHandling
{
	// private static void TeleportCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	server.SpawnPlayer(player, new Vector3(byte.Parse(parameters[0]), byte.Parse(parameters[1]), byte.Parse(parameters[2])));
	// }

	// private static void RespawnCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	server.SpawnPlayer(player, new Vector3(0, 63, 0));
	// }

	// private static void KillCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	int victimIndex = player.Index;
	// 	if (parameters.Length > 0) _ = int.TryParse(parameters[0], out victimIndex);
	// 	Player? victim = server.GetPlayer(victimIndex);
	// 	if (victim is null)
	// 	{
	// 		responseText = "Unknown player.";
	// 		return;
	// 	}
	// 	server.DamagePlayer(victim, null, Player.MaxHealth, ItemName.Default_Death);
	// }

	// private static void ItemInfoCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	responseText = server.Items[int.Parse(parameters[0])].ToString();
	// }

	// private static void ItemUpgradesCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	ItemName item = parameters.Length > 0 ? (ItemName)int.Parse(parameters[0]) : player.CurrentWeapon;

	// 	responseText += "\n\n\n\n\n : ";
	// 	responseText += $"[DMG     ] {ItemsDB.GetDamage(item)}\n : ";
	// 	responseText += $"[FIRERATE] {ItemsDB.GetFireRate(item)}\n : ";
	// 	responseText += $"[CLIP    ] {ItemsDB.GetClipSize(item)}\n : ";
	// 	responseText += $"[RESERVE ] {ItemsDB.GetReserveSize(item)}\n : ";
	// 	responseText += $"[RELOAD  ] {ItemsDB.GetReloadSpeed(item)}";

	// 	GD.Print(responseText);
	// }

	// private static void ZombieCommand(ref string? responseText, Player player, string[] parameters)
	// {
	// 	server.CreateEntity(
	// 		EntityName.Zombie,
	// 		255,

	// 		player.Position + (Vector3.Up * 15),
	// 		Vector3.Zero,
	// 		Vector3.Zero,
	// 		Vector3.Zero
	// 	);
	// }

	// public static async Task HandleCommand(Player player, string[] cmd)
	// {
	// 	string? responseText = null;
	// 	string[] parameters = cmd[1..];

	// 	switch (cmd[0].ToLowerInvariant())
	// 	{
	// 		case "tp":
	// 			TeleportCommand(ref responseText, player, parameters);
	// 			break;
	// 		case "respawn":
	// 			RespawnCommand(ref responseText, player, parameters);
	// 			break;
	// 		case "kill":
	// 			KillCommand(ref responseText, player, parameters);
	// 			break;
	// 		case "ii": // item info
	// 			ItemInfoCommand(ref responseText, player, parameters);
	// 			break;
	// 		case "iu": // item upgrades
	// 			ItemUpgradesCommand(ref responseText, player, parameters);
	// 			break;
	// 		case "zombie": // spawn a zombie
	// 			ZombieCommand(ref responseText, player, parameters);
	// 			break;
	// 		default:
	// 			responseText = "Unknown command.";
	// 			break;
	// 	}

	// 	if (responseText is not null)
	// 	{
	// 		Packet chatResponsePacket = Packet.Create(PacketName.Chat);
	// 		chatResponsePacket.Write<byte>((byte)player.Index);
	// 		chatResponsePacket.Write<byte>((byte)player.Team);
	// 		chatResponsePacket.Write<byte>(0);
	// 		chatResponsePacket.WriteString($"[ONLY VISIBLE TO YOU]:");
	// 		server.SendPacket(player, chatResponsePacket);

	// 		chatResponsePacket = Packet.Create(PacketName.Chat);
	// 		chatResponsePacket.Write<byte>((byte)player.Index);
	// 		chatResponsePacket.Write<byte>((byte)player.Team);
	// 		chatResponsePacket.Write<byte>(0);
	// 		chatResponsePacket.WriteString(responseText);
	// 		server.SendPacket(player, chatResponsePacket);
	// 	}
	// }
}
