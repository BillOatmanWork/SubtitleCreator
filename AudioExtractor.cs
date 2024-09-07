

using NAudio.Wave;
using System.IO;

namespace SubtitleCreator
{
    public static class AudioExtractor
    {
        private static string fileNameIdentifier = "_!SubtitleCreator!";

#pragma warning disable CS8604
        public static string ExtractAudioFromVideoFile(string videoFilePath)
        {
            const int outRate = 16000;
            FileStream fileStream = File.OpenRead(videoFilePath);
            MemoryStream memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            fileStream.Close();
            StreamMediaFoundationReader reader = new StreamMediaFoundationReader(memoryStream);
            WaveFormat outFormat = new WaveFormat(outRate, reader.WaveFormat.Channels);
            MediaFoundationResampler resampler = new MediaFoundationResampler(reader, outFormat);

            string outputFilePath = Path.Combine(Path.GetDirectoryName(videoFilePath), $"{Path.GetFileNameWithoutExtension(videoFilePath)}{fileNameIdentifier}.wav");

            WaveFileWriter.CreateWaveFile(outputFilePath, resampler);

            return outputFilePath;
        }
#pragma warning restore CS8604
    }
}
