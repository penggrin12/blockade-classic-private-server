using System.Collections.Generic;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IHasPlayerLimit
{
	int MaxPlayers { get; }
}

public interface IPlayerQuery
{
	Player? GetPlayer(int index);

	int GetPlayerCount();
	ICollection<Player> GetPlayers();
	IEnumerable<Player> GetPlayersInRadius(Vector3 at, float radius);
}

public interface IPlayerCommand
{
	Player CreatePlayer();
	void RemovePlayer(int playerIndex);
}
