using System.Globalization;

namespace Extensions
{
    public static class Extensions
    {
        /// <summary>
        /// Get the file name and path without the extension
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FullFileNameWithoutExtention(this string fileName)
        {
            int lastIndex = fileName.LastIndexOf(".");
            if (lastIndex != -1)
            {
                fileName = fileName.Substring(0, lastIndex);
            }
            return fileName;
        }

        /// <summary>
        /// Split string and trim all of the pieces
        /// </summary>
        /// <param name="data"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string[] SplitTrim(this string data, char arg)
        {
            string[] ar = data.Split(arg);
            for (int i = 0; i < ar.Length; i++)
            {
                ar[i] = ar[i].Trim();
            }
            return ar;
        }

        /// <summary>
        /// Converts the given string to a title cased string in the en-US culture.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <returns>Title cased string.</returns>
        public static string ToTitleCase(this string s)
        {
            return new CultureInfo("en-US").TextInfo.ToTitleCase(s);
        }

        /// <summary>
        /// Removes the given character from the given string and returns the new string.
        /// </summary>
        /// <param name="s">The given string.</param>
        /// <param name="c">The character to be removed.</param>
        /// <returns>The new string.</returns>
        public static string RemoveChar(this string s, char c)
        {
            return s.Replace(c.ToString(), string.Empty);
        }
    }   
}
