using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Timers;



namespace Daijoubu
{
    class Daijoubu
    {
        internal DiscordSocketClient Client;
        internal string Token;
        internal SocketGuild Guild;
        internal ulong LiveRoleId = 441683574333112340;
        internal Timer Timer = new Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);

        public async Task StartAsync()
        {
            Token = File.ReadAllText("token.txt");
            Timer.AutoReset = true;
            Timer.Elapsed += CheckUsers;

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug
            });

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();


            Client.Ready += () =>
            {
                Log(new LogMessage(LogSeverity.Info, "´Daijoubu", $"Logged in as {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}." +
                                                              $"\nServing {Client.Guilds.Count} guilds with a total of {Client.Guilds.Sum(g => g.Users.Count)} online users."));
                Guild = Client.GetGuild(139677590393716737);
                Timer.Start();

                return Task.CompletedTask;
            };
            await Task.Delay(-1);
        }

        private void CheckUsers(object sender, ElapsedEventArgs e)
        {
            Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Checking users"));

            foreach (var user in Guild.Users)
            {
                if (user.Activity != null && user.Activity.Type == ActivityType.Streaming && user.Activity is Game game)
                {
                    if (game.Details == "Factorio" && user.Roles.All(r => r.Id != LiveRoleId))
                    {
                        Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Added live role to {user}"));
                        user.AddRoleAsync(Guild.GetRole(LiveRoleId));
                    }

                    else if (game.Details != "Factorio" && user.Roles.Any(r => r.Id == LiveRoleId))
                    {
                        Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Removed live role from {user}"));
                        user.RemoveRoleAsync(Guild.GetRole(LiveRoleId));
                    }
                }

                else if (user.Roles.Any(r => r.Id == LiveRoleId))
                {
                    Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Removed live role from {user}"));
                    user.RemoveRoleAsync(Guild.GetRole(LiveRoleId));
                }
            }
        }

        private static Task Log(LogMessage logmsg)
        {
            switch (logmsg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now} [{logmsg.Severity,8}] {logmsg.Source}: {logmsg.Message} {logmsg.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
