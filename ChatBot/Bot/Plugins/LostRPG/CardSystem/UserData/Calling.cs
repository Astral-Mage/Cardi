using Accord;
using ChatBot.Bot.Plugins.LostRPG.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData
{
    public class Calling
    {

        public string Name { get; set; }

        public int CallingId { get; set; }

        public string Description { get; set; }

        public List<int> Tags { get; set; }

        public int Buff { get; set; }

        public int Debuff { get; set; }

        public int Skill { get; set; }

        public string RawString { get; set; }

        public StatData Stats { get; set; }

        public Calling()
        {
            Name = string.Empty;
            CallingId = 0;
            Description = string.Empty;
            Tags = new List<int>();
            Skill = -1;
            Buff = -1;
            Debuff = -1;
            Stats = new StatData();
            RawString = string.Empty;
        }

        public string GetInfo()
        {
            string toreturn = string.Empty;


            toreturn += $"{Name}" +
                $"\\n" +
                $"\\n{(Description == string.Empty ? "--- Blank Description ---" : Description)}" +
                $"\\n";

            if (Stats.Stats.Any())
            {
                StatTypes st = Stats.Stats.First().Key;
                toreturn += $"\\n{Enum.GetName(typeof(StatTypes), st)}: {Stats.GetStat(st)}%" +
                $"\\n";
            }

            if (Tags.Any())
            {
                List<string> tlist = new List<string>();
                foreach (var tag in Tags)
                {
                    tlist.Add(DataDb.TagsDb.GetTag(tag).Name);
                }
                toreturn += $"Tags: {string.Join(" • ", tlist)}";
            }

            if (Buff > -1) toreturn += $"\\nBuff: ‹ {DataDb.EffectDb.GetEffect(Buff).Name} ›";
            if (Debuff > -1) toreturn += $"\\nDebuff: ‹ {DataDb.EffectDb.GetEffect(Debuff).Name} ›";
            if (Skill > -1) toreturn += $"\\nSkill: ‹ {DataDb.SkillsDb.GetSkill(Skill).Name} ›";

            return toreturn;
        }

        public static Calling ReadRawString(string str)
        {
            // -cc name:of+pants tag:3 tui:100
            try
            {
                Dictionary<string, string> splitspec = new Dictionary<string, string>();
                str.Split(" ".ToCharArray()).ToList().ForEach((x) =>
                {
                    var split = x.Split(":".ToCharArray());
                    splitspec.Add(split.First(), split.Last());
                });

                Calling newspec = new Calling();
                newspec.RawString = str;
                newspec.Name = splitspec["name"];
                newspec.Name = newspec.Name.Replace("+", " ");

                var taglist = DataDb.TagsDb.GetAllTags();
                if (splitspec.ContainsKey("tag"))
                {
                    var tlist = splitspec["tag"].Split(",".ToCharArray()).ToList();
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
                    //throw new Exception("Bad tags");
                }

                if (splitspec.ContainsKey("skill")) splitspec["skill"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Skill = Convert.ToInt32(x));
                if (splitspec.ContainsKey("buff")) splitspec["buff"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Buff = Convert.ToInt32(x));
                if (splitspec.ContainsKey("debuff")) splitspec["debuff"].Split(",".ToCharArray()).ToList().ForEach(x => newspec.Debuff = Convert.ToInt32(x));

                List<StatTypes> sList = new List<StatTypes>() { StatTypes.Strength, StatTypes.Dexterity, StatTypes.Constitution, StatTypes.Intelligence, StatTypes.Wisdom, StatTypes.Perception, StatTypes.Libido, StatTypes.Charisma, StatTypes.Intuition, StatTypes.Life};

                sList.ForEach((x) =>
                {
                    if (splitspec.ContainsKey(x.GetDescription())) newspec.Stats.AddStat(x, Convert.ToInt32(splitspec[x.GetDescription()]));
                });

                return newspec;
            }
            catch
            {
                return null;
            }

        }
    }
}
