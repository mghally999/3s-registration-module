using Microsoft.EntityFrameworkCore;
using Threes.Registration.Persistence;

namespace Threes.Registration.Api.Startup;

// applies ef core migrations on startup, retrying a few times so the api can
// come up alongside sql server in docker without a race. the migrations carry
// the lookup seed data, so after this runs the governorate/city tables are
// populated.
public static class DatabaseInitializer
{
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RegistrationDbContext>>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                logger.LogInformation("database migrated");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    "database not ready (attempt {Attempt}/{Max}): {Reason}. retrying...",
                    attempt,
                    maxAttempts,
                    ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        // last attempt: let it throw so the failure is loud.
        await db.Database.MigrateAsync();
    }
}
