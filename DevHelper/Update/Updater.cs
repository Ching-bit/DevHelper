using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Control.Basic;
using Framework.Utils;
using Framework.Utils.Helpers;
using Plugin.AppEnv;
using Plugin.Log;
using UniClient;
using Ursa.Controls;

namespace Framework.Common;

public static class Updater
{
    public static async Task<bool> UpdateAsync(string updateAddresses, string username, string password, LoginViewModel viewModel)
    {
        if (string.IsNullOrEmpty(updateAddresses))
        {
            return true;
        }

        string[] updateBaseUrls = updateAddresses.Split(';');
        if (updateBaseUrls.Length <= 0)
        {
            return true;
        }

        foreach (string updateBaseUrl in updateBaseUrls)
        {
            if (await UpdateAsyncInner(updateBaseUrl, username, password, viewModel))
            {
                return true;
            }
        }

        return false;
    }

    private static async Task<bool> UpdateAsyncInner(string updateBaseUrl, string username, string password, LoginViewModel viewModel)
    {
        try
        {
            string updateConfUrl = HttpHelper.ConcatUrl(updateBaseUrl, SystemConfig.AppConf.UpdateConfPath);
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Query: {HttpPath}", SystemConfig.AppConf.UpdateConfPath);
            UpdateConf updateConf = await GetUpdateConf(updateConfUrl, username, password);

            string fileInfoListUrl = HttpHelper.ConcatUrl(updateBaseUrl, updateConf.UpdateListPath);
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Query: {HttpPath}", updateConf.UpdateListPath);
            List<ClientFileInfo> fileInfoList = await GetFileInfoList(fileInfoListUrl, username, password);

            List<ClientFileInfo> differentFiles = CompareFiles(fileInfoList);
            if (differentFiles.Count <= 0)
            {
                return true;
            }

            string changeLogUrl = HttpHelper.ConcatUrl(updateBaseUrl, updateConf.ChangeLogPath);
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Query: {HttpPath}", updateConf.ChangeLogPath);
            string changeLog = await GetChangeLog(changeLogUrl, username, password);

            ConfirmDialogResult? updateInfoDialogResult =
                await Dialog.ShowCustomModal<UpdateInfoDialog, UpdateInfoDialogViewModel, ConfirmDialogResult>(
                    new UpdateInfoDialogViewModel
                    {
                        IsForce = updateConf.IsForce,
                        ChangeLog = changeLog
                    },
                    options: new DialogOptions
                    {
                        Title = ResourceHelper.FindStringResource("R_STR_NEW_VERSION_RELEASED"),
                        Mode = DialogMode.Info,
                        CanDragMove = true,
                        IsCloseButtonVisible = false,
                        CanResize = false
                    });
            if (false == updateInfoDialogResult?.IsConfirmed)
            {
                return true;
            }

            await UpdateFiles(updateBaseUrl, updateConf, differentFiles, username, password, viewModel);
            return true;
        }
        catch (Exception ex)
        {
            Global.Get<ILog>().Error(LogModule.UPDATE, ex);
            return false;
        }
    }

    private static async Task<UpdateConf> GetUpdateConf(string updateConfUrl, string username, string password)
    {
        string xml = await RequestText(updateConfUrl, username, password);
        return ObjectHelper.FromXmlString<UpdateConf>(xml);
    }

    private static async Task<List<ClientFileInfo>> GetFileInfoList(string fileInfoListUrl, string username, string password)
    {
        string xml = await RequestText(fileInfoListUrl, username, password);
        return ObjectHelper.FromXmlString<List<ClientFileInfo>>(xml);
    }

    private static async Task<string> GetChangeLog(string changeLogUrl, string username, string password)
    {
        string nativeChangeLogUrl = changeLogUrl.Replace(".txt", $".{Global.Get<IGlobalSetting>().Language}.txt");
        try
        {
            return await RequestText(nativeChangeLogUrl, username, password);
        }
        catch
        {
            try
            {
                return await RequestText(changeLogUrl, username, password);
            }
            catch
            {
                return "";
            }
        }
    }

    private static List<ClientFileInfo> CompareFiles(List<ClientFileInfo> fileInfoList)
    {
        List<ClientFileInfo> ret = [];
        foreach (ClientFileInfo fileInfo in fileInfoList)
        {
            string nativeMd5 = CryptHelper.Md5File(fileInfo.FileName);
            Global.Get<ILog>().Debug(LogModule.UPDATE, "[{SameFlag}]{FileName}, RemoteMd5: {RemoteMd5}, NativeMd5: {NativeMd5}",
                fileInfo.Md5 == nativeMd5 ? "✓" : "×", fileInfo.FileName, fileInfo.Md5, nativeMd5);
            if (fileInfo.Md5 != nativeMd5)
            {
                ret.Add(fileInfo);
            }
        }

        return ret;
    }

    private static async Task UpdateFiles(string updateBaseUrl, UpdateConf updateConf, List<ClientFileInfo> targetFiles, string username, string password, LoginViewModel viewModel)
    {
        viewModel.IsUpdating = true;
        if (Path.Exists(SystemConfig.AppConf.UpdateCacheDir))
        {
            Directory.Delete(SystemConfig.AppConf.UpdateCacheDir, true);
        }
        Directory.CreateDirectory(SystemConfig.AppConf.UpdateCacheDir);

        // download files
        viewModel.UpdateFileCount = targetFiles.Count;
        foreach (ClientFileInfo fileInfo in targetFiles)
        {
            viewModel.DownloadedFileCount++;
            viewModel.ErrMsg = fileInfo.FileName;

            string fileUrl = HttpHelper.ConcatUrl(updateBaseUrl, updateConf.RemoteClientDir, fileInfo.FileName);
            string targetPath = Path.Combine(SystemConfig.AppConf.UpdateCacheDir, fileInfo.FileName);
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Download: {FileName}", fileInfo.FileName);
            await DownloadFile(fileUrl, username, password, targetPath);
        }

        // copy files
        // copy Update exe at first
        string updateExeName = SystemConfig.AppConf.UpdateExeName;
        if (OSPlatform.Windows == Global.Get<IAppEnv>().OsPlatform)
        {
            updateExeName += updateExeName.EndsWith(".exe") ? "" : ".exe";
        }
        else if (OSPlatform.Linux == Global.Get<IAppEnv>().OsPlatform)
        {
            if (updateExeName.EndsWith(".exe"))
            {
                updateExeName = updateExeName[..^4];
            }
        }
        
        string updateExeNameWithoutExtension = Path.GetFileNameWithoutExtension(updateExeName);

        List<string> updateExeFiles = targetFiles.Select(x => x.FileName).Where(x => x.StartsWith(updateExeNameWithoutExtension + ".")).ToList();
        if (updateExeFiles.Count > 0)
        {
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Copy Files: {FileNames}", string.Join(", ", updateExeFiles));
            updateExeFiles.ForEach(x =>
            {
                File.Copy(Path.Combine(SystemConfig.AppConf.UpdateCacheDir, x), x, true);
            });
        }

        // copy other files
        List<string> otherFiles = targetFiles.Select(x => x.FileName).Where(x => !updateExeFiles.Contains(x)).ToList();
        Global.Get<ILog>().Debug(LogModule.UPDATE, "Copy Files: {FileNames}", string.Join(", ", otherFiles));
        UpdateInterParameters updateInterParameters = new()
        {
            ExeName = Global.Get<IAppEnv>().AppNameWithExtension,
            UpdateCacheDir = SystemConfig.AppConf.UpdateCacheDir,
            ClientDir = ".\\",
            Username = Global.Get<IAppEnv>().User?.Username ?? string.Empty,
            Password = Global.Get<IAppEnv>().User?.Password ?? string.Empty,
            SkipUpdate = true,
        };

        Process copyProcess = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = updateExeName,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        if (!copyProcess.Start())
        {
            throw new Exception("Failed to start copy process");
        }

        await using NamedPipeClientStream client = new(updateExeName);
        await client.ConnectAsync(5 * 1000);
        if (!client.IsConnected)
        {
            throw new Exception("Cannot connect to pipe");
        }

        await using StreamWriter steamWriter = new(client);
        await steamWriter.WriteLineAsync(updateInterParameters.ToCommandLine());
        await steamWriter.FlushAsync();
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Dispatcher.UIThread.Invoke(() => desktop.Shutdown());
        }
    }

    #region requests
    private static async Task<string> RequestText(string url, string username, string password)
    {
        if (url.StartsWith("http:") || url.StartsWith("https:"))
        {
            return await RequestTextHttp(url, username, password);
        }

        throw new Exception("Unsupported protocol: " + url);
    }

    private static async Task DownloadFile(string url, string username, string password, string targetPath)
    {
        string? targetDirectory = Path.GetDirectoryName(targetPath);
        if (null == targetDirectory)
        {
            throw new Exception($"Can't get the directory of: {targetPath}");
        }
        if (!Directory.Exists(targetDirectory))
        {
            Global.Get<ILog>().Debug(LogModule.UPDATE, "Create directory: {Directory}", targetDirectory);
            Directory.CreateDirectory(targetDirectory);
        }
        
        if (url.StartsWith("http:") || url.StartsWith("https:"))
        {
            await DownloadFileHttp(url, username, password, targetPath);
        }
        else
        {
            throw new Exception("Unsupported protocol: " + url);
        }
    }

    private static async Task<string> RequestTextHttp(string url, string username, string password)
    {
        HttpHelper.IAuth? auth =
                (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) ?
                null :
                new HttpHelper.BasicAuth { Username = username, Password = password };

        using HttpResponseMessage response = await HttpHelper.GetAsync(url, true, auth);
        if (!response.IsSuccessStatusCode)
        {
            Global.Get<ILog>().Error(LogModule.UPDATE, "Connect to update error: {StatusCode}", response.StatusCode);
            throw new Exception("Http status code: " + response.StatusCode);
        }

        return await response.Content.ReadAsStringAsync();
    }

    private static async Task DownloadFileHttp(string url, string username, string password, string targetPath)
    {
        HttpHelper.IAuth? auth =
                (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) ?
                null :
                new HttpHelper.BasicAuth() { Username = username, Password = password };

        using HttpResponseMessage response = await HttpHelper.GetAsync(url, true, auth);
        if (!response.IsSuccessStatusCode)
        {
            Global.Get<ILog>().Error(LogModule.UPDATE, "Connect to update error: {StatusCode}", response.StatusCode);
            throw new Exception("Http status code: " + response.StatusCode);
        }

        await using FileStream fileStream = new(targetPath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream);
    }
    #endregion
}