using System.Threading.Tasks;
using BlockadeClassicPrivateServer.Interfaces;
using Godot;

namespace BlockadeClassicPrivateServer.Net.Shared;

public sealed class Entity(IPacketSender packetSender, IGameManager gameManager, IPlayerQuery playerQuery, IEntityCommand entityCommand, IGameLogic gameLogic)
{
	[Export] public required int Index { get; set; }
	[Export] public required int OwnerPlayerIndex { get; set; }
	[Export] public required EntityName Name { get; set; }

	[Export] public required Vector3 Position { get; set; } = Vector3.Zero;
	[Export] public required Vector3 Rotation { get; set; } = Vector3.Zero;
	[Export] public required Vector3 Force { get; set; } = Vector3.Zero;
	[Export] public required Vector3 Torque { get; set; } = Vector3.Zero;

	public override bool Equals(object? obj)
	{
		if (obj is Entity otherEntity)
			return this.Index == otherEntity.Index;

		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return Index.GetHashCode();
	}

	public override string ToString()
	{
		return $"<{Name} ({Index})";
	}

	// TODO: move all of that to GameLogic

	private async Task GazSmokeDamage()
	{
		Callable gazDamager = Callable.From(() =>
		{
			// TODO: it should also check for the player's skin... but i don't like that
			foreach (Player player in playerQuery.GetPlayersInRadius(Position, 8))
				gameLogic.DamagePlayer(player, playerQuery.GetPlayer(OwnerPlayerIndex), 20, ItemName.M7a2);
		});

		await Task.Delay(2500);
		gameManager.ScoreUpdateTimer.CallThreadSafe(Timer.MethodName.Connect, Timer.SignalName.Timeout, gazDamager);
		await Task.Delay(20000);
		gameManager.ScoreUpdateTimer.CallThreadSafe(Timer.MethodName.Disconnect, Timer.SignalName.Timeout, gazDamager);
		entityCommand.RemoveEntity(Index);
	}

	public void OnSpawn()
	{
		// TODO: also despawn regular smokes
		switch (Name)
		{
			case EntityName.Molotov:
			case EntityName.Gaz_Grenade:
				_ = GazSmokeDamage();
				break;
		}
	}

	public void OnExplode()
	{
		Packet explodePacket = Packet.Create(PacketName.Explode);
		explodePacket.WriteVectorUShort(Position);
		packetSender.BroadcastPacket(explodePacket);

		entityCommand.RemoveEntity(Index);

		const float DamageScale = 1.5f;  // ← Increase this to make *all* explosions hit harder
		ItemName itemName;
		int baseDamage;
		float radius;

		switch (Name)
		{
			case EntityName.Grenade:
				radius = 13f;
				baseDamage = 110;
				itemName = ItemName.M61;
				break;
			case EntityName.He_Grenade: // Mk3
				radius = 13f;
				baseDamage = 120;
				itemName = ItemName.Mk3;
				break;
			case EntityName.Rkg3:
				radius = 13f;
				baseDamage = 130;
				itemName = ItemName.Rkg3;
				break;
			case EntityName.Stielhandgranate:
				radius = 13f;
				baseDamage = 120;
				itemName = ItemName.Stielhandgranate;
				break;
			case EntityName.Shmel:
				radius = 6f;
				baseDamage = 150;
				itemName = ItemName.Shmel;
				break;
			case EntityName.Rpg:
				radius = 6f;
				baseDamage = 150;
				itemName = ItemName.Rpg;
				break;
			case EntityName.M202:
				radius = 5f;
				baseDamage = 100;
				itemName = ItemName.M202;
				break;
			case EntityName.Minefly: // mortar shell
				radius = 16f;
				baseDamage = 140;
				itemName = ItemName.Mortar;
				break;
			case EntityName.C4: // mortar shell
				radius = 12f;
				baseDamage = 120;
				itemName = ItemName.C4;
				break;
			default:
				return;
		}

		Player? ownerPlayer = playerQuery.GetPlayer(OwnerPlayerIndex);

		// quadratic fall‑off: damage = baseDamage x (clamp01(1 - d/radius))^2
		foreach (Player victim in playerQuery.GetPlayersInRadius(Position, radius))
		{
			float distance = Position.DistanceTo(victim.Position);
			float factor   = Mathf.Clamp(1f - (distance / radius), 0f, 1f);
			int damage     = Mathf.RoundToInt((baseDamage * DamageScale) * (factor * factor));

			gameLogic.DamagePlayer(
				victim,
				ownerPlayer,
				damage,
				itemName
			);
		}
	}
}
