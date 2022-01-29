using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.GatchaGame.Enums
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
