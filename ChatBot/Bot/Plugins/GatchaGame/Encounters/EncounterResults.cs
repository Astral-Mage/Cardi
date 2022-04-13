using ChatApi.Objects;
using ChatBot.Bot.Plugins.GatchaGame.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    public class EncounterResults
    {
        /// <summary>
        /// 
        /// </summary>
        public string ResponseStr;

        /// <summary>
        /// 
        /// </summary>
        public int WinningTeam;

        /// <summary>
        /// 
        /// </summary>
        public List<EncounterCard> Participants;

        /// <summary>
        /// 
        /// </summary>
        public int TotalRounds;

        public Dictionary<string, List<Reward>> AllRewards;

        public List<Turn> _Turns;

        public string GetResultsOutput()
        {
            string toReturn = string.Empty;



            return toReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        public EncounterResults()
        {
            ResponseStr = string.Empty;
            WinningTeam = -1;
            Participants = new List<EncounterCard>();
            TotalRounds = 0;
            _Turns = new List<Turn>();
            AllRewards = new Dictionary<string, List<Reward>>();
        }

        public int GetWinningTeam()
        {
            return WinningTeam;
        }
    }
}
