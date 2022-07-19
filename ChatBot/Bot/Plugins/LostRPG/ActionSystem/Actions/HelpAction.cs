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
        }

        public override void Execute(ActionObject ao)
        {
            string toSend = string.Empty;

            toSend += $"[color=white]" +
                            $"         Welcome to [b][color=red]Cardinal System[/color][/b]!" +
                $"\\n    " +
                $"\\n        * Note that this bot is currently in [color=green]Alpha[/color]. Data resets may happen!" +
                $"\\n    " +
                $"\\nType {ao.CommandChar}Create to get started." +
                $"[/color]";

            SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, toSend, ao.User);
        }
    }
}
