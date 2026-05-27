using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PIQI.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PIQIDbContext>
{
    public PIQIDbContext CreateDbContext(string[] args)
    {
        // Adjust the path as needed for your solution structure
        var basePath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "PIQI_Engine.Server");
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PIQIDbContext>();
        var connStr = configuration.GetConnectionString("PIQIDatabase");

        // Use SQLite by default for design-time
        optionsBuilder.UseSqlite(connStr);

        return new PIQIDbContext(optionsBuilder.Options);
    }
}