using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    public class ArmorSocket : EquipmentSocket
    {
        public ArmorTypes GearType;

        public ArmorSocket()
        {
            ItemType = EquipmentTypes.Armor;
            SocketType = SocketTypes.Armor;
            SocketRaritySymbol = " 🛡️";
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
                var tas = AvailableStats[RngGeneration.Rng.Next(AvailableStats.Count)];
                StatModifiers.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas.ToString()}, ";
            }

            ranVal = RngGeneration.Rng.Next(6, 8);
            StatTypes tft = RngGeneration.Rng.Next(0, 2) == 0 ? StatTypes.Mdf : StatTypes.Pdf;
            if (!StatModifiers.ContainsKey(tft))
            {
                StatModifiers[tft] = 0;
            }
            StatModifiers[tft] += ranVal;
            toReturn += $"{ranVal} {tft.ToString()}, ";

            List<StatTypes> tk = StatModifiers.Keys.ToList();
            ranVal = RngGeneration.Rng.Next(2, 6);
            StatTypes sts = tk[RngGeneration.Rng.Next(0, tk.Count)];
            StatModifiers[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}";
            return toReturn;
        }

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]{base.GetShortDescription()} ";

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

        public string GetName(bool withOverride)
        {
            string baseName = "";
            if (!Prefix.Equals(EquipmentPrefixes.None))
                baseName += Prefix.GetDescription() + " ";

            baseName += GearType.GetDescription();

            if (!Suffix.Equals(EquipmentSuffixes.None))
                baseName += " " + Suffix.GetDescription();
            baseName += "";
            return (string.IsNullOrWhiteSpace(NameOverride) || withOverride == false) ? baseName : NameOverride;
        }

        public override string GetName()
        {
            string baseName = "";
            if (!Prefix.Equals(EquipmentPrefixes.None))
                baseName += Prefix.GetDescription() + " ";

            baseName += GearType.GetDescription();

            if (!Suffix.Equals(EquipmentSuffixes.None))
                baseName += " " + Suffix.GetDescription();
            baseName += "";
            return (string.IsNullOrWhiteSpace(NameOverride)) ? baseName : NameOverride;
        }
    }
}
