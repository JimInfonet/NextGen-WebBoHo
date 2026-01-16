namespace InfonetCSCR.Api.Configuration;

/// <summary>
/// Owns core API infrastructure registrations. Feature registrations can move
/// into module methods incrementally without changing runtime behavior.
/// </summary>
public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddCscApiCore(this IServiceCollection services)
  {
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    services.AddScoped<ICompanySetupService, CompanySetupService>();
    services.AddSingleton<IApplicationConfig, ApplicationConfig>();

    return services;
  }
}
