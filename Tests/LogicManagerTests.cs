using System;
using System.Collections.Generic;
using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Logic;

namespace Game.Tests;

public class LogicManagerTests
{
    [Fact]
    public void FixedUpdate_SpawnsPlayerOnDataLoaded()
    {
        var hub = new ChannelHub();
        var logic = new LogicManager(hub);
        logic.Start();
        
        hub.DataToLogic.Writer.TryWrite(new EntityConfigs(new Dictionary<string, EntityConfig>
        {
            { "human", new() { Species = "human", MaxHealth = 100, Speed = 5, Radius = 1, Density = 1, TextureKey = "t" } }
        }));
        hub.DataToLogic.Writer.TryWrite(new DataLoaded(true));
        
        logic.ManualUpdate(1, 1f / 60f);
        
        hub.LogicToPhysic.Reader.TryRead(out var pCmd).Should().BeTrue();
        pCmd.Should().BeOfType<BodySpawn>();
        
        hub.LogicToMain.Reader.TryRead(out var mCmd).Should().BeTrue();
        mCmd.Should().BeOfType<TextureSpawn>();
    }
}
