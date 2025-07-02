using System;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Shared;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IPacketSender
{
	void SendPacket(Player player, PacketName packetName);
	void SendPacket(Player player, Packet packet);
	void SendPacket(int playerIndex, PacketName packetName);
	void SendPacket(int playerIndex, Packet packet);

	void BroadcastPacket(PacketName packetName, int butNotForPlayerIndex = byte.MaxValue);
	void BroadcastPacket(Packet packet, int butNotForPlayerIndex = byte.MaxValue);
}

public interface IPacketEmitter
{
	event Action<Player, Packet>? PacketReceived;
}

public interface ICanServe
{
	bool IsServing { get; }

	Task StartServing(ushort port);
	Task StopServing();
}

public interface IPolling
{
	void Poll();
}
