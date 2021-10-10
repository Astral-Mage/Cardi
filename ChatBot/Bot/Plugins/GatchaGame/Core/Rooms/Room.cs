using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Core.Rooms;
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
            DateTime now = DateTime.Now;

            // order us for combat
            Dictionary<int, List<BaseCard>> Combatants = new Dictionary<int, List<BaseCard>>();
            Combatants[pc.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>();
            Combatants[pc.GetStat(Enums.StatTypes.Spd)].Add(pc);

            foreach (var enemy in Enemies)
            {
                if (Combatants.ContainsKey(enemy.GetStat(Enums.StatTypes.Spd)))
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)].Add(enemy);
                }
                else
                {
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)] = new List<BaseCard>();
                    Combatants[enemy.GetStat(Enums.StatTypes.Spd)].Add(enemy);
                }
            }
            var ordered = Combatants.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            // room size
            int numEnemies = 1;
            int roomsize = 1 + (int)Math.Sqrt(RngGeneration.Rng.Next(1, 10));
            if (roomsize == 1)
            {
                numEnemies = RngGeneration.Rng.Next(6, 10);
                RoomProgress = RngGeneration.Rng.Next(75, 110);
            }
            else if (roomsize == 2)
            {
                numEnemies = RngGeneration.Rng.Next(4, 7);
                RoomProgress = RngGeneration.Rng.Next(44, 87);
            }
            else
            {
                numEnemies = RngGeneration.Rng.Next(1, 5);
                RoomProgress = RngGeneration.Rng.Next(25, 57);
            }

            // combat
            do
            {
                TurnCounter++;
                responseExtra += $"Round {TurnCounter}:\\n";
                foreach (var combatantList in Combatants)
                {
                    foreach (var combatant in combatantList.Value)
                    {
                        if (pc.CurrentVitality <= 0)
                            break;

                        string ut = (combatant.CardType == Enums.CardTypes.PlayerCard) ? "PC" : "NPC";

                        int critChance = (int)Math.Sqrt(.5 * combatant.GetStat(Enums.StatTypes.Crc));
                        int critDmgMultiplier = (int)(20 + (0.1 * combatant.GetStat(Enums.StatTypes.Crt)));
                        int hitChance = (combatant.CardType == Enums.CardTypes.PlayerCard ? 70 : 65) + (int)(.5 * Math.Sqrt(combatant.GetStat(Enums.StatTypes.Atk)));
                        int baseDmg = (combatant.CardType == Enums.CardTypes.PlayerCard ? 20 : 10) + combatant.GetStat(Enums.StatTypes.Dmg);
                        bool isMagical = false;

                        if (combatant.ActiveSockets.Count(x => x.SocketType == Enums.SocketTypes.Weapon) == 1)
                        {
                            WeaponSocket ws = (WeaponSocket)combatant.ActiveSockets.First(x => x.SocketType == Enums.SocketTypes.Weapon);
                            if (ws.DamageType == Enums.DamageTypes.Physical)
                            {
                                baseDmg = (int)(baseDmg * (1 + (.1 * Math.Sqrt(.2 * combatant.GetStat(Enums.StatTypes.Dex)))));
                            }
                            else
                            {
                                isMagical = true;
                                double multiplier = 1 + (.1 * Math.Sqrt(.1 * combatant.GetStat(Enums.StatTypes.Int)));
                                baseDmg = (int)(baseDmg * multiplier);
                            }
                        }

                        if (Enemies.Count(x => x.CardType != Enums.CardTypes.PlayerCard && x.Status != Enums.CharacterStatusTypes.Dead && x.Status != Enums.CharacterStatusTypes.Vengeful && x.Status != Enums.CharacterStatusTypes.Undefined) <= 0)
                            break;

                        BaseCard currentEnemy;

                        if (combatant.CardType == Enums.CardTypes.PlayerCard)
                        {
                            currentEnemy = Enemies.First(x => x.CardType != Enums.CardTypes.PlayerCard &&
                                    x.Status != Enums.CharacterStatusTypes.Dead &&
                                    x.Status != Enums.CharacterStatusTypes.Vengeful &&
                                    x.Status != Enums.CharacterStatusTypes.Undefined);
                        }
                        else
                        {
                            currentEnemy = pc;
                        }


                        int baseEvasion = 0 + (int)(.5 * Math.Sqrt(currentEnemy.GetStat(Enums.StatTypes.Eva)));
                        int mdf = currentEnemy.GetStat(Enums.StatTypes.Mdf);
                        int pdf = currentEnemy.GetStat(Enums.StatTypes.Pdf);
                        int baseDmgReduction = (int)(isMagical ? (0.3 * pdf) + mdf : (0.3 * mdf) + pdf) + (int)(currentEnemy.GetStat(Enums.StatTypes.Con) * 0.3);

                        // spread
                        int spread = 10 - (int)Math.Sqrt(.3 * (combatant.GetStat(Enums.StatTypes.Ats)));
                        if (spread < 0) spread = 0;

                        // determine dmg roll
                        double lowrollpercent = (.1 - (spread * .001));
                        int lowRoll = baseDmg - (int)((0.1 - lowrollpercent) * baseDmg);
                        if (lowRoll < 0) lowRoll = 0;
                        int highRoll = baseDmg + (int)((0.1 - lowrollpercent) * baseDmg);
                        if (highRoll < 1) highRoll = 1;

                        // roll for total dmg
                        int totalDmg = RngGeneration.Rng.Next(lowRoll, highRoll);

                        // add sharpness boon
                        if (combatant.BoonsEarned.Contains(Enums.BoonTypes.Sharpness)) totalDmg = Convert.ToInt32(totalDmg * 1.05);

                        // add empowerment boon
                        if (combatant.BoonsEarned.Contains(Enums.BoonTypes.Empowerment)) totalDmg = Convert.ToInt32(totalDmg * 1.05);

                        // check for crit and add crit dmg
                        if (RngGeneration.Rng.Next(101) < critChance)
                            totalDmg *= (int)(1 + (.1 * critDmgMultiplier));

                        // try to dodge
                        int val = -1;
                        var tdr = 20 + (.7 * Math.Sqrt(baseDmgReduction));

                        if (RngGeneration.Rng.Next(1, 101) < (hitChance - baseEvasion))
                        {
                            // didn't dodge, apply damage
                            var dmgMult = 1 - (.01 * tdr);
                            val = (int)(totalDmg * dmgMult);

                            // add resiliance boon
                            if (combatant.BoonsEarned.Contains(Enums.BoonTypes.Resiliance) && val > 1) val = Convert.ToInt32(totalDmg * 0.95);

                            // if we do negative damage, set damage to zero for now
                            if (val <= 0) val = 0;

                            currentEnemy.CurrentVitality -= val;

                            // check if the enemy is dead
                            if (currentEnemy.CurrentVitality <= 0)
                            {
                                if (currentEnemy.CardType == Enums.CardTypes.PlayerCard)
                                {
                                    // pc has died
                                    responseExtra += $"(NPC) Dmg Done: {val.ToString()} Deathblow | Spread: {spread} | BaseDmg: {baseDmg} +- {10 - Math.Round(lowrollpercent, 3)}% | Dmg with Crit: {totalDmg} | Hit Chance: {hitChance}% | Magical: {isMagical} | Crit Chance: {critChance}% | Crit Dmg Mult: {100 + critDmgMultiplier}% | Enemy BaseDmgRed: {tdr}% | Enemy MDF: {mdf} | Enemy PDF: {pdf} | Enemy Evasion: {baseEvasion}%";
                                    break;
                                }
                                else
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
                                    responseExtra += $"(PC) Dmg Done: {val.ToString()} Deathblow | Spread: {spread} | BaseDmg: {baseDmg} +- {10 - Math.Round(lowrollpercent, 3)}% | Dmg with Crit: {totalDmg} | Hit Chance: {hitChance}% | Magical: {isMagical} | Crit Chance: {critChance}% | Crit Dmg Mult: {critDmgMultiplier}% | Enemy BaseDmgRed: {tdr}% | Enemy MDF: {mdf} | Enemy PDF: {pdf} | Enemy Evasion: {baseEvasion}%";
                                }
                            }
                            else
                            {
                                responseExtra += $"({ut}) Dmg Done: {val.ToString()} | Spread: {spread} | BaseDmg: {baseDmg} +- {10 - Math.Round(lowrollpercent, 3)}% | Dmg with Crit: {totalDmg} | Hit Chance: {hitChance}% | Magical: {isMagical} | Crit Chance: {critChance}% | Crit Dmg Mult: {100 + critDmgMultiplier}% | Enemy BaseDmgRed: {tdr}% | Enemy MDF: {mdf} | Enemy PDF: {pdf} | Enemy Evasion: {baseEvasion}%";
                            }
                        }
                        else
                        {
                            responseExtra += $"({ut}) Dmg Done: Dodge! | Spread: {spread} | BaseDmg: {baseDmg} +- {10 - Math.Round(lowrollpercent, 3)}% | Dmg with Crit: {totalDmg} | Hit Chance: {hitChance}% | Magical: {isMagical} | Crit Chance: {critChance}% | Crit Dmg Mult: {100 + critDmgMultiplier}% | Enemy BaseDmgRed: {tdr}% | Enemy MDF: {mdf} | Enemy PDF: {pdf} | Enemy Evasion: {baseEvasion}%";
                        }
                        responseExtra += "\\n";
                    }
                }

            } while (pc.CurrentVitality > 0 && Enemies.Count(x => x.Status != Enums.CharacterStatusTypes.Dead && x.Status != Enums.CharacterStatusTypes.Undefined && x.Status != Enums.CharacterStatusTypes.Vengeful) > 0 && DateTime.Now - now < TimeSpan.FromMilliseconds(500));
            rs.TotalRounds = TurnCounter;

            // apply rewards to player card
            foreach (var v in rs.StatRewards)
            {
                pc.AddStat(v.Key, v.Value);
            }

            if (Enemies.Count(x => x.Status != Enums.CharacterStatusTypes.Dead && x.Status != Enums.CharacterStatusTypes.Undefined && x.Status != Enums.CharacterStatusTypes.Vengeful) <= 0)
                rs.RoomCleared = this;

            return rs;
        }
    }
}