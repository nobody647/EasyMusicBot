
using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Userlist;
using Discord.Modules;
using Discord.Audio;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord.Legacy;
using YoutubeExtractor;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EasyMusicBot
{
    public class Program
    {
        EasyMusicBot.Form1 f;

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);


        String email;
        String password;
        public bool SREnable;
        int MaxLength;
        String ModRole;
        String RequestRole;
        String SkipRole;
        bool SpChannel;
        String Channel;
        public bool UseVLC;

        static Discord.Channel LRC;

        List<String> RequestOps = new List<String>(); //{"!songrequest", "!request", "!play", "!sr", "!add"};
        List<String> SkipOps = new List<String>(); //{"!skip", "!skipsong", "!songskip", "this song sucks", "!next"};

        public List<Video> VidList = new List<Video>();

        public Process CurVLC;
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

        void ReadConfig()
        {
            try
            {
                StreamReader sr = new StreamReader("config.txt");

                String line = sr.ReadLine();
                line = line.Replace(" ", "");

                while (line != null)
                {
                    if (line.Contains("="))
                    {
                        switch (line.Split('=')[0])
                        {
                            case "email":
                                //MessageBox.Show("Found instance of email! " + line);
                                email = line.Split('=')[1];
                                break;
                            case "password":
                                //MessageBox.Show("Found instance of password! " + line);
                                password = line.Split('=')[1];
                                break;
                            case "srDefault":
                                SREnable = Convert.ToBoolean(line.Split('=')[1]);
                                break;
                            case "requestOps":
                                String[] opsAdd = line.Split('=')[1].Split(',');
                                foreach (string s in opsAdd)
                                {
                                    RequestOps.Add(s);
                                }
                                break;
                            case "skipOps":
                                String[] opsAdd2 = line.Split('=')[1].Split(',');
                                foreach (string s in opsAdd2)
                                {
                                    SkipOps.Add(s);
                                }
                                break;
                            case "maxLength":
                                MaxLength = Convert.ToInt32(line.Split('=')[1]);
                                break;
                            case "modifyRole":
                                ModRole = line.Split('=')[1];
                                break;
                            case "requestRole":
                                RequestRole = line.Split('=')[1];
                                break;
                            case "skipRole":
                                SkipRole = line.Split('=')[1];
                                break;
                            case "spChannel":
                                SpChannel = Convert.ToBoolean(line.Split('=')[1]);
                                break;
                            case "channel":
                                Channel = line.Split('=')[1];
                                break;
                            case "useVLC":
                                UseVLC = Convert.ToBoolean(line.Split('=')[1]);
                                break;
                            default:
                                break;
                        }
                    }
                    line = sr.ReadLine();
                    try
                    {
                        line = line.Replace(" ", "");
                    }
                    catch { }

                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Oh noes! There was a problem reading the config.txt file!");
                Console.WriteLine("If you happen across the devs, make sure to give them this: " + e.Message);
            }

        }

        void WriteToConfig(String a, String b)
        {
            StreamReader sr = new StreamReader("config.txt");

            String line = sr.ReadLine();
            line = line.Replace(" ", "");
            while (!line.Contains(a))
            {
                line = sr.ReadLine();
            }
            StreamWriter sw = new StreamWriter("config.txt");
            //sw.
        }

        void VerifyReady()
        {
            if (email != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (password != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (SREnable != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (MaxLength != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (ModRole != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (RequestRole != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (SkipRole != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (SpChannel != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (Channel != null)
            {

            }
            else
            {
                throw new Exception();
            }
            if (UseVLC != null)
            {

            }
            else
            {
                throw new Exception();
            }
        }

        public void Run()
        {

            ReadConfig();
            VerifyReady();

            Thread t = new Thread(() =>
            {
                f = new EasyMusicBot.Form1(this);
                Application.EnableVisualStyles();
                Application.Run(f);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();



            Client = new DiscordClient(new DiscordConfig
            {
                AppName = "Easy Music Bot",
                AppUrl = "Put url here",
                //AppVersion = DiscordConfig.LibVersion,
                LogLevel = LogSeverity.Info,
                MessageCacheSize = 0,
                UsePermissionsCache = false
            });



            DLVidLoopAsync();

            Client.Connected += (s, e) => {
                Console.WriteLine("Connected to Discord with email " + email);
                foreach (Discord.Channel c in Client.GetServer(104979971667197952).TextChannels)
                {
                    if (c.Name.Equals(Channel))
                    {
                        //MessageBox.Show("che set to " + c.Name);
                        DChannel = c;
                    }
                }
            };


            Client.MessageReceived += (s, e) =>
            {
                LRC = e.Message.Channel;
                Console.WriteLine("Message recieved!");
                InterpretCommand(e.Message);


            };

            try
            {
                Client.Run(async () =>
                {
                    await Client.Connect(email, password);

                });
            }
            catch (Exception e)
            {
                Console.WriteLine("Oh noes! There seems to be a problem with conencting to Discord!");
                Console.WriteLine("Make sure you typed the email and password fields correctly in the config.txt");
                Console.WriteLine("If you happen across the developer, make sure to tell him this: " + e.Message);
            }
            MessageBox.Show("t");
            
        }

        private void Log_Message(object sender, LogMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        async void InterpretCommand(Discord.Message e)
        {
            if (e.Text.ToLower().Equals("!pause"))
            {
                if (UseVLC)
                {
                    SendKeys.SendWait("%(j)");
                }
                else
                {
                    f.axWindowsMediaPlayer1.Ctlcontrols.pause();
                }
                return;
            }
            if (e.Text.ToLower().Equals("!resume") || e.Text.ToLower().Equals("!play"))
            {
                if (UseVLC)
                {
                    SendKeys.SendWait("%(j)");
                }
                else
                {
                    f.axWindowsMediaPlayer1.Ctlcontrols.pause();
                }
                return;
            }
            if (e.Text.ToLower().Equals("!srtoggle"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }
                if (CheckPrivilage(e.User, e.Server, ModRole))
                {
                    await e.Channel.SendMessage("Songrequest toggled");
                    if (SREnable == false)
                    {
                        SREnable = true;
                    }
                    else
                    {
                        SREnable = false;
                    }
                    await e.Channel.SendMessage("Songrequest has been set to " + SREnable);
                }
                else
                {
                    await e.Channel.SendMessage("Sorry, you don't have permission to do that");
                }
                return;
            }

            if (e.Text.ToLower().Equals("!playlist"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("There are " + (VidList.Count) + " videos in the playlist");
                for (int i = 0; i <= 10; i++)
                {
                    try
                    {
                        sb.AppendLine("[" + i + "] " + VidList[i].Snippet.Title);
                    }
                    catch { }

                }
                await e.Channel.SendMessage(sb.ToString());
                return;
            }

            if (e.Text.ToLower().StartsWith("!config"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }
                Console.WriteLine("Config command recieved!");
                if (!CheckPrivilage(e.User, e.Server, ModRole) && !e.User.Name.Equals("Ian"))
                {
                    await e.Channel.SendMessage("Sorry you don't have permission to do that");
                    return;
                }
                try
                {
                    if (e.Text.Split(' ')[1].ToLower().Equals("maxlength"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        MaxLength = Convert.ToInt32(e.Text.Split(' ')[2]);
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("modrole"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        ModRole = e.Text.Split(' ')[2];
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("skiprole"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        SkipRole = e.Text.Split(' ')[2];
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("requestrole"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        RequestRole = e.Text.Split(' ')[2];
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("spchannel"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        SpChannel = Convert.ToBoolean(e.Text.Split(' ')[2]);
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("channel"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        Channel = e.Text.Split(' ')[2];
                    }
                }
                catch
                {
                    await e.Channel.SendMessage("Hmm. That didn't seem to work please try again");
                }
            }

            if (e.Text.ToLower().Equals("!skipall"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }
                if (!CheckPrivilage(e.User, e.Server, SkipRole) && !e.User.Name.Equals("Ian"))
                {
                    await e.Channel.SendMessage("Sorry you don't have permission to do that");
                }
                f.axWindowsMediaPlayer1.Ctlcontrols.stop();
                try { CurVLC.Kill(); } catch { }
                VidList.Clear();
                await e.Channel.SendMessage("Playlist cleared");
            }

            foreach (String st in RequestOps)
            {
                if (e.Text.ToLower().StartsWith(st))
                {
                    if (SpChannel && !e.Channel.Name.Equals(Channel))
                    {
                        await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                        break;
                    }
                    if (!CheckPrivilage(e.User, e.Server, SkipRole))
                    {
                        await e.Channel.SendMessage("Sorry, you don't have permission to do that. Required role: " + SkipRole);
                        break;
                    }
                    if (SREnable == false)
                    {
                        await e.Channel.SendMessage("Sorry, song requesting has been disabled. Use !srToggle to enable");
                        break;
                    }
                    if (e.Text.ToLower().Equals(st))
                    {
                        break;
                    }
                    await VidAdd(e.Text.Remove(0, st.Length + 1));
                }
            }
            foreach (String st in SkipOps)
            {
                if (e.Text.ToLower().StartsWith(st))
                {
                    if (SpChannel && !e.Channel.Name.Equals(Channel))
                    {
                        await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                        break;
                    }
                    if (!CheckPrivilage(e.User, e.Server, SkipRole))
                    {
                        await e.Channel.SendMessage("Sorry, you don't have permission to do that. Required role: " + SkipRole);
                        break;
                    }

                    if (e.Text.Length > st.Length)
                    {
                        int index;
                        if (int.TryParse(e.Text.ToLower().Remove(0, st.Length + 1), out index))
                        {
                            MessageBox.Show("Int");
                            await e.Channel.SendMessage("Removed video in position [" + index + "], *" + VidList[index].Snippet.Title + "*");
                            VidList.RemoveAt(index);
                            f.BoxHandler();
                            return;
                        }
                        else if (VidList.Contains(GetVideoBySearch(e.Text.Remove(0, st.Length + 1))))
                        {
                            MessageBox.Show("vid");
                            Video v = GetVideoBySearch(e.Text.Remove(0, 8));
                            await e.Channel.SendMessage("Removed video *" + v.Snippet.Title + "*");
                            VidList.Remove(v);
                            f.BoxHandler();
                            return;
                        }
                    }
                    f.axWindowsMediaPlayer1.Ctlcontrols.stop();
                    try { CurVLC.Kill(); } catch { }
                    f.BoxHandler();
                }
            }

            if (e.Text.Equals("!debug"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }
                await e.Channel.SendMessage("The debug command was never actually useful. Consequentally, no one used it. I took it out because it was like 10 lines and it makes my program look cleaner. Here's a picture of a sad cat. http://www.theluxuryspot.com/wp-content/uploads/2013/05/Screen-shot-2013-05-09-at-5.10.47-PM.png");
            }
        }

        Boolean CheckPrivilage(User u, Server s, String rName)
        {
            foreach (Role r in u.Roles)
            {
                //MessageBox.Show(r.Name);
                if (r.Name.Equals(rName))
                {
                    return true;
                }
            }
            return false;
        }

        public Video GetVideoBySearch(string message)
        {
            if (message.Contains("youtube.com"))
            {
                String Uncheked = message.Split('=')[1];
                try
                {
                    message = Uncheked.Split('&')[0];
                }
                catch { }
                message = Uncheked;
            }
            //Creates search request
            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = message;
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

        async Task VidAdd(string message)
        {
            Video Result = GetVideoBySearch(message);

            if (Result == null)
            {
                await LRC.SendMessage("Sorry, we couldn't find a video for *" + message + "*");
                return;
            }
            if (!SREnable)
            {
                await LRC.SendMessage("Sorry, song requesting has been disabled");
                return;
            }
            if (VidList.Contains(Result))
            {
                await LRC.SendMessage("Video *" + Result.Snippet.Title + "* is already in the playlist");
                return;
            }
            if (GetVidLength(Result) > MaxLength && MaxLength != 0)
            {
                await LRC.SendMessage("Video *" + Result.Snippet.Title + "* is " + GetVidLength(Result) + " seconds long. This is longer than our max length of " + MaxLength);
                Console.WriteLine("Video " + Result.Snippet.Title + " is " + GetVidLength(Result) + " long. This is longer than our max length of " + MaxLength);
                return;
            }

            Thread t = new Thread(() => { DownloadAudio("http://www.youtube.com/watch?v=" + Result.Id); });
            t.Start();
            VidList.Add(Result);
            //f.BoxHandler();
            Console.WriteLine("Added video " + Result.Snippet.Title);
            await LRC.SendMessage("Added video *" + Result.Snippet.Title + "*");
        }

        int GetVidLength(Video v)
        {
            String Original = v.ContentDetails.Duration;
            String Fixed = Original.Trim('P', 'T', 'S');
            int total;
            if (Fixed.Contains("H"))
            {
                //LRC.SendMessage("Video is over one hour long");
                return (MaxLength + 10);
            }
            if (Fixed.Contains("M"))
            {
                String[] Split = Fixed.Split('M');
                string mins = Split[0];
                Console.WriteLine("Video is " + Split[0] + " minutes and " + Split[1] + " seconds long");
                total = (Convert.ToInt32(mins) * 60) + Convert.ToInt32(Split[1]);
                return total;
            }
            else
            {
                Console.WriteLine("Video is " + Fixed + " seconds long");
                total = Int32.Parse(Fixed);
                return total;
            }
        }

        async Task DLVidLoopAsync()
        {
            while (true)
            {
                while (0 >= VidList.Count)
                {
                    Console.WriteLine("Waiting for video");
                    await PutTaskDelay(1000);
                }
                if (UseVLC)
                {
                    await PlayDLVid("C:/Downloads/" + VidList[0].Snippet.Title + ".mp3");
                }
                else
                {
                    await WMPPlay("C:/Downloads/" + VidList[0].Snippet.Title + ".mp3");
                }

                //f.BoxHandler();
                Console.WriteLine("Done!");
            }
        }

        async Task WMPPlay(string name)
        {
            while (!File.Exists(name.Remove(name.Length - 4) + " !done.mp3"))
            {
                Console.WriteLine("Waiting for download");
                await PutTaskDelay(1000);
            }
            f.axWindowsMediaPlayer1.URL = (name.Remove(name.Length - 4) + " !done.mp3");
            f.axWindowsMediaPlayer1.Ctlcontrols.play();
            while (f.axWindowsMediaPlayer1.playState != WMPLib.WMPPlayState.wmppsStopped)
            {
                await PutTaskDelay(1000);
            }
            VidList.RemoveAt(0);
        }

        async Task PlayDLVid(String name)
        {
            Console.WriteLine("Trying to play video");
            while (!File.Exists(name.Remove(name.Length - 4) + " !done.mp3"))
            {
                Console.WriteLine("Waiting for download");
                await PutTaskDelay(1000);
            }

            Console.WriteLine(name + " Playling download");

            CurVLC = Process.Start("C:/Program Files (x86)/VideoLAN/VLC/vlc.exe", "file:///" + Uri.EscapeUriString(name.Remove(name.Length - 4) + " !done.mp3") + " --play-and-exit --qt-start-minimized");

            while (!CurVLC.HasExited)
            {
                await PutTaskDelay(1000);
            }
            Console.WriteLine("VLC has exited");
            VidList.RemoveAt(0);

        }

        void DownloadAudio(String link)
        {

            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            /*
             * If the video has a decrypted signature, decipher it
             */
            if (video.RequiresDecryption)
            {
                DownloadUrlResolver.DecryptDownloadUrl(video);
            }

            /*
             * Create the audio downloader.
             * The first argument is the video where the audio should be extracted from.
             * The second argument is the path to save the audio file.
             */
            var audioDownloader = new AudioDownloader(video, Path.Combine("C:/Downloads", video.Title + video.AudioExtension));

            // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
            // because the download will take much longer than the audio extraction.
            audioDownloader.DownloadProgressChanged += (sender, args) => Console.WriteLine(args.ProgressPercentage * 0.85);
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => Console.WriteLine(85 + args.ProgressPercentage * 0.15);

            /*
             * Execute the audio downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            if (File.Exists(audioDownloader.SavePath.Remove(audioDownloader.SavePath.Length - 4) + " !done.mp3"))
            {
                return;
            }
            audioDownloader.Execute();
            int dotIndex = audioDownloader.SavePath.LastIndexOf(".");
            String NewName = audioDownloader.SavePath.Insert(dotIndex, " !done");
            audioDownloader.DownloadFinished += (s, e) => System.IO.File.Move(audioDownloader.SavePath, NewName);
            System.IO.File.Move(audioDownloader.SavePath, NewName);

            System.Threading.Thread.CurrentThread.Suspend();


        }

        async Task PutTaskDelay(int delay)
        {
            await Task.Delay(delay);
        }

        public async Task SendCmd(String ms)
        {
            //Discord.Channel che = LRC;
            
            Task<Discord.Message> m = DChannel.SendMessage(ms);
            Discord.Message ml = m.Result;
            await PutTaskDelay(500);
            await ml.Delete();
        }
    }

}
