// Data/ImperialHRDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ImperialHR.Api.Data;

public class ImperialHrDbContextFactory : IDesignTimeDbContextFactory<ImperialHrDbContext>
{
    public ImperialHrDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var conn = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<ImperialHrDbContext>();
        optionsBuilder.UseSqlServer(conn);

        return new ImperialHrDbContext(optionsBuilder.Options);
    }
}
