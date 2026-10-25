using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public sealed class EntityManager(ChannelHub channelHub)
{
    public long CurrentEntityID { get; private set; } = 1;

    private readonly Dictionary<long, Entity> _entities = new(4096);
    private readonly Dictionary<(int, int), List<long>> _entitiesMap = new(4096);

    /// <summary>Creates a new entity and automatically assigns a unique identifier.</summary>
    public long Spawn(SpawnCommand cmd)
    {
        var id = CurrentEntityID++;
        var entity = new Entity { 
            ID = id,
            Position = cmd.Position,
            Kind = cmd.Kind,
        };

        if (!_entities.TryAdd(entity.ID, entity))
        {
            Log.Error("ID collision {ID} on spawn", entity.ID);
            return -1;
        }
        AddToCell(entity.Position.GetCell(), entity.ID);
        channelHub.Core.Writer.TryWrite(cmd with { EntityID = entity.ID });
        
        Log.Information("Spawn entity {entity}", entity.ID);
        return entity.ID;
    }

    /// <summary>Restores a created entity</summary>
    public long Restore(Entity entity)
    {
        if (!_entities.TryAdd(entity.ID, entity))
        {
            Log.Warning("Duplicate entity {ID} on restore, skipped", entity.ID);
            return -1;
        }
        AddToCell(entity.Position.GetCell(), entity.ID);
        if (entity.ID >= CurrentEntityID)
            CurrentEntityID = entity.ID + 1;

        channelHub.Core.Writer.TryWrite(new SpawnCommand(entity.ID, entity.Kind, entity.Position));
        
        Log.Information("Restore entity {entity}", entity.ID);
        return entity.ID;
    }

    public void UpdatePositions(long id, Vector2 position)
    {
        if (!_entities.TryGetValue(id, out var entity))
        {
            Log.Warning("Entity {ID} not found, cannot set position", id);
            return;
        }
            
        var oldCell = entity.Position.GetCell();
        var newCell = position.GetCell();

        if (oldCell == newCell) { entity.Position = position; return; }

        RemoveFromCell(oldCell, id);
        AddToCell(newCell, id);
        entity.Position = position;
    }

    public void DespawnEntity(long id)
    {
        if (!_entities.Remove(id, out var entity)) return;
        RemoveFromCell(entity.Position.GetCell(), id);
        
        Log.Information("Despawn entity {entity}", entity.ID);
    }

    private void AddToCell((int, int) cell, long id)
    {
        if (!_entitiesMap.TryGetValue(cell, out var list))
        {
            list = new List<long>(4);
            _entitiesMap[cell] = list;
        }
        list.Add(id);
    }

    private void RemoveFromCell((int, int) cell, long id)
    {
        if (!_entitiesMap.TryGetValue(cell, out var list)) return;

        int idx = list.IndexOf(id);
        if (idx < 0) return;
        
        list[idx] = list[^1];
        list.RemoveAt(list.Count - 1);

        if (list.Count == 0) _entitiesMap.Remove(cell);
    }
}

    