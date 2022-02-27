using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MakeReqTest
{
    class Program
    {
        private static Random random = new Random();
        private static string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz012!@#$%^&*()_+-=}{[]\';/. `~3456789";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                var c = pool[random.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }

        static void Main(string[] args)
        {
            List<string> uris = new List<string>()
            {
                "" // PUT URLS HERE 
            };

            while (true)
            {
                foreach (var uri in uris)
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    for (int i = 1; i < 5; i++)
                    {
                        sw.Restart();
                        Console.WriteLine($"Starting {uri}");

                        Task t1 = Task.Run(async () => { await MakeReqForSite(uri); });
                        Task t2 = Task.Run(async () => { await MakeReqForSite(uri); });
                        Task t3 = Task.Run(async () => { await MakeReqForSite(uri); });
                        Task t4 = Task.Run(async () => { await MakeReqForSite(uri); });

                        Task.WaitAll(t1, t2, t3, t4);

                        sw.Stop();
                        Console.WriteLine($"{sw.ElapsedMilliseconds} ms");
                    }
                }
            }

            Console.ReadLine();
        }

        private static async Task MakeReqForSite(string uri)
        {
            int len = 1000000;

            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "text/plain";
                request.AllowWriteStreamBuffering = false;

                string text = RandomString(len);
                byte[] packetData = new ASCIIEncoding().GetBytes(text);
                request.ContentLength = len;

                Stream serverStream = await request.GetRequestStreamAsync();

                byte[] buffer = new byte[4096];

                foreach (byte[] copySlice in packetData.Slices(4096))
                {
                    int bytesRead = copySlice.Length;
                    if (bytesRead > 0)
                    {
                        await serverStream.WriteAsync(copySlice, 0, bytesRead);
                        System.Threading.Thread.Sleep(200);
                    }
                    else
                    {
                        break;
                    }
                }
                serverStream.Close();
                request.GetResponse();
            }
            catch (Exception ex)
            {
                string errText = ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                //System.Diagnostics.Debug.WriteLine($"MakeReqForSite: {errText}");
                Console.WriteLine($"MakeReqForSite: {errText}");
            }
        }
    }
}
