using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    [Serializable]
    public class Reward
    {
        public static string CreateRewardString(List<Reward> rewards)
        {
            string toReturn = string.Empty;
            foreach (var rew in rewards)
            {

            }

            return toReturn;
        }

        public Dictionary<StatTypes, double> StatRewards;
        public List<Socket> SocketRewards;
        public RewardTypes RewardType;
        public double Splits;

        public Reward(RewardTypes type, StatTypes stattype, double stat, double splits = 1)
        {
            StatRewards = new Dictionary<StatTypes, double>
            {
                { stattype, stat }
            };
            SocketRewards = null;
            RewardType = type;
            Splits = splits;
        }

        public Reward(RewardTypes type, Socket item)
        { 
            StatRewards = null;
            RewardType = type;
            SocketRewards = new List<Socket>
            {
                item
            };
        }

        Reward()
        {

        }
    }
}
