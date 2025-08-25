using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SalesApp.Application; // <= ADICIONE

namespace SalesApp.Infrastructure;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connStr)
  {
    services.AddDbContext<AppDbContext>(opt =>
    {
      opt.UseMySql(connStr, ServerVersion.AutoDetect(connStr));
    });

    services.AddScoped<IAppDb>(sp => sp.GetRequiredService<AppDbContext>());

    return services;
  }
}
