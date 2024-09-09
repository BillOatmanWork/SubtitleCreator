using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whisper.net.Ggml;

namespace SubtitleCreator
{
    public static class Utilities
    {
        public static string LogName = "SubtitleCreator";
        public static string SubtitlesFolder = "Subtitles";
        public static string WavFolder = "RawAudio";
        public static string Models = "Models";
        public static bool noLog = false;

        public static void CreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

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
        public static string ConvertTimestampToSrtFormat(TimeSpan timestamp)
        {
            return timestamp.ToString("hh\\:mm\\:ss\\,fff").Replace(".", ",");
        }

        public static void CleanLog()
        {
            File.Delete($"{LogName}.log");
        }

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
