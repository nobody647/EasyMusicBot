using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EasyMusicBot
{
    public class Settings
    {

        public static String email;
        public static String password;
        public static bool SREnable;
        public static int MaxLength;
        public static int ConfigPerm;
        public static int RequestPerm;
        public static int SkipPerm;
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
                //line = line.Replace(" ", "");

                while (line != null)
                {
                    if (line.Contains("=") && !line.StartsWith("#"))
                    {
                        switch (line.Split('=')[0].Trim(' '))
                        {
                            case "email":
                                email = line.Split('=')[1].Trim(' ');
                                break;
                            case "password":
                                password = line.Split('=')[1].Trim(' ');
                                break;
                            case "srDefault":
                                SREnable = Convert.ToBoolean(line.Split('=')[1].Trim(' '));
                                break;
                            case "requestOps":
                                String[] opsAdd = line.Split('=')[1].Trim(' ').Split(',');
                                foreach (string s in opsAdd)
                                {
                                    RequestOps.Add(s.Trim(' '));
                                }
                                break;
                            case "skipOps":
                                String[] opsAdd2 = line.Split('=')[1].Trim(' ').Split(',');
                                foreach (string s in opsAdd2)
                                {
                                    SkipOps.Add(s.Trim(' '));
                                }
                                break;
                            case "maxLength":
                                MaxLength = Convert.ToInt32(line.Split('=')[1].Trim(' '));
                                break;
                            case "config":
                                ConfigPerm = Convert.ToInt32(line.Split('=')[1].Trim(' '));
                                break;
                            case "request":
                                RequestPerm = Convert.ToInt32(line.Split('=')[1].Trim(' '));
                                break;
                            case "skip":
                                SkipPerm = Convert.ToInt32(line.Split('=')[1].Trim(' '));
                                break;
                            case "channel":
                                Channel = line.Split('=')[1].Trim(' ');
                                break;
                            case "audioMethod":
                                AudioMethod = line.Split('=')[1].Trim(' ');
                                break;
                            case "VLCPath":
                                VLCPath = line.Split('=')[1].Trim(' ').Replace('\\', '/');
                                break;
                            case "DLPath":
                                DLPath = line.Split('=')[1].Trim(' ').Replace('\\', '/');
                                break;
                            default:
                                break;
                        }
                    }
                    line = sr.ReadLine();

                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Oh noes! There was a problem reading the config.txt file!");
                Console.WriteLine("If you happen across the devs, make sure to give them this: " + e);
            }


        }
    }

}