using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BetterHostedServices;
using Microsoft.Extensions.Hosting;
using SCM.SwissArmyKnife.Extensions;
using WatsonWebsocket;

namespace aspnetcoreserver
{
    public class WebsocketReverseConnectHostedService : CriticalBackgroundService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private WatsonWsClient _websocketClient;

        public WebsocketReverseConnectHostedService(IApplicationEnder applicationEnder, IHttpClientFactory _httpClientFactory) : base(applicationEnder)
        {
            this._httpClientFactory = _httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();
            // we're not guaranteed server is ready at this point - so we wait
            // TOOD more robust handling
            await Task.Delay(TimeSpan.FromSeconds(1)); 
                            
            _websocketClient = new WatsonWsClient("localhost", 8081, ssl: false);
            _websocketClient.ServerConnected += ServerConnected;
            _websocketClient.ServerDisconnected += ServerDisconnected;
            _websocketClient.MessageReceived += MessageReceived; 
            _websocketClient.Start();

            async void ServerConnected(object sender, EventArgs args) 
            {
                // Here the server is now connected
                Console.WriteLine("Reverse connection established - client can now send websocket requests");
            }

            async void MessageReceived(object sender, MessageReceivedEventArgs args) 
            {
                var data = Encoding.UTF8.GetString(args.Data);
                Console.WriteLine("Message from client (server): " + data);
                RpcCall deserialized = JsonSerializer.Deserialize<RpcCall>(data)!;

                Console.WriteLine("DATA (deserialized)");
                deserialized.PrintAsJson();
                
                // todo map on protocol
                // Map websocket remote procedure call to internal http
                using var httpClient = _httpClientFactory.CreateClient();
                
                // TODO get baseurl + port in some non-hardcoded way

                var url = $"http://localhost:5000{deserialized.Url}";
                Console.WriteLine($"Calling HTTP with {url}");
                var response = await httpClient.GetAsync(url);

                await response.EnsureSuccessStatusCodeOrLogAsync(((exception, body) =>
                {
                    Console.WriteLine(exception);
                    Console.WriteLine(body);
                }));
                
                // TODO map errors etc.
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Response from http call: {responseContent}");
                // so it understands we're not embedding a string inside the json
                var responseDeserialized = JsonSerializer.Deserialize<object>(responseContent);

                
                var responseToClient = new RpcCallResponse<object>(deserialized.Url, deserialized.id, responseDeserialized);
                
                
                await _websocketClient.SendAsync(JsonSerializer.Serialize(responseToClient));
            }
            
            void ServerDisconnected(object sender, EventArgs args) 
            {
                Console.WriteLine("Server disconnected");
                args.PrintAsJson();
                // todo reconnect logic here.
            }

            while (stoppingToken.IsCancellationRequested)
            {
                if (!_websocketClient.Connected)
                {
                    Console.WriteLine("Client not connected yet...");
                    _websocketClient.Start();
                }

                await Task.Delay(1000);
            }

            Console.WriteLine("Reverse connect started!");
        }

        public override void Dispose()
        {
            base.Dispose();
            _websocketClient.Dispose();
        }
    }
    
    public record RpcCall(string Url, Guid id);

    public record RpcCallResponse<T>(string Url, Guid id, T data);    
    
    
}

