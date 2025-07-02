using System.IO;
using System.Text.Json;
using BlockadeClassicPrivateServer.Http;
using BlockadeClassicPrivateServer.Net.Shared;
using BlockadeClassicPrivateServer.Voxel;
using Godot;

namespace BlockadeClassicPrivateServer.Net;

public partial class Server : Node
{
	[Export] public string MapId { get; set; } = "1";

	[Export] public ushort GamePort { get; set; } = 7777;
	[Export] public ushort MapPort { get; set; } = 7778;

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
		_gameLogic      = new(_networkManager, _playerManager, _gameManager, _world);
		_entityManager  = new(_networkManager, _gameManager, _gameLogic, _playerManager);

		_networkManager.PacketReceived += async (Player player, Packet packet) => await PacketHandler.HandlePacket(
			player, packet, _gameLogic, _gameManager, _networkManager, _entityManager, _entityManager, _playerManager, _world, _world
		);
	}

	public override async void _Ready()
	{
		// populate items early
		_ = ItemsDB.Items;

		if (OS.HasFeature("editor"))
			await File.WriteAllTextAsync(ProjectSettings.GlobalizePath("res://items.json"), JsonSerializer.Serialize(ItemsDB.Items));

		_ = _mapShareServer.StartServing(MapPort);
		_world.FillWithMapData(await MapDownloader.DownloadMap(MapId));

		// TODO: reenable when network is async
		// PacketReceived += (int byPlayerIndex, Packet recvPacket) => this.HandlePacket(Players[byPlayerIndex], recvPacket);

		_ = _networkManager.StartServing(GamePort);
	}

	public override void _Process(double delta)
	{
		_networkManager.Poll();
	}

	public override async void _ExitTree()
	{
		await _mapShareServer.StopServing();
		await _networkManager.StopServing();
	}
}
