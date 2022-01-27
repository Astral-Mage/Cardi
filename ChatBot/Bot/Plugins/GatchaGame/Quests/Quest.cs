using ChatBot.Bot.Plugins.GatchaGame.Cards.Stats;
using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    [Serializable]
    public class Quest
    {
        public int QuestId { get; set; }

        public string QuestText { get; set; }

        public int LevelRequirement { get; set; }

        public bool Repeatable { get; set; }

        public List<int> PrerequisiteQuest { get; set; }

        public int DepthRequirement { get; set; }

        public QuestReward Rewards { get; set; }

        public double TriggerChance { get; set; }

        public int[] TriggerFloors { get; set; }

        public bool RewardERR { get; set; }

        public int? BlockedBy { get; set; }

        public string QuestName { get; set; }

        public Quest()
        {
            RewardERR = false;
        }

        public virtual void GrantReward(Cards.PlayerCard pc)
        {
            if (pc.CompletedQuests.Contains(QuestId))
            {
                if (Repeatable == false)
                {
                    RewardERR = true;
                    return;
                }
            }
            else
            {
                pc.CompletedQuests.Add(QuestId);
            }


            foreach (var v in Rewards.Stats.Stats)
            {
                if (Rewards.Stats.GetStat(v.Key) != 0)
                    pc.AddStat(v.Key, v.Value, false, false, false);

            }
        }
    }
}