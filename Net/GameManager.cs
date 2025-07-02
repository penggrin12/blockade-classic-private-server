using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Net.Shared;

namespace BlockadeClassicPrivateServer.Net;

public class GameManager(IPacketSender packetSender) : IGameManager
{
	public Dictionary<Team, int> scores = new() { { Team.Blue, 0 }, { Team.Red, 0 }, { Team.Green, 0 }, { Team.Yellow, 0 } };

	// [Export] public string MapId { get; set; } = "1";
	public Godot.Timer RoundTimer { get; } = new() { WaitTime = 300 };
	public Godot.Timer ScoreUpdateTimer { get; } = new() { WaitTime = 1 };

	private GameMode _mode = GameMode.Build;
	// private World World { get; } = new();
	// private ItemData[] Items => ItemsDB.Items;

	// [Signal] public delegate void PlayerConnectedEventHandler(int playerIndex);
	// [Signal] public delegate void PlayerDisconnectedEventHandler(int playerIndex);
	// [Signal] public delegate void PlayerLoggedInEventHandler(int playerIndex);
	// [Signal] public delegate void PacketReceivedEventHandler(int byPlayerIndex, Packet packet);

	public int GetTeamScore(Team team)
	{
		return scores[team];
	}

	public void AddTeamScore(Team team, int score)
	{
		scores[team] += score;
	}

	public string GetMapId() => "1";

	public GameMode GetGameMode()
	{
		return _mode;
	}

	public void SetGameMode(GameMode gameMode)
	{
		_mode = gameMode;
	}

	public async Task ScoreUpdateLoopAsync(CancellationToken cancellationToken)
	{
		PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

		try
		{
			while (await timer.WaitForNextTickAsync(cancellationToken))
			{
				if (_mode != GameMode.Build)
				{
					var scoresPacket = Packet.Create(PacketName.Scores);
					scoresPacket.Write<int>(GetTeamScore(Team.Blue));
					scoresPacket.Write<int>(GetTeamScore(Team.Red));
					scoresPacket.Write<int>(GetTeamScore(Team.Green));
					scoresPacket.Write<int>(GetTeamScore(Team.Yellow));
					scoresPacket.Write<int>((int)RoundTimer.TimeLeft);
					packetSender.BroadcastPacket(scoresPacket);
				}
			}
		}
		catch (OperationCanceledException) {}
		finally { timer.Dispose(); }
	}

	// public override async void _Ready()
	// {
	// 	populate items early
    //     var _ = ItemsDB.Items;

	// 	if (OS.HasFeature("editor"))
	// 		await File.WriteAllTextAsync(ProjectSettings.GlobalizePath("res://items.json"), JsonSerializer.Serialize(Items));

	// 	AddChild(World);
	// 	World.FillWithMapData(await MapDownloader.DownloadMap(MapId));

	// 	AddChild(RoundTimer);
	// 	AddChild(ScoreUpdateTimer);
	// 	RoundTimer.Start();
	// 	ScoreUpdateTimer.Start();

	// 	PacketReceived += (int byPlayerIndex, Packet recvPacket) => this.HandlePacket(Players[byPlayerIndex], recvPacket);

	// 	ServeOn(7777);
	// }

	// public override void _Process(double delta)
	// {
	// 	if (!IsServing) return;

	// 	if (RoundTimer.IsStopped())
	// 	{
	// 		// TODO: server restart. mode/map change.
	// 		RoundTimer.Start();
	// 	}

	// 	foreach (Player player in Players.Values)
	// 	{
	// 		// respawn if under the map
	// 		if (player.IsSpawned && player.IsAlive && (player.Position.Y < 0.5))
	// 			this.SpawnPlayer(player, at: new Vector3(15, 50, 15));
	// 	}

	// 	if (!tcp.IsConnectionAvailable()) return;

	// 	StreamPeerTcp peer = tcp.TakeConnection();
	// 	var thread = new Thread(() => ConnectionThread(peer));
	// 	thread.Start();
	// 	lock (threads) threads.Add(thread);
	// }

	// public override void _ExitTree()
	// {
	// 	IsServing = false;
	// 	List<Thread> threadsCopy;
	// 	lock (threads)
	// 	{
	// 		threadsCopy = [.. threads];
	// 	}

	// 	foreach (var t in threadsCopy)
	// 	{
	// 		t.Join();
	// 	}
	// }
}
