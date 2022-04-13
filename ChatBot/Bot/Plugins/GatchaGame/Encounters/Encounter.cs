using ChatApi;
using ChatApi.Objects;
using ChatBot.Bot.Plugins.GatchaGame.Cards;
using ChatBot.Bot.Plugins.GatchaGame.Dive.Results;
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
        public List<EncounterCard> Participants;

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
        public EncounterResults lastEncounterResults;

        /// <summary>
        /// 
        /// </summary>
        public Channel EncounterChannel;

        public EncounterResults _EncounterResult;

        public string Creator;

        public EncounterTypes EncounterType;


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

        public bool HasTimedOut()
        {
            if (DateTime.Now - CreationDate > PrepTimeout)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        //public void StartASyncEncounter(ApiConnection api, Channel channel)
        //{
        //    if (EncounterStatus == EncounterStatus.Active)
        //        return;
        //
        //    Timer enc = new Timer(new TimeSpan(0, 1, 0).TotalMilliseconds)
        //    {
        //        AutoReset = true
        //    };
        //
        //    enc.Elapsed += Enc_Elapsed;
        //    enc.Enabled = true;
        //    TheTimer = enc;
        //    Api = api;
        //    theChannel = channel;
        //    Rounds = 0;
        //    turncounter = 0;
        //    StartEncounter(EncounterTypes.Bully);
        //    tPants = TPants;
        //    tLocker = new object();
        //    lastEncounterResults = new EncounterResults();
        //    EncounterChannel = channel;
        //    Winners = new List<Cards.BaseCard>();
        //    Losers = new List<Cards.BaseCard>();
        //
        //    TakeShot();
        //}

        //public void TPants()
        //{
        //    if (TheTimer.Enabled)
        //    {
        //        TheTimer.Stop();
        //        EndFightLogic();
        //    }
        //}

        //private void TakeShot()
        //{
        //    EncounterResults lastRes;
        //
        //    lock (tLocker)
        //    {
        //        //StartEncounter( EncounterTypes.Room);
        //        Rounds++;
        //        if (Rounds < 2)
        //        {
        //            string outRes = string.Empty;
        //            do
        //            {
        //                TakeTurn(out lastRes);
        //                outRes += lastRes.ResponseStr;
        //            } while (EncounterStatus == EncounterStatus.Active);
        //
        //            if (lastRes.Winner != null && lastRes.Loser != null)
        //            {
        //                Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"{lastRes.Winner.DisplayName} won round {Rounds} against {lastRes.Loser.DisplayName} over a fight that had {turncounter} turns!", string.Empty);
        //            }
        //            else
        //            {
        //                var allP = GetAllParticipants();
        //                BaseCard u1 = allP.First();
        //                BaseCard u2 = allP.First(x => !x.Equals(u1));
        //
        //                Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, 
        //                    $"{u1.DisplayName} tied against {u2.DisplayName} over {turncounter} turns!"// + 
        //                    //$"\\n{u1.DisplayName} {(u1 as Cards.PlayerCard).GetStatsString()}" +
        //                    //$"\\n{u2.DisplayName} {(u2 as Cards.PlayerCard).GetStatsString()}"
        //                    , string.Empty);
        //            }
        //
        //            lastEncounterResults = lastRes;
        //        }
        //
        //        if (Rounds == 1)
        //        {
        //            tPants?.Invoke();
        //        }
        //        else if (Rounds >= 2)
        //        {
        //            lock (tLocker)
        //            {
        //                TheTimer.Stop();
        //                EndFightLogic();
        //            }
        //        }
        //    }
        //
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Enc_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    TakeShot();
        //}

        //public void EndFightLogic()
        //{
        //    //if (Winners == null || Winners.Count < 1 || (Winners.Count == 2) && !Winners.First().Name.Equals(Winners.Last().Name))
        //    //{
        //    //    Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"The fight is over, somehow ending in a tie!", string.Empty);
        //    //    return;
        //    //}
        //
        //    List<Cards.BaseCard> p1 = Winners.Where(x => x.Name.Equals(Winners.First().Name)).ToList();
        //    double middle = Rounds * 0.5;
        //
        //    Cards.BaseCard overallWinner = (p1.Count > middle) ? p1.First() : Losers.First(x => !x.Name.Equals(p1.First().Name));
        //    Cards.BaseCard overallLoser = p1.First() == overallWinner ? Losers.First(x => !x.Name.Equals(p1.First().Name)) : p1.First();
        //
        //    // submit calc
        //    string submitStr = string.Empty;
        //    TimeSpan submitVal = new TimeSpan(0, 40, 0);
        //
        //    int staLost;
        //    if (overallLoser.GetStat(StatTypes.Sta) > submitVal.TotalSeconds)
        //    {
        //        overallLoser.AddStat(StatTypes.Sta, -(submitVal.TotalSeconds));
        //        staLost = Convert.ToInt32(submitVal.TotalSeconds);
        //    }
        //    else
        //    {
        //        staLost = overallLoser.GetStat(StatTypes.Sta);
        //        overallLoser.SetStat(StatTypes.Sta, 0);
        //    }
        //
        //    //double pants = 90.0 / ucard.GetStat(StatTypes.StM);
        //    double whatever = RngGeneration.XPMULT * staLost;
        //
        //    submitStr += $"{overallLoser.DisplayName}, you submit to {overallWinner.DisplayName}'s aggressive bullying, losing {whatever} stamina. ";
        //
        //    // bully calc
        //    int staWon;
        //    if (overallWinner.GetStat(StatTypes.Sta) + staLost >= overallWinner.GetStat(StatTypes.StM))
        //    {
        //        staWon = Convert.ToInt32(overallWinner.GetStat(StatTypes.StM) - overallWinner.GetStat(StatTypes.Sta));
        //        overallWinner.SetStat(StatTypes.Sta, overallWinner.GetStat(StatTypes.StM));
        //    }
        //    else
        //    {
        //        staWon = staLost;
        //        overallWinner.AddStat(StatTypes.Sta, staLost);
        //    }
        //
        //    //pants = 90.0 / ucard.GetStat(StatTypes.StM);
        //    double whatever2 = RngGeneration.XPMULT * staWon;
        //
        //    submitStr += $"{overallWinner.DisplayName}, you gain {whatever2} stamina for your successful bullying.";
        //    //{ Math.Round(whatever, 0)}/{ Math.Floor(XPMULT * pc.GetStat(StatTypes.StM))}
        //    // finalize
        //    overallWinner.AddStat(StatTypes.Bly, 1, false, false, false);
        //    overallLoser.AddStat(StatTypes.Sbm, 1, false, false, false);
        //    Data.DataDb.UpdateCard((overallWinner as Cards.PlayerCard));
        //    Data.DataDb.UpdateCard((overallLoser as Cards.PlayerCard));
        //    //Api.SendMessage(string.IsNullOrWhiteSpace(theChannel.Code) ? theChannel.Name : theChannel.Code, $"The fight is over! {overallWinner.DisplayName} won {Winners.Count(x => x.Name.Equals(overallWinner.Name))} out of {Rounds} rounds (gaining {Math.Round(whatever2, 0)} " +
        //    //    $"stamina) against the pathetic {overallLoser.DisplayName}, whom loses {Math.Round(whatever, 0)} stamina for the shameful defeat.", string.Empty);
        //}

        /// <summary>
        /// 
        /// </summary>
        public void StartEncounter(EncounterTypes type)
        {
            EncounterType = type;
            EncounterStatus = EncounterStatus.Active;
            TotalTurns = new List<Turn>();
            _EncounterResult = new EncounterResults();
            Participants.ForEach(x => _EncounterResult.AllRewards[x.Participant.Name] = new List<Reward>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="team"></param>
        /// <param name="participant"></param>
        /// <returns></returns>
        public bool AddParticipant(int team, BaseCard participant)
        {
            foreach (var v in Participants)
            {
                if (v.Participant.Name.Equals(participant.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            Participants.Add(new EncounterCard(participant, team));
            return true;
        }

        public Encounter()
        {
            Setup(new TimeSpan(0, 30, 0), null);
        }

        void Setup(TimeSpan timeout, string creator)
        {
            Participants = new List<EncounterCard>();
            EncounterStatus = EncounterStatus.Forming;
            EncounterCode = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
            PrepTimeout = timeout;
            CreationDate = DateTime.Now;
            Creator = creator;
            TotalTurns = new List<Turn>();
            _EncounterResult = new EncounterResults();
        }

        /// <summary>
        /// 
        /// </summary>
        public Encounter(TimeSpan timeout, string creator)
        {
            Setup(timeout, creator);
        }

        public void RemoveParticipantsByType(Type type)
        {
            List<EncounterCard> toRemove = new List<EncounterCard>();

            foreach (var v in Participants)
            {
                if (v.Participant.GetType() == type)
                    toRemove.Add(v);
            }

            foreach(var v in toRemove)
            {
                Participants.Remove(v);
            }
        }

        public List<Turn> TotalTurns;

        public void ResetBossEncounter()
        {
            List<EncounterCard> toRemove = new List<EncounterCard>();
            Participants.ForEach(x =>
            {
                if (x.Participant is Cards.PlayerCard) toRemove.Add(x);
            });
        }

        public EncounterResults RunEncounter(List<Type> CardTypesToReset)
        {
            foreach (var v in CardTypesToReset)
            {
                foreach (var y in Participants)
                {
                    y.Participant.CurrentVitality = y.Participant.GetStat(StatTypes.Vit);
                    y.Participant.Status = CharacterStatusTypes.Alive;
                }
            }

            return RunEncounter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="results"></param>
        public EncounterResults RunEncounter()
        {
            _EncounterResult = new EncounterResults
            {
                Participants = Participants
            };


            do
            {
                List<int> AvailableTeams = new List<int>();
                Turn turn = new Turn(Participants);
                TurnResult tr = turn.TakeTurn(_EncounterResult.AllRewards);
                _EncounterResult._Turns.Add(turn);

                Participants = turn.Participants;

                foreach (var p in Participants)
                {
                    if (AvailableTeams.Contains(p.Team) || p.Participant.CurrentVitality <= 0 || p.Participant.Status == CharacterStatusTypes.Dead || p.Participant.Status == CharacterStatusTypes.Undefined)
                        continue;

                    AvailableTeams.Add(p.Team);
                }

                if (AvailableTeams.Count <= 1)
                {
                    EncounterStatus = EncounterStatus.Resolved;
                }
            } while (EncounterStatus == EncounterStatus.Active);

            _EncounterResult.TotalRounds = _EncounterResult._Turns.Count;
            _EncounterResult.WinningTeam = Participants.First(x => x.Participant.CurrentVitality > 0 && x.Participant.Status != CharacterStatusTypes.Dead).Team;
            return _EncounterResult;
        }
    }
}