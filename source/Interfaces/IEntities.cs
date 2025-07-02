using System.Collections.Generic;
using BlockadeClassicPrivateServer.Shared;
using Godot;

namespace BlockadeClassicPrivateServer.Interfaces;

public interface IHasEntityLimit
{
	int MaxEntities { get; }
}

public interface IEntityQuery
{
	Entity? GetEntity(int index);

	int GetEntityCount();
	ICollection<Entity> GetEntities();
	// IEnumerable<Entity> GetEntitiesInRadius(Vector3 at, float radius);
}

public interface IEntityCommand
{
	Entity CreateEntity(EntityName name, int ownerPlayerIndex, Vector3 position, Vector3 rotation, Vector3 force, Vector3 torque);
	void RemoveEntity(int entityIndex);
}
