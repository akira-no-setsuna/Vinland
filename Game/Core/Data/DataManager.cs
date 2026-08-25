using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services.Threads;
using Serilog;

namespace Game.Core.Data;

public class DataManager(ChannelHub channelHub) : BaseThread
{
    private string _configBasePath;
    private List<EntityConfig> Entities { get; set; } = new();
    protected override void Prepare()
    {
        _configBasePath = Path.Combine(AppContext.BaseDirectory, "Content", "Configs");
        
        var entitiesJson = LoadConfig("entities.json");
        Entities = JsonSerializer.Deserialize<List<EntityConfig>>(entitiesJson);

        foreach (var entityConfig in Entities)
        {
            channelHub.DataToMain.Writer.TryWrite(new TextureLoad(entityConfig.TextureKey));
        }
    }

    protected override void FixedUpdate(float deltaTime)
    {
        
    }
    
    private string LoadConfig(string fileName)
    {
        var fullPath = Path.Combine(_configBasePath, fileName);
        
        if (!File.Exists(fullPath))
        {
            Log.Warning("Configuration file not found: {path}", fullPath);
            return string.Empty;
        }

        return File.ReadAllText(fullPath);
    }
}