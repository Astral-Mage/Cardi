using ChatBot.Bot.Plugins.GatchaGame.Dive.Results;
using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.GatchaGame.Encounters
{
    [Serializable]
    public class Turn
    {
        public List<EncounterCard> Participants;

        public TurnResult _TurnResult;

        public Turn(List<EncounterCard> participants)
        {
            if (participants.Count < 2)
                throw new Exception("Starting fight with less than 2 participants.");

            List<int> Teams = new List<int>();
            foreach (var v in participants)
            {
                if (!Teams.Contains(v.Team)) Teams.Add(v.Team);
            }

            if (Teams.Count < 2)
                throw new Exception("Starting fight with less than 2 teams.");

            Participants = participants;

        }

        public TurnResult TakeTurn(Dictionary<string, List<Reward>> _rewards)
        {
            // prep
            _TurnResult = new TurnResult
            {
                _LivingParticipants = Participants.Count(x => x.Participant.CurrentVitality > 0 || x.Participant.Status == CharacterStatusTypes.Alive)
            };

            // order by speed
            Participants = CombatUtilities.OrderBySpeed(Participants);

            // give all participants a turn in order
            for (int currentAttacker = 0; currentAttacker < Participants.Count; currentAttacker++)
            {
                // get attacker and defender
                EncounterCard _Attacker = Participants[currentAttacker];
                EncounterCard _Defender = CombatUtilities.SelectRandomAvailableEnemy(_Attacker.Team, Participants);
                Rotation cr = new Rotation();

                // make sure attacker is alive
                if (_Attacker.Participant.Status == CharacterStatusTypes.Dead || _Attacker.Participant.CurrentVitality <= 0)
                    continue;

                // check to make sure we still have at least one available defender
                if (null == _Defender)
                    break;

                // sanity check
                if (Participants.Count(x => x.Participant.CurrentVitality > 0) < 2)
                {
                    break;
                }

                // log starting values
                cr._Attacker                                           = _Attacker.Participant.Name;
                cr._Defender                                           = _Defender.Participant.Name;
                cr._AtkStartingVit                                     = _Attacker.Participant.CurrentVitality;
                cr._DefStartingVit                                     = _Defender.Participant.CurrentVitality;

                // calculate attacker stats
                double _BaseDamage                                              = CombatUtilities.CalculateBaseDamage(_Attacker);
                double _AddedDmg                                                = CombatUtilities.CalculateTotalAddedDamage(_Attacker);

                Dictionary<RawDamageType, double> _DmgMultByType                = CombatUtilities.CaculateTotalDamageMult(_Attacker, _Defender.Participant.GetStat(StatTypes.Lvl));
                Dictionary<RawDamageType, double> _DamageByType                 = CombatUtilities.SplitDamageIntoRawTypes(_BaseDamage + _AddedDmg, _Attacker);
                Dictionary<RawDamageType, double> _TotalDamageByType            = CombatUtilities.CalculateTotalDamagePerType(_DamageByType, _DmgMultByType);

                double _HitChance                                               = CombatUtilities.CalculateHitChance(_Attacker);
                double _CritChance                                              = CombatUtilities.CalculateCriticalChance(_Attacker);
                double _CritMult                                                = CombatUtilities.CalculateCriticalMultiplier(_Attacker);

                // calculate defender stats
                double _EvasionChance                                           = CombatUtilities.CalculateEvasionChance(_Defender);
                Dictionary<RawDamageType, double> _FlatDamageReductionByType    = CombatUtilities.CalculateFlatDamageReductionByType(_Defender);
                Dictionary<RawDamageType, double> _DamageReductionMultByType    = CombatUtilities.CalculateDamageReductionMult(_Defender);

                // determine hits and crits
                bool _HitConnected                                              = CombatUtilities.CalculateHits(_HitChance, _EvasionChance);
                bool _CritConnected                                             = CombatUtilities.CalculateCrits(_CritChance);

                double _TotalCombinedDamage                                     = CombatUtilities.CalculateTotalCombinedDamage(_TotalDamageByType, _DamageReductionMultByType, _FlatDamageReductionByType);
                double _CritDamage                                              = CombatUtilities.CalculateAddedCriticalDamage(_TotalCombinedDamage, _CritMult);
                double _TotalOverallDamageDealt                                 = _TotalCombinedDamage;
                _TotalOverallDamageDealt                                        = CombatUtilities.CalculateRandomizedSpread(_TotalOverallDamageDealt, _Attacker.Participant.GetStat(StatTypes.Ats));

                // if the attacker missed, move on
                if (_HitConnected)
                {
                    // if we crit, add crit damage
                    if (_CritConnected)
                        _TotalOverallDamageDealt += _CritDamage;

                    // apply damage
                    _Defender.Participant.CurrentVitality -= Convert.ToInt32(Math.Floor(_TotalOverallDamageDealt));
                    if (_Defender.Participant.CurrentVitality <= 0)
                    {
                        _Defender.Participant.Status = CharacterStatusTypes.Dead;
                        var trewards = _Defender.Participant.GetRewards(EncounterTypes.Room);
                        foreach (Reward rew in trewards)
                        {
                            // level up check
                            if (rew.RewardType == RewardTypes.Stat && rew.StatRewards.ContainsKey(StatTypes.Exp) && _Attacker.Participant is Cards.PlayerCard)
                            {
                                while ((_Attacker.Participant as Cards.PlayerCard).XpNeededToLevel() < rew.StatRewards[StatTypes.Exp])
                                {
                                    // grant lvl reward
                                    Reward lvlRew = new Reward(RewardTypes.Stat, StatTypes.Lvl, 1);
                                    _Attacker.Participant.GrantReward(lvlRew);
                                    if (_rewards.ContainsKey(_Attacker.Participant.Name))
                                        _rewards[_Attacker.Participant.Name].Add(lvlRew);
                                    else
                                        _rewards[_Attacker.Participant.Name] = new List<Reward>() { lvlRew };

                                    // grant xp diff needed to reach lvl reward
                                    Reward tnr = new Reward(RewardTypes.Stat, StatTypes.Exp, (_Attacker.Participant as Cards.PlayerCard).XpNeededToLevel());
                                    if (_rewards.ContainsKey(_Attacker.Participant.Name))
                                        _rewards[_Attacker.Participant.Name].Add(tnr);
                                    else
                                        _rewards[_Attacker.Participant.Name] = new List<Reward>() { tnr };
                                    _Attacker.Participant.GrantReward(tnr);
                                    rew.StatRewards[StatTypes.Exp] -= tnr.StatRewards[StatTypes.Exp];
                                }

                                // grant remaining xp after level ups
                                if (_rewards.ContainsKey(_Attacker.Participant.Name))
                                    _rewards[_Attacker.Participant.Name].Add(rew);
                                else
                                    _rewards[_Attacker.Participant.Name] = new List<Reward>() { rew };
                                _Attacker.Participant.GrantReward(rew);
                            }

                            // most stats
                            if (rew.RewardType == RewardTypes.Stat && !rew.StatRewards.ContainsKey(StatTypes.Exp))
                            {
                                _Attacker.Participant.GrantReward(rew);

                                if (_rewards.ContainsKey(_Attacker.Participant.Name))
                                    _rewards[_Attacker.Participant.Name].Add(rew);
                                else
                                    _rewards[_Attacker.Participant.Name] = new List<Reward>() { rew };
                            }
                        }
                    }
                }

                // log all damage and stuff
                cr._BaseDamage                                                  = _BaseDamage;
                cr._AddedDmg                                                    = _AddedDmg;
                cr._HitChance                                                   = _HitChance;
                cr._CritMult                                                    = _CritMult;
                cr._DmgMultByType                                               = _DmgMultByType;
                cr._DamageByType                                                = _DamageByType;
                cr._TotalDamageByType                                           = _TotalDamageByType;
                cr._FlatDamageReductionByType                                   = _FlatDamageReductionByType;
                cr._DamageReductionMultByType                                   = _DamageReductionMultByType;
                cr._HitConnected                                                = _HitConnected;
                cr._CritConnected                                               = _CritConnected;
                cr._TotalCombinedDamage                                         = _TotalCombinedDamage;
                cr._CritDamage                                                  = _CritDamage;
                cr._TotalOverallDamageDealt                                     = _TotalOverallDamageDealt;
                _TurnResult._Rotations.Add(cr);
            }

            CalculateEndOfTurn();

            return _TurnResult;
        }

        bool winnerFound = false;

        public bool WasWinnerFound()
        {
            return winnerFound;
        }

        public void CalculateEndOfTurn()
        {
            List<EncounterCard> FoundTeamsWithLivingMembers = new List<EncounterCard>();
            foreach(var v in Participants)
            {
                if (v.Participant.CurrentVitality <= 0 || v.Participant.Status == CharacterStatusTypes.Dead)
                    continue;

                if (FoundTeamsWithLivingMembers.Any(x => x.Team == v.Team))
                    continue;

                FoundTeamsWithLivingMembers.Add(v);
            }

            if (FoundTeamsWithLivingMembers.Count <= 1)
            {
                // winner was found
                winnerFound = true;
                _TurnResult._WinnerFound = true;
                _TurnResult._Winner = FoundTeamsWithLivingMembers.First().Participant.Name;
            }
        }
    }
}
