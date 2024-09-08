using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Extensions;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitleCreator
{
    public enum ModelType { Tiny, Base, Small, Medium, Large }

    public class RemoveCommercials
    {
        private string videoFilePath;
        private List<EdlSequence> commercialList = new List<EdlSequence>();
        private List<EdlSequence> commercialListInitial;
        private string ffmpegPath;
        private int leadSeconds;
        private int endSeconds;
        private double bitRate = 0.0;
        private static string fileNameIdentifier = "!ComRemover!";

        public RemoveCommercials(string _videoFilePath, string _ffmpegPath, List<EdlSequence> _commercialListInitial, int _leadSeconds, int _endSeconds)
        {
            videoFilePath = _videoFilePath;
            ffmpegPath = _ffmpegPath;
            commercialListInitial = _commercialListInitial;
            leadSeconds = _leadSeconds;
            endSeconds = _endSeconds;
        }

        public bool DoWorkSetup(out string status, out int commercialCount)
        {
            double videoLength = GetVideoDurationAndBitrate(videoFilePath, ffmpegPath, out double _bitRate);
            bitRate = Math.Round(_bitRate);

            AdjustCommercialList(videoLength);
            commercialCount = commercialList.Count;

            status = ($"Video length is {videoLength} seconds. Number of commercials is {commercialCount}.");

            return true;
        }

        /// <summary>
        /// Remove any commercials that are outside the lead and end seconds.
        /// </summary>
        /// <param name="videoLength"></param>
        private void AdjustCommercialList(double videoLength)
        {
            foreach (EdlSequence edl in commercialListInitial)
            {
                edl.endSec = edl.endSec + 1;

                if (edl.startSec > leadSeconds && edl.endSec < (videoLength - endSeconds))
                    commercialList.Add(edl);
            }
        }

        public bool DoWorkSplit(out string status)
        {
            string finalName = $"{videoFilePath.FullFileNameWithoutExtention()}_processed.mp4";
            string segmentNameTemplate = $"{videoFilePath.FullFileNameWithoutExtention()}_segment_$$$$$_{fileNameIdentifier}.mp4";

            double videoLength = GetVideoDuration(videoFilePath, ffmpegPath);

            for (int i = 0; i < commercialList.Count; i++)
            {
                double startTime = 0;
                if (i == 0)
                    startTime = 0;
                else
                    startTime = commercialList[i - 1].endSec;

                double endTime = 0;

                if (i == commercialList.Count - 1)
                    endTime = videoLength;
                else
                    endTime = commercialList[i].startSec;

                string segmentNameTemp = segmentNameTemplate.Replace("$$$$$", i.ToString() + "_$$$$$");

                // Extract segment (removing any closed captions
                string ffmpegArgs = $"-i \"{videoFilePath}\" -ss {startTime} -to {endTime} -bsf:v \"filter_units=remove_types=6\" -c copy \"{segmentNameTemp}\"";

                // Set up the process to run FFmpeg
                Process ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
                ffmpeg.StartInfo.Arguments = ffmpegArgs;
                ffmpeg.StartInfo.RedirectStandardError = true;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.CreateNoWindow = true;

                // Start the process
                ffmpeg.Start();

                // Read the output
                string output = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
            }

            status = "Split Processing Complete.";

            return true;
        }

        //public bool DoWorkFadeIn(out string status)
        //{
        //    string segmentNameTemplate = $"{videoFilePath.FullFileNameWithoutExtention()}_segment_$$$$$_{fileNameIdentifier}.mp4";

        //    for (int i = 0; i < commercialList.Count; i++)
        //    {
        //        string segmentNameTemp = segmentNameTemplate.Replace("$$$$$", i.ToString() + "_$$$$$");
        //        string segmentName = $"{segmentNameTemplate.Replace("$$$$$", i.ToString() + "_fadein_")}";

        //        string ffmpegArgs = $"-i \"{segmentNameTemp}\" -vf \"fade=t=in:st=0:d=2\" -b:v {bitRate}k -c:a copy \"{segmentName}\"";

        //        // Set up the process to run FFmpeg
        //        Process ffmpeg = new Process();
        //        ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
        //        ffmpeg.StartInfo.Arguments = ffmpegArgs;
        //        ffmpeg.StartInfo.RedirectStandardError = true;
        //        ffmpeg.StartInfo.UseShellExecute = false;
        //        ffmpeg.StartInfo.CreateNoWindow = true;

        //        // Start the process
        //        ffmpeg.Start();

        //        // Read the output
        //        string output = ffmpeg.StandardError.ReadToEnd();
        //        ffmpeg.WaitForExit();
        //    }

        //    status = "Fade In Processing Complete.";

        //    return true;
        //}

        public bool DoWorkFadeOut(out string status)
        {
            string segmentNameTemplate = $"{videoFilePath.FullFileNameWithoutExtention()}_segment_$$$$$_{fileNameIdentifier}.mp4";

            for (int i = 0; i < commercialList.Count; i++)
            {
                string segmentNameTemp = segmentNameTemplate.Replace("$$$$$", i.ToString() + "_$$$$$");
                string segmentName = segmentNameTemp.Replace("$$$$$", "fadeout");

                double fadepoint = Math.Round(GetVideoDuration(segmentNameTemp, ffmpegPath) - 1);

                string ffmpegArgs = $"-i \"{segmentNameTemp}\" -vf \"fade=t=out:st={fadepoint}:d=2\" -b:v {bitRate}k -c:a copy \"{segmentName}\"";

                // Set up the process to run FFmpeg
                Process ffmpeg = new Process();
                ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
                ffmpeg.StartInfo.Arguments = ffmpegArgs;
                ffmpeg.StartInfo.RedirectStandardError = true;
                ffmpeg.StartInfo.UseShellExecute = false;
                ffmpeg.StartInfo.CreateNoWindow = true;

                // Start the process
                ffmpeg.Start();

                // Read the output
                string output = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();
            }

            status = "Fade Out Processing Complete.";

            return true;
        }

        public bool DoWorkConcat(out string status)
        {          
            string segmentNameTemplate = $"{videoFilePath.FullFileNameWithoutExtention()}_segment_$$$$$.mp4";
            string fileList = $"{videoFilePath.FullFileNameWithoutExtention()}_filelist_{fileNameIdentifier}.txt";
            string concatFile = $"{videoFilePath.FullFileNameWithoutExtention()}_concat_{fileNameIdentifier}.mp4";
            string ffmpegArgs = $"-f concat -safe 0 -i \"{fileList}\" -c copy \"{concatFile}\"";

            using (StreamWriter sw = File.CreateText(fileList))
            {
                for (int i = 0; i < commercialList.Count; i++)
                {
                    string segmentName = $"file '{segmentNameTemplate.Replace("$$$$$", i.ToString() + $"_fadeout_{fileNameIdentifier}").Replace("\\", "/")}'";
                    sw.WriteLine(segmentName);
                }
            }

            // Set up the process to run FFmpeg
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
            ffmpeg.StartInfo.Arguments = ffmpegArgs;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            // Start the process
            ffmpeg.Start();

            // Read the output
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            status = "Concatenation Complete.";

            return true;
        }

        public bool DoWorkTrim(out string status)
        {
           // string segmentNameTemplate = $"{videoFilePath.FullFileNameWithoutExtention()}_segment_$$$$$_{fileNameIdentifier}.mp4";
            string concatFile = $"{videoFilePath.FullFileNameWithoutExtention()}_concat_{fileNameIdentifier}.mp4";
            string finalFile = $"{videoFilePath.FullFileNameWithoutExtention()}_final.mp4"; 

            int duration = (int)GetVideoDuration(concatFile, ffmpegPath);
            int start = leadSeconds;
            int end = duration - endSeconds - leadSeconds;

            string ffmpegArgs = $"-i \"{concatFile}\" -ss {start} -t {end} -c copy \"{finalFile}\"";

            // Set up the process to run FFmpeg
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
            ffmpeg.StartInfo.Arguments = ffmpegArgs;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            // Start the process
            ffmpeg.Start();

            // Read the output
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            status = $"Trimming Complete. Final file is {finalFile}";

            return true;
        }
        public string DoWorkExtractAudio(out string status)
        {
            string finalFile = $"{videoFilePath.FullFileNameWithoutExtention()}_final.mp4";
            string outputFilePath = AudioExtractor.ExtractAudioFromVideoFile(finalFile);

            status = "Audio Extraction Complete.";

            return outputFilePath;
        }

        public string DoWorkGenerateSubtitles(out string status, string wavFilePath, ModelType modelType, string appDataDir)
        {
            List<SegmentData> segments = new();
            string srtFile = $"{wavFilePath.FullFileNameWithoutExtention()}.srt";

            string languageCode = "en";
            bool shouldTranslate = false;

            GgmlType ggmlType = GgmlType.Base;

            switch (modelType)
            {
                case ModelType.Tiny:
                    ggmlType = GgmlType.Tiny;
                    break;
                case ModelType.Base:
                    ggmlType = GgmlType.Base;
                    break;
                case ModelType.Small:
                    ggmlType = GgmlType.Small;
                    break;
                case ModelType.Medium:
                    ggmlType = GgmlType.Medium;
                    break;
                case ModelType.Large:
                    ggmlType = GgmlType.LargeV3;
                    break;
            }

            string modelName = Utilities.GgmlTypeToString(ggmlType);
            string modelPath = Path.Combine(appDataDir, modelName);

            segments.Clear();
            if (!File.Exists(modelPath))
            {
                //Console.WriteLine($"Downloading Whisper AI model {modelName}. This might take a while depending on your internet speed..");
                //Console.WriteLine("The application might exit after downloading. Please restart the application manually in that case!");
                // var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(modelType);
                var modelStream = WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType).GetAwaiter().GetResult();
                var fileWriter = File.OpenWrite(modelPath);
                modelStream.CopyTo(fileWriter);
                fileWriter.Close();
                //Console.WriteLine("Downloaded model");
            }

            WhisperProcessor? processor = SetupProcessor(modelPath, languageCode, shouldTranslate, OnNewSegment);

            if (processor is null)
            {
                status = "Something went wrong while setting up the processor.";
                return "";
            }

            void OnNewSegment(SegmentData segmentData)
            {
                var startTime = Utilities.ConvertTimestampToSrtFormat(segmentData.Start);
                var endTime = Utilities.ConvertTimestampToSrtFormat(segmentData.End);
                Console.WriteLine($"CSSS {startTime} ==> {endTime} : {segmentData.Text}");
                segments.Add(segmentData);
            }

            using (Stream fileStream = File.OpenRead(wavFilePath))
            {
                GC.KeepAlive(fileStream);
                GC.KeepAlive(processor);

                processor.Process(fileStream);
                processor.Dispose();
            }

            // Sort segments by start time
            segments.Sort((x, y) => x.Start.CompareTo(y.Start));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            // Remove all consecutive segments that have the same Text content.  For some reason it happens with some models.
         //   segments = RemoveConsecutiveDuplicates(segments);
         segments = RemoveConsecutiveDuplicatesKeepingLast(segments);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            var outputLanguagecode = languageCode.Length == 0 || shouldTranslate ? "en" : languageCode;

            using (StreamWriter writer = new StreamWriter(srtFile))
            {
                var subtitleIndex = 0;
                foreach (var segment in segments)
                {
                    var startTime = Utilities.ConvertTimestampToSrtFormat(segment.Start);
                    var endTime = Utilities.ConvertTimestampToSrtFormat(segment.End);
                    if (startTime == endTime || segment.Text.Trim().Length == 0)
                        continue;

                    writer.WriteLine(++subtitleIndex);
                    writer.WriteLine($"{startTime} --> {endTime}");

                    // Break the text in max 42 characters, but split only on white space
                    var parts = new List<string>();
                    var index = 0;
                    while (index < segment.Text.Length)
                    {
                        if (segment.Text.Length - index <= 42)
                        {
                            parts.Add(segment.Text[index..]);
                            break;
                        }

                        var lastSpace = segment.Text.Substring(index, 42).LastIndexOf(' ');
                        parts.Add(segment.Text.Substring(index, lastSpace));
                        index += lastSpace + 1;
                    }

                    foreach (string part in parts)
                        writer.WriteLine(part.Trim());                    
                    
                    writer.WriteLine();
                }
            }

            status = "Subtitle Generation Complete.";

            return srtFile;
        }

        private static List<SegmentData>? RemoveConsecutiveDuplicates(List<SegmentData> segments)
        {
            if (segments == null || segments.Count == 0)
                return segments;

            List<SegmentData> result = new List<SegmentData>();
            SegmentData? previous = null;

            foreach (var segment in segments)
            {
                if (previous == null || segment.Text != previous.Text)
                    result.Add(segment);

                previous = segment;
            }

            return result;
        }

        private static List<SegmentData>? RemoveConsecutiveDuplicatesKeepingLast(List<SegmentData> segments)
        {
            if (segments == null || segments.Count == 0)
                return segments;

            List<SegmentData> result = new List<SegmentData>();
            SegmentData? previous = null;

            for (int i = segments.Count - 1; i >= 0; i--)
            {
                var segment = segments[i];
                if (previous == null || segment.Text != previous.Text)
                    result.Add(segment);

                previous = segment;
            }

            result.Reverse();
            return result;
        }


        private static WhisperProcessor? SetupProcessor(string modelPath, string languageCode, bool shouldTranslate, OnSegmentEventHandler OnNewSegment)
        {
            var whisperFactory = WhisperFactory.FromPath(modelPath);

            var builder = whisperFactory.CreateBuilder()
                .WithSegmentEventHandler(OnNewSegment);

            if (languageCode.Length > 0)
                builder.WithLanguage(languageCode);
            else
                builder.WithLanguageDetection();

            if (shouldTranslate)
                builder.WithTranslate();

            return builder.Build();
        }

        public bool DoWorkMergeSubtitles(out string status, string srtFile)
        {
            string finalFile = $"{videoFilePath.FullFileNameWithoutExtention()}_final.mp4";
            string finalSubsFile = $"{videoFilePath.FullFileNameWithoutExtention()}_final_subs.mp4";

            string ffmpegArgs = $"-i \"{finalFile}\" -i \"{srtFile}\" -c copy -c:s mov_text \"{finalSubsFile}\"";

            // Set up the process to run FFmpeg
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
            ffmpeg.StartInfo.Arguments = ffmpegArgs;
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            // Start the process
            ffmpeg.Start();

            // Read the output
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            status = "Subtitle Merging Complete.";

            return true;
        }

        public bool DoWorkCleanup(out string status)
        {
            string finalFile = $"{videoFilePath.FullFileNameWithoutExtention()}_final.mp4";

            string? pathToClean = Path.GetDirectoryName(videoFilePath);

            if(pathToClean == null)
            {
                status = "Error: Path to clean does not exist or is inaccessible.";
                return false;
            }

            try
            {
                foreach (string f in Directory.EnumerateFiles(pathToClean, $"*_{fileNameIdentifier}*.*"))
                    File.Delete(f);
            }
            catch { }

            status = $"Cleanup Complete. Final file is {finalFile}";

            return true;
        }

        public double GetVideoDuration(string filePath, string ffmpegPath)
        {
            // Set up the process to run FFmpeg
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
            ffmpeg.StartInfo.Arguments = $"-i \"{filePath}\"";
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            // Start the process
            ffmpeg.Start();

            // Read the output
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

     //       File.WriteAllText("c:\\TestData\\durationOutput.txt", output);

            // Parse the duration from the output
            string durationString = "Duration: ";
            int startIndex = output.IndexOf(durationString) + durationString.Length;
            int endIndex = output.IndexOf(",", startIndex);
            string time = output.Substring(startIndex, endIndex - startIndex).Trim();

            // Convert the duration to seconds
            TimeSpan duration = TimeSpan.Parse(time);
            return duration.TotalSeconds;
        }

        public double GetVideoDurationAndBitrate(string filePath, string ffmpegPath, out double bitRate)
        {
            bitRate = 0.0;

            // Set up the process to run FFmpeg
            Process ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = $"\"{ffmpegPath}\\ffmpeg\"";
            ffmpeg.StartInfo.Arguments = $"-i \"{filePath}\"";
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;

            // Start the process
            ffmpeg.Start();

            // Read the output
            string output = ffmpeg.StandardError.ReadToEnd();
            ffmpeg.WaitForExit();

            //       File.WriteAllText("c:\\TestData\\durationOutput.txt", output);

            // Parse the duration from the output
            string durationString = "Duration: ";
            int startIndex = output.IndexOf(durationString) + durationString.Length;
            int endIndex = output.IndexOf(",", startIndex);
            string time = output.Substring(startIndex, endIndex - startIndex).Trim();

            // Parse the bitrate from the output
            string bitrateString = "bitrate: ";
            int startIndexBitrate = output.IndexOf(bitrateString) + bitrateString.Length;
            int endIndexBitrate = output.IndexOf(" ", startIndexBitrate);
            string br = output.Substring(startIndexBitrate, endIndexBitrate - startIndexBitrate).Trim();
            bitRate = Convert.ToDouble(br);

            // Convert the duration to seconds
            TimeSpan duration = TimeSpan.Parse(time);
            return duration.TotalSeconds;
        }
    }

    /// <summary>
    /// EDL file representation
    /// </summary>
    public class EdlSequence
    {
        public double startSec { get; set; }
        public double endSec { get; set; }
    }
}
