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
    public class WeaponSocket : EquipmentSocket
    {
        public DamageTypes DamageType;
        public DamageTypes SecondaryDamageType;
        public WeaponTypes WeaponType;

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]{base.GetShortDescription()} [color={DamageType.GetDescription()}][{Enum.GetName(typeof(DamageTypes), DamageType)}][/color]";
            if (SecondaryDamageType != DamageTypes.None) toReturn += $"[color={SecondaryDamageType.GetDescription()}][{Enum.GetName(typeof(DamageTypes), SecondaryDamageType)}][/color]";

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

        public override string LevelUp()
        {
            if (SocketLevel == MaxLevel)
            {
                return $"Already Max Rarity.";
            }

            SocketLevel++;
            string toReturn = string.Empty;
            int ranVal;
            if (RNG.Seed.Next(100) < 4)
            {
                List<StatTypes> AvailableStats = new List<StatTypes>();
                //if (!Stats.Stats.ContainsKey(StatTypes.Atk)) AvailableStats.Add(StatTypes.Atk);
                //if (!Stats.Stats.ContainsKey(StatTypes.Spd)) AvailableStats.Add(StatTypes.Spd);
                //if (!Stats.Stats.ContainsKey(StatTypes.Dex)) AvailableStats.Add(StatTypes.Dex);
                //if (!Stats.Stats.ContainsKey(StatTypes.Dmg)) AvailableStats.Add(StatTypes.Dmg);
                //if (!Stats.Stats.ContainsKey(StatTypes.Int)) AvailableStats.Add(StatTypes.Int);
                //if (!Stats.Stats.ContainsKey(StatTypes.Crc)) AvailableStats.Add(StatTypes.Crc);
                //if (!Stats.Stats.ContainsKey(StatTypes.Ats)) AvailableStats.Add(StatTypes.Ats);
                //if (!Stats.Stats.ContainsKey(StatTypes.Crt)) AvailableStats.Add(StatTypes.Crt);

                ranVal = RNG.Seed.Next(1, 5);
                var tas = AvailableStats[RNG.Seed.Next(AvailableStats.Count)];
                Stats.Stats.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas}, ";
            }
            else if (SecondaryDamageType == DamageTypes.None && RNG.Seed.Next(100) < 4)
            {
                SecondaryDamageType = (DamageTypes)RNG.Seed.Next(0, Enum.GetNames(typeof(DamageTypes)).Length - 1);
                toReturn += $"[b][color={SecondaryDamageType.GetDescription()}]{SecondaryDamageType}[/color][/b] attribute, ";
            }

            ranVal = RNG.Seed.Next(5, 10);
            //Stats.Stats[StatTypes.Dmg] += ranVal;
            //toReturn += $"{ranVal} {StatTypes.Dmg}, ";

            List<StatTypes> tk = Stats.Stats.Keys.ToList();
            ranVal = RNG.Seed.Next(2, 5);
            StatTypes sts = tk[RNG.Seed.Next(0, tk.Count)];
            Stats.Stats[sts] += ranVal;
            toReturn += $"{ranVal} {sts}";
            return toReturn;
        }

        public WeaponSocket()
        {
            SocketType = SocketTypes.Weapon;
            SecondaryDamageType = DamageTypes.None;
            SocketRaritySymbol = "🗡️";
        }

        public override string GetName()
        {
            string preSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.First(x => x.Key != StatTypes.Damage && x.Key != StatTypes.DamageType).Key);
            string sufSearch = Enum.GetName(typeof(StatTypes), Stats.Stats.Last(x => x.Key != StatTypes.Damage && x.Key != StatTypes.DamageType).Key);

            string prefix = Enum.Parse(typeof(StatPrefixes), preSearch).GetDescription();
            string suffix = Enum.Parse(typeof(StatSuffixes), sufSearch).GetDescription();

            return (!string.IsNullOrWhiteSpace(NameOverride)) ? $"{NameOverride}" : $"{prefix} {WeaponType.GetDescription()} of {suffix}";
        }
    }
}