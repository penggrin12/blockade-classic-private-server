using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer;

public class PlayerManager : IHasPlayerLimit, IPlayerQuery, IPlayerCommand
{
	private const int _MaxPlayers = 32;
	public int MaxPlayers => _MaxPlayers;

	private ConcurrentDictionary<int, Player> Players { get; init; } = [];

	private int FindFreePlayerIndex()
	{
		for (var resultPlayerIndex = 0; resultPlayerIndex < _MaxPlayers; resultPlayerIndex++)
			if (!Players.ContainsKey(resultPlayerIndex)) return resultPlayerIndex;

		throw new IndexOutOfRangeException($"Couldn't find a new player index. Too many players online? ({Players.Count}/{_MaxPlayers})");
	}

	public int GetPlayerCount() => Players.Count;
	public ICollection<Player> GetPlayers() => Players.Values;
	public IEnumerable<Player> GetPlayersInRadius(Vector3 at, float radius) => Players.Values.Where(x => x.IsSpawned && x.IsAlive && (at.DistanceTo(x.Position) <= radius));

	public Player? GetPlayer(int playerIndex)
	{
		Players.TryGetValue(playerIndex, out Player? player);
		return player;
	}

	public Player CreatePlayer()
	{
		Player player = new Player() { Index = FindFreePlayerIndex(), Name = $"Guest{GD.Randi() % 100000}" };
		Players.TryAdd(player.Index, player);
		return player;
	}

	public void RemovePlayer(int playerIndex)
	{
		Players.Remove(playerIndex, out _);
	}
}
