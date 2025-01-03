using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Extensions;

namespace SubtitleCreator
{
    sealed class SubtitleCreator
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
            string model = "large";
            bool translate = false;
            string language = "en";
            string audioLanguage = "eng";
            bool merge = true;
            bool useSDH = true;
            string outputFile = string.Empty;
            bool attemptToRepair = true;
            bool forceModelUpdate = false;

            bool paramsOK = true;
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-?" || arg.ToLower() == "-h" || arg.ToLower() == "-help")
                {
                    DisplayHelp();
                    return;
                }

                if (arg.ToLower() == "-languagelist")
                {
                    List<string> langList = WhisperLanguageMapper.ListAllLanguages();
                    Utilities.ConsoleWithLog("Possible audio languages are:");
                    foreach (string lang in langList)
                        Utilities.ConsoleWithLog(lang);

                    return;
                }

                if (arg.ToLower() == "-forcemodelupdate")
                {
                    forceModelUpdate = true;
                    continue;
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

                if (arg.ToLower() == "-nosdh")
                {
                    useSDH = false;
                    continue;
                }

                if (arg.ToLower() == "-norepair")
                {
                    attemptToRepair = false;
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
                        if (string.IsNullOrEmpty(WhisperLanguageMapper.GetWhisperCode(audioLanguage)))
                        {
                            paramsOK = false;
                            Utilities.ConsoleWithLog($"Invalid audio language: {audioLanguage}. For a list of possible languages, run SubtitleCreator -LanguageList. English is the default.");
                        }
                        break;

                    case "-model":
                        model = arg.Substring(arg.IndexOf('=') + 1).Trim().ToLower();
                        if (model != "small" && model != "medium" && model != "large")
                        {
                            paramsOK = false;
                            Utilities.ConsoleWithLog($"Invalid model: {model}. Possible models are small, medium, and large.");
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

            string audioLanguageLong = WhisperLanguageMapper.GetLanguageFullName(audioLanguage);

            Utilities.ConsoleWithLog($"ffmpeg Path: {ffmpegPath}");
            Utilities.ConsoleWithLog($"Input File: {inFile}");
            Utilities.ConsoleWithLog($"Audio Language: {audioLanguageLong}");
            Utilities.ConsoleWithLog($"Translate to English: {translate}");
            Utilities.ConsoleWithLog($"Attempt to Repair: {attemptToRepair}");
            Utilities.ConsoleWithLog($"Create SDH Subtitles: {useSDH}");
            Utilities.ConsoleWithLog($"Force Whisper Model Update: {forceModelUpdate}");

            if (string.IsNullOrEmpty(inFile))
            {
                Utilities.ConsoleWithLog($"Parameter inFile must be set.  Exiting.");
                return;
            }

            if (File.Exists(inFile) == false)
            {
                Utilities.ConsoleWithLog($"Input file {inFile} does not exist. Exiting.");
                return;
            }

            if(string.IsNullOrEmpty(ffmpegPath))
            {
                Utilities.ConsoleWithLog("Parameter ffmpegPath must be set.  Exiting.");
                return;
            }

            if (!File.Exists(Path.Combine(ffmpegPath, "ffmpeg.exe")) && !File.Exists(Path.Combine(ffmpegPath, "ffmpeg")))
            {
                Utilities.ConsoleWithLog($"ffmpeg executable not found in the selected folder {ffmpegPath}.  Exiting.");
                return;
            }

            if (merge == true)
                Utilities.ConsoleWithLog($"Output File: {outputFile}");
            else
                Utilities.ConsoleWithLog($"SRT File: {srtFile}");

            Utilities.ConsoleWithLog("");

            string videoLength = GetVideoDuration(ffmpegPath, inFile, out int durationSeconds);
            Utilities.ConsoleWithLog($"Length of the video is {videoLength}.");

            Utilities.ConsoleWithLog("");

            Utilities.ConsoleWithLog("Extracting audio from the video file ... ");
            Watch.WatchStart();
            string audioFilePath = AudioExtractor.ExtractAudioFromVideoFile(inFile, attemptToRepair, ffmpegPath);
            if (string.IsNullOrEmpty(audioFilePath))
            {
                Utilities.ConsoleWithLog("Audio extraction failed. Exiting.");
                return;
            }
            Utilities.ConsoleWithLog($"Audio extraction complete in {Watch.WatchStop()}.");

            CreateTheSubtitles removeCommercials = new CreateTheSubtitles();

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
            Watch.WatchStart();
            _ = removeCommercials.DoWorkGenerateSubtitles(audioFilePath, modelType, forceModelUpdate, workingDir, srtFile, language, translate, useSDH, audioLanguage, durationSeconds);
            Utilities.ConsoleWithLog($"Subtitle creation complete in {Watch.WatchStop()}.");

            if (merge == true)
            {
                Utilities.ConsoleWithLog("Merge process started.");
                Watch.WatchStart();
                removeCommercials.DoWorkMergeSubtitles(srtFile, inFile, outputFile, ffmpegPath, audioLanguage);
                Utilities.ConsoleWithLog($"Merge process completed in {Watch.WatchStop()}. Merged file {outputFile} created.");
                File.Delete(srtFile);
            }
            else
            {
                Utilities.ConsoleWithLog($"Merge process bypassed. Subtitle file {srtFile} created.");
            }

            File.Delete(audioFilePath);
        }

        public static string GetVideoDuration(string ffmpegPath, string videoFilePath, out int durationSeconds)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(ffmpegPath, "ffprobe"),
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoFilePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (double.TryParse(output, out double duration))
            {
                TimeSpan time = TimeSpan.FromSeconds(duration);
                durationSeconds = (int)time.TotalSeconds;
                return $"{time.Hours} hours {time.Minutes} minutes";
            }
            else
            {
                durationSeconds = 0;
                return "00:00";
            }
        }

        /// <summary>
        /// Display help text
        /// </summary>
        public static void DisplayHelp()
        {
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("SubtitleCreator is a command line utility to generate subtitles for a video file and optionally put into a MKV container.");
            Utilities.ConsoleWithLog("Parameters: (Case Insensitive)");
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("-ffmpegpPath=Path to the ffmpeg executable.  Just the folder, the exe is assumed to be ffmpeg.exe.");
            Utilities.ConsoleWithLog("-inFile=The video file the subtitles will be generated for.");
            Utilities.ConsoleWithLog("");
            Utilities.ConsoleWithLog("Optional: -noMerge  By default once the subtitle file is created, it is merged into a MKV container along with the video file. If this is used, the MKV container will not be created and the subtitle file will not be deleted.");
            Utilities.ConsoleWithLog("Optional: -translate  If this is used, subtitles will be translated to English.  Do not use if the audio is already in English.");
            Utilities.ConsoleWithLog("Optional: -audioLanguage=<language>  The Whisper audio language detection feature has problems now.  So this should be specified if the audio is not in english. English is the default.");
            Utilities.ConsoleWithLog("Optional: -language=The language of the audio and therefore the subtitles. en for example is english. This is used for the naming of the subtitles file. Default is en.");
            Utilities.ConsoleWithLog("Optional: -Model=<Language Model>  Options are Small/Medium/Large.  Bigger is better quality, but can be slower. Default = Large.");
            Utilities.ConsoleWithLog("Optional: -noRepair  Sometimes a recording will have audio errors that stop the processing.  By default, the app will attempt to make repairs.  Use of this flag aborts the repair and the app just fails.");
            Utilities.ConsoleWithLog("Optional: -noSDH  Do not generate descriptive lines such as [grunting].  By default, the descriptive (SDH) subtitles will be included.");
            Utilities.ConsoleWithLog("Optional: -forceModelUpdate  Force the update of the Whisper model. If set, the model will be downloaded even if it already exists. Default: false.");
            Utilities.ConsoleWithLog("");

            string example = $"SubtitleCreator -ffmpegPath=\"Path\to\ffmpeg folder\" -inFile=\"c:\\My Movies\\My Little Pony.ts\" -Model=Large";
            Utilities.ConsoleWithLog(example);
            Utilities.ConsoleWithLog("");

            Utilities.ConsoleWithLog("For a list of possible audioLanguages, run SubtitleCreator -LanguageList.");
            Utilities.ConsoleWithLog("");

            Utilities.ConsoleWithLog("Hit enter to continue");
            Console.ReadLine();
        }
    }
}

