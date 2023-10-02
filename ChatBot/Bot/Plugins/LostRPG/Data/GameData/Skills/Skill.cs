using Accord;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data.GameData
{
    [Serializable]
    public class Skill
    {
        public string Name { get; set; }

        public int Level { get; set; }

        public bool Reaction { get; set; }

        public List<string> Tags { get; set; }

        public string RawStr { get; set; }

        public int Speed { get; set; }

        public List<int> SkillEffects { get; set; }

        public string Description { get; set; }

        public int Stamina { get; set; }

        public int Cost { get; set; }

        public int SkillId { get; set; }

        public StatData Stats { get; set; }

        public int MaxCharges { get; set; }

        public TimeSpan Cooldown { get; set; }

        SkillDetails UserSkillDetails { get; set; }

        public Skill(string rawStr)
        {
            Setup();
            RawStr = rawStr;
        }

        public Skill()
        {
            Setup();
        }

        private void Setup()
        {
            Name = string.Empty;
            Level = 0;
            Reaction = false;
            Tags = new List<string>();
            RawStr = string.Empty;
            Speed = 100;
            SkillEffects = new List<int>();
            Description = string.Empty;
            Stamina = 0;
            Cost = 0;
            SkillId = 0;
            Stats = new StatData();
            Cooldown = new TimeSpan();
            MaxCharges = 1;
            UserSkillDetails = new SkillDetails();
        }

        public string GetShortDescription()
        {
            return $"⟨ {Name} ⟩";
        }

        public TimeSpan GetRemainingCooldown()
        {
            if (Cooldown == TimeSpan.MinValue) return Cooldown;

            DateTime now = DateTime.Now;
            TimeSpan leftover = now - UserSkillDetails.lastUse;

            if ((Cooldown - leftover).TotalMilliseconds <= 0) return new TimeSpan(0, 0, 0);
            return (Cooldown - leftover);
        }

        public string GetChargesString()
        {
            string pants = "";
            if (UserSkillDetails.currentCharges == 0) pants = $"[color=red]{UserSkillDetails.currentCharges}[/color]";
            else pants = $"{UserSkillDetails.currentCharges}";

            return $"{pants}/{MaxCharges}";
        }

        public void SetUserSkillDetails(SkillDetails sd)
        {
            UserSkillDetails = sd;
        }

        public SkillDetails GetUserSkillDetails()
        {
            return UserSkillDetails;
        }

        public static Skill ReadRawString(string rawstring)
        {
            // -createskill level:1 effects:17 con:120 speed:100 tags:Weapon reaction:0 target:self name:Ice+Wall reaction
            Skill newSkill = new Skill();

            List<string> slashed = rawstring.Split(" ".ToCharArray()).ToList();
            Dictionary<string, string> brokenSkill = new Dictionary<string, string>();
            foreach (var v in slashed)
            {
                var tsplit = v.Split(":".ToCharArray(), 2).ToList();
                if (!brokenSkill.ContainsKey(tsplit.First()))
                {
                    brokenSkill.Add(tsplit.First(), tsplit.Last());
                }
                else
                {
                    brokenSkill[tsplit.First()] = tsplit.Last();
                }
            }

            // required checks
            try
            {
                if (brokenSkill.ContainsKey("level")) newSkill.Level = Convert.ToInt32(brokenSkill[RequiredSkillTags.level.ToString()]);
                else newSkill.Level = 1;
                newSkill.Name = brokenSkill[RequiredSkillTags.name.ToString()].TrimEnd();
                newSkill.Name = newSkill.Name.Replace("+", " ");
                if (brokenSkill.ContainsKey(RequiredSkillTags.speed.ToString())) newSkill.Speed = Convert.ToInt32(brokenSkill[RequiredSkillTags.speed.ToString()]);
                else newSkill.Speed = 100;
                if (brokenSkill.ContainsKey("tags")) newSkill.Tags = brokenSkill[RequiredSkillTags.tags.ToString()].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                var effects = (brokenSkill.ContainsKey(RequiredSkillTags.effects.ToString()) ? brokenSkill[RequiredSkillTags.effects.ToString()] : null);
                if (effects != null)
                {
                    var brokenEffects = effects.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                    // do skill effect stuff
                    newSkill.SkillEffects = new List<int>();
                    foreach (var eff in brokenEffects)
                    {
                        newSkill.SkillEffects.Add(Convert.ToInt32(eff));
                    }
                }

            }
            catch
            {
                return null;
            }

            // stats
            List<StatTypes> slist = new List<StatTypes>() { StatTypes.Damage, StatTypes.Healing, StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution, StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception, StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition, StatTypes.Life, StatTypes.Shield };
            foreach (var v in slist)
            {
                if (brokenSkill.ContainsKey(v.GetDescription()))
                {
                    newSkill.Stats.AddStat(v, Convert.ToInt32(brokenSkill[v.GetDescription()]));
                }
            }

            // optional checks
            if (brokenSkill.ContainsKey(OptionalSkillTags.reaction.ToString()))
            {
                newSkill.Reaction = true;
            }

            if (brokenSkill.ContainsKey(OptionalSkillTags.description.ToString()))
            {
                newSkill.Description = brokenSkill[OptionalSkillTags.description.ToString()];
                if (newSkill.Description.Count(x => x == '"') > 2)
                {
                    return null;
                }
                newSkill.Description = newSkill.Description.Replace("\"", "");
            }

            if (brokenSkill.ContainsKey("cd"))
            {
                int dur = Convert.ToInt32(brokenSkill["cd"]);
                if (dur == 0) newSkill.Cooldown = new TimeSpan(0, 0, 0);
                else if (dur == 1) newSkill.Cooldown = new TimeSpan(0, 30, 0);
                else if (dur == 2) newSkill.Cooldown = new TimeSpan(2, 0, 0);
                else if (dur == 3) newSkill.Cooldown = new TimeSpan(8, 0, 0);
                else if (dur == 4) newSkill.Cooldown = new TimeSpan(1, 0, 0, 0);
                else if (dur == 5) newSkill.Cooldown = new TimeSpan(3, 0, 0, 0);
                else newSkill.Cooldown = new TimeSpan(7, 0, 0, 0);
            }
            else
            {
                newSkill.Cooldown = new TimeSpan(2, 0, 0);
            }

            if (brokenSkill.ContainsKey("cmax"))
            {
                newSkill.MaxCharges = Convert.ToInt32(brokenSkill["cmax"]);
            }

            return newSkill;
        }
    }
}
