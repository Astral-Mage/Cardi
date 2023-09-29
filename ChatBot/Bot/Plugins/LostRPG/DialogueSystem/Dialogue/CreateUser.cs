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
        public int NameMaxLen = 130;

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
            SystemController.Instance.Respond(null, $"" + $"\\nWhat's your name?" +
                $"\\n" +
                $"\\n[color=blue][b]First off... what would you like to be known as? This is strictly your name, without any fancy titles. Feel free to use bbcode. No icons or newlines. Character limit of: {NameMaxLen}.[/b][/color]" +
                $"\\n[sup]And don't worry, you'll be able to change this later if you end up changing your mind![/sup]", Owner);
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

            if (args.IndexOf("\n") > -1 || args.Contains("icon]") || args.StartsWith("/"))
            {
                SystemController.Instance.Respond(null, "No newlines or icons accepted. Sorry!", Owner);
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
            SystemController.Instance.Respond(null, $"Fantastic, {Card.Alias}. Now what's your Title? For example: The Goblin Slayer!" +
                $"\\n" +
                $"\\n[color=blue][b]You may use bbcode, but no newlines. Character limit of: {TitleLength}.\\nIf you wish to change your name after character creation is complete, you may do so by typing {CommandChar}{ActionNames.setname}.[/b][/color]", Owner);
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

            Card.CurrentTitle = args.Trim();

            var arcs = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Archetype);

            string arcstr = string.Empty;
            foreach (var a in arcs)
            {
                arcstr += $"{a.GetName()}  ";
            }

            SystemController.Instance.Respond(null, "Next, what is your Archetype. An Archetype is what you identify your position in the world to be. Please choose from the list, for now." +
                "\\n" +
                $"\\nAvailable Archetypes: {arcstr}", Owner);
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
            Card.CustomizationHistory.Add(arc);

            var specs = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Specialization);
            string specstr = string.Empty;
            foreach (var a in specs)
            {
                specstr += $"[sup]⌈[/sup]{a.Name}[sub]⌋[/sub] ";
            }

            SystemController.Instance.Respond(null, "Next, what is your primary Specialization. A specialization is your primary magical focus. Please choose from the list, for now." +
                "\\n" +
                $"\\nAvailable Specializations: {specstr}", Owner);
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
            Card.CustomizationHistory.Add(spec);

            string toSend = "Finally, what is your calling:";
            var callings = DataDb.CustomDb.GetAllCustomizationsByType(CustomizationTypes.Calling);
            foreach (var c in callings)
            {
                toSend += $"[sup]⌈[/sup]{c.Name}[sub]⌋[/sub] ";
            }
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
            Card.CustomizationHistory.Add(call);

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
            SystemController.Instance.Respond(null, $"Thanks! Type {CommandChar}card to see your character card!", Owner);
        }
    }
}