using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Roleplay
    {
        public int TotalPosts { get; set; }

        public int TotalSentences { get; set; }

        public int TotalParagraphs { get; set; }

        public int TotalSyllables { get; set; }

        public int TotalWords { get; set; }

        public Dictionary<DateTime, double> PostHistory { get; set; }

        public int UserId { get; set; }

        public double TotalExperience { get; set; }

        public int TotalMisspellings { get; set; }

        public double FKScore { get; set; }

        public Roleplay()
        {
            TotalPosts = 0;
            TotalSentences = 0;
            TotalParagraphs = 0;
            TotalSyllables = 0;
            TotalWords = 0;
            TotalMisspellings = 0;
            TotalExperience = 0;
            FKScore = 0;
            PostHistory = new Dictionary<DateTime, double>();

            UserId = -1;
        }

        public void UpdatePostHistory(double postpoints)
        {
            var sortedDict = from entry in PostHistory orderby entry.Key ascending select entry;
            List<DateTime> keysToRemove = new List<DateTime>();
            DateTime now = DateTime.Now;
            foreach (var value in sortedDict)
            {
                if (now - TimeSpan.FromHours(48) > value.Key)
                {
                    keysToRemove.Add(value.Key);
                }
                else
                {
                    break;
                }
            }

            foreach (var todelete in keysToRemove)
            {
                PostHistory.Remove(todelete);
            }

            PostHistory.Add(now, postpoints);
        }
    }
}
