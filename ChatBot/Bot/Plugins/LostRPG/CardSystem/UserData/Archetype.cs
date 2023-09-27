using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Archetype
    {

        public string Name { get; set; }
        public int ArcId { get; set; }

        public StatData Stats { get; set; }

        public List<int> Skills { get; set; }
        public List<int> Buffs { get; set; }
        public List<int> Debuffs { get; set; }

        public string Description { get; set; }

        public string RawString { get; set; }

        public List<int> Tags { get; set; }

        public Archetype()
        {
            Name = string.Empty;
            ArcId = 0;
            Skills = new List<int>();
            Stats = new StatData();
            Description = string.Empty;
            RawString = string.Empty;
            Buffs = new List<int>();
            Debuffs = new List<int>();
            Tags = new List<int>();
        }

        public string GetInfo()
        {
            string toreturn = string.Empty;

            toreturn += $"{Name}" +
                $"\\n" +
                $"\\n{(Description == string.Empty ? "--- Blank Description ---" : Description)}" +
                $"\\n" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Life)}: {Stats.GetStat(StatTypes.Life)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Strength)}: {Stats.GetStat(StatTypes.Strength)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Dexterity)}: {Stats.GetStat(StatTypes.Dexterity)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Constitution)}: {Stats.GetStat(StatTypes.Constitution)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Intelligence)}: {Stats.GetStat(StatTypes.Intelligence)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Wisdom)}: {Stats.GetStat(StatTypes.Wisdom)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Perception)}: {Stats.GetStat(StatTypes.Perception)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Libido)}: {Stats.GetStat(StatTypes.Libido)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Charisma)}: {Stats.GetStat(StatTypes.Charisma)}%" +
                $"\\n{Enum.GetName(typeof(StatTypes), StatTypes.Intuition)}: {Stats.GetStat(StatTypes.Intuition)}%" +
                $"\\n";

            List<string> tlist = new List<string>();
            foreach (var tag in Tags)
            {
                tlist.Add(DataDb.TagsDb.GetTag(tag).Name);
            }
            toreturn += $"Tags: {string.Join(" • ", tlist)}";

            if (Buffs.Count > 0) toreturn += $"\\n⏫ ";
            foreach (var s in Buffs)
            {
                toreturn += $"⟨ {DataDb.EffectDb.GetEffect(s).Name} ⟩";
            }

            if (Debuffs.Count > 0) toreturn += $"\\n⏬ ";
            foreach (var s in Debuffs)
            {
                toreturn += $"⟨ {DataDb.EffectDb.GetEffect(s).Name} ⟩";
            }


            if (Skills.Count > 0) toreturn += $"\\n❇️ ";
            foreach (var s in Skills)
            {
                toreturn += $"⟨ {DataDb.SkillsDb.GetSkill(s).Name} ⟩";
            }

            return toreturn;
        }

        public static Archetype ReadRawString(string str)
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

                Archetype arc = new Archetype();
                arc.RawString = str;
                arc.Name = splitspec["name"];
                arc.Name = arc.Name.Replace("+", " ");
                if (splitspec.ContainsKey("skills")) splitspec["skills"].Split(",".ToCharArray()).ToList().ForEach(x => arc.Skills.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("buffs")) splitspec["buffs"].Split(",".ToCharArray()).ToList().ForEach(x => arc.Buffs.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("debuffs")) splitspec["debuffs"].Split(",".ToCharArray()).ToList().ForEach(x => arc.Debuffs.Add(Convert.ToInt32(x)));

                var allTags = DataDb.TagsDb.GetAllTags();
                var toaddtags = splitspec["tags"].Split(",".ToCharArray()).ToList();
                
                foreach(var s in toaddtags)
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

                if (arc.Tags.Count != toaddtags.Count)
                {
                    return null;
                }

                arc.Stats.AddStat(StatTypes.Life, Convert.ToInt32(splitspec[StatTypes.Life.GetDescription()]));

                arc.Stats.AddStat(StatTypes.Strength, Convert.ToInt32(splitspec[StatTypes.Strength.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Dexterity, Convert.ToInt32(splitspec[StatTypes.Dexterity.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Constitution, Convert.ToInt32(splitspec[StatTypes.Constitution.GetDescription()]));

                arc.Stats.AddStat(StatTypes.Intelligence, Convert.ToInt32(splitspec[StatTypes.Intelligence.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Wisdom, Convert.ToInt32(splitspec[StatTypes.Wisdom.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Perception, Convert.ToInt32(splitspec[StatTypes.Perception.GetDescription()]));

                arc.Stats.AddStat(StatTypes.Libido, Convert.ToInt32(splitspec[StatTypes.Libido.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Charisma, Convert.ToInt32(splitspec[StatTypes.Charisma.GetDescription()]));
                arc.Stats.AddStat(StatTypes.Intuition, Convert.ToInt32(splitspec[StatTypes.Intuition.GetDescription()]));

                return arc;
            }
            catch
            {
                return null;
            }

        }
    }
}