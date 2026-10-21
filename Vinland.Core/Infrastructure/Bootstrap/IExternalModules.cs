using Microsoft.Extensions.DependencyInjection;

namespace Vinland.Core.Infrastructure.Bootstrap;

public interface IExternalModules
{
    void RegisterServices(IServiceCollection services);
}