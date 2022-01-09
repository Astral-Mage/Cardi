using System;
using Accord;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
{
    [Serializable]
    public enum PlayerActionTimeoutTypes
    {
        [Description("bsc")]
        BulliedSomeoneCooldown,

        [Description("bbc")]
        HasBeenBulliedCooldown,

        [Description("vlc")]
        ViewedLeaderboardCooldown,

        [Description("bcc")]
        ViewedCardCooldown,

        [Description("ddc")]
        DiveCooldown,

        [Description("bac")]
        BullyAttemptCooldown,

    }

    [Serializable]
    public enum LastUsedCooldownType
    {
        [Description("bsc")]
        LastBully,

        [Description("bbc")]
        LastBullied,

        [Description("vlc")]
        LastLeaderboard,

        [Description("bcc")]
        LastCard,

        [Description("ddc")]
        LastDive,

        [Description("bac")]
        LastBullyReq,
    }
}
