using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TnmsLocalizationPlatform.Util;

namespace TnmsLocalizationPlatform.Internal;

public class LanguageDataParser(string languageDataDir)
{
    public Dictionary<string, Dictionary<string, string>> Parse()
    {
        var translations = new Dictionary<string, Dictionary<string, string>>();
        
        if (!Directory.Exists(languageDataDir))
        {
            return translations;
        }

        var jsonFiles = Directory.GetFiles(languageDataDir, "*.json");
        
        foreach (var jsonFile in jsonFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(jsonFile);
                var jsonContent = File.ReadAllText(jsonFile);
                
                var languageData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
                
                if (languageData != null)
                {
                    foreach (var (key, value) in languageData)
                    {
                        languageData[key] = ChatColorUtil.FormatChatMessage(value);
                    }

                    translations[fileName] = languageData;
                }
            }
            catch (Exception)
            {
                // Ignore invalid files
            }
        }
        
        return translations;
    }
}