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
        public static void GetSpecInfo(UserCard card, string message, string channel, List<string> splitmsg, string user)
        {
            string toSend = string.Empty;
            var specs = DataDb.SpecDb.GetAllSpecs();
            if (splitmsg.Count == 1)
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }

                toSend += card.Spec.GetInfo();
            }
            else if (splitmsg.Last().ToLowerInvariant().Equals(InfoTypes.All.GetDescription().ToLowerInvariant()))
            {
                List<string> SpecList = new List<string>();
                specs.ForEach(x => SpecList.Add(x.Name));
                toSend += $"\\n All Available Specializations" +
                    $"\\n" +
                    $"\\n{string.Join(" • ", SpecList)}";
            }
            else if (splitmsg.Count > 1)
            {
                foreach (var spec in specs)
                {
                    if (spec.Name.ToLowerInvariant().Equals(splitmsg.Last().ToLowerInvariant()))
                    {
                        toSend += spec.GetInfo();
                        break;
                    }
                }
            }
            else
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }
                toSend += card.Spec.GetInfo();
            }
            SystemController.Instance.Respond(channel, toSend, user);
        }

        public static void GetTagInfo(UserCard card, string message, string channel, List<string> splitmsg, string user)
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
                card.Spec.Tags.ForEach(x => allTags.Add(x));
                card.Archetype.Tags.ForEach((x) =>
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
            SystemController.Instance.Respond(channel, toSend, user);
        }

        public static void GetArcInfo(UserCard card, string message, string channel, List<string> splitmsg, string user)
        {
            string toSend = string.Empty;

            var arcs = DataDb.ArcDb.GetAllArchetypes();
            if (splitmsg.Count == 1)
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }
                toSend += card.Archetype.GetInfo();
            }
            else if (splitmsg.Last().ToLowerInvariant().Equals(InfoTypes.All.GetDescription().ToLowerInvariant()))
            {
                List<string> SpecList = new List<string>();
                arcs.ForEach(x => SpecList.Add(x.Name));
                toSend += $"\\n All Available Archetypes" +
                    $"\\n" +
                    $"\\n{string.Join(" • ", SpecList)}";
            }
            else if (splitmsg.Count > 1)
            {
                foreach (var arc in arcs)
                {
                    if (arc.Name.ToLowerInvariant().Equals(splitmsg.Last().ToLowerInvariant()))
                    {
                        toSend += arc.GetInfo();
                        break;
                    }
                }
            }
            else
            {
                if (card == null)
                {
                    SystemController.Instance.Respond(channel, $"Please create a character to use this command.", user);
                    return;
                }
                toSend += card.Archetype.GetInfo();
            }
            SystemController.Instance.Respond(channel, toSend, user);
        }
    }
}
