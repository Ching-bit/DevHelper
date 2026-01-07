using System.Text;

namespace Framework.Utils.Helpers;

public static class HttpHelper
{
    public interface IAuth
    {
        public System.Net.Http.Headers.AuthenticationHeaderValue ToAuthValue();
    }

    public class BasicAuth : IAuth
    {
        public required string Username { get; set; }
        public required string Password { get; set; }

        public System.Net.Http.Headers.AuthenticationHeaderValue ToAuthValue()
        {
            return new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));
        }
    }

    public static async Task<HttpResponseMessage> GetAsync(string url, bool ignoreInvalidCert = false, IAuth? auth = null)
    {
        using HttpClient client = CreateHttpClient(ignoreInvalidCert, auth);
        return await client.GetAsync(url);
    }

    private static HttpClient CreateHttpClient(bool ignoreInvalidCert, IAuth? auth)
    {
        HttpClient client = ignoreInvalidCert ?
            new HttpClient(new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cer, chain, errors) => true }) :
            new HttpClient();
        client.DefaultRequestHeaders.Authorization = auth?.ToAuthValue();
        return client;
    }

    public static string ConcatUrl(string url, params string[] appendPaths)
    {
        string ret = url;
        foreach (string path in appendPaths)
        {
            ret = ConcatUrl(ret, path);
        }
        return ret;
    }

    public static string ConcatUrl(string url, string appendPath)
    {
        if (url.EndsWith('/'))
        {
            url = url[..^1];
        }

        if (!appendPath.StartsWith('/'))
        {
            appendPath = "/" + appendPath;
        }

        return url + appendPath;
    }
}