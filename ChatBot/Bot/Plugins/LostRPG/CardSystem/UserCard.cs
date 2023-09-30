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

            foreach (var v in ActiveCustomizations)
            {
                toReturn.Add(DataDb.CustomDb.GetCustomizationById(v.cid));
            }

            return toReturn;
        }

        public void SetStats(StatData stats)
        {
            Stats = stats;
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
            foreach (var sk in GetActiveCustomizationByType(CustomizationTypes.Calling).Skills) toReturn.Add(DataDb.SkillsDb.GetSkill(sk));
            foreach (var sk in GetActiveCustomizationByType(CustomizationTypes.Archetype).Skills) toReturn.Add(DataDb.SkillsDb.GetSkill(sk));
            foreach (var sk in GetActiveCustomizationByType(CustomizationTypes.Specialization).Skills) toReturn.Add(DataDb.SkillsDb.GetSkill(sk));
            foreach (var sk in Skills) toReturn.Add(DataDb.SkillsDb.GetSkill(sk));
            return toReturn;
        }

        public int GetStat(StatTypes type)
        {
            if (!Stats.Stats.ContainsKey(type))
            {
                Stats.Stats.Add(type, 0);
            }
            return Convert.ToInt32(Math.Floor(Stats.Stats[type]));
        }

        public List<Effect> GetActiveEffectByType(EffectTypes types)
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
                    eNames.Add(eft);
                    eft.UserDetails = ae;
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

        public List<Effect> GetPassiveEffectsByType(EffectTypes type)
        {
            List<Effect> effectNames = new List<Effect>();

            List<Effect> ListEffects = DataDb.EffectDb.GetAllEffectsByType(type);
            GetActiveCustomizationByType(CustomizationTypes.Archetype).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

            GetActiveCustomizationByType(CustomizationTypes.Specialization).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

            GetActiveCustomizationByType(CustomizationTypes.Calling).Effects.ForEach((x) =>
            {
                foreach (var eff in ListEffects)
                {
                    if (eff.EffectId == x && eff.Duration == TimeSpan.MaxValue)
                    {
                        effectNames.Add(eff);
                    }
                }
            });

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
            int basemult = 0;

            if (GetActiveCustomizationByType(CustomizationTypes.Specialization).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Specialization).Stats.GetStat(type);
            if (GetActiveCustomizationByType(CustomizationTypes.Calling).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Calling).Stats.GetStat(type);
            if (GetActiveCustomizationByType(CustomizationTypes.Archetype).Stats.Stats.ContainsKey(type)) basemult += GetActiveCustomizationByType(CustomizationTypes.Archetype).Stats.GetStat(type);


            // buffs
            var ebuffs = GetPassiveEffectsByType(EffectTypes.Buff);
            foreach (var buff in ebuffs)
            {
                if (buff.Stats.Stats.ContainsKey(type)) basemult += buff.Stats.GetStat(type);
            }

            // debuffs
            var debuffs = GetPassiveEffectsByType(EffectTypes.Debuff);
            var db2 = GetActiveEffectByType(EffectTypes.Debuff);
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
