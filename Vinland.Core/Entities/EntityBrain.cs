using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class EntityBrain(ChannelHub channelHub)
{
    private Dictionary<long, Entity> _entities;
    public void Update(Dictionary<long, Entity> entities)
    {
        _entities = entities;
        
        if (entities == null) return;
        foreach (var entity in _entities.Values)
        {
            if (entity == null ||
                entity.Kind == EntityKind.Player) continue;
            
            SightProcess(entity);
            
        }
    }

    private void SightProcess(Entity origin)
    {
        if (origin.VisibleEntities == null) return;

        long? bestTargetId = null;
        float bestDistance = float.MaxValue;

        foreach (long targetId in origin.VisibleEntities)
        {
            if (!_entities.TryGetValue(targetId, out Entity candidate))
                continue;
            if (candidate == null) continue;
            if (candidate.Kind != EntityKind.Player) continue;

            float distance =
                (origin.Position - candidate.Position).LengthSquared();

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestTargetId = targetId;
            }
        }
            
        origin.Target = bestTargetId;

        if (bestTargetId != null)
            ChangeState(origin, EntityState.Follow);
        else if (origin.State == EntityState.Follow)
            ChangeState(origin, EntityState.Idle);
    }

    private void ChangeState(Entity entity, EntityState state)
    {
        entity.State = state;
        StateProcess(entity);
    }
    
    private void StateProcess(Entity entity)
    {
        switch (entity.State)
        {
            case EntityState.Idle:
                return;
            case EntityState.Follow:
                if (entity.Target != null)
                    channelHub.Core.Writer.TryWrite(new SetTarget(entity.ID, (long)entity.Target));
                break;
        }
    }
}