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
        public List<Effect> SkillEffects { get; set; }
        public string Description { get; set; }

        public int Stamina { get; set; }

        public int Cost { get; set; }
        public int SkillId { get; set; }

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
            SkillEffects = new List<Effect>();
            Description = string.Empty;
            Stamina = 0;
            Cost = 0;
            SkillId = 0;
        }

        public static Skill ReadRawString(string rawstring)
        {
            // -cs level:1 damage:160 dex:140 str:120 buffs:1 debuffs: 3 speed:90 charge:2 charges:1 tags:Weapon reaction:0 target:enemy cd:3
            Skill newSkill = new Skill();

            List<string> slashed = rawstring.Split('\\').ToList();
            Dictionary<string, string> brokenSkill = new Dictionary<string, string>();
            foreach (var v in slashed)
            {
                var tsplit = v.Split(" ".ToCharArray(), 2).ToList();
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
                newSkill.Level = Convert.ToInt32(brokenSkill[RequiredSkillTags.level.ToString()]);
                newSkill.Name = brokenSkill[RequiredSkillTags.name.ToString()].TrimEnd();
                newSkill.Speed = Convert.ToInt32(brokenSkill[RequiredSkillTags.speed.ToString()]);
                newSkill.Tags = brokenSkill[RequiredSkillTags.tags.ToString()].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                var effects = brokenSkill[RequiredSkillTags.effects.ToString()];
                var brokenEffects = effects.Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                // do skill effect stuff
                Console.WriteLine("TODO: Skill Effect Stuff :TODO");
                newSkill.SkillEffects = new List<Effect>();
                foreach (var eff in brokenEffects)
                {
                    Effect se = new Effect();
                }
            }
            catch
            {
                return null;
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

            return newSkill;
        }
    }
}
