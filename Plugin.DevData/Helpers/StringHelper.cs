using System.Text.RegularExpressions;

namespace Plugin.DevData.Helpers;

public partial class StringHelper
{
    private static string ToNameStyle(string input, NameStyle style)
    {
        string[] words = input.Split(",");
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = ToNameStyleOneWord(words[i].Trim(), style);
        }
        return string.Join(", ", words);
    }
    
    private static string ToNameStyleOneWord(string input, NameStyle style)
    {
        if (style == NameStyle.Original)
        {
            return input;
        }

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
    
    public static string ReplaceMacro(string input, string macro, string macroValue)
    {
        if (ContainsMacro(input, macro, out int startIndex, out int endIndex, out string remark))
        {
            _ = Enum.TryParse(remark, out NameStyle nameStyle);
            input = ReplaceRange(input, startIndex, endIndex, StringHelper.ToNameStyle(macroValue, nameStyle));
        }
        return input;
    }

    public static bool ContainsMacro(string line, string macro, out int startIndex, out int endIndex, out string remark)
    {
        startIndex = -1;
        endIndex = -1;
        remark = string.Empty;
        
        string pattern = $@"\$\{{{Regex.Escape(macro)}(?:\((?<arg>[^\)]*)\))?\}}";

        var match = Regex.Match(line, pattern);

        if (!match.Success) { return false; }
        
        startIndex = match.Index;
        endIndex = match.Index + match.Length - 1;
        remark = match.Groups["arg"].Success ? match.Groups["arg"].Value : string.Empty;
        return true;
    }
    
    public static string ReplaceRange(string input, int startIndex, int endIndex, string replacement)
    {
        if (startIndex < 0 || endIndex >= input.Length || startIndex > endIndex)
        {
            throw new ArgumentOutOfRangeException();
        }
        
        string before = input[..startIndex];
        string after = input[(endIndex + 1)..];

        return before + replacement + after;
    }
}