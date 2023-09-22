using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using System.Collections.Generic;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class BaseAction
    {
        public virtual void Execute(ActionObject ao, UserCard card) {  }

        public string Description { get; set; }

        public CommandSecurity SecurityType { get; set; }

        public ChatTypeRestriction ChatRestriction { get; set; }

        public bool RequiresRegisteredUser { get; set; }

        public List<string> AlternateNames { get; set; }

        public BaseAction()
        {
            Description = string.Empty;
            AlternateNames = new List<string>();
            ChatRestriction = ChatTypeRestriction.Both;
            SecurityType = CommandSecurity.None;
            RequiresRegisteredUser = true;
        }
    }
}
