
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
    private const float MIN_RAYCAST_DISTANCE_SQ = 0.001f;
    
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

        var skippedOrigins  = 0;
        var skippedTargets  = 0;
        var degenerateRays  = 0;
        var raysCast        = 0;

        foreach (var rc in request.Requests)
        {
            if (!entitiesBody.TryGetValue(rc.Origin, out var originBody))
            {
                skippedOrigins++;
                continue;
            }

            if (!GodotObject.IsInstanceValid(originBody))
            {
                skippedOrigins++;
                Log.Warning(
                    "RayCast origin {EntityID} present in registry but Godot body is " +
                    "invalid (null or freed) — possible lifecycle leak; skipping whole request",
                    rc.Origin);
                continue;
            }

            var from = originBody.GlobalPosition;
            var visible = new List<long>(rc.Targets.Count);

            foreach (var targetId in rc.Targets)
            {
                if (!entitiesBody.TryGetValue(targetId, out var targetBody))
                {
                    skippedTargets++;
                    continue;
                }

                if (!GodotObject.IsInstanceValid(targetBody))
                {
                    skippedTargets++;
                    Log.Warning(
                        "RayCast target {EntityID} present in registry but Godot body is " +
                        "invalid (null or freed) — possible lifecycle leak; treating as not visible",
                        targetId);
                    continue;
                }

                var to = targetBody.GlobalPosition;

                // Too close to trust a raycast (degenerate / self-intersection risk) —
                // treat as visible without a physics query.
                if (from.DistanceSquaredTo(to) <= MIN_RAYCAST_DISTANCE_SQ)
                {
                    degenerateRays++;
                    visible.Add(targetId);
                    continue;
                }

                var query = PhysicsRayQueryParameters2D.Create(from, to);
                query.Exclude           = [originBody.GetRid()];
                query.CollideWithAreas  = false;
                query.CollideWithBodies = true;
                
                raysCast++;
                var hit = spaceState.IntersectRay(query);

                if (hit.Count == 0 || hit["rid"].AsRid() == targetBody.GetRid())
                    visible.Add(targetId);
            }

            results.Add(new RayCastResult(rc.Origin, visible));
        }
        
        if (skippedOrigins > 0 || skippedTargets > 0 || degenerateRays > 0)
        {
            Log.Debug(
                "RayCast batch: {RaysCast} rays, {Requests} requests, " +
                "{SkippedOrigins} origins skipped, {SkippedTargets} targets skipped, " +
                "{Degenerate} degenerate (too close)",
                raysCast, request.Requests.Count, skippedOrigins, skippedTargets, degenerateRays);
        }

        return new RayCastResponse(results);
    }
}