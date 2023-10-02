using Accord;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.InformationSystem
{
    public static class InfoCore
    {
        public static void GetCustomizationInfo(UserCard card, string message, string channel, List<string> splitmsg, string user, CustomizationTypes ctype)
        {
            var customs = DataDb.CustomDb.GetAllCustomizationsByType(ctype);
            string tosend = string.Empty;
            if (customs == null || !customs.Any())
            {
                SystemController.Instance.Respond(channel, $"No available customizations of type {ctype}.", user);
                return;
            }
            if (splitmsg.Count == 1)
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }
                tosend += $"[u][b]{ctype} details for {card.Alias}[/b][/u]\\n\\n{card.GetActiveCustomizationByType(ctype).GetInfo(1)}";
            }
            else if (splitmsg.Last().ToLowerInvariant().Equals(InfoTypes.All.GetDescription().ToLowerInvariant()))
            {
                if (customs.Any())
                {
                    tosend = $"\\n                   [u][b]Available {ctype} Customizations[/b][/u]\\n\\n";
                    customs = customs.OrderBy(x => x.Stats.GetStat(CardSystem.UserData.StatTypes.DamageType)).ToList();
                    foreach (var c in customs)
                    {
                        tosend += $"[b]⨠ {c.GetName()}[/b] {c.GetStatString()} " +
                            $" {c.GetEffectString(EffectTypes.Buff)}" +
                            $" {c.GetEffectString(EffectTypes.Debuff)}" +
                            $" {c.GetSkillString()}";
                        if (c != customs.Last()) tosend += "\\n";
                    }
                }
            }
            else if (splitmsg.Count > 1)
            {
                foreach (var spec in customs)
                {
                    if (spec.Name.ToLowerInvariant().Equals(splitmsg.Last().ToLowerInvariant()))
                    {
                        tosend += spec.GetInfo(4);
                        break;
                    }
                }
            }

            if (card == null) SystemController.Instance.Respond(channel, tosend, user);
            else SystemController.Instance.Respond(channel, tosend, user);
        }

        public static string GetAllCustomizationInfoByType(CustomizationTypes ctype, bool includetitle = true)
        {
            string toreturn = "";
            var customs = DataDb.CustomDb.GetAllCustomizationsByType(ctype);

            if (customs.Any())
            {
                if (includetitle) toreturn = $"[u][b]Available {ctype} Customizations[/b][/u]\\n\\n";
                foreach (var c in customs)
                {
                    toreturn += $"[b]⨠ {c.GetName()}[/b]   {c.GetStatString()} " +
                        $"{c.GetEffectString(EffectTypes.Buff)}" +
                        $"{c.GetEffectString(EffectTypes.Debuff)}" +
                        $"{c.GetSkillString()}";
                    if (c != customs.Last()) toreturn += "\\n";
                }
            }

            return toreturn;
        }

        public static void GetTagInfo(UserCard card, string channel, List<string> splitmsg, string user)
        {
            string toSend = string.Empty;

            var arcs = DataDb.TagsDb.GetAllTags();
            if (splitmsg.Count == 1)
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }
                List<int> allTags = new List<int>();
                card.GetActiveCustomizationByType(CustomizationTypes.Specialization).Tags.ForEach(x => allTags.Add(x));
                card.GetActiveCustomizationByType(CustomizationTypes.Archetype).Tags.ForEach((x) =>
                {
                    if (!allTags.Contains(x))
                    {
                        allTags.Add(x);
                    }
                });
                card.GetActiveCustomizationByType(CustomizationTypes.Calling).Tags.ForEach((x) =>
                {
                    if (!allTags.Contains(x))
                    {
                        allTags.Add(x);
                    }
                });

                List<string> allTagsName = new List<string>();
                
                foreach (var tag in allTags)
                {
                    foreach (var Mtag in arcs)
                    {
                        if (Mtag.TagId == tag)
                        {
                            allTagsName.Add(Mtag.Name);
                            break;
                        }
                    }
                }

                toSend += $"All Available Tags for {card.Name}\\n\\n{string.Join(" • ", allTagsName)}";
            }
            else if (splitmsg.Last().ToLowerInvariant().Equals(InfoTypes.All.GetDescription().ToLowerInvariant()))
            {
                List<string> TagList = new List<string>();
                arcs.ForEach(x => TagList.Add(x.Name));
                toSend += $"\\n All Available Tags" +
                    $"\\n" +
                    $"\\n{string.Join(" • ", TagList)}";
            }
            if (!string.IsNullOrWhiteSpace(toSend)) SystemController.Instance.Respond(channel, toSend, user);
        }
    }
}
