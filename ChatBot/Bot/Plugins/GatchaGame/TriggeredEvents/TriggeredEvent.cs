using ChatBot.Bot.Plugins.GatchaGame.Encounters;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using ChatBot.Bot.Plugins.GatchaGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace ChatBot.Bot.Plugins.GatchaGame
{
    [Serializable]
    public class TriggeredEvent
    {
        public readonly int EventId;
        public string EventTitle;
        public readonly TimeSpan Timeout;
        public readonly DateTime CreatedDate;
        public readonly TimeSpan Cooldown;
        public string TimeoutMessage;
        public string StartMessage;
        public string CooldownMessage;
        public bool TimedOut;
        public TimeSpan WaveTimerTimer;
        public Timer WaveTimer;

        public List<Cards.PlayerCard> PendingPlayers;
        public delegate void EventKiller();
        public EventKiller tPants;
        public List<string> ActiveRooms;

        public ChatApi.ApiConnection apiC;
        Encounters.Encounter TheEncounter;

        public List<Cards.PlayerCard> TotalParticipants;
        public TriggeredEventState _EventState;
        public void TPants()
        {
            // do wave stuff here
            if (PendingPlayers.Count <= 0)
                return;

            List<Cards.PlayerCard> toDelete = new List<Cards.PlayerCard>();
            List<Cards.PlayerCard> toRefresh = new List<Cards.PlayerCard>();

            // add players to encounter
            foreach (var pp in PendingPlayers)
            {
                if (!RngGeneration.TryGetCard(pp.Name, out Cards.PlayerCard tpc))
                    toDelete.Add(pp);

                toRefresh.Add(tpc);
                TheEncounter.AddParticipant(1 ,tpc);
            }

            // remove any erronious entries
            if (toDelete.Count > 0)
                toDelete.ForEach(x => PendingPlayers.Remove(x));

            PendingPlayers = toRefresh;

            // announce
            foreach(var v in ActiveRooms)
            {
                apiC.SendMessage(v, $"Wave triggered with {PendingPlayers.Count} Adventurers.", null, ChatApi.MessageType.Basic);
            }

            // starting boss hp
            int curBossHp = TheEncounter.Participants.First(x => x.Participant.CardType == CardTypes.BossEnemyCard).Participant.CurrentVitality;

            // do encounter stuff
            TheEncounter.StartEncounter(EncounterTypes.Boss);
            var results = TheEncounter.RunEncounter();

            // check for winner and hand out rewards
            if (TheEncounter.Participants.Any(x => x.Participant.CardType == CardTypes.BossEnemyCard && x.Participant.CurrentVitality <= 0))
            {
                for (int i = 0; i < ActiveRooms.Count; i++)
                {
                    apiC.SendMessage(ActiveRooms[i], $"The boss died but I haven't parsed the results yet.", string.Empty);

                }

                var trewards = TheEncounter.Participants.First(x => x.Participant.CardType == CardTypes.BossEnemyCard).Participant.GetRewards(EncounterTypes.Room);

                // loop through each reward
                foreach (Reward rew in trewards)
                {

                    // loop through each participant
                    foreach (EncounterCard cp in TheEncounter.Participants.Where(x => x.Participant.CardType == CardTypes.PlayerCard).ToList())
                    {


                        // level up check
                        //if (rew.RewardType == RewardTypes.Stat && rew.StatRewards.ContainsKey(StatTypes.Exp))
                        //{
                        //
                        //
                        //
                        //
                        //    while ((cp.Participant as Cards.PlayerCard).XpNeededToLevel() < rew.StatRewards[StatTypes.Exp])
                        //    {
                        //        // grant lvl reward
                        //        Reward lvlRew = new Reward(RewardTypes.Stat, StatTypes.Lvl, 1);
                        //        cp.Participant.GrantReward(lvlRew);
                        //        if (_rewards.ContainsKey(cp.Participant.Name))
                        //            _rewards[cp.Participant.Name].Add(lvlRew);
                        //        else
                        //            _rewards[cp.Participant.Name] = new List<Reward>() { lvlRew };
                        //    
                        //        // grant xp diff needed to reach lvl reward
                        //        Reward tnr = new Reward(RewardTypes.Stat, StatTypes.Exp, (cp.Participant as Cards.PlayerCard).XpNeededToLevel());
                        //        if (_rewards.ContainsKey(cp.Participant.Name))
                        //            _rewards[cp.Participant.Name].Add(tnr);
                        //        else
                        //            _rewards[cp.Participant.Name] = new List<Reward>() { tnr };
                        //        cp.Participant.GrantReward(tnr);
                        //        rew.StatRewards[StatTypes.Exp] -= tnr.StatRewards[StatTypes.Exp];
                        //    }
                        //    
                        //    // grant remaining xp after level ups
                        //    if (_rewards.ContainsKey(cp.Participant.Name))
                        //        _rewards[cp.Participant.Name].Add(rew);
                        //    else
                        //        _rewards[cp.Participant.Name] = new List<Reward>() { rew };
                        //    cp.Participant.GrantReward(rew);
                        //}

                        // most stats
                        //if (rew.RewardType == RewardTypes.Stat && !rew.StatRewards.ContainsKey(StatTypes.Exp))
                        //{
                        //    _Attacker.Participant.GrantReward(rew);
                        //
                        //    if (_rewards.ContainsKey(_Attacker.Participant.Name))
                        //        _rewards[_Attacker.Participant.Name].Add(rew);
                        //    else
                        //        _rewards[_Attacker.Participant.Name] = new List<Reward>() { rew };
                        //}
                    }

                }









                _EventState = TriggeredEventState.Resolved;
            }
            else
            {
                for (int i = 0; i < ActiveRooms.Count; i++)
                    apiC.SendMessage(ActiveRooms[i], $"The boss killed all the Adventurers but I haven't parsed the results yet. Boss Hp: {curBossHp} ⨠ {TheEncounter.Participants.First(x => x.Participant.CardType == Enums.CardTypes.BossEnemyCard).Participant.CurrentVitality}", string.Empty);
            }

            // reset for new wave
            PendingPlayers = new List<Cards.PlayerCard>();
            TheEncounter.ResetBossEncounter();

            // check to see if we've timed out
            if (DateTime.Now - CreatedDate > Timeout)
            {
                if (WaveTimer.Enabled)
                {
                    WaveTimer.Stop();
                }
            }
        }

        public TriggeredEvent(int eventId, ChatApi.ApiConnection api)
        {
            EventId = eventId;
            WaveTimerTimer = new TimeSpan(1, 30, 0);
            Timeout = new TimeSpan(18, 0, 0);
            CreatedDate = DateTime.Now;
            EventTitle = "[b]The Hungering Devourer[/b]";
            TimeoutMessage = $"{EventTitle} event ended incomplete after {Timeout.ToString("c")}.";
            StartMessage = $"Starting {EventTitle} event! Type " + "{cmdc}" + $"{CommandStrings.TB} to join the current assault force!";
            CooldownMessage = $"{EventTitle} event has cooled down, and may once more be triggered...";
            Cooldown = new TimeSpan(48, 0, 0);
            PendingPlayers = new List<Cards.PlayerCard>();
            TimedOut = false;
            tPants = TPants;
            apiC = api;
            TotalParticipants = new List<Cards.PlayerCard>();
            _EventState = TriggeredEventState.Active;
        }

        public void StartEvent()
        {
            // sanity check
            if (null != WaveTimer)
                return;

            // generate boss and start encounter
            TheEncounter = new Encounters.Encounter(new TimeSpan(), null);
            var enemy = RngGeneration.GenerateBossEnemy(2);

            // reset vars
            TotalParticipants = new List<Cards.PlayerCard>();
            PendingPlayers = new List<Cards.PlayerCard>();
            TimedOut = false;

            // add boss and start encounter
            TheEncounter.AddParticipant(0, enemy);


            // start timer
            WaveTimer = new Timer(WaveTimerTimer.TotalMilliseconds);
            WaveTimer.Elapsed += WaveTimer_Elapsed; ;
            WaveTimer.Enabled = true;
            WaveTimer.Start();
        }

        public void TriggerWave()
        {
            TPants();
        }

        public void EndEvent()
        {

        }

        private void WaveTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            tPants?.Invoke();
        }

        public List<string> GetCommandStrings()
        {
            List<string> toReturn = new List<string>
            {
                CommandStrings.TB
            };

            return toReturn;
        }
    }
}
