using Accord;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.Data.GameData
{
    [Serializable]
    public class Effect
    {
        public EffectTypes EffectType { get; set; }

        public List<int> Tags { get; set; }
        public StatData Stats { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TimeSpan GlobalDuration { get; set; }
        public int ProcChance { get; set; }
        public ProcTriggers ProcTrigger { get; set;}
        public int EffectId { get; set; }
        public EffectTargets Target { get; set; }
        public DateTime CreationDate { get; set; }

        public Effect()
        {
            Tags = new List<int>();
            Stats = new StatData();
            Name = string.Empty;
            Description = string.Empty;
            GlobalDuration = new TimeSpan();
            ProcChance = 0;
            ProcTrigger = ProcTriggers.None;
            EffectId = 0;
            Target = EffectTargets.Self;
            CreationDate = DateTime.Now;
        }

        public string GetShortDescription()
        {
            return $"⟨ {Name} ⟩";
        }

        public TimeSpan GetRemainingDuration()
        {
            if (GlobalDuration == TimeSpan.MaxValue) return GlobalDuration;

            DateTime now = DateTime.Now;
            TimeSpan leftover = now - CreationDate;

            return GlobalDuration - leftover;
        }

        public static Effect ReadRawString(string str)
        {
            // ce type:buff target:self dur:4 tags:2 name:Haste pchance:100 ptrigger:hit dex:150 per:150 speed:150
            try
            {
                Effect be = new Effect();
                List<string> brokenraw = str.Split(" ".ToCharArray()).ToList();
                Dictionary<string, string> brokenEt = new Dictionary<string, string>();
                foreach (var bit in brokenraw)
                {
                    var split = bit.Split(":".ToCharArray()).ToList();
                    brokenEt.Add(split.First(), split.Last());
                }

                Enum.TryParse(Convert.ToString(brokenEt["type"]).Capitalize(), out EffectTypes et);
                be.EffectType = et;
                be.Name = Convert.ToString(brokenEt["name"]);
                be.Name = be.Name.Replace("+", " ");
                Enum.TryParse(Convert.ToString(brokenEt["target"]).Capitalize(), out EffectTargets ttar);
                be.Target = ttar;

                if (brokenEt.ContainsKey("pchance")) be.ProcChance = Convert.ToInt32(brokenEt["pchance"]);
                else be.ProcChance = 100;
                if (brokenEt.ContainsKey("ptrigger"))
                {
                    Enum.TryParse(Convert.ToString(brokenEt["ptrigger"]).Capitalize(), out ProcTriggers ptrig);
                    be.ProcTrigger = ptrig;
                }
                else be.ProcTrigger = ProcTriggers.None;

                int dur = Convert.ToInt32(brokenEt["dur"]);
                if (dur == 0) be.GlobalDuration = TimeSpan.MaxValue;
                else if (dur == 1) be.GlobalDuration = new TimeSpan(1, 0, 0);
                else if (dur == 2) be.GlobalDuration = new TimeSpan(8, 0, 0);
                else if (dur == 3) be.GlobalDuration = new TimeSpan(48, 0, 0);
                else be.GlobalDuration = new TimeSpan(7, 0, 0, 0);
                if (brokenEt.ContainsKey("tags"))
                {
                    bool found;
                    var specs = DataDb.CustomDb.GetAllCustomizations().Where(x => x.Customization == CustomizationTypes.Specialization);
                    bool throwex = false;
                    foreach (var tag in brokenEt["tags"].Split(",".ToCharArray()))
                    {
                        if (int.TryParse(tag, out int res))
                        {
                            foreach (var spec in specs)
                            {
                                if (spec.Id == res)
                                {
                                    be.Tags.Add(res);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            found = false;
                            foreach (var spec in specs)
                            {
                                if (spec.Name.ToLowerInvariant().Equals(tag.ToLowerInvariant()))
                                {
                                    be.Tags.Add(spec.Id);
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                throwex = true;
                            }
                        }
                    }
                    if (throwex)
                    {
                        return null;
                    }
                }

                if (brokenEt.ContainsKey(StatTypes.Strength.GetDescription())) be.Stats.AddStat(StatTypes.Strength, Convert.ToInt32(brokenEt[StatTypes.Strength.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Dexterity.GetDescription())) be.Stats.AddStat(StatTypes.Dexterity, Convert.ToInt32(brokenEt[StatTypes.Dexterity.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Constitution.GetDescription())) be.Stats.AddStat(StatTypes.Constitution, Convert.ToInt32(brokenEt[StatTypes.Constitution.GetDescription()]));

                if (brokenEt.ContainsKey(StatTypes.Intelligence.GetDescription())) be.Stats.AddStat(StatTypes.Intelligence, Convert.ToInt32(brokenEt[StatTypes.Intelligence.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Wisdom.GetDescription())) be.Stats.AddStat(StatTypes.Wisdom, Convert.ToInt32(brokenEt[StatTypes.Wisdom.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Perception.GetDescription())) be.Stats.AddStat(StatTypes.Perception, Convert.ToInt32(brokenEt[StatTypes.Perception.GetDescription()]));

                if (brokenEt.ContainsKey(StatTypes.Libido.GetDescription())) be.Stats.AddStat(StatTypes.Libido, Convert.ToInt32(brokenEt[StatTypes.Libido.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Charisma.GetDescription())) be.Stats.AddStat(StatTypes.Charisma, Convert.ToInt32(brokenEt[StatTypes.Charisma.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Intuition.GetDescription())) be.Stats.AddStat(StatTypes.Intuition, Convert.ToInt32(brokenEt[StatTypes.Intuition.GetDescription()]));

                if (brokenEt.ContainsKey(StatTypes.Speed.GetDescription())) be.Stats.AddStat(StatTypes.Speed, Convert.ToInt32(brokenEt[StatTypes.Speed.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Shield.GetDescription())) be.Stats.AddStat(StatTypes.Shield, Convert.ToInt32(brokenEt[StatTypes.Shield.GetDescription()]));
                if (brokenEt.ContainsKey(StatTypes.Barrier.GetDescription())) be.Stats.AddStat(StatTypes.Barrier, Convert.ToInt32(brokenEt[StatTypes.Barrier.GetDescription()]));


                return be;
            }
            catch
            {
                return null;
            }

        }
    }
}
