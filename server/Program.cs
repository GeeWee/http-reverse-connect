using System;
using System.Text;
using System.Threading.Tasks;
using WatsonWebsocket;

WatsonWsClient client = new WatsonWsClient("localhost", 8081, false);
client.ServerConnected += ServerConnected;
client.ServerDisconnected += ServerDisconnected;
client.MessageReceived += MessageReceived; 
client.Start();

void MessageReceived(object sender, MessageReceivedEventArgs args) 
{
    Console.WriteLine("Message from server: " + Encoding.UTF8.GetString(args.Data));
}

async void ServerConnected(object sender, EventArgs args) 
{
    // Here the server is now connected
    Console.WriteLine("Client access established");

    var response = await client.SendAndWaitAsync("doRequest");
    Console.WriteLine("RESPONSE??");
    Console.WriteLine(response);
}

void ServerDisconnected(object sender, EventArgs args) 
{
    Console.WriteLine("Server disconnected");
}

Console.WriteLine("Client started");



bool keepRunning = true;



Console.CancelKeyPress += myHandler;

void myHandler(object sender, ConsoleCancelEventArgs args)
{
    Console.WriteLine("Cancel pressed");
    keepRunning = false;
}

while (keepRunning == true)
{
    await Task.Delay(500);
}