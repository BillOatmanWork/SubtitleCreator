using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubtitleCreator
{
    class SubtitleCreator
    {
        static void Main(string[] args)
        {
            // Get out of making everything static
            SubtitleCreator p = new SubtitleCreator();
            p.RealMain(args);
        }

        public void RealMain(string[] args)
        {
            Console.WriteLine("SubtitleCreator version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("");

            string txtOutputFile = Path.Combine(System.AppContext.BaseDirectory + "HdHomerun.m3u");
            string selectedDevice = string.Empty;

            // Read in params
            bool paramsOK = true;
            foreach (string arg in args)
            {
                if(arg.ToLower() == "-all")
                {
                    continue;
                }

                if (arg.ToLower() == "-?" || arg.ToLower() == "-h" || arg.ToLower() == "-help")
                {
                    DisplayHelp();
                    return;
                }

                switch (arg.Substring(0, arg.IndexOf('=')).ToLower())
                {
                    case "-deviceid":
                        selectedDevice = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        Console.WriteLine("DeviceID: " + selectedDevice);
                        break;

                    case "-out":
                        txtOutputFile = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    default:
                        paramsOK = false;
                        Console.WriteLine("Unknown parameter: " + arg);
                        break;
                }
            }

            if (paramsOK == false)
            {
                return;
            }

            Console.WriteLine("Out: " + txtOutputFile);
            Console.WriteLine("");


        }

        public void DisplayHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("SubtitleCreator is a command line utility to generate subtitles for a video file and put into a MKV container.");
            Console.WriteLine("Parameters: (Case Insensitive)");
            Console.WriteLine("");
            Console.WriteLine("-All[Optional] Put all of the HDHomerun channels in the M3U file.  Default: Only use the favorite (starred) channels.");
            Console.WriteLine("DeviceID=[Optional] The device ID of your hdmomerun device. Not needed if you only have one device on your network.");
            Console.WriteLine("Out=[Optional] The fully qualified path where the m3u file that will be created. Default: OTA.m3u in the SubtitleCreator directory.");
            Console.WriteLine("");

            string example = $"SubtitleCreator ";

            Console.WriteLine("");

            Console.WriteLine("Hit enter to continue");
            Console.ReadLine();
        }
    }
}

