using System.Diagnostics;
using System.IO.Pipes;

namespace UniClient.Update;

class Program
{
    private const int TIMEOUT = 20 * 1000;  // ms
    private static readonly string CURRENT_FILE_NAME = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName + "");

    static void Main()
    {
        try
        {
            CopyFiles();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Environment.ExitCode = -1;
        }
    }

    private static void CopyFiles()
    {
        // get parameters by Pipe
        string pipeName = CURRENT_FILE_NAME;

        using NamedPipeServerStream server = new(pipeName);
        Task connTask = server.WaitForConnectionAsync();
        if (!connTask.Wait(TIMEOUT))
        {
            throw new Exception("Wait for connection timeout");
        }
        
        using StreamReader reader = new(server);
        string? updateArgs = reader.ReadLine();
        if (null == updateArgs)
        {
            throw new Exception("Update args is null");
        }
        server.Close();

        string? clientExeName = GetArg(updateArgs, "--exe");
        string? updateCacheDir = GetArg(updateArgs, "--update_cache");
        string? clientDir = GetArg(updateArgs, "--client_dir");
        Console.WriteLine("");
        Console.WriteLine($"ClientExeName: {clientExeName}");
        Console.WriteLine($"UpdateCacheDir: {updateCacheDir}");
        Console.WriteLine($"ClientDir: {clientDir}");
        if (null == clientExeName || null == updateCacheDir || null == clientDir)
        {
            throw new Exception("Invalid parameters");
        }

        // kill client process
        Console.WriteLine("");
        Process[] processes = Process.GetProcessesByName(clientExeName.EndsWith(".exe") ? clientExeName[..^4] : clientExeName);
        foreach (Process p in processes)
        {
            Console.WriteLine($"CloseMainWindow: {p.ProcessName}");
            if (!p.CloseMainWindow() || p.WaitForExit(TIMEOUT))
            {
                Console.WriteLine($"Kill: {p.ProcessName}");
                p.Kill();
                p.WaitForExit(TIMEOUT);
            }
            
            if (!p.HasExited)
            {
                throw new Exception($"{p.ProcessName} failed to exit");
            }
        }
        Console.WriteLine("Client process killed");

        // copy files
        Console.WriteLine("");
        Console.WriteLine("Copy starting");
        CopyDir(updateCacheDir, clientDir);
        Console.WriteLine("Copy finished");

        // run client
        Console.WriteLine("");
        string clientExePath = Path.Combine(clientDir, clientExeName);
        Console.WriteLine($"Run: {clientExePath}");
        ProcessStartInfo clientStartInfo = new()
        {
            FileName = clientExePath,
            UseShellExecute = false
        };
        Process clientProcess = new() { StartInfo = clientStartInfo };
        if (!clientProcess.Start())
        {
            throw new Exception("Failed to run client");
        }

        Console.WriteLine("Connect to client pipe");
        using NamedPipeClientStream client = new(clientExeName);

        client.Connect(TIMEOUT);
        using StreamWriter steamWriter = new(client);
        steamWriter.WriteLine(updateArgs);
        steamWriter.Flush();

        Console.WriteLine("App ended");
    }

    private static string? GetArg(string argLine, string argName)
    {
        int beginIndex = argLine.IndexOf(argName + "=", StringComparison.Ordinal);
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

    private static void CopyDir(string srcDir, string destDir)
    {
        if (!Directory.Exists(srcDir))
        {
            return;
        }

        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        foreach (string srcFilePath in Directory.GetFiles(srcDir))
        {
            if (Path.GetFileName(srcFilePath).StartsWith(CURRENT_FILE_NAME + "."))
            {
                continue;
            }
            string destFilePath = Path.Combine(destDir, Path.GetFileName(srcFilePath));
            Console.WriteLine($"Copy file: {srcFilePath} => {destFilePath}");
            try
            {
                File.Copy(srcFilePath, destFilePath, true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        foreach (string srcSubDir in Directory.GetDirectories(srcDir))
        {
            string destSubDir = Path.Combine(destDir, Path.GetFileName(srcSubDir));
            CopyDir(srcSubDir, destSubDir);
        }
    }
}