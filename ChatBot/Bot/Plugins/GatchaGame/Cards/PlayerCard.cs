using Accord;
using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards
{
    [Serializable]
    public class PlayerCard : BaseCard
    {
        public DateTime LastStaUpdate;
        public string ClassDisplayName;
        public string SpeciesDisplayName;
        public string Signature;
        public List<int> CompletedQuests;
        public bool DisplayPvpInfo;
        public bool Verbose;
        public Dictionary<PlayerActionTimeoutTypes, TimeSpan> BaseCooldowns;
        public Dictionary<LastUsedCooldownType, DateTime> LastTriggeredCds;


        public PlayerCard() : base(CardTypes.PlayerCard)
        {
            LastStaUpdate = DateTime.Now;
            ClassDisplayName = string.Empty;
            SpeciesDisplayName = string.Empty;
            Signature = string.Empty;
            CompletedQuests = new List<int>();
            LevelScaleValue = 1.0;
            DisplayPvpInfo = true;
            Verbose = false;
            LastTriggeredCds = new Dictionary<LastUsedCooldownType, DateTime>
            {
                { LastUsedCooldownType.LastBully,       DateTime.Now - new TimeSpan(10, 0, 0) },
                { LastUsedCooldownType.LastLeaderboard, DateTime.Now - new TimeSpan(10, 0, 0) },
                { LastUsedCooldownType.LastCard,        DateTime.Now - new TimeSpan(10, 0, 0) },
                { LastUsedCooldownType.LastBullied,     DateTime.Now - new TimeSpan(10, 0, 0) },
                { LastUsedCooldownType.LastDive,        DateTime.Now - new TimeSpan(10, 0, 0) },
                { LastUsedCooldownType.LastBullyReq,    DateTime.Now - new TimeSpan(10, 0, 0) }
            };

            BaseCooldowns = new Dictionary<PlayerActionTimeoutTypes, TimeSpan>
            {
                { PlayerActionTimeoutTypes.BulliedSomeoneCooldown,      new TimeSpan(3, 00, 0) },
                { PlayerActionTimeoutTypes.ViewedLeaderboardCooldown,   new TimeSpan(1, 30, 0) },
                { PlayerActionTimeoutTypes.ViewedCardCooldown,          new TimeSpan(0, 30, 0) },
                { PlayerActionTimeoutTypes.HasBeenBulliedCooldown,      new TimeSpan(1, 30, 0) },
                { PlayerActionTimeoutTypes.DiveCooldown,                new TimeSpan(1, 30, 0) },
                { PlayerActionTimeoutTypes.BullyAttemptCooldown,        new TimeSpan(0, 05, 0) },
            };
        }

        public double XpNeededToLevel()
        {
            return Convert.ToInt32((-150 + (300 * Math.Pow(GetStat(StatTypes.Lvl), 1.8)))) - GetStat(StatTypes.Exp);
        }

        public override bool GrantReward(Reward reward)
        {
            switch (reward.RewardType)
            {
                case RewardTypes.Stat:
                    {
                        foreach (var rew in reward.StatRewards)
                        {
                            AddStat(rew.Key, rew.Value);
                        }
                    }
                    break;
                default:
                    throw new Exception("Unhandled Reward type");
            }

            return false;
        }

        public override List<Reward> GetRewards(EncounterTypes encounterType)
        {
            List<Reward> rewards = new List<Reward>();

            switch (encounterType)
            {
                case EncounterTypes.Bully:
                    {
                        int stamax = 15;
                        double sta = Math.Round(RngGeneration.XPMULT * GetStat(StatTypes.Sta), 1);
                        if (sta > stamax)
                            sta = stamax;

                        rewards.Add(new Reward(RewardTypes.Stat, StatTypes.Sta, sta / RngGeneration.XPMULT));
                        rewards.Add(new Reward(RewardTypes.Stat, StatTypes.Bly, 1));
                    }
                    break;
                case EncounterTypes.Submit:
                    {
                        int stamax = 8;
                        double sta = Math.Round(RngGeneration.XPMULT * GetStat(StatTypes.Sta), 1);
                        if (sta > stamax)
                            sta = stamax;

                        rewards.Add(new Reward(RewardTypes.Stat, StatTypes.Sta, sta / RngGeneration.XPMULT));
                        rewards.Add(new Reward(RewardTypes.Stat, StatTypes.Bly, 1));
                    }
                    break;
                default:
                    {
                    }
                    break;
            }

            return rewards;
        }

        public string GetStatsString()
        {
            string cardStr = string.Empty;
            cardStr += $"[sup][b]Stats: [/b]";

            StatTypes focStat = (StatTypes)GetStat(StatTypes.Foc);
            string focString = "[color=green]⨁[/color]";

            //cardStr += $"{StatTypes.Pvr.GetDescription()} ‣ {GetStat(StatTypes.Pvr)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Vit) ? focString : "")}{StatTypes.Vit.GetDescription()} ‣ {GetStat(StatTypes.Vit)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Atk) ? focString : "")}{StatTypes.Atk.GetDescription()} ‣ {GetStat(StatTypes.Atk)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Dmg) ? focString : "")}{StatTypes.Dmg.GetDescription()} ‣ {GetStat(StatTypes.Dmg)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Pdf) ? focString : "")}{StatTypes.Pdf.GetDescription()} ‣ {GetStat(StatTypes.Pdf)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Mdf) ? focString : "")}{StatTypes.Mdf.GetDescription()} ‣ {GetStat(StatTypes.Mdf)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Dex) ? focString : "")}{StatTypes.Dex.GetDescription()} ‣ {GetStat(StatTypes.Dex)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Int) ? focString : "")}{StatTypes.Int.GetDescription()} ‣ {GetStat(StatTypes.Int)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Spd) ? focString : "")}{StatTypes.Spd.GetDescription()} ‣ {GetStat(StatTypes.Spd)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Con) ? focString : "")}{StatTypes.Con.GetDescription()} ‣ {GetStat(StatTypes.Con)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Eva) ? focString : "")}{StatTypes.Eva.GetDescription()} ‣ {GetStat(StatTypes.Eva)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Crc) ? focString : "")}{StatTypes.Crc.GetDescription()} ‣ {GetStat(StatTypes.Crc)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Crt) ? focString : "")}{StatTypes.Crt.GetDescription()} ‣ {GetStat(StatTypes.Crt)}" + " | ";
            cardStr += $"{((focStat == StatTypes.Ats) ? focString : "")}{StatTypes.Ats.GetDescription()} ‣ {GetStat(StatTypes.Ats)}";

            cardStr += "[/sup]";

            return cardStr;
        }

        public override int GetStat(StatTypes type, bool includeModifiers = true, bool includeEquipment = true, bool includePassives = true, bool includeLevels = false)
        {
            return base.GetStat(type, includeModifiers, includeEquipment, includePassives, includeLevels);
        }

        private KeyValuePair<StatTypes, double> LevelUpStat(StatTypes type, double multiplier, bool minone = false, int addlVal = 0)
        {
            StatTypes tkey = type;
            double tval = GetPreciseStat(type, false, false, false) * multiplier;

            if (minone && tval < 1)
                tval = 1;

            tval += addlVal;

            KeyValuePair<StatTypes, double> levelOutput = new KeyValuePair<StatTypes, double>(tkey, tval);
            AddStat(type, tval, false, false, false);
            return levelOutput;
        }

        public override string LevelUp()
        {
            double levelMult = 0;
            double focusMult = 0;
            List<KeyValuePair<StatTypes, double>> levelOutputs = new List<KeyValuePair<StatTypes, double>>();

            //AddStat(StatTypes.Lvl, 1, false, false, false);


            levelOutputs.Add(LevelUpStat(StatTypes.Vit, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Atk, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Dmg, levelMult, true, RngGeneration.Rng.Next(1, 5)));

            levelOutputs.Add(LevelUpStat(StatTypes.Mdf, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Pdf, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Con, levelMult, true, RngGeneration.Rng.Next(1, 5)));

            levelOutputs.Add(LevelUpStat(StatTypes.Spd, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Crt, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Int, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Dex, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Crc, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Ats, levelMult, true, RngGeneration.Rng.Next(1, 5)));
            levelOutputs.Add(LevelUpStat(StatTypes.Eva, levelMult, true, RngGeneration.Rng.Next(1, 5)));

            // focus stats
            StatTypes focStat;
            if (!RngGeneration.GetAllFocusableStats().Contains((StatTypes)GetStat(StatTypes.Foc)))
            {
                focStat = RngGeneration.GetRandomFocusableStat();
            }
            else
            {
                focStat = (StatTypes)GetStat(StatTypes.Foc);
            }
            levelOutputs.Add(LevelUpStat(focStat, focusMult, true, RngGeneration.Rng.Next(3, 6)));

            // vit boost
            levelOutputs.Add(LevelUpStat(StatTypes.Vit, 0, true, RngGeneration.Rng.Next(10, 16)));

            // random bonus stat
            focStat = RngGeneration.GetRandomFocusableStat();
            levelOutputs.Add(LevelUpStat(focStat, focusMult, true, RngGeneration.Rng.Next(2, 6)));

            // compose level-up string
            string levelOutput = string.Empty;
            Dictionary<StatTypes, double> levelOutputsCombined = new Dictionary<StatTypes, double>();

            foreach (var v in levelOutputs)
            {
                if (!levelOutputsCombined.ContainsKey(v.Key))
                {
                    levelOutputsCombined[v.Key] = 0;
                }

                levelOutputsCombined[v.Key] += v.Value;
            }

            foreach (var v in levelOutputsCombined)
            {
                levelOutput += $" {v.Key.ToString()} ‣ {Math.Round(v.Value, 1)} ";

                if (!v.Equals(levelOutputsCombined.Last()))
                {
                    levelOutput += "|";
                }
            }

            return levelOutput;
        }
    }
}
