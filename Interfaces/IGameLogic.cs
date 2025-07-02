using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IGameLogic
{
	void SpawnPlayer(Player player, Vector3? at = null, int forWhom = byte.MaxValue);
	bool DamageZombie(int zombieEntityIndex, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine);
	bool DamagePlayer(Player victim, Player? attacker, int amount, ItemName weaponId, BodyPart hitbox = BodyPart.Spine, bool allowRespawn = true);
	void SendChatMessage(Player? toPlayer, Player? byPlayer, bool teamSay, string message);
}
