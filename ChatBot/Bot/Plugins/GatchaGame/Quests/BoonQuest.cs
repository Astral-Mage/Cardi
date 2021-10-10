using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    class BoonQuest : Quest
    {
        public BoonTypes BoonType { get; set; }

        public override void GrantReward(Cards.PlayerCard pc)
        {
            base.GrantReward(pc);

            if (!pc.BoonsEarned.Contains(BoonType))
                pc.BoonsEarned.Add(BoonType);
        }
    }
}
