using Serilog;
using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InfonetCSCR.Reporting
{
  public class MvcApplication : System.Web.HttpApplication
  {
    protected void Application_Start()
    {
      AreaRegistration.RegisterAllAreas();
      FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
      RouteConfig.RegisterRoutes(RouteTable.Routes);
      BundleConfig.RegisterBundles(BundleTable.Bundles);

      // Setup Serilog
      Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);
      Log.Logger = new LoggerConfiguration()
            .ReadFrom.AppSettings()
            .CreateLogger();

      JsonValueProviderFactory jsonValueProviderFactory = null;

      foreach (var factory in ValueProviderFactories.Factories)
      {
        if (factory is JsonValueProviderFactory)
        {
          jsonValueProviderFactory = factory as JsonValueProviderFactory;
        }
      }

      //remove the default JsonVAlueProviderFactory
      if (jsonValueProviderFactory != null) ValueProviderFactories.Factories.Remove(jsonValueProviderFactory);

      //add the custom one
      ValueProviderFactories.Factories.Add(new CustomJsonValueProviderFactory());
    }
  }
}
