
using Discord;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;
using Discord.Commands.Permissions.Levels;
using Discord.Commands;
using Discord.Modules;
using EasyMusicBot.Modules;

namespace EasyMusicBot
{
    public class Program
    {
        public static Form1 f;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

       

        public static Discord.Channel LRC;

        
        public DiscordClient Client;

        YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "AIzaSyBTYNvJ80kHSE8AypP7Yst5Fshc8ZibHRA",
        });

        private Discord.Channel DChannel;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run();
        }

        public static void VerifyReady()
        {
            Settings.ReadConfig();
            if (Settings.email == null) throw new Exception();

            if (Settings.password == null) throw new Exception();

            if (Settings.ConfigPerm == null) throw new Exception();

            if (Settings.RequestPerm == null) throw new Exception();

            if (Settings.SkipPerm == null) throw new Exception();

            if (Settings.AudioMethod == null) throw new Exception();

            if (!Settings.AudioMethod.Equals("WMPDl") && !Settings.AudioMethod.Equals("VLCDl") && !Settings.AudioMethod.Equals("VLCSt") && !Settings.AudioMethod.Equals("IE")) throw new Exception();

            if (Settings.DLPath == null) throw new Exception();
        }

        public void Run()
        {
            Settings.ReadConfig();
            Thread t = new Thread(() =>
            {
                f = new EasyMusicBot.Form1(this);
                Application.EnableVisualStyles();
                Application.Run(f);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            Client = new DiscordClient(x =>
            {
                x.AppName = "Easy Music Bot";
                x.AppUrl = "Google.com";
                x.MessageCacheSize = 0;
                x.UsePermissionsCache = false;
                x.EnablePreUpdateEvents = true;
                x.LogLevel = LogSeverity.Info;
                //x.LogHandler = OnLogMessage;
            })
            .UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
                x.HelpMode = HelpMode.Public;
                //x.ExecuteHandler = OnCommandExecuted;
                x.ErrorHandler = OnCommandError;
            })
            .UsingPermissionLevels(Permissions.CheckPrivilage)
            .UsingModules()
            .AddModule<Modules.MusicModule>("Music", ModuleFilter.None)
            .AddModule<MiscModule>("Misc", ModuleFilter.None);
            Client.LoggedIn += (s, e) => {
                foreach (Discord.Channel c in Client.GetServer(104979971667197952).TextChannels)
                {
                    if (c.Name.Equals(Settings.Channel))
                    {
                        DChannel = c;
                    }
                }
            };


            Client.MessageReceived += (s, e) =>
            {
                LRC = e.Message.Channel;
            };

            try
            {
                Client.ExecuteAndWait(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            //await Client.Connect("jetfuelcantmeltmusicbot@gmail.com", "vlamzwo647");
                            await Client.Connect(Settings.email, Settings.password);
                            Console.WriteLine("Connected to Discord with email " + Settings.email);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("We done fucked up "+ex);
                            Client.Log.Error($"Login Failed", ex);
                            await Task.Delay(Client.Config.FailedReconnectDelay);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Oh noes! There seems to be a problem with conencting to Discord!");
                Console.WriteLine("Make sure you typed the email and password fields correctly in the config.txt");
                Console.WriteLine("If you happen across the developer, make sure to tell him this: " + e.Message);
            }
            
        }

        private void OnCommandError(object sender, CommandErrorEventArgs e)
        {
            string msg = e.Exception?.GetBaseException().Message;
            if (msg == null) //No exception - show a generic message
            {
                switch (e.ErrorType)
                {
                    case CommandErrorType.Exception:
                        //msg = "Unknown error.";
                        break;
                    case CommandErrorType.BadPermissions:
                        msg = "You do not have permission to run this command.";
                        break;
                    case CommandErrorType.BadArgCount:
                        //msg = "You provided the incorrect number of arguments for this command.";
                        break;
                    case CommandErrorType.InvalidInput:
                        //msg = "Unable to parse your command, please check your input.";
                        break;
                    case CommandErrorType.UnknownCommand:
                        //msg = "Unknown command.";
                        break;
                }
            }
        }

        public async Task SendCmd(String s, int ms)
        {
            //Discord.Channel che = LRC;
            
            Task<Discord.Message> m = DChannel.SendMessage(s);
            Discord.Message ml = m.Result;
            await Task.Delay(ms);
            await ml.Delete();
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            Debug.WriteLine("I'm out of here");
        }
        public Video GetVideoBySearch(string Message)
        {
            //If message is a youtube link, change message to video ID
            if (Message.Contains("youtu.be"))
            {
                Message = Message.Substring(Message.LastIndexOf('/'));
                if (Message.Contains('?')) Message = Message.Remove(Message.IndexOf('?'));
            }
            if (Message.Contains("youtube.com"))
            {
                Message = Message.Substring(Message.IndexOf('='));
                if (Message.Contains('&')) Message = Message.Remove(Message.IndexOf('&'));
            }

            //Creates search request
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Message;
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 1;

            //Excecutes search and stores response as an array
            var searchListResponse = searchListRequest.Execute();

            //Returns ID of first (only) item in list
            if (searchListResponse.Items.Count != 0)
            {
                VideosResource.ListRequest ResultBuilder = youtubeService.Videos.List("snippet, id, contentDetails");
                ResultBuilder.Id = searchListResponse.Items[0].Id.VideoId;
                return ResultBuilder.Execute().Items[0];
            }
            else
            {
                return null;
            }
        }
    }

}
