using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.MsSql;
using Threes.Registration.Persistence;
using Xunit;

namespace Threes.Registration.IntegrationTests;

// boots the real api against a throwaway sql server 2019 container. the api's
// own startup migration runs against the container, so these tests exercise the
// exact ef core mapping, migrations and seed that production uses. messaging is
// switched to masstransit's in-memory transport so the tests do not need
// rabbitmq.
public sealed class RegistrationApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // pinned to a CU that still ships sqlcmd at /opt/mssql-tools (the testcontainers
    // 3.8 readiness probe uses that path). the newer 2019-latest image moved it
    // to /opt/mssql-tools18 and the probe would never go ready.
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2019-CU18-ubuntu-20.04")
        .Build();

    // computed after the container starts. we take testcontainers' connection
    // string but force Encrypt=False / TrustServerCertificate=True so the
    // client does not try (and fail) a tls handshake against the container's
    // self-signed setup, which otherwise surfaces as a generic "could not open
    // a connection" error.
    private string _connectionString = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();

        // force ipv4 + the explicit mapped port. testcontainers hands back
        // "localhost", which SqlClient can resolve to ipv6 ::1 while the sql
        // container only listens on ipv4, producing an instant "could not open
        // a connection" (error 40). pinning 127.0.0.1 sidesteps that.
        var mappedPort = _sqlServer.GetMappedPublicPort(1433);
        _connectionString = new SqlConnectionStringBuilder(_sqlServer.GetConnectionString())
        {
            DataSource = $"127.0.0.1,{mappedPort}",
            InitialCatalog = "ThreesRegistration",
            Encrypt = false,
            TrustServerCertificate = true,
        }.ConnectionString;

        // env vars load AFTER appsettings.json in the default config order, so
        // setting them here is what actually overrides the connection string the
        // app reads (an in-memory config source was being shadowed by
        // appsettings.json -> the app kept trying localhost:1433).
        Environment.SetEnvironmentVariable("ConnectionStrings__RegistrationDatabase", _connectionString);
        Environment.SetEnvironmentVariable("Messaging__UseInMemory", "true");

        // accessing Services builds the host. the api itself skips auto-migrate
        // under the Testing environment, so this is the only migration that runs.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();

        // the first connection can still race the mapped port, so retry. on the
        // final attempt let it throw with the real (inner) reason.
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch when (attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }

    public new async Task DisposeAsync() => await _sqlServer.DisposeAsync();
}

[CollectionDefinition(nameof(RegistrationApiCollection))]
public sealed class RegistrationApiCollection : ICollectionFixture<RegistrationApiFactory>;
