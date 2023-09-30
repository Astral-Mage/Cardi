using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class HelpAction : BaseAction
    {
        public HelpAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Whisper;
            RequiresRegisteredUser = false;

            AlternateNames.Add(ActionNames.h);
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                            $"         [b][color=red][/color][/b]" +
                $"\\n    " +
                $"\\n        * Note that this bot is currently in [color=green]Alpha[/color]. Data resets may happen!" +
                $"\\n    " +
                $"\\nType {ao.CommandChar}Help..." +
                $"[/color]";

            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, toSend, ao.User);
        }
    }
}
