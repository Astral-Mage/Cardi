using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem
{
    public class UserCard
    {
        // user stuff
        public string Name { get; protected set; }

        public int UserId { get; set; }

        public string Alias { get; set; }

        public RoleplayData RpData { get; set; }


        public StatData Stats { get; set; }

        public bool Verbose { get; set; }

        public string CurrentTitle { get; set; }

        public List<string> Titles { get; set; }

        public List<int> Skills { get; set; }

        public Specialization Spec { get; set; }

        public Archetype Archetype { get; set; }

        public UserCard(string name)
        {
            Name = name;
            Alias = Name;
            UserId = -1;
            Verbose = true;
            RpData = new RoleplayData();
            Stats = new StatData();
            CurrentTitle = string.Empty;
            Titles = new List<string>();
            Skills = new List<int>();
            Spec = null;
            Archetype = null;
        }

        public void SetStats(StatData stats)
        {
            Stats = stats;
        }

        public int GetStat(StatTypes type)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }
            return Convert.ToInt32(Math.Floor(Stats.Stats[type]));
        }

        public int GetMultipliedStat(StatTypes type)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }

            int basemult = Spec.Stats.GetStat(type);
            basemult += Archetype.Stats.GetStat(type) - 100;
            double percentMult = basemult * .01f;
            percentMult = Math.Round(percentMult, 2);
            return Convert.ToInt32(Math.Floor(Stats.Stats[type] * percentMult));
        }
    }
}
