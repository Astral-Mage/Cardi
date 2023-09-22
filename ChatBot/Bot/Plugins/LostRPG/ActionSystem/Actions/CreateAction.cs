using ChatBot.Bot.Plugins.LostRPG.CardSystem;
using ChatBot.Bot.Plugins.LostRPG.Data;
using ChatBot.Bot.Plugins.LostRPG.Data.Enums;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem;
using ChatBot.Bot.Plugins.LostRPG.DialogueSystem.Dialogue;
using ChatBot.Core;

namespace ChatBot.Bot.Plugins.LostRPG.ActionSystem.Actions
{
    public class CreateAction : BaseAction
    {
        public CreateAction()
        {
            Description = "";
            SecurityType = CommandSecurity.None;
            ChatRestriction = ChatTypeRestriction.Whisper;
            RequiresRegisteredUser = false;
        }

        public override void Execute(ActionObject ao, UserCard card)
        {
            if (card == null)
            {
                DialogueController.Instance.StartDialogue(typeof(CreateUser), ao.User, ao.Channel);
                return;
            }
            else
            {
                SystemController.Instance.Respond(ChatRestriction == ChatTypeRestriction.Whisper ? null : ao.Channel, $"You've already created your character, {card.Alias}", ao.User);
                return;
            }
        }
    }
}
