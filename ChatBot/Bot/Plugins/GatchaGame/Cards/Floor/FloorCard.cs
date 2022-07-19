using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards.Floor
{
    /// <summary>
    /// A floor card
    /// </summary>
    public class FloorCard
    {
        public string name;
        public int floor;
        public int currentxp;
        public int neededxp;
        public DateTime firstseen;
        public string rawenemies;
        public List<string> enemies;
        public string notes;
    }
}
