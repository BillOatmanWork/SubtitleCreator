

using NAudio.Wave;
using System.IO;

namespace SubtitleCreator
{
    public static class AudioExtractor
    {
        private static string fileNameIdentifier = "_!SubtitleCreator!";
        private static string outputFilePath = string.Empty;

#pragma warning disable CS8604
        public static string ExtractAudioFromVideoFile(string videoFilePath)
        {
            const int outRate = 16000;
            using (FileStream fileStream = File.OpenRead(videoFilePath))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                    using (StreamMediaFoundationReader reader = new StreamMediaFoundationReader(memoryStream))
                    {
                        WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
                        using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, outFormat))
                        {
                            outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");
                            WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                        }
                    }
                }
            }

            return outputFilePath;
        }

        //public static string ExtractAudioFromVideoFile(string videoFilePath)
        //{
        //    const int outRate = 16000;
        //    const int bufferSize = 81920; // 80 KB buffer size
        //    string outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");

        //    using (FileStream fileStream = File.OpenRead(videoFilePath))
        //    using (MemoryStream memoryStream = new MemoryStream())
        //    {
        //        byte[] buffer = new byte[bufferSize];
        //        int bytesRead;
        //        while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
        //        {
        //            memoryStream.Write(buffer, 0, bytesRead);
        //        }

        //        memoryStream.Position = 0; // Reset the position to the beginning of the stream
        //        using (StreamMediaFoundationReader reader = new StreamMediaFoundationReader(memoryStream))
        //        {
        //            WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
        //            using (MediaFoundationResampler resampler = new MediaFoundationResampler(reader, outFormat))
        //            {
        //                WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
        //            }
        //        }
        //    }

        //    return outputFilePath;
        //}
#pragma warning restore CS8604
    }
}
