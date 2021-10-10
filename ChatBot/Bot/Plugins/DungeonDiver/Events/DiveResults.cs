using System.Collections.Generic;

namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// lists out the results of our dives
    /// </summary>
    public class DiveResults
    {
        public int gold;
        public int xp;
        public int prog;
        public FloorCard fc;
        public PlayerCard pc;
        public string eventReturnDescription = string.Empty;
        public List<Event> events = new List<Event>();
    }
}
