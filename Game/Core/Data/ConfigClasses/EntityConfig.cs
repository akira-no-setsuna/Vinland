namespace Game.Core.Data.ConfigClasses;

public class EntityConfig
{
    public string Species { get; set; }
    public float MaxHealth { get; set; }
    public float Speed { get; set; }

    public float Radius { get; set; }
    public float Density { get; set; }

    public string TextureKey { get; set; }
}