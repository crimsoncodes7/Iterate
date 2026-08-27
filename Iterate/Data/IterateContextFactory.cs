using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Iterate.Data;

public class IterateContextFactory : IDesignTimeDbContextFactory<IterateContext>
{
    public IterateContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IterateContext>();
        optionsBuilder.UseSqlite("Data Source=iterate.db");
        return new IterateContext(optionsBuilder.Options);
    }
}