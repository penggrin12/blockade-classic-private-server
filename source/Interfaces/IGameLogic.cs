using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

// TODO: spread this out
public interface IPlayerLogic
{
	void SpawnPlayer(Player player, Vector3? at = null, int forWhom = byte.MaxValue);
	bool DamagePlayer(Player victim, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine, bool allowRespawn = true);
}

public interface IEntityLogic
{
	bool DamageZombie(int zombieEntityIndex, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine);
}

public interface IPlayerDisconnector
{
	Task DisconnectPlayer(Player player, bool informVictim = true);
	Task DisconnectEveryone();
	Task ReconnectEveryone();
}

public interface IChatSender
{
	void SendChatMessage(Player? toPlayer, Player? byPlayer, bool teamSay, string message);
}
