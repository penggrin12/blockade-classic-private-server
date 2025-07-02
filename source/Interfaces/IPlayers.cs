using System.Collections.Generic;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IHasLimit
{
	int Max { get; }
	int Count { get; }
}

public interface IPlayerQuery : IHasLimit
{
	Player? GetPlayer(int index);

	ICollection<Player> GetPlayers();
	IEnumerable<Player> GetPlayersInRadius(Vector3 at, float radius);
}

public interface IPlayerCommand : IHasLimit
{
	Player? CreatePlayer();
	bool RemovePlayer(int playerIndex);
}
