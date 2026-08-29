using System;
using Game.Core.Infrastructure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;

namespace Game.Core.Application.Render;

/// <summary>
///     Гладкая отрисовка физических тел (полигонов) для отладки.
///     Все координаты физики (метры) автоматически масштабируются в пиксели через PPM.
/// </summary>
public class PhysicsDebugRenderer
{
    // 🔑 КЛЮЧЕВАЯ КОНСТАНТА: должна совпадать с PPM в Game1 и MapColliderGenerator
    private const float PPM = PhysicsScale.PIXELS_PER_METER;

    // Более контрастные цвета для отладки
    private static readonly Color StaticColor = new(0, 255, 0, 180); // Ярко-зелёный (стены)
    private static readonly Color DynamicColor = new(255, 50, 50, 220); // Ярко-красный (игрок/враги)
    private static readonly Color KinematicColor = new(50, 150, 255, 220); // Ярко-синий
    private readonly Texture2D _pixel;
    private readonly SpriteBatch _spriteBatch;

    public PhysicsDebugRenderer(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        _spriteBatch = spriteBatch;
        _pixel = CreatePixelTexture(graphicsDevice);
    }

    private Texture2D CreatePixelTexture(GraphicsDevice gd)
    {
        var tex = new Texture2D(gd, 1, 1);
        tex.SetData([Color.White]);
        return tex;
    }

    public void Draw(BodyCollection bodyList, Matrix viewMatrix)
    {
        if (bodyList == null) return;

        foreach (var body in bodyList)
        {
            var color = body.BodyType switch
            {
                BodyType.Static => StaticColor,
                BodyType.Dynamic => DynamicColor,
                BodyType.Kinematic => KinematicColor,
                _ => Color.White
            };

            foreach (var fixture in body.FixtureList)
                if (fixture.Shape is PolygonShape poly)
                    DrawPolygon(body, poly.Vertices, color);
                else if (fixture.Shape is CircleShape circle) DrawCircle(body, circle, color);

            // Рисуем маленький крестик в центре масс тела (в пикселях)
            DrawCross(body.Position.ToScreen(), 3, Color.Yellow);
        }
    }

    private void DrawPolygon(Body body, Vertices vertices, Color color)
    {
        for (var i = 0; i < vertices.Count; i++)
        {
            var next = (i + 1) % vertices.Count;

            // 🔑 Переводим локальные вершины в мировые координаты (метры) → пиксели
            var v1 = body.GetWorldPoint(vertices[i]).ToScreen();
            var v2 = body.GetWorldPoint(vertices[next]).ToScreen();

            // Толщина линии 2.0f — в экранных пикселях, не масштабируется
            DrawLine(v1, v2, color, 2.0f);
        }
    }

    private void DrawCircle(Body body, CircleShape circle, Color color)
    {
        // 🔑 Центр и радиус переводим в пиксели
        var center = body.Position.ToScreen();
        var radiusPixels = circle.Radius * PPM;
        var segments = 16;

        for (var i = 0; i < segments; i++)
        {
            var angle1 = (float)(i * 2.0 * Math.PI / segments);
            var angle2 = (float)((i + 1) * 2.0 * Math.PI / segments);

            var v1 = center + new Vector2((float)Math.Cos(angle1) * radiusPixels,
                (float)Math.Sin(angle1) * radiusPixels);
            var v2 = center + new Vector2((float)Math.Cos(angle2) * radiusPixels,
                (float)Math.Sin(angle2) * radiusPixels);
            DrawLine(v1, v2, color, 1.0f);
        }
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        var distance = Vector2.Distance(start, end);
        if (distance < 0.01f) return;

        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        _spriteBatch.Draw(
            _pixel,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(distance, thickness),
            SpriteEffects.None,
            0f
        );
    }

    private void DrawCross(Vector2 centerPixels, float size, Color color)
    {
        DrawLine(centerPixels + new Vector2(-size, 0), centerPixels + new Vector2(size, 0), color, 1.5f);
        DrawLine(centerPixels + new Vector2(0, -size), centerPixels + new Vector2(0, size), color, 1.5f);
    }

    public void Dispose()
    {
        _pixel?.Dispose();
    }
}