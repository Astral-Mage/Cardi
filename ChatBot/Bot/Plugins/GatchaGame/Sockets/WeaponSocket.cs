using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
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

        public override string LevelUp()
        {
            if (SocketRarity == RarityTypes.Thirty)
            {
                return $"Already Max Rarity.";
            }

            SocketRarity = SocketRarity + 1;
            string toReturn = string.Empty;
            int ranVal = 0;

            if (RngGeneration.Rng.Next(100) < 4)
            {
                List<StatTypes> AvailableStats = new List<StatTypes>();
                if (!StatModifiers.ContainsKey(StatTypes.Atk)) AvailableStats.Add(StatTypes.Atk);
                if (!StatModifiers.ContainsKey(StatTypes.Spd)) AvailableStats.Add(StatTypes.Spd);
                if (!StatModifiers.ContainsKey(StatTypes.Dex)) AvailableStats.Add(StatTypes.Dex);
                if (!StatModifiers.ContainsKey(StatTypes.Dmg)) AvailableStats.Add(StatTypes.Dmg);
                if (!StatModifiers.ContainsKey(StatTypes.Int)) AvailableStats.Add(StatTypes.Int);
                if (!StatModifiers.ContainsKey(StatTypes.Crc)) AvailableStats.Add(StatTypes.Crc);
                if (!StatModifiers.ContainsKey(StatTypes.Ats)) AvailableStats.Add(StatTypes.Ats);
                if (!StatModifiers.ContainsKey(StatTypes.Crt)) AvailableStats.Add(StatTypes.Crt);

                ranVal = RngGeneration.Rng.Next(1, 5);
                var tas = AvailableStats[RngGeneration.Rng.Next(AvailableStats.Count)];
                StatModifiers.Add(tas, ranVal);
                toReturn += $"{ranVal} {tas.ToString()}, ";
            }
            else if (SecondaryDamageType == DamageTypes.None && RngGeneration.Rng.Next(100) < 4)
            {
                SecondaryDamageType = (DamageTypes)RngGeneration.Rng.Next(0, Enum.GetNames(typeof(DamageTypes)).Length-1);
                toReturn += $"[b][color={SecondaryDamageType.GetDescription()}]{SecondaryDamageType.ToString()}[/color][/b] attribute, ";
            }

            ranVal = RngGeneration.Rng.Next(5, 10);
            StatModifiers[StatTypes.Dmg] += ranVal;
            toReturn += $"{ranVal} {StatTypes.Dmg.ToString()}, ";

            List<StatTypes> tk = StatModifiers.Keys.ToList();
            ranVal = RngGeneration.Rng.Next(2, 5);
            StatTypes sts = tk[RngGeneration.Rng.Next(0, tk.Count)];
            StatModifiers[sts] += ranVal;
            toReturn += $"{ranVal} {sts.ToString()}";
            return toReturn;
        }

        public WeaponSocket()
        {
            ItemType = EquipmentTypes.Weapon;
            SocketType = SocketTypes.Weapon;
            SecondaryDamageType = DamageTypes.None;
            SocketRaritySymbol = "🗡️";
        }

        public string GetName(bool withOverride)
        {
            string baseName = "";
            if (!Prefix.Equals(EquipmentPrefixes.None))
                baseName += Prefix.GetDescription() + " ";

            baseName += WeaponType.GetDescription();

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

            baseName += WeaponType.GetDescription();

            if (!Suffix.Equals(EquipmentSuffixes.None))
                baseName += " " + Suffix.GetDescription();
            baseName += "";
            return (string.IsNullOrWhiteSpace(NameOverride)) ? baseName : NameOverride;
        }
    }
}