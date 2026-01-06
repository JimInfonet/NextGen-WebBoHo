var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
string language = builder.Configuration.GetSection("Language").Get<string>();
var cultures = new List<CultureInfo> {
    new(language)
};
bool useHttps = bool.Parse(configuration["UseHttps"] ?? "true");

// Setup serilog
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Cookie Authentication
builder.Services.AddDataProtection()
  .PersistKeysToFileSystem(new DirectoryInfo("C:\\KeyDir"))
  .SetApplicationName("InfonetCSCR");

// TODO: Move applicable cookie settings to appsettings.json

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
    options.Cookie.Name = ".AspNet.SharedCookie";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = useHttps ? CookieSecurePolicy.Always : CookieSecurePolicy.None;
    options.Cookie.Path = "/";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    if (!string.IsNullOrEmpty(configuration["CookieDomain"])
      && string.Equals(configuration["CookieDomain"], "localhost",
      StringComparison.InvariantCultureIgnoreCase))
    {
      // We don't need to set the Cookie.Domain if domain is set to an IP address
      options.Cookie.Domain = configuration["CookieDomain"]; 
    }
    options.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(configuration["CookieExpiryTimeInMinute"]));
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth";
    options.LogoutPath = "/Auth";
  });

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
builder.Services.AddHttpClient("InfonetApi", httpClient =>
{
  httpClient.BaseAddress = new Uri(configuration["ApiBaseUrl"]);
  httpClient.Timeout = Timeout.InfiniteTimeSpan;
}).SetHandlerLifetime(TimeSpan.FromMinutes(10));
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization(options =>
    {
      options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResources));
    });
builder.Services
    .AddControllersWithViews(options =>
    {
      options.Filters.Add<GlobalExceptionFilter>();
    })
    .AddDataAnnotationsLocalization(options =>
    {
      options.DataAnnotationLocalizerProvider = (type, factory) =>
      {
        var assemblyName = new AssemblyName(typeof(SharedResources).GetTypeInfo().Assembly.FullName!);
        return factory.Create(nameof(SharedResources), assemblyName.Name!);
      };
    });

#region services
builder.Services.AddTransient<IValidationMessageTranslationService, ValidationMessageTranslationService>();
builder.Services.AddSingleton<IApplicationConfig, ApplicationConfig>();
builder.Services.AddTransient<IApiService, InfonetApiService>();
builder.Services.AddTransient<IHttpService, HttpService>();
builder.Services.AddSingleton<IApplicationPolicyService, ApplicationPolicyService>();
builder.Services.AddSingleton<IApplicationPolicy, ApplicationPolicy>();
builder.Services.AddTransient<IPolicyService, PolicyService>();
builder.Services.AddTransient<ISideBarItemsPolicyService, SideBarItemsPolicyService>();
builder.Services.AddTransient<ICardDefinitionService, CardDefinitionService>();
builder.Services.AddTransient<ICardProductRestrictionService, CardProductRestrictionService>();
builder.Services.AddTransient<ICheckInCheckOutService, CheckInCheckOutService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IPromotionalItemsService, PromotionalItemsService>();
builder.Services.AddTransient<IPurchasingAndReceivingService, PurchasingAndReceivingService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ITimesheetReportsService, TimesheetReportsService>();
builder.Services.AddTransient<ICompanySetupService, CompanySetupService>();
builder.Services.AddTransient<IDiscountTableService, DiscountTableService>();
builder.Services.AddTransient<IKitsService, KitsService>();
builder.Services.AddTransient<IHotButtonsService, HotButtonsService>();
builder.Services.AddTransient<IHotButtonPagesService, HotButtonPagesService>();
builder.Services.AddTransient<ICardRestrictionProfileService, CardRestrictionProfileService>();
builder.Services.AddTransient<IFunctionButtonsService, FunctionButtonsService>();
builder.Services.AddTransient<IFunctionListService, FunctionListService>();
builder.Services.AddTransient<ICustomerGroupService, CustomerGroupService>();
builder.Services.AddTransient<IGroupButtonsService, GroupButtonsService>();
builder.Services.AddTransient<ICashDrawsService, CashDrawsService>();
builder.Services.AddTransient<ITaxService, TaxService>();
builder.Services.AddTransient<INegativePositiveCardsService, NegativePositiveCardsService>();
builder.Services.AddTransient<IReasonsService, ReasonsService>();
builder.Services.AddTransient<IRestrictionsService, RestrictionsService>();
builder.Services.AddTransient<IUserGridService, UserGridService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<IAssociatedChargesService, AssociatedChargesService>();
builder.Services.AddTransient<IUserDetailsFormService, UserDetailsFormService>();
builder.Services.AddTransient<IUserGroupGridService, UserGroupGridService>();
builder.Services.AddTransient<IGenericService, GenericService>();
builder.Services.AddTransient<IUserGroupFormService, UserGroupFormService>();
builder.Services.AddTransient<ICharacteristicService, CharacteristicService>();
builder.Services.AddTransient<IFamilyService, FamilyService>();
builder.Services.AddTransient<ICommissionService, CommissionService>();
builder.Services.AddTransient<IUnitOfMeasureService, UnitOfMeasureService>();
builder.Services.AddTransient<IVendorsService, VendorsService>();
builder.Services.AddTransient<IRegistersService, RegistersService>();
builder.Services.AddTransient<IPrinterService, PrinterService>();
builder.Services.AddTransient<IOrderUnitService, OrderUnitService>();
builder.Services.AddTransient<IGroupService, GroupService>();
builder.Services.AddTransient<IExcelReportsService, ExcelReportsService>();
builder.Services.AddTransient<IFuelPriceChangeService, FuelPriceChangeService>();
builder.Services.AddTransient<ITillsService, TillsService>();
builder.Services.AddTransient<IPolicyByClassService, PolicyByClassService>();
builder.Services.AddTransient<IPolicyByEntityService, PolicyByEntityService>();
builder.Services.AddTransient<ITextReportsService, TextReportsService>();
builder.Services.AddTransient<IElectronicJournalService, ElectronicJournalService>();
builder.Services.AddTransient<IStoreShiftsService, StoreShiftsService>();
builder.Services.AddTransient<IStoreService, StoreService>();
builder.Services.AddTransient<IPricebookManagementService, PricebookManagementService>();
builder.Services.AddTransient<IRegionService, RegionService>();
builder.Services.AddTransient<ISendDataToStoreService, SendDataToStoreService>();
builder.Services.AddTransient<IPriceGroupService, PriceGroupService>();
builder.Services.AddTransient<IReportService, ReportService>();
builder.Services.AddTransient<IRepTillCloseService, RepTillCloseService>();
builder.Services.AddTransient<IRepCloseBatchService, RepCloseBatchService>();
builder.Services.AddTransient<IRepArStatService, RepArStatService>();

#endregion services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  if (useHttps)
  {
    app.UseHsts();
  }
}

if (useHttps)
{
  app.UseHttpsRedirection();
}
app.UseSession();
app.UseStaticFiles();

// Setting UI culture
app.UseRequestLocalization(options =>
{
  options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(language);
  options.SupportedUICultures = cultures;
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
// Load session keys from cookies
app.useSessionKeysLoader();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
