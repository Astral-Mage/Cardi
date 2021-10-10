using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Sockets
{
    [Serializable]
    public abstract class EquipmentSocket : Socket
    {
        public EquipmentTypes ItemType;
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
            toReturn += $"[color=pink][{ItemType.GetDescription()}][/color]";
            return toReturn;
        }
    }
}
