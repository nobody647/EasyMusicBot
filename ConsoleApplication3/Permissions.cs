using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Modules;
using Discord.Commands.Permissions;
using Discord.Commands;

namespace EasyMusicBot
{
    public enum PrivillageLevels
    {
        Guest = 0,
        Member = 1,
        Mod = 2,
        Admin = 3,
        Dev = 3
    }
    public static class Permissions
    {
        public static int CheckPrivilage(User u, Channel ch)
        {
            //if(isDev) return 4
            if (u.ServerPermissions.ManageServer) return 3;
            if (u.ServerPermissions.ManageChannels) return 2;
            if (u.Roles.Count() > 1) return 1;
            return 0;
        }
    }
}
