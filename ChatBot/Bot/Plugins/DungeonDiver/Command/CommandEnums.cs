using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins
{
    /// <summary>
    /// public, private, or any is acceptable for this command
    /// </summary>
    public enum BotCommandRestriction
    {
        Whisper,
        Message,
        Both
    }

    /// <summary>
    /// whether or not there's security access required for this command
    /// </summary>
    public enum CommandSecurity
    {
        Ops,
        None
    }
}
