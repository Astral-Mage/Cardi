using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.CardSystem.UserData;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;
using System;
using System.Linq;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public enum SetTags
    {
        alias,
        spec,
        arc,
        calling,
        title,
        weapon,
        armor,
        special,
        box,
    }

    public class SetAction : BaseAction
    {
        int AliasCharLimit = 150;
        int TitleCharLimit = 250;

        public SetAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Whisper;
            RequiresRegisteredUser = true;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            // -set alias bob
            // -set spec gravity mage
            // -set box 1 empowering pants
            // -skills move 4 1
            // -skills shop
            // -skills buy 2
            // -skills add 4
            // -skills remove 8

            var split = ao.Message.Split(" ".ToCharArray(), 2).ToList();
            SetTags res = SetTags.alias;
            if (string.IsNullOrWhiteSpace(ao.Message) || card == null || !Enum.TryParse<SetTags>(split.First(), true, out res))
            {
                string resp = string.Empty;
                if (card == null) resp = "You must be an active user to use this command.";
                else if (string.IsNullOrWhiteSpace(ao.Message))
                {
                    resp += $"Available {ao.CommandChar}set subcommands: ";
                    foreach (var st in Enum.GetNames(typeof(SetTags)))
                    {
                        resp += $"{st},  ";
                    }
                    resp.TrimEnd(',');
                }
                SystemController.Instance.Respond(ao.Channel, resp, ao.User);
            }

            switch(res)
            {
                case SetTags.alias:
                    {
                        SetAlias(card, split.Last());
                    }
                    break;
                case SetTags.title:
                    {
                        SetTitle(card, split.Last());
                    }
                    break;
                case SetTags.arc:
                    {
                        SetCustomization(card, split.Last(), CustomizationTypes.Archetype);
                    }
                    break;
                case SetTags.spec:
                    {
                        SetCustomization(card, split.Last(), CustomizationTypes.Specialization);
                    }
                    break;
                case SetTags.calling:
                    {
                        SetCustomization(card, split.Last(), CustomizationTypes.Calling);
                    }
                    break;
                case SetTags.weapon:
                    {

                    }
                    break;
                case SetTags.armor:
                    {

                    }
                    break;
                case SetTags.special:
                    {

                    }
                    break;
                default:
                    break;
            }


        }

        public void SetEquippedName(UserCard card, string newName)
        {

        }

        public void SetCustomization(UserCard card, string newCustom, CustomizationTypes ctype)
        {
            var currentcustom = card.GetActiveCustomizationByType(ctype);
            var allcustoms = DataDb.CustomDb.GetAllCustomizationsByType(ctype);
            BaseCustomization newcustom = null;
            foreach (var c in allcustoms)
            {
                if (c.Name.ToLowerInvariant().Equals(newCustom.ToLowerInvariant()))
                {
                    newcustom = c;
                    break;
                }
            }

            if (newcustom == null)
            {
                SystemController.Instance.Respond(null, $"Unable to locate new {ctype}: {newCustom}. Please try again.", card.Alias);
                return;
            }



            if (!card.SetActiveCustomization(currentcustom, newcustom))
            {
                SystemController.Instance.Respond(null, $"Unable to disable old {ctype}: {currentcustom.Name}. Please contact a moderator.", card.Alias);
            }

            DataDb.CardDb.UpdateUserCard(card);
            SystemController.Instance.Respond(null, $"{card.Alias}, you have successfully converted from {currentcustom.GetName()} to {newcustom.GetName()}!", card.Name);
        }

        public void SetAlias(UserCard card, string newAlias)
        {
            if (string.IsNullOrEmpty(newAlias))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", card.Alias);
                return;
            }

            if (newAlias.IndexOf("\n") > -1 || newAlias.Contains("icon]") || newAlias.StartsWith("/") || newAlias.Contains("[url="))
            {
                SystemController.Instance.Respond(null, "No newlines, links, or icons accepted. Sorry!", card.Alias);
                return;
            }

            if (newAlias.Length > AliasCharLimit)
            {
                SystemController.Instance.Respond(null, $"Sorry, but your max length for your name is [color=red]{newAlias.Length}[/color]/{AliasCharLimit}.", card.Alias);
                return;
            }

            string oldalias = card.Alias;
            card.Alias = newAlias;
            DataDb.CardDb.UpdateUserCard(card);
            SystemController.Instance.Respond(null, $"{oldalias}, you are now known as {card.Alias}.", card.Name);
        }

        public void SetTitle(UserCard card, string newAlias)
        {
            if (string.IsNullOrEmpty(newAlias))
            {
                SystemController.Instance.Respond(null, "You must enter a value. Sorry!", card.Alias);
                return;
            }

            if (newAlias.IndexOf("\n") > -1 || newAlias.Contains("icon]") || newAlias.StartsWith("/") || newAlias.Contains("[url="))
            {
                SystemController.Instance.Respond(null, "No newlines, links, or icons accepted. Sorry!", card.Alias);
                return;
            }

            if (newAlias.Length > TitleCharLimit)
            {
                SystemController.Instance.Respond(null, $"Sorry, but your max length for your title is [color=red]{newAlias.Length}[/color]/{TitleCharLimit}.", card.Alias);
                return;
            }

            string oldtitle = card.CurrentTitle;
            card.CurrentTitle = newAlias;
            DataDb.CardDb.UpdateUserCard(card);
            SystemController.Instance.Respond(null, $"{oldtitle}, you are now known as {card.CurrentTitle}.", card.Name);
        }
    }
}
