using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class EntityBrain()
{
    public void Update(Dictionary<long, Entity> entities)
    {
        SightProcess(entities);
    }

    private void SightProcess(Dictionary<long, Entity> entities)
    {
        if (entities == null) return;

        foreach (var origin in entities.Values)
        {
            if (origin == null) continue;
            if (origin.Kind == EntityKind.Player) continue;
            if (origin.VisibleEntities == null) continue;

            long? bestTargetId = null;
            float bestDistance = float.MaxValue;

            foreach (long targetId in origin.VisibleEntities)
            {
                if (!entities.TryGetValue(targetId, out Entity candidate))
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
                origin.State = EntityState.Follow;
            else if (origin.State == EntityState.Follow)
                origin.State = EntityState.Idle;
        }
    }
}