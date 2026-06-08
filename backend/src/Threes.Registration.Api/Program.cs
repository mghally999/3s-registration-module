using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using Threes.Registration.Api.Middleware;
using Threes.Registration.Api.Startup;
using Threes.Registration.Application;
using Threes.Registration.Infrastructure;
using Threes.Registration.Persistence;

var builder = WebApplication.CreateBuilder(args);

// structured logging. the console template includes the CorrelationId that the
// middleware pushes, so every line ties back to a single request. serilog's
// request logging (added below) gives us the per-request duration.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                        "{Properties:j}{NewLine}{Exception}"));

var connectionString = builder.Configuration.GetConnectionString("RegistrationDatabase")
    ?? throw new InvalidOperationException("connection string 'RegistrationDatabase' is not configured.");

// the three application-owned layers. domain has no registration of its own.
builder.Services.AddApplication();
builder.Services.AddPersistence(connectionString);
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Threes Registration API", Version = "v1" });
    options.EnableAnnotations();
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// health: sql server explicitly + the masstransit bus health check that
// AddMassTransit registers for us covers rabbitmq.
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sql-server", tags: new[] { "ready" });

const string corsPolicy = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(corsPolicy, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// bring the schema (and lookup seed) up before serving traffic. the
// integration tests run their own migration against a throwaway container, so
// we skip the auto-migrate under the "Testing" environment to avoid racing the
// WebApplicationFactory host startup.
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.MigrateDatabaseAsync();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Threes Registration API v1");
    options.DocumentTitle = "Threes Registration API";
});

app.UseCors(corsPolicy);

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.Run();

// exposed so the integration test project can boot the api with
// WebApplicationFactory<Program>.
public partial class Program;
