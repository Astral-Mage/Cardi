using Accord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.DungeonDiver.Minigames.PlayingCardGame
{
    public class PlayingCard
    {
        public Rank _rank;
        public Suit _suit;

        public string GetDescription()
        {
            return $"<{_rank.GetDescription()} {_suit.GetDescription()}>";
        }
    }
}
