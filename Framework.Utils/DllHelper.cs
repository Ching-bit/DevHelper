using System.Reflection;

namespace Framework.Utils.Helpers;

public static class DllHelper
{
    private static readonly HashSet<string> LoadedDlls = [];

    static DllHelper()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        string dllName = args.Name.Split(",")[0];
        if (!LoadedDlls.Add(dllName))
        {
            return null;
        }

        try
        {
            return Assembly.Load(dllName);
        }
        catch
        {
            return null;
        }
    }

    public static Assembly LoadFrom(string dllPath)
    {
        return Assembly.LoadFrom(dllPath);
    }

    public static T? CreateInstance<T>(string dllPath, string namespaceName, string className)
    {
        Assembly assembly = LoadFrom(dllPath);
        string viewFullName = namespaceName + "." + className;
        object? obj = assembly.CreateInstance(viewFullName);
        return (T?)obj;
    }

    public static Type? GetType(string dllPath, string namespaceName, string className)
    {
        Assembly assembly = LoadFrom(dllPath);
        return assembly.GetType($"{namespaceName}.{className}");
    }
}