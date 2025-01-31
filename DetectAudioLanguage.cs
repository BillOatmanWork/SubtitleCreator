using System;
using System.IO;
using Whisper.net;

namespace SubtitleCreator
{
    public static class DetectAudioLanguage
    {
        public static string? DetectLanguage(string pathToModel, string pathToAudioFile)
        {
            var whisperFactory = WhisperFactory.FromPath(pathToModel);
            var whisper = whisperFactory.CreateBuilder().WithLanguageDetection().Build();
            var floatAudioData = LoadFirst30SecondsAsFloatArray(pathToAudioFile);
            var detectedLanguage = whisper.DetectLanguage(floatAudioData);

            return detectedLanguage;
        }

        private static float[] LoadFirst30SecondsAsFloatArray(string pathToAudioFile)
        {
            using (var reader = new BinaryReader(File.OpenRead(pathToAudioFile)))
            {
                // Read the WAV header to determine format
                var header = reader.ReadBytes(44);
                int sampleRate = BitConverter.ToInt32(header, 24);
                int bitsPerSample = BitConverter.ToInt16(header, 34);
                int numChannels = BitConverter.ToInt16(header, 22);

                // Calculate the number of bytes for 30 seconds of audio
                int bytesPerSample = bitsPerSample / 8;
                int bytesPerSecond = sampleRate * bytesPerSample * numChannels;
                int bytesToRead = bytesPerSecond * 30;

                var byteArray = reader.ReadBytes(bytesToRead);

                int floatCount = byteArray.Length / bytesPerSample;
                float[] floatArray = new float[floatCount];

                for (int i = 0; i < floatCount; i++)
                {
                    switch (bitsPerSample)
                    {
                        case 8:
                            floatArray[i] = (byteArray[i] - 128) / 128f; // 8-bit audio
                            break;
                        case 16:
                            floatArray[i] = BitConverter.ToInt16(byteArray, i * bytesPerSample) / 32768f; // 16-bit audio
                            break;
                        case 24:
                            int sample24 = byteArray[i * 3] | (byteArray[i * 3 + 1] << 8) | (byteArray[i * 3 + 2] << 16);
                            if ((sample24 & 0x800000) != 0) sample24 |= unchecked((int)0xFF000000); // Sign extend if negative
                            floatArray[i] = sample24 / 8388608f; // 24-bit audio
                            break;
                        case 32:
                            floatArray[i] = BitConverter.ToInt32(byteArray, i * bytesPerSample) / (float)Int32.MaxValue; // 32-bit audio
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported bits per sample: {bitsPerSample}");
                    }
                }

                return floatArray;
            }
        }
    }
}
