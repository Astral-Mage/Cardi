using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class StatData
    {
        public Dictionary<StatTypes, double> Stats;
        public List<Modifier> Modifiers;

        public void AddModifier(StatTypes statType, int numTurns)
        {
            Modifiers.Add(new Modifier() { StatType = statType, TurnDuration = numTurns });
        }

        public int GetStat(StatTypes type, bool baseStat = false)
        {
            return Convert.ToInt32(Math.Floor(Stats[type]));
        }

        public double GetPreciseStat(StatTypes type, bool baseStat = false)
        {
            return Stats[type];
        }

        public bool AddStat(StatTypes type, int value)
        {
            if (Stats.ContainsKey(type))
                return false;

            Stats[type] = value;
            return true;
        }

        public bool TryGetStat(StatTypes type, out int value, bool baseStat = false)
        {
            value = 0;
            try
            {
                value = GetStat(type, baseStat);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public StatData()
        {
            Stats = new Dictionary<StatTypes, double>();
            Modifiers = new List<Modifier>();
        }
    }
}
