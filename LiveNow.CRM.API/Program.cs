using LiveNow.CRM.API.Middlewares;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.API.Controllers;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

var jwtSettings = JwtSettings.FromConfiguration(builder.Configuration, builder.Environment.IsProduction());
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add Infrastructure services (EF Core, DbContext, UnitOfWork)
builder.Services.AddInfrastructure(builder.Configuration);

// Business services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IFinancialCalculator, FinancialCalculatorService>();
builder.Services.AddScoped<IPaymentFeeCalculator, PaymentFeeCalculator>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IRaceService, RaceService>();
builder.Services.AddScoped<IRaceEditionService, RaceEditionService>();
builder.Services.AddScoped<IRaceSlotService, RaceSlotService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
    builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IChecklistService, ChecklistService>();
    builder.Services.AddScoped<ICancellationService, CancellationService>();
    builder.Services.AddScoped<ITransferService, TransferService>();

// Add CORS for WinUI client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        string[] allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Configuration["BootstrapAdmin:Email"] is not null &&
    app.Configuration["BootstrapAdmin:Password"] is not null)
{
    using IServiceScope scope = app.Services.CreateScope();
    IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    string email = app.Configuration["BootstrapAdmin:Email"]!;
    string username = app.Configuration["BootstrapAdmin:Username"] ?? email.Split('@')[0];
    string name = app.Configuration["BootstrapAdmin:Name"] ?? username;
    await userService.EnsureBootstrapAdminAsync(name, username, email, app.Configuration["BootstrapAdmin:Password"]!);
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
