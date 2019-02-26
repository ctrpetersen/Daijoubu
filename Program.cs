using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;


namespace Daijoubu
{
    class Program
    {
        private static void Main(string[] args) => new Daijoubu().StartAsync().GetAwaiter().GetResult();
    }
}
