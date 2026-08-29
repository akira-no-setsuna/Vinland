using System;
using System.Collections.Generic;
using Game.Core.Data.ConfigClasses;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Logic;
using Game.Core.Physics;

namespace Game.Tests;

public class DeterminismTests
{
    [Fact]
    public void Simulate300Ticks_ProducesIdenticalHashes()
    {
        long RunSimulation()
        {
            var hub = new ChannelHub();
            var logic = new LogicManager(hub);
            var phys = new PhysicsManager(hub);
            logic.Start();
            phys.Start();

            hub.DataToLogic.Writer.TryWrite(new EntityConfigs(new Dictionary<string, EntityConfig>
            {
                { "human", new() { Species = "human", MaxHealth = 100, Speed = 5, Radius = 1, Density = 1, TextureKey = "t" } }
            }));
            hub.DataToLogic.Writer.TryWrite(new DataLoaded(true));

            long hash = 0;
            for (int i = 0; i < 300; i++)
            {
                logic.ManualUpdate(i, 1f / 60f);
                phys.ManualUpdate(i, 1f / 60f);

                while (hub.PhysicsToMain.Reader.TryRead(out var cmd))
                {
                    if (cmd is PositionsUpdate pu)
                    {
                        foreach (var p in pu.Positions)
                            hash = HashCode.Combine(hash, p.Position.X, p.Position.Y);
                    }
                }
            }
            return hash;
        }

        var hash1 = RunSimulation();
        var hash2 = RunSimulation();
        
        hash1.Should().Be(hash2);
        hash1.Should().NotBe(0);
    }
}
