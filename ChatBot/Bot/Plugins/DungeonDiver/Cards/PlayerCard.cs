using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// A player card
    /// </summary>
    public class PlayerCard
    {
        public void Initialize()
        {
            gold = 0;
            created = DateTime.Now;
            lastDive = DateTime.Now - new TimeSpan(24, 0, 0);
            species = "Unknown";
            mainClass = "Adventurer";
            level = 1;
            killed = 0;
            weapon = "Heirloom Sword";
            gear = "Adventurer's Belongings";
            special = "Strength of Will";
            weaponlvl = 0;
            gearlvl = 0;
            speciallvl = 0;
            xp = 0;
            weaponperklvl = 0;
            gearperklvl = 0;
            specialperklvl = 0;
            colortheme = "white";
            name = string.Empty;
            signature = string.Empty;
            nickname = string.Empty;
            CompletedQuests = new List<int>();
            // stats

        }

        public int gold { get; set; }
        public string name { get; set; }
        public DateTime created { get; set; }
        public DateTime lastDive { get; set; }
        public string species { get; set; }
        public string mainClass { get; set; }
        public int level { get; set; }
        public int killed { get; set; }
        public string weapon { get; set; }
        public string gear { get; set; }
        public string special { get; set; }
        public int weaponlvl { get; set; }
        public int gearlvl { get; set; }
        public int speciallvl { get; set; }
        public int xp { get; set; }
        public int weaponperklvl { get; set; }
        public int gearperklvl { get; set; }
        public int specialperklvl { get; set; }
        public string colortheme { get; set; }
        public string nickname { get; set; }
        public string signature { get; set; }
        public int MaxStamina { get; set; }
        public int CurrentStamina { get; set; }
        public List<int> CompletedQuests { get; set; }

        public string GetPublicName()
        {
            return string.IsNullOrWhiteSpace(nickname) ? $"{name}" : $"{nickname}";
        }

        public PlayerCard(string name)
        {
            Initialize();
            this.name = name;
        }

        public PlayerCard()
        {
            Initialize();
        }
    }
}
