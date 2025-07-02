using BlockadeClassicPrivateServer.Shared;

namespace BlockadeClassicPrivateServer.Voxel;

public struct Block()
{
	public const int MaxHealth = 100;

	public BlockType Type { get; set; } = BlockType.None;
	public byte Health { get; set; } = MaxHealth;
}
