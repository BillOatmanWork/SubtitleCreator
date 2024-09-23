﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitleCreator
{
    /// <summary>
    /// All the different Whisper models available
    /// </summary>
    public enum ModelType { Tiny, Base, Small, Medium, Large }

    public class CreateTheSubtitles
    {
        public CreateTheSubtitles()
        {
        }

        /// <summary>
        /// Generate the subtitles from the audio (wav) file
        /// </summary>
        /// <param name="wavFilePath"></param>
        /// <param name="modelType"></param>
        /// <param name="appDataDir"></param>
        /// <param name="srtFile"></param>
        /// <param name="languageCode"></param>
        /// <param name="shouldTranslate"></param>
        /// <param name="audioLanguage"></param>
        /// <returns></returns>
        public bool DoWorkGenerateSubtitles(string wavFilePath, ModelType modelType, string workingDir, string srtFile, string languageCode, bool shouldTranslate, string audioLanguage)
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
            string modelPath = Path.Combine(workingDir, modelName);

            segments.Clear();
            if (!File.Exists(modelPath))
            {
                Utilities.ConsoleWithLog($"Downloading Whisper model {modelName}.");
                try
                {
                    using (Stream modelStream = WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType).GetAwaiter().GetResult())
                    using (FileStream fileWriter = File.OpenWrite(modelPath))
                    {
                        modelStream.CopyTo(fileWriter);
                    }

                    Utilities.ConsoleWithLog("Whisper model download complete.");
                }
                catch (Exception ex)
                {
                    Utilities.ConsoleWithLog($"Exception downloading model: {ex.Message}");
                    return false;
                }
            }

            void OnNewSegment(SegmentData segmentData)
            {
                var startTime = Utilities.ConvertTimespanToSrtFormat(segmentData.Start);
                var endTime = Utilities.ConvertTimespanToSrtFormat(segmentData.End);
                segments.Add(segmentData);
            }

            using (WhisperProcessor? processor = SetupProcessor(modelPath, languageCode, shouldTranslate, audioLanguage, OnNewSegment))
            {
                if (processor is null)
                {
                    Utilities.ConsoleWithLog("Something went wrong while setting up the Whisper processor.");
                    return false;
                }

                using (Stream fileStream = File.OpenRead(wavFilePath))
                {
                    //GC.KeepAlive(fileStream);
                    //GC.KeepAlive(processor);

                    processor.Process(fileStream);
                }
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
                    var startTime = Utilities.ConvertTimespanToSrtFormat(segment.Start);
                    var endTime = Utilities.ConvertTimespanToSrtFormat(segment.End);
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

        /// <summary>
        /// Remove duplicate entries in the list of segments, keeping only the first one.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Remove duplicate entries in the list of segments, keeping only the last one.
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create the Whisper processor
        /// </summary>
        /// <param name="modelPath"></param>
        /// <param name="languageCode"></param>
        /// <param name="shouldTranslate"></param>
        /// <param name="audioLanguage"></param>
        /// <param name="OnNewSegment"></param>
        /// <returns></returns>
        private static WhisperProcessor? SetupProcessor(string modelPath, string languageCode, bool shouldTranslate, string audioLanguage, OnSegmentEventHandler OnNewSegment)
        {
            var whisperFactory = WhisperFactory.FromPath(modelPath);

            var builder = whisperFactory.CreateBuilder()
                .WithSegmentEventHandler(OnNewSegment);

            if (languageCode.Length > 0)
                builder.WithLanguage(languageCode);
            else
                builder.WithLanguageDetection();

            if (shouldTranslate)
            { 
                switch(audioLanguage)
                {
                    case "eng":
                        builder.WithLanguage("en");
                        break;
                    case "fra":
                        builder.WithLanguage("fr");
                        break;
                    case "spa":
                        builder.WithLanguage("es");
                        break;
                }

                builder.WithTranslate();
            }

            return builder.Build();
        }

        /// <summary>
        /// Merge the video file and the subtitles file into a MKV container
        /// </summary>
        /// <param name="srtFile"></param>
        /// <param name="inFile"></param>
        /// <param name="finalFile"></param>
        /// <param name="ffmpegPath"></param>
        /// <param name="audioLanguage"></param>
        /// <returns></returns>
        public bool DoWorkMergeSubtitles(string srtFile, string inFile, string finalFile, string ffmpegPath, string audioLanguage)
        {
            // set audio track to spanish, french, english
            // -metadata:s:a:0 language=spa    fra   eng
            string ffmpegArgs = $"-i \"{inFile}\" -i \"{srtFile}\" -c copy -c:s srt -metadata:s:s:0 language=eng  -metadata:s:a:0 language={audioLanguage} \"{finalFile}\"";

            // Set up the process to run FFmpeg
            using (Process ffmpeg = new Process())
            {
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

            return true;
        }
    }
}