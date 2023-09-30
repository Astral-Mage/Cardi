using Accord;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects
{
    [Serializable]
    class PassiveSocket : Socket
    {
        public override string GetName()
        {
            string preSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.Last().Key);
            
            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), sufSearch).GetDescription();
            
            return (!string.IsNullOrWhiteSpace(NameOverride)) ? NameOverride : prefix + " " + suffix;
        }

        public PassiveSocket()
        {
            SocketType = SocketTypes.Passive;
            SocketRaritySymbol = "✨";

        }

        public string GetName(bool withOverride)
        {
            string preSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.Last().Key);
            
            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), sufSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride)) ? $"{NameOverride}" : $"[color=white]{prefix} {suffix}[/color]";
        }

        public override string LevelUp()
        {

            string toReturn = string.Empty;
            int ranVal = 0;

            if (SocketLevel == MaxLevel)
            {
                return $"Already Max Rarity.";
            }

            SocketLevel = SocketLevel + 1;

            if (RNG.Seed.Next(100) < 4)
            {
                List<StatTypes> AvailableStats = new List<StatTypes>();
                //if (!StatModifiers.ContainsKey(StatTypes.Con)) AvailableStats.Add(StatTypes.Con);
                //if (!StatModifiers.ContainsKey(StatTypes.Spd)) AvailableStats.Add(StatTypes.Spd);
                //if (!StatModifiers.ContainsKey(StatTypes.Dex)) AvailableStats.Add(StatTypes.Dex);
                //if (!StatModifiers.ContainsKey(StatTypes.Dmg)) AvailableStats.Add(StatTypes.Dmg);
                //if (!StatModifiers.ContainsKey(StatTypes.Int)) AvailableStats.Add(StatTypes.Int);
                //if (!StatModifiers.ContainsKey(StatTypes.Vit)) AvailableStats.Add(StatTypes.Vit);
                //if (!StatModifiers.ContainsKey(StatTypes.Atk)) AvailableStats.Add(StatTypes.Atk);
                //if (!StatModifiers.ContainsKey(StatTypes.Mdf)) AvailableStats.Add(StatTypes.Mdf);
                //if (!StatModifiers.ContainsKey(StatTypes.Pdf)) AvailableStats.Add(StatTypes.Pdf);
                //if (!StatModifiers.ContainsKey(StatTypes.Crc)) AvailableStats.Add(StatTypes.Crc);
                //if (!StatModifiers.ContainsKey(StatTypes.Ats)) AvailableStats.Add(StatTypes.Ats);
                //if (!StatModifiers.ContainsKey(StatTypes.Crt)) AvailableStats.Add(StatTypes.Crt);


                ranVal = RNG.Seed.Next(2, 4);
                var tas = AvailableStats[RNG.Seed.Next(AvailableStats.Count - 1)];
                Stats.Stats.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas.ToString()}, ";
            }

            List<StatTypes> tk = Stats.Stats.Keys.ToList();
            ranVal = RNG.Seed.Next(3, 8);
            StatTypes sts = tk[RNG.Seed.Next(0, tk.Count - 1)];
            Stats.Stats[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}, ";

            tk = Stats.Stats.Keys.ToList();
            ranVal = RNG.Seed.Next(3, 8);
            sts = tk[RNG.Seed.Next(0, tk.Count - 1)];
            Stats.Stats[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}";
            return toReturn;
        }

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]";
            //toReturn += $"[color=pink][Passive][/color]";

            if (Stats.Stats.Count > 0)
            {
                int counter = Stats.Stats.Count;
                toReturn += " ";
                foreach (var v in Stats.Stats)
                {
                    toReturn += $"{v.Key.GetDescription()} ‣ {v.Value}";
                    if (counter > 1)
                    {
                        toReturn += " | ";
                        counter--;
                    }
                }
                toReturn += "[/sup]";
            }

            return toReturn;
        }
    }
}
