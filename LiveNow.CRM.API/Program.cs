using LiveNow.CRM.API.Middlewares;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Add CORS for WinUI client
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

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
