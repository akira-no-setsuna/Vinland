using Serilog;
using Vinland.Core.Infrastructure.Channels;

namespace Vinland.Core.Entities;

public class Perception(ChannelHub channelHub)
{
    public void Update(
        Dictionary<long, Entity> entities,
        Dictionary<(int, int), List<long>> entitiesMap)
    {
        var requests = new List<RayCast>(entities.Count);

        foreach (var entity in entities.Values)
        {
            var candidates = CollectCandidates(entity, entities, entitiesMap);

            if (candidates.Count == 0)
            {
                entity.VisibleEntities.Clear();
                continue;
            }

            requests.Add(new RayCast(entity.ID, candidates));
        }

        if (requests.Count > 0)
            channelHub.Core.Writer.TryWrite(new RayCastRequest(requests));
    }

    public void HandleResponse(
        Dictionary<long, Entity> entities,
        RayCastResponse response)
    {
        Log.Debug($"Perception Response: {response.Results.Count}");
        foreach (var result in response.Results)
        {
            if (!entities.TryGetValue(result.Origin, out var entity)) continue;

            entity.VisibleEntities.Clear();
            entity.VisibleEntities.AddRange(result.VisibleTargets);
        }
    }

    private static List<long> CollectCandidates(
        Entity self,
        Dictionary<long, Entity> entities,
        Dictionary<(int, int), List<long>> entitiesMap)
    {
        float r  = self.VisionRange;
        float r2 = r * r;
        var pos  = self.Position;
        long selfId = self.ID;

        var candidates = new List<long>();

        foreach (var cell in new CircleCells(pos, r))
        {
            if (!entitiesMap.TryGetValue(cell, out var ids)) continue;

            foreach (var id in ids)
            {
                if (id == selfId) continue;
                if (!entities.TryGetValue(id, out var other)) continue;
                if ((other.Position - pos).LengthSquared() < r2)
                    candidates.Add(id);
            }
        }

        return candidates;
    }
}