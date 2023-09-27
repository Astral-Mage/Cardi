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
    public class ArmorSocket : EquipmentSocket
    {
        public ArmorTypes GearType;

        public ArmorSocket()
        {
            SocketType = SocketTypes.Armor;
            SocketRaritySymbol = " 🛡️";
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
                //if (!Stats.Stats.ContainsKey(StatTypes.Con)) AvailableStats.Add(StatTypes.Con);
                //if (!Stats.Stats.ContainsKey(StatTypes.Spd)) AvailableStats.Add(StatTypes.Spd);
                //if (!Stats.Stats.ContainsKey(StatTypes.Dex)) AvailableStats.Add(StatTypes.Dex);
                //if (!Stats.Stats.ContainsKey(StatTypes.Dmg)) AvailableStats.Add(StatTypes.Dmg);
                //if (!Stats.Stats.ContainsKey(StatTypes.Int)) AvailableStats.Add(StatTypes.Int);
                //if (!Stats.Stats.ContainsKey(StatTypes.Vit)) AvailableStats.Add(StatTypes.Vit);
                //if (!Stats.Stats.ContainsKey(StatTypes.Atk)) AvailableStats.Add(StatTypes.Atk);
                //if (!Stats.Stats.ContainsKey(StatTypes.Mdf)) AvailableStats.Add(StatTypes.Mdf);
                //if (!Stats.Stats.ContainsKey(StatTypes.Pdf)) AvailableStats.Add(StatTypes.Pdf);
                //if (!Stats.Stats.ContainsKey(StatTypes.Crc)) AvailableStats.Add(StatTypes.Crc);
                //if (!Stats.Stats.ContainsKey(StatTypes.Ats)) AvailableStats.Add(StatTypes.Ats);
                //if (!Stats.Stats.ContainsKey(StatTypes.Crt)) AvailableStats.Add(StatTypes.Crt);

                ranVal = RNG.Seed.Next(2, 4);
                var tas = AvailableStats[RNG.Seed.Next(AvailableStats.Count)];
                Stats.Stats.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas.ToString()}, ";
            }

            ranVal = RNG.Seed.Next(6, 8);
            //StatTypes tft = RNG.Seed.Next(0, 2) == 0 ? StatTypes.Mdf : StatTypes.Pdf;
            //if (!Stats.Stats.ContainsKey(tft))
            //{
            //    Stats.Stats[tft] = 0;
            //}
            //Stats.Stats[tft] += ranVal;
            //toReturn += $"{ranVal} {tft.ToString()}, ";

            List<StatTypes> tk = Stats.Stats.Keys.ToList();
            ranVal = RNG.Seed.Next(2, 6);
            StatTypes sts = tk[RNG.Seed.Next(0, tk.Count)];
            Stats.Stats[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}";
            return toReturn;
        }

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]{base.GetShortDescription()} ";

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

        public string GetName(bool withOverride)
        {
            string preSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.Last().Key);

            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), sufSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride) || withOverride == true) ? NameOverride : prefix + $" {GearType.GetDescription()} of " + suffix;
        }

        public override string GetName()
        {
            string preSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.First().Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.Last().Key);

            var preNames = Enum.GetNames(typeof(EquipmentPrefixes));
            var sufNames = Enum.GetNames(typeof(EquipmentSuffixes));

            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), sufSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride)) ? NameOverride : prefix + $" {GearType.GetDescription()} of " + suffix;
        }
    }
}
