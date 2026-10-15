using System.Collections.Generic;
using Vinland.Core.Data.ConfigClasses;

namespace Vinland.Core.Infrastructure.Channels.Commands;

public abstract record DataCommand;

public sealed record TextureLoad(
    string TextureKey
) : DataCommand;

public sealed record EntityConfigs(
    Dictionary<string, EntityConfig> Configs
) : DataCommand;

public sealed record DataLoaded(
    bool Success
) : DataCommand;