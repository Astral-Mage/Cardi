namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    public class Quest
    {
        public int QuestId { get; set; }

        public string QuestText { get; set; }

        public int LevelRequirement { get; set; }

        public bool Repeatable { get; set; }

        public int? PrerequisiteQuest { get; set; }

        public int DepthRequirement { get; set; }

        public QuestReward Rewards { get; set; }

        public double TriggerChance { get; set; }

        public int[] TriggerFloors { get; set; }

        public bool RewardERR { get; set; }

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

            if (Rewards.Gold != 0) pc.SetStat(Enums.StatTypes.Gld, Rewards.Gold);
            if (Rewards.Experience != 0) pc.AddStat(Enums.StatTypes.Exp, Rewards.Experience);
            //if (Rewards.Progress != 0) pc.AddStat(Enums.StatTypes.Prg, Rewards.Progress);

            if (Rewards.MonsterKills != 0) pc.AddStat(Enums.StatTypes.Kil, Rewards.MonsterKills);
            if (Rewards.BossKills != 0) pc.AddStat(Enums.StatTypes.KiB, Rewards.BossKills);

            if (Rewards.Stamina != 0) pc.AddStat(Enums.StatTypes.Sta, Rewards.Stamina);
        }
    }
}