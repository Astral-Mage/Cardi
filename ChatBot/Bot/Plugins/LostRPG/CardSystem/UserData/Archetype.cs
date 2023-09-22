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

        public List<int> Specs { get; set; }

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
            Specs = new List<int>();
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

                var spec = DataDb.SpecDb.GetAllSpecs();
                var toaddspecs = splitspec["specs"].Split(",".ToCharArray()).ToList();
                
                foreach(var s in toaddspecs)
                {
                    bool found = false;
                    foreach (var ta in spec)
                    {
                        if (ta.Name.ToLowerInvariant().Equals(s.ToLowerInvariant()))
                        {
                            found = true;
                            arc.Specs.Add(ta.SpecId);
                        }

                        if (found) break;
                    }
                }

                if (arc.Specs.Count != toaddspecs.Count)
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