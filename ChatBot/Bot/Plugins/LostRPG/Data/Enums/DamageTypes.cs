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


        [Description("cyan")]
        Magic,


        [Description("pink")]
        Lust,
    }
}
