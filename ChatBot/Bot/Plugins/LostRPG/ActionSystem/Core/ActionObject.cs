using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class ActionObject
    {
        public string User { get; set; }
        public string CommandChar { get; set; }
        public string Channel { get; set; }

        public ActionObject()
        {
            User = string.Empty;
            CommandChar = string.Empty;
            Channel = string.Empty;

        }
    }
}
