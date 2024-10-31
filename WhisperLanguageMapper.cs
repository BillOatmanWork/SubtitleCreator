// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace SubtitleCreator
{
    public static class WhisperLanguageMapper
    {
        #region Whisper Language Mapping
        private static readonly Dictionary<string, (string FullName, string WhisperCode)> LanguageMap = new()
        {
            { "afr", ("Afrikaans", "af") },
            { "ara", ("Arabic", "ar") },
            { "hye", ("Armenian", "hy") },
            { "aze", ("Azerbaijani", "az") },
            { "bel", ("Belarusian", "be") },
            { "bos", ("Bosnian", "bs") },
            { "bul", ("Bulgarian", "bg") },
            { "cat", ("Catalan", "ca") },
            { "zho", ("Chinese", "zh") },
            { "hrv", ("Croatian", "hr") },
            { "ces", ("Czech", "cs") },
            { "dan", ("Danish", "da") },
            { "nld", ("Dutch", "nl") },
            { "eng", ("English", "en") },
            { "est", ("Estonian", "et") },
            { "fin", ("Finnish", "fi") },
            { "fra", ("French", "fr") },
            { "glg", ("Galician", "gl") },
            { "deu", ("German", "de") },
            { "ell", ("Greek", "el") },
            { "heb", ("Hebrew", "he") },
            { "hin", ("Hindi", "hi") },
            { "hun", ("Hungarian", "hu") },
            { "isl", ("Icelandic", "is") },
            { "ind", ("Indonesian", "id") },
            { "ita", ("Italian", "it") },
            { "jpn", ("Japanese", "ja") },
            { "kan", ("Kannada", "kn") },
            { "kaz", ("Kazakh", "kk") },
            { "kor", ("Korean", "ko") },
            { "lav", ("Latvian", "lv") },
            { "lit", ("Lithuanian", "lt") },
            { "mkd", ("Macedonian", "mk") },
            { "msa", ("Malay", "ms") },
            { "mar", ("Marathi", "mr") },
            { "mri", ("Maori", "mi") },
            { "nep", ("Nepali", "ne") },
            { "nor", ("Norwegian", "no") },
            { "fas", ("Persian", "fa") },
            { "pol", ("Polish", "pl") },
            { "por", ("Portuguese", "pt") },
            { "ron", ("Romanian", "ro") },
            { "rus", ("Russian", "ru") },
            { "srp", ("Serbian", "sr") },
            { "slk", ("Slovak", "sk") },
            { "slv", ("Slovenian", "sl") },
            { "spa", ("Spanish", "es") },
            { "swa", ("Swahili", "sw") },
            { "swe", ("Swedish", "sv") },
            { "tgl", ("Tagalog", "tl") },
            { "tam", ("Tamil", "ta") },
            { "tha", ("Thai", "th") },
            { "tur", ("Turkish", "tr") },
            { "ukr", ("Ukrainian", "uk") },
            { "urd", ("Urdu", "ur") },
            { "vie", ("Vietnamese", "vi") },
            { "cym", ("Welsh", "cy") }
        };
        #endregion Whisper Language Mapping

        /// <summary>
        /// Method to list all FFmpeg language codes with their full names
        /// </summary>
        /// <returns></returns>
        public static List<string> ListAllLanguages()
        {
            var languages = new List<string>();
            foreach (var entry in LanguageMap)
            {
                languages.Add($"{entry.Key}: {entry.Value.FullName}");
            }

            return languages;
        }

        /// <summary>
        /// Method to get the WhisperFactory code from an FFmpeg language code
        /// </summary>
        /// <param name="ffmpegCode">The FFmpeg language code of the audio.</param>
        /// <returns></returns>
        public static string GetWhisperCode(string ffmpegCode)
        {
            if (LanguageMap.TryGetValue(ffmpegCode, out var languageInfo))
            {
                return languageInfo.WhisperCode;
            }

            return string.Empty;
        }

        public static string GetLanguageFullName(string code)
        {
            if (LanguageMap.TryGetValue(code, out var languageInfo))
            {
                return languageInfo.FullName;
            }
            else
            {
                return "Unknown language code";
            }
        }
    }
}
