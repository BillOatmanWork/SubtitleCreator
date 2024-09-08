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

            string ffmpegPath = string.Empty;
            string inFile = string.Empty;
            string outputFile = Path.Combine(System.AppContext.BaseDirectory + "HdHomerun.m3u");
            string model = "medium";

            // Read in params
            bool paramsOK = true;
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-?" || arg.ToLower() == "-h" || arg.ToLower() == "-help")
                {
                    DisplayHelp();
                    return;
                }

                switch (arg.Substring(0, arg.IndexOf('=')).ToLower())
                {
                    case "-ffmpegPath":
                        ffmpegPath = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    case "-inFile":
                        inFile = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    case "-outFile":
                        outputFile = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    case "-model":
                        model = arg.Substring(arg.IndexOf('=') + 1).Trim().ToLower();
                        if (model != "small" && model != "medium" && model != "large")
                        {
                            paramsOK = false;
                            Console.WriteLine("Invalid model: " + model);
                        }
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

            Console.WriteLine("ffmpegPath: " + ffmpegPath);
            Console.WriteLine("Out: " + outputFile);
            Console.WriteLine("");


        }

        public void DisplayHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("SubtitleCreator is a command line utility to generate subtitles for a video file and put into a MKV container.");
            Console.WriteLine("Parameters: (Case Insensitive)");
            Console.WriteLine("");
            Console.WriteLine("Optional: -Model=<Language Model>  Options are Small/Medium/Large.  Bigger is bettwr quakity, but also slower. Default = Medium.");
            Console.WriteLine("");
            Console.WriteLine(""); Console.WriteLine("");

            string example = $"SubtitleCreator ";

            Console.WriteLine("");

            Console.WriteLine("Hit enter to continue");
            Console.ReadLine();
        }
    }
}

