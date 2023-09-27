using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Bot.Plugins.LostRPG.EquipmentSystem.EquipmentObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem
{
    public class UserCard
    {
        // user stuff
        public string Name { get; protected set; }

        public int UserId { get; set; }

        public string Alias { get; set; }

        public Roleplay RpData { get; set; }


        public StatData Stats { get; set; }

        public bool Verbose { get; set; }

        public string CurrentTitle { get; set; }

        public List<string> Titles { get; set; }

        public List<int> Skills { get; set; }

        public Specialization Spec { get; set; }

        public Archetype Archetype { get; set; }

        public Calling Calling { get; set; }

        // equipment
        public List<Socket> ActiveSockets { get; set; }

        public UserCard(string name)
        {
            Name = name;
            Alias = Name;
            UserId = -1;
            Verbose = true;
            RpData = new Roleplay();
            Stats = new StatData();
            CurrentTitle = string.Empty;
            Titles = new List<string>();
            Skills = new List<int>();
            Spec = null;
            Archetype = null;
            Calling = null;
            ActiveSockets = new List<Socket>();
        }

        public void SetStats(StatData stats)
        {
            Stats = stats;
        }

        public int GetStat(StatTypes type)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }
            return Convert.ToInt32(Math.Floor(Stats.Stats[type]));
        }

        public List<Effect> GetPassiveEffectsByType(EffectTypes type)
        {
            List<Effect> effectNames = new List<Effect>();
            switch(type)
            {
                case EffectTypes.Debuff:
                    {
                        if (Spec.Debuffs.Any())
                        {
                            Spec.Debuffs.ForEach((x) =>
                            {
                                var tee = DataDb.EffectDb.GetEffect(x);
                                if (tee.GlobalDuration == TimeSpan.MaxValue)
                                {
                                    effectNames.Add(tee);
                                }
                            });
                        }
                        if (Archetype.Debuffs.Any())
                        {
                            Archetype.Debuffs.ForEach((x) =>
                            {
                                var tee = DataDb.EffectDb.GetEffect(x);
                                if (tee.GlobalDuration == TimeSpan.MaxValue)
                                {
                                    effectNames.Add(tee);
                                }
                            });
                        }
                        if (Calling.Debuff > 0)
                        {
                            var tee = DataDb.EffectDb.GetEffect(Calling.Debuff);
                            if (tee.GlobalDuration == TimeSpan.MaxValue)
                            {
                                effectNames.Add(tee);
                            }
                        }
                    }
                    break;
                case EffectTypes.Buff:
                    {
                        if (Spec.Buffs.Any())
                        {
                            Spec.Buffs.ForEach((x) =>
                            {
                                var tee = DataDb.EffectDb.GetEffect(x);
                                if (tee.GlobalDuration == TimeSpan.MaxValue)
                                {
                                    effectNames.Add(tee);
                                }
                            });
                        }
                        if (Archetype.Buffs.Any())
                        {
                            Archetype.Buffs.ForEach((x) =>
                            {
                                var tee = DataDb.EffectDb.GetEffect(x);
                                if (tee.GlobalDuration == TimeSpan.MaxValue)
                                {
                                    effectNames.Add(tee);
                                }
                            });
                        }
                        if (Calling.Buff > 0)
                        {
                            var tee = DataDb.EffectDb.GetEffect(Calling.Buff);
                            if (tee.GlobalDuration == TimeSpan.MaxValue)
                            {
                                effectNames.Add(tee);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception();
            }

            return effectNames;
        }

        public int GetMultipliedStat(StatTypes type, bool includeEquipment = true)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }

            // raw base stat
            int baseVal = Stats.GetStat(type);

            // add equipment
            if (includeEquipment)
            {
                ActiveSockets.ForEach((x) =>
                {
                    if (x.Stats.Stats.ContainsKey(type)) baseVal += x.Stats.GetStat(type);
                });
            }



            // multipliers
            // add  spec
            int basemult = Spec.Stats.GetStat(type);

            // buffs
            var ebuffs = GetPassiveEffectsByType(EffectTypes.Buff);
            foreach (var buff in ebuffs)
            {
                if (buff.Stats.Stats.ContainsKey(type)) basemult += buff.Stats.GetStat(type) - 100;
            }

            // arc / calling
            basemult += Archetype.Stats.GetStat(type) - 100;
            if (Calling.Stats.Stats.ContainsKey(type)) basemult += Calling.Stats.GetStat(type) - 100;
            double percentMult = basemult * .01f;
            percentMult = Math.Round(percentMult, 2);

            // deliver stat
            return Convert.ToInt32(Math.Floor(baseVal * percentMult));
        }
    }
}
