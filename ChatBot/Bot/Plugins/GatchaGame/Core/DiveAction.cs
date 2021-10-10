using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        int BaseStaminaToDive = 45;

        public void DiveAction(string channel, string user, string message)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            int curSta = (pc.GetStat(Enums.StatTypes.Sta) / 1200);
            if ( curSta < BaseStaminaToDive)
            {
                Respond(channel, $"You need [color=red]{curSta}[/color][b]/{BaseStaminaToDive}[/b] stamina to dive.", user);
                return;
            }

            int toAdd = pc.GetStat(Enums.StatTypes.Sta) - (45 * 1200);
            if (toAdd > 108000)
                toAdd = 108000;

            // reduce the player's stamina
            pc.SetStat(Enums.StatTypes.Sta, toAdd);

            var floorData = Data.DataDb.GetAllFloors();
            int floorChoice = pc.GetStat(Enums.StatTypes.Dff);
            int maxFloors = RngGeneration.Rng.Next(3, 8);
            int numFloors = 0;
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (!int.TryParse(message, out int res))
                    return;

                if (floorData.Count < res)
                    res = floorData.Count;
                else if (res <= 0)
                    res = 1;

                floorChoice = res;
            }

            if (floorChoice < 1) floorChoice = 1;

            Core.Rooms.DiveResults diveResults = new Core.Rooms.DiveResults();
            DateTime now = DateTime.Now;

            // start at full hp
            pc.CurrentVitality = pc.GetStat(Enums.StatTypes.Vit);
            diveResults.FloorCard = floorData.First(x => x.floor.Equals(floorChoice));
            string allResponseData = string.Empty;
            int roomCount = 0;
            do
            {
                Room room = new Room();
                diveResults.CombinedRoomResults.Add(room.Execute(pc, diveResults.FloorCard, out string responseData));
                numFloors++;

                if (room.Enemies.Count(x => x.Status == Enums.CharacterStatusTypes.Alive) <= 0 &&
                    room.Enemies.Count(x => x.Status == Enums.CharacterStatusTypes.Smug) <= 0)
                {
                    diveResults.ClearedFloors++;
                }

                
                allResponseData += "\\n" + $"Room {roomCount++}: " + responseData ?? string.Empty;

                // check for level up here
                do
                {
                    if (pc.GetStat(Enums.StatTypes.Exp) > Math.Pow(pc.GetStat(Enums.StatTypes.Lvl), (pc.GetStat(Enums.StatTypes.Lvl) > 15) ? 4.5 : 3.5))
                    {
                        // leveled up
                        diveResults.LevelUps++;
                        pc.AddStat(Enums.StatTypes.Lvl, 1);

                    }
                } while (pc.GetStat(Enums.StatTypes.Exp) > Math.Pow(pc.GetStat(Enums.StatTypes.Lvl), (pc.GetStat(Enums.StatTypes.Lvl) > 15) ? 4.5 : 3.5));

            } while (pc.CurrentVitality > 0 && DateTime.Now - now < TimeSpan.FromSeconds(1) && numFloors <= maxFloors);
            bool includeFullClearBonus = false;
            if (numFloors > maxFloors)
            {
                includeFullClearBonus = true;
            }

            // quest stuff
            List<Quest> hitQuests = QuestManager.RollQuests(numFloors, pc, diveResults.FloorCard);




            // post results
            ParseResultsAndReply(diveResults, channel, pc, hitQuests, includeFullClearBonus, allResponseData);
        }

        public void ParseResultsAndReply(Core.Rooms.DiveResults results, string channel, Cards.PlayerCard pc, List<Quest> hitQuests, bool includeFullClearBonus = false, string extraResponseData = "")
        {
            var unpackedResults = results.CombinedRoomResults;

            int totalKills = 0;
            int totalBossKills = 0;
            int totalGoldGained = 0;
            int totalStardustGained = 0;
            int totalExpGained = 0;
            int totalProgressMade = 0;
            int totalRounds = 0;
            int totalLevelUps = results.LevelUps;
            int totalRoomsCleared = results.ClearedFloors;

            foreach(var v in unpackedResults)
            {
                totalKills += v.EnemiesDefeated.Count;
                if (v.StatRewards.ContainsKey(Enums.StatTypes.Gld)) totalGoldGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Gld).Value;
                if (v.StatRewards.ContainsKey(Enums.StatTypes.Sds)) totalStardustGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Sds).Value;
                if (v.StatRewards.ContainsKey(Enums.StatTypes.Exp)) totalExpGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Exp).Value;

                totalRounds += v.TotalRounds;
                totalProgressMade += v.RoomCleared.RoomProgress;
            }

            if (hitQuests.Count > 0)
            {
                foreach (var quest in hitQuests)
                {
                    if (quest.RewardERR == true)
                        continue;

                    if (quest.Rewards.Gold != 0) totalGoldGained += quest.Rewards.Gold;
                    if (quest.Rewards.Experience != 0) totalExpGained += quest.Rewards.Experience;
                    if (quest.Rewards.MonsterKills != 0) totalKills += quest.Rewards.MonsterKills;
                    if (quest.Rewards.BossKills != 0) totalBossKills += quest.Rewards.BossKills;
                }
            }

            string replyString = string.Empty;
            var allblurbs = Data.DataDb.GetBlurbs(Enums.BlurbTypes.Defeated);
            string defeatedBlurb = allblurbs[RngGeneration.Rng.Next(allblurbs.Count)];

            defeatedBlurb = defeatedBlurb.Replace("{enemies}", $"{totalKills}");

            if (pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Weapon) > 0)
            {
                defeatedBlurb = defeatedBlurb.Replace("{weapon}", $"{pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Weapon).GetName()}");
            }
            else
            {
                defeatedBlurb = defeatedBlurb.Replace("{weapon}", $"Bare Fists");
            }

            if (pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Armor) > 0)
            {
                defeatedBlurb = defeatedBlurb.Replace("{armor}", $"{pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Armor).GetName()}");
            }
            else
            {
                defeatedBlurb = defeatedBlurb.Replace("{armor}", $"Birthday Suit");
            }

            if (pc.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Passive) > 0)
            {
                defeatedBlurb = defeatedBlurb.Replace("{passive}", $"{pc.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Passive).GetName()}");
            }
            else
            {
                defeatedBlurb = defeatedBlurb.Replace("{passive}", $"Bashful Gaze");
            }



            replyString += $"{pc.DisplayName}, you entered {totalRoomsCleared} room(s) on floor {results.FloorCard.floor}: {results.FloorCard.name}. {defeatedBlurb}. You gained " +
                $"{totalGoldGained} gold, gained {totalStardustGained} stardust, gained {totalExpGained} experience, contributed {totalProgressMade} progress,";

            if (totalLevelUps < 1)
            {
                replyString += $" and lasted {totalRounds} total rounds of combat.";
            }
            else
            {
                replyString += $" lasted {totalRounds} total rounds of combat, and leveled up {totalLevelUps} time(s)!";
            }

            if (hitQuests.Count > 0)
            {
                foreach (var quest in hitQuests)
                {
                    if (quest.RewardERR == true)
                        continue;

                    replyString += " " + quest.QuestText
                        .Replace("{gold}", $"{quest.Rewards.Gold}")
                        .Replace("{boontype}", $"{((quest is BoonQuest) ? (quest as BoonQuest).BoonType.ToString() : "")}")
                        .Replace("{gold}", $"{quest.Rewards.Gold}");
                }
            }

            if (includeFullClearBonus)
            {
                int bonus = RngGeneration.Rng.Next(2, 7);
                int toAdd = 1;
                int lvlDiff = (pc.GetStat(Enums.StatTypes.Lvl) - totalLevelUps) - results.FloorCard.floor;
                if (lvlDiff > 5)
                {
                    if (bonus * .2 < 1)
                        toAdd = 1;
                    if (bonus * .2 > 2)
                        toAdd = 2;
                    pc.AddStat(Enums.StatTypes.Sds, toAdd);
                }
                else if (lvlDiff > 2)
                {
                    pc.AddStat(Enums.StatTypes.Sds, (int)(bonus * .5));
                    toAdd = (int)(bonus * .5);
                }
                else if (lvlDiff < -2)
                {
                    pc.AddStat(Enums.StatTypes.Sds, (int)(bonus * 1.5));
                    toAdd = (int)(bonus * 1.5);
                }
                else
                {
                    pc.AddStat(Enums.StatTypes.Sds, bonus);
                    toAdd = bonus;
                }
                replyString += $" You fully cleared this wing and found [color=purple]{toAdd}[/color] stardust along the way.";
            }

            //if (string.IsNullOrWhiteSpace(channel))
            //    replyString += " [sub]" + extraResponseData + "[/sub]";

            Data.DataDb.UpdateCard(pc);
            Respond(channel, replyString, pc.Name);
        }
    }
}