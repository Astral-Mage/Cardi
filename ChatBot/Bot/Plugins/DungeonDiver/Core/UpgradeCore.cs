using System;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins
{
    public partial class GameCore : PluginBase
    {
        /// <summary>
        /// the starting cost for upgrades
        /// </summary>
        int UpgradeBaseCost = 50;

        /// <summary>
        /// gets a list of commands for the upgrade sub menu
        /// </summary>
        /// <returns>command as list</returns>
        private List<Command> GetUpgradeSubCommandList()
        {
            return new List<Command>
            {
                new Command(CommandStrings.Help, ChatTypeRestriction.Whisper, CommandSecurity.None, "returns the list of help options"),
                new Command(CommandStrings.Weapon, ChatTypeRestriction.Whisper, CommandSecurity.None, "upgrades your weapon"),
                new Command(CommandStrings.Armor, ChatTypeRestriction.Whisper, CommandSecurity.None, "upgrades your gear"),
                new Command(CommandStrings.Special, ChatTypeRestriction.Whisper, CommandSecurity.None, "upgrades your special"),
            };
        }

        /// <summary>
        /// handles menu commands dealing with item upgrades
        /// </summary>
        /// <param name="command">command being sent</param>
        /// <param name="channel">source channel</param>
        /// <param name="pc">user sending the command</param>
        public void UpgradeSubMenu(Command command, string channel, PlayerCard pc)
        {


            switch (command.command)
            {
                case "help":
                    {
                        UpgradeHelpAction(pc.name);
                    }
                    break;
                case "weapon":
                    {
                        UpgradeItemAction(pc, ItemType.Weapon);
                    }
                    break;
                case "gear":
                    {
                        UpgradeItemAction(pc, ItemType.Gear);
                    }
                    break;
                case "special":
                    {
                        UpgradeItemAction(pc, ItemType.Special);
                    }
                    break;
            }
        }
    }
}
