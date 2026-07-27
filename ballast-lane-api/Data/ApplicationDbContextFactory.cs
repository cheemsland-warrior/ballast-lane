using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BallastLaneApi.Data
{
    // Design-time factory for EF tools (migrations, database update) so tools can create
    // ApplicationDbContext without relying on the app's runtime DI setup.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Prefer environment variable, fall back to localhost docker defaults
            var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION")
                       ?? "Host=localhost;Port=5432;Database=yourappdb;Username=postgres;Password=Secret";

            builder.UseNpgsql(conn, b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
