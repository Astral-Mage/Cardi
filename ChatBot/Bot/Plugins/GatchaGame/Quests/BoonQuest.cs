using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    [Serializable]
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
