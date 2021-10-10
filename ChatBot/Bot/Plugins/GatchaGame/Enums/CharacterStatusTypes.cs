using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    public enum CharacterStatusTypes
    {
        Undefined,
        Dead,
        Alive,
        Smug, // alive and defeated the opponent
        Vengeful, // dead and defeated the opponent
    }
}
