using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using System.Diagnostics;

namespace EasyMusicBot
{
    internal class MiscModule : IModule
    {
        private ModuleManager Manager;
        private DiscordClient Client;

        void IModule.Install(ModuleManager manager)
        {
            Manager = manager;
            Client = manager.Client;

            manager.CreateCommands("", group =>
            {
                group.PublicOnly();

                group.CreateCommand("echo")
                    .Parameter("EM")
                    .Do(async e =>
                    {
                        Console.WriteLine("poop");
                        await e.Channel.SendMessage(e.GetArg("EM"));
                    });

                group.CreateCommand("debug")
                   .Do(async e =>
                   {
                       StringBuilder sb = new StringBuilder();
                       sb.AppendLine("User ID: " + e.User.Id);
                       sb.AppendLine("User Permission Level: " + Enum.GetName(typeof(PrivillageLevels), Permissions.CheckPrivilage(e.User, e.Channel)));
                       sb.AppendLine("Channel ID: " + e.Channel.Id);
                       sb.AppendLine("Role IDs:");
                       foreach (Role r in e.User.Roles)
                       {
                           if (r.IsEveryone) break;
                           sb.AppendLine(r.Name + ": " + r.Id);
                       }

                       await e.Channel.SendMessage(sb.ToString());

                       foreach (string s in Settings.RequestOps)
                       {
                           await e.Channel.SendMessage(s);
                       }
                   });

                group.CreateCommand("running")
                    .Do(async e =>
                    {
                        Process[] processes = Process.GetProcesses();
                        StringBuilder sb = new StringBuilder();
                        foreach (Process p in processes)
                        {
                            if (!String.IsNullOrEmpty(p.MainWindowTitle))
                            {
                                sb.AppendLine(p.MainWindowTitle);
                            }
                        }
                        await e.Channel.SendMessage(sb.ToString());
                    });
            });
        }
    }
}
