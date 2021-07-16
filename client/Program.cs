using System;
using System.Dynamic;
using System.Text;
using System.Threading.Tasks;
using client;
using SCM.SwissArmyKnife.Extensions;
using WatsonWebsocket;

WatsonWsServer server = new WatsonWsServer("127.0.0.1", 8081, false);
server.ClientConnected += ClientConnected;
server.ClientDisconnected += ClientDisconnected;
server.MessageReceived += MessageReceived; 
server.Start();

async void ClientConnected(object sender, ClientConnectedEventArgs args) 
{
    Console.WriteLine("Client connected: " + args.IpPort);

    var reverseHttpClient = new ReverseConnectHttpClient(server, args.IpPort);

    Console.WriteLine("Sending http request");
    var response = await reverseHttpClient.GetAsync<object>("http://localhost:5000/weather");
    Console.WriteLine($"Server response");
    Console.WriteLine(response);
}

void ClientDisconnected(object sender, ClientDisconnectedEventArgs args) 
{
    Console.WriteLine("Client disconnected: " + args.IpPort);
}

async void MessageReceived(object sender, MessageReceivedEventArgs args) 
{ 
    Console.WriteLine("Message received from " + args.IpPort + ": " + Encoding.UTF8.GetString(args.Data));

    var data = Encoding.UTF8.GetString(args.Data);
    
    
}


bool keepRunning = true;


Console.CancelKeyPress += myHandler;

void myHandler(object sender, ConsoleCancelEventArgs args)
{
    Console.WriteLine("Cancel pressed");
    keepRunning = false;
}

Console.WriteLine("Server started");

while (keepRunning == true)
{
    await Task.Delay(500);
}