using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Dynamics;

namespace Vinland.Core.Physic;

/// <summary>
/// Гладкая отрисовка физических тел (полигонов) для отладки.
/// </summary>
public class PhysicsDebugRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;

    // Более контрастные цвета для отладки
    private static readonly Color StaticColor = new Color(0, 255, 0, 180);      // Ярко-зелёный (стены)
    private static readonly Color DynamicColor = new Color(255, 50, 50, 220);   // Ярко-красный (игрок/враги)
    private static readonly Color KinematicColor = new Color(50, 150, 255, 220);// Ярко-синий

    public PhysicsDebugRenderer(GraphicsDevice graphicsDevice)
    {
        _spriteBatch = new SpriteBatch(graphicsDevice);
        _pixel = CreatePixelTexture(graphicsDevice);
    }

    private Texture2D CreatePixelTexture(GraphicsDevice gd)
    {
        var tex = new Texture2D(gd, 1, 1);
        tex.SetData(new[] { Color.White });
        return tex;
    }

    public void Draw(World world, Matrix viewMatrix)
    {
        // 🔑 ГЛАВНОЕ ИЗМЕНЕНИЕ: LinearClamp вместо PointClamp для гладких линий
        _spriteBatch.Begin(
            samplerState: SamplerState.LinearClamp, 
            transformMatrix: viewMatrix,
            blendState: BlendState.AlphaBlend // Чтобы цвета полупрозрачно накладывались на игру
        );

        foreach (var body in world.BodyList)
        {
            var color = body.BodyType switch
            {
                BodyType.Static => StaticColor,
                BodyType.Dynamic => DynamicColor,
                BodyType.Kinematic => KinematicColor,
                _ => Color.White
            };

            foreach (var fixture in body.FixtureList)
            {
                if (fixture.Shape is PolygonShape poly)
                {
                    DrawPolygon(body, poly.Vertices, color);
                }
                else if (fixture.Shape is CircleShape circle)
                {
                    DrawCircle(body, circle, color);
                }
            }

            // Рисуем маленький крестик в центре масс тела
            DrawCross(body.Position.ToMono(), 3, Color.Yellow);
        }

        _spriteBatch.End();
    }

    private void DrawPolygon(Body body, nkast.Aether.Physics2D.Common.Vertices vertices, Color color)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            int next = (i + 1) % vertices.Count;

            // Переводим локальные вершины в мировые координаты
            var v1 = body.GetWorldPoint(vertices[i]).ToMono();
            var v2 = body.GetWorldPoint(vertices[next]).ToMono();

            // Толщина линии 2.0f делает её хорошо заметной
            DrawLine(v1, v2, color, 2.0f);
        }
    }

    private void DrawCircle(Body body, CircleShape circle, Color color)
    {
        // Упрощённая отрисовка круга через 16 сегментов (для отладки этого достаточно)
        var center = body.Position.ToMono();
        float radius = circle.Radius;
        int segments = 16;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(i * 2.0 * Math.PI / segments);
            float angle2 = (float)((i + 1) * 2.0 * Math.PI / segments);

            var v1 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
            var v2 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

            DrawLine(v1, v2, color, 1.0f);
        }
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
    {
        float distance = Vector2.Distance(start, end);
        if (distance < 0.01f) return; // Защита от деления на ноль или артефактов

        float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);

        _spriteBatch.Draw(
            _pixel,
            start,
            null,
            color,
            angle,
            Vector2.Zero,                 // origin в левом верхнем углу текстуры
            new Vector2(distance, thickness),
            SpriteEffects.None,
            0f
        );
    }

    private void DrawCross(Vector2 center, float size, Color color)
    {
        DrawLine(center + new Vector2(-size, 0), center + new Vector2(size, 0), color, 1.5f);
        DrawLine(center + new Vector2(0, -size), center + new Vector2(0, size), color, 1.5f);
    }
}