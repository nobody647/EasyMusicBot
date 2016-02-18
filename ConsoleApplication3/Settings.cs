using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EasyMusicBot
{
    public class Settings
    {

        public static String email;
        public static String password;
        public static bool SREnable;
        public static int MaxLength;
        public static String ModRole;
        public static String RequestRole;
        public static String SkipRole;
        public static bool SpChannel;
        public static String Channel;
        public static String AudioMethod;
        public static String VLCPath;
        public static String DLPath;
        public static Process CurAM;
        public static List<String> RequestOps = new List<String>(); //{"!songrequest", "!request", "!play", "!sr", "!add"};
        public static List<String> SkipOps = new List<String>(); //{"!skip", "!skipsong", "!songskip", "this song sucks", "!next"};

        public static void ReadConfig()
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
    }
    public class GlobalSettings
    {
        private const string path = "./config/global.json";
        private static GlobalSettings _instance = new GlobalSettings();

        public static void Load()
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"{path} is missing.");
            //_instance = JsonConvert.DeserializeObject<GlobalSettings>(File.ReadAllText(path));

        }
        public static void Save()
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonConvert.SerializeObject(_instance, Formatting.Indented));
        }

        //Discord
        public class DiscordSettings
        {
            [JsonProperty("username")]
            public string Email;
            [JsonProperty("password")]
            public string Password;
        }
        [JsonProperty("discord")]
        private DiscordSettings _discord = new DiscordSettings();
        public static DiscordSettings Discord => _instance._discord;

        //Users
        public class UserSettings
        {
            [JsonProperty("dev")]
            public ulong DevId;
        }
        [JsonProperty("users")]
        private UserSettings _users = new UserSettings();
        public static UserSettings Users => _instance._users;

        //Github
        public class GithubSettings
        {
            [JsonProperty("username")]
            public string Username;
            [JsonProperty("password")]
            public string Password;
            [JsonIgnore]
            public string Token => Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password));
        }
        [JsonProperty("github")]
        private GithubSettings _github = new GithubSettings();
        public static GithubSettings Github => _instance._github;
    }

}