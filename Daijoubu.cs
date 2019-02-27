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
    internal class Daijoubu
    {
        internal DiscordSocketClient Client;
        internal string Token;
        internal SocketGuild Guild;
        internal ulong LiveRoleId = 441683574333112340;
        internal ulong WeebRoleId = 334669440421462017;
        internal string GameToTrack = "Factorio";
        internal Timer Timer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);

        public async Task StartAsync()
        {
            Token = File.ReadAllText("token.txt");
            Timer.AutoReset = true;
            Timer.Elapsed += CheckUsers;

            Client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
                MessageCacheSize = 100
            });

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            Client.Ready += () =>
            {
                Log(new LogMessage(LogSeverity.Info, "´Daijoubu",
                    $"Logged in as {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}." +
                    $"\nServing {Client.Guilds.Count} guilds with a total of {Client.Guilds.Sum(g => g.Users.Count)} online users."));
                Guild = Client.GetGuild(139677590393716737);
                Timer.Start();

                return Task.CompletedTask;
            };

            Client.ReactionAdded += AddWeeb;
            Client.ReactionRemoved += RemoveWeeb;

            await Task.Delay(-1);
        }

        private Task AddWeeb(Cacheable<IUserMessage, ulong> cMsg, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (cMsg.Id != 549988110423818240) return Task.CompletedTask;
            var reactionAuthor = Guild.GetUser(reaction.UserId);

            if (reactionAuthor.Roles.Any(r => r.Id == WeebRoleId)) return Task.CompletedTask;
            Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Added weeb role to {reactionAuthor}"));
            reactionAuthor.AddRoleAsync(Guild.GetRole(WeebRoleId));

            return Task.CompletedTask;
        }

        private Task RemoveWeeb(Cacheable<IUserMessage, ulong> cMsg, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (cMsg.Id != 549988110423818240) return Task.CompletedTask;
            var reactionAuthor = Guild.GetUser(reaction.UserId);

            if (reactionAuthor.Roles.All(r => r.Id != WeebRoleId)) return Task.CompletedTask;
            Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Removed weeb role from {reactionAuthor}"));
            reactionAuthor.RemoveRoleAsync(Guild.GetRole(WeebRoleId));

            return Task.CompletedTask;
        }

        private void CheckUsers(object sender, ElapsedEventArgs e)
        {
            Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Checking users"));

            foreach (var user in Guild.Users)
            {
                if (user.Activity != null && user.Activity.Type == ActivityType.Streaming && user.Activity is Game game)
                {
                    if (game.Details == GameToTrack && user.Roles.All(r => r.Id != LiveRoleId))
                    {
                        user.AddRoleAsync(Guild.GetRole(LiveRoleId));
                        Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Added live role to {user}"));
                    }

                    else if (game.Details != GameToTrack && user.Roles.Any(r => r.Id == LiveRoleId))
                    {
                        user.RemoveRoleAsync(Guild.GetRole(LiveRoleId));
                        Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Removed live role from {user}"));
                    }
                }

                else if (user.Roles.Any(r => r.Id == LiveRoleId))
                {
                    user.RemoveRoleAsync(Guild.GetRole(LiveRoleId));
                    Log(new LogMessage(LogSeverity.Info, "Daijoubu", $"Removed live role from {user}"));
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

            Console.WriteLine(
                $"{DateTime.Now} [{logmsg.Severity,8}] {logmsg.Source}: {logmsg.Message} {logmsg.Exception}");
            Console.ResetColor();
            return Task.CompletedTask;
        }
    }
}
