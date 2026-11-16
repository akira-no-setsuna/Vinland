
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Serilog;
using Vinland.Core.Connectors;
using Vinland.Core.Infrastructure.Channels;
using Vector2 = System.Numerics.Vector2;

namespace Vinland.Godot.Connector;

public class PhysicsManager(Node2D level, Dictionary<long, CharacterBody2D> entitiesBody)
{
    public void SetVelocity(SetVelocity cmd)
    {
        if (!entitiesBody.TryGetValue(cmd.EntityID, out var entityBody))
        {
            Log.Debug(
                "Skipping {Command}: entity {EntityID} not found in body registry ",
                nameof(SetVelocity), cmd.EntityID);
            return;
        }

        if (!GodotObject.IsInstanceValid(entityBody))
        {
            Log.Warning(
                "Skipping {Command}: entity {EntityID} is present in registry but its ",
                nameof(SetVelocity), cmd.EntityID);
            return;
        }
        entityBody.Velocity = cmd.Velocity.ToGodot();
    }
    
    public RayCastResponse HandleRayCasts(RayCastRequest request)
    {
        var spaceState = level.GetWorld2D().DirectSpaceState;

        var results = new List<RayCastResult>(request.Requests.Count);

        foreach (var rc in request.Requests)
        {
            if (!entitiesBody.TryGetValue(rc.Origin, out var originBody) ||
                !GodotObject.IsInstanceValid(originBody))
                continue;

            var from = originBody.GlobalPosition;
            var visible = new List<long>(rc.Targets.Count);

            foreach (var targetId in rc.Targets)
            {
                if (!entitiesBody.TryGetValue(targetId, out var targetBody) ||
                    !GodotObject.IsInstanceValid(targetBody))
                    continue;

                var to = targetBody.GlobalPosition;
                    
                if (from.DistanceSquaredTo(to) < 1f)
                {
                    visible.Add(targetId);
                    continue;
                }

                var query = PhysicsRayQueryParameters2D.Create(from, to);
                
                query.Exclude = [originBody.GetRid()];
                query.CollideWithAreas  = false;
                query.CollideWithBodies = true;

                var hit = spaceState.IntersectRay(query);

                if (hit.Count == 0)
                {
                    visible.Add(targetId);
                }
                else if (hit["rid"].AsRid() == targetBody.GetRid())
                {
                    visible.Add(targetId);
                }
            }

            results.Add(new RayCastResult(rc.Origin, visible));
        }

        return new RayCastResponse(results);
    }
}