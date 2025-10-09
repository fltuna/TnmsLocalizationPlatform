using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TnmsLocalizationPlatform.Data;

public class LocalizationDbContextFactory : IDesignTimeDbContextFactory<LocalizationDbContext>
{
    public LocalizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LocalizationDbContext>();
        
        optionsBuilder.UseSqlite("Data Source=localization_design.db");
        
        return new LocalizationDbContext(optionsBuilder.Options);
    }
}
