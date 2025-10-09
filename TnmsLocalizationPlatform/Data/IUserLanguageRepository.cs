using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TnmsLocalizationPlatform.Models;

namespace TnmsLocalizationPlatform.Data;

public interface IUserLanguageRepository
{
    Task<UserLanguage?> GetByIdAsync(long steamId);
    Task<IEnumerable<UserLanguage>> GetAllAsync();
    Task<UserLanguage> AddAsync(UserLanguage userLanguage);
    Task UpdateAsync(UserLanguage userLanguage);
    Task DeleteAsync(long steamId);
    Task DeleteAsync(UserLanguage userLanguage);
    IQueryable<UserLanguage> Query();
}
