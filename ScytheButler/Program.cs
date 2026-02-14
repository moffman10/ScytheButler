using Discord;
using Discord.WebSocket;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using ScytheButler.Core;

class Program
{
    public static async Task Main(string[] args)
    {
        // Start Discord bot
        var botTask = new Bot().RunAsync();

        // Start a dummy TCP listener on Fly.io's assigned port
        _ = Task.Run(() => StartTcpListener());

        await botTask;
    }

    private static void StartTcpListener()
    {
        int port = int.TryParse(Environment.GetEnvironmentVariable("PORT"), out var p) ? p : 8080;
        var listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Dummy TCP listener running on port {port}");

        while (true)
        {
            // Accept connections (we don't actually need to respond)
            var client = listener.AcceptTcpClient();
            client.Close();
        }
    }
}
