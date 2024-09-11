using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using Extensions;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitleCreator
{
    public enum ModelType { Tiny, Base, Small, Medium, Large }

    public class RemoveCommercials
    {
        public RemoveCommercials()
        {
        }


        public bool DoWorkGenerateSubtitles(string wavFilePath, ModelType modelType, string appDataDir, string srtFile, string languageCode, bool shouldTranslate)
        {
            List<SegmentData> segments = new();

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
                //Utilities.ConsoleWithLog($"Downloading Whisper AI model {modelName}. This might take a while depending on your internet speed..");
                //Utilities.ConsoleWithLog("The application might exit after downloading. Please restart the application manually in that case!");
                // var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(modelType);
                var modelStream = WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType).GetAwaiter().GetResult();
                var fileWriter = File.OpenWrite(modelPath);
                modelStream.CopyTo(fileWriter);
                fileWriter.Close();
                //Utilities.ConsoleWithLog("Downloaded model");
            }

            WhisperProcessor? processor = SetupProcessor(modelPath, languageCode, shouldTranslate, OnNewSegment);

            if (processor is null)
            {
                Utilities.ConsoleWithLog("Something went wrong while setting up the processor.");
                return false;
            }

            void OnNewSegment(SegmentData segmentData)
            {
                var startTime = Utilities.ConvertTimestampToSrtFormat(segmentData.Start);
                var endTime = Utilities.ConvertTimestampToSrtFormat(segmentData.End);
             //   Utilities.ConsoleWithLog($"CSSS {startTime} ==> {endTime} : {segmentData.Text}");
                segments.Add(segmentData);
            }

            

            //var audioData = File.ReadAllBytes(wavFilePath);
            //var mel = model.AudioToMel(audioData);
            //var (language, probs) = model.DetectLanguage(mel);

            //Console.WriteLine($"Detected language: {language}

            using (Stream fileStream = File.OpenRead(wavFilePath))
            {
                GC.KeepAlive(fileStream);
                GC.KeepAlive(processor);

                // Read the file stream into a byte array
                //byte[] audioData;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    fileStream.CopyTo(ms);
                //    audioData = ms.ToArray();
                //}

                //// Convert byte array to float array
                //// Ensure audioData length is a multiple of 4
                //int floatArrayLength = audioData.Length / sizeof(float);
                //float[] floatData = new float[floatArrayLength / 4];
                //Buffer.BlockCopy(audioData, 0, floatData, 0, (floatArrayLength / 4) * sizeof(float));

                //var detectedLanguage = processor.DetectLanguage(floatData);

                //if(detectedLanguage.Length > 0)
                //{
                //    Utilities.ConsoleWithLog($"Detected language: {detectedLanguage}");
                //}

                //fileStream.Seek(0, SeekOrigin.Begin);

                processor.Process(fileStream);
                processor.Dispose();
            }

            // Sort segments by start time
            segments.Sort((x, y) => x.Start.CompareTo(y.Start));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            // Remove all consecutive segments that have the same Text content.  For some reason it happens with some models.
            // segments = RemoveConsecutiveDuplicates(segments);
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

            Utilities.ConsoleWithLog("Subtitle Generation Complete.");

            return true;
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

        public bool DoWorkMergeSubtitles(string srtFile, string inFile, string finalFile, string ffmpegPath)
        {
            string ffmpegArgs = $"-i \"{inFile}\" -i \"{srtFile}\" -c copy -c:s srt -metadata:s:s:0 language=eng -map_metadata -1 \"{finalFile}\"";

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

            return true;
        }
    }
 
}
