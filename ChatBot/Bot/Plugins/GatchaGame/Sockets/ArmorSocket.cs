using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;

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
