using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace BlockadeClassicPrivateServer.Http;

public class HttpServer
{
	public bool IsStarted { get; private set; } = false;

	public Func<TcpClient, NetworkStream, StreamWriter, string, Task>? OnClientConnection;

	private TcpListener? tcp;
	private readonly CancellationTokenSource cts = new();

	public void Start(int port)
	{
		if (OnClientConnection is null)
			throw new Exception("OnClientConnection cannot be null before starting HttpServer.");

		IsStarted = true;
		tcp = new TcpListener(IPAddress.Any, port);
		tcp.Start();
		GD.Print($"[HTTP ] listening on port {port}.");
	}

	public async Task Stop()
	{
		IsStarted = false;
		await cts.CancelAsync();
		tcp?.Stop();
		tcp = null;
		GD.Print($"[HTTP ] stopped listening.");
	}

	public async Task Poll()
	{
		if (!IsStarted)
		{
			GD.PushError("Attempted to poll a HttpServer that is stopped.");
			return;
		}

		try
		{
			while (IsStarted)
			{
				_ = HandleClientAsync(await tcp!.AcceptTcpClientAsync(cts.Token));
			}
		}
		catch (OperationCanceledException) {}
	}

	private async Task HandleClientAsync(TcpClient client)
	{
		GD.Print($"[HTTP ] {client.Client.RemoteEndPoint} connected.");

		await using NetworkStream stream = client.GetStream();
		using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
		await using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true) { NewLine = "\r\n", AutoFlush = true };

		try
		{
			var requestLine = await reader.ReadLineAsync();
			if (requestLine?.StartsWith("GET ") != true)
				return;

			string[] parts = requestLine.Split(' ');
			if (parts.Length < 2)
			{
				GD.PushError($"[HTTP ] {client.Client.RemoteEndPoint} parts too short. ({requestLine}: {parts.Length})");
				return;
			}

			string path = parts[1];
			await OnClientConnection!.Invoke(client, stream, writer, path);
		}
		finally
		{
			GD.Print($"[HTTP ] {client.Client.RemoteEndPoint} disconnected.");
			client.Close();
		}
	}
}
