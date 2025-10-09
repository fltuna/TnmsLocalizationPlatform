using System.Globalization;
using Sharp.Shared;
using Sharp.Shared.Objects;

namespace TnmsLocalizationPlatform.Shared;

public interface ITnmsLocalizationPlatform
{
    public const string ModSharpModuleIdentity = "TnmsLocalizationPlatform";
    
    /// <summary>
    /// Create Localizer for your module
    /// </summary>
    /// <param name="module"></param>
    /// <returns></returns>
    public ITnmsLocalizer CreateStringLocalizer(ILocalizableModule module);
    
    /// <summary>
    /// Reload all modules translations from files.
    /// </summary>
    /// <returns></returns>
    public bool ReloadAllTranslations();
    
    /// <summary>
    /// Set client culture.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="culture"></param>
    public void SetClientCulture(IGameClient client, CultureInfo culture);
    
    /// <summary>
    /// Get client culture. If not set, return server default culture.
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public CultureInfo GetClientCulture(IGameClient client);
}