using Extensions;
using System;
using System.IO;

namespace SubtitleCreator
{
    class SubtitleCreator
    {
        
        public string workingDir = AppContext.BaseDirectory;

        static void Main(string[] args)
        {
            // Get out of making everything static
            SubtitleCreator p = new SubtitleCreator();
            p.RealMain(args);
        }

        public void RealMain(string[] args)
        {
            Utilities.CleanLog();

            Utilities.ConsoleWithLog("SubtitleCreator version " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            Utilities.ConsoleWithLog("");

            string ffmpegPath = string.Empty;
            string inFile = string.Empty;            
            string model = "medium";
            bool translate = false;
            string language = "en";
            string audioLanguage = "eng";
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

                    case "-audiolanguage":
                        audioLanguage = arg.Substring(arg.IndexOf('=') + 1).Trim().ToLower();
                        if (audioLanguage != "eng" && audioLanguage != "fra" && audioLanguage != "spa")
                        {
                            paramsOK = false;
                            Utilities.ConsoleWithLog($"Invalid audio language: {audioLanguage}. Possible values are eng = English, fra = French, spa = spanish. English is the default.");
                        }
                        break;

                    case "-model":
                        model = arg.Substring(arg.IndexOf('=') + 1).Trim().ToLower();
                        if (model != "small" && model != "medium" && model != "large")
                        {
                            paramsOK = false;
                            Utilities.ConsoleWithLog("Invalid model: " + model);
                        }
                        break;

                    default:
                        paramsOK = false;
                        Utilities.ConsoleWithLog("Unknown parameter: " + arg);
                        break;
                }
            }

            if (paramsOK == false)
                return;

            if (translate && string.IsNullOrEmpty(language) && language != "en")
            {
                Utilities.ConsoleWithLog("Language cannot be specified when translating to English. Setting to 'en'.");
                language = "en";
            }

            if (merge == true)
                outputFile = $"{inFile.FullFileNameWithoutExtention()}_subs.mkv";

            string fNameLang = string.IsNullOrEmpty(language) ? "" : language;
            string srtFile = $"{inFile.FullFileNameWithoutExtention()}.{fNameLang}.srt";

            Utilities.ConsoleWithLog($"ffmpeg Path: {ffmpegPath}");
            Utilities.ConsoleWithLog($"Input File: {inFile}");
            Utilities.ConsoleWithLog($"Audio Language: {audioLanguage}");
            Utilities.ConsoleWithLog($"Translate to English: {translate}");

            if (merge == true)
                Utilities.ConsoleWithLog($"Output File: {outputFile}");
            else
                Utilities.ConsoleWithLog($"SRT File: {srtFile}");

            Utilities.ConsoleWithLog("");

            Utilities.ConsoleWithLog("Extracting audio from the video file ... ");
            string audioFilePath = AudioExtractor.ExtractAudioFromVideoFile(inFile);
            if (string.IsNullOrEmpty(audioFilePath))
            {
                Utilities.ConsoleWithLog("Audio extraction failed. Exiting.");
                return;
            }
            Utilities.ConsoleWithLog("Audio extraction complete.");

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

            Utilities.ConsoleWithLog("Creating subtitles file. Please be patient ... ");
            bool subsCreated = removeCommercials.DoWorkGenerateSubtitles(audioFilePath, modelType, workingDir, srtFile, language, translate, audioLanguage);
            Utilities.ConsoleWithLog("Subtitle creation complete.");

            if(merge == true)
            {
                Utilities.ConsoleWithLog("Merge process started.");
                removeCommercials.DoWorkMergeSubtitles(srtFile, inFile, outputFile, ffmpegPath, audioLanguage);
                Utilities.ConsoleWithLog($"Merge process completed. Merged file {outputFile} created.");
                File.Delete(srtFile);
            }
            else
            {
                Utilities.ConsoleWithLog($"Merge process bypassed. Subtitle file {srtFile} created.");
            }

            File.Delete(audioFilePath);
        }

        /// <summary>
        /// Display help text
        /// </summary>
        public void DisplayHelp()
        {
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("SubtitleCreator is a command line utility to generate subtitles for a video file and optionally put into a MKV container.");
            Utilities.ConsoleWithLog("Parameters: (Case Insensitive)");
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("-ffmpegpPath=Path to the ffmpeg executable.  Just the folder, the exe is assumed to be ffmpeg.exe.");
            Utilities.ConsoleWithLog("-inFile=The video file the subtitles will be generated for.");
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("Optional: -nomerge  By default once the subtitle file is created, it is merged into a MKV container along with the video file. If this is used, the MKV container will not be created and the subtitle file will not be deleted. ");
            Utilities.ConsoleWithLog("Optional: -translate  If this is used, subtitles will be translated to English.  Do not use if the audio is already in English.");
            Utilities.ConsoleWithLog("Optional: -audioLanguage=<language>  The Whisper audio language detection feature has problems now.  So this should be specified if the audio is not in english. Possible values are eng = English, fra = French, spa = spanish. English is the default.");
            Utilities.ConsoleWithLog("Optional: -language=The language of the audio and therefore the subtitles. en for example is english. This is used for the naming of the subtitles file. Default is none.");
            Utilities.ConsoleWithLog("Optional: -Model=<Language Model>  Options are Small/Medium/Large.  Bigger is better quality, but also slower. Default = Medium.");
            Utilities.ConsoleWithLog("");

            string example = $"SubtitleCreator -ffmpegPath=\"Path\to\ffmpeg folder\" -inFile=\"c:\\My Movies\\My Little Pony.ts\" -Model=Large";

            Utilities.ConsoleWithLog("");

            Utilities.ConsoleWithLog("Hit enter to continue");
            Console.ReadLine();
        }
    }
}

