using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.Data.Enums
{
    public enum InfoTypes
    {
        [Description("spec")]
        Spec,
        [Description("arc")]
        Arc,
        [Description("all")]
        All,
        [Description("tags")]
        Tags,

    }
}
