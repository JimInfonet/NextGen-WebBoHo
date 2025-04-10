using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Interop;
using Owin;
using System;
using System.IO;

namespace InfonetCSCR.Reporting
{
  public partial class Startup
  {
    // For more information on configuring authentication, please visit https://go.microsoft.com/fwlink/?LinkId=301864
    public void ConfigureAuth(IAppBuilder app)
    {
      // Enable the application to use a cookie to store information for the signed in user
      // Configure the sign in cookie
      app.UseCookieAuthentication(new CookieAuthenticationOptions
      {
        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
        CookieName = ".AspNet.SharedCookie",
        ExpireTimeSpan = TimeSpan.FromMinutes(120),
        SlidingExpiration = true,
        TicketDataFormat = new AspNetTicketDataFormat(
          new DataProtectorShim(
            DataProtectionProvider.Create(new DirectoryInfo("C:\\KeyDir"),
            (builder) =>
            {
              builder.SetApplicationName("InfonetCSCR"); // Application Name must be same across the applications share same cookie authentication
            })
            .CreateProtector(
              "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
              "Cookies",
              "v2"))),
        CookieManager = new ChunkingCookieManager()
      });

      System.Web.Helpers.AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    }
  }
}