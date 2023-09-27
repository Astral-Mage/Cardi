using ChatBot.Bot.Plugins.GatchaGame.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Cards
{
    /// <summary>
    /// a basic bot command
    /// </summary>
    public class Command
    {
        public string command;
        public ChatTypeRestriction restriction;
        public CommandSecurity security;
        public string description;
        public string tags;

        /// <summary>
        /// our constructor
        /// </summary>
        /// <param name="cmd">the command</param>
        /// <param name="rest">chat type restriction</param>
        /// <param name="cmdSecurity">security access required</param>
        /// <param name="description">summary of the command</param>
        /// <param name="tags">tags</param>
        public Command(string cmd, ChatTypeRestriction rest, CommandSecurity cmdSecurity, string description = null, string tags = null)
        {
            command = cmd;
            restriction = rest;
            security = cmdSecurity;
            this.description = description;
            this.tags = tags;
        }
    }
}
