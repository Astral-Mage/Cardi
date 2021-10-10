using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards.Stats
{
    [Serializable]
    public class Modifier
    {
        public StatTypes StatType;
        public int TurnDuration;
        public TurnSteps UpdateStep;
    }
}
