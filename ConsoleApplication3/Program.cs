
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

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        String email;
        String password;
        public bool SREnable;
        int MaxLength;
        String ModRole;
        String RequestRole;
        String SkipRole;
        bool SpChannel;
        String Channel;
        public String AudioMethod;
        String VLCPath;
        String DLPath;

        Stopwatch watch = new Stopwatch();
        public bool Skipping = false;

        static Discord.Channel LRC;

        List<String> RequestOps = new List<String>(); //{"!songrequest", "!request", "!play", "!sr", "!add"};
        List<String> SkipOps = new List<String>(); //{"!skip", "!skipsong", "!songskip", "this song sucks", "!next"};

        public List<Video> VidList = new List<Video>();

        public Process CurAM;
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
                    //if (line.StartsWith("#")) break;
                    if (line.Contains("=") && !line.StartsWith("#"))
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
                            case "audioMethod":
                                AudioMethod = line.Split('=')[1];
                                break;
                            case "VLCPath":
                                VLCPath = line.Split('=')[1].Replace('\\', '/');
                                break;
                            case "DLPath":
                                DLPath = line.Split('=')[1].Replace('\\', '/');
                                break;
                            default:
                                break;
                        }
                    }
                    line = sr.ReadLine();
                    if (!line.Contains("Path")) try { line = line.Replace(" ", ""); } catch { }

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
            //MessageBox.Show(AudioMethod);
            if (email == null) throw new Exception();

            if (password == null) throw new Exception();

            if (ModRole == null) throw new Exception();

            if (RequestRole == null) throw new Exception();

            if (SkipRole == null) throw new Exception();

            if (Channel == null) throw new Exception();

            if (AudioMethod == null) throw new Exception();

            if (!AudioMethod.Equals("WMPDl") && !AudioMethod.Equals("VLCDl") && !AudioMethod.Equals("VLCSt") && !AudioMethod.Equals("IE")) throw new Exception();

            if (DLPath == null) throw new Exception();
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



            VidLoopAsync();

            Client.Connected += (s, e) => {
                Console.WriteLine("Connected to Discord with email " + email);
                //Client.SetGame(null);
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

        async void InterpretCommand(Discord.Message e)
        {
            if (e.Text.ToLower().Equals("!pause"))
            {
                if (AudioMethod.Contains("VLC"))
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
                if (AudioMethod.Contains("VLC"))
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
                        SetDiscordStatus();
                    }
                    else
                    {
                        SREnable = false;
                        SetDiscordStatus();
                    }
                    await e.Channel.SendMessage("Songrequest has been set to " + SREnable);
                }
                else
                {
                    await e.Channel.SendMessage("Sorry, you don't have permission to do that");
                }
                return;
            }
            if (e.Text.Equals("T")) SetDiscordStatus();

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
                Console.WriteLine("Config command recieved!");
                if (!CheckPrivilage(e.User, e.Server, ModRole) && !e.User.Name.Equals("Ian"))
                {
                    await e.Channel.SendMessage("Sorry you don't have permission to do that. Required role: "+ModRole);
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
                    if (e.Text.Split(' ')[1].ToLower().Equals("channel"))
                    {
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        Channel = e.Text.Split(' ')[2];
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("audiomethod"))
                    {
                        ClearPlaylist();
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        AudioMethod = e.Text.Split(' ')[2];
                    }
                    if (e.Text.Split(' ')[1].ToLower().Equals("dlpath"))
                    {
                        ClearPlaylist();
                        await e.Channel.SendMessage("Config changed!");
                        Console.WriteLine("Thing changed!");
                        DLPath = e.Text.Split(' ')[2];
                    }
                    VerifyReady();
                }
                catch
                {
                    await e.Channel.SendMessage("Hmm. That didn't seem to work please try again");
                }
            }

            if (e.Text.ToLower().Equals("!skipall") || e.Text.ToLower().Equals("!clearplaylist"))
            {
                if (SpChannel && !e.Channel.Name.Equals(Channel))
                {
                    await e.Channel.SendMessage("This isn't " + Channel + ". Please direct all your songrequesty needs there");
                    return;
                }
                if (!CheckPrivilage(e.User, e.Server, ModRole) && !e.User.Name.Equals("Ian"))
                {
                    await e.Channel.SendMessage("Sorry you don't have permission to do that. Required role: " + ModRole);
                }
                Skipping = true;
                ClearPlaylist();
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
                    //MessageBox.Show(e.Text.Remove(0, st.Length + 1));
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
                    //MessageBox.Show("SKip");
                    Skipping = true;
                    f.axWindowsMediaPlayer1.Ctlcontrols.stop();
                    try { CurAM.Kill(); } catch { }
                    await PutTaskDelay(110);
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
            if (rName.Equals("@everyone"))
            {
                return true;
            }
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
                if(Message.Contains('&')) Message = Message.Remove(Message.IndexOf('&'));
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
            if (Result.Snippet.ChannelTitle.ToLower().Contains("vevo"))
            {
                //await LRC.SendMessage("Sorry, Vevo videos are currently nut supported by EasyMusicbot");
                //return;
            }

            if (AudioMethod.Contains("Dl"))
            {
                Thread t = new Thread(() => { DownloadAudio(Result.Id); });
                t.Start();
            }
            VidList.Add(Result);
            f.BoxHandler();
            Console.WriteLine("Added video " + Result.Snippet.Title);
            await LRC.SendMessage("Added video *" + Result.Snippet.Title + "*");
        }

        int GetVidLength(Video v)
        {
            try
            {
                String Original = v.ContentDetails.Duration;
                String Fixed = Original.Trim('P', 'T');
                int total = 0;

                if (Fixed.Contains("H"))
                {
                    total += Convert.ToInt32(Fixed.Split('H')[0]) * 3600;
                    Fixed = Fixed.Substring(Fixed.IndexOf('H') + 1);
                }
                if (Fixed.Contains("M"))
                {
                    total += Convert.ToInt32(Fixed.Split('M')[0]) * 60;
                    Fixed = Fixed.Substring(Fixed.IndexOf('M') + 1);
                }
                if (Fixed.Contains("S"))
                {
                    total += Convert.ToInt32(Fixed.TrimEnd('S'));
                    Fixed = Fixed.Substring(Fixed.IndexOf('S') + 1);
                }
                //MessageBox.Show(total.ToString());
                return total;
            }
            catch
            {
                return MaxLength + 1;
            }
            
        }

        async Task VidLoopAsync()
        {
            while (true)
            {
                while (0 >= VidList.Count)
                {
                    //Console.WriteLine("Waiting for video");
                    await PutTaskDelay(1000);
                }

                if (AudioMethod.Equals("VLCDl")) await PlayDLVid(VidList[0]);

                else if (AudioMethod.Equals("VLCSt")) await PlaySTVid(VidList[0]);

                else if (AudioMethod.Equals("WMPDl")) await WMPPlay(VidList[0]);

                Client.SetGame(null);

                f.BoxHandler();
                Console.WriteLine("Done!");
            }
        }

        async Task WMPPlay(Video v)
        {
            while (!DownloadExists(v, true))
            {
                Client.SetGame("Waiting for download");
                Console.WriteLine("Waiting for download");
                await PutTaskDelay(1000);
            }
            Client.SetGame(VidList[0].Snippet.Title);

            f.axWindowsMediaPlayer1.URL = (v.Id.Remove(v.Id.Length - 4) + "!done.mp3");
            f.axWindowsMediaPlayer1.Ctlcontrols.play();
            while (f.axWindowsMediaPlayer1.playState != WMPLib.WMPPlayState.wmppsStopped)
            {
                await PutTaskDelay(100);
            }
            VidList.RemoveAt(0);
        }

        async Task PlayDLVid(Video v)
        {
            Console.WriteLine("Trying to play video");
            while (!DownloadExists(v, true))
            {
                Client.SetGame("Waiting for download");
                Console.WriteLine("Waiting for download");
                await PutTaskDelay(1000);
            }
            Client.SetGame(VidList[0].Snippet.Title);

            Console.WriteLine(v.Snippet.Title + " Playling download");

            CurAM = Process.Start(VLCPath+"/vlc.exe", "file:///" + DLPath + "/" + v.Id + "!done.mp3 --qt-start-minimized");

            while (!CurAM.HasExited)
            {
                await PutTaskDelay(100);
            }
            Console.WriteLine("VLC has exited");
            VidList.RemoveAt(0);

        }
        
        async Task PlaySTVid(Video v)
        {
            Client.SetGame(VidList[0].Snippet.Title);

            CurAM = Process.Start(VLCPath + "/vlc.exe", " --no-video http://www.youtube.com/watch?v=" + v.Id + " --qt-start-minimized");

            Skipping = false;
            watch.Restart();

            while (!CurAM.HasExited) await PutTaskDelay(100);

            if (watch.ElapsedMilliseconds < 10000 && GetVidLength(v) > 10 && !Skipping)
            {
                if (DownloadExists(v, true)) await LRC.SendMessage("Video appears to encrypted, playing download");
                else
                {
                    await LRC.SendMessage("Video appears to be encrypted, starting download");

                    //Thread t = new Thread(() => { DownloadAudio(v.Id); });
                    //t.Start();
                    Client.SetGame("Waiting for download");
                    DownloadAudio(v.Id);
                    await LRC.SendMessage("Download finished");
                }
                await PlayDLVid(v);
                return;
            }
            Skipping = false;
            Console.WriteLine("VLC has exited");
            VidList.RemoveAt(0);
        }

        void DownloadAudio(String Id)
        {
            // TODO: FIX THIS SHIT
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls("http://www.youtube.com/watch?v=" + Id);

            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

            if (video.RequiresDecryption) DownloadUrlResolver.DecryptDownloadUrl(video);

            var audioDownloader = new AudioDownloader(video, Path.Combine("C:/Downloads", Id + video.AudioExtension));

            double progress = 0;
            audioDownloader.DownloadProgressChanged += (sender, args) =>
            {
                if (!Math.Round(args.ProgressPercentage * 0.85, 0, MidpointRounding.AwayFromZero).Equals(progress))
                {
                    progress = Math.Round(args.ProgressPercentage * 0.85, 0, MidpointRounding.AwayFromZero);
                    Console.WriteLine(progress);
                }
            };
            audioDownloader.AudioExtractionProgressChanged += (sender, args) =>
            {
                if (!Math.Round(args.ProgressPercentage * 0.85, 0, MidpointRounding.AwayFromZero).Equals(progress))
                {
                    progress = Math.Round(args.ProgressPercentage * 0.85, 0, MidpointRounding.AwayFromZero);
                    Console.WriteLine(progress);
                }
            };

            if (DownloadExists(GetVideoBySearch(Id), true)) return;

            try
            {
                audioDownloader.Execute();
            }

            catch(System.Net.WebException e)
            {
                LRC.SendMessage("Sorry, that video cannot be added");
                try { VidList.Remove(GetVideoBySearch(Id)); } catch { }
                return;
            }
            
            
            int dotIndex = audioDownloader.SavePath.LastIndexOf(".");
            String NewName = audioDownloader.SavePath.Insert(dotIndex, "!done");
            audioDownloader.DownloadFinished += (s, e) => System.IO.File.Move(audioDownloader.SavePath, NewName);
            System.IO.File.Move(audioDownloader.SavePath, NewName);
            Console.WriteLine("Download finished!");
            return;

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
        void ClearPlaylist()
        {
            f.axWindowsMediaPlayer1.Ctlcontrols.stop();
            VidList.Clear();
            try { CurAM.Kill(); } catch { }
        }
        bool DownloadExists(Video v, bool done)
        {
            if (done) if (File.Exists(DLPath + "/" + v.Id + "!done.mp3")) return true;
            if (File.Exists(DLPath + "/" + v.Id + "!done.mp3")) return true;
            return false;
        }
        public void SetDiscordStatus()
        {
            if(SREnable) Client.SetStatus(UserStatus.Online);
            if(!SREnable) Client.SetStatus(UserStatus.Idle);
            //MessageBox.Show(SREnable.ToString());
        }
        static void OnProcessExit(object sender, EventArgs e)
        {
            Debug.WriteLine("I'm out of here");
        }
    }

}
