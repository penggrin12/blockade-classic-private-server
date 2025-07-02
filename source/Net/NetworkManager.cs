using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Net;

public class NetworkManager(IPlayerQuery playersQuery, IHasLimit playersLimit, IPlayerCommand playersCommand) : IPacketSender, IPacketEmitter, ICanServe
{
	private const int NetTickrate = 60;
	private const int NetTickrateMs = 1000 / NetTickrate;

	private readonly ConcurrentDictionary<int, ConcurrentQueue<Packet>> _packetsQueue = [];
	private readonly CancellationTokenSource cts = new();
	private readonly SynchronizationContext _syncContext = SynchronizationContext.Current!;

	public bool IsServing { get; private set; } = false;
	public event Action<Player, Packet>? PacketReceived;

	private async Task HandleConnection(TcpClient client, CancellationToken cancellationToken)
	{
		client.NoDelay = true;

		var ipPort = $"{client.Client.RemoteEndPoint}";
		GD.Print($"[ NET ] {ipPort} connected.");

		NetworkStream stream = client.GetStream();

		Player? player = null;
		try
		{
			player = playersCommand.CreatePlayer();
			if (player is null)
			{
				GD.PushWarning($"[ NET ] {ipPort} couldn't connect because there is too many players already. ({playersQuery.Count} >= {playersLimit.Max})");
				await stream.WriteAsync(Packet.Create(PacketName.AppDisconnect).Build(), cancellationToken);
				return;
			}

			var myPacketsQueue = new ConcurrentQueue<Packet>();
			if (!_packetsQueue.TryAdd(player.Index, myPacketsQueue))
			{
				GD.PushWarning($"[ NET ] {ipPort} {player}'s packetsQueue already exists.");
				await stream.WriteAsync(Packet.Create(PacketName.AppDisconnect).Build(), cancellationToken);
				return;
			}

			GD.Print($"[ NET ] {ipPort} {player} connected.");
			// CallThreadSafe(MethodName.EmitSignal, SignalName.PlayerConnected, player.Index);

			Packet greetPacket = Packet.Create(PacketName.Chat);
			greetPacket.Write<byte>((byte)player.Index);
			greetPacket.Write<byte>((byte)player.Team);
			greetPacket.Write<byte>(0); // team say
			greetPacket.WriteString($"Hi, {player.Name}! Your index is {player.Index}.");
			await stream.WriteAsync(greetPacket.Build(), cancellationToken);

			ulong lastPacketReceivedAt = Time.GetTicksMsec();
			while (true) // TODO: wrap this in some sort of global timeout
			{
				await Task.Delay(NetTickrateMs, cancellationToken); // TODO

				if ((!client.Connected) || cancellationToken.IsCancellationRequested || (!IsServing) || (!player.Connected))
					break;

				var currentTime = Time.GetTicksMsec();

				// timeout if no packets were sent in 120 seconds
				if ((currentTime - lastPacketReceivedAt) > 120_000)
				{
					GD.PushWarning($"[ NET ] {ipPort} {player} timed out.");
					return;
				}

				// send everything i can
				int sent = 0;
				while (myPacketsQueue.TryDequeue(out Packet? packetToSend))
				{
					await stream.WriteAsync(packetToSend.Build(), cancellationToken);
					sent++;
				}
				if (sent > 0) await stream.FlushAsync(cancellationToken);

				if (client.Available < Packet.HeaderLength) continue; // must at least have the header

				// we got a packet
				// ! TODO: handle multiple packets a tick

				lastPacketReceivedAt = Time.GetTicksMsec();

				byte[] header = new byte[Packet.HeaderLength];
				await stream.ReadExactlyAsync(header.AsMemory(0, Packet.HeaderLength), cancellationToken);

				if (header[0] != Packet.MagicByte)
				{
					GD.PushWarning($"[ NET ] {ipPort} {player} sent an incorrect magic byte. ({header[0]})");
					return;
				}

				ushort packetLength = BitConverter.ToUInt16(header, 2);

				if (packetLength > Packet.ReadBufferSize)
				{
					GD.PushWarning($"[ NET ] {ipPort} {player} sent a packed thats too large. ({packetLength} > {Packet.ReadBufferSize})");
					return;
				}

				byte[] body = new byte[packetLength - Packet.HeaderLength];
				await stream.ReadExactlyAsync(body.AsMemory(0, packetLength - Packet.HeaderLength), cancellationToken);

				Packet packet = Packet.Parse(header[1], packetLength, body);

				if (packet.Name is not PacketName.Position)
					GD.PrintRich($"[ NET ] [color=#11ee11][C -> S][/color] {player.Index} | {packet}");

				_syncContext.Post(_ => PacketReceived?.Invoke(player, packet), null);
			}
		}
		catch (OperationCanceledException) {}
		catch (Exception e)
		{
			GD.PushError(e);
		}
		finally
		{
			GD.Print($"[ NET ] {ipPort} {player} disconnected.");

			if (client.Connected)
			{
				try
				{
					await stream.WriteAsync(Packet.Create(PacketName.AppDisconnect).Build());
					await stream.FlushAsync();
				}
				catch (IOException) { }
			}

			if (player is not null)
			{
				player.Connected = false;
				_packetsQueue.TryRemove(player.Index, out _);
				playersCommand.RemovePlayer(player.Index);
			}

			client.Close();
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
				SendPacket(player, (Packet)packet.Clone());
			}
		}
	}

	public async Task StartServing(ushort port)
	{
		GD.Print($"[ NET ] listening on port {port}");

		using TcpListener tcp = new(IPAddress.Any, port);
		tcp.Start();

		IsServing = true;

		try
		{
			while (!cts.IsCancellationRequested)
			{
				TcpClient client = await tcp.AcceptTcpClientAsync(cts.Token);
				_ = HandleConnection(client, cts.Token);
			}
		}
		catch (OperationCanceledException) {}
		finally
		{
			await StopServing();
		}
	}

	public Task StopServing()
	{
		IsServing = false;
		cts.Cancel();
		return Task.CompletedTask;
	}
}
