using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services.Threads;
using Serilog;

namespace Game.Core.Data;

public class DataManager(ChannelHub channelHub)
{
    private string _configBasePath;
    private Dictionary<string, EntityConfig> EntityConfigs { get; set; } = new();
    
    public void Start()
    {
        try
        {
            _configBasePath = Path.Combine(AppContext.BaseDirectory, "Content", "Configs");
        
            var entitiesJson = LoadConfig("entities.json");
            var configs = JsonSerializer.Deserialize<List<EntityConfig>>(entitiesJson);
        
            EntityConfigs = configs.ToDictionary(x => x.Species, x => x);
        
            HashSet<string> textureKeys = new();
            foreach (var entityConfig in EntityConfigs)
            {
                if (textureKeys.Add(entityConfig.Value.TextureKey))
                    channelHub.DataToMain.Writer.TryWrite(new TextureLoad(entityConfig.Value.TextureKey));
            }
        
            channelHub.DataToLogic.Writer.TryWrite(new EntityConfigs(EntityConfigs));
        
            channelHub.DataToLogic.Writer.TryWrite(new DataLoaded(true));
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while loading configuration file");
        }
        
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