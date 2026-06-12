using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.Tilemaps;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D;
using nkast.Aether.Physics2D.Common.Decomposition;
using nkast.Aether.Physics2D.Dynamics;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Serilog;

namespace Vinland.Core.Physic;

public class TiledPhysicsBuilder
{
    private const int PixelsPerMeter = 1;
    private const string CollisionLayerName = "collision";
    
    public void InitializeFromMap(World world, Tilemap tilemap)
    {
        /*if (!tilemap.Layers.OfType<TilemapObjectLayer>().)
        {
            Log.Warning("Collision layer '{LayerName}' not found in map '{MapName}'.", CollisionLayerName, tilemap.Name);
            return;
        }*/
        
        
        foreach (var objectGroup in tilemap.Layers.OfType<TilemapObjectLayer>())
        {
            if (objectGroup.Name == CollisionLayerName)
            {
                foreach (var obj in objectGroup.Objects)
                { 
                    
                    if (obj is TilemapPolygonObject polygonObj)
                    {   
                        Vertices vertices = new Vertices();
                        Vector2Aether basePosition = polygonObj.Position.ToAether(); 
                        var body = world.CreateBody();
                        body.BodyType = BodyType.Static;
                        body.Position = basePosition / PixelsPerMeter;
                        
                        foreach (var localPoint in polygonObj.Points)
                        {
                            vertices.Add(( localPoint.ToAether()) /  PixelsPerMeter);
                        }
                        
                        if (!vertices.IsCounterClockWise())
                        {
                            vertices.Reverse();
                        }
                        
                        CreatePolygonFixture(body, vertices);
                    }

                    // 2. Если это НЕЗАМКНУТАЯ ломаная линия (Polyline)
                    /*else if (obj is TiledmapPolylineObject polylineObj)
                    {
                        Vector2 basePosition = polylineObj.Position;

                        foreach (var localPoint in polylineObj.Points)
                        {
                            Vector2 globalPoint = basePosition + localPoint;
                
                            // Идеально для путей патрулирования врагов или цепочек платформ
                            System.Console.WriteLine($"Точка линии: {globalPoint.X}, {globalPoint.Y}");
                        }
                    }*/
                }
            }
            
        }
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
            var shapes = Triangulate.ConvexPartition(vert, TriangulationAlgorithm.Bayazit);
            foreach (var convexShape in shapes)
            {
                var shape = new PolygonShape(convexShape, 1f);
                body.CreateFixture(shape);
            }
        }
        
        Log.Information("Map collider is created");
    }
}