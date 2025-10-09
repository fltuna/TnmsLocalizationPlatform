# [TnmsLocalizationPlatform](https://github.com/fltuna/TnmsLocalizationPlatform)

## What is this?

This is a common platform for managing player language settings and enabling each plugin to easily support multiple languages.

On the first connection, the player's `cl_language` setting is automatically retrieved and saved.

## Features

### Automatic Management of Player Language Settings

When a player connects to the server, their `cl_language` setting is automatically retrieved and saved to the database. On subsequent connections, the saved language setting is used.

### Localizer for Multilingual Support

Provides multilingual support based on Microsoft.Extensions.Localization through the `ITnmsLocalizer` interface.

## Usage

### Adding Dependencies

Install `TnmsLocalizationPlatform.Shared` from NuGet.

```shell
dotnet add package TnmsLocalizationPlatform.Shared
```

### Plugin Development

#### Getting an Instance

```csharp
private ITnmsLocalizationPlatform? _localizationPlatform;

public void OnAllModulesLoaded()
{
    _localizationPlatform = SharedSystem.GetSharpModuleManager()
        .GetRequiredSharpModuleInterface<ITnmsLocalizationPlatform>(
            ITnmsLocalizationPlatform.ModSharpModuleIdentity).Instance;
}
```

#### Implementing ILocalizableModule

Implement `ILocalizableModule` in your plugin.

```csharp
public class YourPlugin : IModSharpModule, ILocalizableModule
{
    public string ModuleDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    
    // ...other implementations
}
```

#### Creating and Using a Localizer

```csharp
private ITnmsLocalizer _localizer = null!;

// Creating a Localizer
public void OnAllModulesLoaded()
{
    _localizer = _localizationPlatform.CreateStringLocalizer(this);
}

// Usage example
public void SendLocalizedMessage(IGameClient client, string key, params object[] args)
{
    var localizedString = _localizer.ForClient(client, key, args);
    client.PrintToChat(localizedString.Value);
}

// Getting a string for a specific culture
public string GetLocalizedString(string key, CultureInfo culture, params object[] args)
{
    if (_localizer != null)
    {
        return _localizer[key, culture, args].Value;
    }
    return key;
}
```

#### Getting Player Language Settings

```csharp
public void CheckPlayerLanguage(IGameClient client)
{
    var playerCulture = _localizer.GetClientCulture(client);
    Console.WriteLine($"Player {client.PlayerName} uses language: {playerCulture.Name}");
}
```

### Placing Language Files

modules/yourmodle/lang/

### Another develop info

For more details, please refer to the code documentation.
