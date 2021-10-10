using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.DungeonDiver.Minigames.PlayingCardGame
{
    public class Game
    {
        public List<Player> Players;
        public List<PlayingCard> Deck;
        public List<PlayingCard> Table = new List<PlayingCard>();
        public List<PlayingCard> Discards = new List<PlayingCard>();
    }
}