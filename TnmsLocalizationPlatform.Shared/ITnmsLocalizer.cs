using System.Globalization;
using System.Collections.Generic;
using Sharp.Shared.Objects;

namespace TnmsLocalizationPlatform.Shared;

public interface ITnmsLocalizer
{
    public TnmsLocalizedString this[string name] { get; }
    
    public TnmsLocalizedString this[string name, params object[] arguments] { get; }
    
    public TnmsLocalizedString this[string name, CultureInfo culture] { get; }
    
    public TnmsLocalizedString this[string name, CultureInfo culture, params object[] arguments] { get; }
    
    public CultureInfo GetClientCulture(IGameClient client);
    
    public TnmsLocalizedString ForClient(IGameClient client, string name);
    
    public TnmsLocalizedString ForClient(IGameClient client, string name, params object[] arguments);
    
    public IEnumerable<KeyValuePair<string, string>> GetAllStringsByCulture(CultureInfo culture);
    
    public IEnumerable<KeyValuePair<string, string>> GetAllStrings();
}