using System;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    [Serializable]
    public enum CombatSteps
    {
        PreCombat,
        Attacker,
        Defender,
        Damage,
        PostCombat,
    }
}
