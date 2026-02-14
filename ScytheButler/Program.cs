using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using ScytheButler.Core;

class Program
{
    public static Task Main(string[] args)
        => new Bot().RunAsync();
}
