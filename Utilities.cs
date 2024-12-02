using System;
using System.Globalization;
using System.IO;
using Whisper.net.Ggml;

namespace SubtitleCreator
{
    public static class Utilities
    {
        public static string LogName = "SubtitleCreator";
        public static string SubtitlesFolder = "Subtitles";
        public static string WavFolder = "RawAudio";
        public static string Models = "Models";
        public static bool noLog;

        /// <summary>
        /// Convert from te enum to the actual Whisper model name
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static string GgmlTypeToString(GgmlType modelType)
        {
            var modelTypeString = modelType switch
            {
                GgmlType.Base => "base",
                GgmlType.BaseEn => "base-en",
                GgmlType.LargeV2 => "large-v2",
                GgmlType.LargeV1 => "large-v1",
                GgmlType.LargeV3 => "large-v3",
                GgmlType.Medium => "medium",
                GgmlType.MediumEn => "medium-en",
                GgmlType.Small => "small",
                GgmlType.SmallEn => "small-en",
                GgmlType.Tiny => "tiny",
                GgmlType.TinyEn => "tiny-en",
                _ => "unknown"
            };

            return $"ggml-{modelTypeString}.bin";
        }

        /// <summary>
        /// Convert q timespan to what SRT files use
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static string ConvertTimespanToSrtFormat(TimeSpan timestamp)
        {
            return timestamp.ToString("hh\\:mm\\:ss\\,fff", CultureInfo.InvariantCulture).Replace(".", ",");
        }

        /// <summary>
        /// Delete any existing log file
        /// </summary>
        public static void CleanLog()
        {
            File.Delete($"{LogName}.log");
        }

        /// <summary>
        /// Write the supplied strong to the console and optionally to a log file
        /// </summary>
        /// <param name="text"></param>
        public static void ConsoleWithLog(string text)
        {
            Console.WriteLine(text);

            if (noLog == false)
            {
                using (StreamWriter file = File.AppendText($"{LogName}.log"))
                {
                    file.Write(text + Environment.NewLine);
                }
            }
        }
    }
}
