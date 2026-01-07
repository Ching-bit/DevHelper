using System.Diagnostics;
using System.Net.Sockets;

namespace Framework.Utils;

public static class NetworkHelper
{
    public static long Delay(string hostname, int port, int millisecondsTimeout = 1000)
    {
        try
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            using TcpClient client = new();
            IAsyncResult result = client.BeginConnect(hostname, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(millisecondsTimeout);

            if (!success)
            {
                return -1;
            }

            client.EndConnect(result);
            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }
        catch
        {
            return -1;
        }
    }
}
