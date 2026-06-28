using System.Linq;
using MonoGame.Extended.Tilemaps;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Common.Decomposition;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;
using Vinland.Core;

namespace Game.Core.Physics;

public class MapColliderGenerator
{
    private const string COLLISION_LAYER_NAME = "collision";
    private const int MIN_POLYGON_VERTICES = 3;
    
    public void InitializeFromMap(World world, Tilemap tilemap)
    {
        
        // Trying to find the collision layer
        var collisionLayer = tilemap.Layers
            .OfType<TilemapObjectLayer>()
            .FirstOrDefault(l => l.Name.Equals(COLLISION_LAYER_NAME));

        if (collisionLayer == null)
        {
            Log.Warning("Collision layer '{LayerName}' not found in map '{MapName}'.",
                COLLISION_LAYER_NAME, tilemap.Name);
            return;
        }
        
        int initializedCount = 0;
        int skippedCount = 0;
        
        foreach (var obj in collisionLayer.Objects)
        {
            try
            {
                switch (obj)
                {
                    case TilemapPolygonObject polygonObject:
                        if (!TryCreatePolygonCollider(world, polygonObject)) skippedCount++;
                        else initializedCount++;
                        break;
                    default:
                        //TODO: Add support for other collider types
                        Log.Debug("Unsupported collider type '{Type}' on map '{MapName}'.",
                            obj.GetType().Name, tilemap.Name);
                        skippedCount++;
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to create collider for object on map '{MapName}'.", tilemap.Name);
                skippedCount++;
            }
        }
        
        if (skippedCount > 0)
        {
            Log.Warning("On map '{MapName}': {Initialized} initialized, {Skipped} skipped.",
                tilemap.Name, initializedCount, skippedCount);
        }
        else
        {
            Log.Information("On map '{MapName}': all {Count} colliders initialized successfully.",
                tilemap.Name, initializedCount);
        }
    }

    private bool TryCreatePolygonCollider(World world, TilemapPolygonObject polygonObj)
    {
        if (polygonObj.Points == null || polygonObj.Points.Length< MIN_POLYGON_VERTICES)
        {
            Log.Warning("Polygon object has too few vertices ({Count}), skipping.",
                polygonObj.Points?.Length ?? 0);
            return false;
        }
        
        var vertices = new Vertices(polygonObj.Points.Length);
        foreach (var localPoint in polygonObj.Points)
        {
            vertices.Add(localPoint.ToAether());
        }
        
        // Aether Physics requires CCW polygon orientation
        if (!vertices.IsCounterClockWise())
        {
            vertices.Reverse();
        }

        var body = world.CreateBody(
            bodyType: BodyType.Static,
            position: polygonObj.Position.ToAether());
        
        CreatePolygonFixture(body, vertices);
        return true;

    }
    private void CreatePolygonFixture(Body body, Vertices vert)
    {
        if (vert.IsConvex())
        {
            var shape = new PolygonShape(vert, 1f);
            body.CreateFixture(shape);
        }
        else
        {
            foreach (var convexShape in Triangulate.ConvexPartition(vert, TriangulationAlgorithm.Bayazit))
            {
                body.CreateFixture(new PolygonShape(convexShape, 1f));
            }
        }
    }
}