using System;

namespace TnmsLocalizationPlatform.Shared;

/// <summary>
/// A lightweight alternative to Microsoft.Extensions.Localization.LocalizedString
/// </summary>
public readonly struct TnmsLocalizedString
{
    public string Name { get; }
    public string Value { get; }
    public bool ResourceNotFound { get; }

    public TnmsLocalizedString(string name, string value, bool resourceNotFound = false)
    {
        Name = name;
        Value = value;
        ResourceNotFound = resourceNotFound;
    }

    // Implicit conversion to string
    public static implicit operator string(TnmsLocalizedString localizedString)
    {
        return localizedString.Value;
    }

    public override string ToString() => Value;
    
    public override bool Equals(object? obj)
    {
        return obj is TnmsLocalizedString other && 
               Name == other.Name && 
               Value == other.Value && 
               ResourceNotFound == other.ResourceNotFound;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Value, ResourceNotFound);
    }
}
