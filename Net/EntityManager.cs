using System;
using System.Collections.Generic;
using BlockadeClassicPrivateServer.Interfaces;
using BlockadeClassicPrivateServer.Net.Shared;
using Godot;

namespace BlockadeClassicPrivateServer;

public class EntityManager(IPacketSender packetSender, IGameManager gameManager, IGameLogic gameLogic, IPlayerQuery playerQuery) : IHasEntityLimit, IEntityQuery, IEntityCommand
{
	private const int _MaxEntities = 32;
	public int MaxEntities => _MaxEntities;

	private Dictionary<int, Entity> Entities { get; init; } = [];

	private int FindFreeEntityIndex()
	{
		for (var resultEntityIndex = 0; resultEntityIndex < MaxEntities; resultEntityIndex++)
			if (!Entities.ContainsKey(resultEntityIndex)) return resultEntityIndex;

		throw new IndexOutOfRangeException($"Couldn't find a new entity index. Too many entities online? ({Entities.Count}/{MaxEntities})");
	}

	public int GetEntityCount() => Entities.Count;
	public ICollection<Entity> GetEntities() => Entities.Values;

	public Entity? GetEntity(int entityIndex)
	{
		Entities.TryGetValue(entityIndex, out Entity? entity);
		return entity;
	}

	public void RemoveEntity(int entityIndex)
	{
		Entities.Remove(entityIndex);
		GD.Print($"[ NET ] Entity ({entityIndex}) was removed.");

		Packet destroyEntity = Packet.Create(PacketName.DestroyEntity);
		destroyEntity.Write<int>(entityIndex);
		packetSender.BroadcastPacket(destroyEntity);
	}

	public Entity CreateEntity(EntityName name, int ownerPlayerIndex, Vector3 position, Vector3 rotation, Vector3 force, Vector3 torque)
	{
		Entity entity = new(packetSender, gameManager, playerQuery, this, gameLogic)
		{
			Index = FindFreeEntityIndex(),
			OwnerPlayerIndex = ownerPlayerIndex,
			Name = name,

			Position = position,
			Rotation = rotation,
			Force = force,
			Torque = torque,
		};
		Entities.Add(entity.Index, entity);

		GD.Print($"[ NET ] {ownerPlayerIndex} created a {name} ({entity.Index}).");

		Packet createEntityPacket = Packet.Create(PacketName.CreateEntity);
		createEntityPacket.Write<byte>((byte)entity.OwnerPlayerIndex);
		createEntityPacket.Write<int>(entity.Index);
		createEntityPacket.Write<byte>((byte)entity.Name);

		createEntityPacket.WriteVectorUShort(entity.Position);
		createEntityPacket.WriteVectorFloat(entity.Rotation);
		createEntityPacket.WriteVectorFloat(entity.Force);
		createEntityPacket.WriteVectorFloat(entity.Torque);
		packetSender.BroadcastPacket(createEntityPacket);

		// entity.OnSpawn(this);

		return entity;
	}
}
