using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Core.Rooms;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    public class Room
    {
        public List<EnemyCard> Enemies;
        public int RoomProgress;
        public int TurnCounter;

        public Room()
        {
            Enemies = new List<EnemyCard>();
            RoomProgress = 0;
            TurnCounter = 0;
        }

        public RoomResults Execute(Cards.PlayerCard pc, FloorCard fc, out string responseExtra)
        {
            RoomResults rs = new RoomResults();
            List<EnemyCard> Enemies = new List<EnemyCard>();
            Enemies = RngGeneration.GenerateEnemyCluster(fc, pc);
            responseExtra = string.Empty;
            responseExtra += $"NumEn: {Enemies.Count}";
            DateTime now = DateTime.Now;

            // order us for combat
            Dictionary<int, List<BaseCard>> Combatants = new Dictionary<int, List<BaseCard>>
            {
                [pc.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>()
            };
            Combatants[pc.GetStat(Enums.StatTypes.Spd)].Add(pc);
            Encounter encounter = new Encounter();
            encounter.AddParticipant(1, pc);

            foreach (var enemy in Enemies)
            {
                encounter.AddParticipant(2, enemy);

                if (Combatants.ContainsKey(enemy.GetStat(Enums.StatTypes.Spd)))
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)].Add(enemy);
                }
                else
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>
                    {
                        enemy
                    };
                }
            }
            var ordered = Combatants.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            // room size
            int roomsize = 1 + (int)Math.Sqrt(RngGeneration.Rng.Next(1, 10));
            if (roomsize == 1)
            {
                RoomProgress = RngGeneration.Rng.Next(75, 110);
            }
            else if (roomsize == 2)
            {
                RoomProgress = RngGeneration.Rng.Next(44, 87);
            }
            else
            {
                RoomProgress = RngGeneration.Rng.Next(25, 57);
            }

            encounter.StartEncounter();

            // combat
            do
            {
                encounter.TakeTurn(out EncounterResults results);
                responseExtra += results.ResponseStr;
                TurnCounter++;

                foreach (var currentEnemy in results.DefeatedParticipants)
                {
                    if (currentEnemy.CardType == Enums.CardTypes.EnemyCard)
                    {
                        // enemy has died
                        (currentEnemy as EnemyCard).Status = Enums.CharacterStatusTypes.Dead;
                        rs.EnemiesDefeated.Add((currentEnemy as EnemyCard));

                        if (!rs.StatRewards.ContainsKey(Enums.StatTypes.Exp))
                            rs.StatRewards.Add(Enums.StatTypes.Exp, currentEnemy.GetStat(Enums.StatTypes.Exp));
                        else
                            rs.StatRewards[Enums.StatTypes.Exp] += currentEnemy.GetStat(Enums.StatTypes.Exp);

                        if (!rs.StatRewards.ContainsKey(Enums.StatTypes.Sds))
                            rs.StatRewards.Add(Enums.StatTypes.Sds, currentEnemy.GetStat(Enums.StatTypes.Sds));
                        else
                            rs.StatRewards[Enums.StatTypes.Sds] += currentEnemy.GetStat(Enums.StatTypes.Sds);

                        if (!rs.StatRewards.ContainsKey(Enums.StatTypes.Gld))
                            rs.StatRewards.Add(Enums.StatTypes.Gld, RngGeneration.Rng.Next(currentEnemy.GetStat(Enums.StatTypes.Gld) + 1));
                        else
                            rs.StatRewards[Enums.StatTypes.Gld] += RngGeneration.Rng.Next(currentEnemy.GetStat(Enums.StatTypes.Gld) + 1);

                        if (!rs.StatRewards.ContainsKey(Enums.StatTypes.Kil))
                            rs.StatRewards.Add(Enums.StatTypes.Kil, 1);
                        else
                            rs.StatRewards[Enums.StatTypes.Kil] += 1;

                        if (currentEnemy.CardType == Enums.CardTypes.BossEnemyCard || currentEnemy.CardType == Enums.CardTypes.LegendaryEnemyCard)
                        {
                            if (!rs.StatRewards.ContainsKey(Enums.StatTypes.KiB))
                                rs.StatRewards.Add(Enums.StatTypes.KiB, 1);
                            else
                                rs.StatRewards[Enums.StatTypes.KiB] += 1;
                        }
                    }
                }

            } while (encounter.EncounterStatus == EncounterStatus.Active);
            rs.TotalRounds = TurnCounter;
            // && DateTime.Now - now < TimeSpan.FromMilliseconds(500)
            // apply rewards to player card
            foreach (var v in rs.StatRewards)
            {
                pc.AddStat(v.Key, v.Value, false, false, false);
            }

            // if we cleared the room fully
            if (Enemies.Count(x => x.Status != Enums.CharacterStatusTypes.Dead && x.Status != Enums.CharacterStatusTypes.Undefined && x.Status != Enums.CharacterStatusTypes.Vengeful) <= 0)
            {
                rs.RoomCleared = this;
                pc.AddStat(Enums.StatTypes.Prg, RoomProgress, false, false, false);
            }

            return rs;
        }
    }
}