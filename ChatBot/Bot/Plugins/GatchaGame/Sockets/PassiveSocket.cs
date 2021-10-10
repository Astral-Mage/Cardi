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

        public override string GetShortDescription()
        {
            string toReturn = string.Empty;
            toReturn += $"[sup]";

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
