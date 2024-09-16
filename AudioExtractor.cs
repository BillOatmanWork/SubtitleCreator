

using NAudio.Wave;
using System;
using System.IO;

namespace SubtitleCreator
{
    /// <summary>
    /// Extracts audio from a video file.
    /// </summary>
    public static class AudioExtractor
    {
        private static string fileNameIdentifier = "_!SubtitleCreator!";
        private static string outputFilePath = string.Empty;

        public static string ExtractAudioFromVideoFile(string videoFilePath)
        {
            const int outRate = 16000;
            outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");

            using (var reader = new MediaFoundationReader(videoFilePath))
            {
                try
                {
                    WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                    using (var resampler = new MediaFoundationResampler(reader, outFormat))
                    {
                        resampler.ResamplerQuality = 60; // Adjust quality if needed
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }
                catch (Exception ex)
                {
                    Utilities.ConsoleWithLog($"Exception extracting audio from the video file. {ex.Message}");
                    outputFilePath = string.Empty;
                }
            }

            return outputFilePath;
        }
    }
}
