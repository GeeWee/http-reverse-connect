using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WatsonWebsocket;

namespace client
{
    public class ReverseConnectHttpClient
    {
        public WatsonWsServer server;
        public string ClientIpPort;
        private string _message;

        public ReverseConnectHttpClient(WatsonWsServer server, string clientIpPort)
        {
            this.server = server;
            ClientIpPort = clientIpPort;
        }

        public async Task<T> GetAsync<T>(string url)
        {
            server.MessageReceived += MessageReceived;

            var call = new RpcCall(url, HttpMethod.Get.ToString(), new Dictionary<string, string>(), Body: new Dictionary<string, object>());
            await server.SendAsync(ClientIpPort, JsonSerializer.Serialize(call));

            for (int i = 0; i < 10; i++)
            {
                if (this._message == null)
                {
                    Console.WriteLine("Message not received yet");
                    await Task.Delay(500);
                    Console.WriteLine();
                }
                else
                {
                    return JsonSerializer.Deserialize<T>(this._message);
                }
            }

            Console.WriteLine("No message received within timeout");
            throw new Exception("No message received");
        }
        
        
        async void MessageReceived(object sender, MessageReceivedEventArgs args) 
        {
            var data = Encoding.UTF8.GetString(args.Data);
            Console.WriteLine(data);
            // todo id validation
            this._message = data;
        }
        
    }

    public record RpcCall(string Url, string Method, Dictionary<string, string> Headers, Dictionary<string, object> Body);

    // public record RpcCall(string Url, Guid id);

    public record RpcCallResponse<T>(string Url, Guid id, T data);    
}