using System;
using Godot;

namespace BlockadeClassicPrivateServer.Shared;

public sealed class Player : IEquatable<Player>
{
	public const int MaxHealth = 100;

	public bool Connected { get; set; } = true;

	public required int Index { get; init; }
	public required string Name { get; init; }
	public string Clan { get; set; } = "";
	public Flag Country { get; set; } = Flag.Xx;
	public Network Network { get; set; } = Network.ST;

	public Team Team { get; set; } = Team.Blue;
	public int Kills { get; set; } = 0;
	public int Deaths { get; set; } = 0;

	public bool IsSpawned { get; set; } = false;
	public bool IsAlive => Health > 0;
	public int Health { get; set; } = 0;
	public int Armor { get; set; } = 100;
	public bool HasHelmet { get; set; } = true;

	public ItemName WeaponMelee { get; set; } = ItemName.Shovel;
	public ItemName WeaponPrimary { get; set; } = ItemName.M14;
	public ItemName WeaponSecondary { get; set; } = ItemName.Glock;

	public ItemName WeaponUtility1 { get; set; } = ItemName.None;
	public ItemName WeaponUtility2 { get; set; } = ItemName.None;
	public ItemName WeaponUtility3 { get; set; } = ItemName.None;
	public ItemName WeaponGrenade1 { get; set; } = ItemName.None;
	public ItemName WeaponGrenade2 { get; set; } = ItemName.None;

	public ItemName CurrentWeapon { get; set; } = ItemName.Shovel;

	public int AmmoPrimary { get; set; } = 0;
	public int AmmoSecondary { get; set; } = 0;

	// TODO: those should be set dynamically somehow
	public int AmmoUtility1 { get; set; } = 1;
	public int AmmoUtility2 { get; set; } = 2;
	public int AmmoUtility3 { get; set; } = 1;
	public int AmmoGrenade1 { get; set; } = 2;
	public int AmmoGrenade2 { get; set; } = 1;

	public Vector3 Position { get; set; } = new Vector3(15, 50, 15);
	public Vector2 Angle { get; set; } = Vector2.Zero;
	public PlayerMovementState MovementState { get; set; } = PlayerMovementState.StandingUp;

	public bool Equals(Player? other)
    {
        return other is not null && Index == other.Index;
    }

    public override bool Equals(object? obj) => Equals(obj as Player);

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
