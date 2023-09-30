using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    [Serializable]
    public class SkillDetails
    {
        public DateTime lastUse { get; set; }
        public int currentCharges { get; set; }

        public SkillDetails()
        {
            lastUse = DateTime.MinValue;
            currentCharges = 1;
        }
    }
}
