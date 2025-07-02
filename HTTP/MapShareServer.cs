using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using Godot;

namespace BlockadeClassicPrivateServer.Http;

public sealed class MapShareServer : ICanServe
{
	private readonly HttpServer httpServer = new();

	public bool IsServing { get; private set; } = false;

	public async Task StartServing(ushort port)
	{
		IsServing = true;

		httpServer.OnClientConnection = OnClientConnection;
		httpServer.Start(port);

		await httpServer.Poll();
	}

	public async Task StopServing()
	{
		IsServing = false;

		await httpServer.Stop();

		httpServer.OnClientConnection = null;
	}

	private async Task OnClientConnection(TcpClient client, NetworkStream stream, StreamWriter writer, string path)
	{
		if (!path.EndsWith(".map"))
		{
			await writer.WriteAsync("HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n");
			await writer.FlushAsync();
			GD.PushError($"[HTTP ] {client.Client.RemoteEndPoint} requested path doesn't end with .map ({path})");
			return;
		}

		int lastSlash = path.LastIndexOf('/');
		int dotMap = path.LastIndexOf(".map");

		if ((lastSlash == -1) || (dotMap <= lastSlash))
		{
			await writer.WriteAsync("HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n");
			await writer.FlushAsync();
			return;
		}

		string idPart = path.Substring(lastSlash + 1, dotMap - lastSlash - 1);

		if (!int.TryParse(idPart, out _))
		{
			GD.PushError($"[HTTP ] {client.Client.RemoteEndPoint} tried requesting a non numerical map ID. ({idPart})");
			await writer.WriteAsync("HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n");
			await writer.FlushAsync();
			return;
		}

		GD.Print($"[HTTP ] {client.Client.RemoteEndPoint} requests map ID: {idPart}");

		await using var fileStream = new FileStream($"./maps/{idPart}.map", FileMode.Open);

		GD.Print($"[HTTP ] {client.Client.RemoteEndPoint} sending the map. ({idPart})");

		byte[] header = Encoding.ASCII.GetBytes(
			"HTTP/1.1 200 OK\r\n" +
			"Content-Type: application/octet-stream\r\n" +
			$"Content-Length: {fileStream.Length}\r\n" +
			"Connection: close\r\n\r\n");
		await stream.WriteAsync(header);
		await stream.FlushAsync();
		await fileStream.CopyToAsync(stream);

		GD.Print($"[HTTP ] {client.Client.RemoteEndPoint} received the map. {idPart}");
	}
}
