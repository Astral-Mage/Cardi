﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Quests
{
    /// <summary>
    /// Handles quests
    /// </summary>
    public static class QuestManager
    {
        /// <summary>
        /// Our master list of available quests for players
        /// </summary>
        static List<Quest> MasterQuestBook = new List<Quest>();

        /// <summary>
        /// random value for ... randomization
        /// </summary>
        public static Random Rng = new Random();

        /// <summary>
        /// Our default constructor
        /// </summary>
        static QuestManager()
        {
            MasterQuestBook.Add(new Quest()
            {
                QuestId = 1034,
                TriggerFloors = new int[] { },
                LevelRequirement = 0,
                PrerequisiteQuest = null,
                DepthRequirement = 0,
                Repeatable = true,
                QuestText = "You found a discarded pouch containing [color=yellow][b]{gold}[/b][/color] gold!",
                TriggerChance = 10.0,
                Rewards = new QuestReward()
                { Gold = 12, Experience = 3, MonsterKills = 0, BossKills = 0, Progress = 7, Stamina = 0, OtherReward = UniqueRewards.None }
            });
            MasterQuestBook.Add(new BoonQuest()
            {
                QuestId = 2010,
                TriggerFloors = new int[] { },
                LevelRequirement = 0,
                PrerequisiteQuest = null,
                DepthRequirement = 2,
                Repeatable = false,
                QuestText = "You found a boon of [color=cyan][b]{boontype}[/b][/color]! This boon auguments your offensive slot to increase it's base effectiveness.",
                TriggerChance = 0.1,
                BoonType = Enums.BoonTypes.Sharpness,
                Rewards = new QuestReward()
                { Gold = 0, Experience = 0, MonsterKills = 0, BossKills = 0, Progress = 0, Stamina = 0, OtherReward = UniqueRewards.None }
            });
            MasterQuestBook.Add(new BoonQuest()
            {
                QuestId = 2010,
                TriggerFloors = new int[] { },
                LevelRequirement = 0,
                PrerequisiteQuest = null,
                DepthRequirement = 2,
                Repeatable = false,
                QuestText = "You found a boon of [color=cyan][b]{boontype}[/b][/color]! This boon auguments your defensive slot to increase it's base effectiveness.",
                TriggerChance = 0.1,
                BoonType = Enums.BoonTypes.Resiliance,
                Rewards = new QuestReward()
                { Gold = 0, Experience = 0, MonsterKills = 0, BossKills = 0, Progress = 0, Stamina = 0, OtherReward = UniqueRewards.None }
            }); MasterQuestBook.Add(new BoonQuest()
            {
                QuestId = 2010,
                TriggerFloors = new int[] { },
                LevelRequirement = 0,
                PrerequisiteQuest = null,
                DepthRequirement = 2,
                Repeatable = false,
                QuestText = "You found a boon of [color=cyan][b]{boontype}[/b][/color]! This boon auguments your passive slot to increase it's base effectiveness.",
                TriggerChance = 0.1,
                BoonType = Enums.BoonTypes.Empowerment,
                Rewards = new QuestReward()
                { Gold = 0, Experience = 0, MonsterKills = 0, BossKills = 0, Progress = 0, Stamina = 0, OtherReward = UniqueRewards.None }
            });
        }

        /// <summary>
        /// Handles calculating quests
        /// </summary>
        /// <param name="numFloorsHit">dive progress</param>
        /// <param name="fc">current floor card</param>
        /// <param name="pc">current player card</param>
        public static List<Quest> RollQuests(int numFloorsHit, Cards.PlayerCard pc, FloorCard fc)
        {
            List<Quest> hitQuests = new List<Quest>();

            // compile list of available quests
            List<Quest> availableQuests = MasterQuestBook.Where((x) =>
            {
                if (x.PrerequisiteQuest != null)
                {
                    if (!pc.CompletedQuests.Contains(x.PrerequisiteQuest.Value))
                    {
                        return false;
                    }
                }
                if (x.DepthRequirement <= numFloorsHit && x.LevelRequirement <= pc.GetStat(Enums.StatTypes.Lvl) && 
                (!pc.CompletedQuests.Contains(x.QuestId) || x.Repeatable) )
                {
                    return true;
                }
                return false;
            }).ToList();

            // check to see if any quests are triggered
            for (int i = 0; i < numFloorsHit; i++)
            {
                double roll = Rng.NextDouble() * 100.0;

                List<Quest> tQ = availableQuests.Where(x => x.TriggerChance >= roll && (x.TriggerFloors.Contains(fc.floor) || x.TriggerFloors.Length == 0)).ToList();
                if (tQ.Count <= 0)
                {
                    continue;
                }

                Quest hitQuest = tQ[Rng.Next(0, tQ.Count)];

                // automatically complete quests for now
                if (!hitQuest.Repeatable)
                {
                    availableQuests.Remove(hitQuest);
                }
                hitQuest.GrantReward(pc);
                hitQuests.Add(hitQuest);
            }
            return hitQuests;
        }

        static void TallyQuestByType(List<Quest> quests, FloorCard fc, PlayerCard pc, int progress)
        {

        }
    }
}