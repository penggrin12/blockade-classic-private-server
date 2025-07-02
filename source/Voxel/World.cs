using System.Collections.Generic;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Voxel;

public partial class World : IWorldChunked, IWorldVoxelQuery, IWorldVoxelCommand, IWorldMapDataFillable
{
	private const int _ChunksWidth = 16;
	private const int _ChunksHeight = 4;
	private const int _ChunksDepth = 16;

	private readonly Chunk[,,] chunks = new Chunk[_ChunksWidth, _ChunksHeight, _ChunksDepth];
	private readonly Dictionary<Team, List<Vector3I>> spawnPoints = new()
	{
		{ Team.Blue, [] },
		{ Team.Red, [] },
		{ Team.Green, [] },
		{ Team.Yellow, [] },

		{ Team.Bot, [] },
	};

	private Vector3I _center = new(128, 15, 128);

	public int ChunksWidth => _ChunksWidth;
	public int ChunksHeight => _ChunksHeight;
	public int ChunksDepth => _ChunksDepth;
	public int ChunkSize => Chunk.Size;

	public Vector3I CenterPoint => _center;

	public World()
	{
		for (var x = 0; x < ChunksWidth; x++)
		{
			for (var y = 0; y < ChunksHeight; y++)
			{
				for (var z = 0; z < ChunksDepth; z++)
				{
					chunks[x, y, z] = new() { Position = new(x, y, z) };
				}
			}
		}
	}

	public Vector3I FindSpawnPoint(Team team)
	{
		var spawns = spawnPoints[team];
		return spawns[(int)(GD.Randi() % spawns.Count)];
	}

	private Chunk GetChunk(Vector3I at) => chunks[at.X, at.Y, at.Z];
	private Chunk GetChunk(int x, int y, int z) => chunks[x, y, z];

	public Block GetBlock(Vector3I at) => GetBlock(at.X, at.Y, at.Z);
	public Block GetBlock(int x, int y, int z)
	{
		return GetChunk(x / Chunk.Size, y / Chunk.Size, z / Chunk.Size).GetBlock(x % Chunk.Size, y % Chunk.Size, z % Chunk.Size);
	}

	public int DamageBlock(Vector3I at, int amount) => DamageBlock(at.X, at.Y, at.Z, amount);
	public int DamageBlock(int x, int y, int z, int amount)
	{
		return GetChunk(x / Chunk.Size, y / Chunk.Size, z / Chunk.Size).DamageBlock(x % Chunk.Size, y % Chunk.Size, z % Chunk.Size, amount);
	}

	public void SetBlock(Vector3I at, BlockType blockType) => SetBlock(at.X, at.Y, at.Z, blockType);
	public void SetBlock(int x, int y, int z, BlockType blockType)
	{
		GD.Print($"[VOXEL] placing {blockType} at ({x}, {y}, {z})");

		switch (blockType)
		{
			case BlockType.R_B_Blue:
				spawnPoints[Team.Blue].Add(new Vector3I(x, y, z));
				break;
			case BlockType.R_B_Red:
				spawnPoints[Team.Red].Add(new Vector3I(x, y, z));
				break;
			case BlockType.R_B_Green:
				spawnPoints[Team.Green].Add(new Vector3I(x, y, z));
				break;
			case BlockType.R_B_Yellow:
				spawnPoints[Team.Yellow].Add(new Vector3I(x, y, z));
				break;
			case BlockType.R_Z:
				spawnPoints[Team.Bot].Add(new Vector3I(x, y, z));
				break;
			case BlockType.R_Center:
				_center = new Vector3I(x, y, z);
				break;
		}

		GetChunk(x / Chunk.Size, y / Chunk.Size, z / Chunk.Size).SetBlock(x % Chunk.Size, y % Chunk.Size, z % Chunk.Size, blockType);
	}

	public ICollection<ChunkData> FindChangedChunks()
	{
		GD.Print("[VOXEL] Finding dirty chunks...");
		List<ChunkData> dirtyChunkDatas = [];
		foreach (Chunk chunk in chunks)
		{
			if (!chunk.IsDirty) continue;
			dirtyChunkDatas.Add(chunk.ToChunkData());
		}
		GD.Print($"[VOXEL] Found {dirtyChunkDatas.Count} dirty chunks.");
		return dirtyChunkDatas;
	}

	public void FillWithMapData(byte[] mapData)
	{
		const int ExpectedSize = _ChunksWidth * Chunk.Size * _ChunksHeight * Chunk.Size * _ChunksDepth * Chunk.Size;
		if (mapData.Length != ExpectedSize)
		{
			GD.PushError($"[VOXEL] Map data has incorrect size. Expected {ExpectedSize}, but got {mapData.Length}.");
			return;
		}

		int i = 0;
		for (var x = 0; x < _ChunksWidth * Chunk.Size; x++)
		{
			for (var z = 0; z < _ChunksDepth * Chunk.Size; z++)
			{
				for (var y = 0; y < _ChunksHeight * Chunk.Size; y++)
				{
					BlockType blockType = (BlockType)mapData[i];

					int chunkX = x / Chunk.Size, chunkY = y / Chunk.Size, chunkZ = z / Chunk.Size;
					int localX = x % Chunk.Size, localY = y % Chunk.Size, localZ = z % Chunk.Size;

					ref Block block = ref chunks[chunkX, chunkY, chunkZ].GetBlockRef(localX, localY, localZ);
					block.Type = blockType;
					block.Health = (byte)(blockType == BlockType.None ? 0 : Block.MaxHealth);

					i++;
				}
			}
		}

		FindAndRegisterSpecialBlocks();

		GD.Print("[VOXEL] World successfully filled with map data.");
	}

	private void FindAndRegisterSpecialBlocks()
	{
		_center = new Vector3I(128, 15, 128);
		foreach (var list in spawnPoints.Values)
			list.Clear();

		for (int x = 0; x < ChunksWidth * Chunk.Size; x++)
		{
			for (int y = 0; y < ChunksHeight * Chunk.Size; y++)
			{
				for (int z = 0; z < ChunksDepth * Chunk.Size; z++)
				{
					switch (GetBlock(x, y, z).Type)
					{
						case BlockType.R_B_Blue:
							spawnPoints[Team.Blue].Add(new Vector3I(x, y, z));
							break;
						case BlockType.R_B_Red:
							spawnPoints[Team.Red].Add(new Vector3I(x, y, z));
							break;
						case BlockType.R_B_Green:
							spawnPoints[Team.Green].Add(new Vector3I(x, y, z));
							break;
						case BlockType.R_B_Yellow:
							spawnPoints[Team.Yellow].Add(new Vector3I(x, y, z));
							break;
						case BlockType.R_Z:
							spawnPoints[Team.Bot].Add(new Vector3I(x, y, z));
							break;
						case BlockType.R_Center:
							_center = new Vector3I(x, y, z);
							break;
					}
				}
			}
		}
	}
}
