using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    [Serializable]
    public class StatData
    {
        public Dictionary<StatTypes, double> Stats { get; set; }

        public int GetStat(StatTypes type)
        {
            if (!Stats.ContainsKey(type))
            {
                Stats.Add(type, 0);
            }
            return Convert.ToInt32(Math.Floor(Stats[type]));
        }

        public double GetPreciseStat(StatTypes type)
        {
            return Stats[type];
        }

        public void AddStat(StatTypes type, int value)
        {
            if (!Stats.ContainsKey(type))
            {
                Stats[type] = value;
                return;
            }

            Stats[type] += value;
            return;
        }

        public void SetStat(StatTypes type, int value)
        {
            Stats[type] = value;
        }

        public void SetStat(StatTypes type, double value)
        {
            Stats[type] = value;
        }

        public StatData()
        {
            Stats = new Dictionary<StatTypes, double>();
        }
    }
}
