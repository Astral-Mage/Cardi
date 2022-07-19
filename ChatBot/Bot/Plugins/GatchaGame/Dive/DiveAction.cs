using ChatBot.Bot.Plugins.GatchaGame.Cards.Floor;
using ChatBot.Bot.Plugins.GatchaGame.Data;
using ChatBot.Bot.Plugins.GatchaGame.Dive.Results;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Quests;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public partial class GatchaGame : PluginBase
    {
        int REQUIRED_DIVE_STAMINA = 1;
        //int BaseStaminaToDive = Convert.ToInt32(new TimeSpan(0, REQUIRED_DIVE_STAMINA, 0).TotalSeconds);

        public void DiveAction(string channel, string user, string message)
        {
            if (!Data.DataDb.UserExists(user))
                return;

            if (!RngGeneration.TryGetCard(user, out Cards.PlayerCard pc))
                return;

            //int artificalmax = 90;
            //double pants = 90.0 / pc.GetStat(StatTypes.StM);
            double whatever = XPMULT * pc.GetStat(StatTypes.Sta);

            int curSta = (pc.GetStat(StatTypes.Sta));
            if ( curSta < Convert.ToInt32(new TimeSpan(0, REQUIRED_DIVE_STAMINA, 0).TotalSeconds))
            {
                SystemController.Instance.Respond(channel, $"You need {Math.Round(whatever, 0)}/{Convert.ToInt32(XPMULT * new TimeSpan(0, REQUIRED_DIVE_STAMINA, 0).TotalSeconds)} stamina to dive.", user);
                return;
            }

            int newStam = pc.GetStat(Enums.StatTypes.Sta) - Convert.ToInt32(new TimeSpan(0, REQUIRED_DIVE_STAMINA, 0).TotalSeconds);
            if (newStam < 0)
                newStam = 0;

            // reduce the player's stamina
            pc.SetStat(Enums.StatTypes.Sta, newStam);

            var floorData = DataDb.GetAllFloors();
            int floorChoice = pc.GetStat(StatTypes.Dff);
            int maxFloors = RngGeneration.Rng.Next(3, 14);
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
            pc.CurrentVitality = pc.GetStat(StatTypes.Vit);

            if (44 < floorChoice) floorChoice = floorChoice = 44;

            diveResults.FloorCard = floorData.First(x => x.floor.Equals(floorChoice));
            string allResponseData = string.Empty;
            int roomCount = 0;
            do
            {
                Room room = new Room();
                diveResults.CombinedRoomResults.Add(room.Execute(pc, diveResults.FloorCard, out string responseData));
                numFloors++;

                if (room.Enemies.Count(x => x.Status == CharacterStatusTypes.Alive) <= 0)
                    diveResults.ClearedFloors++;
                
                allResponseData += "\\n" + $"Room {roomCount++}:\\n" + responseData ?? string.Empty;

            } while (pc.CurrentVitality > 0 && /*DateTime.Now - now < TimeSpan.FromSeconds(1) &&*/ numFloors <= maxFloors);
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

            Dictionary<StatTypes, double> AllStatRewards = new Dictionary<StatTypes, double>();
            int totalRounds = 0;

            foreach (var v in unpackedResults)
            {
                totalRounds += v.EncounterResults.TotalRounds;
                foreach (var y in v.EncounterResults.AllRewards)
                {
                    foreach (var z in y.Value)
                    {
                        if (z.RewardType != RewardTypes.Stat)
                            continue;

                        if (AllStatRewards.ContainsKey(z.StatRewards.First().Key))
                            AllStatRewards[z.StatRewards.First().Key] += z.StatRewards.First().Value;
                        else
                            AllStatRewards[z.StatRewards.First().Key] = z.StatRewards.First().Value;
                    }
                }
            }

            int totalKills = AllStatRewards.ContainsKey(StatTypes.Kil) ? Convert.ToInt32(AllStatRewards[StatTypes.Kil]) : 0;
            int totalGoldGained = AllStatRewards.ContainsKey(StatTypes.Gld) ? Convert.ToInt32(AllStatRewards[StatTypes.Gld]) : 0;
            int totalStardustGained = AllStatRewards.ContainsKey(StatTypes.Sds) ? Convert.ToInt32(AllStatRewards[StatTypes.Sds]) : 0;
            int totalExpGained = AllStatRewards.ContainsKey(StatTypes.Exp) ? Convert.ToInt32(AllStatRewards[StatTypes.Exp]) : 0;
            int totalProgressMade = AllStatRewards.ContainsKey(StatTypes.Prg) ? Convert.ToInt32(AllStatRewards[StatTypes.Prg]) : 0;
            int totalLevelUps = AllStatRewards.ContainsKey(StatTypes.Lvl) ? Convert.ToInt32(AllStatRewards[StatTypes.Lvl]) : 0;
            int totalRoomsCleared = results.ClearedFloors;

            foreach(var v in unpackedResults)
            {
                totalKills += v.EnemiesDefeated.Count;
                if (v.StatRewards.ContainsKey(StatTypes.Gld)) totalGoldGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Gld).Value;
                if (v.StatRewards.ContainsKey(StatTypes.Sds)) totalStardustGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Sds).Value;
                if (v.StatRewards.ContainsKey(StatTypes.Exp)) totalExpGained += v.StatRewards.First(x => x.Key == Enums.StatTypes.Exp).Value;

                totalRounds += v.TotalRounds;
                totalProgressMade += v.RoomCleared.RoomProgress;
            }

            if (hitQuests.Count > 0)
            {
                foreach (var quest in hitQuests)
                {
                    if (quest.RewardERR == true)
                        continue;

                    foreach (var v in quest.Rewards.Stats.Stats)
                        if (quest.Rewards.Stats.GetStat(v.Key) != 0) totalGoldGained += quest.Rewards.Stats.GetStat(v.Key);
                }
            }

            string replyString = string.Empty;
            var allblurbs = Data.DataDb.GetBlurbs(Enums.BlurbTypes.Defeated);
            string defeatedBlurb = allblurbs[RngGeneration.Rng.Next(allblurbs.Count)];

            defeatedBlurb = defeatedBlurb.Replace("{enemies}", $"[color=red]{totalKills}[/color]");

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

            replyString += $"{pc.DisplayName}, you entered [b]{totalRoomsCleared}[/b] room(s) on floor [b]{results.FloorCard.floor}[/b]: {results.FloorCard.name}. {defeatedBlurb}.";

            if (hitQuests.Count > 0)
            {
                foreach (var quest in hitQuests)
                {
                    if (quest.RewardERR == true)
                        continue;
                    string QUEST_TEXT_COLOR = "yellow";
                    replyString += $" [color={QUEST_TEXT_COLOR}]{quest.QuestText}[/color]".Replace("{boontype}", $"{((quest.Rewards.OtherReward == UniqueRewards.Boon) ? (quest as BoonQuest).BoonType.ToString() : "")}");

                    foreach (var v in quest.Rewards.Stats.Stats)
                    {
                        replyString = replyString.Replace("{" + $"{v.Key.ToString().ToLowerInvariant()}" + "}", v.Value.ToString());
                    }
                }
            }

            replyString += "[b]";
            replyString += " You gained " +
              $"[color=yellow][b]{totalGoldGained}[/b][/color] gold, gained [color=purple][b]{totalStardustGained}[/b][/color] stardust, gained [color=green][b]{totalExpGained}[/b][/color] experience, contributed [color=blue][b]{totalProgressMade}[/b][/color] progress,";


            replyString += $" and lasted [b][color=red]{totalRounds}[/color][/b] total rounds of combat.";

            for(int x = 0; x < totalLevelUps; x++)
            {
                replyString += " Level up!: " + pc.LevelUp();
                if (x < totalLevelUps - 1)
                    replyString += " | ";
            }

            if (includeFullClearBonus)
            {
                int bonus = RngGeneration.Rng.Next(6, 14);
                int toAdd = 1;
                int lvlDiff = (pc.GetStat(Enums.StatTypes.Lvl) - totalLevelUps) - results.FloorCard.floor;
                if (lvlDiff > 5)
                {
                    if (bonus * .2 < 1)
                        toAdd = 1;
                    if (bonus * .2 > 2)
                        toAdd = 2;
                    pc.AddStat(Enums.StatTypes.Sds, toAdd, false, false, false);
                }
                else if (lvlDiff > 2)
                {
                    pc.AddStat(Enums.StatTypes.Sds, (int)(bonus * .5), false, false, false);
                    toAdd = (int)(bonus * .5);
                }
                else if (lvlDiff < -2)
                {
                    pc.AddStat(Enums.StatTypes.Sds, (int)(bonus * 1.5), false, false, false);
                    toAdd = (int)(bonus * 1.5);
                }
                else
                {
                    pc.AddStat(Enums.StatTypes.Sds, bonus, false, false, false);
                    toAdd = bonus;
                }
                replyString += $" You fully cleared this wing and found [color=purple][b]{toAdd}[/b][/color] extra stardust along the way. ";
            }

            replyString += "[/b]";

            if (pc.Verbose)
            {
                if (string.IsNullOrWhiteSpace(channel))
                {
                    extraResponseData = string.Empty;

                    for (int eachRoom = 0; eachRoom < unpackedResults.Count; eachRoom++)
                    {
                        extraResponseData += $"\\nRoom {eachRoom + 1} : ";
                        for (int eachTurn = 0; eachTurn < unpackedResults[eachRoom].EncounterResults._Turns.Count; eachTurn++)
                        {
                            Turn ct = unpackedResults[eachRoom].EncounterResults._Turns[eachTurn];
                            TurnResult ctr = ct._TurnResult;
                            extraResponseData += $"Turn {eachTurn + 1} | {ct._TurnResult._LivingParticipants} Living Participants\\n";

                            for (int eachRotation = 0; eachRotation < ctr._Rotations.Count; eachRotation++)
                            {
                                Rotation cr = ctr._Rotations[eachRotation];

                                EncounterCard atk = ct.Participants.First(x => x.Participant.Name.Equals(cr._Attacker, StringComparison.InvariantCultureIgnoreCase));
                                EncounterCard dfd = ct.Participants.First(x => x.Participant.Name.Equals(cr._Defender, StringComparison.InvariantCultureIgnoreCase));

                                extraResponseData += $"({cr._AtkStartingVit}) {atk.Participant.DisplayName}";
                                extraResponseData += $" vs ({cr._DefStartingVit}) {dfd.Participant.DisplayName}";

                                extraResponseData += $" | Hit: {cr._HitChance}";
                                extraResponseData += $" | CritChc: {cr._CritChance}";

                                if (cr._HitConnected)
                                {
                                    if (cr._CritConnected)
                                    {
                                        extraResponseData += $" | Crit!";
                                        extraResponseData += $" | CritMult: {Math.Round(cr._CritMult, 2)}";
                                        extraResponseData += $" | AddCritDmg: {Math.Round(cr._CritDamage, 2)}";
                                    }
                                    else
                                    {
                                        extraResponseData += $" | Hit!";
                                    }

                                    extraResponseData += $" | BaseDmg: {cr._BaseDamage}";
                                    extraResponseData += $" | AddDmg: {cr._AddedDmg}";

                                    extraResponseData += $" | ByType(";
                                    foreach (var byType in cr._DamageByType.Keys)
                                    {
                                        extraResponseData += $" [color={(byType == RawDamageType.Magical ? "pink" : "brown")}]{byType} ⨠ ";
                                        extraResponseData += $" Dmg: {cr._DamageByType[byType]}";
                                        extraResponseData += $" | Mult: {Math.Round(cr._DmgMultByType[byType], 2)}";
                                        extraResponseData += $" | Ttl: {Math.Round(cr._TotalDamageByType[byType], 2)}";
                                        extraResponseData += $" | FlatDR: {Math.Round(cr._FlatDamageReductionByType[byType], 2)}";
                                        extraResponseData += $" | DRMult: {Math.Round(cr._DamageReductionMultByType[byType], 2)}[/color]";
                                    }
                                    extraResponseData += $" )";

                                    extraResponseData += $" | BaseTtlDmg: {Math.Round(cr._TotalCombinedDamage, 2)}";
                                    extraResponseData += $" | TotalOverallDamageDealt: {Math.Round(cr._TotalOverallDamageDealt, 2)}";
                                }
                                else
                                {
                                    extraResponseData += $" | Evade!";
                                }
                                extraResponseData += "\\n";
                            }
                            extraResponseData += "\\n";
                        }
                    }

                    replyString += " [sub]" + extraResponseData + "[/sub]";
                }
            }


            Data.DataDb.UpdateCard(pc);

            // add progress to highest current floor
            var allFloors = DataDb.GetAllFloors();
            allFloors.Sort((x, y) => x.floor.CompareTo(y.floor));
            FloorCard hfc = allFloors.First(x => x.currentxp < x.neededxp);

            if (hfc != null)
            {
                hfc.currentxp += totalProgressMade;
                DataDb.UpdateFloor(hfc);

                if (hfc.currentxp >= hfc.neededxp)
                {
                    replyString += $"You've discovered a new floor! ";

                    var tFloor = allFloors.First(x => x.currentxp < x.neededxp);
                    if (hfc == tFloor)
                    {
                        // error
                    }

                    hfc = tFloor;
                    replyString += $"{hfc.name} : {hfc.notes} [sub]Floor: {hfc.floor}[/sub]";
                }
            }

            SystemController.Instance.Respond(channel, replyString, pc.Name);
        }
    }
}