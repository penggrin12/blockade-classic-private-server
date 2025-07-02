using System.Collections.Generic;
using BlockadeClassicPrivateServer.Net.Shared;
using BlockadeClassicPrivateServer.Voxel;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IWorldChunked
{
	int ChunksWidth { get; }
    int ChunksHeight { get; }
    int ChunksDepth { get; }

	int ChunkSize { get; }
}

public interface IWorldMapDataFillable
{
	void FillWithMapData(byte[] mapData);
}

public interface IWorldVoxelCommand
{
	int DamageBlock(Vector3I at, int amount);
	int DamageBlock(int x, int y, int z, int amount);

	void SetBlock(Vector3I at, BlockType blockType);
	void SetBlock(int x, int y, int z, BlockType blockType);
}

public interface IWorldVoxelQuery
{
	Vector3I CenterPoint { get; }
	Vector3I FindSpawnPoint(Team team);

	Block GetBlock(Vector3I at);
	Block GetBlock(int x, int y, int z);

	ICollection<ChunkData> FindChangedChunks();
}

