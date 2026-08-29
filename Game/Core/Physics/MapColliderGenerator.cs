using System;
using System.Linq;
using Game.Core.Infrastructure;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Tilemaps;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Common.Decomposition;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;

namespace Game.Core.Physics;

public class MapColliderGenerator
{
    private const string COLLISION_LAYER_NAME = "collision";
    private const int MIN_POLYGON_VERTICES = 3;
    private const int ELLIPSE_SEGMENTS = 16;
    private const float DENSITY = 1f;

    public void InitializeFromMap(World world, Tilemap tilemap)
    {
        var collisionLayer = tilemap.Layers
            .OfType<TilemapObjectLayer>()
            .FirstOrDefault(l => l.Name.Equals(COLLISION_LAYER_NAME, StringComparison.Ordinal));

        if (collisionLayer == null)
        {
            Log.Warning("Collision layer '{LayerName}' not found in map '{MapName}'.",
                COLLISION_LAYER_NAME, tilemap.Name);
            return;
        }

        var initializedCount = 0;
        var skippedCount = 0;

        foreach (var obj in collisionLayer.Objects)
            try
            {
                var created = obj switch
                {
                    TilemapPolygonObject polygon => TryCreatePolygonCollider(world, polygon),
                    TilemapRectangleObject rectangle => TryCreateRectangleCollider(world, rectangle),
                    TilemapEllipseObject ellipse => TryCreateEllipseCollider(world, ellipse),
                    TilemapPolylineObject polyline => TryCreatePolylineCollider(world, polyline),
                    _ => false
                };

                if (created)
                    initializedCount++;
                else
                    skippedCount++;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to create collider for object '{ObjName}' on map '{MapName}'.",
                    obj.Name, tilemap.Name);
                skippedCount++;
            }

        if (skippedCount > 0)
            Log.Warning("On map '{MapName}': {Initialized} initialized, {Skipped} skipped.",
                tilemap.Name, initializedCount, skippedCount);
        else
            Log.Information("On map '{MapName}': all {Count} colliders initialized successfully.",
                tilemap.Name, initializedCount);
    }

    private bool TryCreatePolygonCollider(World world, TilemapPolygonObject polygonObj)
    {
        if (polygonObj.Points == null || polygonObj.Points.Length < MIN_POLYGON_VERTICES)
        {
            Log.Warning("Polygon object has too few vertices ({Count}), skipping.",
                polygonObj.Points?.Length ?? 0);
            return false;
        }

        var vertices = new Vertices(polygonObj.Points.Length);

        foreach (var localPoint in polygonObj.Points)
            vertices.Add(localPoint.ToWorld());

        if (!vertices.IsCounterClockWise())
            vertices.Reverse();

        var body = CreateStaticBody(world, polygonObj.Position, polygonObj.Rotation);
        CreatePolygonFixture(body, vertices);

        return true;
    }

    private bool TryCreateRectangleCollider(World world, TilemapRectangleObject rectObj)
    {
        var bounds = rectObj.Bounds;

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            Log.Warning("Rectangle object has invalid size ({Width}x{Height}), skipping.",
                bounds.Width, bounds.Height);
            return false;
        }

        var w = bounds.Width / PhysicsScale.PIXELS_PER_METER;
        var h = bounds.Height / PhysicsScale.PIXELS_PER_METER;

        // Вершины задаются относительно начала координат тела (top-left pivot)
        var vertices = new Vertices(4)
        {
            new Vector2(0f, 0f),
            new Vector2(w, 0f),
            new Vector2(w, h),
            new Vector2(0f, h)
        };

        if (!vertices.IsCounterClockWise())
            vertices.Reverse();

        var body = CreateStaticBody(world, rectObj.Position, rectObj.Rotation);
        CreatePolygonFixture(body, vertices);

        return true;
    }

    private bool TryCreateEllipseCollider(World world, TilemapEllipseObject ellipseObj)
    {
        var bounds = ellipseObj.Bounds;

        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            Log.Warning("Ellipse object has invalid size ({Width}x{Height}), skipping.",
                bounds.Width, bounds.Height);
            return false;
        }

        var halfWidth = bounds.Width * 0.5f / PhysicsScale.PIXELS_PER_METER;
        var halfHeight = bounds.Height * 0.5f / PhysicsScale.PIXELS_PER_METER;

        var vertices = new Vertices(ELLIPSE_SEGMENTS);

        for (var i = 0; i < ELLIPSE_SEGMENTS; i++)
        {
            var angle = MathHelper.TwoPi * i / ELLIPSE_SEGMENTS;

            // Смещаем эллипс так, чтобы его центр совпадал с центром bounding box'а объекта
            vertices.Add(new Vector2(
                halfWidth + halfWidth * MathF.Cos(angle),
                halfHeight + halfHeight * MathF.Sin(angle)));
        }

        if (!vertices.IsCounterClockWise())
            vertices.Reverse();

        var body = CreateStaticBody(world, ellipseObj.Position, ellipseObj.Rotation);
        CreatePolygonFixture(body, vertices);

        return true;
    }

    private bool TryCreatePolylineCollider(World world, TilemapPolylineObject polylineObj)
    {
        if (polylineObj.Points == null || polylineObj.Points.Length < 2)
        {
            Log.Warning("Polyline object has too few vertices ({Count}), skipping.",
                polylineObj.Points?.Length ?? 0);
            return false;
        }

        var vertices = new Vertices(polylineObj.Points.Length);

        foreach (var localPoint in polylineObj.Points)
            vertices.Add(localPoint.ToWorld());

        var body = CreateStaticBody(world, polylineObj.Position, polylineObj.Rotation);

        // В Aether.Physics2D 2.5.0 ChainShape.CreateChain может отсутствовать,
        // поэтому используем надежную цепочку из EdgeShape
        for (var i = 0; i < vertices.Count - 1; i++)
            body.CreateFixture(new EdgeShape(vertices[i], vertices[i + 1]));

        return true;
    }

    private Body CreateStaticBody(World world, Vector2 pixelPosition, float rotationDegrees)
    {
        var body = world.CreateBody();

        body.BodyType = BodyType.Static;
        body.Position = pixelPosition.ToWorld();

        // Tiled: градусы, по часовой стрелке
        // Aether: радианы, против часовой стрелки
        body.Rotation = -MathHelper.ToRadians(rotationDegrees);

        return body;
    }

    private void CreatePolygonFixture(Body body, Vertices vertices)
    {
        if (vertices.IsConvex())
            body.CreateFixture(new PolygonShape(vertices, DENSITY));
        else
            foreach (var convexPart in Triangulate.ConvexPartition(vertices, TriangulationAlgorithm.Bayazit))
                body.CreateFixture(new PolygonShape(convexPart, DENSITY));
    }
}