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

        public List<CustomizationDetails> ActiveCustomizations { get; set; }

        public List<EffectDetails> ActiveEffects { get; set; }

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
            ActiveSockets = new List<Socket>();
            ActiveEffects = new List<EffectDetails>();
            ActiveCustomizations = new List<CustomizationDetails>();
        }

        public List<BaseCustomization> GetActiveCustomizations()
        {
            List<BaseCustomization> toReturn = new List<BaseCustomization>();

            foreach (var v in ActiveCustomizations.Where(x => x.isactive))
            {
                toReturn.Add(DataDb.CustomDb.GetCustomizationById(v.cid));
            }

            return toReturn;
        }

        public void SetStats(StatData stats)
        {
            Stats = stats;
        }

        public void Restore()
        {
            Stats.SetStat(StatTypes.CurrentLife, GetMultipliedStat(StatTypes.Life));
            Stats.SetStat(StatTypes.CurrentLust, 0);
            Stats.SetStat(StatTypes.CurrentLust, 0);


            List<EffectDetails> tokill = new List<EffectDetails>();
            foreach (var ae in ActiveEffects)
            {
                if (ae.EffectType == EffectTypes.Debuff) tokill.Add(ae);
            }
            tokill.ForEach(x => ActiveEffects.Remove(x));
            DataDb.CardDb.UpdateUserCard(this);
        }

        public DamageTypes GetDamageType()
        {
            return GetActiveCustomizationByType(CustomizationTypes.Specialization).GetDamageType();
        }

        public bool SetActiveCustomization(BaseCustomization oldcustom, BaseCustomization newcustom)
        {

            bool foundhistory = false;
            bool diddisable = false;
            foreach (var ac in ActiveCustomizations)
            {
                if (ac.cid == newcustom.Id)
                {
                    ac.isactive = true;
                    foundhistory = true;
                }

                if (ac.cid == oldcustom.Id)
                {
                    ac.isactive = false;
                    diddisable = true;
                }
            }

            if (!foundhistory)
            {
                ActiveCustomizations.Add(new CustomizationDetails() { cid = newcustom.Id, currentlevel = 1, isactive = true });
            }


            return diddisable;
        }

        public BaseCustomization GetActiveCustomizationByType(CustomizationTypes type)
        {
            foreach (var v in ActiveCustomizations)
            {
                if (v.isactive)
                {
                    var tc = DataDb.CustomDb.GetCustomizationById(v.cid);
                    if (tc.Customization == type) return tc;
                }
            }
            return null;
        }

        public List<Skill> GetUsableSkills()
        {
            List<Skill> toReturn = new List<Skill>();

            foreach (var sk in GetActiveCustomizations())
            {
                if (sk.Skills.Any())
                {
                    sk.Skills.ForEach((x) =>
                    {
                        var skill = DataDb.SkillsDb.GetSkill(x);
                        if (skill.Level <= GetStat(StatTypes.Level))
                        {
                            //skill.SetUserSkillDetails(skill);
                            toReturn.Add(skill);
                        }
                    });
                }





            }

            foreach (var sk in Skills)
            {
                var skill = DataDb.SkillsDb.GetSkill(sk);
                if (skill.Level <= GetStat(StatTypes.Level))
                    toReturn.Add(skill);
            }
            return toReturn;
        }

        public int GetTotalStatMultiplier(StatTypes type)
        {
            int toreturn = 0;
            foreach(var cus in GetActiveCustomizations())
            {
                if (cus.Stats.Stats.ContainsKey(type))
                    toreturn += cus.Stats.GetStat(type);
            }
            return toreturn;
        }

        public int GetStat(StatTypes type)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }
            return Convert.ToInt32(Math.Floor(Stats.Stats[type]));
        }

        public List<Effect> GetActiveEffectByType(EffectTypes types, int level = 1)
        {
            List<Effect> eNames = new List<Effect>();
            List<EffectDetails> toRemove = new List<EffectDetails>();
            foreach (var ae in ActiveEffects)
            {
                if (ae.GetRemainingDuration().TotalMilliseconds <= 0)
                {
                    toRemove.Add(ae);
                }
                else if (ae.EffectType == types)
                {
                    var eft = DataDb.EffectDb.GetEffect(ae.eid);
                    if (eft.Level <= level)
                    {
                        eNames.Add(eft);
                        eft.UserDetails = ae;
                    }
                }
            }

            if (toRemove.Any())
            {
                foreach (var ae in toRemove)
                {
                    ActiveEffects.Remove(ae);
                }
                DataDb.CardDb.UpdateUserCard(this);
            }

            return eNames;
        }

        public List<Effect> GetPassiveEffectsByType(EffectTypes type, int level = 1)
        {
            List<Effect> effectNames = new List<Effect>();
            List<Effect> ListEffects = DataDb.EffectDb.GetAllEffectsByType(type);


            GetActiveCustomizationByType(CustomizationTypes.Archetype).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue && level >= eff.Level)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

            GetActiveCustomizationByType(CustomizationTypes.Specialization).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue && level >= eff.Level)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

            GetActiveCustomizationByType(CustomizationTypes.Calling).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue && level >= eff.Level)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

            return effectNames;
        }

        public int GetMultipliedStat(StatTypes type, bool includeEquipment = true, Skill skillToAdd = null)
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
            int basemult = 0;

            if (GetActiveCustomizationByType(CustomizationTypes.Specialization).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Specialization).Stats.GetStat(type);
            if (GetActiveCustomizationByType(CustomizationTypes.Calling).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Calling).Stats.GetStat(type);
            if (GetActiveCustomizationByType(CustomizationTypes.Archetype).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Archetype).Stats.GetStat(type);
            if (skillToAdd != null && skillToAdd.Stats.Stats.ContainsKey(type)) basemult += skillToAdd.Stats.GetStat(type);

            // buffs
            var ebuffs = GetPassiveEffectsByType(EffectTypes.Buff);
            var db2 = GetActiveEffectByType(EffectTypes.Buff);
            db2.ForEach(x => ebuffs.Add(x));
            foreach (var buff in ebuffs)
            {
                if (buff.Stats.Stats.ContainsKey(type)) basemult += buff.Stats.GetStat(type);
            }

            // debuffs
            var debuffs = GetPassiveEffectsByType(EffectTypes.Debuff);
            db2 = GetActiveEffectByType(EffectTypes.Debuff);
            db2.ForEach(x => debuffs.Add(x));
            foreach (var debuff in debuffs)
            {
                if (debuff.Stats.Stats.ContainsKey(type)) basemult += debuff.Stats.GetStat(type);
            }

            if (basemult < -100) basemult = 0;

            double percentMult = basemult * .01f;
            percentMult = Math.Round(percentMult, 2);

            // deliver stat
            return Convert.ToInt32(Math.Floor(baseVal * (1 + percentMult)));
        }
    }
}
