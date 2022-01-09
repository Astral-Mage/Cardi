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
        public List<BaseCard> DefeatedParticipants;

        /// <summary>
        /// 
        /// </summary>
        public Cards.BaseCard Winner;

        /// <summary>
        /// 
        /// </summary>
        public Cards.BaseCard Loser;

        /// <summary>
        /// 
        /// </summary>
        public EncounterResults()
        {
            ResponseStr = string.Empty;
            DefeatedParticipants = new List<BaseCard>();

            Winner = null;
            Loser = null;
        }
    }
}
