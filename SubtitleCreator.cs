using Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubtitleCreator
{
    class SubtitleCreator
    {
        public string appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\SubtitleCreator\";

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

            if (!Directory.Exists(appDataDir))
                Directory.CreateDirectory(appDataDir);

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

            RemoveCommercials removeCommercials = new RemoveCommercials();

            ModelType modelType = ModelType.Medium;
            switch (model.ToLower())
            {
                case "small":
                    modelType = ModelType.Small;
                    break;
                case "medium":
                    modelType = ModelType.Medium;
                    break;
                case "large":
                    modelType = ModelType.Large;
                    break;
            }   

            Console.Write("Creatng subtitles file. Please be patient ... ");
            bool subsCreated = removeCommercials.DoWorkGenerateSubtitles(audioFilePath,  modelType, appDataDir, srtFile, language, translate);
            Console.WriteLine("Subtitle creation complete.");

            if(merge == true)
            {

                Console.WriteLine($"Merge process completed. Merged file {outputFile} created.");
                File.Delete(srtFile);
            }
            else
            {
                Console.WriteLine($"Merge process bypassed. Subtitle file {srtFile} created.");
            }

            File.Delete(audioFilePath);

        }

        public void DisplayHelp()
        {
            Console.WriteLine("");
            Console.WriteLine("SubtitleCreator is a command line utility to generate subtitles for a video file and put into a MKV container.");
            Console.WriteLine("Parameters: (Case Insensitive)");
            Console.WriteLine("");
            Console.WriteLine("-ffmpegpPath=Path to the ffmpeg executable.  Just the folder, the exe is assumed to be ffmpeg.exe.");
            Console.WriteLine("-inFile=The video file the subtitles will be generated for.");
            Console.WriteLine("");
            Console.WriteLine("Optional: -nomerge  By default once the subtitle file is created, it is merged into a MKV container along with the video file. If this is used, the MKV container will not be created and the subtitle file will not be deleted. ");
            Console.WriteLine("Optional: -translate  If this is used, subtitles will be translated to English.  Do not use if the audio is already in English.");
            Console.WriteLine("Optional: -language=The language of the audio and therefore the subtitles. en for example is english. Default is none.");
            Console.WriteLine("Optional: -Model=<Language Model>  Options are Small/Medium/Large.  Bigger is better quality, but also slower. Default = Medium.");
            Console.WriteLine("");

            string example = $"SubtitleCreator -ffmpegPath=\"Path\to\ffmpeg folder\" -inFile=\"c:\\My Movies\\My Little Pony.ts\" -Model=Large";

            Console.WriteLine("");

            Console.WriteLine("Hit enter to continue");
            Console.ReadLine();
        }
    }
}

