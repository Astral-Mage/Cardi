using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Specialization
    {

        public string Name { get; set; }

        public int SpecId { get; set; }

        public string Description { get; set; }

        public List<int> Tags { get; set; }

        public List<int> Buffs { get; set; }

        public List<int> Debuffs { get; set; }

        public List<int> Skills { get; set; }

        public string RawString { get; set; }

        public StatData Stats {get; set;}

        public Specialization()
        {
            Name = string.Empty;
            SpecId = 0;
            Description = string.Empty;
            Tags = new List<int>();
            Skills = new List<int>();
            Buffs = new List<int>();
            Debuffs = new List<int>();
            Stats = new StatData();
            RawString = string.Empty;
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

            if (Buffs.Count > 0) toreturn += $"\\nBuffs: {string.Join(" • ", Buffs)}";
            if (Debuffs.Count > 0) toreturn += $"\\nDebuffs: {string.Join(" • ", Debuffs)}";
            if (Skills.Count > 0) toreturn += $"\\nSkills: {string.Join(" • ", Skills)}";

            return toreturn;
        }

        public static Specialization ReadRawString(string str)
        {
            // -createspec name:fire+boy life:110 str:90 dex:120 con:90 int:120 wis:100 per:100 lib:90 cha:100 tui:100 skills:13,104 buffs:1 debuffs:9 tags:3,4
            try
            {
                Dictionary<string, string> splitspec = new Dictionary<string, string>();
                str.Split(" ".ToCharArray()).ToList().ForEach((x) =>
                {
                    var split = x.Split(":".ToCharArray());
                    splitspec.Add(split.First(), split.Last());
                });

                Specialization newspec = new Specialization();
                newspec.RawString = str;
                newspec.Name = splitspec["name"];
                newspec.Name = newspec.Name.Replace("+", " ");

                var taglist = DataDb.TagsDb.GetAllTags();
                if (splitspec.ContainsKey("tags"))
                {
                    var tlist = splitspec["tags"].Split(",".ToCharArray()).ToList();
                    List<string> newTags = new List<string>();
                    foreach (var tag in tlist)
                    {
                        if (Int32.TryParse(tag, out int itag))
                        {
                            if (taglist.Any(x => x.TagId == Convert.ToInt32(tag)))
                            {
                                newspec.Tags.Add(Convert.ToInt32(tag));
                            }
                            else
                            {
                                newTags.Add(tag);
                            }
                        }
                        else if (taglist != null && taglist.Any(x => x.Name.ToLowerInvariant().Equals(tag.ToLowerInvariant())))
                        {

                            newspec.Tags.Add(taglist.First(x => x.Name.ToLowerInvariant().Equals(tag.ToLowerInvariant())).TagId);
                        }
                        else
                        {
                            newTags.Add(tag);
                        }
                    }
                    if (newTags.Any())
                    {
                        return null;
                    }
                }
                else
                {
                    throw new Exception("Bad tags");
                }

                if (splitspec.ContainsKey("skills")) splitspec["skills"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Skills.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("buffs")) splitspec["buffs"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Buffs.Add(Convert.ToInt32(x)));
                if (splitspec.ContainsKey("debuffs")) splitspec["debuffs"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Debuffs.Add(Convert.ToInt32(x)));

                newspec.Stats.AddStat(StatTypes.Life, Convert.ToInt32(splitspec[StatTypes.Life.GetDescription()]));

                newspec.Stats.AddStat(StatTypes.Strength, Convert.ToInt32(splitspec[StatTypes.Strength.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Dexterity, Convert.ToInt32(splitspec[StatTypes.Dexterity.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Constitution, Convert.ToInt32(splitspec[StatTypes.Constitution.GetDescription()]));

                newspec.Stats.AddStat(StatTypes.Intelligence, Convert.ToInt32(splitspec[StatTypes.Intelligence.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Wisdom, Convert.ToInt32(splitspec[StatTypes.Wisdom.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Perception, Convert.ToInt32(splitspec[StatTypes.Perception.GetDescription()]));

                newspec.Stats.AddStat(StatTypes.Libido, Convert.ToInt32(splitspec[StatTypes.Libido.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Charisma, Convert.ToInt32(splitspec[StatTypes.Charisma.GetDescription()]));
                newspec.Stats.AddStat(StatTypes.Intuition, Convert.ToInt32(splitspec[StatTypes.Intuition.GetDescription()]));

                return newspec;
            }
            catch
            {
                return null;
            }
            
        }
    }
}
