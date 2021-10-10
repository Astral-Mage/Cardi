namespace ChatBot
{
    /// <summary>
    /// Simple static utility class
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Replaces the first instance of a string inside another string
        /// </summary>
        /// <param name="text">string to replace something in</param>
        /// <param name="search">string we're replacing</param>
        /// <param name="replace">string to replace wtih</param>
        /// <returns>the new string</returns>
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
