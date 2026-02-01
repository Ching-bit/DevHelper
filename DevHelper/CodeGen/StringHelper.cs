using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace UniClient;

public partial class StringHelper
{
    public static string ToNameStyle(string input, NameStyle style)
    {
        if (style == NameStyle.Original)
            return input;
        
        List<string> words = SplitWords(input);
        
        return style switch
        {
            NameStyle.lowerCamel => string.Concat(words.First().ToLower(),
                string.Concat(words.Skip(1).Select(Capitalize))),
            NameStyle.UpperCamel => string.Concat(words.Select(Capitalize)),
            NameStyle.lower_snake => string.Join("_", words.Select(w => w.ToLower())),
            NameStyle.UPPER_SNAKE => string.Join("_", words.Select(w => w.ToUpper())),
            NameStyle.all_lower => input.ToLower(),
            NameStyle.ALL_UPPER => input.ToUpper(),
            _ => input
        };
    }
    
    private static List<string> SplitWords(string input)
    {
        string normalized = input.Replace("_", " ");

        MatchCollection matches = MyRegex().Matches(normalized);
        return matches.Select(m => m.Value).ToList();
    }

    private static string Capitalize(string word)
    {
        if (string.IsNullOrEmpty(word)) return word;
        if (word.Length == 1) return word.ToUpper();
        return char.ToUpper(word[0]) + word.Substring(1).ToLower();
    }

    [GeneratedRegex(@"[A-Z]?[a-z]+|[A-Z]+(?![a-z])|\d+")]
    private static partial Regex MyRegex();
}