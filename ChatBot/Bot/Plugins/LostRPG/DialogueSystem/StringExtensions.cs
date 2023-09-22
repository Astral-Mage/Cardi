using System;

namespace ChatBot.Bot.Plugins.LostRPG.DialogueSystem
{
    public static class StringExtensions
    {
        public static int Count(this string input, string substr)
        {
            int freq = 0;

            int index = input.IndexOf(substr);
            while (index >= 0)
            {
                index = input.IndexOf(substr, index + substr.Length);
                freq++;
            }
            return freq;
        }
    }
}
