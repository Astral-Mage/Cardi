using ChatBot.Bot.Plugins.LostRPG.Data.Enums;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class BaseAction
    {
        public virtual void Execute(ActionObject ao) { }

        public string Description { get; set; }

        public CommandSecurity SecurityType { get; set; }

        public ChatTypeRestriction ChatRestriction { get; set; }
    }
}
