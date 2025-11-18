using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SkillBridge.API.Data;

public class SkillBridgeContextFactory : IDesignTimeDbContextFactory<SkillBridgeContext>
{
    public SkillBridgeContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<SkillBridgeContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new SkillBridgeContext(optionsBuilder.Options);
    }
}
