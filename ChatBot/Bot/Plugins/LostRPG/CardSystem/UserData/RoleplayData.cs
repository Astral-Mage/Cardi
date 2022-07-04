using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class RoleplayData
    {
        public int TotalPosts { get; set; }

        public int TotalSentences { get; set; }

        public int TotalParagraphs { get; set; }

        public int TotalSyllables { get; set; }

        public int TotalWords { get; set; }

        public string PostHistory { get; set; }

        public int UserId { get; set; }

        public double TotalExperience { get; set; }


        public int TotalMisspellings { get; set; }

        public RoleplayData()
        {
            TotalPosts = 0;
            TotalSentences = 0;
            TotalParagraphs = 0;
            TotalSyllables = 0;
            TotalWords = 0;
            TotalMisspellings = 0;
            TotalExperience = 0;

            PostHistory = string.Empty;

            UserId = -1;
        }
    }
}
