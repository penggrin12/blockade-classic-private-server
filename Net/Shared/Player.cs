using Godot;

namespace BlockadeClassicPrivateServer.Net.Shared;

public partial class Player : Resource
{
	public const int MaxHealth = 100;

	// those cant be { get; init; } because then godot complains
	// TODO: does it even need to have Exports for proper signal serialization?

	[Export] public bool Connected { get; set; } = true;

	[Export] public required int Index { get; set; }
	[Export] public required string Name { get; set; }
	[Export] public string Clan { get; set; } = "";
	[Export] public Flag Country { get; set; } = Flag.Xx; // TODO: verify that it uses Flag values
	[Export] public Network Network { get; set; } = Network.ST;

	[Export] public Team Team { get; set; } = Team.Blue;
	[Export] public int Kills { get; set; } = 0;
	[Export] public int Deaths { get; set; } = 0;

	[Export] public bool IsSpawned { get; set; } = false;
	public bool IsAlive => Health > 0;
	[Export] public int Health { get; set; } = 0;
	[Export] public int Armor { get; set; } = 100;
	[Export] public bool HasHelmet { get; set; } = true;

	[Export] public ItemName WeaponMelee { get; set; } = ItemName.Shovel;
	[Export] public ItemName WeaponPrimary { get; set; } = ItemName.None;
	[Export] public ItemName WeaponSecondary { get; set; } = ItemName.None;

	[Export] public ItemName WeaponUtility1 { get; set; } = ItemName.None;
	[Export] public ItemName WeaponUtility2 { get; set; } = ItemName.None;
	[Export] public ItemName WeaponUtility3 { get; set; } = ItemName.None;
	[Export] public ItemName WeaponGrenade1 { get; set; } = ItemName.None;
	[Export] public ItemName WeaponGrenade2 { get; set; } = ItemName.None;

	[Export] public ItemName CurrentWeapon { get; set; } = ItemName.Shovel;

	[Export] public int AmmoPrimary { get; set; } = 0;
	[Export] public int AmmoSecondary { get; set; } = 0;

	// TODO: those should be set dynamically somehow
	[Export] public int AmmoUtility1 { get; set; } = 1;
	[Export] public int AmmoUtility2 { get; set; } = 2;
	[Export] public int AmmoUtility3 { get; set; } = 1;
	[Export] public int AmmoGrenade1 { get; set; } = 2;
	[Export] public int AmmoGrenade2 { get; set; } = 1;

	[Export] public Vector3 Position { get; set; } = new Vector3(15, 50, 15);
	[Export] public Vector2 Angle { get; set; } = Vector2.Zero;
	[Export] public PlayerMovementState MovementState { get; set; } = PlayerMovementState.StandingUp;

	public override bool Equals(object? obj)
	{
		if (obj is Player otherPlayer)
			return this.Index == otherPlayer.Index;

		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Index.GetHashCode();
	}

	public override string ToString()
	{
		return $"<Player {Index} ({Name})>";
	}
}

public enum PlayerMovementState : byte
{
	StandingUp = 0, // includes walking
	Sprinting = 1,
	Crouching = 2,
}
