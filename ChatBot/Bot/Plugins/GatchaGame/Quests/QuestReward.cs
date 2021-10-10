using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    public enum UniqueRewards
    {
        None,
        Boon,
    }

    public class QuestReward
    {
        public int Gold;
        public int Experience;
        public int Stamina;
        public int Progress;
        public int MonsterKills;
        public int BossKills;
        public UniqueRewards OtherReward;
    }
}
