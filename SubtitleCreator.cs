using Extensions;
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
            string model = "medium";
            bool translate = false;
            string language = string.Empty;
            bool merge = true;
            string outputFile = string.Empty;

            bool paramsOK = true;
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-?" || arg.ToLower() == "-h" || arg.ToLower() == "-help")
                {
                    DisplayHelp();
                    return;
                }

                if (arg.ToLower() == "-translate")
                {
                    translate = true;
                    continue;
                }

                if (arg.ToLower() == "-nomerge")
                {
                    merge = false;
                    continue;
                }

                switch (arg.Substring(0, arg.IndexOf('=')).ToLower())
                {
                    case "-ffmpegpath":
                        ffmpegPath = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    case "-infile":
                        inFile = arg.Substring(arg.IndexOf('=') + 1).Trim();
                        break;

                    case "-language":
                        language = arg.Substring(arg.IndexOf('=') + 1).Trim();
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

            if (translate && string.IsNullOrEmpty(language) && language != "en")
            {
                Console.WriteLine("Language cannot be specified when translating to English. Setting to 'en'.");
                language = "en";
            }

            if (merge == true)
                outputFile = Path.Combine(inFile.FullFileNameWithoutExtention(), "_subs", ".mkv");

            string fNameLang = string.IsNullOrEmpty(language) ? "" : language;
            string srtFile = $"{inFile.FullFileNameWithoutExtention()}_{fNameLang}.srt";

            Console.WriteLine("ffmpegPath: " + ffmpegPath);
            Console.WriteLine("In: " + inFile);

            if(merge == true)
                Console.WriteLine("Out: " + outputFile);
            else
                Console.WriteLine("SRT: " + srtFile);

            Console.WriteLine("");

            Console.Write("Extracting audio from the video file ... ");
            string audioFilePath = AudioExtractor.ExtractAudioFromVideoFile(inFile);
            Console.WriteLine("Audio extraction complete.");


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

