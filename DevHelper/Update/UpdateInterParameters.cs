namespace Framework.Common;

public class UpdateInterParameters
{
    public required string ExeName { get; set; }
    public required string UpdateCacheDir { get; set; }
    public required string ClientDir { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool SkipUpdate { get; set; }

    public string ToCommandLine()
    {
        return
            $"--exe={ExeName}" +
            $" --update_cache={UpdateCacheDir}" +
            $" --client_dir={ClientDir}" +
            $" --username={Username}" +
            $" --password={Password}" +
            $" --skip_update={SkipUpdate}";
    }

    public static UpdateInterParameters FromCommandLine(string commandLine)
    {
        return new()
        {
            ExeName = GetArg(commandLine, "--exe") ?? string.Empty,
            UpdateCacheDir = GetArg(commandLine, "--update_cache") ?? string.Empty,
            ClientDir = GetArg(commandLine, "--client_dir") ?? string.Empty,
            Username = GetArg(commandLine, "--username") ?? string.Empty,
            Password = GetArg(commandLine, "--password") ?? string.Empty,
            SkipUpdate = GetArg(commandLine, "--skip_update")?.ToLower() == "true"
        };
    }

    private static string? GetArg(string argLine, string argName)
    {
        int beginIndex = argLine.IndexOf(argName + "=");
        if (beginIndex < 0)
        {
            return null;
        }
        beginIndex += argName.Length + 1;

        int endIndex = argLine.IndexOf(' ', beginIndex);
        if (endIndex < 0)
        {
            endIndex = argLine.Length;
        }

        return argLine[beginIndex..endIndex];
    }
}