using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.Data.GameData;
using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    [Serializable]
    public class BaseCustomization
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public StatData Stats { get; set; }

        public List<int> Skills { get; set; }

        public List<int> Effects { get; set; }

        public string Description { get; set; }

        public string RawString { get; set; }

        public List<int> Tags { get; set; }

        public CustomizationTypes Customization { get; set; }

        public bool Active { get; set; }

        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public BaseCustomization()
        {
            Name = string.Empty;
            Id = -1;
            Skills = new List<int>();
            Stats = new StatData();
            Description = string.Empty;
            RawString = string.Empty;
            Tags = new List<int>();
            Effects = new List<int>();
            Level = 1;
            Active = false;
            MaxLevel = 3;
        }

        public virtual string GetName(bool raw = false)
        {
            string tosend = string.Empty;
            string color = "white";

            if (Customization == CustomizationTypes.Specialization)
            {
                if ((DamageTypes)Stats.GetStat(StatTypes.DamageType) == DamageTypes.Physical) color = "brown";
                else if ((DamageTypes)Stats.GetStat(StatTypes.DamageType) == DamageTypes.Magic) color = "cyan";
                else if ((DamageTypes)Stats.GetStat(StatTypes.DamageType) == DamageTypes.Lust) color = "pink";
            }

            string opener = $"[b][color={color}]⟪";
            string closer = "⟫[/color][/b]";
            tosend += $"{opener} {Name} {closer}";
            return raw ? Name : tosend;
        }

        public string GetSkillString(bool includelinebreak = false, int leveloverride = 1)
        {
            string toreturn = "";
            string symbol = "❇️";
            int levelToUse = (leveloverride == 0) ? Level : leveloverride;
            List<Skill> skillsToUse = new List<Skill>();

            foreach (var skill in Skills)
            {
                var sk = DataDb.SkillsDb.GetSkill(skill);
                skillsToUse.Add(sk);
            }


            for (int x = 1; x <= levelToUse; x++)
            {
                if (skillsToUse.Any(y => y.Level == x))
                {
                    for (int z = 1; z <= x; z++) toreturn += symbol;
                    toreturn += " ";
                }

                foreach (var ef in skillsToUse.Where(y => y.Level == x)) toreturn += ef.GetShortDescription() + " ";

                if (skillsToUse.Any(y => y.Level == x))
                {
                    if (x == levelToUse && includelinebreak || x < levelToUse) toreturn += "\\n";
                }
            }
            return toreturn;
        } 

        public string GetSkills()
        {
            string toreturn = string.Empty;
            foreach (var s in Skills)
            {
                var sk = DataDb.SkillsDb.GetSkill(s);
                if (sk.Level <= Level) toreturn += $"⟨ {sk.Name} ⟩ ";

            }
            return toreturn;
        }

        public string GetEffectString(EffectTypes type, bool includelinebreak = false, int leveloverride = 0)
        {
            string toSend = "";
            string buff = "⏫";
            string debuff = "⏬";
            string symbol = (type == EffectTypes.Buff) ? buff : debuff;
            int levelToUse = (leveloverride == 0) ? Level : leveloverride;
            List<Effect> effectsToUse = new List<Effect>();
            foreach(var e in Effects)
            {
                Effect ef = DataDb.EffectDb.GetEffect(e);
                if (ef.EffectType == type) effectsToUse.Add(ef);
            }

            for (int x = 1; x <= levelToUse; x++)
            {
                if (effectsToUse.Any(y => y.Level == x))
                {
                    for (int z = 1; z <= x; z++) toSend += symbol;
                    toSend += " ";
                }

                foreach (var ef in effectsToUse.Where(y => y.Level == x)) toSend += ef.GetShortDescription() + " ";

                if (effectsToUse.Any(y => y.Level == x))
                {
                    if  (x == levelToUse && includelinebreak || x < levelToUse) toSend += "\\n";
                }
            }

            return toSend;

        }

        public DamageTypes GetDamageType()
        {
            if (Customization != CustomizationTypes.Specialization) return DamageTypes.None;
            return (DamageTypes)Stats.GetStat(StatTypes.DamageType);
        }

        public string GetStatString(bool includelinebreak = false)
        {
            string toreturn = "";
            if (Stats.Stats.Any())
            {
                foreach (var s in Stats.Stats)
                {
                    if (s.Key == StatTypes.DamageType) continue;
                    if (s.Value > 0) toreturn += $"{s.Key}: [color=green][sup]↑[/sup]{s.Value}%[/color]";
                    else if (s.Value == 0) toreturn += "";
                    else toreturn += $"{s.Key}: [color=red][sub]↓[/sub]{s.Value}%[/color]";
                    if (Stats.Stats.Last().Key != s.Key) toreturn += "  ";
                }
                if (includelinebreak) toreturn += "\\n";
            }

            return toreturn;
        }

        public string GetTagsString(bool includelinebreak = false)
        {
            string toreturn = "";
            if (Tags.Any())
            {
                List<string> tlist = new List<string>();
                foreach (var tag in Tags)
                {
                    tlist.Add(DataDb.TagsDb.GetTag(tag).Name);
                }
                toreturn += $"Tags: {string.Join(" • ", tlist)}";
            }
            if (includelinebreak) toreturn += "\\n";
            return toreturn;
        }

        public virtual string GetInfo(int levelOverride = 0)
        {
            string toreturn = $"                                 {GetName()}\\n";
            toreturn += GetStatString(true);

            if (levelOverride == 0)
            {
                toreturn += GetEffectString(EffectTypes.Buff, true);
                toreturn += GetEffectString(EffectTypes.Debuff, true);

                toreturn += GetSkillString(false, 4);
                toreturn += GetTagsString();
            }
            else
            {
                toreturn += GetEffectString(EffectTypes.Buff, true, levelOverride);
                toreturn += GetEffectString(EffectTypes.Debuff, true, levelOverride);

                toreturn += GetSkillString(false, levelOverride);
                toreturn += GetTagsString();
            }

            return toreturn;
        }

        public static BaseCustomization ReadRawString(string str, out string msg)
        {
            try
            {
                // parse
                List<string> missedTags = new List<string>();
                Dictionary<string, string> splitspec = new Dictionary<string, string>();
                str.Split(" ".ToCharArray()).ToList().ForEach((x) =>
                {
                    var split = x.Split(":".ToCharArray());
                    splitspec.Add(split.First(), split.Last());
                });

                // start checking tags
                BaseCustomization arc = new BaseCustomization();
                arc.RawString = str;

                if (splitspec.ContainsKey("name"))
                {
                    arc.Name = splitspec["name"];
                    arc.Name = arc.Name.Replace("+", " ");
                }
                else missedTags.Add("name");

                if (splitspec.ContainsKey("type"))
                {
                    if (int.TryParse(splitspec["type"], out int parsedTypeAsInt))
                    {
                        arc.Customization = (CustomizationTypes)parsedTypeAsInt;
                    }
                    else
                    {
                        arc.Customization = (CustomizationTypes)Enum.Parse(typeof(CustomizationTypes), splitspec["type"], true);
                    }
                }
                else missedTags.Add("type");


                if (arc.Customization == CustomizationTypes.Specialization)
                {
                    if (splitspec.ContainsKey("dtype"))
                    {
                        if (int.TryParse(splitspec["dtype"], out int parsedTypeAsInt))
                        {
                            arc.Stats.SetStat(StatTypes.DamageType, parsedTypeAsInt);
                        }
                        else
                        {
                            arc.Stats.SetStat(StatTypes.DamageType, (int)Enum.Parse(typeof(DamageTypes), splitspec["dtype"], true));
                        }
                    }
                    else missedTags.Add("dtype");
                }

                if (splitspec.ContainsKey("skills")) splitspec["skills"].Split(",".ToCharArray()).ToList().ForEach(x => arc.Skills.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("effects")) splitspec["effects"].Split(",".ToCharArray()).ToList().ForEach(x => arc.Effects.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("tags"))
                {
                    var allTags = DataDb.TagsDb.GetAllTags();
                    var toaddtags = splitspec["tags"].Split(",".ToCharArray()).ToList();

                    foreach (var s in toaddtags)
                    {
                        bool found = false;
                        foreach (var ta in allTags)
                        {
                            if (ta.Name.ToLowerInvariant().Equals(s.ToLowerInvariant()))
                            {
                                found = true;
                                arc.Tags.Add(ta.TagId);
                            }

                            if (found) break;
                        }
                    }
                }

                List<StatTypes> statsToCheck = new List<StatTypes>() { StatTypes.Life, StatTypes.Lust, StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution, StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception, StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition, StatTypes.Damage, StatTypes.Experience, StatTypes.Gold, StatTypes.Healing, StatTypes.Shield, StatTypes.Barrier, StatTypes.Speed };

                foreach (var stc in statsToCheck)
                {
                    if (splitspec.ContainsKey(stc.GetDescription())) arc.Stats.AddStat(stc, Convert.ToInt32(splitspec[stc.GetDescription()]));
                }

                if (missedTags.Any())
                {
                    msg = "Missed Tags: " + string.Join(", ", missedTags);
                    return null;
                }

                msg = string.Empty;
                return arc;
            }
            catch
            {
                msg = "Parsing failure on type or dtype.";
                return null;
            }
        }
    }
}