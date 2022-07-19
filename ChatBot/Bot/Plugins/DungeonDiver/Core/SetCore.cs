using ChatBot.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        /// <summary>
        /// list of valid colors
        /// </summary>
        readonly List<string> colors = new List<string>() { "red", "blue", "yellow", "green", "pink", "gray", "orange", "purple", "brown", "cyan", "black", "white" };

        /// <summary>
        /// returns a list of commands for the set menu
        /// </summary>
        /// <returns></returns>
        List<Command> GetSetSubCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, ChatTypeRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.Species, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your species"),
                new Command(CommandStrings.Class, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your class"),
                new Command(CommandStrings.Weapon, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your weapon"),
                new Command(CommandStrings.Armor, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your gear"),
                new Command(CommandStrings.Special, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your special"),
                new Command(CommandStrings.Color, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your card's color theme"),
                new Command(CommandStrings.Signature, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your card's signature"),
                new Command(CommandStrings.Nickname, ChatTypeRestriction.Whisper, CommandSecurity.None, "sets your character's nickname"),
            };
        }

        /// <summary>
        /// the main command switch
        /// </summary>
        /// <param name="command">the command being sent</param>
        /// <param name="channel">the source channel</param>
        /// <param name="message">cleaned up message</param>
        /// <param name="pc">pc making the command</param>
        public void SetSubMenu(Command command, string message, PlayerCard pc)
        {
            message = message.Replace("&gt;", ">");
            message = message.Replace("&lt;", "<");

            if (message.Contains("\\n"))
            {
                SystemController.Instance.Respond(null, $"Sorry, but you can't insert newlines into any set commands!", pc.name);
                return;
            }

            switch (command.command)
            {
                case CommandStrings.Help:
                    {
                        SetHelpAction(pc.name);
                    }
                    break;
                case CommandStrings.Nickname:
                    {
                        SetCardTypeAction(pc, CardType.Nickname, message);
                    }
                    break;
                case CommandStrings.Color:
                    {
                        SetColorAction(pc, message);
                    }
                    break;
                case CommandStrings.Weapon:
                    {
                        SetItemAction(pc, ItemType.Weapon, message);
                    }
                    break;
                case CommandStrings.Armor:
                    {
                        SetItemAction(pc, ItemType.Gear, message);
                    }
                    break;
                case CommandStrings.Special:
                    {
                        SetItemAction(pc, ItemType.Special, message);
                    }
                    break;
                case CommandStrings.Class:
                    {
                        SetCardTypeAction(pc, CardType.Class, message);
                    }
                    break;
                case CommandStrings.Species:
                    {
                        SetCardTypeAction(pc, CardType.Species, message);
                    }
                    break;
                case CommandStrings.Signature:
                    {
                        SetCardTypeAction(pc, CardType.Signature, message);
                    }
                    break;
            }
        }
    }
}
