using System;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Voxel;

public partial class World
{
	private class Chunk
	{
		public const int Size = 16;

		private readonly Block[,,] _blocks = new Block[Size, Size, Size];

		public Vector3I Position { get; set; } = Vector3I.Zero;
		public bool IsDirty { get; private set; } = false;

		public Chunk()
		{
			for (var x = 0; x < Size; x++)
			{
				for (var y = 0; y < Size; y++)
				{
					for (var z = 0; z < Size; z++)
					{
						_blocks[x, y, z] = new() { Type = BlockType.None, Health = 0 };
					}
				}
			}
		}

		public ChunkData ToChunkData()
		{
			byte[] blockTypesArray = new byte[Size * Size * Size];

			int i = 0;
			for (var x = 0; x < Size; x++)
			{
				for (var z = 0; z < Size; z++)
				{
					for (var y = 0; y < Size; y++)
					{
						blockTypesArray[i] = (byte)_blocks[x, y, z].Type;
						i++;
					}
				}
			}

			return new(Position, Size, blockTypesArray);
		}

		public ref Block GetBlockRef(Vector3I at) => ref _blocks[at.X, at.Y, at.Z];
		public ref Block GetBlockRef(int x, int y, int z) => ref _blocks[x, y, z];

		public Block GetBlock(Vector3I at) => _blocks[at.X, at.Y, at.Z];
		public Block GetBlock(int x, int y, int z) => _blocks[x, y, z];

		public int DamageBlock(Vector3I at, int amount) => DamageBlock(at.X, at.Y, at.Z, amount);
		public int DamageBlock(int x, int y, int z, int amount)
		{
			IsDirty = true;

			ref Block block = ref GetBlockRef(x, y, z);
			block.Health = (byte)Math.Max(0, block.Health - amount);

			if (block.Health <= 0)
				DestroyBlock(x, y, z);

			GD.Print($"[VOXEL] Damaged {block.Type} ({x}, {y}, {z})@{Position} by {amount} hp. HP now: {block.Health}.");

			return block.Health;
		}

		public void DestroyBlock(Vector3I at) => DestroyBlock(at.X, at.Y, at.Z);
		public void DestroyBlock(int x, int y, int z) => SetBlock(x, y, z, BlockType.None);

		public void SetBlock(Vector3I at, BlockType blockType) => SetBlock(at.X, at.Y, at.Z, blockType);
		public void SetBlock(int x, int y, int z, BlockType blockType)
		{
			IsDirty = true;

			ref Block block = ref _blocks[x, y, z];
			block.Type = blockType;
			block.Health = (byte)(blockType == BlockType.None ? 0 : Block.MaxHealth);
		}
	}
}
