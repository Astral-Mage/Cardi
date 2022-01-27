using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using System;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    [Serializable]
    public enum UniqueRewards
    {
        None,
        Boon,
    }

    [Serializable]
    public class QuestReward
    {
        public BaseStats Stats { get; set; }
        public UniqueRewards OtherReward;
    }
}
