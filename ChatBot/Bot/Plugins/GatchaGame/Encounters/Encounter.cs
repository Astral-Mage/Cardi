using ChatApi;
using ChatApi.Objects;
using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using ChatBot.Bot.Plugins.GatchaGame.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{


    public class Encounter
    {
        /// <summary>
        /// 
        /// </summary>
        int turncounter;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, List<BaseCard>> Participants;

        /// <summary>
        /// 
        /// </summary>
        List<EncounterCard> orderedbyturns;

        /// <summary>
        /// 
        /// </summary>
        public EncounterStatus EncounterStatus;

        /// <summary>
        /// 
        /// </summary>
        public string EncounterCode;

        /// <summary>
        /// 
        /// </summary>
        public TimeSpan PrepTimeout;

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreationDate;

        /// <summary>
        /// 
        /// </summary>
        private Timer TheTimer;

        /// <summary>
        /// 
        /// </summary>
        ApiConnection Api;

        /// <summary>
        /// 
        /// </summary>
        Channel theChannel;

        /// <summary>
        /// 
        /// </summary>
        object tLocker;

        /// <summary>
        /// 
        /// </summary>
        public EncounterResults lastEncounterResults;

        /// <summary>
        /// 
        /// </summary>
        public Channel EncounterChannel;

        List<Cards.BaseCard> Winners;

        List<Cards.BaseCard> Losers;

        public Cards.BaseCard Bully;

        public Cards.BaseCard Bullied;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="outres"></param>
        /// <param name="winner"></param>
        /// <param name="loser"></param>
        public delegate void PantsHandler();

        /// <summary>
        /// 
        /// </summary>
        public PantsHandler tPants;

        /// <summary>
        /// 
        /// </summary>
        public int Rounds;

        /// <summary>
        /// 
        /// </summary>
        public void StartASyncEncounter(ApiConnection api, Channel channel)
        {
            if (EncounterStatus == EncounterStatus.Active)
                return;

            Timer enc = new Timer(new TimeSpan(0, 1, 0).TotalMilliseconds)
            {
                AutoReset = true
            };

            enc.Elapsed += Enc_Elapsed;
            enc.Enabled = true;
            TheTimer = enc;
            Api = api;
            theChannel = channel;
            Rounds = 0;
            turncounter = 0;
            StartEncounter();
            tPants = TPants;
            tLocker = new object();
            lastEncounterResults = new EncounterResults();
            EncounterChannel = channel;
            Winners = new List<Cards.BaseCard>();
            Losers = new List<Cards.BaseCard>();

            TakeShot();
        }

        public void TPants()
        {
            if (TheTimer.Enabled)
            {
                TheTimer.Stop();
                EndFightLogic();
            }
        }

        public List<Cards.BaseCard> GetAllParticipants()
        {
            List<BaseCard> toSend = new List<BaseCard>();
            foreach (var v in Participants)
            {
                foreach (var x in v.Value)
                {
                    toSend.Add(x);
                }
            }

            return toSend;
        }

        private void TakeShot()
        {
            EncounterResults lastRes;

            lock (tLocker)
            {
                StartEncounter();
                Rounds++;
                if (Rounds < 4)
                {
                    string outRes = string.Empty;
                    do
                    {
                        TakeTurn(out lastRes);
                        outRes += lastRes.ResponseStr;
                    } while (EncounterStatus == EncounterStatus.Active);

                    if (lastRes.Winner != null && lastRes.Loser != null)
                    {
                        Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"{lastRes.Winner.DisplayName} won round {Rounds} against {lastRes.Loser.DisplayName} over a fight that had {turncounter} turns!", string.Empty);
                    }
                    else
                    {
                        var allP = GetAllParticipants();
                        BaseCard u1 = allP.First();
                        BaseCard u2 = allP.First(x => !x.Equals(u1));

                        Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, 
                            $"{u1.DisplayName} tied against {u2.DisplayName} over {turncounter} turns!"// + 
                            //$"\\n{u1.DisplayName} {(u1 as Cards.PlayerCard).GetStatsString()}" +
                            //$"\\n{u2.DisplayName} {(u2 as Cards.PlayerCard).GetStatsString()}"
                            , string.Empty);
                    }

                    lastEncounterResults = lastRes;
                }

                if (Rounds == 3)
                {
                    tPants?.Invoke();
                }
                else if (Rounds >= 4)
                {
                    lock (tLocker)
                    {
                        TheTimer.Stop();
                        EndFightLogic();
                    }
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Enc_Elapsed(object sender, ElapsedEventArgs e)
        {
            TakeShot();
        }

        public void EndFightLogic()
        {
            if (Winners == null || Winners.Count < 1 || (Winners.Count == 2) && !Winners.First().Name.Equals(Winners.Last().Name))
            {
                Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"The fight is over, somehow ending in a tie!", string.Empty);
                return;
            }

            List<Cards.BaseCard> p1 = Winners.Where(x => x.Name.Equals(Winners.First().Name)).ToList();
            double middle = Rounds * 0.5;

            Cards.BaseCard overallWinner = (p1.Count > middle) ? p1.First() : Losers.First(x => !x.Name.Equals(p1.First().Name));
            Cards.BaseCard overallLoser = p1.First() == overallWinner ? Losers.First(x => !x.Name.Equals(p1.First().Name)) : p1.First();

            // submit calc
            string submitStr = string.Empty;
            TimeSpan submitVal = new TimeSpan(0, 40, 0);

            int staLost;
            if (overallLoser.GetStat(StatTypes.Sta) > submitVal.TotalSeconds)
            {
                overallLoser.AddStat(StatTypes.Sta, -(submitVal.TotalSeconds));
                staLost = Convert.ToInt32(submitVal.TotalSeconds);
            }
            else
            {
                staLost = overallLoser.GetStat(StatTypes.Sta);
                overallLoser.SetStat(StatTypes.Sta, 0);
            }

            //double pants = 90.0 / ucard.GetStat(StatTypes.StM);
            double whatever = RngGeneration.XPMULT * staLost;

            submitStr += $"{overallLoser.DisplayName}, you submit to {overallWinner.DisplayName}'s aggressive bullying, losing {whatever} stamina. ";

            // bully calc
            int staWon;
            if (overallWinner.GetStat(StatTypes.Sta) + staLost >= overallWinner.GetStat(StatTypes.StM))
            {
                staWon = Convert.ToInt32(overallWinner.GetStat(StatTypes.StM) - overallWinner.GetStat(StatTypes.Sta));
                overallWinner.SetStat(StatTypes.Sta, overallWinner.GetStat(StatTypes.StM));
            }
            else
            {
                staWon = staLost;
                overallWinner.AddStat(StatTypes.Sta, staLost);
            }

            //pants = 90.0 / ucard.GetStat(StatTypes.StM);
            double whatever2 = RngGeneration.XPMULT * staWon;

            submitStr += $"{overallWinner.DisplayName}, you gain {whatever2} stamina for your successful bullying.";
            //{ Math.Round(whatever, 0)}/{ Math.Floor(XPMULT * pc.GetStat(StatTypes.StM))}
            // finalize
            overallWinner.AddStat(StatTypes.Bly, 1, false, false, false);
            overallLoser.AddStat(StatTypes.Sbm, 1, false, false, false);
            Data.DataDb.UpdateCard((overallWinner as Cards.PlayerCard));
            Data.DataDb.UpdateCard((overallLoser as Cards.PlayerCard));
            Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"The fight is over! {overallWinner.DisplayName} won {Winners.Count(x => x.Name.Equals(overallWinner.Name))} out of {Rounds} rounds (gaining {Math.Round(whatever2, 0)} " +
                $"stamina) against the pathetic {overallLoser.DisplayName}, whom loses {Math.Round(whatever, 0)} stamina for the shameful defeat.", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        public void StartEncounter()
        {
            turncounter = 0;
            orderedbyturns = new List<EncounterCard>();
            EncounterStatus = EncounterStatus.Active;

            foreach (var v in Participants)
            {
                foreach (var x in v.Value)
                {
                    x.Status = CharacterStatusTypes.Alive;
                    x.CurrentVitality = x.GetStat(StatTypes.Vit);
                    orderedbyturns.Add(new EncounterCard(x, v.Key));
                }
            }

            if (orderedbyturns.Count <= 1)
                throw new Exception("Attempting to start combat encounter with 1 or fewer participants.");

            orderedbyturns.OrderByDescending(x => x.Participant.GetStat(StatTypes.Spd));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="participant"></param>
        /// <returns></returns>
        public bool AddParticipant(int team, BaseCard participant)
        {
            foreach (var key in Participants.Keys)
            {
                if (Participants[key].Contains(participant))
                    return false;
            }

            if (!Participants.ContainsKey(team))
                Participants[team] = new List<BaseCard>();

            Participants[team].Add(participant);
            return true;
        }

        public Encounter()
        {
            turncounter = 0;
            Participants = new Dictionary<int, List<BaseCard>>();
            orderedbyturns = new List<EncounterCard>();
            EncounterStatus = EncounterStatus.Forming;
            EncounterCode = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            PrepTimeout = new TimeSpan(0, 30, 0);
            CreationDate = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        public Encounter(TimeSpan timeout)
        {
            turncounter = 0;
            Participants = new Dictionary<int, List<BaseCard>>();
            orderedbyturns = new List<EncounterCard>();
            EncounterStatus = EncounterStatus.Forming;
            EncounterCode = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            PrepTimeout = timeout;
            CreationDate = DateTime.Now;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        public void TakeTurn(out EncounterResults results)
        {
            results = new EncounterResults();
            turncounter++;
            EncounterCard previousEc = null;
            foreach (var ec in orderedbyturns)
            {
                /////////////
                // setup step
                /////////////
                results.ResponseStr += $"Turn {turncounter}:\\n";
                if (previousEc != ec)
                {
                    if (ec.Participant.Status == CharacterStatusTypes.Alive)
                    {  
                        previousEc = ec;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    break;
                }

                BaseCard combatant = ec.Participant;

                // find an enemy
                List<int> validTeams = new List<int>();
                foreach (var key in Participants.Keys)
                {
                    if (key != ec.Team)
                    {
                        validTeams.Add(key);
                    }
                }

                if (validTeams.Count < 1)
                {
                    // we're done
                    EncounterStatus = EncounterStatus.Resolved;
                    break;
                }

                int teamToUse = validTeams[RngGeneration.Rng.Next(0, validTeams.Count-1)];
                var availableEnemies = orderedbyturns.Where(x => x.Team.Equals(teamToUse) && x.Participant.Status != CharacterStatusTypes.Dead && x.Participant.Status != CharacterStatusTypes.Vengeful && x.Participant.Status != CharacterStatusTypes.Undefined).ToList();

                if (availableEnemies.Count < 1)
                {
                    // we're done
                    EncounterStatus = EncounterStatus.Resolved;
                    break;
                }

                ////////////////
                // attacker step
                ////////////////
                EncounterCard ce = availableEnemies[RngGeneration.Rng.Next(0, availableEnemies.Count - 1)];
                BaseCard currentEnemy = ce.Participant;
                int critChance = (int)Math.Sqrt(.5 * combatant.GetStat(Enums.StatTypes.Crc));
                int critDmgMultiplier = (int)(20 + (0.1 * combatant.GetStat(Enums.StatTypes.Crt)));
                int hitChance = (combatant.CardType == Enums.CardTypes.PlayerCard ? 70 : 65) + (int)(.5 * Math.Sqrt(combatant.GetStat(Enums.StatTypes.Atk)));
                int baseDmg = combatant.GetStat(Enums.StatTypes.Dmg);
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

                    if (ws.SecondaryDamageType != DamageTypes.None)
                    {
                        bool secondaryMagical = ws.SecondaryDamageType != DamageTypes.Physical;
                        if (ws.SecondaryDamageType == ws.DamageType)
                        {
                            double multiplier = .2;
                            baseDmg += (int)(((secondaryMagical) ? combatant.GetStat(StatTypes.Int) : combatant.GetStat(StatTypes.Dex)) * multiplier);
                        }
                        else
                        {
                            double multiplier = .4;
                            baseDmg += (int)(((secondaryMagical) ? combatant.GetStat(StatTypes.Int) : combatant.GetStat(StatTypes.Int)) * multiplier);
                        }
                    }
                }


                // spread
                int spread = 10 - (int)Math.Sqrt(.3 * (combatant.GetStat(Enums.StatTypes.Ats)));
                if (spread < 0) spread = 0;

                // determine dmg roll
                //double lowrollpercent = (.1 - (spread * .001));
                double mypercent = baseDmg * (spread * .01);
                int lowRoll = Convert.ToInt32(baseDmg - mypercent);
                if (lowRoll < 0) lowRoll = 0;
                int highRoll = Convert.ToInt32(baseDmg + mypercent);
                if (highRoll < 1) highRoll = 1;

                // roll for total dmg
                int totalDmg = RngGeneration.Rng.Next(lowRoll, highRoll);

                // add sharpness boon
                if (combatant.BoonsEarned.Contains(Enums.BoonTypes.Sharpness)) totalDmg = Convert.ToInt32(totalDmg * 1.05);

                // add resiliance boon
                if (combatant.BoonsEarned.Contains(Enums.BoonTypes.Resiliance)) totalDmg = Convert.ToInt32(totalDmg * 1.05);

                // check for crit and add crit dmg
                if (RngGeneration.Rng.Next(101) < critChance)
                    totalDmg *= (int)(1 + (.1 * critDmgMultiplier));

                ////////////////
                // defender step
                ////////////////
                int baseEvasion = 0 + (int)(.5 * Math.Sqrt(currentEnemy.GetStat(Enums.StatTypes.Eva)));
                int mdf = currentEnemy.GetStat(Enums.StatTypes.Mdf);
                int pdf = currentEnemy.GetStat(Enums.StatTypes.Pdf);
                int baseDmgReduction = (int)(isMagical ? (0.3 * pdf) + mdf : (0.3 * mdf) + pdf) + (int)(currentEnemy.GetStat(Enums.StatTypes.Con) * 0.3);

                // add empowerment boon
                if (currentEnemy.BoonsEarned.Contains(Enums.BoonTypes.Empowerment)) baseDmgReduction = Convert.ToInt32(totalDmg * 1.05);

                // get total damage reduction
                int val = -1;
                var tdr = 20 + (.7 * Math.Sqrt(baseDmgReduction));

                //////////////////
                // resolution step
                //////////////////
                int hitValue = RngGeneration.Rng.Next(1, 101);
                if (hitValue < (hitChance - baseEvasion))
                {
                    // didn't dodge, apply damage
                    var dmgMult = 1 - (.01 * tdr);
                    val = (int)(totalDmg * dmgMult);

                    // if we do negative damage, set damage to zero for now
                    if (val <= 0) val = 0;

                    // level difference multiplier
                    double diff = 0;
                    if (combatant.CardType == CardTypes.PlayerCard && currentEnemy.CardType == CardTypes.PlayerCard)
                    {
                        diff = combatant.GetStat(StatTypes.Lvl) - currentEnemy.GetStat(StatTypes.Lvl);
                    }

                    if (diff < 0)
                    {
                        if (diff < 5) diff = 5;

                        diff *= -5;
                        diff *= .01;
                        val = Convert.ToInt32(val * diff);
                        if (val < 1) val = 1;
                    }

                    // deal the damage
                    currentEnemy.CurrentVitality -= val;

                    // check if the enemy is dead
                    if (currentEnemy.CurrentVitality <= 0)
                    {
                        if (currentEnemy.CardType == CardTypes.PlayerCard && combatant.CardType == CardTypes.PlayerCard)
                        {
                            results.Loser = currentEnemy;
                            results.Winner = combatant;
                            Winners.Add(combatant);
                            Losers.Add(currentEnemy);
                        }

                        results.DefeatedParticipants.Add(ce.Participant);
                        ce.Participant.Status = CharacterStatusTypes.Dead;
                        results.ResponseStr += $"{combatant.DisplayName} - Dmg done to {currentEnemy.DisplayName}: [b]Deathblow[/b] {val.ToString()}";
                    }
                    else
                    {
                        results.ResponseStr += $"{combatant.DisplayName} - Dmg done to {currentEnemy.DisplayName}: {val.ToString()}";
                    }
                }
                else
                {
                    results.ResponseStr += $"{combatant.DisplayName} - {currentEnemy.DisplayName} evaded the attack!";
                }
                results.ResponseStr += "\\n";

                availableEnemies = orderedbyturns.Where(x => x.Team.Equals(teamToUse) && x.Participant.Status != CharacterStatusTypes.Dead && x.Participant.Status != CharacterStatusTypes.Vengeful && x.Participant.Status != CharacterStatusTypes.Undefined).ToList();

                if (availableEnemies.Count < 1)
                {
                    // we're done
                    EncounterStatus = EncounterStatus.Resolved;
                    break;
                }
            }
        }
    }
}