using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards
{
    public class PlayerCard : BaseCard
    {
        public DateTime LastStaUpdate;
        public string ClassDisplayName;
        public string SpeciesDisplayName;
        public string Signature;
        public List<int> CompletedQuests;

        public PlayerCard() : base(Enums.CardTypes.PlayerCard)
        {
            LastStaUpdate = DateTime.Now;
            ClassDisplayName = string.Empty;
            SpeciesDisplayName = string.Empty;
            Signature = string.Empty;
            CompletedQuests = new List<int>();
            LevelScaleValue = 1.6;
        }

        public override int GetStat(StatTypes type, bool includeModifiers = true, bool includeEquipment = true, bool includePassives = true, bool includeLevels = true)
        {
            return base.GetStat(type, includeModifiers, includeEquipment, includePassives, includeLevels);
        }
    }
}
