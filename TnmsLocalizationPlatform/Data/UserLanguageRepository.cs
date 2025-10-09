using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TnmsLocalizationPlatform.Models;

namespace TnmsLocalizationPlatform.Data;

public class UserLanguageRepository : IUserLanguageRepository
{
    private readonly LocalizationDbContext _context;

    public UserLanguageRepository(LocalizationDbContext context)
    {
        _context = context;
    }

    public async Task<UserLanguage?> GetByIdAsync(long steamId)
    {
        return await _context.UserLanguages
            .FirstOrDefaultAsync(ul => ul.SteamId == steamId);
    }

    public async Task<IEnumerable<UserLanguage>> GetAllAsync()
    {
        return await _context.UserLanguages.ToListAsync();
    }

    public async Task<UserLanguage> AddAsync(UserLanguage userLanguage)
    {
        _context.UserLanguages.Add(userLanguage);
        await _context.SaveChangesAsync();
        return userLanguage;
    }

    public async Task UpdateAsync(UserLanguage userLanguage)
    {
        _context.UserLanguages.Update(userLanguage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(long steamId)
    {
        var userLanguage = await _context.UserLanguages.FindAsync(steamId);
        if (userLanguage != null)
        {
            _context.UserLanguages.Remove(userLanguage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(UserLanguage userLanguage)
    {
        _context.UserLanguages.Remove(userLanguage);
        await _context.SaveChangesAsync();
    }

    public IQueryable<UserLanguage> Query()
    {
        return _context.UserLanguages.AsQueryable();
    }
}
