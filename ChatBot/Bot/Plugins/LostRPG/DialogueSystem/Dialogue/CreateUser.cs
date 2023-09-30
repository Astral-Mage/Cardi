using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Dialogue
{
    public class CreateUser : Dialogue
    {

        public int TitleLength = 250;
        public int NameMaxLen = 150;

        public CreateUser(string owner, string commandChar, string channel) : base(typeof(CreateUser).GetHashCode(), owner, commandChar, channel)
        {
            MaxSteps = 6;
            ChildType = GetType();
            TypeOfDialogue = Enums.DialogueType.Conversation;
            Locale = Enums.DialogueLocale.Private;
        }

        UserCard Card;

        public void One(string args)
        {
            SystemController.Instance.Respond(null, $"" + $"Welcome! Please type your name to get started. You may include BBcode, but no newlines, links, or icons. Everything you enter during this process can be changed later. Character limit: {NameMaxLen}.", Owner);
            Card = new UserCard(Owner);
        }

        public void Two(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", Owner);
                CurrentStep = 1;
                BackingUp = true;
                return;
            }

            if (args.IndexOf("\n") > -1 || args.Contains("icon]") || args.StartsWith("/") || args.Contains("[url="))
            {
                SystemController.Instance.Respond(null, "No newlines, links, or icons accepted. Sorry!", Owner);
                CurrentStep = 1;
                BackingUp = true;
                return;
            }

            if (args.Length > NameMaxLen)
            {
                SystemController.Instance.Respond(null, $"Sorry, but your max length for your name is [color=red]{args.Length}[/color]/{NameMaxLen}.", Owner);
                CurrentStep = 1;
                BackingUp = true;
                return;
            }

            Card.Alias = args;
            SystemController.Instance.Respond(null, $"Fantastic, {Card.Alias}. Now what's your Title? For example: The Goblin Slayer! Same BBCode rules apply, and your character limit is: {TitleLength}", Owner);
        }

        public void Three(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", Owner);
                CurrentStep = 2;
                BackingUp = true;
                return;
            }

            if (args.IndexOf("\n") > -1 || args.Contains("icon]") || args.StartsWith("/") || args.Contains("[url="))
            {
                SystemController.Instance.Respond(null, "No newlines, links, or icons accepted. Sorry!", Owner);
                CurrentStep = 2;
                BackingUp = true;
                return;
            }

            if (args.Length > NameMaxLen)
            {
                SystemController.Instance.Respond(null, $"Sorry, but your max length for your name is [color=red]{args.Length}[/color]/{TitleLength}.", Owner);
                CurrentStep = 1;
                BackingUp = true;
                return;
            }

            Card.CurrentTitle = args.Trim();

            SystemController.Instance.Respond(null, "Next, what is your Archetype. An Archetype is something that describes you." +
                "\\n" +
                $"\\n{InformationSystem.InfoCore.GetAllCustomizationInfoByType(CustomizationTypes.Archetype)}", Owner);
        }

        public void Four(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", Owner);
                CurrentStep = 3;
                BackingUp = true;
                return;
            }

            List<BaseCustomization> arcs = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Archetype);
            BaseCustomization arc = null;
            if (arcs.Any(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant())))
            {
                arc = arcs.First(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant()));
            }
            else
            {
                SystemController.Instance.Respond(null, "You must enter a valid archetype. Sorry!", Owner);
                CurrentStep = 3;
                BackingUp = true;
                return;
            }

            arc.Active = true;
            Card.ActiveCustomizations.Add(new CustomizationDetails() { cid = arc.Id, isactive = true });

            SystemController.Instance.Respond(null, "Next, what is your primary Specialization. A specialization is your primary focus in life." +
                "\\n" +
                $"\\n{InformationSystem.InfoCore.GetAllCustomizationInfoByType(CustomizationTypes.Specialization)}", Owner);
        }

        public void Five(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", Owner);
                CurrentStep = 4;
                BackingUp = true;
                return;
            }

            var specs = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Specialization);
            BaseCustomization spec = null;
            if (specs.Any(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant())))
            {
                spec = specs.First(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant()));
            }
            else
            {
                SystemController.Instance.Respond(null, "You must enter a valid specialization. Sorry!", Owner);
                CurrentStep = 4;
                BackingUp = true;
                return;
            }

            spec.Active = true;
            Card.ActiveCustomizations.Add(new CustomizationDetails() { cid = spec.Id, isactive = true });

            string toSend = "Finally, what is your calling:";
            var callings = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Calling);
            toSend += $"\\n\\n{InformationSystem.InfoCore.GetAllCustomizationInfoByType(CustomizationTypes.Calling)}";
            SystemController.Instance.Respond(null, toSend, Owner);
        }

        public void Six(string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", Owner);
                CurrentStep = 5;
                BackingUp = true;
                return;
            }

            var specs = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Calling);
            BaseCustomization call = null;
            if (specs.Any(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant())))
            {
                call = specs.First(x => x.Name.ToLowerInvariant().Equals(args.Trim().ToLowerInvariant()));
            }
            else
            {
                SystemController.Instance.Respond(null, "You must enter a valid calling. Sorry!", Owner);
                CurrentStep = 5;
                BackingUp = true;
                return;
            }

            call.Active = true;
            Card.ActiveCustomizations.Add(new CustomizationDetails() { cid = call.Id, isactive = true });

            Card.ActiveSockets.Add(EquipmentSystem.EquipmentController.GenerateSocketItem(SocketTypes.Weapon));
            Card.ActiveSockets.Add(EquipmentSystem.EquipmentController.GenerateSocketItem(SocketTypes.Armor));
            Card.ActiveSockets.Add(EquipmentSystem.EquipmentController.GenerateSocketItem(SocketTypes.Passive));

            Card.Stats.AddStat(StatTypes.Life, 1000);
            Card.Stats.AddStat(StatTypes.CurrentLife, Card.GetMultipliedStat(StatTypes.Life));
            Card.Stats.AddStat(StatTypes.Level, 1);
            Card.Stats.AddStat(StatTypes.Experience, 0);

            Card.Stats.AddStat(StatTypes.Kills, 0);
            Card.Stats.AddStat(StatTypes.Gold, 0);
            Card.Stats.AddStat(StatTypes.Stardust, 0);
            int baseStat = 20;
            Card.Stats.AddStat(StatTypes.Strength, baseStat);
            Card.Stats.AddStat(StatTypes.Dexterity, baseStat);
            Card.Stats.AddStat(StatTypes.Constitution, baseStat);

            Card.Stats.AddStat(StatTypes.Intelligence, baseStat);
            Card.Stats.AddStat(StatTypes.Wisdom, baseStat);
            Card.Stats.AddStat(StatTypes.Perception, baseStat);

            Card.Stats.AddStat(StatTypes.Libido, baseStat);
            Card.Stats.AddStat(StatTypes.Charisma, baseStat);
            Card.Stats.AddStat(StatTypes.Intuition, baseStat);

            DataDb.CardDb.AddNewUser(Card);
            SystemController.Instance.Respond(null, $"Thanks! Type {CommandChar}card to see your character card! Also, you may type {CommandChar}help to see a full breakdown of the bot's features.", Owner);
        }
    }
}