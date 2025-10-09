using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Sharp.Shared.Objects;
using TnmsLocalizationPlatform.Shared;

namespace TnmsLocalizationPlatform.Internal;

public class CustomStringLocalizer(Dictionary<string, Dictionary<string, string>> translations) : ITnmsLocalizer
{
    private Dictionary<string, Dictionary<string, string>> _translations = translations;
    
    internal void UpdateTranslations(Dictionary<string, Dictionary<string, string>> newTranslations)
    {
        _translations = newTranslations;
    }
    
    public TnmsLocalizedString this[string name]
    {
        get
        {
            if (!_translations.TryGetValue(TnmsLocalizationPlatform.Instance.ServerDefaultCulture.TwoLetterISOLanguageName, out var translation))
            {
                translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
            }
            
            var value = translation.GetValueOrDefault(name, name);
            var notFound = !translation.ContainsKey(name);
            return new TnmsLocalizedString(name, value, notFound);
        }
    }

    public TnmsLocalizedString this[string name, params object[] arguments]
    {
        get
        {
            if (!_translations.TryGetValue(TnmsLocalizationPlatform.Instance.ServerDefaultCulture.TwoLetterISOLanguageName, out var translation))
            {
                translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
            }
            var format = translation.GetValueOrDefault(name, name);
            var notFound = !translation.ContainsKey(name);
        
            var value = arguments.Length == 0
                ? format 
                : string.Format(format, arguments);
            return new TnmsLocalizedString(name, value, notFound);
        }
    }
    
    public TnmsLocalizedString this[string name, CultureInfo culture]
    {
        get
        {
            if (!_translations.TryGetValue(culture.TwoLetterISOLanguageName, out var translation))
            {
                return this[name];
            }
            
            var value = translation.GetValueOrDefault(name, name);
            var notFound = !translation.ContainsKey(name);
            return new TnmsLocalizedString(name, value, notFound);
        }
    }
    
    public TnmsLocalizedString this[string name, CultureInfo culture, params object[] arguments]
    {
        get
        {
            if (!_translations.TryGetValue(culture.TwoLetterISOLanguageName, out var translation))
            {
                return this[name, arguments];
            }
        
            var format = translation.GetValueOrDefault(name, name);
            var notFound = !translation.ContainsKey(name);
            var value = arguments.Length == 0 
                ? format 
                : string.Format(format, arguments);
            return new TnmsLocalizedString(name, value, notFound);
        }
    }

    public CultureInfo GetClientCulture(IGameClient client)
    {
        if (TnmsLocalizationPlatform.Instance.ClientCultures.TryGetValue(client.Slot, out var culture))
            return culture;

        return TnmsLocalizationPlatform.Instance.ServerDefaultCulture;
    }

    public TnmsLocalizedString ForClient(IGameClient client, string name)
    {
        if (!TnmsLocalizationPlatform.Instance.ClientCultures.TryGetValue(client.Slot, out var culture))
        {
            culture = TnmsLocalizationPlatform.Instance.ServerDefaultCulture;
        }
        
        if (!_translations.TryGetValue(culture.TwoLetterISOLanguageName, out var translation))
        {
            if (!_translations.TryGetValue(TnmsLocalizationPlatform.Instance.ServerDefaultCulture.TwoLetterISOLanguageName, out translation))
            {
                translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
            }
        }
        
        var value = translation.GetValueOrDefault(name, name);
        var notFound = !translation.ContainsKey(name);
        return new TnmsLocalizedString(name, value, notFound);
    }

    public TnmsLocalizedString ForClient(IGameClient client, string name, params object[] arguments)
    {
        if (!TnmsLocalizationPlatform.Instance.ClientCultures.TryGetValue(client.Slot, out var culture))
        {
            culture = TnmsLocalizationPlatform.Instance.ServerDefaultCulture;
        }
        
        if (!_translations.TryGetValue(culture.TwoLetterISOLanguageName, out var translation))
        {
            if (!_translations.TryGetValue(TnmsLocalizationPlatform.Instance.ServerDefaultCulture.TwoLetterISOLanguageName, out translation))
            {
                translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
            }
        }
        
        var format = translation.GetValueOrDefault(name, name);
        var notFound = !translation.ContainsKey(name);
        var value = arguments.Length == 0
            ? format 
            : string.Format(format, arguments);
        return new TnmsLocalizedString(name, value, notFound);
    }

    public IEnumerable<KeyValuePair<string, string>> GetAllStringsByCulture(CultureInfo culture)
    {
        if (!_translations.TryGetValue(culture.TwoLetterISOLanguageName, out var translation))
        {
            translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
        }
        
        return translation;
    }

    public IEnumerable<KeyValuePair<string, string>> GetAllStrings()
    {
        if (!_translations.TryGetValue(TnmsLocalizationPlatform.Instance.ServerDefaultCulture.TwoLetterISOLanguageName, out var translation))
        {
            translation = _translations.FirstOrDefault().Value ?? new Dictionary<string, string>();
        }
        
        return translation;
    }
}