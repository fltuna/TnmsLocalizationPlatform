using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Listeners;
using Sharp.Shared.Objects;
using TnmsLocalizationPlatform.Data;
using TnmsLocalizationPlatform.Internal;
using TnmsLocalizationPlatform.Services;
using TnmsLocalizationPlatform.Shared;
using TnmsDatabaseUtil.Shared;

namespace TnmsLocalizationPlatform;

public class TnmsLocalizationPlatform : IModSharpModule, ITnmsLocalizationPlatform, IClientListener
{
    internal readonly ILogger Logger;
    internal readonly ISharedSystem SharedSystem;
    private readonly string _dllPath;

    private LocalizationDbContext? _dbContext;
    private UserLanguageService _userLanguageService = null!;
    
    private readonly ConcurrentDictionary<string, CustomStringLocalizer> _loadedLocalizers = new();

    public TnmsLocalizationPlatform(ISharedSystem sharedSystem,
        string? dllPath,
        string? sharpPath,
        Version? version,
        IConfiguration? coreConfiguration,
        bool hotReload)
    {
        ArgumentNullException.ThrowIfNull(dllPath);
        ArgumentNullException.ThrowIfNull(sharpPath);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(coreConfiguration);
        
        SharedSystem = sharedSystem;
        Logger = sharedSystem.GetLoggerFactory().CreateLogger<TnmsLocalizationPlatform>();
        
        _dllPath = dllPath;

    }




    public int ListenerVersion => 1;
    public int ListenerPriority => 10;

    public string DisplayName => "TnmsLocalizationPlatform";
    public string DisplayAuthor => "faketuna";

    internal static TnmsLocalizationPlatform Instance { get; private set; } = null!;

    internal readonly ConcurrentDictionary<byte, CultureInfo> ClientCultures = new();

    // TODO() Get ServerDefault culture from config
    internal CultureInfo ServerDefaultCulture { get; set; } = new("en-US");

    public bool Init()
    {
        Instance = this;
        
        Logger.LogInformation("TnmsLocalizationPlatform initialized");
        return true;
    }

    public void PostInit()
    {
        SharedSystem.GetSharpModuleManager().RegisterSharpModuleInterface(this,
            ITnmsLocalizationPlatform.ModSharpModuleIdentity, (ITnmsLocalizationPlatform)this);
        SharedSystem.GetClientManager().InstallClientListener(this);
    }

    public void OnAllModulesLoaded()
    {
        if (!InitializeDatabase())
        {
            Logger.LogError("Failed to initialize database in OnAllModulesLoaded. and this plugin will not work.");
        }
    }

    public void Shutdown()
    {
        _dbContext?.Dispose();
        SharedSystem.GetClientManager().RemoveClientListener(this);
        Logger.LogInformation("TnmsLocalizationPlatform shutdown");
    }

    private bool InitializeDatabase()
    {
        try
        {
            // TODO() Configure database from config
            var dbParams = new DbConnectionParameters
            {
                ProviderType = TnmsDatabaseProviderType.Sqlite,
                Host = Path.Combine(_dllPath, "localization.db")
            };

            
            var options = ConnectionStringUtil.ConfigureDbContext<LocalizationDbContext>(dbParams);
            _dbContext = new LocalizationDbContext(options.Options);
            
            if (!ApplyDatabaseMigrations())
            {
                Logger.LogError("Failed to apply database migrations");
                return false;
            }

            var repository = new UserLanguageRepository(_dbContext);
            _userLanguageService = new UserLanguageService(repository);

            Logger.LogInformation("Database initialized successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize database");
            return false;
        }
    }

    private bool ApplyDatabaseMigrations()
    {
        try
        {
            if (_dbContext == null)
            {
                Logger.LogError("DbContext is null");
                return false;
            }

            var pendingMigrations = _dbContext.Database.GetPendingMigrations().ToList();
            
            if (pendingMigrations.Any())
            {
                Logger.LogInformation("Found {Count} pending migration(s): {Migrations}", 
                    pendingMigrations.Count, string.Join(", ", pendingMigrations));

                if (IsAutoMigrationEnabled())
                {
                    Logger.LogInformation("Auto-applying database migrations...");
                    _dbContext.Database.Migrate();
                    Logger.LogInformation("Database migrations applied successfully");
                }
                else
                {
                    Logger.LogWarning("Pending migrations detected but auto-migration is disabled.");
                    Logger.LogWarning("Please run 'dotnet ef database update' manually to apply migrations.");
                }
            }
            else
            {
                Logger.LogInformation("Database is up to date, no pending migrations");
            }

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during migration process");
            return false;
        }
    }

    private bool IsAutoMigrationEnabled()
    {
        // TODO() Load this from config
        return true;
    }

    public ITnmsLocalizer CreateStringLocalizer(ILocalizableModule module)
    {
        _loadedLocalizers[module.ModuleDirectory] = new CustomStringLocalizer(new LanguageDataParser(Path.Combine(module.ModuleDirectory, "lang")).Parse());
        return _loadedLocalizers[module.ModuleDirectory];
    }

    public bool ReloadAllTranslations()
    {
        try
        {
            foreach (var (modulePath, localizer) in _loadedLocalizers)
            {
                localizer.UpdateTranslations(new LanguageDataParser(Path.Combine(modulePath, "lang")).Parse());
            }
            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error reloading translations");
            return false;
        }
    }

    public void SetClientCulture(IGameClient client, CultureInfo culture)
    {
        ClientCultures[client.Slot] = culture;
    }

    public CultureInfo GetClientCulture(IGameClient client)
    {
        return ClientCultures.TryGetValue(client.Slot, out var culture) ? culture : ServerDefaultCulture;
    }


    public void OnClientConnected(IGameClient client)
    {
        // Default to server culture for fallback until we load the actual culture
        ClientCultures[client.Slot] = ServerDefaultCulture;
        
        Task.Run(async () =>
        {
            try
            {
                var savedLanguage = await _userLanguageService.GetUserLanguageCodeAsync(client.SteamId.AccountId);
                if (!string.IsNullOrEmpty(savedLanguage))
                {
                    var culture = CultureInfo.GetCultureInfo(savedLanguage);
                    ClientCultures[client.Slot] = culture;
                    Logger.LogDebug("Loaded saved language {Language} for player {SteamId}", savedLanguage,
                        client.SteamId.AccountId);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading saved language for {SteamId}", client.SteamId.AccountId);
            }
            
            SharedSystem.GetClientManager().QueryConVar(client, "cl_language", (gameClient, status, name, value) =>
                {
                    if (status != QueryConVarValueStatus.ValueIntact)
                    {
                        Logger.LogWarning("Failed to get client language for {SteamId}", gameClient.SteamId.AccountId);
                        return;
                    }
                    
                    var culture = ParseClientLanguageToCulture(value);
                    ClientCultures[gameClient.Slot] = culture;
            });
        });
    }

    public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
    {
        ClientCultures.Remove(client.Slot, out var culture);
        
        if (culture == null)
            return;
        
        Task.Run(() =>
        {
            try
            {
                _ = _userLanguageService.SaveUserLanguageAsync(
                    client.SteamId.AccountId, culture.TwoLetterISOLanguageName);
                Logger.LogDebug("Saved language {Language} for player {SteamId}", culture.TwoLetterISOLanguageName,
                    client.SteamId.AccountId);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving language to database for {SteamId}", client.SteamId.AccountId);
            }
        });
    }
    
    
    private CultureInfo ParseClientLanguageToCulture(string language)
    {
        return CultureInfo.GetCultureInfo(GetLanguageCodeFromClientLanguage(language));
    }
    
    private string GetLanguageCodeFromClientLanguage(string clientLanguage)
    {
        if (_cs2ClientLanguageMapping.TryGetValue(clientLanguage, out var langCode))
        {
            return langCode;
        }

        return ServerDefaultCulture.TwoLetterISOLanguageName;
    }
    
    // TODO() Add more language mapping
    private readonly Dictionary<string, string> _cs2ClientLanguageMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Japanese", "ja-JP" },
        { "English", "en-US" },
    };
}