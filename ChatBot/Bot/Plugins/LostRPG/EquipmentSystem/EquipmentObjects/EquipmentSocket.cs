using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;

namespace ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects
{
    [Serializable]
    public abstract class EquipmentSocket : Socket
    {
        public EquipmentPrefixes Prefix;
        public EquipmentSuffixes Suffix;
        public int Bonuses;

        public EquipmentSocket()
        {
            Bonuses = 0;
        }

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            return toReturn;
        }
    }
}
