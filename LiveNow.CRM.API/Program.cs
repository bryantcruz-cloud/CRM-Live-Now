using LiveNow.CRM.API.Middlewares;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
