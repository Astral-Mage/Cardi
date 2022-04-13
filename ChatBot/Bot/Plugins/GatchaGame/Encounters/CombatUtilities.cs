using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    [Serializable]
    public static class CombatUtilities
    {
        const int BASE_HIT_CHANCE = 80;
        const int BASE_LVL_DIFF_MULTIPLIER = 6;
        const int BASE_LVL_DIFF_SPREAD = 6;


        public static Dictionary<RawDamageType, double> SplitDamageIntoRawTypes(double baseDmg, EncounterCard card)
        {
            Dictionary<RawDamageType, double> toReturn = new Dictionary<RawDamageType, double>();

            // break down weapon types
            if (card.Participant.ActiveSockets.Any(x => x.SocketType == SocketTypes.Weapon))
            {
                WeaponSocket ws = card.Participant.ActiveSockets.First(x => x.SocketType == SocketTypes.Weapon) as WeaponSocket;
                toReturn.Add(ws.DamageType <= 0 ? RawDamageType.Physical : RawDamageType.Magical, 0);

                if (ws.SecondaryDamageType != DamageTypes.None)
                {
                    var dtype = ws.SecondaryDamageType <= 0 ? RawDamageType.Physical : RawDamageType.Magical;
                    if (!toReturn.Any(x => x.Key == dtype))
                        toReturn.Add(dtype, 0);
                }
            }
            // no weapon, base dmg to higher of int or dex
            else
            {
                toReturn.Add(card.Participant.GetStat(StatTypes.Int) > card.Participant.GetStat(StatTypes.Dex) ? RawDamageType.Magical : RawDamageType.Physical, 0);
            }

            // apply damage
            var keyList = toReturn.Keys.ToList();
            int numDiv = keyList.Count;
            for (int x = 0; x < keyList.Count; x++)
            {
                toReturn[keyList[x]] = baseDmg / numDiv;
            }

            return toReturn;
        }

        public static Dictionary<RawDamageType, double> CalculateDamageReductionMult(EncounterCard card)
        {
            Dictionary<RawDamageType, double> toReturn = new Dictionary<RawDamageType, double>
            {

                // add base 20%
                [RawDamageType.Magical] = 0.15,
                [RawDamageType.Physical] = 0.15
            };

            // add resiliance boon
            if (card.Participant.BoonsEarned.Contains(BoonTypes.Sharpness))
            {
                toReturn[RawDamageType.Magical] += .05;
                toReturn[RawDamageType.Physical] += .05;
            }

            // add phys and magic defense stats
            const int STAT_CLAMP = 2000;
            double statClamp = .4 / STAT_CLAMP;

            double mdf = statClamp * card.Participant.GetStat(StatTypes.Mdf);
            double pdf = statClamp * card.Participant.GetStat(StatTypes.Pdf);
            if (mdf > STAT_CLAMP) mdf = STAT_CLAMP;
            if (pdf > STAT_CLAMP) pdf = STAT_CLAMP;
            toReturn[RawDamageType.Magical] += mdf;
            toReturn[RawDamageType.Physical] += pdf;

            return toReturn;
        }

        public static bool CalculateHits(double hitChance, double evasionChance)
        {
            return RngGeneration.Rng.Next(0, 101) < (hitChance - evasionChance);
        }

        public static bool CalculateCrits(double critChance)
        {
            return RngGeneration.Rng.Next(0, 101) < critChance;
        }

        public static Dictionary<RawDamageType, double> CalculateFlatDamageReductionByType(EncounterCard card)
        {
            Dictionary<RawDamageType, double> toReturn = new Dictionary<RawDamageType, double>
            {
                [RawDamageType.Magical] = card.Participant.GetStat(StatTypes.Con) * 0.3,
                [RawDamageType.Physical] = card.Participant.GetStat(StatTypes.Con) * 0.3
            };

            return toReturn;
        }

        public static double CalculateEvasionChance(EncounterCard card)
        {
            return Math.Sqrt(card.Participant.GetStat(StatTypes.Eva));
        }

        public static double CalculateAddedCriticalDamage(double baseDmg, double critMult)
        {
            return (baseDmg * critMult) - baseDmg;
        }

        public static double CalculateTotalCombinedDamage(Dictionary<RawDamageType, double> baseDmg, Dictionary<RawDamageType, double> dmgRedMult, Dictionary<RawDamageType, double> flatDamageReduction, bool minOne = true)
        {
            double tcd = 0;
            foreach (var v in baseDmg)
            {
                tcd += v.Value;
                tcd -= flatDamageReduction[v.Key];
                tcd *= 1 - dmgRedMult[v.Key];
                if (minOne && tcd < 1)
                    tcd = 1;
            }
            return tcd;
        }

        public static Dictionary<RawDamageType, double> CalculateTotalDamagePerType(Dictionary<RawDamageType, double> baseDmg, Dictionary<RawDamageType, double> damageMult)
        {
            Dictionary<RawDamageType, double> toReturn = new Dictionary<RawDamageType, double>();

            foreach (var v in baseDmg)
                toReturn[v.Key] = v.Value * damageMult[v.Key];

            return toReturn;
        }

        public static Dictionary<RawDamageType, double> CaculateTotalDamageMult(EncounterCard card, int enemyLvl)
        {
            // init
            Dictionary<RawDamageType, double> toReturn = new Dictionary<RawDamageType, double>
            {
                // add base 1
                [RawDamageType.Magical] = 1.0,
                [RawDamageType.Physical] = 1.0
            };

            // add mult for subtype
            if (card.Participant.ActiveSockets.Any(x => x.SocketType == SocketTypes.Weapon))
            {
                WeaponSocket ws = card.Participant.ActiveSockets.First(x => x.SocketType == SocketTypes.Weapon) as WeaponSocket;

                if (ws.SecondaryDamageType != DamageTypes.None)
                {
                    toReturn[RawDamageType.Magical] += .1;
                    toReturn[RawDamageType.Physical] += .1;
                }
            }

            // add level wobble
            double toAdd = .01 * BASE_LVL_DIFF_MULTIPLIER * CalculateLevelWobble(card.Participant.GetStat(StatTypes.Lvl), enemyLvl, BASE_LVL_DIFF_SPREAD);
            toReturn[RawDamageType.Magical] += toAdd;
            toReturn[RawDamageType.Physical] += toAdd;

            // add sharpness boon
            if (card.Participant.BoonsEarned.Contains(BoonTypes.Sharpness))
            {
                toReturn[RawDamageType.Magical] += .05;
                toReturn[RawDamageType.Physical] += .05;
            }

            // add dex and int
            double magicMult = .07 * Math.Sqrt(.1 * card.Participant.GetStat(StatTypes.Int));
            double physMult = .07 * Math.Sqrt(.1 * card.Participant.GetStat(StatTypes.Dex));
            toReturn[RawDamageType.Magical] += magicMult;
            toReturn[RawDamageType.Physical] += physMult;

            return toReturn;
        }

        static double CalculateLevelWobble(int attackerLvl, int defenderLvl, int maxdiff)
        {
            // calc wobble
            double diff = attackerLvl - defenderLvl;

            // clamp values to max
            if (diff < -maxdiff) diff = -maxdiff;
            if (diff > maxdiff) diff = maxdiff;

            return diff;
        }

        public static double CalculateTotalAddedDamage(EncounterCard card)
        {
            double toReturn = 0.0;

            return toReturn;
        }

        public static double CalculateBaseDamage(EncounterCard card)
        {
            return card.Participant.GetStat(StatTypes.Dmg);
        }

        public static List<EncounterCard> GetAvailableEnemies(int pteam, List<EncounterCard> participants)
        {
            List<EncounterCard> availableEnemies = new List<EncounterCard>();
            foreach (var v in participants)
            {
                if (v.Team != pteam &&
                    v.Participant.CurrentVitality > 0 &&
                    v.Participant.Status != CharacterStatusTypes.Dead &&
                    v.Participant.Status != CharacterStatusTypes.Undefined)
                {
                    availableEnemies.Add(v);
                }
            }

            if (availableEnemies.Count < 1)
                return null;

            return availableEnemies;
        }

        public static EncounterCard SelectRandomAvailableEnemy(int pteam, List<EncounterCard> participants)
        {
            List<EncounterCard> ae = GetAvailableEnemies(pteam, participants);
            if (ae == null || ae.Count < 1)
                return null;

            return ae[RngGeneration.Rng.Next(0, ae.Count)];
        }

        public static List<EncounterCard> OrderBySpeed(List<EncounterCard> participants)
        {
            return participants.OrderByDescending(x => x.Participant.GetStat(Enums.StatTypes.Spd)).ToList();
        }

        public static double CalculateHitChance(EncounterCard card)
        {
            return BASE_HIT_CHANCE + (int)(.8 * Math.Sqrt(card.Participant.GetStat(StatTypes.Atk)));
        }

        public static double CalculateCriticalMultiplier(EncounterCard card)
        {
            // init and set base crit mult to 20%
            double toReturn = .2;

            toReturn += .01 * (0.1 * card.Participant.GetStat(StatTypes.Crt));
            return 1 + toReturn;
        }

        public static double CalculateCriticalChance(EncounterCard card)
        {
            return Math.Sqrt(.5 * card.Participant.GetStat(StatTypes.Crc));
        }

        public static int CalculateRandomizedSpread(double baseDmg, double spreadValue)
        {
            int spread = 10 - (int)Math.Sqrt(.3 * spreadValue);
            if (spread < 0)
                spread = 0;

            double lowRoll = baseDmg - (baseDmg * (spread * .01));

            if (lowRoll < 0)
                lowRoll = 0;

            double highRoll = baseDmg;
            return RngGeneration.Rng.Next(Convert.ToInt32(lowRoll), Convert.ToInt32(highRoll));
        }
    }
}
