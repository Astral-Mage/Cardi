using System;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    [Serializable]
    public enum CharacterStatusTypes
    {
        Undefined,
        Dead,
        Alive,
        Smug, // alive and defeated the opponent
        Vengeful, // dead and defeated the opponent
    }
}
