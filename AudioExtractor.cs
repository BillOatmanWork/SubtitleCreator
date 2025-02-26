// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;

namespace SubtitleCreator
{
    /// <summary>
    /// Extracts audio from a video file.
    /// </summary>
    public static class AudioExtractor
    {
        private const string fileNameIdentifier = "_!SubtitleCreator!";
        private static string outputFilePath = string.Empty;

        /// <summary>
        /// Enry point to extract the audio fromthe video file so Whisper can process it.
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="attemptRepair"></param>
        /// <param name="ffmpegPath"></param>
        /// <returns></returns>
        public static string ExtractAudioFromVideoFile(string videoFilePath, bool attemptRepair, string ffmpegPath)
        {
            string newVideoFilePath = videoFilePath;
            bool tempFileCreated = false;

            if (Path.GetExtension(videoFilePath).ToLower() == ".mkv")
            {
                Utilities.ConsoleWithLog("Input file is in a MKV container.  Extracting the video.");

                newVideoFilePath = ExtractVideoFromMKV(videoFilePath, ffmpegPath);

                Utilities.ConsoleWithLog("The video file has been extracted.  Now let's get back to extracting its audio.");

                tempFileCreated = true;
            }

            outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");

            try
            {
                ExtractTheAudio(newVideoFilePath);
            }
            catch (Exception ex)
            {
                if (!ex.Message.ToLower().Contains("media type is invalid"))
                {
                    Utilities.ConsoleWithLog($"Unrecoverable exception extracting audio from the video file. {ex.Message}");
                    Utilities.ConsoleWithLog("Not attempting to repair.");
                    outputFilePath = string.Empty;
                }
                else
                if (attemptRepair == false)
                {
                    Utilities.ConsoleWithLog($"Exception extracting audio from the video file. {ex.Message}");
                    Utilities.ConsoleWithLog("Not attempting to repair.");
                    outputFilePath = string.Empty;
                }
                else
                if (string.IsNullOrEmpty(ffmpegPath))
                {
                    Utilities.ConsoleWithLog($"Exception extracting audio from the video file and ffmpegPath not specified. {ex.Message}");
                    Utilities.ConsoleWithLog("Not attempting to repair.");
                    outputFilePath = string.Empty;
                }
                else
                {
                    Utilities.ConsoleWithLog($"Exception extracting audio from the video file.  {ex.Message}");
                    Utilities.ConsoleWithLog("Attempting to repair.");

                    string repairedFile = RepairAudio(videoFilePath, ffmpegPath);
                    try
                    {
                        ExtractTheAudio(repairedFile);
                    }
                    catch (Exception ex2)
                    {
                        Utilities.ConsoleWithLog($"Exception extracting audio from the repaired video file.  {ex2.Message}");
                        outputFilePath = string.Empty;
                    }
                    finally
                    {
                        File.Delete(repairedFile);
                    }
                }
            }

            if (tempFileCreated == true)
                File.Delete(newVideoFilePath);

            return outputFilePath;
        }

        /// <summary>
        /// Perform the actual audio extraction.
        /// </summary>
        /// <param name="videoFilePath"></param>
        private static void ExtractTheAudio(string videoFilePath)
        {
            const int outRate = 16000;

            using (var reader = new MediaFoundationReader(videoFilePath))
            {
                WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                using (var resampler = new MediaFoundationResampler(reader, outFormat))
                {
                    resampler.ResamplerQuality = 60; // Adjust quality if needed
                    WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                }
            }
        }

        /// <summary>
        /// Attempt to repair the audio in the video file.  This is a last ditch effort to get the audio extracted.  If this fails, the audio extraction will fail.
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <param name="ffmpegPath"></param>
        /// <returns></returns>
        private static string RepairAudio(string videoFilePath, string ffmpegPath)
        {
            // repair audio ffmpeg" -i "NFL Fantasy Live 2024_09_20_18_00_00.ts" -c:v copy -c:a aac "NFL Fantasy Live 2024_09_20_18_00_00.mp4"
            string intermediateFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.mp4");
            string ffmpegArgs = $"-i \"{videoFilePath}\" -c:v copy -c:a aac \"{intermediateFilePath}\"";

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

            return intermediateFilePath;
        }

        private static string ExtractVideoFromMKV(string videoFilePath, string ffmpegPath)
        {
            // check for mkv input file and use ffmpeg to convert to mp4  ffmpeg -i input.mkv -c copy -map 0:v -map 0:a output_video.mp4
            string intermediateFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.mp4");
            string ffmpegArgs = $"-i \"{videoFilePath}\" -c copy -map 0:v -map 0:a \"{intermediateFilePath}\"";

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

            return intermediateFilePath;
        }
    }
}
