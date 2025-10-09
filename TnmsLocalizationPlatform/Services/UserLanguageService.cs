using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TnmsLocalizationPlatform.Data;
using TnmsLocalizationPlatform.Models;

namespace TnmsLocalizationPlatform.Services;

public class UserLanguageService
{
    private readonly IUserLanguageRepository _repository;
    
    public UserLanguageService(IUserLanguageRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<UserLanguage?> GetUserLanguageAsync(long steamId)
    {
        return await _repository.GetByIdAsync(steamId);
    }
    
    public async Task<string?> GetUserLanguageCodeAsync(long steamId)
    {
        var userLanguage = await GetUserLanguageAsync(steamId);
        return userLanguage?.LanguageCode;
    }
    
    public async Task<UserLanguage> SaveUserLanguageAsync(long steamId, string languageCode)
    {
        var existingUserLanguage = await GetUserLanguageAsync(steamId);
        
        if (existingUserLanguage != null)
        {
            existingUserLanguage.LanguageCode = languageCode;
            existingUserLanguage.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(existingUserLanguage);
            return existingUserLanguage;
        }
        else
        {
            var newUserLanguage = new UserLanguage
            {
                SteamId = steamId,
                LanguageCode = languageCode,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return await _repository.AddAsync(newUserLanguage);
        }
    }
    
    public async Task<bool> DeleteUserLanguageAsync(long steamId)
    {
        await _repository.DeleteAsync(steamId);
        return true;
    }
    
    public async Task<List<UserLanguage>> GetAllUserLanguagesAsync()
    {
        return (await _repository.GetAllAsync()).ToList();
    }
}
