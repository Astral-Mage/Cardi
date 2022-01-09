using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    class EncounterCard
    {
        /// <summary>
        /// 
        /// </summary>
        public Cards.BaseCard Participant;

        /// <summary>
        /// 
        /// </summary>
        public int Team;

        public EncounterCard(Cards.BaseCard participant, int team)
        {
            Participant = participant;
            Team = team;
        }
    }
}
