using ChatApi;
using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.MagicSystem;
using System.Collections.Generic;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Dialogue
{
    public class CreateUser : Dialogue
    {
        public CreateUser(string owner, string commandChar) : base(typeof(CreateUser).GetHashCode(), owner, commandChar)
        {
            MaxSteps = 3;
            ChildType = GetType();
            TypeOfDialogue = Enums.DialogueType.Conversation;
            Locale = Enums.DialogueLocale.Private;
        }

        UserCard Card;

        public void One(string args)
        {
            Respond(null, $"First off... what would you like to be known as? This is what you'll be called by and how others will call you. " +
                $"This is strictly your name, without any fancy titles. Feel free to use bbcode. No icons or newlines. " +
                $"And don't worry, you'll be able to change this later if you end up changing your mind!", Owner);
            Card = new UserCard(Owner);
        }

        public void Two(string args)
        {
            if (args.IndexOf("\n") > -1 || args.Contains("icon]") || args.StartsWith("/"))
            {
                Respond(null, $"No newlines or icons accepted. Sorry!", Owner);
                CurrentStep = 1;
                BackingUp = true;
                return;
            }

            Card.Alias = args;

            List<Magic> magic = DataDb.Instance.GetMagic();
            string magicStr = string.Empty;
            for (int i = 0; i < magic.Count; i++)
            {
                magicStr += $"[color={magic[i].Color}][b]{magic[i].Name}[/b][/color]";

                if (i < (magic.Count - 1))
                {
                    magicStr += " ";
                }
            }

            Respond(null, $"Okay, great~ Welcome to LostRPG, {Card.Alias}!" +
                $"\\n\\n" +
                $"Now, what kind of Magic calls to you?" +
                $"\\n\\n" +
                $"Available types of magic: {magicStr}", Owner);

        }

        public void Three(string args)
        {
            args = args.Trim().StripPunctuation().ToLowerInvariant();
            List<Magic> magic = DataDb.Instance.GetMagic();
            if (magic.Any(x => x.Name.ToLowerInvariant().Equals(args)))
            {
                Magic tm = magic.First(x => x.Name.ToLowerInvariant().Equals(args));
                Respond(null, $"Great choice, {Card.Alias}. You're now a Mage that specializes in [color={tm.Color}][b]{tm.Name}[/b][/color] magic! " +
                    $"Did you know that being a Mage doesn't mean you NEED to cast spells from the back lines. " +
                    $"There are plenty of melee-oriented fighting styles! There's also ways other than fighting to defeat an enemy...", Owner);
                Card.MainMagic = tm;
                DataDb.Instance.AddNewUser(Card);
                return;
            }
            else
            {
                Respond(null, $"Sorry, I didn't understand... that doesn't seem to be a valid type of magic. Try again by typing Yes, No, Repeat, or Cancel.", Owner);
                CurrentStep = 2;
                BackingUp = true;
                return;
            }
        }
    }
}