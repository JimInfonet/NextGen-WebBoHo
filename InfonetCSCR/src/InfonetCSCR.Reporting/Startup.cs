using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InfonetCSCR.Reporting.Startup))]
namespace InfonetCSCR.Reporting
{
  public partial class Startup
  {
    // TODO: Remove unused packages from this project
    public void Configuration(IAppBuilder app)
    {
      ConfigureAuth(app);
    }
  }
}
