using ChatBot.Core;
using System;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        /// <summary>
        /// character limit for item categories
        /// </summary>
        readonly int ItemCharLimit = 150;

        /// <summary>and i
        /// displays a help message
        /// </summary>
        /// <param name="sendingUser">requesting user</param>
        public void SetHelpAction(string sendingUser)
        {
            string toSend = "\\nThis is where you can customize your card to your liking!" +
                "\\n\\nYou can customize many different parts of your card. Let's start with your species and class." +
                "\\nIf you type [color=pink]-set species Bird[/color], you'll change your species to a bird!" +
                "\\nThe same thing works for class! [color=pink]-set class Wizard[/color] will set your class to Wizard!" +
                "\\n\\nYou can customize your equipment too. Your three equipment slots are your [color=orange]weapon, gear, special[/color]." +
                "\\nJust like species and class, you can set these too! [color=pink]-set weapon Amazing Wizard's Wand[/color] will give you an Amazing Wizard's Wand!" +
                "\\nYou can do the exact same command structure to set your [color=orange]weapon[/color], [color=orange]gear[/color], and your [color=orange]special[/color] slots. Give it a try!" +
                "\\n\\nGet fancy with bbcode! No icons, eicons, or urls allowed! And watch the character-length limit!" +
                "\\n\\nSome more useful commands include:\\n" +
                "[color=red]-set color red[/color] to set your card's theme color to red. Only valid bbcode colors are accepted!\\n" +
                "[color=green]-set sig[/color] to set your card's signature. This can include icons and eicons, so go crazy (leave empty to set back to default!)\\n" +
                "[color=blue]-set nick[/color] to set your card's nickname. This will display instead of your default name on your card (leave empty to set back to default!)\\n";
            SystemController.Instance.Respond(null, toSend, sendingUser);
        }

        /// <summary>
        /// handles setting user's card to a specific valid color
        /// </summary>
        /// <param name="pc">user requesting the change</param>
        /// <param name="color">specified color</param>
        public void SetColorAction(PlayerCard pc, string color)
        {
            if (colors.Contains(color.ToLowerInvariant()))
            {
                pc.colortheme = color.ToLower();
                GameDb.UpdateCard(pc);

                SystemController.Instance.Respond(null, $"Successfully set your card color theme to [color={color}]{color}[/color]!", pc.name);
            }
            else
            {
                SystemController.Instance.Respond(null, $"Please enter a valid color: [color=white]white[/color], [color=black]black[/color], [color=red]red[/color], [color=blue]blue[/color], [color=yellow]yellow[/color], [color=green]green[/color], [color=pink]pink[/color], [color=gray]gray[/color], [color=orange]orange[/color], [color=purple]purple[/color], [color=brown]brown[/color], [color=cyan]cyan[/color]", pc.name);
            }
        }

        /// <summary>
        /// handles setting the customized name for an item
        /// </summary>
        /// <param name="pc">player setting an item</param>
        /// <param name="cardType">type of item being set</param>
        /// <param name="message">source message</param>
        public void SetCardTypeAction(PlayerCard pc, CardType cardType, string message)
        {
            int charlimit = ItemCharLimit;
            if (cardType == CardType.Signature)
            {
                charlimit = 200;
            }
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message.Length > charlimit)
                {
                    SystemController.Instance.Respond(null, $"Sorry, character limit for {Enum.GetName(typeof(CardType), cardType)} is: [color=red]{message.Length}[/color] [b]/ {charlimit}[/b]", pc.name);
                    return;
                }

                if (cardType != CardType.Signature)
                {
                    if (message.Contains("\"") || message.Contains("[url") || message.Contains("[eicon") || message.Contains("[icon") || message.Contains("[user"))
                    {
                        SystemController.Instance.Respond(null, $"Sorry, but you can't use eicon, icon, quotes, user, or url bbcode.", pc.name);
                        return;
                    }
                }
                else if (message.Contains("\"") || message.Contains("[url") || message.Contains("[user"))
                {
                    SystemController.Instance.Respond(null, $"Sorry, but you can't use eicon, icon, quotes, user, or url bbcode.", pc.name);
                    return;
                }

                if (cardType == CardType.Class) pc.mainClass = message;
                else if (cardType == CardType.Species) pc.species = message;
                else if (cardType == CardType.Signature) pc.signature = message;
                else if (cardType == CardType.Nickname) pc.nickname = message;

                GameDb.UpdateCard(pc);

                SystemController.Instance.Respond(null, $"Your new {Enum.GetName(typeof(CardType), cardType)} is set to: {message}!", pc.name);
            }
            else
            {
                if (cardType == CardType.Nickname)
                {
                    pc.nickname = string.Empty;
                    GameDb.UpdateCard(pc);
                    SystemController.Instance.Respond(null, $"Your new {Enum.GetName(typeof(CardType), cardType)} is set to: {pc.name}!", pc.name);
                }
                else if (cardType == CardType.Signature)
                {
                    pc.signature = string.Empty;
                    GameDb.UpdateCard(pc);
                    SystemController.Instance.Respond(null, $"Your new {Enum.GetName(typeof(CardType), cardType)} is hidden!", pc.name);
                }
                else
                {
                    SystemController.Instance.Respond(null, $"Please enter a valid {Enum.GetName(typeof(CardType), cardType)}: -set {Enum.GetName(typeof(CardType), cardType)} Blah'thok", pc.name);
                }
            }
        }

        /// <summary>
        /// handles setting the customized name for an item
        /// </summary>
        /// <param name="pc">player setting an item</param>
        /// <param name="itemType">type of item being set</param>
        /// <param name="message">source message</param>
        public void SetItemAction(PlayerCard pc, ItemType itemType, string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                if (message.Length > ItemCharLimit)
                {
                    SystemController.Instance.Respond(null, $"Sorry, character limit for {Enum.GetName(typeof(ItemType), itemType)} is: [color=red]{message.Length}[/color] [b]/ {ItemCharLimit}[/b]", pc.name);
                    return;
                }
                if (message.Contains("[url") || message.Contains("[eicon") || message.Contains("[icon"))
                {
                    SystemController.Instance.Respond(null, $"Sorry, but you can't use eicon, icon, or url bbcode.", pc.name);
                    return;
                }

                if (itemType == ItemType.Weapon) pc.weapon = message;
                else if (itemType == ItemType.Gear) pc.gear = message;
                else if (itemType == ItemType.Special) pc.special = message;

                GameDb.UpdateCard(pc);

                SystemController.Instance.Respond(null, $"Your new {Enum.GetName(typeof(ItemType), itemType)} is set to: {message}!", pc.name);
            }
            else
            {
                SystemController.Instance.Respond(null, $"Please enter a valid {Enum.GetName(typeof(ItemType), itemType)}: -set {Enum.GetName(typeof(ItemType), itemType)} Blah'thok", pc.name);
            }
        }
    }
}
