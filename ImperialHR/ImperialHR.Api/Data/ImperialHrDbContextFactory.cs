using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ImperialHR.Api.Data;

public class ImperialHrDbContextFactory : IDesignTimeDbContextFactory<ImperialHrDbContext>
{
    public ImperialHrDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ImperialHrDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=ImperialHR;Trusted_Connection=True;MultipleActiveResultSets=true"
        );

        return new ImperialHrDbContext(optionsBuilder.Options);
    }
}
