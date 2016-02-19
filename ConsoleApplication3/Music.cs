using Discord;
using Discord.Commands;
using Discord.Commands.Permissions.Levels;
using Discord.Commands.Permissions.Visibility;
using Discord.Modules;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExtractor;

namespace EasyMusicBot.Modules
{
    internal class MusicModule : IModule
    {
        YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = "AIzaSyBTYNvJ80kHSE8AypP7Yst5Fshc8ZibHRA",
        });
        public static List<Video> VidList = new List<Video>();
        Stopwatch watch = new Stopwatch();
        public static bool Skipping = false;

        private ModuleManager _manager;
        private DiscordClient _client;

        void IModule.Install(ModuleManager manager)
        {
            _manager = manager;
            _client = manager.Client;

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

                group.CreateCommand("add")
                    .Alias(Settings.RequestOps.ToArray())
                    .Description("Adds a video (either by search or URL) to the playlist")
                    .MinPermissions(Settings.RequestPerm)
                    .Parameter("p", ParameterType.Unparsed)
                    .Do(async e =>
                    {
                        if (Settings.SREnable == false)
                        {
                            await e.Channel.SendMessage("Sorry, song requesting has been disabled. Use !srToggle to enable");
                            return;
                        }
                        Video Result = GetVideoBySearch(e.GetArg("p"));
                        if (Result == null)
                        {
                            await e.Message.Channel.SendMessage("Sorry, we couldn't find a video for *" + e.GetArg("p") + "*");
                            return;
                        }
                        if (!Settings.SREnable)
                        {
                            await e.Message.Channel.SendMessage("Sorry, song requesting has been disabled");
                            return;
                        }
                        if (VidList.Contains(Result))
                        {
                            await e.Message.Channel.SendMessage("Video *" + Result.Snippet.Title + "* is already in the playlist");
                            return;
                        }
                        if (GetVidLength(Result) > Settings.MaxLength && Settings.MaxLength != 0)
                        {
                            await e.Message.Channel.SendMessage("Video *" + Result.Snippet.Title + "* is " + GetVidLength(Result) + " seconds long. This is longer than our max length of " + Settings.MaxLength);
                            Console.WriteLine("Video " + Result.Snippet.Title + " is " + GetVidLength(Result) + " long. This is longer than our max length of " + Settings.MaxLength);
                            return;
                        }
                        if (Result.Snippet.ChannelTitle.ToLower().Contains("vevo"))
                        {
                            //await LRC.SendMessage("Sorry, Vevo videos are currently nut supported by EasyMusicbot");
                            //return;
                        }

                        if (Settings.AudioMethod.Contains("Dl"))
                        {
                            Thread t = new Thread(() => { DownloadAudio(Result.Id); });
                            t.Start();
                        }
                        VidList.Add(Result);
                        Program.f.BoxHandler();
                        Console.WriteLine("Added video " + Result.Snippet.Title);
                        await e.Message.Delete();
                    });
                group.CreateCommand("skip")
                    .Alias(Settings.SkipOps.ToArray())
                    .Description("Skips either the current video or a specified video")
                    .Parameter("p", ParameterType.Multiple)
                    .MinPermissions(Settings.SkipPerm)
                    .Do(async e =>
                    {
                        if (e.Args.Any())
                        {
                            int index;
                            if (int.TryParse(e.GetArg("p"), out index))
                            {
                                if (index > VidList.Count - 1) return;
                                await e.Channel.SendMessage("Removed video in position [" + index + "], *" + VidList[index].Snippet.Title + "*");
                                VidList.RemoveAt(index);
                                Program.f.BoxHandler();
                                return;
                            }
                            MessageBox.Show("vid");
                            Video v = GetVideoBySearch(e.GetArg("p"));
                            MessageBox.Show(v.Snippet.Title);
                            foreach (Video v2 in VidList)
                            {
                                if (v.Snippet.Title.Equals(v2.Snippet.Title))
                                {
                                    await e.Channel.SendMessage("Removed video *" + v.Snippet.Title + "*");
                                    VidList.Remove(v2);
                                    Program.f.BoxHandler();
                                    return;
                                }
                            }
                            Program.f.BoxHandler();
                            return;
                        }
                        Skipping = true;
                        Program.f.axWindowsMediaPlayer1.Ctlcontrols.stop();
                        try { Settings.CurAM.Kill(); } catch { }
                        await Task.Delay(110);
                        Program.f.BoxHandler();
                        await e.Message.Delete();
                    });
                group.CreateCommand("pause")
                    .Description("Pauses the current song")
                    .MinPermissions(Settings.RequestPerm)
                    .Do(e =>
                    {
                        if (Settings.AudioMethod.Contains("VLC"))
                        {
                            SendKeys.SendWait("%(j)");
                        }
                        else
                        {
                            Program.f.axWindowsMediaPlayer1.Ctlcontrols.pause();
                        }
                    });
                group.CreateCommand("play")
                    .Description("Pauses the current song")
                    .MinPermissions(Settings.RequestPerm)
                    .Do(e =>
                    {
                        if (Settings.AudioMethod.Contains("VLC"))
                        {
                            SendKeys.SendWait("%(j)");
                        }
                        else
                        {
                            Program.f.axWindowsMediaPlayer1.Ctlcontrols.pause();
                        }
                    });
                group.CreateCommand("srtoggle")
                    .Description("Toggles songrequest on or off")
                    .MinPermissions(Settings.ConfigPerm)
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage("Songrequest toggled");
                        if (Settings.SREnable == false)
                        {
                            Settings.SREnable = true;
                        }
                        else
                        {
                            Settings.SREnable = false;
                        }
                        await e.Channel.SendMessage("Songrequest has been set to " + Settings.SREnable);
                    });
                group.CreateCommand("playlist")
                    .Description("Shows the current playlist")
                    .Do(async e =>
                    {
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
                    });
                group.CreateCommand("config")
                    .Description("Change the config. Ask Ian to do it for you if it seems not to be working")
                    .Parameter("setting", ParameterType.Required)
                    .Parameter("value", ParameterType.Unparsed)
                    .MinPermissions(Settings.ConfigPerm)
                    .Do(async e =>
                    {
                        Console.WriteLine("Config command recieved!");
                        try
                        {
                            if (e.GetArg("setting").Equals("maxlength"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.MaxLength = Convert.ToInt32(e.GetArg("value"));
                            }
                            if (e.GetArg("setting").Equals("modrole"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.ConfigPerm = Convert.ToInt32(e.GetArg("value"));
                            }
                            if (e.GetArg("setting").Equals("skiprole"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.SkipPerm = Convert.ToInt32(e.GetArg("value"));
                            }
                            if (e.GetArg("setting").Equals("requestrole"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.RequestPerm = Convert.ToInt32(e.GetArg("value"));
                            }
                            if (e.GetArg("setting").Equals("spchannel"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.SpChannel = Convert.ToBoolean(e.GetArg("value"));
                            }
                            if (e.GetArg("setting").Equals("channel"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.Channel = e.GetArg("value");
                            }
                            if (e.GetArg("setting").Equals("channel"))
                            {
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.Channel = e.GetArg("value");
                            }
                            if (e.GetArg("setting").Equals("audiomethod"))
                            {
                                ClearPlaylist();
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.AudioMethod = e.GetArg("value");
                            }
                            if (e.GetArg("setting").Equals("dlpath"))
                            {
                                ClearPlaylist();
                                await e.Channel.SendMessage("Config changed!");
                                Console.WriteLine("Thing changed!");
                                Settings.DLPath = e.GetArg("value");
                            }
                            Program.VerifyReady();
                        }
                        catch
                        {
                            await e.Channel.SendMessage("Hmm. That didn't seem to work please try again");
                        }
                    });
                group.CreateCommand("skipall")
                    .Alias(new string[1] { "clearplaylist" })
                    .Description("Clear the entire playlist")
                    .MinPermissions(Settings.SkipPerm)
                    .Do(async e =>
                    {
                        Skipping = true;
                        ClearPlaylist();
                        if (Settings.SpChannel && e.Channel.Name.Equals(Settings.Channel)) e.Channel.SendMessage("Playlist cleared");
                        if (Settings.SpChannel && !e.Channel.Name.Equals(Settings.Channel)) await e.Message.Delete();
                    });
                group.CreateCommand("debug")
                    .Do(async e =>
                    {
                        await e.Channel.SendMessage(e.User.ServerPermissions.ToString());
                    });
            });

            VidLoopAsync();
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
                return Settings.MaxLength + 1;
            }

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
                if (!Math.Round(args.ProgressPercentage * 0.15, 0, MidpointRounding.AwayFromZero).Equals(progress))
                {
                    progress = Math.Round(85 + args.ProgressPercentage * 0.15, 0, MidpointRounding.AwayFromZero);
                    Console.WriteLine(progress);
                }
            };

            if (DownloadExists(GetVideoBySearch(Id), true)) return;

            try
            {
                audioDownloader.Execute();
            }

            catch (System.Net.WebException e)
            {
                Program.LRC.SendMessage("Sorry, that video cannot be added");
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

        bool DownloadExists(Video v, bool done)
        {
            if (done) if (File.Exists(Settings.DLPath + "/" + v.Id + "!done.mp3")) return true;
            if (File.Exists(Settings.DLPath + "/" + v.Id + "!done.mp3")) return true;
            return false;
        }

        void ClearPlaylist()
        {
            Program.f.axWindowsMediaPlayer1.Ctlcontrols.stop();
            VidList.Clear();
            try { Settings.CurAM.Kill(); } catch { }
        }

        async Task VidLoopAsync()
        {
            while (true)
            {

                while (0 >= VidList.Count)
                {
                    await Task.Delay(1000);
                }

                if (Settings.AudioMethod.Equals("VLCDl")) await PlayDLVid(VidList[0]);

                else if (Settings.AudioMethod.Equals("VLCSt")) await PlaySTVid(VidList[0]);

                else if (Settings.AudioMethod.Equals("WMPDl")) await WMPPlay(VidList[0]);

                _client.SetGame(null);

                Program.f.BoxHandler();
                Console.WriteLine("Done!");
            }


        }

        async Task WMPPlay(Video v)
        {
            while (!DownloadExists(v, true))
            {
                _client.SetGame("Waiting for download");
                Console.WriteLine("Waiting for download");
                await Task.Delay(1000);
            }
            _client.SetGame(VidList[0].Snippet.Title);

            Program.f.axWindowsMediaPlayer1.URL = (v.Id.Remove(v.Id.Length - 4) + "!done.mp3");
            Program.f.axWindowsMediaPlayer1.Ctlcontrols.play();
            while (Program.f.axWindowsMediaPlayer1.playState != WMPLib.WMPPlayState.wmppsStopped)
            {
                await Task.Delay(100);
            }
            VidList.RemoveAt(0);
        }

        async Task PlayDLVid(Video v)
        {
            Console.WriteLine("Trying to play video");
            while (!DownloadExists(v, true))
            {
                _client.SetGame("Waiting for download");
                Console.WriteLine("Waiting for download");
                await Task.Delay(1000);
            }
            _client.SetGame(VidList[0].Snippet.Title);

            Console.WriteLine(v.Snippet.Title + " Playling download");

            Settings.CurAM = Process.Start(Settings.VLCPath + "/vlc.exe", "file:///" + Settings.DLPath + "/" + v.Id + "!done.mp3 --qt-start-minimized");

            while (!Settings.CurAM.HasExited)
            {
                await Task.Delay(100);
            }
            Console.WriteLine("VLC has exited");
            VidList.RemoveAt(0);

        }

        async Task PlaySTVid(Video v)
        {
            _client.SetGame(VidList[0].Snippet.Title);

            Settings.CurAM = Process.Start(Settings.VLCPath + "/vlc.exe", " --no-video http://www.youtube.com/watch?v=" + v.Id + " --qt-start-minimized");
            Skipping = false;
            watch.Restart();

            while (!Settings.CurAM.HasExited) await Task.Delay(100);
            if (watch.ElapsedMilliseconds < 10000 && !Skipping)
            {
                if (DownloadExists(v, true)) await Program.LRC.SendMessage("Video appears to encrypted, playing download");
                else
                {
                    await Program.LRC.SendMessage("Video appears to be encrypted, starting download");

                    //Thread t = new Thread(() => { DownloadAudio(v.Id); });
                    //t.Start();
                    _client.SetGame("Waiting for download");
                    DownloadAudio(v.Id);
                    await Program.LRC.SendMessage("Download finished");
                }
                await PlayDLVid(v);
                return;
            }
            Skipping = false;
            Console.WriteLine("VLC has exited");
            VidList.RemoveAt(0);
        }
    }
}