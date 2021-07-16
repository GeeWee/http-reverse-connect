using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

            var call = new RpcCall(url, Guid.NewGuid());
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

    public record RpcCall(string Url, Guid id);

    public record RpcCallResponse<T>(string Url, Guid id, T data);    
}