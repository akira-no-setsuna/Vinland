using System;
using Microsoft.Xna.Framework.Graphics;

namespace Vinland.Monogame.Main.Render;

public class VisualEntity
{
    public required Guid Id { get; init; }
    public Vector2 Position { get; set; }
    public Texture2D Texture { get; set; }
}