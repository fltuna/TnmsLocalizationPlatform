using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sharp.Shared.Definition;

namespace TnmsLocalizationPlatform.Util;

public static class ChatColorUtil
{
    private class TrieNode
    {
        public readonly Dictionary<char, TrieNode> Children = new(4);
        public string? Replacement;
        public int KeyLength;
    }

    private static readonly Dictionary<string, string> ColorMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "{WHITE}", ChatColor.White },
        { "{DEFAULT}", ChatColor.White },
        { "{DARKRED}", ChatColor.DarkRed },
        { "{PINK}", ChatColor.Pink },
        { "{GREEN}", ChatColor.Green },
        { "{LIGHTGREEN}", ChatColor.LightGreen },
        { "{LIME}", ChatColor.Lime },
        { "{RED}", ChatColor.Red },
        { "{GREY}", ChatColor.Grey },
        { "{GRAY}", ChatColor.Grey },
        { "{YELLOW}", ChatColor.Yellow },
        { "{GOLD}", ChatColor.Gold },
        { "{SILVER}", ChatColor.Silver },
        { "{BLUE}", ChatColor.Blue },
        { "{DARKBLUE}", ChatColor.DarkBlue },
        { "{PURPLE}", ChatColor.Purple },
        { "{LIGHTRED}", ChatColor.LightRed },
        { "{MUTED}", ChatColor.Muted },
        { "{HEAD}", ChatColor.Head }
    };

    private static readonly TrieNode Root = BuildTrie();

    private static TrieNode BuildTrie()
    {
        var root = new TrieNode();
        
        foreach (var (key, value) in ColorMappings)
        {
            var node = root;
            var upperKey = key.ToUpperInvariant();
            
            foreach (char c in upperKey)
            {
                if (!node.Children.TryGetValue(c, out var next))
                {
                    next = new TrieNode();
                    node.Children[c] = next;
                }
                node = next;
            }
            
            node.Replacement = value;
            node.KeyLength = key.Length;
        }
        
        return root;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static char ToUpperAscii(char c)
    {
        return c >= 'a' && c <= 'z' ? (char)(c - 32) : c;
    }

    private const int StackBufferSize = 512;
    
    public static string FormatChatMessage(string message)
    {
        if (string.IsNullOrEmpty(message)) 
            return message;
        
        if (message.IndexOf('{') == -1)
            return message;

        string result = message.Length <= StackBufferSize 
            ? ProcessColorCodesStackalloc(message) 
            : ProcessColorCodesHeap(message);

        return result;
    }

    private static string ProcessColorCodesStackalloc(string message)
    {
        ReadOnlySpan<char> span = message.AsSpan();
        
        Span<char> buffer = stackalloc char[StackBufferSize];
        
        int writeIndex = 0;
        int readIndex = 0;

        while (readIndex < span.Length)
        {
            if (span[readIndex] != '{')
            {
                buffer[writeIndex++] = span[readIndex++];
                continue;
            }

            var node = Root;
            int matchLength = 0;
            string? matchReplacement = null;
            
            int lookAhead = readIndex;
            while (lookAhead < span.Length)
            {
                char c = ToUpperAscii(span[lookAhead]);
                
                if (!node.Children.TryGetValue(c, out node))
                    break;
                
                lookAhead++;
                
                if (node.Replacement != null)
                {
                    matchLength = node.KeyLength;
                    matchReplacement = node.Replacement;
                }
            }

            if (matchReplacement != null)
            {
                if (writeIndex + matchReplacement.Length > buffer.Length)
                {
                    return ProcessColorCodesHeap(message);
                }
                
                matchReplacement.AsSpan().CopyTo(buffer.Slice(writeIndex));
                writeIndex += matchReplacement.Length;
                readIndex += matchLength;
            }
            else
            {
                buffer[writeIndex++] = span[readIndex++];
            }
        }

        return new string(buffer.Slice(0, writeIndex));
    }

    private static string ProcessColorCodesHeap(string message)
    {
        ReadOnlySpan<char> span = message.AsSpan();
        
        char[] buffer = ArrayPool<char>.Shared.Rent(message.Length);
        
        try
        {
            int writeIndex = 0;
            int readIndex = 0;

            while (readIndex < span.Length)
            {
                if (span[readIndex] != '{')
                {
                    buffer[writeIndex++] = span[readIndex++];
                    continue;
                }

                var node = Root;
                int matchLength = 0;
                string? matchReplacement = null;
                
                int lookAhead = readIndex;
                while (lookAhead < span.Length)
                {
                    char c = ToUpperAscii(span[lookAhead]);
                    
                    if (!node.Children.TryGetValue(c, out node))
                        break;
                    
                    lookAhead++;
                    
                    if (node.Replacement != null)
                    {
                        matchLength = node.KeyLength;
                        matchReplacement = node.Replacement;
                    }
                }

                if (matchReplacement != null)
                {
                    matchReplacement.AsSpan().CopyTo(buffer.AsSpan(writeIndex));
                    writeIndex += matchReplacement.Length;
                    readIndex += matchLength;
                }
                else
                {
                    buffer[writeIndex++] = span[readIndex++];
                }
            }

            return new string(buffer, 0, writeIndex);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }
}
