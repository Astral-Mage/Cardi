using System;
using System.Linq;
using WeCantSpell.Hunspell;
using OpenNLP.Tools.SentenceDetect;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.RoleplaySystem
{
    public class RoleplayController
    {
        private static RoleplayController instance;

        private readonly WordList Dictionary;

        private RoleplayController() 
        {
            Dictionary = WordList.CreateFromFiles(@"../../Dictionaries/en_US.dic");
        }

        private static readonly object iLocker = new object();

        public static RoleplayController Instance
        {
            get
            {
                lock (iLocker)
                {
                    if (instance == null)
                    {
                        instance = new RoleplayController();
                    }
                    return instance;
                }
            }
        }

        private double CalculateLimitExperience(double spw, int misspellings, int words, Roleplay rpData)
        {
            double overallSyllablesPerWord = (rpData.TotalWords > 0) ? rpData.TotalSyllables / (double)rpData.TotalWords : 0;
            double overallWordsPerPost = (rpData.TotalPosts > 0) ? rpData.TotalWords / (double)rpData.TotalPosts : 0;
            double overallMisspellingsPerPost = (rpData.TotalPosts > 0) ? rpData.TotalMisspellings / (double)rpData.TotalPosts : 0;

            double currentSyllablesPerWord = spw;
            double currentWordsPerPost = words;
            double currentMisspellingsPerPost = misspellings;

            double SyllablesPerWordDeviation = 0;
            if (currentSyllablesPerWord > overallSyllablesPerWord)
            {
                SyllablesPerWordDeviation = 1;
            }
            else if (currentSyllablesPerWord < overallSyllablesPerWord)
            {
                SyllablesPerWordDeviation = -1;
            }

            double WordsPerPostDeviation = 0;
            if (currentWordsPerPost > overallWordsPerPost)
            {
                WordsPerPostDeviation = 1;
            }
            else if (currentWordsPerPost > overallWordsPerPost)
            {
                WordsPerPostDeviation = -1;
            }

            double MisspellingsPerPostDeviation = 0;
            if (currentMisspellingsPerPost < overallMisspellingsPerPost)
            {
                MisspellingsPerPostDeviation = 1;
            }
            else if (currentMisspellingsPerPost > overallMisspellingsPerPost)
            {
                MisspellingsPerPostDeviation = -0.5;
            }

            double overallQuality = Convert.ToInt32(SyllablesPerWordDeviation + WordsPerPostDeviation + MisspellingsPerPostDeviation);
            double toReturn = overallQuality > 0 ? overallQuality * 5.0 : 0;
            return toReturn;
        }

        private double CalculateLengthExperience(int sentences, int words)
        {
            int MIN_LEN = 20;
            if (words < MIN_LEN)
            {
                return 0;
            }

            double ourX = (.25 * sentences) + (.25 * (words));
            double flatReturn = 3;
            if (words > 25)
                flatReturn += 3;
            else if (words > 40)
                flatReturn += 7;
            return (.1 * ourX) + flatReturn;
        }

        private double CalculateBaseExperience(double fkVal, int misspellings, int words)
        {                                                                                           
            if (fkVal > 100)
                fkVal = 100;

            double fkMax = 120;
            double flatOne = -0.008;
            double flatTwo = 0.65;
            double fkPow = Math.Pow(fkVal, 2);
            double xp = ((flatOne * fkPow) + fkMax)  * flatTwo;
            double toReturn = xp;

            if (fkVal == 100)
                toReturn *= .75;

            if (words - misspellings < (double)words * .2)
                toReturn *= .8;

            if (words < 20)
                toReturn *= .3;
            else if (words < 40)
            {
                toReturn *= .75;
            }

            return Math.Round(toReturn, 2);
        }

        public void ParsePost(string user, string message, string channel = null)
        {
            int sentences;
            int words = 0;
            int chars = 0;
            int misspellings = 0;
            int syllables = 0;
            int spamwords = 0;
            int paragraphs = 0;

            string replyString = string.Empty;
            EnglishMaximumEntropySentenceDetector sd = new EnglishMaximumEntropySentenceDetector("../../Dictionaries/EnglishSD.nbin");
            UserCard card = DataDb.CardDb.GetCard(user);

            paragraphs = message.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
            var parsedpost = sd.SentenceDetect(message);
            sentences = parsedpost.Length;
            
            foreach (var v in parsedpost)
            {
                var wordsInSentence = v.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                words += wordsInSentence.Count();


                foreach(var word in wordsInSentence)
                {
                    int syls = word.SyllableCount();
                    syllables += syls;
                    chars += word.Length;
                    if (!Char.IsUpper(word.First()))
                    {
                        misspellings += (Dictionary.Check(word) == false) ? 1 : 0;
                    }

                    if (word.Length > 10)
                    {
                        var groupedByLetter = word.GroupBy(c => c).Select(c => new { Letter = c.Key, Count = c.Count() });
                        if (groupedByLetter.Any(x => (x.Count / word.Length) > .5)) spamwords++;
                    }
                }
            }

            double flatA = 206.835;
            double flatB = 1.015;
            double flatC = 84.6;
            double wordsOverSentences = Math.Round((double)words / sentences, 2);
            double syllablesOverWords = Math.Round((double)syllables / words, 2);
            double FleshKincaid = Math.Round(flatA - (flatB * wordsOverSentences) - (flatC * syllablesOverWords), 2);
            double wps = Math.Round((double)words / sentences, 2);
            double spw = Math.Round((double)syllables / words, 2);


            double Experience = CalculateBaseExperience(FleshKincaid, misspellings, words);
            double LenXp = CalculateLengthExperience(sentences, words);
            double LimitXp = CalculateLimitExperience(syllables / (double)words, misspellings, words, card.RpData);


            card.RpData.TotalPosts += 1;
            card.RpData.TotalParagraphs += paragraphs;
            card.RpData.TotalSentences += sentences;
            card.RpData.TotalSyllables += syllables;
            card.RpData.TotalWords += words;
            card.RpData.TotalMisspellings += misspellings;
            card.RpData.TotalExperience += Experience + LenXp + LimitXp;
            double postpoints = Experience + LenXp + LimitXp;
            int exp = Convert.ToInt32(Math.Round(postpoints, 0));
            card.Stats.AddStat(StatTypes.Experience, exp);
            card.RpData.UpdatePostHistory(postpoints);

            if (channel != null) channel = null;
            //if (card.Verbose) SystemController.Instance.Respond(channel,$"Post Experience Earned: {exp}", user);
            DataDb.CardDb.UpdateUserCard(card);
            DataDb.RpDb.UpdateUserRoleplayData(card.RpData);
        }
    }
}
