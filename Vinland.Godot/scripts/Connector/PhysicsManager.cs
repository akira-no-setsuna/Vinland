
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Vinland.Core.Connectors;
using Vinland.Core.Infrastructure.Channels;
using Vector2 = System.Numerics.Vector2;

namespace Vinland.Godot.Connector;

public class PhysicsManager(Node2D worldNode)
{
    private readonly PhysicsRayQueryParameters2D _query = new (); 
    
    private readonly Lock _velocityLock = new();
    private List<long> _velIds = new ();
    private List<Vector2> _velocities = new();
    
    private readonly ConcurrentQueue<RayCastTask> _rayCastQueue = new();
    
    private readonly Lock _snapshotLock = new();
    private PositionSnapshot _latestSnapshot = new(new List<long>(), new List<Vector2>());

    public void ApplyVelocities(List<long> entityIds, List<Vector2> velocities)
    {
        lock (_velocityLock)
        {
            _velIds = entityIds;
            _velocities = velocities;
        }
    }

    public Task<List<bool>> RayCastAsync(Vector2 from, List<Vector2> to, uint collisionMask = 1)
    {
        var tcs = new TaskCompletionSource<List<bool>>();
        _rayCastQueue.Enqueue(new RayCastTask(from, to, collisionMask, tcs));
        return tcs.Task;
    }

    public PositionSnapshot GetPositionSnapshot()
    {
        lock (_snapshotLock)
        {
            return _latestSnapshot;
        }
    }
    
    public void Update(Dictionary<long, CharacterBody2D> entities)
    {
        ApplyPendingVelocities(entities);
        ProcessPendingRayCasts();
        PublishSnapshot(entities);
    }

    private void ApplyPendingVelocities(Dictionary<long, CharacterBody2D> entities)
    {
        List<long> ids;
        List<Vector2> vels;

        lock (_velocityLock)
        {
            ids = _velIds;
            vels = _velocities;
            
            _velIds = null;
            _velocities = null;
        }

        if (ids is null || vels is null) return;
        
        for (int i = 0; i < ids.Count; i++)
        {
            if (!entities.TryGetValue(ids[i], out var entity)) continue;
            if (!GodotObject.IsInstanceValid(entity)) continue;
            
            entity.Velocity = vels[i].ToGodot();
            entity.MoveAndSlide();
        }
    }

    private void ProcessPendingRayCasts()
    {
        var space = worldNode.GetWorld2D().DirectSpaceState;

        while (_rayCastQueue.TryDequeue(out var item))
        {
            var results = new List<bool>();
            
            _query.From = item.From.ToGodot();
            _query.CollisionMask = item.CollisionMask;

            for (int i = 0; i < item.To.Count; i++)
            {
                _query.To = item.To[i].ToGodot(); 
                results.Add(space.IntersectRay(_query).Count > 0); 
            }
        
            item.Completion.SetResult(results);
        }
    }

    private void PublishSnapshot(Dictionary<long, CharacterBody2D> entities)
    {
        var ids = new List<long>();
        var pos = new List<Vector2>();

        foreach (var entity in entities)
        {
            ids.Add(entity.Key);
            pos.Add(entity.Value.Position.ToNet());
        }
        
        var newSnapshot = new PositionSnapshot(ids, pos);
        lock (_snapshotLock)
        {
            _latestSnapshot = newSnapshot;
        }
    }

    private record RayCastTask(Vector2 From, List<Vector2> To, uint CollisionMask, TaskCompletionSource<List<bool>> Completion);
}