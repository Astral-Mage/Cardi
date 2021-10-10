using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    public class WeaponSocket : EquipmentSocket
    {
        public DamageTypes DamageType;
        public WeaponTypes WeaponType;

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]{base.GetShortDescription()} [color=red][{Enum.GetName(typeof(DamageTypes), DamageType)} Damage][/color]";

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

        public WeaponSocket()
        {
            ItemType = EquipmentTypes.Weapon;
            SocketType = SocketTypes.Weapon;
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