

using NAudio.Wave;
using System;
using System.IO;

namespace SubtitleCreator
{
    public static class AudioExtractor
    {
        private static string fileNameIdentifier = "_!SubtitleCreator!";
        private static string outputFilePath = string.Empty;

#pragma warning disable CS8604
        //public static string ExtractAudioFromVideoFile(string videoFilePath)
        //{
        //    const int outRate = 16000;
        //    using (FileStream fileStream = File.OpenRead(videoFilePath))
        //    {
        //        using (MemoryStream memoryStream = new MemoryStream())
        //        {
        //            fileStream.CopyTo(memoryStream);
        //            fileStream.Close();
        //            using (StreamMediaFoundationReader reader = new StreamMediaFoundationReader(memoryStream))
        //            {
        //                WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
        //                using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, outFormat))
        //                {
        //                    outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");
        //                    WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
        //                }
        //            }
        //        }
        //    }

        //    return outputFilePath;
        //}

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

#pragma warning restore CS8604
    }
}
