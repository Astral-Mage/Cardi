using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Core.Rooms
{
    public class RoomResults
    {
        public Room RoomCleared;

        public List<EnemyCard> EnemiesDefeated;
        public List<Socket> SocketRewards;
        public Dictionary<StatTypes, int> StatRewards;
        public List<JObject> MiscRewards;
        public int TotalRounds;

        public Dictionary<string, List<Reward>> AllRewards;
        public EncounterResults EncounterResults;
        public RoomResults()
        {
            EnemiesDefeated = new List<EnemyCard>();
            RoomCleared = new Room();
            SocketRewards = new List<Socket>();
            StatRewards = new Dictionary<StatTypes, int>();
            MiscRewards = new List<JObject>();
            TotalRounds = 0;

            AllRewards = new Dictionary<string, List<Reward>>();
            EncounterResults = null;
        }
    }
}
