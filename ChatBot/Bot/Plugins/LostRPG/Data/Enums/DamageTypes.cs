using System;
using System.ComponentModel;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    [Serializable]
    public enum DamageTypes
    {
        [Description("gray")]
        None = -1,

        [Description("brown")]
        Physical,

        [Description("yellow")]
        Lightning,

        [Description("cyan")]
        Ice,

        [Description("red")]
        Fire,

        [Description("purple")]
        Astral,

        [Description("green")]
        Nature,

        [Description("blue")]
        Aqua,

        [Description("black")]
        Void,

        [Description("pink")]
        Perversion,
    }
}
