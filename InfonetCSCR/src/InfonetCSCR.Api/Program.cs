// TODO: Add validation

using InfonetCSCR.Api.Configuration;
using InfonetCSCR.Api.Converter;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var InfonetAllowedSpecificOrigins = "_infonetAllowedSpecificOrigins";

// Setup serilog
Log.Logger = new LoggerConfiguration()
  .ReadFrom.Configuration(builder.Configuration)
  .Enrich.FromLogContext()
  .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

// Add services to the container.
#region services
builder.Services.AddControllers();
builder.Services.AddCscApiCore();
builder.Services.AddTransient<IUserService, UserService>();
//builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<ICheckInCheckOutService, CheckInCheckOutService>();
builder.Services.AddTransient<ITimesheetReportsService, TimesheetReportsService>();
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddTransient<IVendorService, VendorService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<IStockItemService, StockItemService>();
builder.Services.AddTransient<IFuelService, FuelService>();
builder.Services.AddTransient<IStockItemReportsService, StockItemReportsService>();
builder.Services.AddTransient<IStockDescriptionElementsService, StockDescriptionElementsService>();
builder.Services.AddTransient<IPLUMastService, PLUMastService>();
builder.Services.AddTransient<IStockBranchService, StockBranchService>();
builder.Services.AddTransient<IStockPriceService, StockPriceService>();
builder.Services.AddTransient<IPricebookTransferService, PricebookTransferService>();
builder.Services.AddTransient<IKitMasterService, KitMasterService>();
builder.Services.AddTransient<IKitItemService, KitItemService>();
builder.Services.AddTransient<ISaleService, SaleService>();
builder.Services.AddTransient<IProductTaxExemptService, ProductTaxExemptService>();
builder.Services.AddTransient<IStockTransferService, StockTransferService>();
builder.Services.AddTransient<ITaxExemptCategoryService, TaxExemptCategoryService>();
builder.Services.AddTransient<ITaxExemptVendorsService, TaxExemptVendorsService>();
builder.Services.AddTransient<ITaxExemptRateService, TaxExemptRateService>();
builder.Services.AddTransient<IEncryptionService, EncryptionService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IPolicyService, PolicyService>();
builder.Services.AddTransient<IRegionService, RegionService>();
builder.Services.AddTransient<IStoreService, StoreService>();
builder.Services.AddTransient<IStockItemDetailsService, StockItemDetailsService>();
builder.Services.AddTransient<ICardDefinitionService, CardDefinitionService>();
builder.Services.AddTransient<IDataDefinitionService, DataDefinitionService>();
builder.Services.AddTransient<IPriceGroupService, PriceGroupService>();
builder.Services.AddTransient<ITaxMasterService, TaxMasterService>();
builder.Services.AddTransient<IAssociationService, AssociationService>();
builder.Services.AddTransient<ITendersService, TendersService>();
builder.Services.AddTransient<ICardProductRestrictionService, CardProductRestrictionService>();
builder.Services.AddTransient<ITransSaleTendService, TransSaleTendService>();
builder.Services.AddTransient<ITillsSaleTendService, TillsSaleTendService>();
builder.Services.AddTransient<IHotButtonsService, HotButtonsService>();
builder.Services.AddTransient<IDataTransferReportsService, DataTransferReportsService>();
builder.Services.AddTransient<IEODGroupService, EODGroupService>();
builder.Services.AddTransient<IStockTaxService, StockTaxService>();
builder.Services.AddTransient<IPromotionalItemsService, PromotionalItemsService>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IStockAssociationService, StockAssociationService>();
builder.Services.AddTransient<IStockTakesAndReceivingService, StockTakesAndReceivingService>();
builder.Services.AddTransient<ISubDepartmentService, SubDepartmentService>();
builder.Services.AddTransient<ISubDetailService, SubDetailService>();
builder.Services.AddTransient<ICardRestrictionProfileService, CardRestrictionProfileService>();
builder.Services.AddTransient<IPurchaseHeaderService, PurchaseHeaderService>();
builder.Services.AddTransient<IPurchasingAndReceivingService, PurchasingAndReceivingService>();
builder.Services.AddTransient<ICustomerCardService, CustomerCardService>();
builder.Services.AddTransient<IPurchaseDetailService, PurchaseDetailService>();
builder.Services.AddTransient<IAddItemsToThePOService, AddItemsToThePOService>();
builder.Services.AddTransient<IDiscountTableService, DiscountTableService>();
builder.Services.AddTransient<IHotButtonPagesService, HotButtonPagesService>();
builder.Services.AddTransient<IFunctionButtonsService, FunctionButtonsService>();
builder.Services.AddTransient<IFunctionListService, FunctionListService>();
builder.Services.AddTransient<ICustomerGroupService, CustomerGroupService>();
builder.Services.AddTransient<IDashboardChartService, DashboardChartService>();
builder.Services.AddTransient<IGroupButtonsService, GroupButtonsService>();
builder.Services.AddTransient<ICashDrawsService, CashDrawsService>();
builder.Services.AddTransient<ITaxRateService, TaxRateService>();
builder.Services.AddTransient<INegativePositiveCardsService, NegativePositiveCardsService>();
builder.Services.AddTransient<IRestrictionsService, RestrictionsService>();
builder.Services.AddTransient<IBrandService, BrandService>();
builder.Services.AddTransient<IUserGroupService, UserGroupService>();
builder.Services.AddTransient<IUigService, UigService>();
builder.Services.AddTransient<IGenericService, GenericService>();
builder.Services.AddTransient<ISecurityLevelService, SecurityLevelService>();
builder.Services.AddTransient<ICharacteristicService, CharacteristicService>();
builder.Services.AddTransient<IReasonsService, ReasonsService>();
builder.Services.AddTransient<IFamilyService, FamilyService>();
builder.Services.AddTransient<ICommissionService, CommissionService>();
builder.Services.AddTransient<IUnitOfMeasureService, UnitOfMeasureService>();
builder.Services.AddTransient<IRegistersService, RegistersService>();
builder.Services.AddTransient<IOrderUnitService, OrderUnitService>();
builder.Services.AddTransient<IShiftService, ShiftService>();
builder.Services.AddTransient<IVendorPartService, VendorPartService>();
builder.Services.AddTransient<IVendorSortService, VendorSortService>();
builder.Services.AddTransient<IGroupService, GroupService>();
builder.Services.AddTransient<IExcelReportsService, ExcelReportsService>();
builder.Services.AddTransient<IFuelPriceChangeService, FuelPriceChangeService>();
builder.Services.AddTransient<ITillsService, TillsService>();
builder.Services.AddTransient<IPolicyByClassService, PolicyByClassService>();
builder.Services.AddTransient<ITextReportsService, TextReportsService>();
builder.Services.AddTransient<IElectronicJournalService, ElectronicJournalService>();
builder.Services.AddTransient<IPolicyByEntityService, PolicyByEntityService>();
builder.Services.AddTransient<IStoreShiftsService, StoreShiftsService>();
builder.Services.AddTransient<IPricebookManagementService, PricebookManagementService>();
builder.Services.AddTransient<ISendDataToStoreService, SendDataToStoreService>();
builder.Services.AddTransient<IRepTillCloseService, RepTillCloseService>();
builder.Services.AddTransient<IRepCloseBatchService, RepCloseBatchService>();
builder.Services.AddTransient<IRepArStatService, RepArStatService>();
#endregion services

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// 10-17-2023, Jim added to opt lambda expression to be able to debug API codes
//builder.Services.AddSwaggerGen(opt =>
//{
//    //opt.SwaggerDoc("v1", new OpenApiInfo { Title = "MyAPI", Version = "v1" });
//    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Please enter token",
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        BearerFormat = "JWT",
//        Scheme = "bearer"
//    });
//    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type=ReferenceType.SecurityScheme,
//                    Id="Bearer"
//                }
//            },
//            new string[]{}
//        }
//    });
//});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
  {
    {
      new OpenApiSecurityScheme
      {
        Reference = new OpenApiReference
        {
          Type=ReferenceType.SecurityScheme,
          Id="Bearer"
        }
      },
      Array.Empty<string>()
    }
  });
});

//JWT authentication
//Comment out following to run Swagger without going through JWT

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS
string[] trustedOrigins = configuration.GetSection("TrustedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
  options.AddPolicy(InfonetAllowedSpecificOrigins, policy =>
  {
    policy.WithOrigins(trustedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // preserve C# casing
    });

//Jim, Apr.29,2025 register custom converters for DateOnly and TimeOnly
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

bool useHttps = Boolean.Parse(configuration["UseHttps"] ?? "true");
if (useHttps)
{
  app.UseHttpsRedirection();
}
// Exception handling must wrap authentication, authorization, and endpoints.
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors(InfonetAllowedSpecificOrigins);

//Comment out following to run Swagger without going through JWT
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

app.Run();
