using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    class PassiveSocket : Socket
    {
        public override string GetName()
        {

            string preSearch = Enum.GetName(typeof(StatTypes), StatModifiers.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), StatModifiers.Last().Key);

            var preNames = Enum.GetNames(typeof(StatPrefixes));
            var sufNames = Enum.GetNames(typeof(StatSuffixes));

            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), preSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride)) ? NameOverride : prefix + " " + suffix;
        }

        public PassiveSocket()
        {
            SocketType = SocketTypes.Passive;
            SocketRaritySymbol = "✨";

        }

        public string GetName(bool withOverride)
        {
            string preSearch = Enum.GetName(typeof(StatTypes), StatModifiers.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), StatModifiers.Last().Key);

            var preNames = Enum.GetNames(typeof(StatPrefixes));
            var sufNames = Enum.GetNames(typeof(StatSuffixes));

            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), preSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride) || withOverride  == true) ? NameOverride : prefix + " " + suffix;
        }

        public override string LevelUp()
        {

            string toReturn = string.Empty;
            int ranVal = 0;

            if (SocketRarity == RarityTypes.Thirty)
            {
                return $"Already Max Rarity.";
            }

            SocketRarity = SocketRarity + 1;

            if (RngGeneration.Rng.Next(100) < 4)
            {
                List<StatTypes> AvailableStats = new List<StatTypes>();
                if (!StatModifiers.ContainsKey(StatTypes.Con)) AvailableStats.Add(StatTypes.Con);
                if (!StatModifiers.ContainsKey(StatTypes.Spd)) AvailableStats.Add(StatTypes.Spd);
                if (!StatModifiers.ContainsKey(StatTypes.Dex)) AvailableStats.Add(StatTypes.Dex);
                if (!StatModifiers.ContainsKey(StatTypes.Dmg)) AvailableStats.Add(StatTypes.Dmg);
                if (!StatModifiers.ContainsKey(StatTypes.Int)) AvailableStats.Add(StatTypes.Int);
                if (!StatModifiers.ContainsKey(StatTypes.Vit)) AvailableStats.Add(StatTypes.Vit);
                if (!StatModifiers.ContainsKey(StatTypes.Atk)) AvailableStats.Add(StatTypes.Atk);
                if (!StatModifiers.ContainsKey(StatTypes.Mdf)) AvailableStats.Add(StatTypes.Mdf);
                if (!StatModifiers.ContainsKey(StatTypes.Pdf)) AvailableStats.Add(StatTypes.Pdf);
                if (!StatModifiers.ContainsKey(StatTypes.Crc)) AvailableStats.Add(StatTypes.Crc);
                if (!StatModifiers.ContainsKey(StatTypes.Ats)) AvailableStats.Add(StatTypes.Ats);
                if (!StatModifiers.ContainsKey(StatTypes.Crt)) AvailableStats.Add(StatTypes.Crt);


                ranVal = RngGeneration.Rng.Next(2, 4);
                var tas = AvailableStats[RngGeneration.Rng.Next(AvailableStats.Count - 1)];
                StatModifiers.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas.ToString()}, ";
            }

            List<StatTypes> tk = StatModifiers.Keys.ToList();
            ranVal = RngGeneration.Rng.Next(3, 8);
            StatTypes sts = tk[RngGeneration.Rng.Next(0, tk.Count - 1)];
            StatModifiers[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}, ";

            tk = StatModifiers.Keys.ToList();
            ranVal = RngGeneration.Rng.Next(3, 8);
            sts = tk[RngGeneration.Rng.Next(0, tk.Count - 1)];
            StatModifiers[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}";
            return toReturn;
        }

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]";
            //toReturn += $"[color=pink][Passive][/color]";

            if (StatModifiers.Count > 0)
            {
                int counter = StatModifiers.Count;
                toReturn += " ";
                foreach (var v in StatModifiers)
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
