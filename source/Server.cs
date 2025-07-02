using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Http;
using BlockadeClassicPrivateServer.Net;
using BlockadeClassicPrivateServer.Shared;
using BlockadeClassicPrivateServer.Voxel;
using Godot;

namespace BlockadeClassicPrivateServer;

public partial class Server : Node
{
	[Export] public string MapId { get; set; } = "1";
	[Export] public GameMode Mode = GameMode.Classic;

	[Export] public ushort GamePort { get; set; } = 7777;
	[Export] public ushort MapPort { get; set; } = 7778;

	private readonly CancellationTokenSource cts = new();

	private readonly MapShareServer _mapShareServer;

	private readonly World _world;
	private readonly PlayerManager _playerManager;
	private readonly NetworkManager _networkManager;
	private readonly EntityManager _entityManager;
	private readonly GameManager _gameManager;
	private readonly GameLogic _gameLogic;

	public Server()
	{
		_mapShareServer = new();

		_world          = new();
		_playerManager  = new();
		_networkManager = new(_playerManager, _playerManager, _playerManager);
		_gameManager    = new(_networkManager);
		_gameLogic      = new(_networkManager, _gameManager, _gameManager, _gameManager, _playerManager, _world);
		_entityManager  = new(_networkManager, _gameLogic, _playerManager);

		_gameManager.SetGameMode(Mode);

		_networkManager.PacketReceived += async (Player player, Packet packet) => await PacketHandler.HandlePacket(
			player, packet, _gameLogic, _gameLogic, _gameLogic, _gameLogic, _gameManager, _gameManager, _networkManager, _entityManager, _entityManager, _playerManager, _world, _world
		);
		PacketHandler.CommandReceived += async (Player player, string[] cmd) => await CommandHandler.HandleCommand(
			player, cmd, _networkManager, _playerManager, _gameLogic, _entityManager
		);
	}

	public override async void _Ready()
	{
		GetTree().AutoAcceptQuit = false;

		// populate items early
		_ = ItemsDB.Items;

		if (OS.HasFeature("editor"))
			await File.WriteAllTextAsync(ProjectSettings.GlobalizePath("res://items.json"), JsonSerializer.Serialize(ItemsDB.Items));

		_ = _mapShareServer.StartServing(MapPort);
		_world.FillWithMapData(await MapDownloader.DownloadMap(_gameManager.GetMapId()));

		_ = _networkManager.StartServing(GamePort);
		_ = _gameManager.ScoreUpdateLoopAsync(cts.Token);
	}

	public override async void _Notification(int what)
	{
		if ((what != Node.NotificationWMCloseRequest) && (what != Node.NotificationCrash)) return;

		// exit behaviour

		GD.Print("[ !!! ] Stopping...");
		cts.Cancel();
		await Task.WhenAll(_mapShareServer.StopServing(), _networkManager.StopServing());

		GetTree().Quit();
	}
}
