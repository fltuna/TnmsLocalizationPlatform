using Microsoft.EntityFrameworkCore;
using TnmsLocalizationPlatform.Models;

namespace TnmsLocalizationPlatform.Data;

public class LocalizationDbContext : DbContext
{
    public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<UserLanguage> UserLanguages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<UserLanguage>(entity =>
        {
            entity.HasKey(e => e.SteamId);
            entity.Property(e => e.LanguageCode).IsRequired().HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // インデックスを追加
            entity.HasIndex(e => e.LanguageCode);
        });
    }
}
