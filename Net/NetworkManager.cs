using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Net;

// ! CRITICAL TODO: rewrite this as async
public class NetworkManager(IPlayerQuery playersQuery, IHasPlayerLimit playersLimit, IPlayerCommand playersCommand) : IPacketSender, IPacketEmitter, ICanServe, IPolling
{
	private const int NetTickrate = 60;
	private const int NetTickrateMs = 1000 / NetTickrate;

	private readonly TcpServer _tcp = new();
	private readonly List<Thread> _threads = [];
	private readonly ConcurrentDictionary<int, ConcurrentQueue<Packet>> _packetsQueue = new();

	public bool IsServing { get; private set; } = false;
	public event Action<Player, Packet>? PacketReceived;

	public void Poll()
	{
		if (!IsServing) return;

		// foreach (Player player in Players.Values)
		// {
		// 	// respawn if under the map
		// 	if (player.IsSpawned && player.IsAlive && (player.Position.Y < 0.5))
		// 		this.SpawnPlayer(player, at: new Vector3(15, 50, 15));
		// }

		if (!_tcp.IsConnectionAvailable()) return;

		StreamPeerTcp peer = _tcp.TakeConnection();
		var thread = new Thread(() => ConnectionThread(peer));
		thread.Start();
		lock (_threads) _threads.Add(thread);
	}

	private void ConnectionThread(StreamPeerTcp peer)
	{
		var connectedIpPort = $"{peer.GetConnectedHost()}:{peer.GetConnectedPort()}";

		GD.Print($"[ NET ] {connectedIpPort} is connecting...");

		int playerCount = playersQuery.GetPlayerCount();
		if (playerCount >= playersLimit.MaxPlayers)
		{
			GD.Print($"[ NET ] {connectedIpPort} couldn't connect because there is too many players already. ({playerCount} >= {playersLimit.MaxPlayers})");
			peer.DisconnectFromHost();
			lock (_threads) _threads.Remove(Thread.CurrentThread);
			return;
		}

		for (uint connectionTimeout = 0; peer.GetStatus() == StreamPeerTcp.Status.Connecting; connectionTimeout++)
		{
			if (!IsServing) break;
			if (connectionTimeout > 100) // 10 seconds
			{
				GD.Print($"[ NET ] {connectedIpPort} timed out on connection.");
				peer.DisconnectFromHost();
				lock (_threads) _threads.Remove(Thread.CurrentThread);
				return;
			}
			Thread.Sleep(100);
			peer.Poll();
		}

		Player player = playersCommand.CreatePlayer();

		try
		{
			peer.SetNoDelay(true);

			// if (!Players.TryAdd(player.Index, player))
			// {
			// 	GD.Print($"[ NET ] {connectedIpPort} {player}'s index already exists.");
			// 	peer.PutData(Packet.Create(PacketName.AppDisconnect).Build());
			// 	return;
			// }

			var myPacketsQueue = new ConcurrentQueue<Packet>();
			if (!_packetsQueue.TryAdd(player.Index, myPacketsQueue))
			{
				GD.Print($"[ NET ] {connectedIpPort} {player}'s packetsQueue already exists.");
				peer.PutData(Packet.Create(PacketName.AppDisconnect).Build());
				return;
			}

			GD.Print($"[ NET ] {connectedIpPort} {player} connected.");
			// CallThreadSafe(MethodName.EmitSignal, SignalName.PlayerConnected, player.Index);

			Packet greetPacket = Packet.Create(PacketName.Chat);
			greetPacket.Write<byte>((byte)player.Index);
			greetPacket.Write<byte>((byte)player.Team);
			greetPacket.Write<byte>(0); // team say
			greetPacket.WriteString($"Hi, {player.Name}! Your index is {player.Index}.");
			peer.PutData(greetPacket.Build());

			ulong lastPacketReceivedAt = Time.GetTicksMsec();
			while (IsServing && player.Connected) // TODO: wrap this in some sort of global timeout
			{
				Thread.Sleep(NetTickrateMs); // TODO
				peer.Poll();

				if ((peer.GetStatus() != StreamPeerTcp.Status.Connected) || (!IsServing) || (!player.Connected))
					break;

				var currentTime = Time.GetTicksMsec();

				// timeout if no packets were sent in 120 seconds
				if ((currentTime - lastPacketReceivedAt) > 120_000)
				{
					GD.Print($"[ NET ] {connectedIpPort} {player} timed out");
					peer.PutData(Packet.Create(PacketName.AppDisconnect).Build());
					return;
				}

				// send everything i can
				while (myPacketsQueue.TryDequeue(out Packet? packetToSend))
					peer.PutData(packetToSend.Build());

				int received = peer.GetAvailableBytes();
				if (received < (sizeof(byte) * 2) + sizeof(ushort)) continue; // must at least have the header

				// we got a packet
				// ! CRITICAL TODO: handle multiple packets a tick

				lastPacketReceivedAt = Time.GetTicksMsec();

				byte gotMagic = peer.GetU8();
				if (gotMagic != Packet.MagicByte)
				{
					GD.PushError($"[ NET ] {connectedIpPort} {player} sent an invalid magic byte (got: {gotMagic}, expected: {Packet.MagicByte})");
					peer.PutData(Packet.Create(PacketName.AppDisconnect).Build());
					return;
				}

				Packet packet = Packet.Parse(peer);

				if (packet.Name is not PacketName.Position)
					GD.PrintRich($"[ NET ] [color=#11ee11][C -> S][/color] {player.Index} | {packet}");

				PacketReceived?.Invoke(player, packet);
			}
		}
		finally
		{
			player.Connected = false;

			// try to disconnect "gracefully" just incase
			try { if (peer.GetStatus() == StreamPeerTcp.Status.Connected) peer.PutData(Packet.Create(PacketName.AppDisconnect).Build()); } catch (Exception) { }
			try { if (peer.GetStatus() == StreamPeerTcp.Status.Connected) peer.DisconnectFromHost(); } catch (Exception) { }

			GD.Print($"[ NET ] {connectedIpPort} {player} disconnected.");
			_packetsQueue.TryRemove(player.Index, out _);
			playersCommand.RemovePlayer(player.Index);

			lock (_threads) _threads.Remove(Thread.CurrentThread);
			// CallThreadSafe(MethodName.EmitSignal, SignalName.PlayerDisconnected, player.Index);
		}
	}

	public void SendPacket(Player player, PacketName packetName) => SendPacket(player.Index, Packet.Create(packetName));
	public void SendPacket(Player player, Packet packet) => SendPacket(player.Index, packet);
	public void SendPacket(int playerIndex, PacketName packetName) => SendPacket(playerIndex, Packet.Create(packetName));
	public void SendPacket(int playerIndex, Packet packet)
	{
		if ((packet.Name != PacketName.Position) && (packet.Name != PacketName.ChunkData) && (packet.Name != PacketName.Scores))
			GD.PrintRich($"[ NET ] [color=#ee1111][S -> C][/color] {playerIndex} | {packet}");

		Player? player = playersQuery.GetPlayer(playerIndex);
		if (player is null)
		{
			GD.PrintRich($"[ NET ] [color=#ee1111]{playerIndex} was not found in the players list. Packet isn't sent.[/color]");
			return;
		}

		if (!player.Connected)
		{
			GD.PrintRich($"[ NET ] [color=#ee1111]{playerIndex} is not connected. Packet isn't sent.[/color]");
			return;
		}

		if (_packetsQueue.TryGetValue(playerIndex, out var theirPacketsQueue))
			theirPacketsQueue.Enqueue(packet);
	}

	public void BroadcastPacket(PacketName packetName, int butNotForPlayerIndex = byte.MaxValue) => BroadcastPacket(Packet.Create(packetName), butNotForPlayerIndex);
	public void BroadcastPacket(Packet packet, int butNotForPlayerIndex = byte.MaxValue)
	{
		foreach (Player player in playersQuery.GetPlayers())
		{
			if (player.Index != butNotForPlayerIndex)
			{
				SendPacket(player, (Packet)packet.Duplicate());
			}
		}
	}

	public async Task StartServing(ushort port)
	{
		GD.Print($"[ NET ] listening on port {port}");
		_tcp.Listen(port, "*");
		IsServing = true;
	}

	public async Task StopServing()
	{
		IsServing = false;
		List<Thread> threadsCopy;
		lock (_threads)
		{
			threadsCopy = [.. _threads];
		}

		foreach (var t in threadsCopy)
		{
			t.Join();
		}
	}
}
