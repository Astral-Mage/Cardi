using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
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

        public BaseCustomization()
        {
            Name = string.Empty;
            Id = 0;
            Skills = new List<int>();
            Stats = new StatData();
            Description = string.Empty;
            RawString = string.Empty;
            Tags = new List<int>();
            Effects = new List<int>();
            Level = 1;
            Active = false;
        }

        public virtual string GetName(bool raw = false)
        {
            string tosend = string.Empty;
            string opener = "[color=white]⟪";
            string closer = "⟫[/color]";
            tosend += $"{opener} {Name} {closer}";
            return raw ? Name : tosend;
        }

        public string GetSkillString(bool includelinebreak = false)
        {
            string toreturn = "";

            if (Skills.Any())
            {
                toreturn += $"❇️ ";
                foreach (var s in Skills)
                {
                    toreturn += $"⟨ {DataDb.SkillsDb.GetSkill(s).Name} ⟩";
                }
                if (includelinebreak) toreturn += "\\n";
            }
            return toreturn;
        }

        public string GetEffectString(EffectTypes type, bool includelinebreak = false)
        {
            string toreturn = "⏫ ";
            string list = "";
            if (Effects.Any())
            {
                for (int x = 0; x < Effects.Count; x++)
                {
                    var eff = DataDb.EffectDb.GetEffect(Effects[x]);
                    if (eff.EffectType == type)
                        list += eff.GetShortDescription();
                }
            }
            if (includelinebreak && !string.IsNullOrWhiteSpace(list)) toreturn += "\\n";

            return (String.IsNullOrWhiteSpace(list)) ? string.Empty : toreturn + list;
        }

        public string GetStatString(bool includelinebreak = false)
        {
            string toreturn = "";
            if (Stats.Stats.Any())
            {
                foreach (var s in Stats.Stats)
                {
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

        public virtual string GetInfo()
        {
            string toreturn = $"                                 {GetName()}\\n\\n";
            toreturn += GetStatString(true);
            if (Stats.Stats.Any()) toreturn += "\\n";

            toreturn += GetEffectString(EffectTypes.Buff, true);
            toreturn += GetEffectString(EffectTypes.Debuff, true);

            toreturn += GetSkillString(true);
            toreturn += GetTagsString();

            return toreturn;
        }

        public static BaseCustomization ReadRawString(string str)
        {
            //-createarc name:pants+mcgee life:110 str:90 dex:120 con:90 int:120 wis:100 per:100 lib:90 cha:100 tui:100 skills:13,104 buffs:1 debuffs:9 specs:1,5
            try
            {
                Dictionary<string, string> splitspec = new Dictionary<string, string>();
                str.Split(" ".ToCharArray()).ToList().ForEach((x) =>
                {
                    var split = x.Split(":".ToCharArray());
                    splitspec.Add(split.First(), split.Last());
                });

                BaseCustomization arc = new Archetype();
                arc.RawString = str;
                arc.Name = splitspec["name"];
                arc.Name = arc.Name.Replace("+", " ");
                if (int.TryParse(splitspec["type"], out int parsedTypeAsInt))
                {
                    arc.Customization = (CustomizationTypes)parsedTypeAsInt;
                }
                else
                {
                    arc.Customization = (CustomizationTypes)Enum.Parse(typeof(CustomizationTypes), splitspec["type"], true);
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

                List<StatTypes> statsToCheck = new List<StatTypes>() { StatTypes.Life, StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution, StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception, StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition, StatTypes.Damage, StatTypes.Experience, StatTypes.Gold, StatTypes.Healing, StatTypes.Shield, StatTypes.Barrier, StatTypes.Speed };

                foreach (var stc in statsToCheck)
                {
                    if (splitspec.ContainsKey(stc.GetDescription())) arc.Stats.AddStat(stc, Convert.ToInt32(splitspec[stc.GetDescription()]));
                }

                return arc;
            }
            catch
            {
                return null;
            }

        }
    }
}